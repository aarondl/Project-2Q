using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Blackjack {

    /*/// <summary>
    /// Encompasses the data required for a relay client.
    /// </summary>
    public class RelayClient : IDisposable {

        public RelayClient(string botName, TcpClient connection) {
            this.botName = botName;
            this.connection = connection;
            buffer = new byte[RelayServer.PacketSize];
        }

        public IAsyncResult readResult;
        public string botName; //This way we can monitor him for explosions.
        public TcpClient connection;
        public byte[] buffer;


        #region IDisposable Members

        public void Dispose() {
            if ( readResult != null )
                readResult.AsyncWaitHandle.Close();
            connection.Close();
        }

        #endregion
    }*/

}
