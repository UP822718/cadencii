/*
 * makeRes.cs
 * Copyright © 2010 kbinani
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
import java.io.*;

class makeRes{
    static String infile = "";
    static String outfile = "";
    static String package_name = "";
    static String name_space = "";

	private static String getDirectoryName( String path )
	{
        File f = new File( path );
        return f.getParent();
	}

	public static String combine( String path1, String path2 )
	{
        if ( path1.endsWith( File.separator ) ) {
            path1 = path1.substring( 0, path1.length() - 1 );
        }
        if ( path2.startsWith( File.separator ) ) {
            path2 = path2.substring( 1 );
        }
        return path1 + File.separator + path2;
	}

	public static String getFileName( String path )
	{
        File f = new File( path );
        return f.getName();
	}

    public static void main( String[] args )
    {
        // 引数を解釈
        String current = "";
        for( String s : args ){
            if( s.startsWith( "-" ) ){
                current = s;
            }else{
                if( current.equals( "-i" ) ){
                    infile = s;
                    current = "";
                }else if( current.equals( "-o" ) ){
                    outfile = s;
                    current = "";
                }else if( current.equals( "-p" ) ){
                    package_name = s;
                    current = "";
                }else if( current.equals( "-n" ) ){
                    name_space = s;
                    current = "";
                }
            }
        }

        if( infile.equals( "" ) || outfile.equals( "" ) ){
            System.out.println( "makeRes:" );
            System.out.println( "    -i    input file" );
            System.out.println( "    -o    output file" );
            System.out.println( "    -p    package name [optional]" );
            System.out.println( "    -n    namespace [optional]" );
            return;
        }
        if( !(new File( infile )).exists() ){
            System.out.println( "error; input file does not exists" );
            return;
        }

		BufferedWriter sw = null;
		BufferedReader sr = null;
		try{
        	sw = new BufferedWriter( new OutputStreamWriter( new FileOutputStream( outfile ), "UTF8" ) );
        	sr = new BufferedReader( new InputStreamReader( new FileInputStream( infile ), "UTF8" ) );
            String basedir = getDirectoryName( infile );
            // header
            String cs_space = (name_space.equals( "" ) ? "" : "    ");
            sw.write( "//this file was autogenerated by makeRes" );
            sw.newLine();
            sw.write( "//makeRes" );
            for( int i = 0; i < args.length; i++ ){
                sw.write( " " + args[i] );
            }
            sw.newLine();
            sw.write( "#if JAVA" );
            sw.newLine();
            if( package_name != "" ){
                sw.write( "package " + package_name + ";" );
                sw.newLine();
                sw.newLine();
            }
			sw.write( "import java.awt.*;" );
			sw.newLine();
			sw.write( "import java.io.*;" );
			sw.newLine();
			sw.write( "import javax.imageio.*;" );
			sw.newLine();
			sw.write( "import javax.swing.*;" );
			sw.newLine();
			sw.write( "import com.github.cadencii.*;" );
			sw.newLine();
			sw.write( "#" + "else" );
			sw.newLine();
			sw.write( "using System;" );
			sw.newLine();
			sw.write( "using System.IO;" );
			sw.newLine();
			sw.write( "using com.github.cadencii;" );
			sw.newLine();
			sw.write( "using com.github.cadencii.java.awt;" );
			sw.newLine();
			sw.newLine();
			if( !name_space.equals( "" ) ){
				sw.write( "namespace " + name_space + "{" );
				sw.newLine();
			}
			sw.write( "#endif" );
			sw.newLine();
			sw.newLine();
			sw.write( cs_space + "public class Resources{" );
			sw.newLine();
			sw.write( cs_space + "    private static String basePath = null;" );
			sw.newLine();
			sw.newLine();
			sw.write( cs_space + "    public static void setBasePath( String value ){" );
			sw.newLine();
			sw.write( cs_space + "        basePath = value;" );
			sw.newLine();
			sw.write( cs_space + "    }" );
			sw.newLine();
			sw.newLine();
			sw.write( cs_space + "    private static String getBasePath(){" );
			sw.newLine();
			sw.write( cs_space + "        if( basePath == null ){" );
			sw.newLine();
			sw.write( cs_space + "            basePath = fsys.combine( PortUtil.getApplicationStartupPath(), \"resources\" );" );
			sw.newLine();
			sw.write( cs_space + "        }" );
			sw.newLine();
			sw.write( cs_space + "        return basePath;" );
			sw.newLine();
			sw.write( cs_space + "    }" );
			sw.newLine();
			sw.newLine();
			String line = "";
            while( (line = sr.readLine()) != null ){
                // 区切り文字を置換
                line = line.replace( "/", File.separator );
                line = line.replace( "\\", File.separator );
                String[] spl = line.split( "\t" );
                if( spl.length < 3 ){
                    continue;
                }
                String name = spl[0];
                String type = spl[1];
                String tpath = spl[2];
                String path = combine( basedir, tpath );

                if( type.equals( "Image" ) ){
                    String instance = "s_" + name;
                    String fname = getFileName( tpath );
					sw.write( cs_space + "    private static Image " + instance + " = null;" );
					sw.newLine();
					sw.write( cs_space + "    public static Image get_" + name + "(){" );
					sw.newLine();
					sw.write( cs_space + "        if( " + instance + " == null ){" );
					sw.newLine();
					sw.write( cs_space + "            String res_path = fsys.combine( getBasePath(), \"" + fname + "\" );" );
					sw.newLine();
					sw.write( cs_space + "            try{" );
					sw.newLine();
					sw.write( "#if JAVA" );
					sw.newLine();
					sw.write( cs_space + "                " + instance + " = ImageIO.read( new File( res_path ) );" );
					sw.newLine();
					sw.write( "#else" );
					sw.newLine();
					sw.write( cs_space + "                " + instance + " = new Image();" );
					sw.newLine();
					sw.write( cs_space + "                " + instance + ".image = new System.Drawing.Bitmap( res_path );" );
					sw.newLine();
					sw.write( "#endif" );
					sw.newLine();
					sw.write( cs_space + "            }catch( Exception ex ){" );
					sw.newLine();
					sw.write( cs_space + "                serr.println( \"Resources#get_" + name + "; ex=\" + ex + \"; res_path=\" + res_path );" );
					sw.newLine();
					sw.write( cs_space + "            }" );
					sw.newLine();
					sw.write( cs_space + "        }" );
					sw.newLine();
					sw.write( cs_space + "        return " + instance + ";" );
					sw.newLine();
					sw.write( cs_space + "    }" );
					sw.newLine();
					sw.newLine();
                }else if( type.equals( "Icon" ) ){
                    String instance = "s_" + name;
                    String fname = getFileName( tpath );
					sw.write( "#if JAVA" );
					sw.newLine();
					sw.write( cs_space + "    private static Image " + instance + " = null;" );
					sw.newLine();
					sw.write( cs_space + "    public static Image get_" + name + "(){" );
					sw.newLine();
					sw.write( "#else" );
					sw.newLine();
					sw.write( cs_space + "    private static System.Drawing.Icon " + instance + " = null;" );
					sw.newLine();
					sw.write( cs_space + "    public static System.Drawing.Icon get_" + name + "(){" );
					sw.newLine();
					sw.write( "#endif" );
					sw.newLine();
					sw.write( cs_space + "        if( " + instance + " == null ){" );
					sw.newLine();
					sw.write( cs_space + "            String res_path = fsys.combine( getBasePath(), \"" + fname + "\" );" );
					sw.newLine();
					sw.write( cs_space + "            try{" );
					sw.newLine();
					sw.write( "#if JAVA" );
					sw.newLine();
					sw.write( cs_space + "                " + instance + " = ImageIO.read( new File( res_path ) );" );
					sw.newLine();
					sw.write( "#else" );
					sw.newLine();
					sw.write( cs_space + "                " + instance + " = new System.Drawing.Icon( res_path );" );
					sw.newLine();
					sw.write( "#endif" );
					sw.newLine();
					sw.write( cs_space + "            }catch( Exception ex ){" );
					sw.newLine();
					sw.write( cs_space + "                serr.println( \"Resources#get_" + name + "; ex=\" + ex + \"; res_path=\" + res_path );" );
					sw.newLine();
					sw.write( cs_space + "            }" );
					sw.newLine();
					sw.write( cs_space + "        }" );
					sw.newLine();
					sw.write( cs_space + "        return " + instance + ";" );
					sw.newLine();
					sw.write( cs_space + "    }" );
					sw.newLine();
					sw.newLine();
                }else if( type.equals( "Cursor" ) ){
                    String instance = "s_" + name;
                    String fname = getFileName( tpath );
			        sw.write( cs_space + "    private static Cursor " + instance + " = null;" );
			        sw.newLine();
			        sw.write( cs_space + "    public static Cursor get_" + name + "(){" );
			        sw.newLine();
			        sw.write( cs_space + "        if( " + instance + " == null ){" );
			        sw.newLine();
			        sw.write( cs_space + "            String res_path = fsys.combine( getBasePath(), \"" + fname + "\" );" );
			        sw.newLine();
			        sw.write( cs_space + "            try{" );
			        sw.newLine();
			        sw.write( "#if JAVA" );
			        sw.newLine();
			        sw.write( cs_space + "                Image img = ImageIO.read( new File( res_path ) );" );
			        sw.newLine();
			        sw.write( cs_space + "                " + instance + " = Toolkit.getDefaultToolkit().createCustomCursor( img, new Point( 0, 0 ), \"" + name + "\" );" );
			        sw.newLine();
			        sw.write( "#else" );
			        sw.newLine();
			        sw.write( cs_space + "                FileStream fs = null;" );
			        sw.newLine();
			        sw.write( cs_space + "                try{" );
			        sw.newLine();
			        sw.write( cs_space + "                    fs = new FileStream( res_path, FileMode.Open, FileAccess.Read );" );
			        sw.newLine();
			        sw.write( cs_space + "                    " + instance + " = new Cursor( fs );" );
			        sw.newLine();
			        sw.write( cs_space + "                }catch( Exception ex0 ){" );
			        sw.newLine();
			        sw.write( cs_space + "                    serr.println( \"Resources#get_" + name + "; ex0=\" + ex0 + \"; res_path=\" + res_path );" );
			        sw.newLine();
			        sw.write( cs_space + "                }finally{" );
			        sw.newLine();
			        sw.write( cs_space + "                    if( fs != null ){" );
			        sw.newLine();
			        sw.write( cs_space + "                        try{" );
			        sw.newLine();
			        sw.write( cs_space + "                            fs.Close();" );
			        sw.newLine();
			        sw.write( cs_space + "                        }catch( Exception ex2 ){" );
			        sw.newLine();
			        sw.write( cs_space + "                            PortUtil.stderr.println( \"Resources#get_" + name + "; ex2=\" + ex2 + \"; res_path=\" + res_path );" );
			        sw.newLine();
			        sw.write( cs_space + "                        }" );
			        sw.newLine();
			        sw.write( cs_space + "                    }" );
			        sw.newLine();
			        sw.write( cs_space + "                }" );
			        sw.newLine();
			        sw.write( "#endif" );
			        sw.newLine();
			        sw.write( cs_space + "            }catch( Exception ex ){" );
			        sw.newLine();
			        sw.write( cs_space + "                serr.println( \"Resources#get_" + name + "; ex=\" + ex + \"; res_path=\" + res_path );" );
			        sw.newLine();
			        sw.write( cs_space + "            }" );
			        sw.newLine();
			        sw.write( cs_space + "        }" );
			        sw.newLine();
			        sw.write( cs_space + "        return " + instance + ";" );
			        sw.newLine();
			        sw.write( cs_space + "    }" );
			        sw.newLine();
			        sw.newLine();
                }
            }
            sw.write( cs_space + "}" );
            sw.newLine();
            if( !name_space.equals( "" ) ){
                sw.write( "#if !JAVA" );
                sw.newLine();
                sw.write( "}" );
                sw.newLine();
                sw.write( "#endif" );
                sw.newLine();
            }
        }catch( Exception ex ){
        }finally{
        	if( sw != null ){
        		try{
        			sw.close();
        		}catch( Exception ex2 ){
        		}
        	}
        	if( sr != null ){
        		try{
        			sr.close();
        		}catch( Exception ex2 ){
        		}
        	}
        }
    }
}