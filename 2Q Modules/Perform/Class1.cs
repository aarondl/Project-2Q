using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

using Project2Q.SDK;
using Project2Q.SDK.Injections;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;
using Project2Q.SDK.ModuleSupport;

namespace Perform { //Your default namepsace is perform not Perform.
    public class Main : IModuleCreator {

        public PingEvent        pingData;
        public ConnectEvent     connectData;
        private readonly string performfile = @"modules/perform.cfg";
        private bool[]          done;

        Dictionary<string, List<string>> performs;

        private int count( string text, char look ) {
            int count = 0;
            for ( int i = 0; i < text.Length; i++ ) {
                if ( text[i] == look )
                    count++;
            }
            return count;
        }

        public string After( string text, string after ) {
            return text.Remove( 0, text.IndexOf( after ) + after.Length + 1 );
        }

        public override void Initialize() {
            
            done = new bool[IModule.MaxServers];
            performs = new Dictionary<string, List<string>>();

            if ( !File.Exists( performfile ) )
                return;

            using ( StreamReader SR = new StreamReader( performfile ) ) {
                string line = "Hello!";
                string section = null;
                List<string> commands = new List<string>();

                while ( !SR.EndOfStream ) {

                    line = SR.ReadLine();

                    if ( line.Contains( "//" ) ) line = line.Remove( line.IndexOf( "//" ) ); line = line.Trim();
                    if ( line == "" ) continue;

                    if ( line.Contains( "{" ) && count( line, '{' ) == 1 ) {
                        line = line.Remove( line.IndexOf( "{" ) ).ToLower().Trim();
                        if ( performs.ContainsKey( line ) ) 
                            section = null;
                        else
                            section = line;
                        continue;
                    }

                    if ( line.Contains( "}" ) ) {
                        if ( section != null ) 
                            performs.Add( section, commands );
                        section = null;
                        commands = new List<string>();
                        continue;
                    }

                    if ( section == null )
                        continue;

                    string[] args = line.Split( ' ' );

                    string command; // see?
                    //string 
                    // commands
                    switch ( args[0] ) {
                        case "m"    : goto case "msg";
                        case "j"    : goto case "join";

                        case "msg"  : command = "PRIVMSG " + args[1] + " :" + After( line, args[1] );     break;
                        case "join": command = "JOIN " + args[1]  + ( ( args.Length == 3 ) ? " " + args[2] : "" ); break;
                        default     : command = line;                                                     break;
                    }

                    //variables
                    if ( section != null ) {
                        commands.Add( command );
                    }
                }
            }
            mp.RegisterEvent( "Ping", new CrossAppDomainDelegate( Ping ) );
            mp.RegisterEvent( "Connect", new CrossAppDomainDelegate( Connect ) );
        }

        public void Connect() {
            done[connectData.serverId] = false;
        }

        public void Ping() {
            int sid = pingData.serverId;

            if ( !done[sid] ) {
                done[sid] = true;
                Configuration.ServerConfig w = (Configuration.ServerConfig)mp.RequestVariable( Request.ServerConfiguration, sid );
                string name = w.Name.ToLower();

                if ( performs.ContainsKey( name ) ) {
                    returns = new string[performs[name].Count];
                    for ( int i = 0; i < performs[name].Count; i++ ) {
                        if ( performs[name][i].Contains( "*" ) ) {
                            string variable = performs[name][i].Remove( 0, performs[name][i].IndexOf( "*" ) + 1 ); variable = variable.Remove( variable.IndexOf( ' ' ) );
                            if ( variable == "" )
                                continue;

                            switch ( variable ) {
                                case "me": performs[name][i] = performs[name][i].Replace( "*me", w.NickName ); break;
                                // add more!
                            }
                        }
                        returns[i] = performs[name][i];
                        //Console.WriteLine( returns[i] );
                    }
                }
            }
        }
    }
}

