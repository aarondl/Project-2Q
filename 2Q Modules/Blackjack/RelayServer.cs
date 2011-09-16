using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;

using Project2Q.SDK.ModuleSupport;

namespace Blackjack {

    /*/// <summary>
    /// The protocol commands.
    /// </summary>
    public enum RelayProtocol : byte {
        Register = 0,
        Unregister = 1,
        Send = 2,
        Ping = 3,
        Pong = 4,
    };

    /// <summary>
    /// This class allows for relay of blackjack communication through multiple blackjack bots.
    /// </summary>
    public class RelayServer : IDisposable {

        /*
         * PROTOCOL DESCRIPTION:
         * 
         * Packet:
         * 
         *  BJ:COMMAND:STRLEN:STR
         * 
         * BJ = The letters BJ, 2 bytes
         * : = Spacer, 1 byte
         * COMMAND = 0-5 for each command-type, 1 byte
         * : = Spacer, 1 byte
         * STRLEN = The length of the argument, 4 bytes (integer)
         * : = Spacer, 1 byte
         * STR = The argument itself. x bytes, see above
         *  
         *  Commands:
         *   Registering for channel relay, Argument: botname#/&channel
         *   Unregistering for channel relay, Argument: botname#/&channel
         *   Send this Message (Only issued from Server), Argument: Raw IRC Data to send to the server
         *   Ping (Only issued from Server), Argument: 4 byte integer, must be relayed to stay connected.
         *   Pong (Only issued from Client in response to Ping), Argument: 4 byte integer, must be relayed to stay connected.
         */

        /*private static readonly int PortLow = 5587;
        private static readonly int PortHigh = 5600;
        public static int portRotation;
        public static readonly int MaxClients = 8;
        public static readonly int PacketSize = 4096;

        private IAsyncResult acceptAsync;
        private int port;
        private Dictionary<string, RelayChannel> channels;
        private TcpListener tcp;
        private ModuleProxy.SendDataDelegate sendFunction;
        private byte[] buffer;

        /// <summary>
        /// Just keeps track of which ports are in use.
        /// </summary>
        static RelayServer() {
            portRotation = PortLow;
        }

        /// <summary>
        /// Constructs and initializes a listening socket.
        /// </summary>
        public RelayServer(ModuleProxy.SendDataDelegate sendFunction) {
            this.sendFunction = sendFunction;
            if ( portRotation > PortHigh )
                throw new OverflowException();
            port = portRotation++;
            buffer = new byte[PacketSize];

            channels = new Dictionary<string, RelayChannel>();

            tcp = new TcpListener( IPAddress.Any, port );
            tcp.Start( MaxClients );
            acceptAsync = tcp.BeginAcceptTcpClient( new AsyncCallback( NewConnectionIncoming ), 0 );
        }

        public void NewConnectionIncoming(IAsyncResult iar) {

            TcpClient tcpC = tcp.EndAcceptTcpClient( iar );
            NetworkStream ns = tcpC.GetStream();
            ns.ReadTimeout = 5000; //If this thing does not read in 5 seconds, kill it.
            int nread = 0;
            try {
                nread = ns.Read( buffer, 0, PacketSize );
            }
            catch ( SocketException se ) {
                if ( se.SocketErrorCode != SocketError.TimedOut )
                    throw se;
            }

            //This command should be registration, if not, dump him.
            if ( nread <= 5 || buffer[0] != 'B' || buffer[1] != 'J' || buffer[2] != ':' || buffer[3] != (byte)RelayProtocol.Register || buffer[4] != ':' )
                tcpC.Close();

            uint argSize = BitConverter.ToUInt32( buffer, 5 );
            if ( argSize < 3 )
                tcpC.Close();

            string argument = BitConverter.ToString(buffer, 9, (int)argSize);
            int findTok = argument.IndexOf( '#' );
            if ( findTok < 0 )
                findTok = argument.IndexOf( '&' );
            string botName = argument.Substring( 0, findTok + 1 );
            string chanName = argument.Substring( findTok );

            RelayChannel channel;
            if ( channels.TryGetValue( chanName, out channel ) ) {

            }
            else {

            }

            if ( next >= 0 )
                acceptAsync = tcp.BeginAcceptTcpClient( new AsyncCallback( NewConnectionIncoming ), next );
        }

        public void InformationRead(IAsyncResult iar) {

        }

        /// <summary>
        /// This function delivers the notice message to the target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="target">The user or channel to send to.</param>
        public void Notice(string message, string target) {
            if ( target[0] == '#' || target[0] == '&' ) {

            }
            else {

            }
        }

        /// <summary>
        /// This function delivers the privmsg to the target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="target">The user or channel to send to.</param>
        public void Privmsg(string message, string target) {
            if ( target[0] == '#' || target[0] == '&' ) {

            }
            else {

            }
        }


        #region IDisposable Members

        public void Dispose() {
            this.acceptAsync.AsyncWaitHandle.Close();
            this.tcp.Stop();

            foreach ( RelayChannel bjrc in channels )
                bjrc.Dispose();
        }

        #endregion
    }*/

}
