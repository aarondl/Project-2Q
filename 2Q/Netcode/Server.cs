using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Reflection;
using System.IO;

using Project2Q.SDK;
using Project2Q.SDK.ModuleSupport;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;

namespace Project2Q.Core {

    /// <summary>
    /// Represents a connection to a server and it's state.
    /// </summary>
    internal sealed class Server : IDisposable {

        #region Static Components

        private static Server[] servers;
        private static EventInfo[] eventList;

        /// <summary>
        /// Creates shared variables for Server.
        /// </summary>
        static Server() {
            servers = new Server[Project2QService.MaxServers];
            for ( int i = 0; i < Project2QService.MaxServers; i++ )
                servers[i] = null;

            eventList = typeof( IRCEvents ).GetEvents( BindingFlags.Instance | BindingFlags.Public );
        }

        /// <summary>
        /// Returns the next useable ServerID.
        /// </summary>
        /// <returns>A ServerID if there's one to be had. Else -1.</returns>
        private static int NextServerId {
            get {
                for ( int i = 0; i < Project2QService.MaxServers; i++ )
                    if ( servers[i] == null )
                        return i;
                return -1;
            }
        }

        /// <summary>
        /// Gets the Server object corresponding to that ID.
        /// </summary>
        /// <param name="sid">Server ID of object.</param>
        /// <returns>The Server requested.</returns>
        public static Server GetServer(int sid) {
            return servers[sid];
        }

        #endregion

        #region Variables + Properties

        //2Q stuff
        private Project2QService.WriteLogFunction writeLogFunction = null;
        private Configuration.ServerConfig config;
        private IRCEvents irce;
        private SocketPipe socketPipe;
        private Thread socketPipeThread;
        private int serverId;
        private bool authmode;
        private IPAddress currentIP;

        private UserCollection uc;
        private ChannelCollection cc;
        private IRCHost currentHost;

        /// <summary>
        /// Server States.
        /// </summary>
        [Flags]
        public enum State {
            /// <summary>
            /// The state is halted, not to be restarted.
            /// </summary>
            Halt = 0x10,
            /// <summary>
            /// Disconnected, will attempt to reconnect.
            /// </summary>
            Disconnected = 0x8,
            /// <summary>
            /// Disconnecting, we are attempting to disconnect.
            /// </summary>
            Disconnecting = 0x4,
            /// <summary>
            /// Connecting, in the process of connecting.
            /// </summary>
            Connecting = 0x2,
            /// <summary>
            /// Connected and all is well.
            /// </summary>
            Connected = 0x1,
        }

        private State state;

        /// <summary>
        /// Returns the Socket Pipe Thread so we can join it.
        /// Not to be used by anything other than 2Q class.
        /// </summary>
        public Thread SocketPipeThread {
            get { return socketPipeThread; }
        }

        /// <summary>
        /// Gets the list of host names this server attempts
        /// to connect to.
        /// </summary>
        public string [] HostNames {
            get { return config.HostList; }
        }

        /// <summary>
        /// Gets or sets the current IP address of the bot. Can be null.
        /// </summary>
        public IPAddress CurrentIP {
            get { return currentIP; }
            set { currentIP = value; }
        }
        
        /// <summary>
        /// Gets the hostname the server used to connect with.
        /// </summary>
        public string HostName {
            get { return socketPipe.HostConnectedWith; }
        }

        /// <summary>
        /// Gets the Nickname the server tries to connect with.
        /// </summary>
        public string Nickname {
            get { return config.NickName; }
        }

        /// <summary>
        /// Gets the alternate nickname the server tries to connect with.
        /// </summary>
        public string AlternateNick {
            get { return config.AltNickname; }
        }

        /// <summary>
        /// Gets or Sets the current nickname the server is using.
        /// </summary>
        public string CurrentNickName {
            get { return currentHost.Nick; }
            set { currentHost.Nick = value; }
        }

        public Configuration.ServerConfig Config {
            get { return config; }
        }

        /// <summary>
        /// Gets or Sets the current host the server is using.
        /// </summary>
        public IRCHost CurrentHost {
            get { return currentHost; }
            set { currentHost = value; }
        }

        /// <summary>
        /// Gets the Username the server tries to connect with.
        /// </summary>
        public string Username {
            get { return config.Username; }
        }

        /// <summary>
        /// Gets the Information the server tries to connect with.
        /// </summary>
        public string Info {
            get { return config.IrcInfo; }
        }

        /// <summary>
        /// Gets the port the server tries to connect to.
        /// </summary>
        public int Port {
            get { return config.ServerPort; }
        }

        /// <summary>
        /// Returns the current server state.
        /// </summary>
        public State ServerState {
            get { return state; }
        }

        /// <summary>
        /// Returns the ID for this instance of Server.
        /// </summary>
        public int ServerID {
            get { return serverId; }
        }

        /// <summary>
        /// Returns the event object for this instance of server.
        /// </summary>
        public IRCEvents EventObject {
            get { return irce; }
        }

        /// <summary>
        /// Returns the UserCollection object for this instance of server.
        /// </summary>
        public UserCollection UserDatabase {
            get { return uc; }
        }

        /// <summary>
        /// Returns the ChannelCollection object for this instance of server.
        /// </summary>
        public ChannelCollection ChannelDatabase {
            get { return cc; }
        }

        /// <summary>
        /// Checks to see if the server is in authentication mode.
        /// </summary>
        public bool IsInAuthMode {
            get { return authmode; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a server from a ServerConfig.
        /// </summary>
        /// <param name="config">The configuration to build the Server from.</param>
        public Server(Configuration.ServerConfig config)
            : this( config, null ) {
        }

        /// <summary>
        /// Creates a server from a ServerConfig with a parameter
        /// to provide an interface for logging.
        /// </summary>
        /// <param name="config">The configuration to build the Server from.</param>
        /// <param name="logFunction">The function to call to log text for the application.</param>
        public Server(Configuration.ServerConfig config, Project2QService.WriteLogFunction logFunction) {

            //Instantiate and load databases
            uc = new UserCollection();
            cc = new ChannelCollection();
            uc.LoadRegisteredUsers( "userdb\\" + config.Name + ".udb" );

            //Rip up this little guy to help us out :D
            currentHost = new IRCHost();
            currentIP = null;

            this.authmode = !File.Exists( "userdb\\" + config.Name + ".udb" );

            this.writeLogFunction = logFunction;

            //Assign a Server ID
            this.serverId = Server.NextServerId;
            if ( this.serverId == -1 ) throw new OverflowException( "Too many servers created." );
            servers[serverId] = this;

            //Save the configuration.
            this.config = config;

            //Tie default static handlers together for this instance of IRCEvents.
            irce = new IRCEvents();

            //Initialize the socket pipe before the modules. Modules are scary.
            state = State.Disconnected;

            socketPipe = new SocketPipe(
                this.serverId, config.RetryTimeout, config.OperationTimeout, config.SendInhibit, config.SocketBufferSize ); //Default values for now, get them from config later plz.
            socketPipe.OnDisconnect += new SocketPipe.NoParams( this.OnDisconnect );
            socketPipe.OnReceive += new SocketPipe.ReceiveData( this.OnReceive );

        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a server, blind to authmode differences.
        /// </summary>
        /// <returns>Wether or not modules should be activated.</returns>
        public bool Init() {

            if ( authmode ) {
                WriteLog( "Building Server: " + config.Name + "in auth mode." );
                InitializeAuthMode();
            }
            else {
                WriteLog( "Building Server: " + config.Name );
                InitializeNormalMode();
            }

            return !authmode; //if authmode is on, do not activate modules.

        }

        /// <summary>
        /// Sets up a server to connect and pick up authentication information
        /// before starting up normally.
        /// </summary>
        public void InitializeAuthMode() {

            WriteLog( " Attaching Needed Handlers..." );

            irce.Ping += new IRCEvents.String1( IRCEventHandlers.PingHandler );
            irce.UserMessage += new IRCEvents.UserMessageType( IRCEventHandlers.AuthModeHandler ); //Special handler.
            irce.Connect += new IRCEvents.ServerEvent( IRCEventHandlers.ConnectHandler );

        }

        /// <summary>
        /// Exits authmode and initializes the server to run normally.
        /// </summary>
        public void ExitAuthMode() {

            WriteLog( " Initial Authentication completed." );

            WriteLog( " Detaching Handlers..." );
            //We may be able to just leave these attached but I don't see why we would want to.
            irce.Ping -= new IRCEvents.String1( IRCEventHandlers.PingHandler );
            irce.UserMessage -= new IRCEvents.UserMessageType( IRCEventHandlers.AuthModeHandler ); //Special handler.
            irce.Connect -= new IRCEvents.ServerEvent( IRCEventHandlers.ConnectHandler );

            this.authmode = false;

            WriteLog( " Exiting Authmode." );

            //Activate all the modules on this server now that we're done authmode.
            Project2QService.AuthModeSpecialActivateModules( serverId );

            InitializeNormalMode();

        }

        /// <summary>
        /// Initializes a server to connect and run normally.
        /// </summary>
        public void InitializeNormalMode() {

            WriteLog( " Tying in events..." );
            AttachHandlers();

        }

        /// <summary>
        /// Connects the Server. This call starts a thread and
        /// consumes the current one for execution of Recv/Sends.
        /// </summary>
        public void Connect() {

            if ( ( state & State.Halt ) != 0 )
                return;

            state |= State.Connecting;

            try {
                WriteLog( "Trying to connect to: [" + config.Name + "]..." );

                socketPipe.Initialize();

                int i = 2;
                while ( !socketPipe.Connect( config.HostList, (ushort)config.ServerPort ) ) {
                    if ( ( state & State.Halt ) == State.Halt ) return;
                    WriteLog( "Trying to connect to " + config.Name + ": Attempt " + (i++).ToString() );
                }
            }
            catch ( ThreadInterruptedException ) {
                return;
            }

            state = State.Connected;
            WriteLog( "Connection Established: [" + socketPipe.HostConnectedWith + "]" );

            this.CurrentNickName = this.config.NickName; //Subject to change quickly!

            QueueData( irce.OnConnect( this.ServerID ) );

            if ( ( state & State.Connected ) != 0 ) {
                BeginSending();
                BeginReceiving();
            }
        }

        /// <summary>
        /// Begins receiving.
        /// </summary>
        public void BeginReceiving() {
            if ( ( state & State.Connected ) == 0 || ( state & State.Halt ) != 0 )
                return;

            //This is a threaded function and will only return when the thread
            //is having problems or is going to shut down. No mans land, we probably
            //won't return QQ.
            socketPipe.ConstantSiphon();
        }

        /// <summary>
        /// Begins sending data on a seperate thread.
        /// </summary>
        /// <returns>Success?</returns>
        public bool BeginSending() {
            if ( ( state & State.Connected ) == 0 || ( state & State.Halt ) != 0 )
                return false;

            try {
                socketPipeThread = new Thread( new ThreadStart( socketPipe.ConstantPump ) );
                socketPipeThread.Start();
            }
            catch ( ThreadStartException ) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Queues all commands to be sent.
        /// </summary>
        /// <param name="commands">The array of raw commands.</param>
        /// <returns>If it all sent okay.</returns>
        public bool QueueData(string[][] commands) {
            if ( commands == null )
                return true;
            foreach ( string[] qq in commands )
                if ( qq == null )
                    continue;
                else
                    foreach ( string q in qq ) {
                        if ( q == null ) continue;
                        if ( !SendData( q ) ) return false;
                    }
            return true;
        }

        /// <summary>
        /// Sends data into the Socket Pipe.
        /// </summary>
        /// <param name="text">The text to send.</param>
        public bool SendData(string text) {
            if ( ( state & State.Connected ) == 0 )
                return false;
            socketPipe.Pump( text );

            return true;
        }

        /// <summary>
        /// Receives data and throws it into parse for it
        /// to have it's events fired etc.
        /// </summary>
        /// <param name="sockId">The connection ID.</param>
        /// <param name="data">The data received.</param>
        public void OnReceive(int sockId, string data) {
#if DEBUG
            WriteLog( sockId.ToString() + " -> " + data );
#endif
            IRCProtocol.Parse( this, data );
        }

        /// <summary>
        /// This event callback will be hit in the event that the
        /// server disconnects from us for whatever reason. If we
        /// have not been told to halt. Then reconnect.
        /// </summary>
        /// <param name="sockId">The socket id.</param>
        public void OnDisconnect(int sockId) {
            if ( ( state & State.Halt ) == 0 ) {
                state = State.Disconnected;
                //Clear the databases.
                uc.RemoveAll();
                cc.RemoveAll();
                //Clear IP, as the reason for disconnect may have been an IP reset.
                this.currentIP = null;
                //Tell the dumb modules /sigh.
                this.irce.OnDisconnect( serverId, "Disonnected from server, no reason known." ); //TODO: Maybe provide a real reason? Kek.
                WriteLog( "Disconnected: [" + socketPipe.HostConnectedWith + "]. Reconnecting in: " + (config.RetryTimeout/1000).ToString() + "seconds..." );
                Thread.Sleep( config.RetryTimeout );
                Connect();
            }
            else {
                //Clear IP, just in case ^_^
                this.currentIP = null;
                this.irce.OnDisconnect( serverId, "Intentional Shutdown." );
                WriteLog( "Connection closed: [" + socketPipe.HostConnectedWith + "]" );
                state = State.Disconnected | State.Halt;
            }
        }

        /// <summary>
        /// Halts the server. With an option to
        /// halt it permanently or temporarily.
        /// </summary>
        /// <param name="permanent">Is the shutdown permanent?</param>
        public void Halt(bool permanent) {

            state |= State.Disconnecting;

            if ( permanent ) state |= State.Halt;

            socketPipe.Close();

            state &= ~State.Disconnecting;
            state |= State.Disconnected;
        }

        /// <summary>
        /// Halts the server by sending a quit message,
        /// the remote host should close the socket for us.
        /// </summary>
        /// <param name="permanent">Is this shutdown permanent?</param>
        public void SoftHalt(bool permanent) {
            state |= State.Disconnecting;

            if ( permanent ) state |= State.Halt;

            socketPipe.Pump( "QUIT :" + this.config.QuitMessage );
        }

        /// <summary>
        /// Writes to the applications log and log file. Silently fails
        /// if no logging parameters were sent to the Server on creation.
        /// </summary>
        /// <param name="text">Text to log.</param>
        public void WriteLog(string text) {
            if ( this.writeLogFunction != null )
                writeLogFunction.Invoke( text );
        }

        /// <summary>
        /// Attaches default event handlers for the server. (see:IRCEventHandlers.cs)
        /// </summary>
        public void AttachHandlers() {

            Type handlerType = typeof( IRCEventHandlers );

            foreach ( EventInfo e in eventList ) {
                string eventHandlerName = e.Name + "Handler";
                MethodInfo mi = handlerType.GetMethod( eventHandlerName,
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod );
                if ( mi == null )
                    continue;
                Delegate d = null;
                if ( mi.IsStatic )
                    d = Delegate.CreateDelegate( e.EventHandlerType, mi );
                else
                    d = Delegate.CreateDelegate( e.EventHandlerType, irce, mi.Name, true );
                e.AddEventHandler( irce, d );
            }
        }

        /// <summary>
        /// Detaches default event handlers for the server. (see:IRCEventHandlers.cs)
        /// </summary>
        public void DetachHandlers() {
            Type handlerType = typeof( IRCEventHandlers );

            foreach ( EventInfo e in eventList ) {
                string eventHandlerName = e.Name + "Handler";
                MethodInfo mi = handlerType.GetMethod( eventHandlerName,
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod );
                if ( mi == null )
                    continue;
                Delegate d = null;
                if ( mi.IsStatic )
                    d = Delegate.CreateDelegate( e.EventHandlerType, mi );
                else
                    d = Delegate.CreateDelegate( e.EventHandlerType, irce, mi.Name, true );
                e.RemoveEventHandler( irce, d );
            }
        }

        /// <summary>
        /// Retrieves a variable from the server for a module.
        /// </summary>
        /// <param name="var">The variable requested.</param>
        /// <returns>The variable requested.</returns>
        public static object RetrieveVariable(Request var, int sid) {

            if ( var == Request.Configuration )
                return Project2QService.Config;
            else if ( var == Request.ModuleList )
                return Project2QService.Modules;

            if ( sid < 0 || sid > Project2QService.MaxServers )
                return null;

            Server s = Server.GetServer( sid );
            if ( s == null )
                return null;

            switch ( var ) {
                case Request.ChannelCollection:
                    return s.ChannelDatabase;
                case Request.UserCollection:
                    return s.UserDatabase;
                case Request.Configuration:
                    return Project2QService.Config;
                case Request.ServerConfiguration:
                    return s.Config;
                case Request.IRCEvents:
                    return s.EventObject;
                case Request.ModuleList:
                    return Project2QService.Modules;
                case Request.SendData:
                    return new ModuleProxy.SendDataDelegate( s.SendData );
                case Request.CurrentIP:
                    return s.CurrentIP;
            }

            return null;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Frees up all resources used by the server.
        /// </summary>
        public void Dispose() {
            //We're gonna cheat here, normally this is only used to free up "resources"
            Server.servers[this.serverId] = null;
            socketPipe.Dispose();
            uc.SaveRegisteredUsers( "userdb\\" + config.Name + ".udb" );
            uc.RemoveAll();
            cc.RemoveAll();
        }

        #endregion
    }

}
