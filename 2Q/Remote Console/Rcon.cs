using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Authentication;
using System.Threading;
using System.IO;

using Project2Q.SDK;

namespace Project2Q.Core {

    /// <summary>
    /// A remote console for controlling the current assembly.
    /// Remote Console uses SSL and HMAC-MD5 for security.
    /// </summary>
    internal sealed class Rcon : IDisposable {

        #region Variables + Properties
        private bool shutdown;
        private bool acceptBlocking;
        private Thread acceptThread;
        private Socket listenSocket;
        private Project2QService.WriteLogFunction writeLogFunction = null;
        private Configuration.RconConfig rconfig;
        private RconUserFile rconUsers;
        private SocketPipe[] rconConnections;
        /// <summary>
        /// The maximum number of Remote Console Connections
        /// </summary>
        private static readonly int MaxRconConnections = 32;

        /// <summary>
        /// Returns the next useable Rcon ID. -1 if none was found.
        /// </summary>
        private int NextRconID {
            get {
                for ( int i = 0; i < MaxRconConnections; i++ )
                    if ( rconConnections[i] == null )
                        return i;
                return -1;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an Rcon Server with the ability to log.
        /// </summary>
        /// <param name="rconfig">An RConsole Configuration to build off.</param>
        /// <param name="writeLog">The function to call with logging information.</param>
        public Rcon(Configuration.RconConfig rconfig, Project2QService.WriteLogFunction writeLog) 
            : this(rconfig) {
            writeLogFunction = writeLog;
        }

        /// <summary>
        /// Creates an Rcon Server
        /// </summary>
        /// <param name="rconfig">RConsole Configuration to build off.</param>
        public Rcon(Configuration.RconConfig rconfig) {

            rconConnections = new SocketPipe[Rcon.MaxRconConnections];
            for ( int i = 0; i < Rcon.MaxRconConnections; i++ )
                rconConnections[i] = null;

            this.rconfig = rconfig;
            if ( File.Exists( rconfig.UserFile ) )
                rconUsers = new RconUserFile( rconfig.UserFile );
            else
                rconUsers = null; //For now.
            listenSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            listenSocket.SendBufferSize = rconfig.SocketBufferSize;
            listenSocket.ReceiveBufferSize = rconfig.SocketBufferSize;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins listening on the socket.
        /// </summary>
        /// <returns>Success?</returns>
        public bool BeginListen() {

            this.shutdown = false;
            this.acceptBlocking = false;

            try {
                if ( !listenSocket.IsBound )
                    listenSocket.Bind( new IPEndPoint( IPAddress.Any, rconfig.ListenPort ) );
                WriteLog( "Listening on port: " + rconfig.ListenPort );
                listenSocket.Listen( 0 );
            }
            catch ( SocketException ) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// An accepting function that never returns until
        /// a Halt or socket error occurs.
        /// </summary>
        public void ConstantAccept() {

            Socket s = null;
            acceptThread = Thread.CurrentThread;

            while ( !shutdown ) {
                try {
                    this.acceptBlocking = true;
                    WriteLog( "Accepting any connections on the listenSocket." );
                    s = listenSocket.Accept();
                    this.acceptBlocking = false;
                }
                catch ( SocketException se ) {
                    if ( se.SocketErrorCode == SocketError.Interrupted )
                        shutdown = true;
                }

                if ( s != null ) {
                    Thread t = new Thread( new ParameterizedThreadStart( Authenticate ) );
                    t.Start( s );
                }
            }

        }

        /// <summary>
        /// Authenticates a socket before accepting into
        /// a permanent connection (it's connection attempt
        /// may yet be rejected.)
        /// </summary>
        /// <param name="socketObj">The object to cast into a socket to query.</param>
        public void Authenticate(Object socketObj) {
            Socket s = (Socket)socketObj;

            int rconConId = this.NextRconID; //Don't bother sending CreateRootUser this ID. The socket
            if ( rconConId == -1 ) return; //doesn't exist for long enough for us to care. But it's important that it's there.

            if ( rconUsers == null ) {
                CreateRootUser( s );
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "<{0}.{1}@{2}>",
                System.Diagnostics.Process.GetCurrentProcess().Id,
                DateTime.Now.ToString( "ddmmyyhhmmss" ),
                System.Environment.UserDomainName );

            string authmsg = "authenticate " + sb.ToString();
            int offset = 0;
            SocketError errorCode;
            do {
                try {
                    offset += s.Send( IRCProtocol.Ascii.GetBytes( authmsg ), 
                        offset, authmsg.Length - offset, SocketFlags.None, out errorCode );
                }
                catch ( SocketException ) {
                    s.Close();
                    return; //Give up. They can reconnect.
                }
            } while ( offset < authmsg.Length );

            if ( errorCode != SocketError.Success ) { s.Close(); return; }

            //Get Auth response.
            byte[] recvBuf = new byte[rconfig.SocketBufferSize];
            offset = s.Receive( recvBuf, 0, rconfig.SocketBufferSize, SocketFlags.None, out errorCode );
            if ( offset == 0 || errorCode != SocketError.Success ) { s.Close(); return; }
            string authReply = IRCProtocol.Ascii.GetString( recvBuf, 0, offset );

            //Parse into name/hash and verify both for correctness.
            string[] unameAndHash = authReply.Split( ' ' );
            if ( unameAndHash.Length != 2 ) { s.Close(); return; }
            
            //Verify username.
            string username = unameAndHash[0];
            Nullable<RconUserFile.RconUser> rc = rconUsers.GetUser(username);
            if ( rc == null ) { s.Close(); return; }

            //Verify their hash.
            string theirHash = unameAndHash[1];
            string pass = rc.Value.Password;

            HMACMD5 hmac = new HMACMD5( IRCProtocol.Ascii.GetBytes( pass ) );
            hmac.Initialize();
            hmac.TransformFinalBlock( IRCProtocol.Ascii.GetBytes( sb.ToString() ), 0, theirHash.Length );
            string ourHash = RconUserFile.GetHashFromDigest( hmac.Hash );

            if ( !ourHash.Equals( theirHash ) ) { s.Close(); return; }

            authmsg = "success";
            offset = 0;
            do {
                try {
                    offset += s.Send( IRCProtocol.Ascii.GetBytes( authmsg ),
                        offset, authmsg.Length - offset, SocketFlags.None, out errorCode );
                }
                catch ( SocketException ) {
                    s.Close();
                    return; //Give up. They can reconnect.
                }
            } while ( offset < authmsg.Length );

            if ( errorCode != SocketError.Success ) { s.Close(); return; }

            //DO IT MAD AMOUNTS OF ARRAY LOOKUPS FOR NO REASON. L2TEMP VARIABLE PLZ
            rconConnections[rconConId] = new SocketPipe( s, rconConId, 30000, 5000, 0 );
            rconConnections[rconConId].OnReceive += new SocketPipe.ReceiveData( OnReceive );
            rconConnections[rconConId].OnDisconnect += new SocketPipe.NoParams( OnDisconnect );

            Thread t = new Thread( new ThreadStart( rconConnections[rconConId].ConstantPump ) );
            t.Start(); //Start sending anything we can :D
            rconConnections[rconConId].ConstantSiphon(); //Consume current thread generated by ConstantAccept.
            
        }

        /// <summary>
        /// Prompts a connected unauthenticated user to provide a root username
        /// and password, then very rudely disconnects him.
        /// </summary>
        /// <param name="s">The socket to use to do this.</param>
        public void CreateRootUser(Socket s) {

            string authmsg = "createroot";
            int offset = 0;
            SocketError errorCode;
            do {
                try {
                    offset += s.Send( IRCProtocol.Ascii.GetBytes( authmsg ),
                        offset, authmsg.Length - offset, SocketFlags.None, out errorCode );
                }
                catch ( SocketException ) {
                    s.Close();
                    return; //Give up. They can reconnect.
                }
            } while ( offset < authmsg.Length );

            if ( errorCode != SocketError.Success ) { s.Close(); return; }

            //Get Auth response. (firstauth - see config) (root username) (32 byte passwordhash)
            byte[] recvBuf = new byte[rconfig.SocketBufferSize];
            offset = s.Receive( recvBuf, 0, rconfig.SocketBufferSize, SocketFlags.None, out errorCode );
            if ( offset == 0 || errorCode != SocketError.Success ) { s.Close(); return; }

            string[] authReply = IRCProtocol.Ascii.GetString( recvBuf, 0, offset ).Split( ' ' );
            if ( authReply.Length != 3 ) { s.Close(); return; }

            if ( !authReply[0].Equals( rconfig.FirstAuth ) || authReply[2].Length != 32 ) { s.Close(); return; }

            RconUserFile.RconUser rc = new RconUserFile.RconUser( authReply[1], authReply[2], 11 );
            rconUsers = new RconUserFile( rconfig.UserFile, rc );

            authmsg = "success";
            offset = 0;
            do {
                try {
                    offset += s.Send( IRCProtocol.Ascii.GetBytes( authmsg ),
                        offset, authmsg.Length - offset, SocketFlags.None, out errorCode );
                }
                catch ( SocketException ) {
                    s.Close();
                    return; //Give up. They can reconnect.
                }
            } while ( offset < authmsg.Length );

            s.Close();
        }

        /// <summary>
        /// Receives disconnect events from any socket.
        /// </summary>
        /// <param name="socketId">Socket ID.</param>
        void OnDisconnect(int socketId) {
            rconConnections[socketId].Close(); //Let us recycle. Also note we don't give a crap
            rconConnections[socketId] = null;  //if they've disconnected or not. Jerks.
        }

        /// <summary>
        /// Receives data from any socket.
        /// </summary>
        /// <param name="socketId">Socket ID.</param>
        /// <param name="data">The data that was read.</param>
        void OnReceive(int socketId, string data) {
            //Do stuff
        }

        /// <summary>
        /// Writes to the application log and the application log file.
        /// Silently fails if this RCon object was created with no log function.
        /// </summary>
        /// <param name="text">The text to log.</param>
        public void WriteLog(string text) {
            if ( writeLogFunction == null )
                return;
            writeLogFunction.Invoke( text );
        }

        /// <summary>
        /// Halts the listening socket and all connections
        /// opened at the time.
        /// </summary>
        /// <param name="permanent">Is this shutdown permanent?</param>
        public void Halt(bool permanent) {
            shutdown = true;

            if ( permanent )
                listenSocket.Close();
            else
                listenSocket.Disconnect( !permanent );

            for ( int i = 0; i < MaxRconConnections; i++ )
                if ( rconConnections[i] != null ) {
                    rconConnections[i].Close();
                    rconConnections[i] = null;
                }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Makes sure all sockets in use get closed.
        /// </summary>
        public void Dispose() {
            if ( acceptBlocking )
                acceptThread.Interrupt();
            listenSocket.Close();
            for ( int i = 0; i < MaxRconConnections; i++ )
                if ( rconConnections[i] != null )
                    rconConnections[i].Close();
        }

        #endregion
    }

}
