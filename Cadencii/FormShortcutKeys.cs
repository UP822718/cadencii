﻿/*
 * FormShortcutKeys.cs
 * Copyright (C) 2009-2010 kbinani
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
#if JAVA
package org.kbinani.cadencii;

//INCLUDE-SECTION IMPORT ..\BuildJavaUI\src\org\kbinani\Cadencii\FormShortcutKeys.java

import java.awt.event.*;
import java.util.*;
import javax.swing.*;
import org.kbinani.*;
import org.kbinani.apputil.*;
import org.kbinani.windows.forms.*;
#else
using System;
using System.Windows.Forms;
using org.kbinani.apputil;
using org.kbinani;
using org.kbinani.java.awt.event_;
using org.kbinani.java.util;
using org.kbinani.javax.swing;
using org.kbinani.windows.forms;

namespace org.kbinani.cadencii {
    using BEventArgs = System.EventArgs;
    using BFormClosingEventArgs = System.Windows.Forms.FormClosingEventArgs;
    using BKeyEventArgs = System.Windows.Forms.KeyEventArgs;
    using boolean = System.Boolean;
    using BPreviewKeyDownEventArgs = System.Windows.Forms.PreviewKeyDownEventArgs;
    using java = org.kbinani.java;
#endif

#if JAVA
    public class FormShortcutKeys extends BForm{
#else
    public class FormShortcutKeys : BForm {
#endif
        private BMenuItem m_dumy;
        private TreeMap<String, ValuePair<String, BKeys[]>> m_dict;
        private TreeMap<String, ValuePair<String, BKeys[]>> m_first_dict;
        private static int columnWidthCommand = 240;
        private static int columnWidthShortcutKey = 140;

        public FormShortcutKeys( TreeMap<String, ValuePair<String, BKeys[]>> dict ) {
#if JAVA
            super();
#endif
            try {
#if JAVA
                initialize();
#else
                InitializeComponent();
#endif
            } catch ( Exception ex ) {
#if DEBUG
                Console.WriteLine( "FormShortcutKeys#.ctor; ex=" + ex );
#endif
            }

#if DEBUG
            PortUtil.println( "FormShortcutKeys#.ctor; dict.size()=" + dict.size() );
#endif
            list.setColumnHeaders( new String[] { "Command", "Shortcut Key" } );
            list.setColumnWidth( 0, columnWidthCommand );
            list.setColumnWidth( 1, columnWidthShortcutKey );

            registerEventHandlers();
            setResources();
            ApplyLanguage();

            m_dict = dict;
            m_dumy = new BMenuItem();
#if !JAVA
            m_dumy.ShowShortcutKeys = true;
#endif
            m_first_dict = new TreeMap<String, ValuePair<String, BKeys[]>>();
            CopyDict( m_dict, m_first_dict );
            UpdateList();
            Util.applyFontRecurse( this, AppManager.editorConfig.getBaseFont() );
        }

        public void ApplyLanguage() {
            setTitle( _( "Shortcut Config" ) );

            btnOK.setText( _( "OK" ) );
            btnCancel.setText( _( "Cancel" ) );
            btnRevert.setText( _( "Revert" ) );
            btnLoadDefault.setText( _( "Load Default" ) );

            list.setColumnHeaders( new String[] { _( "Command" ), _( "Shortcut Key" ) } );
#if JAVA
            System.err.println( "info; FormShortcutKeys#ApplyLanguage; \"toolTip.SetToolTip( list, _( \"Select command and hit key(s) you want to set.\\nHit Backspace if you want to remove shortcut key.\" ) )" );
#else
            toolTip.SetToolTip( list, _( "Select command and hit key(s) you want to set.\nHit Backspace if you want to remove shortcut key." ) );
#endif

            int num_groups = list.getGroupCount();
            for ( int i = 0; i < num_groups; i++ ) {
                String name = list.getGroupNameAt( i );
                if ( name.Equals( "listGroupFile" ) ) {
                    list.setGroupHeader( name, _( "File" ) );
                } else if ( name.Equals( "listGroupEdit" ) ) {
                    list.setGroupHeader( name, _( "Edit" ) );
                } else if ( name.Equals( "listGroupVisual" ) ) {
                    list.setGroupHeader( name, _( "View" ) );
                } else if ( name.Equals( "listGroupJob" ) ) {
                    list.setGroupHeader( name, _( "Job" ) );
                } else if ( name.Equals( "listGroupLyric" ) ) {
                    list.setGroupHeader( name, _( "Lyrics" ) );
                } else if ( name.Equals( "listGroupSetting" ) ) {
                    list.setGroupHeader( name, _( "Setting" ) );
                } else if ( name.Equals( "listGroupHelp" ) ) {
                    list.setGroupHeader( name, _( "Help" ) );
                } else if ( name.Equals( "listGroupTrack" ) ) {
                    list.setGroupHeader( name, _( "Track" ) );
                } else if ( name.Equals( "listGroupScript" ) ) {
                    list.setGroupHeader( name, _( "Script" ) );
                } else if ( name.Equals( "listGroupOther" ) ) {
                    list.setGroupHeader( name, _( "Others" ) );
                }
            }
        }

        private static String _( String id ) {
            return Messaging.getMessage( id );
        }

        public TreeMap<String, ValuePair<String, BKeys[]>> getResult() {
            TreeMap<String, ValuePair<String, BKeys[]>> ret = new TreeMap<String, ValuePair<String, BKeys[]>>();
            CopyDict( m_dict, ret );
            return ret;
        }

        private static void CopyDict( TreeMap<String, ValuePair<String, BKeys[]>> src, TreeMap<String, ValuePair<String, BKeys[]>> dest ) {
            dest.clear();
            for ( Iterator itr = src.keySet().iterator(); itr.hasNext(); ) {
                String name = (String)itr.next();
                String key = src.get( name ).getKey();
                BKeys[] values = src.get( name ).getValue();
                Vector<BKeys> cp = new Vector<BKeys>();
                foreach ( BKeys k in values ) {
                    cp.add( k );
                }
                dest.put( name, new ValuePair<String, BKeys[]>( key, cp.toArray( new BKeys[] { } ) ) );
            }
        }

        private void UpdateList() {
            list.clear();
            for ( Iterator itr = m_dict.keySet().iterator(); itr.hasNext(); ) {
                String display = (String)itr.next();
                Vector<BKeys> a = new Vector<BKeys>();
                foreach ( BKeys key in m_dict.get( display ).getValue() ) {
                    a.add( key );
                }
                try {
                    m_dumy.setAccelerator( PortUtil.getKeyStrokeFromBKeys( a.toArray( new BKeys[] { } ) ) );
                } catch( Exception ex ) {
                    a.clear();
                }
                BListViewItem item = new BListViewItem( new String[] { display, AppManager.getShortcutDisplayString( a.toArray( new BKeys[] { } ) ) } );
                String name = m_dict.get( display ).getKey();
                item.setName( name );
                //item.Tag = a;
                String group = "";
#if DEBUG
                PortUtil.println( "FormShortcutKeys#UpdateList; name=" + name );
#endif
                if ( name.StartsWith( "menuFile" ) ) {
                    group = "listGroupFile";
                } else if ( name.StartsWith( "menuEdit" ) ) {
                    group = "listGroupEdit";
                } else if ( name.StartsWith( "menuVisual" ) ) {
                    group = "listGroupVisual";
                } else if ( name.StartsWith( "menuJob" ) ) {
                    group = "listGroupJob";
                } else if ( name.StartsWith( "menuLyric" ) ) {
                    group = "listGroupLyric";
                } else if ( name.StartsWith( "menuTrack" ) ) {
                    group = "listGroupTrack";
                } else if ( name.StartsWith( "menuScript" ) ) {
                    group = "listGroupScript";
                } else if ( name.StartsWith( "menuSetting" ) ) {
                    group = "listGroupSetting";
                } else if ( name.StartsWith( "menuHelp" ) ) {
                    group = "listGroupHelp";
                } else {
                    group = "listGroupOther";
                }
                list.addItem( group, item );
            }
            UpdateColor();
            ApplyLanguage();
        }

        private void list_PreviewKeyDown( Object sender, BPreviewKeyDownEventArgs e ) {
        }

        private void list_KeyDown( Object sender, BKeyEventArgs e ) {
            String selected_group = "";
            int selected_index = -1;
            int num_groups = list.getGroupCount();
            for ( int i = 0; i < num_groups; i++ ) {
                String name = list.getGroupNameAt( i );
                int indx = list.getSelectedIndex( name );
                if ( indx >= 0 ) {
                    selected_group = name;
                    selected_index = indx;
                    break;
                }
            }

            if ( selected_index < 0 ) {
                return;
            }
            int index = selected_index;
#if JAVA
            KeyStroke stroke = KeyStroke.getKeyStroke( e.getKeyCode(), e.getModifiers() );
#else
            KeyStroke stroke = KeyStroke.getKeyStroke( 0, 0 );
            stroke.keys = e.KeyCode | e.Modifiers;
#endif
            int code = stroke.getKeyCode();
            int modifier = stroke.getModifiers();

            Vector<BKeys> capturelist = new Vector<BKeys>();
            BKeys capture = BKeys.None;
            for ( Iterator itr = AppManager.SHORTCUT_ACCEPTABLE.iterator(); itr.hasNext(); ) {
                BKeys k = (BKeys)itr.next();
#if JAVA
                if( code == k.getValue() ){
#else
                if ( code == (int)k ) {
#endif
                    capturelist.add( k );
                    if ( (modifier & InputEvent.ALT_MASK) == InputEvent.ALT_MASK ) {
                        capturelist.add( BKeys.Alt );
                    }
                    if ( (modifier & InputEvent.CTRL_MASK) == InputEvent.CTRL_MASK ) {
                        capturelist.add( BKeys.Control );
                    }
                    if ( (modifier & InputEvent.SHIFT_MASK) == InputEvent.SHIFT_MASK ) {
                        capturelist.add( BKeys.Shift );
                    }
                    capture = k;
                    break;
                }
            }

            // 指定されたキーの組み合わせが、ショートカットとして適切かどうか判定
            try {
#if JAVA
                //m_dumy.setAccelerator( KeyStroke.getKeyStroke( capture.getValue(), modifier ) );
#else
                //m_dumy.setAccelerator( KeyStroke.getKeyStroke( (int)capture, modifier ) );
#endif
            } catch ( Exception ex ) {
#if JAVA
                System.err.println( "info; FormShortcutKeys#list_KeyDown; not implemented yet \"e.KeyCode & Keys.Up ... e.Handled = true\"" );
#else
                if ( ((e.KeyCode & Keys.Up) != Keys.Up) &&
                     ((e.KeyCode & Keys.Down) != Keys.Down) ) {
                    e.Handled = true;
                }
#endif
                return;
            }
            BListViewItem item = list.getItemAt( selected_group, index );
            item.setSubItemAt( 1, AppManager.getShortcutDisplayString( capturelist.toArray( new BKeys[] { } ) ) );
            list.setItemAt( selected_group, index, item );
            String display = list.getItemAt( selected_group, index ).getSubItemAt( 0 );
            if ( m_dict.containsKey( display ) ) {
                m_dict.get( display ).setValue( capturelist.toArray( new BKeys[] { } ) );
            }
            UpdateColor();
#if !JAVA
            e.Handled = true;
#endif
        }

        private void btnRevert_Click( Object sender, BEventArgs e ) {
            CopyDict( m_first_dict, m_dict );
            UpdateList();
        }

        private void btnLoadDefault_Click( Object sender, BEventArgs e ) {
            for ( int i = 0; i < EditorConfig.DEFAULT_SHORTCUT_KEYS.size(); i++ ) {
                String name = EditorConfig.DEFAULT_SHORTCUT_KEYS.get( i ).Key;
                BKeys[] keys = EditorConfig.DEFAULT_SHORTCUT_KEYS.get( i ).Value;
                for ( Iterator itr = m_dict.keySet().iterator(); itr.hasNext(); ) {
                    String display = (String)itr.next();
                    if ( name.Equals( m_dict.get( display ).getKey() ) ) {
                        m_dict.get( display ).setValue( keys );
                        break;
                    }
                }
            }
            UpdateList();
        }

        private void UpdateColor() {
            int num_groups = list.getGroupCount();
            for ( int k = 0; k < num_groups; k++ ) {
                String name = list.getGroupNameAt( k );
                for ( int i = 0; i < list.getItemCount( name ); i++ ) {
                    String compare = list.getItemAt( name, i ).getSubItemAt( 1 );
                    if ( compare.Equals( "" ) ) {
                        list.setItemBackColorAt( name, i, java.awt.Color.white );
                        continue;
                    }
                    boolean found = false;
                    for ( int n = 0; n < num_groups; n++ ) {
                        String search_name = list.getGroupNameAt( n );
                        for ( int j = 0; j < list.getItemCount( search_name ); j++ ) {
                            if ( n == k && i == j ) {
                                continue;
                            }
                            if ( compare.Equals( list.getItemAt( search_name, j ).getSubItemAt( 1 ) ) ) {
                                found = true;
                                break;
                            }
                        }
                        if ( found ) {
                            break;
                        }
                    }
                    if ( found ) {
                        list.setItemBackColorAt( name, i, java.awt.Color.yellow );
                    } else {
                        list.setItemBackColorAt( name, i, java.awt.Color.white );
                    }
                }
            }
        }

        private void FormShortcutKeys_FormClosing( Object sender, BFormClosingEventArgs e ) {
            columnWidthCommand = list.getColumnWidth( 0 );
            columnWidthShortcutKey = list.getColumnWidth( 1 );
#if DEBUG
            Console.WriteLine( "FormShortcutKeys_FormClosing; columnWidthCommand,columnWidthShortcutKey=" + columnWidthCommand + "," + columnWidthShortcutKey );
#endif
        }

        private void btnCancel_Click( Object sender, BEventArgs e ) {
            setDialogResult( BDialogResult.CANCEL );
        }

        private void btnOK_Click( Object sender, BEventArgs e ) {
            setDialogResult( BDialogResult.OK );
        }

        private void registerEventHandlers() {
#if JAVA
#else
            this.list.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler( this.list_PreviewKeyDown );
            this.list.KeyDown += new System.Windows.Forms.KeyEventHandler( this.list_KeyDown );
            this.btnLoadDefault.Click += new System.EventHandler( this.btnLoadDefault_Click );
            this.btnRevert.Click += new System.EventHandler( this.btnRevert_Click );
            this.FormClosing += new FormClosingEventHandler( FormShortcutKeys_FormClosing );
            btnOK.Click += new EventHandler( btnOK_Click );
            btnCancel.Click += new EventHandler( btnCancel_Click );
#endif
        }

        private void setResources() {
        }

#if JAVA
        #region UI Impl for Java
        //INCLUDE-SECTION FIELD ..\BuildJavaUI\src\org\kbinani\Cadencii\FormShortcutKeys.java
        //INCLUDE-SECTION METHOD ..\BuildJavaUI\src\org\kbinani\Cadencii\FormShortcutKeys.java
        #endregion
#else
        #region UI Impl for C#
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose( boolean disposing ) {
            if ( disposing && (components != null) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.btnCancel = new org.kbinani.windows.forms.BButton();
            this.btnOK = new org.kbinani.windows.forms.BButton();
            this.list = new org.kbinani.windows.forms.BListView();
            this.btnLoadDefault = new org.kbinani.windows.forms.BButton();
            this.btnRevert = new org.kbinani.windows.forms.BButton();
            this.toolTip = new System.Windows.Forms.ToolTip( this.components );
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point( 325, 403 );
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size( 75, 23 );
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point( 244, 403 );
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size( 75, 23 );
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // list
            // 
            this.list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.list.FullRowSelect = true;
            this.list.Location = new System.Drawing.Point( 12, 12 );
            this.list.MultiSelect = false;
            this.list.Name = "list";
            this.list.Size = new System.Drawing.Size( 388, 343 );
            this.list.TabIndex = 9;
            this.list.UseCompatibleStateImageBehavior = false;
            this.list.View = System.Windows.Forms.View.Details;
            // 
            // btnLoadDefault
            // 
            this.btnLoadDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoadDefault.Location = new System.Drawing.Point( 113, 361 );
            this.btnLoadDefault.Name = "btnLoadDefault";
            this.btnLoadDefault.Size = new System.Drawing.Size( 95, 23 );
            this.btnLoadDefault.TabIndex = 11;
            this.btnLoadDefault.Text = "Load Default";
            this.btnLoadDefault.UseVisualStyleBackColor = true;
            // 
            // btnRevert
            // 
            this.btnRevert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRevert.Location = new System.Drawing.Point( 12, 361 );
            this.btnRevert.Name = "btnRevert";
            this.btnRevert.Size = new System.Drawing.Size( 95, 23 );
            this.btnRevert.TabIndex = 10;
            this.btnRevert.Text = "Revert";
            this.btnRevert.UseVisualStyleBackColor = true;
            // 
            // FormShortcutKeys
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 12F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size( 412, 438 );
            this.Controls.Add( this.btnLoadDefault );
            this.Controls.Add( this.btnRevert );
            this.Controls.Add( this.list );
            this.Controls.Add( this.btnCancel );
            this.Controls.Add( this.btnOK );
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormShortcutKeys";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Shortcut Config";
            this.ResumeLayout( false );

        }

        #endregion

        private BButton btnCancel;
        private BButton btnOK;
        private BListView list;
        private BButton btnLoadDefault;
        private BButton btnRevert;
        private System.Windows.Forms.ToolTip toolTip;
        #endregion
#endif
    }

#if !JAVA
}
#endif
