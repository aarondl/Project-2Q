using System;
using System.Collections.Generic;
using System.Text;

namespace BATBot {

    /// <summary>
    /// Exposes to the BATBot and modules commands to aid
    /// in communicating with a server.
    /// </summary>
    public static class IRCCommands {

        /// <summary>
        /// Sends a Text Message to the target.
        /// </summary>
        /// <param name="s">Server object to send to.</param>
        /// <param name="target">Username or Channel</param>
        /// <param name="text">Content</param>
        public static string[] CreateMessage(string target, string text) {
            return new string[] {
                IRCProtocol.CreateMessageString( target, text )
            };
        }

        /// <summary>
        /// Sends an IRC connection message to the server.
        /// </summary>
        /// <param name="s">The server to send this message to.</param>
        public static string[] CreateConnectInfo(int serverId) {
            return new string[] { 
                IRCProtocol.CreateNickString( serverId ),
                IRCProtocol.CreateUserString( serverId )
            };
        }

    }

}