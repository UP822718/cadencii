/*
 * VocaloidVstDriver.cs
 * Copyright © 2013 kbinani
 *
 * This file is part of cadencii.
 *
 * cadencii is free software; you can redistribute it and/or
 * modify it under the terms of the GPLv3 License.
 *
 * cadencii is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
using System;
using System.Collections.Generic;
using VstSdk;
using cadencii.vsq;

namespace cadencii.vsti.vocaloid
{
    unsafe class VocaloidVstDriver : VSTiDriverBase
    {
        public delegate bool RenderCallback(float[] left, float[] right, int samples);

        private readonly RendererKind kind_;
        private readonly MemoryManager allocator_ = new MemoryManager();
        private float[] left_buffer_;
        private float[] right_buffer_;
        private float*[] output_;

        public VocaloidVstDriver(RendererKind kind)
        {
            kind_ = kind;
        }

        public override RendererKind getRendererKind()
        {
            return kind_;
        }

        public void render(IEnumerable<MidiEvent> sequence, ITempoMaster tempo, int sample_rate, RenderCallback callback)
        {
            long clock = 0;
            long processed = 0;

            blockSize = sample_rate;

            setSampleRate(sample_rate);
            aEffect.Dispatch(AEffectOpcodes.effMainsChanged, 0, 1, IntPtr.Zero, 0);

            List<MidiEvent> event_buffer = new List<MidiEvent>();
            foreach (var item in sequence) {
                if (item.clock == clock) {
                    event_buffer.Add(item);
                } else {
                    int process_samples = (int)((long)(tempo.getSecFromClock(clock) * sampleRate) - processed);

                    send(event_buffer.ToArray(), process_samples);

                    while (process_samples > 0) {
                        int amount = Math.Min(process_samples, blockSize);
                        if (!dispatchProcessReplacing(amount, callback)) {
                            return;
                        }
                        process_samples -= amount;
                        processed += amount;
                    }

                    event_buffer.Clear();
                    event_buffer.Add(item);
                    clock = item.clock;
                }
            }

            while (dispatchProcessReplacing(blockSize, callback)) ;
        }

        private bool dispatchProcessReplacing(int amount, RenderCallback callback)
        {
            if (output_ == null) {
                output_ = new float*[2];
            }
            if (left_buffer_ == null) {
                left_buffer_ = new float[blockSize];
            }
            if (right_buffer_ == null) {
                right_buffer_ = new float[blockSize];
            }
            fixed (float* left = &left_buffer_[0])
            fixed (float* right = &right_buffer_[0])
            fixed (float** output_ptr = &output_[0]) {
                output_ptr[0] = left;
                output_ptr[1] = right;
                aEffect.ProcessReplacing(IntPtr.Zero, new IntPtr(output_ptr), amount);
                return callback(left_buffer_, right_buffer_, amount);
            }
        }
    }
}
