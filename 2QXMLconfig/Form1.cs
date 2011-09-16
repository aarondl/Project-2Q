using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace _QXMLconfig
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Module Stuctured Settlement.
        /// </summary>
        struct Module
        {
            public string PrettyName; //The pretty name of the Module
            public string NameSpace;  //The Namespace of the module, followed by it's class
            public string FileNames;  //The Files the modules uses.
        }

        Module[] Modules = new Module[32];

        public int moduleselected;
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load( object sender, EventArgs e ) {
            bot_cbo_autojoin.SelectedIndex = 0;

        }

        private void menu_open_Click( object sender, EventArgs e ) {
            Stream config;
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Config.xml|Config.xml";
            openFileDialog1.ShowDialog();
            try {
                if ( ( config = openFileDialog1.OpenFile() ) != null ) {
                    using ( config ) {
                        MessageBox.Show( "worked" );
                    }
                }
            }
            catch ( Exception ) {
                //MessageBox.Show( "Error: Could not read file from disk. Original error: " + ex.Message );
            }
        }

        public void ParseOpen( Stream s ) {

        }

        private void menu_new_Click( object sender, EventArgs e ) {
            //Bot Group
            bot_txt_nick.Text = "P2Q";
            bot_txt_altnick.Text = "Project2Q";
            bot_txt_info.Text = "2Q Beta";
            bot_txt_username.Text = "2Q";
            bot_txt_quit.Text = "HI ILU GUYZ BAI!";
            bot_cbo_autojoin.SelectedIndex = 1;

            //Connection Group
            con_txt_irc.Text = "irc.gamesurge.net";
            con_txt_port.Text = "6667";
            con_txt_perform.Text = "join #project2q alfred\r\njoin #phishcave\r\njoin #internetz";

            //Module Group
            Modules[0].PrettyName = "SteamID Lookup";
            Modules[0].NameSpace = "SteamID.Main";
            Modules[0].FileNames = @"SteamID\bin\Debug\SteamID.dll";

            Modules[1].PrettyName = "Webserver Module";
            Modules[1].NameSpace = "Webserver.Server";
            Modules[1].FileNames = @"Webserver\bin\Debug\Webserver.dll";

            Modules[2].PrettyName = "Simple Channel Administration";
            Modules[2].NameSpace = "Simple.Main";
            Modules[2].FileNames = @"Simple\bin\Debug\Simple.dll";

            mod_lbo_current.Items.AddRange( 
                new object[] {
                    Modules[0].PrettyName,
                    Modules[1].PrettyName,
                    Modules[2].PrettyName
                }
            );

            label15.Text = "LENGTH: " + Modules.Length;
        }

        private void mod_lbo_current_SelectedIndexChanged( object sender, EventArgs e ) {
            moduleselected = mod_lbo_current.SelectedIndex;
            mod_txt_pretty.Text = Modules[moduleselected].PrettyName;
            mod_txt_namespace.Text = Modules[moduleselected].NameSpace;
            mod_txt_files.Text = Modules[moduleselected].FileNames;
            mod_btn_edit.Enabled = true;
        }

        private void mod_btn_edit_Click( object sender, EventArgs e ) {
            Modules[moduleselected].PrettyName = mod_txt_pretty.Text;
            Modules[moduleselected].NameSpace = mod_txt_namespace.Text;
            Modules[moduleselected].FileNames = mod_txt_files.Text;
            
        }

        private void mod_btn_add_Click( object sender, EventArgs e ) {
            moduleselected = mod_lbo_current.Items.Count;
            Modules[moduleselected].PrettyName = mod_txt_pretty.Text;
            Modules[moduleselected].NameSpace = mod_txt_namespace.Text;
            Modules[moduleselected].FileNames = mod_txt_files.Text;
            mod_lbo_current.Items.Add( Modules[moduleselected].PrettyName );
            /*string[][] tmpmod = new string[][] { };
            for ( int i = 0; i < modules.Length / 3; i++ ) {
                tmpmod = new string[][i] { modules[i][0], modules[i][1], modules[i][2] };
            }*/
        }

        public void UpdateModulesList() {
            //int l = modules.Length;
        }

        public void MakeConfig() {
            string perform;
            string auto;
            if ( bot_cbo_autojoin.SelectedIndex == 0 ) {
                auto = "true";
            }
            else {
                auto = "false";
            }

            perform = con_txt_perform.Text.Replace( '\n', ';' ).Replace( ";;", ";" );
            
            XmlTextWriter cxml = new XmlTextWriter( @"C:\Config.xml", null );

            cxml.Formatting = Formatting.Indented;
            cxml.WriteComment( "Project2Q Config File" );
            cxml.WriteComment( "Created on: " + DateTime.Today.ToShortDateString() );

            cxml.WriteStartElement( "Configuration" );
            //Settings Block
            cxml.WriteComment( "All your bot's settings :D" );
            cxml.WriteStartElement( "Settings" );
            cxml.WriteElementString( "nickname", bot_txt_nick.Text );
            cxml.WriteElementString( "alternate", bot_txt_altnick.Text );
            cxml.WriteElementString( "username", bot_txt_username.Text );
            cxml.WriteElementString( "info", bot_txt_info.Text );
            cxml.WriteElementString( "port", con_txt_port.Text );
            cxml.WriteElementString( "quitMessage", bot_txt_quit.Text );
            cxml.WriteElementString( "autoJoinOnInvite", auto );
            cxml.WriteElementString( "operationTimeout", "5000" ); //these
            cxml.WriteElementString( "retryTimeout", "30000" );    //i'm
            cxml.WriteElementString( "socketBufferSize", "4096" ); //not
            cxml.WriteElementString( "sendInhibit", "1000" );      //sure about
            cxml.WriteEndElement();

            //Server Block
            cxml.WriteComment( "Server Information :D" );
            cxml.WriteStartElement( "Server" );
            cxml.WriteAttributeString( "name", con_txt_servername.Text );
            cxml.WriteElementString( "dns", con_txt_irc.Text );
            cxml.WriteElementString( "port", con_txt_port.Text );
            cxml.WriteElementString( "autoJoinOnInvite", auto );
            cxml.WriteElementString( "sendInhibit", "4096" );
            cxml.WriteStartElement( "perform" );
            cxml.WriteString( perform );
            cxml.WriteEndElement();
            cxml.WriteEndElement();

            //Module Block
            cxml.WriteComment( "Module Stuff :D" );
            cxml.WriteStartElement( "Modules" );
            cxml.WriteAttributeString( "prefix", "?" );
            cxml.WriteAttributeString( "modulePath", @"..\..\..\..\..\2Q Modules" );

            for ( int i = 0; i < 32; i++ ) {
                //Just a test ^_^
                if ( ( Modules[i].NameSpace == null ) || ( Modules[i].PrettyName == null ) || ( Modules[i].FileNames == null ) )
                    break;
                cxml.WriteStartElement( "Module" );
                cxml.WriteAttributeString( "name", Modules[i].NameSpace );
                cxml.WriteElementString( "prettyname", Modules[i].PrettyName );
                cxml.WriteElementString( "filenames", Modules[i].FileNames );
                cxml.WriteEndElement();
            }

            cxml.WriteEndElement();

            cxml.WriteEndElement();
            cxml.Close();
        }

        private void button1_Click( object sender, EventArgs e ) {
            MakeConfig();
        }
            
    }
}