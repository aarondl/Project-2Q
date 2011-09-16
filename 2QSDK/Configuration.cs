using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using Project2Q.SDK.ModuleSupport;

namespace Project2Q.SDK {

    /// <summary>
    /// Represents a total bot configuration.
    /// </summary>
    public sealed class Configuration {

        public readonly bool RCON = false;
        private RconConfig rconConfig;
        private LinkedList<ServerConfig> serverTempList;
        private LinkedList<ModuleConfig> moduleTempList;

        private static void NextNode(XmlReader R) {
            R.Read();
            R.Read();
            R.Read();
        }

        public Configuration(string pathToConfig) {
            XmlReaderSettings configReaderSettings = new XmlReaderSettings();
            Stream s = typeof(Configuration).Assembly.GetManifestResourceStream("Project2Q.SDK.ConfigSchema.xsd");
            StreamReader configStreamReader = new StreamReader(pathToConfig);

            configReaderSettings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings;
            configReaderSettings.ValidationType = ValidationType.Schema;
            configReaderSettings.IgnoreWhitespace = true;
            configReaderSettings.IgnoreComments = true;
            configReaderSettings.ValidationEventHandler += new ValidationEventHandler(configReaderSettings_ValidationEventHandler);
            configReaderSettings.Schemas.Add(null, XmlReader.Create(s));

            XmlReader cfr = XmlReader.Create(configStreamReader, configReaderSettings);
            try {

                #region Config::Settings
                cfr.Read(); // jump from <Configuration> to <Settings>
                NextNode(cfr);
                string defaultNickName = cfr.Value;
                NextNode(cfr);
                string defaultAlternateNickName = cfr.Value;
                NextNode(cfr);
                string defaultUserName = cfr.Value;
                NextNode(cfr);
                string defaultUserInfo = cfr.Value;
                NextNode(cfr);
                int defaultPort = int.Parse(cfr.Value);
                NextNode(cfr);
                string defaultQuitMessage = cfr.Value;
                NextNode( cfr );
                bool defaultAutoJoinOnInvite = bool.Parse(cfr.Value);
                NextNode(cfr);
                int defaultOperationTimeout = int.Parse(cfr.Value);
                NextNode(cfr);
                int defaultRetryTimeout = int.Parse(cfr.Value);
                NextNode(cfr);
                int defaultSocketBufferSize = int.Parse(cfr.Value);
                NextNode(cfr);
                int defaultSendInhibit = int.Parse(cfr.Value);
                
                #endregion

                #region Config::Servers

                //<Servers>
                NextNode(cfr);

                ServerConfig serv;
                string[] hostList;
                string serverName, username, userInfo, nickName, altNick, quitMessage;
                int sendInhibit, serverPort, operationTimeout, retryTimeout, socketBufferSize;
                bool autoJoinOnInvite, jj;
                uint ii;
                this.serverTempList = new LinkedList<ServerConfig>();

                while (cfr.Name.Equals("Server")) { //Quick Cleanup
                    hostList = new string[32];
                    serverName = cfr.GetAttribute(0);
                    nickName = defaultNickName;
                    username = defaultUserName;
                    userInfo = defaultUserInfo;
                    altNick = defaultAlternateNickName;
                    autoJoinOnInvite = defaultAutoJoinOnInvite;
                    operationTimeout = defaultOperationTimeout;
                    retryTimeout = defaultRetryTimeout;
                    socketBufferSize = defaultSocketBufferSize;
                    sendInhibit = defaultSendInhibit;
                    serverPort = defaultPort;
                    quitMessage = defaultQuitMessage;
                    
                    ii = 0;
                    jj = true;
                    cfr.Read();

                    while (jj) { 
                        switch (cfr.Name) {
                        case "dns":
                            cfr.Read();
                            if (ii < 32) hostList[ii++] = cfr.Value;
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "nickname":
                            cfr.Read();
                            nickName = cfr.Value;
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "alternate":
                            cfr.Read();
                            altNick = cfr.Value;
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "username":
                            cfr.Read();
                            username = cfr.Value;
                            cfr.Read();
                            cfr.Read();
                            break;
                        case "info":
                            cfr.Read();
                            userInfo = cfr.Value;
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "port":
                            cfr.Read();
                            serverPort = int.Parse(cfr.Value);
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "quitMessage":
                            cfr.Read();
                            quitMessage = cfr.Value;
                            cfr.Read();
                            cfr.Read();
                            break;
                        case "autoJoinOnInvite":
                            cfr.Read();
                            autoJoinOnInvite = bool.Parse(cfr.Value);
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "operationTimeout":
                            cfr.Read();
                            operationTimeout = int.Parse( cfr.Value );
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "retryTimeout":
                            cfr.Read();
                            retryTimeout = int.Parse( cfr.Value );
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "socketBufferSize":
                            cfr.Read();
                            socketBufferSize = int.Parse( cfr.Value );
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        case "sendInhibit":
                            cfr.Read();
                            sendInhibit = int.Parse(cfr.Value);
                            cfr.Read(); //This will read past the data
                            cfr.Read(); //This will read to the next node.
                            break;
                        default:
                            cfr.Read();
                            jj = false;
                            break;
                        }
                    }
                    serv = new ServerConfig(nickName, altNick, username, userInfo, serverName, autoJoinOnInvite, 
                        hostList, serverPort, sendInhibit, operationTimeout, retryTimeout, socketBufferSize, quitMessage);

                    serverTempList.AddLast(serv);
                }

                #endregion

                #region Config::Modules

                if (cfr.Name.Equals("Modules")) {

                    moduleTempList = new LinkedList<ModuleConfig>();
                    string[] fileNames, servers = null;
                    string[] includes = new string[128];
                    string moduleName = null, moduleLang = null, prettyName = null;
                    ModuleConfig.ModulePath = cfr.GetAttribute( "modulePath" );
                    ModuleConfig.IncludePath = cfr.GetAttribute( "includePath" );
                    ModuleConfig.FrameworkPath = cfr.GetAttribute( "frameworkPath" );
                    ModuleConfig.ModulePrefix = cfr.GetAttribute( "prefix" );
                    ModuleConfig.ModulePrefix = ModuleConfig.ModulePrefix == null ? "?" : ModuleConfig.ModulePrefix;
                    ModuleConfig.FrameworkPath = ModuleConfig.FrameworkPath == null ?
                        System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("WINDIR"), @"Microsoft.NET\Framework\v2.0.50727") 
                        : ModuleConfig.FrameworkPath;

                    cfr.Read();

                    while (cfr.Name.Equals("Module")) {
                        moduleName = cfr.GetAttribute(0);
                        cfr.Read();

                        if ( cfr.Name.Equals( "prettyname" ) ) {
                            cfr.Read();
                            prettyName = cfr.Value;
                            cfr.Read(); cfr.Read(); cfr.Read();
                        }
                        else {
                            prettyName = moduleName;
                            cfr.Read();
                        }

                        fileNames = cfr.Value.Split(';');
                        cfr.Read(); cfr.Read();

                        servers = null;
                        bool isScript = false;

                        if (cfr.Name.Equals("script")) {
                            cfr.Read();
                            cfr.Read();
                            includes = cfr.Value.Split(';');

                            NextNode(cfr);
                            moduleLang = cfr.Value;
                            NextNode(cfr);

                            isScript = true;
                        }

                        if (cfr.Name.Equals("servers")) {
                            cfr.Read();
                            servers = cfr.Value.Split(';');
                            Array.Sort<string>( servers );
                            NextNode(cfr);
                        }

                        if ((cfr.Name.Equals("Module")) && (cfr.NodeType.Equals(System.Xml.XmlNodeType.EndElement)))
                            cfr.Read();

                        this.moduleTempList.AddLast(new ModuleConfig(moduleName, prettyName, moduleLang, fileNames, includes, servers, isScript));
                     }
                }

                #endregion

                #region Config::RemoteConsole

                cfr.Read();
                if (cfr.Name.Equals("RemoteConsole")) {
                    int rconOperationTimeout = defaultOperationTimeout;
                    int rconRetryTimeout = defaultRetryTimeout;
                    int rconSocketBufferSize = defaultSocketBufferSize;
                    int rconSendInhibit = defaultSendInhibit;
                    cfr.Read(); cfr.Read();
                    string initialAuth = cfr.Value;
                    NextNode(cfr);
                    int rconPort = int.Parse(cfr.Value);
                    cfr.Read(); cfr.Read();

                    if (cfr.Name.Equals("operationTimeout")) {
                        cfr.Read();
                        rconOperationTimeout = int.Parse(cfr.Value);
                        cfr.Read(); cfr.Read();
                    }
                    if (cfr.Name.Equals("retryTimeout")) {
                        cfr.Read();
                        rconRetryTimeout = int.Parse(cfr.Value);
                        cfr.Read(); cfr.Read();
                    }
                    if (cfr.Name.Equals("socketBufferSize")) {
                        cfr.Read();
                        rconSocketBufferSize = int.Parse(cfr.Value);
                        cfr.Read(); cfr.Read();
                    }
                    if (cfr.Name.Equals("sendInhibit")) {
                        cfr.Read();
                        rconSendInhibit = int.Parse(cfr.Value);
                        cfr.Read(); cfr.Read();
                    }

                    this.rconConfig = new RconConfig(initialAuth, rconOperationTimeout, rconRetryTimeout,
                        rconSendInhibit, rconSocketBufferSize, rconPort);
                }

                #endregion

            }

            finally {
                cfr.Close();
                configStreamReader.Close();
                s.Close();
            }
        }

        void configReaderSettings_ValidationEventHandler(object sender, ValidationEventArgs e) {
            Console.WriteLine(e.Message); //Would be nice to be able to trigger this...
            throw new FormatException("Invalid 2Q configuration file");
        }

        public Configuration()
            : this("2Q.xml") {
        }

        /// <summary>
        /// Returns all servers mentioned in this config.
        /// </summary>
        public ServerConfig[] Servers {
            get
            {
                ServerConfig[] res = new ServerConfig[serverTempList.Count];
                serverTempList.CopyTo(res, 0);
                return res;
            }
        }

        /// <summary>
        /// Returns all modules loaded through the config
        /// </summary>
        public ModuleConfig[] Modules
        {
            get
            {
                if ( moduleTempList == null ) return null;
                ModuleConfig[] temp = new ModuleConfig[moduleTempList.Count];
                moduleTempList.CopyTo(temp, 0);
                return temp;
            }
        }

        /// <summary>
        /// Returns remote console config
        /// </summary>
        public RconConfig RemoteConsole
        {
            get { return this.rconConfig; }
        }

        /// <summary>
        /// Is a subset of a Configuration, encapsulates configuration
        /// for a single server.
        /// </summary>
        public sealed class ServerConfig {
            private string nickname, altnick, ircinfo, username;
            private bool autojoinoninvite;
            private int serverport, sendinhibit, operationTimeout, retryTimeout, socketBufferSize;
            private string name;
            private string quitMessage;
            private string[] hostList;

            /// <summary>
            /// The quit message for this server.
            /// </summary>
            public string QuitMessage {
                get { return quitMessage; }
            }

            /// <summary>
            /// The name of the server.
            /// </summary>
            public string Name {
                get { return name; }
            }

            /// <summary>
            /// List of hosts to try to connect to.
            /// </summary>
            public string[] HostList {
                get { return hostList; }
            }

            /// <summary>
            /// Nickname to use in case primary nickname fails.
            /// </summary>
            public string AltNickname {
                get { return altnick; }
            }

            /// <summary>
            /// IRC Username.
            /// </summary>
            public string Username {
                get { return username; }
            }

            /// <summary>
            /// Nickname :D
            /// </summary>
            public string NickName {
                get { return nickname; }
            }

            /// <summary>
            /// The IRC Info used to connect.
            /// </summary>
            public string IrcInfo {
                get { return ircinfo; }
            }

            /// <summary>
            /// The autojoin on invite flag for said server.
            /// </summary>
            public bool AutoJoinOnInvite {
                get { return autojoinoninvite; }
            }

            /// <summary>
            /// The port of the server.
            /// </summary>
            public int ServerPort {
                get { return serverport; }
            }

            /// <summary>
            /// Time to wait between sends on the socket.
            /// </summary>
            public int SendInhibit {
                get { return sendinhibit; }
            }

            /// <summary>
            /// Time to wait before an IO operation fails.
            /// </summary>
            public int OperationTimeout {
                get { return operationTimeout; }
            }

            /// <summary>
            /// Time to wait between retries in IO operations.
            /// </summary>
            public int RetryTimeout {
                get { return retryTimeout; }
            }

            /// <summary>
            /// The size of the socket buffer.
            /// </summary>
            public int SocketBufferSize {
                get { return socketBufferSize; }
            }

            /// <summary>
            /// Constructor lays down all values and properties ensure read only.
            /// </summary>
            /// <param name="nickName">IRC Server Nickname.</param>
            /// <param name="ircInfo">Userinfo.</param>
            /// <param name="name">Server name (internal use)</param>
            /// <param name="autoJoinOnInvite">Duh</param>
            /// <param name="hostlist">A list of hosts</param>
            /// <param name="port">Duh</param>
            /// <param name="sendInhibit">The time between sends to the server.</param>
            public ServerConfig(string nickName, string altnick, string username, string ircInfo, string name, 
                bool autoJoinOnInvite, string[] hostlist, int port, int sendInhibit, int operationTimeout,
                int retryTimeout, int socketBufferSize, string quitMessage)
            {

                this.name = name;
                this.hostList = hostlist;
                this.nickname = nickName;
                this.altnick = altnick;
                this.username = username;
                this.ircinfo = ircInfo;
                this.serverport = port;
                this.sendinhibit = sendInhibit;
                this.autojoinoninvite = autoJoinOnInvite;
                this.operationTimeout = operationTimeout;
                this.retryTimeout = retryTimeout;
                this.socketBufferSize = socketBufferSize;
                this.quitMessage = quitMessage;
            }
        }

        /// <summary>
        /// Subset of Configuration, 2Q module configuration object.
        /// </summary>
        public sealed class ModuleConfig {
            private string fullName, prettyName;
            private ModuleProxy.CodeType language;
            private string[] fileNames;
            private string[] includes;
            private string[] servers;
            private bool isScript;

            private static string modulePath;
            private static string includePath;
            private static string frameworkPath;
            private static string modulePrefix;

            /// <summary>
            /// Gets or Sets the Module Path (where most modules can be found.)
            /// </summary>
            public static string ModulePath {
                get { return modulePath; }
                set { modulePath = value; }
            }

            /// <summary>
            /// Gets or Sets the Include Path (where most references can be found.)
            /// </summary>
            public static string IncludePath {
                get { return includePath; }
                set { includePath = value; }
            }

            /// <summary>
            /// Gets or Sets the Framework Path (to verify script compilation integrity)
            /// </summary>
            public static string FrameworkPath {
                get { return frameworkPath; }
                set { frameworkPath = value; }
            }

            /// <summary>
            /// Gets or Sets the Module Prefix.
            /// </summary>
            public static string ModulePrefix {
                get { return modulePrefix; }
                set { modulePrefix = value; }
            }

            /// <summary>
            /// Module name
            /// </summary>
            public string FullName
            {
                get { return this.fullName; }
            }

            /// <summary>
            /// The Display name for the Module
            /// </summary>
            public string PrettyName {
                get { return this.prettyName; }
            }

            /// <summary>
            /// Module language (csharp, vb...) 
            /// </summary>
            public ModuleProxy.CodeType Language
            {
                get {
                    if (!this.isScript)
                        throw new FileLoadException("Not a script module.");

                    return this.language;
                }
            }
            /// <summary>
            /// Module location
            /// </summary>
            public string[] FileNames
            {
                get { return this.fileNames; }
            }
            /// <summary>
            /// Needed system files
            /// </summary>
            public string[] Includes
            {
                get
                {
                    if (!this.isScript)
                        throw new FileLoadException("Not a script module.");

                    return this.includes;
                }
            }

            /// <summary>
            /// DLL or script
            /// </summary>
            public bool IsScript
            {
                get { return this.isScript; }
            }

            /// <summary>
            /// Checks if the module should be loaded for the servername.
            /// </summary>
            /// <param name="servername">The name of the server.</param>
            /// <returns>Yes or no.</returns>
            public bool QueryServerList(string servername) {
                if ( servers == null )
                    return true;
                return Array.BinarySearch<string>( servers, servername ) < 0 ? false : true;
            }

            /// <summary>
            /// Main constructor for modules
            /// </summary>
            /// <param name="fullname">Module name</param>
            /// <param name="prettyname">The pretty display name for this module.</param>
            /// <param name="servers">The servers that the module will be active on.</param>
            /// <param name="language">Module language</param>
            /// <param name="filenames">Module file(s) location</param>
            /// <param name="includes">Needed system files</param>
            /// <param name="isScript">Is this a script?</param>
            public ModuleConfig(string fullname, string prettyname, string language, string[] filenames, string[] includes, string[] servers, bool isScript)
            {
                this.fullName = fullname;
                this.prettyName = prettyname;
                this.fileNames = filenames;
                this.servers = servers;
                if (isScript) 
                {
                    this.includes = includes;

                    // Language conversion
                    switch (language.ToLower())
                    {
                        case "c#":
                        case "csharp":
                            this.language = ModuleProxy.CodeType.CSharp;
                            break;
                        case "visual basic":
                        case "vb":
                        case "vbasic":
                            this.language = ModuleProxy.CodeType.VisualBasic;
                            break;
                        default:
                            throw new FileLoadException("Script language invalid.");
                    }
                }
                this.isScript = isScript;
            }
        }

        /// <summary>
        /// Subset  of Configuration, carries details for operating the
        /// Remote Console.
        /// </summary>
        public sealed class RconConfig {
            public string firstAuth, userFile;
            public int operationTimeout, retryTimeout, sendInhibit, socketBufferSize, listenPort;

            public string UserFile
            {
                get { return this.userFile; }
            }
            public string FirstAuth
            {
                get { return this.firstAuth; }
            }
            public int OperationTimeout
            {
                get { return this.operationTimeout; }
            }
            public int RetryTimeout
            {
                get { return this.retryTimeout; }
            }
            public int SendInhibit
            {
                get { return this.sendInhibit; }
            }
            public int SocketBufferSize
            {
                get { return this.socketBufferSize; }
            }
            public int ListenPort
            {
                get { return this.listenPort; }
            }

            /// <summary>
            /// Constructor to initialize values
            /// </summary>
            /// <param name="firstauth">First Authentication to the user system.</param>
            /// <param name="operationtimeout">Operation timeout for sockets.</param>
            /// <param name="retrytimeout">Time before retries in connections.</param>
            /// <param name="sendinhibit">Time between data sends.</param>
            /// <param name="socketbufferSize">The socket buffer size.</param>
            /// <param name="listenport">The port on which remote console listens.</param>
            public RconConfig(string firstauth, int operationtimeout, int retrytimeout, int sendinhibit,
                int socketbufferSize, int listenport)
            {
                this.firstAuth = firstauth;
                this.operationTimeout = operationtimeout;
                this.retryTimeout = retrytimeout;
                this.sendInhibit = sendinhibit;
                this.socketBufferSize = socketbufferSize;
                this.listenPort = listenport;
            }
        }
    }

}
