﻿#if ENABLE_AQUESTONE
/*
 * AquesToneDriver.cs
 * Copyright (c) 2009 kbinani
 *
 * This file is part of org.kbinani.cadencii.
 *
 * org.kbinani.cadencii is free software; you can redistribute it and/or
 * modify it under the terms of the GPLv3 License.
 *
 * org.kbinani.cadencii is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 */
using System;
using System.Text;
using org.kbinani.vsq;
using bocoree;
using bocoree.java.awt;
using bocoree.java.io;
using VstSdk;

namespace org.kbinani.cadencii {
    using boolean = System.Boolean;

    public class AquesToneDriver : vstidrv {
        public static readonly String[] PHONES = new String[] { 
            "ア", "イ", "ウ", "エ", "オ",
            "カ", "キ", "ク", "ケ", "コ",
            "サ", "シ", "ス", "セ", "ソ",
            "タ", "チ", "ツ", "テ", "ト",
            "ナ", "ニ", "ヌ", "ネ", "ノ",
            "ハ", "ヒ", "フ", "ヘ", "ホ",
            "マ", "ミ", "ム", "メ", "モ",
            "ヤ", "ユ", "イェ", "ヨ",
            "ラ", "リ", "ル", "レ", "ロ",
            "ワ", "ヲ",
            "ン",
            "ガ", "ギ", "グ", "ゲ", "ゴ",
            "ザ", "ジ", "ズ", "ゼ", "ゾ",
            "ダ", "デ", "ド",
            "バ", "ビ", "ブ", "ベ", "ボ",
            "パ", "ピ", "プ", "ペ", "ポ",
            "キャ", "キュ", "キェ", "キョ",
            "シャ", "シュ", "シェ", "ショ",
            "チャ", "チュ", "チェ", "チョ",
            "ニャ", "ニュ", "ニェ", "ニョ",
            "ヒャ", "ヒュ", "ヒェ", "ヒョ",
            "ミャ", "ミュ", "ミェ", "ミョ",
            "リャ", "リュ", "リェ", "リョ",
            "ギャ", "ギュ", "ギェ", "ギョ",
            "ジャ", "ジュ", "ジェ", "ジョ",
            "ウィ", "ウェ", "ウォ",
            "ツァ", "ツィ", "ツェ", "ツォ",
            "ファ", "フィ", "フェ", "フォ",
            "ビャ", "ビュ", "ビェ", "ビョ",
            "ピャ", "ピュ", "ピェ", "ピョ",
            "スィ", "ティ", "ズィ", "ディ",
            "トゥ", "ドゥ", "デュ", "テュ",
        };
        private static readonly SingerConfig female_f1 = new SingerConfig() { VOICENAME = "Female_F1", Program = 0, Original = 0 };
        private static readonly SingerConfig male_hk = new SingerConfig() { VOICENAME = "Male_HK", Program = 1, Original = 1 };
        private static readonly SingerConfig auto_f1 = new SingerConfig() { VOICENAME = "Auto_F1", Program = 2, Original = 2 };
        private static readonly SingerConfig auto_hk = new SingerConfig() { VOICENAME = "Auto_HK", Program = 3, Original = 3 };

        public static readonly SingerConfig[] SINGERS = new SingerConfig[] { female_f1, male_hk, auto_f1, auto_hk };

        private Dimension uiWindowRect = new Dimension( 373, 158 );

        public System.Windows.Forms.Form pluginUi = null;

        public override boolean open( string dll_path, int block_size, int sample_rate ) {
            int strlen = 260;
            StringBuilder sb = new StringBuilder( strlen );
            win32.GetProfileString( "AquesTone", "FileKoe_00", "", sb, (uint)strlen );
            String koe_old = sb.ToString();

            String required = getKoeFilePath();
            boolean refresh_winini = false;
            if ( !required.Equals( koe_old ) && !koe_old.Equals( "" ) ) {
                refresh_winini = true;
                win32.WriteProfileString( "AquesTone", "FileKoe_00", required );
            }
            boolean ret = false;
            try {
                ret = base.open( dll_path, block_size, sample_rate );
            } catch ( Exception ex ) {
                ret = false;
                PortUtil.stderr.println( "AquesToneDriver#open; ex=" + ex );
            }

            try {
                pluginUi = new System.Windows.Forms.Form();
                pluginUi.Text = "AquesToneWindow";
                pluginUi.Location = new System.Drawing.Point( int.MinValue, int.MinValue );
                pluginUi.Show();
                pluginUi.Refresh();
                pluginUi.Hide();
                pluginUi.Location = new System.Drawing.Point( 0, 0 );
                unsafe {
                    aEffect.Dispatch( ref aEffect, AEffectOpcodes.effEditOpen, 0, 0, (void*)pluginUi.Handle.ToPointer(), 0.0f );
                }
                updatePluginUiRect();
                pluginUi.ClientSize = new System.Drawing.Size( uiWindowRect.width, uiWindowRect.height );
                pluginUi.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
#if DEBUG
                for ( int i = 0; i < aEffect.numParams; i++ ) {
                    PortUtil.stdout.println( "AquesToneDriver#open; #" + i + " " + getParameterName( i ) + "=" + getParameterDisplay( i ) + getParameterLabel( i ) );
                }
#endif
            } catch ( Exception ex ) {
                PortUtil.stderr.println( "AquesToneDriver#open; ex=" + ex );
            }

            if ( refresh_winini ) {
                win32.WriteProfileString( "AquesTone", "FileKoe_00", koe_old );
            }
            return ret;
        }

        private void updatePluginUiRect() {
            if ( pluginUi != null ) {
                win32.EnumChildWindows( pluginUi.Handle, enumChildProc, 0 );
            }
        }

        private bool enumChildProc( IntPtr hwnd, int lParam ) {
            RECT rc = new RECT();
            win32.GetWindowRect( hwnd, ref rc );
            uiWindowRect = new Dimension( rc.right - rc.left, rc.bottom - rc.top );
            return false; //最初のやつだけ検出できればおｋなので
        }

        public override void close() {
            if ( pluginUi != null ) {
                pluginUi.Close();
            }
            base.close();
        }

        private static String getKoeFilePath() {
            String ret = PortUtil.combinePath( AppManager.getCadenciiTempDir(), "jphonefifty.txt" );
            if ( !PortUtil.isFileExists( ret ) ) {
                BufferedWriter bw = null;
                try {
                    bw = new BufferedWriter( new OutputStreamWriter( new FileOutputStream( ret ), "Shift_JIS" ) );
                    foreach ( String s in PHONES ) {
                        bw.write( s ); bw.newLine();
                    }
                } catch ( Exception ex ) {
                    PortUtil.stderr.println( "AquesToneDriver#getKoeFilePath; ex=" + ex );
                } finally {
                    if ( bw != null ) {
                        try {
                            bw.close();
                        } catch ( Exception ex2 ) {
                            PortUtil.stderr.println( "AquesToneDriver#getKoeFilePath; ex=" + ex2 );
                        }
                    }
                }
            }
            return ret;
        }
    }

}
#endif
