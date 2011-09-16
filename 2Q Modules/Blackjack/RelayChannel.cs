using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Blackjack {

    /// <summary>
    /// Constitutes all information about the channel being relayed.
    /// </summary>
    /*public class RelayChannel : IDisposable {

        /// <summary>
        /// Constructs a new channel.
        /// </summary>
        /// <param name="channelName">The name of the channel.</param>
        public RelayChannel(string channelName) {
            name = channelName;
            rotationIndex = 0;

            clients = new RelayClient[RelayServer.MaxClients];
        }

        public void Notice(string message) {

        }

        /// <summary>
        /// Gets the index of the next use-able client slot if there is one. Returns -1 if not.
        /// </summary>
        public int GetNext {
            get {
                for ( int i = 0; i < RelayServer.MaxClients; i++ )
                    if ( clients[i] == null )
                        return i;
                return -1;
            }
        }

        public RelayClient[] clients;

        #region IDisposable Members

        public void Dispose() {
            for ( int i = 0; i < RelayServer.MaxClients; i++ ) {
                readResults[i].AsyncWaitHandle.Close();
                relayBots[i].Close();
            }
        }

        #endregion
    }*/

}
