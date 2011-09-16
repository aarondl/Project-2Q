using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

using Injections = Project2Q.SDK.Injections;

using Project2Q.SDK;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;
using Project2Q.SDK.ModuleSupport;

namespace EightBall {
    public class Main : IModuleCreator{
        
        public override void Initialize() {
            mp.RegisterParse( Configuration.ModuleConfig.ModulePrefix + "8", new CrossAppDomainDelegate( Magic ),
               IRCEvents.ParseTypes.ChannelMessage );
        }

        public IRCEvents.ParseReturns parseReturns;
        
        public void BoldReply( string boldtxt, string txt ) {
            returns = new string[] {
                "PRIVMSG " + parseReturns.Channel.Name + " :" + (char)2 + boldtxt + (char)2 + txt,
            };
        }

        public void Magic() {
            Random r = new Random();
            string text = parseReturns.Text;
            string[] splits = text.Split( ' ' );
            string[] EightBall = { 
                "As I see it, yes", 
                "Ask again later", 
                "Better not tell you now", 
                "Cannot predict now",
                "Concentrate and ask again",
                "Don't count on it",
                "It is certain",
                "It is decidedly so",
                "Most likely",
                "My reply is no",
                "My sources say no",
                "Outlook good",
                "Outlook not so good",
                "Reply hazy, try again",
                "Signs point to yes",
                "Very doubtful",
                "Without a doubt",
                "Yes",
                "Yes - definitely",
                "You may rely on it"
            };

            if ( splits.Length >= 3 ) {
                BoldReply( "8Ball: ", "Not enough arguments." );
            }

            if ( splits[splits.Length - 1].EndsWith( "?" ) ) {
                BoldReply( "8Ball: ", EightBall[r.Next( 20 )] );
            }
            else { BoldReply( "8Ball: ", "Not a Question!" ); }
        }
    }
}
