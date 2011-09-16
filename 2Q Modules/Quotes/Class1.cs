using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

using Injections = Project2Q.SDK.Injections;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;
using Project2Q.SDK;
using Project2Q.SDK.ModuleSupport;

namespace QuoteSpace {

    public class Quotes : IModuleCreator {

        public override void  ActivationComplete() {
            mp.RegisterParse( Configuration.ModuleConfig.ModulePrefix + "echo", new CrossAppDomainDelegate( Lololmethod ),
                IRCEvents.ParseTypes.ChannelMessage );
            mp.RegisterParse( Configuration.ModuleConfig.ModulePrefix + "uptime", new CrossAppDomainDelegate( RoflMethod ),
                IRCEvents.ParseTypes.ChannelMessage );
            mp.RegisterWildcardParse( "http://*.*", new CrossAppDomainDelegate( KekeMethod ), 
                IRCEvents.ParseTypes.ChannelMessage );
            dt = DateTime.Now;
        }

        private DateTime dt;

        public Injections.UserMessageEvent userMessageData;
        public Project2Q.SDK.IRCEvents.ParseReturns parseReturns;

        [UserLevelRequired(200)]
        public void Lololmethod() {
            if ( parseReturns.Text.Length > "?echo ".Length )
                parseReturns.Text = parseReturns.Text.Substring( "?echo ".Length );

            returns = new string[] {
                BoldNickReturn( parseReturns.User.Nickname, parseReturns.Channel.Name, parseReturns.Text),
            };
        }

        [UserLevelRequired(300)]
        public void RoflMethod() {

            returns = new string[] {
                BoldNickReturn( parseReturns.User.Nickname, parseReturns.Channel.Name, ((TimeSpan)(DateTime.Now - dt)).ToString())
            };
        }

        public void KekeMethod() {
            string url = parseReturns.Text.Substring( parseReturns.Text.IndexOf( "http://" ) );
            int n = url.IndexOf( ' ' );
            if ( n != -1 )
                url = url.Substring( 0, n );

            HttpWebRequest htp = null;
            try {
                htp = (HttpWebRequest)WebRequest.Create( url );
            }
            catch (UriFormatException) {
                returns = new string[] {
                    BoldNickReturn( parseReturns.User.Nickname, parseReturns.Channel.Name, "Bad URL."),
                };
                return;
            }

            HttpWebResponse htpr = null;
            try {
                htpr = (HttpWebResponse)htp.GetResponse();
            }
            catch {
                returns = new string[] {
                    BoldNickReturn( parseReturns.User.Nickname, parseReturns.Channel.Name, "HTTP Request not completed."),
                };
                return;
            }

            StreamReader res = new StreamReader(htpr.GetResponseStream());

            char[] buf = new char[1024];
            int x = 0;
            string title = null;
            ASCIIEncoding ae = new ASCIIEncoding();
            StringBuilder sb = new StringBuilder();
            while ( ( x = res.ReadBlock( buf, 0, 1024 ) ) > 0 ) {
                string s = sb.Append( buf ).ToString();
                int starttag = s.IndexOf( "<title>" );
                if ( starttag == -1 )
                    continue;
                s = s.Substring( starttag + 7 );
                title = s.Substring( 0, s.IndexOf( "</title>" ) );
                title = title.Trim();//title.Trim( '\r', '\n', '\t' );
            }

            res.Close();

            if ( title != null ) {
                returns = new string[] { BoldNickReturn( parseReturns.User.Nickname, parseReturns.Channel.Name, title) , };
            }
            else returns = null;
            
        }

        public static string BoldNickReturn(string nick, string channel, string text) {
            return "PRIVMSG " + channel + " :\u0002" + nick + "\u0002: " + text;
        }

    }

}
