using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Project2Q.Core {

    /// <summary>
    /// A class wrapping a socket.
    /// </summary>
    internal class SocketPipe : IDisposable {

        #region Variables + Properties

        Socket socket;
        private bool receiveBlocking;
        private bool sendSleeping;
        private bool shutdown;
        private int sockBufferSize;
        private int retryTimeout;
        private int operationTimeout;
        private int sendInhibit;
        private Thread recvThread;
        private Thread sendThread;
        private SocketError lastSocketError;
        private int sockId;

        private Queue<byte[]> sendBuffer;

        private string hostConnectedWith;
        private int portConnectedWith;

        /// <summary>
        /// Returns the host the socket was connected to with.
        /// </summary>
        public string HostConnectedWith {
            get { return hostConnectedWith; }
        }
        
        /// <summary>
        /// Returns the port the socket was connected to with.
        /// </summary>
        public int PortConnectedWith {
            get { return portConnectedWith; }
        }

        /// <summary>
        /// Returns the last socket error this socket pipe encountered.
        /// </summary>
        public SocketError LastSocketError {
            get { return lastSocketError; }
        }

        /// <summary>
        /// Returns the Socket Pipe ID number associated with this context.
        /// </summary>
        public int SocketID {
            get { return sockId; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a socket pipe from an existing connection. Uses
        /// default values.
        /// </summary>
        /// <param name="s">The socket to adopt.</param>
        /// <param name="socketId">The ID of the connection.</param>
        public SocketPipe(Socket s, int socketId)
            : this( s, socketId, 30000, 5000, 1000 ) {
        }

        /// <summary>
        /// Creates a socket pipe from an existing connection. Uses
        /// no default values.
        /// </summary>
        /// <param name="s">The socket to adopt.</param>
        /// <param name="socketId">The ID of the connection.</param>
        /// <param name="retryTimeout">Time between reconnect attempts.</param>
        /// <param name="operationTimeout">Time that it takes for a socket operation to timeout.</param>
        /// <param name="sendInhibit">Time between send operations.</param>
        public SocketPipe(Socket s, int socketId, int retryTimeout, int operationTimeout, int sendInhibit) {
            socket = s;
            this.sockId = socketId;
            sendBuffer = new Queue<byte[]>( 10 );
            s.ReceiveTimeout = operationTimeout;
            s.SendTimeout = operationTimeout;
            this.shutdown = false;
        }

        /// <summary>
        /// Creates a socket pipe with full default values.
        /// </summary>
        /// <param name="socketId">The ID of the connection.</param>
        public SocketPipe(int socketId)
            : this( socketId, 30000, 10000, 1000, 4096 ) {
        }

        /// <summary>
        /// Creates a socket pipe with a default value for the socket buffer size.
        /// </summary>
        /// <param name="socketId">The ID of the connection.</param>
        /// <param name="retryTimeout">The time between connection attempts.</param>
        /// <param name="operationTimeout">The time for a socket operation to timeout.</param>
        /// <param name="sendInhibit">The time between writes to the socket (0 for instant send).</param>
        public SocketPipe(int socketId, int retryTimeout, int operationTimeout, int sendInhibit)
            : this( socketId, retryTimeout, operationTimeout, sendInhibit, 4096 ) {
        }

        /// <summary>
        /// Creates a socket pipe with no default values.
        /// </summary>
        /// <param name="socketId">The ID of the connection.</param>
        /// <param name="retryTimeout">The time between connection attempts.</param>
        /// <param name="operationTimeout">The timeout of send/recv operations.</param>
        /// <param name="sendInhibit">The time between send calls.</param>
        /// <param name="sockBufferSize">The size of the packets.</param>
        public SocketPipe(int socketId, int retryTimeout, int operationTimeout, int sendInhibit, int sockBufferSize) {
            this.retryTimeout = retryTimeout;
            this.operationTimeout = operationTimeout;
            this.sendInhibit = sendInhibit;
            this.sockId = socketId;
            this.sockBufferSize = sockBufferSize;

            shutdown = false;
            receiveBlocking = false;
            sendSleeping = false;

            socket = null; //Let Connect deal with creating a socket.

            sendBuffer = new Queue<byte[]>( 10 );
        }

        #endregion

        #region Events

        public delegate void NoParams(int socketId);
        public delegate void ReceiveData(int socketId, string data);
        public event NoParams OnDisconnect;
        public event ReceiveData OnReceive;

        #endregion

        #region Methods

        /// <summary>
        /// Allows methods in this class to execute.
        /// </summary>
        public void Initialize() {
            if ( socket != null )
                socket.Close();
            socket = null;
            this.shutdown = false;
        }

        /// <summary>
        /// Creates a socket to work with.
        /// </summary>
        /// <returns>Success?</returns>
        private bool CreateSocket() {
            try {
                socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            }
            catch ( SocketException ) {
                return false;
            }
            socket.Blocking = true;
            socket.ReceiveTimeout = operationTimeout;
            socket.SendTimeout = operationTimeout;
            socket.ReceiveBufferSize = sockBufferSize;
            socket.SendBufferSize = sockBufferSize;
            return true;
        }

        /// <summary>
        /// Connects the socket.
        /// </summary>
        /// <param name="host">The hostname to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <returns>Success?</returns>
        public bool Connect(string host, ushort port) {
            if ( shutdown ) return false;
            try {
                return Connect( Dns.GetHostAddresses( host ), port );
            }
            catch ( SocketException se ) {
                lastSocketError = se.SocketErrorCode;
                return false;
            }
        }

        /// <summary>
        /// Connects the socket.
        /// </summary>
        /// <param name="hosts">A list of hostnames.</param>
        /// <param name="port">The port to connect to.</param>
        /// <returns>Success?</returns>
        public bool Connect(string [] hosts, ushort port) {
            if ( shutdown ) return false;
            foreach ( string h in hosts ) {
                if ( h == null )
                    continue;
                hostConnectedWith = h;
                try {
                    if ( Connect( Dns.GetHostAddresses( h ), port ) )
                        return true;
                }
                catch ( SocketException se ) {
                    lastSocketError = se.SocketErrorCode;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Connects the socket. The last level of overloads, unless last level is explicitly
        /// called, all Connects will pass through this function.
        /// </summary>
        /// <param name="hosts">The list of hosts to try to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <returns>Success?</returns>
        public bool Connect(IPAddress[] hosts, ushort port) {
            if ( shutdown ) return false;
            foreach ( IPAddress ip in hosts ) {
                if ( hostConnectedWith == null ) hostConnectedWith = ip.ToString();
                if ( Connect( ip, port ) )
                    return true;
                Thread.Sleep( retryTimeout );
            }
            return false;
        }

        /// <summary>
        /// Connects the socket. All other Connects bubble down to this one.
        /// </summary>
        /// <param name="host">IPAddress designating whom to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <returns>Success?</returns>
        public bool Connect(IPAddress host, ushort port) {

            if ( shutdown ) return false;

            try {
                portConnectedWith = (int)port;
                if ( socket == null ) {
                    shutdown = false;
                    CreateSocket();
                }
                socket.Connect( host, (int)port );
                IPHostEntry iphe = Dns.GetHostEntry( host );
                if ( iphe.HostName != null )
                    hostConnectedWith = iphe.HostName;
                return true;
            }
            catch ( SocketException se ) {
                lastSocketError = se.SocketErrorCode;
                socket.Close();
                socket = null;
                return false;
            }

        }

        /// <summary>
        /// Pumps data into the Socket Pipe.
        /// </summary>
        /// <param name="data">The data to send.</param>
        public void Pump(string data) {
            Pump( IRCProtocol.Ascii.GetBytes(data + IRCProtocol.CRLF) );
        }

        /// <summary>
        /// Pumps data into the Socket Pipe.
        /// </summary>
        /// <param name="data">The data to send.</param>
        private void Pump(byte[] data) {

            lock ( sendBuffer )
                sendBuffer.Enqueue( data );

            if ( this.sendSleeping )
                sendThread.Interrupt();
        }

        /// <summary>
        /// Constantly pumps the data queued by Pump()'s into
        /// the Socket Pipe.
        /// </summary>
        public void ConstantPump() {

            sendThread = Thread.CurrentThread;
            byte[] buffer;
            int bufferSize;
            int offset;
            bool moreToProcess;

            while ( !shutdown ) {

                buffer = null;
                lock ( sendBuffer )
                    if ( sendBuffer.Count > 0 )
                        buffer = sendBuffer.Dequeue();

                if ( buffer != null ) {

#if DEBUG
                    Server.GetServer( this.sockId ).WriteLog( this.sockId.ToString() + " <- " + IRCProtocol.Ascii.GetString( buffer ) );
#endif

                    bufferSize = buffer.Length;
                    offset = 0;
                    while ( offset < bufferSize ) {
                        SocketError errorCode = SocketError.Success;
                        try {
                            offset += socket.Send( buffer, offset, bufferSize - offset, SocketFlags.None, out errorCode );
                            if ( errorCode != SocketError.Success ) {
                                lastSocketError = errorCode;
                                Close();
                                return;
                            }
                        }
                        catch ( SocketException se ) {
                            if ( se.SocketErrorCode != SocketError.Shutdown &&
                                se.SocketErrorCode != SocketError.Disconnecting )
                                Close();
                            lastSocketError = se.SocketErrorCode;
                            return;
                        }
                        catch ( ObjectDisposedException ) {
                            //Shutdown should be true. Let it die.
                        }
                        catch ( NullReferenceException ) {
                            //Will usually only be null in shutdown case. Let it die.
                        }
                        lastSocketError = errorCode;
                    }

                    lock ( sendBuffer )
                        moreToProcess = sendBuffer.Count > 0;
                }
                else moreToProcess = false;

                if ( !moreToProcess && !shutdown ) {
                    try {
                        //Delay the thread from eating process time.
                        this.sendSleeping = true;
                        Thread.Sleep( this.operationTimeout );
                        this.sendSleeping = false;
                    }
                    catch ( ThreadInterruptedException ) {
                        //shutdown should = true, we will fall out.
                    }
                }
                else if ( !shutdown )
                    try {
                        if ( sendInhibit == 0 ) this.sendSleeping = true;
                        Thread.Sleep( this.sendInhibit );
                        if ( sendInhibit == 0 ) this.sendSleeping = false;
                    }
                    catch ( ThreadInterruptedException ) {
                        if ( sendInhibit == 0 )
                            this.sendSleeping = false;
                        //When we disconnect this will get a Thread Interruption for no reason.
                        //Probably because the exception is still on the stack when it hits the sleep.
                    }

            }

        }

        /// <summary>
        /// Siphons data from the Socket Pipe.
        /// </summary>
        public void ConstantSiphon() {
            recvThread = Thread.CurrentThread;
            byte[] receiveBuffer = new byte[socket.ReceiveBufferSize];
            byte[] overflowBuffer = new byte[512]; //Max IRC message size. Shouldn't ever need more.
            bool overflow = false;
            int overflowLen = 0;
            byte[] eventMsg;
            int bytes = 0;

            while ( !shutdown ) {

                SocketError errorCode = SocketError.Success;

                try {
                    this.receiveBlocking = true;
                    socket.Blocking = true; //Hack because this resets itself somehow
                    bytes = socket.Receive( receiveBuffer, 0, socket.ReceiveBufferSize, SocketFlags.None, out errorCode );
                    this.receiveBlocking = false;
                }
                catch ( SocketException se ) {
                    lastSocketError = se.SocketErrorCode;
                    if ( se.SocketErrorCode != SocketError.Interrupted &&
                        se.SocketErrorCode != SocketError.Shutdown &&
                        se.SocketErrorCode != SocketError.Disconnecting )
                        Close();
                    if ( se.SocketErrorCode != SocketError.Interrupted )
                        return;
                    if ( se.SocketErrorCode == SocketError.ConnectionAborted ||
                        se.SocketErrorCode == SocketError.ConnectionReset ||
                        se.SocketErrorCode == SocketError.NotConnected )
                        Close();
                    //else let shutdown kill it.
                }
                catch ( ThreadInterruptedException ) {
                    //shutdown should be false, let it die.
                }
                catch ( ObjectDisposedException ) {
                    //shutdown should be false, let it die.
                }
                catch ( NullReferenceException ) {
                    //shutdown should be false, let it die.
                }

                lastSocketError = errorCode;

                //Call the event if we're not shutting down.
                if ( bytes > 0 && !shutdown && OnReceive != null ) {
                    int i = 0;
                    int offset = 0;

                    if ( overflow ) {
                        //Check for special case: data..cr|lf
                        if ( overflowBuffer[overflowLen - 1] == 13 && receiveBuffer[0] == 10 ) {
                            eventMsg = new byte[overflowLen-1];
                            for ( int j = 0; j < overflowLen-1; j++ )
                                eventMsg[j] = overflowBuffer[j];
                            OnReceive( this.sockId, IRCProtocol.Ascii.GetString( eventMsg ) );
                            i++; //We ate the LF already.
                            offset++;
                        }
                        //Check for special case: data..|crlf
                        if ( overflowBuffer[0] == 13 && overflowBuffer[1] == 10 ) {
                            eventMsg = new byte[overflowLen];
                            for ( int j = 0; j < overflowLen; j++ )
                                eventMsg[j] = overflowBuffer[j];
                            OnReceive( this.sockId, IRCProtocol.Ascii.GetString( eventMsg ) );
                            i += 2; //We ate the CR LF already.
                            offset += 2; //We ate the CR LF already.
                        }
                        //Do regular case: data..|data..crlf
                        for ( ; i < bytes - 2; i++ ) //Unnecessary should always terminate through the break.
                            if ( receiveBuffer[i + 1] == 13 && receiveBuffer[i + 2] == 10 ) {
                                eventMsg = new byte[i - offset + 1 + overflowLen];
                                for ( int j = 0; j < overflowLen; j++ )
                                    eventMsg[j] = overflowBuffer[j];
                                for ( int j = 0; j <= i; j++ )
                                    eventMsg[j - offset + overflowLen] = receiveBuffer[j];
                                offset = i + 3;
                                i += 2;
                                OnReceive( this.sockId, IRCProtocol.Ascii.GetString( eventMsg ) );
                                break; //Let the for below continue this work regularily.
                            }
                    }
                    for ( ; i < bytes - 2; i++ )
                        if ( receiveBuffer[i + 1] == 13 && receiveBuffer[i + 2] == 10 ) {
                            eventMsg = new byte[i - offset + 1];
                            for ( int j = offset; j <= i; j++ )
                                eventMsg[j - offset] = receiveBuffer[j];
                            offset = i + 3;
                            i += 2;
                            OnReceive( this.sockId, IRCProtocol.Ascii.GetString( eventMsg ) );
                        }

                    if ( i >= socket.ReceiveBufferSize - 2 && 
                        receiveBuffer[socket.ReceiveBufferSize - 2] != 13 &&
                        receiveBuffer[socket.ReceiveBufferSize - 1] != 10 ) {
                        //CRLF was not the last two, so we didn't eat the message already.
                        //The bytes received was the size of the buffer so there might be more waiting for us.

                        //Offset SHOULD be the first alpha in the command.
                        overflowLen = socket.ReceiveBufferSize - offset;
                        for ( int j = 0; j < overflowLen; j++ )
                            overflowBuffer[j] = receiveBuffer[j + offset];

                        overflow = true; //So we know to concat the data from overflow.
                    }
                    else
                        overflow = false;
                }
                else if ( bytes == 0 && errorCode != SocketError.TimedOut )
                    Close(); //This means the host has been closed remotely.

            }
        }

        /// <summary>
        /// Stops the Socket Pipe.
        /// </summary>
        public void Close() {
         shutdown = true;

            if ( receiveBlocking )
                recvThread.Interrupt();
            if ( sendSleeping )
                sendThread.Interrupt();

            if ( socket != null ) {
                socket.Close();
                socket = null;
            }

            if ( OnDisconnect != null )
                OnDisconnect( this.sockId );
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose() {
            shutdown = true;

            if ( receiveBlocking )
                recvThread.Interrupt();
            if ( sendSleeping )
                sendThread.Interrupt();

            if ( socket != null ) {
                socket.Close();
                socket = null;
            }
        }

        #endregion
    }

}
