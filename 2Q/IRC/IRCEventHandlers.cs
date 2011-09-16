using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;

namespace Project2Q.Core {

    public class IRCEventHandlers {

        #region Default Handlers

        /// <summary>
        /// Default Ping Handler function.
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        /// <param name="postBack">The expected reply to the server.</param>
        public static string[] PingHandler(int serverId, string postBack) {
            return new string[] {
                    "PONG :" + postBack
                };
        }

        /// <summary>
        /// Default Connect Handler
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        public static string[] ConnectHandler(int serverId) {
            return IRCCommands.CreateConnectInfo( serverId );
        }

        /// <summary>
        /// Default Err_NickNameInUseHandler. Sends other nicknames to the server
        /// if the first is rejected.
        /// </summary>
        /// <param name="serverId">The server id.</param>
        /// <param name="badnick">The nickname that was rejected.</param>
        /// <returns>The commands to execute.</returns>
        public static string[] Err_NickNameInUseHandler(int serverId, string badnick) {
            Server s = Server.GetServer( serverId );
            string nextnick = null;

            if ( badnick.Equals( s.Nickname ) )
                nextnick = s.AlternateNick;
            else
                nextnick = badnick + "_";

            return new string[] {
                "NICK :" + nextnick,
            };
        }

        /// <summary>
        /// Default BotJoinHandler. When the bot joins a channel run a /WHO
        /// to make sure we cache the addresses of everyone.
        /// </summary>
        /// <param name="serverid">The server id.</param>
        /// <param name="c">The channel joined.</param>
        /// <returns>The commands to execute.</returns>
        public static string[] BotJoinHandler(int serverid, Channel c) {
            return new string[] {
                "NAMES " + c.Name,
                "WHO " + c.Name,
            };
        }

        /// <summary>
        /// Default ServerMode handler. Used to collect bot modes.
        /// </summary>
        /// <param name="serverId">The server id.</param>
        /// <param name="u">The user that was altered (should normally be us).</param>
        /// <param name="recipient">The recipient of the mode.</param>
        /// <param name="modestring">The modes applied.</param>
        /// <returns></returns>
        public static string[] ServerModeMessageHandler(int serverId, User u, string recipient, string modestring) {
            
            Server s = Server.GetServer( serverId );

            if ( s.CurrentIP == null && s.CurrentNickName == recipient ) {
                //We can try to dns the user hostname to retrieve our IP.
                IPAddress[] ips = Dns.GetHostAddresses( u.Hostname );
                //Should only ever be one -_-
                if ( ips.Length > 0 )
                    s.CurrentIP = ips[0];
            }

            return null;

        }

        /// <summary>
        /// Default WhoHandler. Simply makes sure all the users on the channel have
        /// valid masks and are known about.
        /// </summary>
        /// <param name="serverid">The server id.</param>
        /// <param name="c">The channel queried.</param>
        /// <param name="user">The user parsed.</param>
        /// <returns>The commands to execute.</returns>
        /*public static string[] WhoHandler(int serverid, Channel c, IRCHost user) {

            Server s = Server.GetServer( serverid );

            User u = null;

            try {
                u = s.UserDatabase[user.Nick];
            }
            catch ( KeyNotFoundException ) {
                u = new User( user );
                s.UserDatabase.AddUser( u );
                s.UserDatabase.Authenticate( u );
            }

            ChannelUser cu = null;
            try {
                cu = c[user.Nick];
                cu.InternalUser = u;
            }
            catch ( KeyNotFoundException ) {
                //THIS SHOULD NEVER HAPPEN DO NOTHING.
            }

            return null;
        }*/

        /// <summary>
        /// Default AuthModeHandler function.
        /// </summary>
        /// <param name="sid">The serverid of the event.</param>
        /// <param name="nick">The nickname of the user the message was sent to.</param>
        /// <param name="userhost">The userhost of the user the message was sent from.</param>
        /// <param name="text">The text the user wrote.</param>
        public static string[] AuthModeHandler(int sid, string nick, User userhost, string text) {

            Server s = Server.GetServer( sid );

            if ( s.IsInAuthMode && s.CurrentNickName == nick ) {

                Project2Q.SDK.UserSystem.RegisteredUser ru = new Project2Q.SDK.UserSystem.RegisteredUser();
                ru.HostList.Add( new Project2Q.SDK.UserSystem.IRCHost( userhost.CurrentHost.FullHost ) );
                ru.Privegeles.SetPriveleges( Project2Q.SDK.UserSystem.Priveleges.SuperUser );
                ru.Privegeles.NumericalLevel = 1000;
                s.UserDatabase.AddRegisteredUser( ru );
                s.UserDatabase.Authenticate( userhost );
                s.ExitAuthMode();
            }

            return null;
        }

        #endregion

    }

}
