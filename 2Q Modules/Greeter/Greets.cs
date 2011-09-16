using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ModuleSupport;
using Project2Q.SDK.ChannelSystem;
using Project2Q.SDK;
using Injections = Project2Q.SDK.Injections;

namespace Greeter {
    public class Greets : IModuleCreator, IDisposable {

        private Dictionary<string, string> greets;

        public override void Initialize() {

            FileStream f = null;

            try {
                f = new FileStream( @"modules\greets.dat", FileMode.OpenOrCreate, FileAccess.ReadWrite );
                f.Seek( 0, SeekOrigin.Begin );

                if ( f.Length == 0 ) {
                    greets = new Dictionary<string, string>();
                }
                else {
                    BinaryFormatter bf = new BinaryFormatter();
                    greets = (Dictionary<string,string>)bf.Deserialize( f );
                }
            }
            catch {
                return;
            }
            finally {
                if ( f != null )
                    f.Close();
            }
        }

        public override void ActivationComplete() {
            mp.RegisterEvent( "ChannelMessage", new CrossAppDomainDelegate( ChannelMsg ) );
            mp.RegisterEvent( "Join", new CrossAppDomainDelegate( JoinMsg ) );
        }

        public Injections.ChannelMessageEvent channelMessageData;
        public Injections.JoinPartEvent onJoinData;
        public string currentServerName;

        public void ChannelMsg() {

            currentServerName =
                ( (Configuration.ServerConfig)mp.RequestVariable( Request.ServerConfiguration, channelMessageData.sid ) ).Name;

            if ( channelMessageData.text.Length > 7 && channelMessageData.text.StartsWith( Configuration.ModuleConfig.ModulePrefix + "addgt" ) ) {
                string greet = channelMessageData.text.Substring( 7 ); //7 = length(addgt+moduleprefix+space)
                if ( greets.ContainsKey( currentServerName + "." + channelMessageData.channel.Name ) ) {
                    greets[currentServerName + "." + channelMessageData.channel.Name] = greet;
                    UserBoldChannelMessage( "Greet updated." );
                }
                else {
                    greets.Add( currentServerName + "." + channelMessageData.channel.Name, greet );
                    UserBoldChannelMessage( "Greet added." );
                }
            }
            else if ( channelMessageData.text.Equals( Configuration.ModuleConfig.ModulePrefix + "rmgt" ) ) {
                if ( greets.ContainsKey( currentServerName + "." + channelMessageData.channel.Name ) ) {
                    greets.Remove( currentServerName + "." + channelMessageData.channel.Name );
                    UserBoldChannelMessage( "Greet removed." );
                }
                else
                    UserBoldChannelMessage( "No greet found for (" + currentServerName + "." + channelMessageData.channel.Name + ")." );

            }
            else if ( channelMessageData.text.Equals( Configuration.ModuleConfig.ModulePrefix + "showgt" ) ) {
                if ( greets.ContainsKey( currentServerName + "." + channelMessageData.channel.Name ) ) {
                    UserBoldChannelMessage( "Greet for (" + currentServerName + "." + channelMessageData.channel.Name + ") is: \""
                        + greets[currentServerName + "." + channelMessageData.channel.Name] + "\"");
                }
                else
                    UserBoldChannelMessage( "No greet found for (" + currentServerName + "." + channelMessageData.channel.Name + ")." );
            }

        }

        public void JoinMsg() {

            //Replace: [n] = nickname, [h0...] = host0... [p] = privstring [u] = userlevel, [h] = current host, [c] = channel name

            if ( greets.ContainsKey( currentServerName + "." + onJoinData.channel.Name ) ) {

                string toparse = greets[currentServerName + "." + onJoinData.channel.Name];

                toparse = toparse.Replace( "[n]", onJoinData.user.InternalUser.Nickname );
                toparse = toparse.Replace( "[c]", onJoinData.channel.Name );
                toparse = toparse.Replace( "[h]", onJoinData.user.InternalUser.CurrentHost.ToString() );
                if ( onJoinData.user.InternalUser.UserAttributes != null ) {
                    toparse = toparse.Replace( "[p]", onJoinData.user.InternalUser.UserAttributes.Privegeles.PrivelegeString );
                    toparse = toparse.Replace( "[u]", onJoinData.user.InternalUser.UserAttributes.Privegeles.NumericalLevel.ToString() );
                }
                else {
                    toparse = toparse.Replace( "[p]", "none" );
                    toparse = toparse.Replace( "[u]", "0" );
                }

                //Find any hxx and hx args.
                int n = toparse.Length;
                int hostnum = -1;
                bool doubledigit = false;
                //max length = [h99] = 5
                for ( int i = 0; i < n - 5; i++ ) {

                    if ( toparse[i] == '[' && toparse[i + 1] == 'h' &&
                            ( toparse[i + 3] == ']' ||
                                ( i + 4 < n && toparse[i + 4] == ']' )
                            )
                        ) {
                        if ( i + 4 < n && toparse[i + 4] == ']' ) {
                            //Two numbers
                            StringBuilder sb = new StringBuilder();
                            try {
                                sb.AppendFormat( "{0}{1}", toparse[i + 2], toparse[i + 3] );
                                hostnum = int.Parse( sb.ToString() );
                                doubledigit = true;
                            }
                            catch ( FormatException ) {
                                hostnum = -1;
                                continue;
                            }
                        }
                        else {
                            //one number
                            try {
                                hostnum = int.Parse( toparse[i + 2].ToString() );
                            }
                            catch ( FormatException ) {
                                hostnum = -1;
                                continue;
                            }
                        }

                        if ( hostnum >= 0 ) {
                            string rep = null;
                            if ( doubledigit )
                                rep = "[h" + hostnum.ToString( "d2" ) + "]";
                            else
                                rep = "[h" + hostnum.ToString() + "]";

                            if ( onJoinData.user.InternalUser.UserAttributes == null ||
                                hostnum > onJoinData.user.InternalUser.UserAttributes.HostList.Count ) {
                                toparse = toparse.Replace( rep, "" );
                                n = n - rep.Length;
                            }
                            else {
                                toparse = toparse.Replace( rep, onJoinData.user.InternalUser.UserAttributes.HostList[hostnum].ToString() );
                                n = n - rep.Length + onJoinData.user.InternalUser.UserAttributes.HostList[hostnum].ToString().Length;
                            }
                        }
                    }

                }

                returns = new string[] { "NOTICE " + onJoinData.user.InternalUser.Nickname + " :" + toparse, };

            }

            
        }

        public void UserBoldChannelMessage(string text) {
            returns = new string[] {
                "NOTICE " + channelMessageData.channelUser.InternalUser.Nickname + " :" + (char)2 + channelMessageData.channelUser.InternalUser.Nickname + (char)2 + ": " + text,
            };
        }


        #region IDisposable Members

        public void Dispose() {
            FileStream f = null;

            try {
                f = new FileStream( @"modules\greets.dat", FileMode.OpenOrCreate, FileAccess.ReadWrite );
                f.Seek( 0, SeekOrigin.Begin );

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize( f, greets );
            }
            catch {
                return;
            }
            finally {
                if ( f != null )
                    f.Close();
            }
        }

        #endregion
    }
}
