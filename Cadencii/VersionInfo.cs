/*
 * VersionInfo.cs
 * Copyright © 2008-2011 kbinani
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

package com.github.cadencii;

//INCLUDE-SECTION IMPORT ./ui/java/VersionInfo.java

import java.awt.*;
import java.awt.event.*;
import java.awt.image.*;
import com.github.cadencii.*;
import com.github.cadencii.windows.forms.*;
import com.github.cadencii.apputil.*;

#else

using System;
using System.Windows.Forms;
using com.github.cadencii.apputil;
using com.github.cadencii;
using com.github.cadencii.java.awt;
using com.github.cadencii.java.awt.image;
using com.github.cadencii.windows.forms;

namespace com.github.cadencii
{
    using BEventArgs = System.EventArgs;
    using BKeyEventArgs = System.Windows.Forms.KeyEventArgs;
    using boolean = System.Boolean;
    using BPaintEventArgs = System.Windows.Forms.PaintEventArgs;
    using BEventHandler = System.EventHandler;
    using Graphics = com.github.cadencii.java.awt.Graphics2D;
#endif

#if JAVA
    public class VersionInfo extends BDialog {
#else
    public class VersionInfo : BDialog
    {
#endif
        const float m_speed = 35f;
        const int m_height = 380;
#if JAVA
        const int FONT_SIZE = 12;
#else
        const int FONT_SIZE = 10;
#endif

        private readonly Color m_background = Color.white;

        private double m_scroll_started;
        private AuthorListEntry[] m_credit;
        private String m_version;
        private boolean m_credit_mode = false;
        private float m_last_t = 0f;
        private float m_last_speed = 0f;
        private float m_shift = 0f;
        private int m_button_width_about = 75;
        private int m_button_width_credit = 75;
        private BufferedImage m_scroll = null;
        private BufferedImage m_scroll_with_id = null;
        private String m_app_name = "";
        private Color m_app_name_color = Color.black;
        private Color m_version_color = new Color( 105, 105, 105 );
        private boolean m_shadow_enablde = false;
        private BTimer timer;
        private boolean m_show_twitter_id = false;

        public VersionInfo( String app_name, String version )
        {
#if JAVA
            super();
            initialize();
            timer = new BTimer();
#else
            InitializeComponent();
            if ( this.components == null ) {
                this.components = new System.ComponentModel.Container();
            }
            timer = new BTimer( this.components );
#endif
            m_version = version;
            m_app_name = app_name;

            timer.setDelay( 30 );
            registerEventHandlers();
            setResources();
            applyLanguage();

#if !JAVA
            this.SetStyle( ControlStyles.DoubleBuffer, true );
            this.SetStyle( ControlStyles.UserPaint, true );
            this.SetStyle( ControlStyles.AllPaintingInWmPaint, true );
#endif

            m_credit = new AuthorListEntry[] { };
            lblVstLogo.setForeground( m_version_color );
#if DEBUG
            //m_scroll = generateAuthorListB( false );
            //m_scroll_with_id = generateAuthorListB( true );
#endif
            chkTwitterID.setVisible( false );
        }

        public boolean isShowTwitterID()
        {
            return m_show_twitter_id;
        }

        public void setShowTwitterID( boolean value )
        {
            m_show_twitter_id = value;
        }

        public void applyLanguage()
        {
            String about = PortUtil.formatMessage( _( "About {0}" ), m_app_name );
            String credit = _( "Credit" );
            Dimension size1 = Util.measureString( about, btnFlip.getFont() );
            Dimension size2 = Util.measureString( credit, btnFlip.getFont() );
            m_button_width_about = Math.Max( 75, (int)(size1.width * 1.3) );
            m_button_width_credit = Math.Max( 75, (int)(size2.width * 1.3) );
            if ( m_credit_mode ) {
#if !JAVA
                btnFlip.setPreferredSize( new Dimension( m_button_width_about, btnFlip.getHeight() ) );
#endif
                btnFlip.setText( about );
            } else {
#if !JAVA
                btnFlip.setPreferredSize( new Dimension( m_button_width_credit, btnFlip.getHeight() ) );
#endif
                btnFlip.setText( credit );
            }
            setTitle( about );
        }

        public static String _( String s )
        {
            return Messaging.getMessage( s );
        }

        /// <summary>
        /// バージョン番号表示の文字色を取得または設定します
        /// </summary>
        public Color getVersionColor()
        {
            return m_version_color;
        }

        public void setVersionColor( Color value )
        {
            m_version_color = value;
            lblVstLogo.setForeground( value );
        }

        /// <summary>
        /// アプリケーション名表示の文字色を取得または設定します
        /// </summary>
        public Color getAppNameColor()
        {
            return m_app_name_color;
        }

        public void setAppNameColor( Color value )
        {
            m_app_name_color = value;
        }

#if !JAVA
        public void setCredit( BufferedImage value )
        {
            m_scroll = value;
        }
#endif

        public String getAppName()
        {
            return m_app_name;
        }

        public void setAppName( String value )
        {
            m_app_name = value;
        }

        public void setAuthorList( AuthorListEntry[] value )
        {
            m_credit = value;
            m_scroll = generateAuthorListB( false );
            m_scroll_with_id = generateAuthorListB( true );
        }

        private BufferedImage generateAuthorListB( boolean show_twitter_id )
        {
            int shadow_shift = 2;
            String font_name = "Arial";
            Font font = new Font( font_name, java.awt.Font.PLAIN, FONT_SIZE );
            Dimension size = Util.measureString( "the quick brown fox jumped over the lazy dogs. THE QUICK BROWN FOX JUMPED OVER THE LAZY DOGS. 0123456789", font );
            int width = getWidth();
            int height = size.height;
            //StringFormat sf = new StringFormat();
            BufferedImage ret = new BufferedImage( (int)width, (int)(40f + m_credit.Length * height * 1.1f), BufferedImage.TYPE_INT_BGR );
            Graphics2D g = ret.createGraphics();
            g.setColor( Color.white );
            g.fillRect( 0, 0, ret.getWidth( null ), ret.getHeight( null ) );
            int align = 0;
            int valign = 0;
            //sf.Alignment = StringAlignment.Center;
            Font f = new Font( font_name, java.awt.Font.BOLD, (int)(FONT_SIZE * 1.2f) );
            if ( m_shadow_enablde ) {
                g.setColor( new Color( 0, 0, 0, 40 ) );
                PortUtil.drawStringEx(
                    g,
                    m_app_name,
                    f,
#if JAVA
                    new Rectangle( shadow_shift, shadow_shift + height, width, height ),
#else
                    new Rectangle( shadow_shift, shadow_shift, width, height ),
#endif
                    align,
                    valign );
            }
            g.setColor( Color.black );
            PortUtil.drawStringEx(
                g,
                m_app_name,
                f,
#if JAVA
                new Rectangle( 0, height, width, height ),
#else
                new Rectangle( 0, 0, width, height ),
#endif
                align,
                valign );
            for ( int i = 0; i < m_credit.Length; i++ ) {
                AuthorListEntry itemi = m_credit[i];
                Font f2 = new Font( font_name, itemi.getStyle(), FONT_SIZE );
                String id = show_twitter_id ? itemi.getTwitterID() : "";
                if ( id == null ) {
                    id = "";
                }
                String str = itemi.getName() + (id.Equals( "" ) ? "" : (" (" + id + ")"));
                if ( m_shadow_enablde ) {
                    g.setColor( new Color( 0, 0, 0, 40 ) );
                    PortUtil.drawStringEx(
                        g,
                        str,
                        font,
#if JAVA
                        new Rectangle( 0 + shadow_shift, 40 + (int)(i * height * 1.1) + shadow_shift + height, width, height ),
#else
                        new Rectangle( 0 + shadow_shift, 40 + (int)(i * height * 1.1) + shadow_shift, width, height ),
#endif
                        align,
                        valign );
                }
                g.setColor( Color.black );
                PortUtil.drawStringEx( 
                    g,
                    str,
                    f2,
#if JAVA
                    new Rectangle( 0, 40 + (int)(i * height * 1.1) + height, width, height ),
#else
                    new Rectangle( 0, 40 + (int)(i * height * 1.1), width, height ),
#endif
                    align,
                    valign );
            }
            return ret;
        }

        void btnSaveAuthorList_Click( Object sender, BEventArgs e )
        {
#if !JAVA
#if DEBUG
            SaveFileDialog dlg = null;
            try {
                dlg = new SaveFileDialog();
                if ( dlg.ShowDialog() == DialogResult.OK ) {
                    javax.imageio.ImageIO.write( m_scroll, "png", new java.io.File( dlg.FileName ) );
                }
            } catch ( Exception ex ) {
                Logger.write( typeof( VersionInfo ) + ".btnSaveAuthorList_Click; ex=" + ex + "\n" );
            } finally {
                if ( dlg != null ) {
                    dlg.Dispose();
                }
            }
#endif
#endif
        }

        public void btnOK_Click( Object sender, BEventArgs e )
        {
            setDialogResult( BDialogResult.OK );
            timer.stop();
            close();
        }

        public void btnFlip_Click( Object sender, BEventArgs e )
        {
            m_credit_mode = !m_credit_mode;
            if ( m_credit_mode ) {
                try {
                    btnFlip.setText( PortUtil.formatMessage( _( "About {0}" ), m_app_name ) );
                } catch ( Exception ex ) {
                    btnFlip.setText( "About " + m_app_name );
                }
                m_scroll_started = PortUtil.getCurrentTime();
                m_last_speed = 0f;
                m_last_t = 0f;
                m_shift = 0f;
                pictVstLogo.setVisible( false );
                lblVstLogo.setVisible( false );
                chkTwitterID.setVisible( true );
                timer.start();
            } else {
                timer.stop();
                btnFlip.setText( _( "Credit" ) );
                pictVstLogo.setVisible( true );
                lblVstLogo.setVisible( true );
                chkTwitterID.setVisible( false );
            }
            this.repaint();
        }

        public void timer_Tick( Object sender, BEventArgs e )
        {
#if JAVA
            this.repaint();
#else
            invalidate();
#endif
        }

        public void VersionInfo_Paint( Object sender, BPaintEventArgs e )
        {
            try {
#if JAVA
                e.Graphics.setColor( Color.white );
                e.Graphics.fillRect( 0, 0, this.getWidth(), this.getHeight() );
                paintCor( e.Graphics );
#else
                paintCor( new Graphics2D( e.Graphics ) );
#endif
            } catch ( Exception ex ) {
#if DEBUG
                serr.println( "VersionInfo_Paint; ex=" + ex );
#endif
            }
        }

        private void paintCor( Graphics g1 )
        {
            Graphics2D g = (Graphics2D)g1;
            g.clipRect( 0, 0, getWidth(), m_height );
            g.setColor( Color.white );
            g.fillRect( 0, 0, getWidth(), getHeight() );
            //g.clearRect( 0, 0, getWidth(), getHeight() );
            if ( m_credit_mode ) {
                float times = (float)(PortUtil.getCurrentTime() - m_scroll_started) - 3f;
                float speed = (float)((2.0 - math.erfc( times * 0.8 )) / 2.0) * m_speed;
                float dt = times - m_last_t;
                m_shift += (speed + m_last_speed) * dt / 2f;
                m_last_t = times;
                m_last_speed = speed;
                BufferedImage image = m_show_twitter_id ? m_scroll_with_id : m_scroll;
                if ( image != null ) {
                    float dx = (getWidth() - image.getWidth( null )) * 0.5f;
                    g.drawImage( image, (int)dx, (int)(90f - m_shift), null );
                    if ( 90f - m_shift + image.getHeight( null ) < 0 ) {
                        m_shift = -m_height * 1.5f;
                    }
                }
                int grad_height = 60;
                Rectangle top = new Rectangle( 0, 0, getWidth(), grad_height );
                Rectangle bottom = new Rectangle( 0, m_height - grad_height, getWidth(), grad_height );
                g.clipRect( 0, m_height - grad_height + 1, getWidth(), grad_height - 1 );
                g.setClip( null );
            } else {
                g.setFont( new Font( "Century Gorhic", java.awt.Font.BOLD, FONT_SIZE * 2 ) );
                g.setColor( m_app_name_color );
                g.drawString( m_app_name, 20, 60 );
                g.setFont( new Font( "Arial", 0, FONT_SIZE ) );
                String[] spl = PortUtil.splitString( m_version, '\n' );
                int y = 100;
                int delta = (int)(FONT_SIZE * 1.1);
                if( delta == FONT_SIZE ){
                    delta++;
                }
                for( int i = 0; i < spl.Length; i++ ){
                    g.drawString( (i == 0 ? "version" : "") + spl[i], 25, y );
                    y += delta;
                }
            }
        }

        private void VersionInfo_KeyDown( Object sender, BKeyEventArgs e )
        {
#if JAVA
            if( (e.getKeyCode() & KeyEvent.VK_ESCAPE) == KeyEvent.VK_ESCAPE )
#else
            if ( (e.KeyCode & Keys.Escape) == Keys.Escape )
#endif
 {
                setDialogResult( BDialogResult.CANCEL );
                close();
            }
        }

        private void VersionInfo_FontChanged( Object sender, BEventArgs e )
        {
#if JAVA
            Util.applyFontRecurse( this, getFont() );
#else
            for ( int i = 0; i < this.Controls.Count; i++ ) {
                Util.applyFontRecurse( this.Controls[i], new java.awt.Font( this.Font ) );
            }
#endif
        }

        public void chkTwitterID_CheckedChanged( Object sender, BEventArgs e )
        {
            m_show_twitter_id = chkTwitterID.isSelected();
            repaint();
        }

        private void registerEventHandlers()
        {
#if JAVA
            this.panelCredit.paintEvent.add( new BPaintEventHandler( this, "VersionInfo_Paint" ) );
#else
            this.Paint += new System.Windows.Forms.PaintEventHandler( this.VersionInfo_Paint );
            this.KeyDown += new System.Windows.Forms.KeyEventHandler( this.VersionInfo_KeyDown );
            this.FontChanged += new BEventHandler( this.VersionInfo_FontChanged );
#endif
            this.timer.Tick += new BEventHandler( timer_Tick );
            this.btnFlip.Click += new BEventHandler( btnFlip_Click );
            this.btnOK.Click += new BEventHandler( btnOK_Click );
            this.chkTwitterID.CheckedChanged += new BEventHandler( chkTwitterID_CheckedChanged );
        }

        private void setResources()
        {
            pictVstLogo.setImage( Resources.get_VSTonWht() );
        }

        #region ui implementation
#if JAVA
        //INCLUDE-SECTION FIELD ./ui/java/VersionInfo.java
        //INCLUDE-SECTION METHOD ./ui/java/VersionInfo.java
#else
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose( boolean disposing )
        {
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
        private void InitializeComponent()
        {
            this.btnFlip = new com.github.cadencii.windows.forms.BButton();
            this.btnOK = new com.github.cadencii.windows.forms.BButton();
            this.lblVstLogo = new com.github.cadencii.windows.forms.BLabel();
            this.pictVstLogo = new com.github.cadencii.windows.forms.BPictureBox();
            this.chkTwitterID = new com.github.cadencii.windows.forms.BCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictVstLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // btnFlip
            // 
            this.btnFlip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnFlip.Location = new System.Drawing.Point( 13, 391 );
            this.btnFlip.Name = "btnFlip";
            this.btnFlip.Size = new System.Drawing.Size( 75, 21 );
            this.btnFlip.TabIndex = 2;
            this.btnFlip.Text = "クレジット";
            this.btnFlip.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point( 211, 391 );
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size( 75, 21 );
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // lblVstLogo
            // 
            this.lblVstLogo.BackColor = System.Drawing.Color.White;
            this.lblVstLogo.Location = new System.Drawing.Point( 25, 277 );
            this.lblVstLogo.Name = "lblVstLogo";
            this.lblVstLogo.Size = new System.Drawing.Size( 263, 32 );
            this.lblVstLogo.TabIndex = 5;
            this.lblVstLogo.Text = "VST PlugIn Technology by Steinberg Media Technologies GmbH";
            // 
            // pictVstLogo
            // 
            this.pictVstLogo.BackColor = System.Drawing.Color.White;
            this.pictVstLogo.Location = new System.Drawing.Point( 27, 304 );
            this.pictVstLogo.Name = "pictVstLogo";
            this.pictVstLogo.Size = new System.Drawing.Size( 88, 60 );
            this.pictVstLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictVstLogo.TabIndex = 4;
            this.pictVstLogo.TabStop = false;
            // 
            // chkTwitterID
            // 
            this.chkTwitterID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkTwitterID.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkTwitterID.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkTwitterID.Location = new System.Drawing.Point( 148, 391 );
            this.chkTwitterID.Name = "chkTwitterID";
            this.chkTwitterID.Size = new System.Drawing.Size( 57, 21 );
            this.chkTwitterID.TabIndex = 8;
            this.chkTwitterID.Text = "Twtr ID";
            this.chkTwitterID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkTwitterID.UseVisualStyleBackColor = true;
            // 
            // VersionInfo
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size( 300, 419 );
            this.Controls.Add( this.chkTwitterID );
            this.Controls.Add( this.pictVstLogo );
            this.Controls.Add( this.lblVstLogo );
            this.Controls.Add( this.btnOK );
            this.Controls.Add( this.btnFlip );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size( 306, 451 );
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size( 306, 451 );
            this.Name = "VersionInfo";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VersionInfoEx";
            ((System.ComponentModel.ISupportInitialize)(this.pictVstLogo)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private BButton btnFlip;
        private BButton btnOK;
        private BPictureBox pictVstLogo;
        private BLabel lblVstLogo;
        private BCheckBox chkTwitterID;
#endif
        #endregion
    }

#if !JAVA
}
#endif
