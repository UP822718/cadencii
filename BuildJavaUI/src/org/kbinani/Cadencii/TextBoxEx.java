package org.kbinani.Cadencii;

import java.awt.BorderLayout;
import java.awt.Component;
import java.awt.Frame;
import java.awt.Point;
import java.awt.event.ComponentEvent;
import java.awt.event.ComponentListener;
import java.awt.event.KeyEvent;
import java.awt.event.KeyListener;
import java.awt.event.WindowEvent;
import java.awt.event.WindowFocusListener;
import javax.swing.JPanel;
import javax.swing.JTextField;
import javax.swing.JWindow;
import org.kbinani.BEvent;
import org.kbinani.windows.forms.BKeyEventArgs;
import org.kbinani.windows.forms.BKeyEventHandler;

public class TextBoxEx extends JWindow implements WindowFocusListener, ComponentListener, KeyListener {
    private JPanel jContentPane = null;
    private JTextField jTextField = null;

    public void selectAll(){
        jTextField.selectAll();
    }
    
    public String getText(){
        return jTextField.getText();
    }

    public void setText( String value ){
        jTextField.setText( value );
    }
    
    public boolean isImeModeOn(){
        return jTextField.getInputContext().isCompositionEnabled();
    }

    public void setImeModeOn( boolean value ){
        jTextField.getInputContext().setCompositionEnabled( value );
    }

    /* REGION bocoree.windows.forms.[component] */
    /* root implementation of bocoree.windows.forms.[component] is in BTextBox.cs */
    private Object m_tag = null;

    public Object getTag(){
        return m_tag;
    }

    public void setTag( Object value ){
        m_tag = value;
    }
    /* END REGION */

    /* REGION java.awt.Component */
    /* root implementation of java.awt.Component is in BForm.cs(java) */
    public BEvent<BKeyEventHandler> keyUpEvent = new BEvent<BKeyEventHandler>();
    public BEvent<BKeyEventHandler> keyDownEvent = new BEvent<BKeyEventHandler>();
    public BEvent<BKeyEventHandler> keyPressedEvent = new BEvent<BKeyEventHandler>();

    public void keyPressed( KeyEvent e0 ){
        try{
            BKeyEventArgs e = new BKeyEventArgs( e0 );
            keyDownEvent.raise( this, e );
        }catch( Exception ex ){
            System.err.println( "BForm#keyPressed; ex=" + ex );
        }
    }

    public void keyReleased( KeyEvent e0 ){
        try{
            BKeyEventArgs e = new BKeyEventArgs( e0 );
            keyUpEvent.raise( this, e );
        }catch( Exception ex ){
            System.err.println( "BForm#keyReleased; ex=" + ex );
        }
    }

    public void keyTyped( KeyEvent e0 ){
        try{
            BKeyEventArgs e = new BKeyEventArgs( e0 );
            keyPressedEvent.raise( this, e );
        }catch( Exception ex ){
            System.err.println( "BForm#keyTyped; ex=" + ex );
        }
    }
    /* END REGION java.awt.Component */

    public void windowGainedFocus( WindowEvent e ){
        System.out.println( "focusGained" );
    }
    
    public void windowLostFocus( WindowEvent e ){
        System.out.println( "focusLost" );
        setVisible( false );
    }

    public void componentHidden(ComponentEvent e){
    }

    public void componentMoved(ComponentEvent e){
        Component parent = e.getComponent();
        Point p = parent.getLocation();
        setLocation( p );
    }

    public void componentResized(ComponentEvent e){
    }

    public void componentShown(ComponentEvent e){
    }
     
    /*public static void main( String[] args ){
        JFrame empty = new JFrame( "empty owner" );
        OnScreenInputTextBox textbox = new OnScreenInputTextBox( empty );

        empty.setDefaultCloseOperation( JFrame.EXIT_ON_CLOSE );
        empty.setSize( 200, 100 );
        empty.setLocation( 100, 100 );
        empty.setVisible( true );
        empty.addComponentListener( textbox );
        
        Point loc = empty.getLocationOnScreen();
        textbox.setAlwaysOnTop( true );
        textbox.setLocation( loc );
        textbox.setVisible( true );
        textbox.requestFocus();
    }*/

    /**
     * @param owner
     */
    public TextBoxEx(Frame owner) {
        super( owner );
        initialize();
        addWindowFocusListener( this );
        jTextField.addKeyListener( this );
    }

    /**
     * This method initializes this
     * 
     * @return void
     */
    private void initialize() {
        this.setSize(115, 22);
        this.setContentPane(getJContentPane());
    }

    /**
     * This method initializes jContentPane
     * 
     * @return javax.swing.JPanel
     */
    private JPanel getJContentPane() {
        if (jContentPane == null) {
            jContentPane = new JPanel();
            jContentPane.setLayout(new BorderLayout());
            jContentPane.add(getJTextField(), BorderLayout.CENTER);
        }
        return jContentPane;
    }

    /**
     * This method initializes jTextField   
     *  
     * @return javax.swing.JTextField   
     */
    private JTextField getJTextField() {
        if (jTextField == null) {
            jTextField = new JTextField();
        }
        return jTextField;
    }

}