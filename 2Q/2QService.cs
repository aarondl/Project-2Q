using System;
using System.ServiceProcess;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Security.Permissions;

using Project2Q.SDK;
using Project2Q.SDK.ModuleSupport;

[assembly: PermissionSet(SecurityAction.RequestMinimum, Name = "FullTrust")]
namespace Project2Q.Core {

    /// <summary>
    /// This class encapsulates the Bot that runs 
    /// as a Windows Service. The Entrypoint for the entire system.
    /// </summary>
    public sealed class Project2QService : /*ServiceBase, */IDisposable {

        #region Main

        /// <summary>
        /// MAIN :D
        /// </summary>
        /// <param name="args">2Q can take a single arguement to specify a new config file location.</param>
        [MTAThread]
        public static void Main(string[] args) {
            /*if ( args.Length > 1 )
                ServiceBase.Run( new Project2Q(args[1]) );
            else
                ServiceBase.Run( new Project2Q() );*/
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor, uses the current directory
        /// and the name Config.xml to load the bot.
        /// </summary>
        public Project2QService() : this("Config.xml") {}

        public void SendMessage(int sid, string msg) {
            Server.GetServer( sid ).SendData( msg );
        }

        /// <summary>
        /// Constructs a 2Q.
        /// </summary>
        /// <param name="configFile">Path to the configuration file.</param>
        public Project2QService(string configFile) {

            /*this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
            this.CanHandlePowerEvent = true;
            this.AutoLog = false;
            this.ServiceName = "2Q";*/

            config = new Configuration( configFile );
            //config = new Configuration();

            logFile = new FileStream( Environment.CurrentDirectory + "\\2Q.log", FileMode.Append,
                FileAccess.Write, FileShare.None );
            

        }

        /// <summary>
        /// Constructs a 2Q.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        /// <param name="consoleRead">Redirection of stdin.</param>
        /// <param name="consoleWrite">Redirection of stdout.</param>
        public Project2QService(string configFile, TextReader consoleRead, TextWriter consoleWrite) {
            config = new Configuration( configFile );

            logFile = new FileStream( Environment.CurrentDirectory + "\\2Q.log", FileMode.Append,
                FileAccess.Write, FileShare.None );

            Console.SetIn( consoleRead );
            Console.SetOut( consoleWrite );

            modules = new IModule[MaxModules];
            servers = new ServerThread[MaxServers];
        }

        private static FileStream logFile;

        #endregion

        #region Types

        /// <summary>
        /// Immutable once created, a ServerThread keeps track
        /// of a ServerThread pair.
        /// </summary>
        private class ServerThread {
            public ServerThread(Server s, Thread t) {
                server = s;
                thread = t;
            }
            private Server server;
            private Thread thread;
            private bool loadModules;

            /// <summary>
            /// Returns the Server portion of the struct.
            /// </summary>
            public Server Server { get { return server; } }
            /// <summary>
            /// Returns the thread portion of the struct.
            /// </summary>
            public Thread Thread { get { return thread; } }
            /// <summary>
            /// Gets or sets wether or not we should be loading modules.
            /// </summary>
            public bool LoadModules {
                get { return loadModules; }
                set { loadModules = value; }
            }

        }

        public delegate void WriteLogFunction(string textToLog);

        #endregion

        #region Variables + Properties

        /// <summary>
        /// Gets the config for the current run.
        /// </summary>
        public static Configuration Config {
            get { return config; }
        }

        public static IModule[] Modules {
            get { return modules; }
        }

        private static Configuration config;
        private static Rcon rconsole;

        /// <summary>
        /// The maximum amount of modules we can load.
        /// </summary>
        public static readonly int MaxModules = 32;
        public static readonly int MaxServers = 32;        
        private static IModule[] modules;
        private static ServerThread[] servers;

        #endregion

        #region Modules

        /// <summary>
        /// Activates the modules for a given server, only to be used when Authmode
        /// was in play. Otherwise leave it to normal execution.
        /// </summary>
        /// <param name="sid">The server to activate modules on.</param>
        public static void AuthModeSpecialActivateModules(int sid) {

            //This is getting ugly, as multiple servers can try to access this method at the same time.

            lock ( modules ) {

                servers[sid].LoadModules = true;

                //Check to see if all of our servers are activated/ready to be activated.
                bool allServersReady = true;
                for ( int s = 0; s < MaxServers; s++ )
                    if ( servers[s] == null ) break;
                    else allServersReady = allServersReady && servers[s].LoadModules;

                //Activate the modules on the requested server.
                for ( int m = 0; m < MaxModules; m++ ) {
                    if ( modules[m] == null ) continue;
                    if ( !modules[m].ModuleConfig.QueryServerList( servers[sid].Server.Config.Name ) )
                        continue; //Do not activate this module if it's not supposed to be active.
                    modules[m].Activate( sid );
                    WriteLog( "  Activated Link: " + servers[sid].Server.Config.Name + "." + modules[m].ModuleConfig.FullName );
                    if ( allServersReady ) {
                        modules[m].ActivationComplete();
                        WriteLog( "  Activation Complete: " + modules[m].ModuleConfig.FullName );
                    }
                }

            }
        }

        /// <summary>
        /// Retrieves the next useable module ID if there is one to be had.
        /// Returns -1 if it cannot find one.
        /// </summary>
        private static int NextModuleID {
            get {
                for ( int i = 0; i < MaxModules; i++ )
                    if ( modules[i] == null )
                        return i;
                return -1;
            }
        }

        /// <summary>
        /// Loads modules for the bot.
        /// </summary>
        /// <param name="mods">The modules to load.</param>
        public static void LoadModules(Configuration.ModuleConfig[] mods) {

            if ( mods == null ) return;

            foreach ( Configuration.ModuleConfig c in mods ) {

                //Check if all files are present.
                //This check is redundant. It happens again in the actual module loading but we can skip a lot
                //of memory allocation and what-not stuff by doing it here first.
                bool filecheck = true;
                foreach ( string filename in c.FileNames )
                    if ( !File.Exists( Path.GetFullPath( filename ) ) &&
                        ( Configuration.ModuleConfig.ModulePath == null ||
                         !File.Exists( Path.GetFullPath( Path.Combine( Configuration.ModuleConfig.ModulePath, filename ) ) ) ) ) {
                        WriteLog( "  Module load Failed: " + c.FullName + ", File not found: " + Path.GetFileName( filename ) );
                        filecheck = false;
                        break;
                    }

                //TODO: Make sure this check works. Seems okay.
                if ( filecheck && c.IsScript )
                    foreach ( string filename in c.Includes )
                        if ( !File.Exists( Path.GetFullPath( filename ) ) &&
                            ( Configuration.ModuleConfig.IncludePath == null ||
                             !File.Exists( Path.GetFullPath( Path.Combine( Configuration.ModuleConfig.IncludePath, filename ) ) ) ) ) {
                            if ( !File.Exists( filename ) &&
                                !File.Exists( Path.GetFullPath( Path.Combine( Configuration.ModuleConfig.FrameworkPath, filename ) ) ) ) {
                                WriteLog( "  Module load Failed: " + c.FullName + ", Referenced Dependency not found: " + Path.GetFileName( filename ) );
                                filecheck = false;
                                break;
                            }
                        }

                if ( !filecheck )
                    continue;

                //Assign moduleId if we can.
                int moduleId = Project2QService.NextModuleID;
                if ( moduleId == -1 ) throw new OverflowException( "Too many modules created." );

                //Load up the modules, catch ModuleLoadExceptions to avoid crashing if a bad module is around.
                if ( c.IsScript ) {
                    ScriptedModule sm = new ScriptedModule( c, moduleId );

                    try {
                        sm.LoadModule();
                    }
                    catch ( ModuleLoadException mle ) {
                        WriteLog( "  Module load failed: " + "." + c.FullName + ", " + mle.Message );
                        string[] errors = mle.CompilerErrors;
                        if ( errors != null )
                            foreach ( string error in errors )
                                WriteLog( "   Compiler Error: " + error );
                        continue;
                    }

                    modules[moduleId] = sm;

                    //WriteLog( "  Module Loaded: " + c.FullName ); Cheating for nicer output. See StartServerThreads

                }
                else {
                    CompiledModule cm = new CompiledModule( c, moduleId );

                    try {
                        cm.LoadModule();
                    }
                    catch ( ModuleLoadException mle ) {
                        WriteLog( "  Module load failed: " + mle.Message );
                        continue;
                    }

                    modules[moduleId] = cm;

                    //WriteLog( "  Module Loaded: " + c.FullName ); Cheating for nicer output. See StartServerThreads
                }

            }
        }

        /// <summary>
        /// Unloads all modules.
        /// </summary>
        public static void UnloadModules() {
            foreach ( IModule i in modules ) {
                if ( i != null && i.IsLoaded ) {
                    i.UnloadModule();
                    //WriteLog( "  Module Unloaded: " + i.ModuleConfig.FullName ); Cheating for nicer output. See OnStop
                }
            }
        }

        /// <summary>
        /// Reloads all modules.
        /// </summary>
        public static void ReloadModules() {
            foreach ( IModule i in modules ) {
                if ( i != null && !i.IsLoaded ) {
                    i.LoadModule();
                    WriteLog( "  Module Reloaded: " + i.ModuleConfig.FullName );
                }
            }
        }

        /// <summary>
        /// Load a module.
        /// </summary>
        /// <param name="mc">The module config of the module to load.</param>
        public static void LoadModule(Configuration.ModuleConfig mc) {
            LoadModules( new Configuration.ModuleConfig[] { mc } );
        }

        /// <summary>
        /// Reloads a specific module.
        /// </summary>
        /// <param name="moduleId">The module to reload.</param>
        public static void ReloadModule(int moduleId) {
            if ( modules[moduleId] != null && !modules[moduleId].IsLoaded ) {
                modules[moduleId].UnloadModule();
                modules[moduleId].LoadModule();
                WriteLog( "  Module Reloaded: " + modules[moduleId].ModuleConfig.FullName );
            }
        }

        /// <summary>
        /// Unloads a specific module.
        /// </summary>
        /// <param name="moduleId">The module to unload.</param>
        public static void UnloadModule(int moduleId) {
            if ( modules[moduleId] != null && modules[moduleId].IsLoaded ) {
                modules[moduleId].UnloadModule();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Takes the configuration info, and begins the threading
        /// model to take care of all the Server objects created.
        /// </summary>
        private void StartServerThreads() {

            //Make sure the file system environment is set up for the servers:
            if ( !Directory.Exists( "userdb" ) )
                Directory.CreateDirectory( "userdb" );
            if ( !Directory.Exists( "logs" ) )
                Directory.CreateDirectory( "logs" );
            if ( !Directory.Exists( "modules" ) )
                Directory.CreateDirectory( "modules" );
            
            Configuration.ServerConfig[] list = config.Servers;

            WriteLogFunction wlf = new WriteLogFunction( WriteLog );

            bool allServersReady = true;

            //Loads & Initializes all servers.
            for ( int i = 0; i < list.Length; i++ ) {
                Server s = new Server( list[i], wlf );
                Thread t = new Thread( s.Connect );
                servers[i] = new ServerThread( s, t );
                servers[i].LoadModules = s.Init();
                allServersReady = allServersReady && servers[i].LoadModules;
            }

            //Load all modules.
            WriteLog( "Loading Modules..." );
            LoadModules( config.Modules );

            //Activate all the modules.
            for ( int m = 0; m < MaxModules; m++ ) {
                if ( modules[m] == null ) continue;

                WriteLog( "  Module Loaded: " + modules[m].ModuleConfig.FullName );

                for ( int s = 0; s < MaxServers; s++ ) {
                    if ( servers[s] == null || !servers[s].LoadModules || 
                        !modules[m].ModuleConfig.QueryServerList( servers[s].Server.Config.Name ) ) continue;

                    modules[m].Activate( s );
                    WriteLog( "    Activated Link: " + servers[s].Server.Config.Name + "." + modules[m].ModuleConfig.FullName );
                }

                if ( allServersReady ) {
                    WriteLog( "    Activation Completed: " + modules[m].ModuleConfig.FullName );
                    modules[m].ActivationComplete();
                }
            }

            foreach ( ServerThread st in servers )
                if ( st == null ) continue;
                else st.Thread.Start();

        }


        /// <summary>
        /// Starts the Remote Console Server.
        /// </summary>
        private void StartRCON() {

            Configuration.RconConfig rconfig = config.RemoteConsole;
            rconsole = new Rcon( rconfig, new WriteLogFunction( WriteLog ) );
            rconsole.BeginListen();

            Thread t = new Thread( new ThreadStart( rconsole.ConstantAccept ) );
            t.Start();

        }

        /// <summary>
        /// Adds a timestamp to the text and writes it to the Services Eventlog.
        /// </summary>
        /// <param name="text">The text to write to the EventLog.</param>
        public static void WriteLog(string text) {
            //string tolog = DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) + " - " + text + System.Environment.NewLine;
            string tolog = text + System.Environment.NewLine;
            byte[] entry = IRCProtocol.Ascii.GetBytes( tolog );
            Console.Write( tolog );
            //this.EventLog.WriteEntry( DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) + " - " + text );
            logFile.Write( entry, 0, entry.Length );
        }

        /// <summary>
        /// Reads in the application EventLog and spits it into
        /// a configurable destination defined by Configuration.
        /// If none exist this method silently fails.
        /// </summary>
        private void DumpLog() {

            /*string logFile = "BATLog.FIXME.log";
            if ( logFile != null && logFile != string.Empty ) {

                FileStream fs = new FileStream( "BATLog.FIXME.LOG", FileMode.Append, FileAccess.Write, FileShare.None );

                IEnumerator ie = this.EventLog.Entries.GetEnumerator();

                byte[] entry;

                while ( ie.MoveNext() ) {
                    string writeEntry = ( (string)ie.Current ) + "\n";
                    System.Text.ASCIIEncoding ae = new System.Text.ASCIIEncoding();
                    entry = ae.GetBytes( writeEntry );
                    fs.Write( entry, 0, entry.Length );
                }
                fs.Dispose();
            }*/
        }

        #endregion

        #region Service Events

        /// <summary>
        /// Happens when the service starts.
        /// </summary>
        /// <param name="args">Arguments from the user</param>
        public void OnStart(object args) {

            //ARGS IS CHANGED INTO AN ARRAY OF STRING REMEMBER THAT PLZ

            WriteLog( "2Q Started." );

            //MessageBox.Show( "STARTING" );

            //DumpLog();

            //if ( config.RCON )
            //    StartRCON();
            StartServerThreads();

            /*if ( config.RCON == true )
                StartRCON();*/

            /*Thread t = new Thread( new ThreadStart(rofl) );
            roflThread = t;
            roflThread.Start();*/

            //base.OnStart( args );
            
        }

        /// <summary>
        /// When the service recovers from a pause.
        /// </summary>
        public void OnContinue() {

            throw new Exception( "OnContinue Needs reimplementing." );

            //WriteLog( "2Q Unpaused." );
        }

        /// <summary>
        /// When the service is paused.
        /// </summary>
        public void OnPause() {

            throw new Exception( "OnPause Needs reimplementing." );

            /*Dictionary<int, ServerThread>.Enumerator e = allThreads.GetEnumerator();
            while ( e.MoveNext() ) {
                //Close server, join the executing thread until it's ceased execution.
                e.Current.Value.Server.Halt( false );
                e.Current.Value.Thread.Join(); //Hopefully it closes quickly.
                e.Current.Value.Server.SocketPipeThread.Join(); //Hopefully it closes quickly!
            }
            e.Dispose();

            WriteLog( "2Q Paused." );*/
        }

        /// <summary>
        /// When the service is shutdown.
        /// </summary>
        public void OnShutdown() {

            throw new Exception( "OnShutdown needs reimplementing" );

            /*foreach ( ServerThread st in servers ) {
                st.Thread.Join();
                st.Server.SocketPipeThread.Join();
            }

            Dictionary<int, ServerThread>.Enumerator e = allThreads.GetEnumerator();

            while ( e.MoveNext() ) {
                //Close server, join the executing thread until it's ceased execution.
                //e.Current.Value.Server.Halt( true );
                e.Current.Value.Thread.Join(); //Hopefully it closes quickly.
                e.Current.Value.Server.SocketPipeThread.Join(); //Hopefully it closes quickly!
            }
            e.Dispose();

            WriteLog( "2Q Shutdown." );

            DumpLog();*/

            //ExitCode = 0;
        }

        /// <summary>
        /// When the service is stopped.
        /// </summary>
        public void OnStop() {

            //if ( config.RCON ) //Since this just forcibly closes all connections etc.
            //    rconsole.Halt( true );

            foreach ( ServerThread st in servers )
                if ( st == null ) continue;
                else
                    //Tell server to shut down
                    st.Server.SoftHalt( true );

            WriteLog( "Unloading Modules..." );
            //Deactivate all the modules.
            for ( int m = 0; m < MaxModules; m++ ) {
                if ( modules[m] == null ) continue;
                WriteLog( "  Unloading: " + modules[m].ModuleConfig.FullName );
                for ( int s = 0; s < MaxServers; s++ ) {
                    if ( servers[s] == null ) continue;
                    if ( modules[m].IsActive( s ) ) {
                        modules[m].Deactivate( s );
                        WriteLog( "    Deactivated Link: " + servers[s].Server.Config.Name + "." + modules[m].ModuleConfig.FullName );
                    }
                }
            }

            foreach ( ServerThread st in servers ) {

                if ( st == null )
                    continue;

                //Wait for death of server thread on our end.
                if ( ( st.Server.ServerState & Server.State.Connecting ) > 0 )
                    st.Thread.Interrupt();
                if ( st.Thread.ThreadState == System.Threading.ThreadState.Running )
                    st.Thread.Join();
                
                //Wait for the death of the server's internal thread.
                if ( st.Server.SocketPipeThread != null )
                    if ( st.Server.SocketPipeThread.ThreadState == System.Threading.ThreadState.Running )
                        st.Server.SocketPipeThread.Join();

                //Destroy it.
                st.Server.Dispose();
            }

            //WriteLog( "Unloading Modules..." ); Cheating for nicer output, see above where we deactivate.
            //Unload all the modules.
            UnloadModules();

            WriteLog( "2Q Stopped." );

            DumpLog();

            //ExitCode = 0;
        }

        /// <summary>
        /// When the service recovers or goes into a power event.
        /// </summary>
        /// <param name="powerStatus">Power status to adjust to.</param>
        /// <returns>Success?</returns>
        public bool OnPowerEvent(PowerBroadcastStatus powerStatus) {

            switch ( powerStatus ) {
                case PowerBroadcastStatus.Suspend:
                    OnPause();
                    return true;
                case PowerBroadcastStatus.ResumeSuspend:
                    OnContinue();
                    return true;
                case PowerBroadcastStatus.QuerySuspend:
                    return true;
            }
            return true;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the object
        /// </summary>
        void IDisposable.Dispose() {
            logFile.Close();
        }

        #endregion
    }
}
