using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Collections;
using System.IO;
using System.Web;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;

using Project2Q.SDK;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;
using Project2Q.SDK.ModuleSupport;

namespace Webserver {

    public class ServerConfig {
        private string serverpath;
        private string configpath;
        private string logfile;

        public string ServerPath {
            get { return serverpath; }
        }

        public string ConfigPath {
            get { return configpath; }
        }

        public string LogFile {
            get { return logfile; }
        }

        public Hashtable config;
        public Hashtable cgi;
        public Hashtable mime;

        public ServerConfig() {
            serverpath = Environment.CurrentDirectory + "\\modules\\webserver\\";
            logfile = Environment.CurrentDirectory + "\\modules\\webserver\\webserver.log";
            configpath = ServerPath + "cfg\\";
            config = LoadConfig( "config.cfg" );
            cgi = LoadConfig( "cgi.cfg" );
            mime = LoadConfig( "mime.cfg" );
        }

        public Hashtable LoadConfig( string file ) {
            Hashtable ht = new Hashtable();
            using ( StreamReader sr = new StreamReader( ConfigPath + file ) ) {
                string line;
                while ( ( line = sr.ReadLine() ) != null ) {
                    if ( line.Length < 2 )
                        continue;

                    //if it starts with a comment, lets continue.
                    if ( line[0] == '/' && line[1] == '/' )
                        continue;

                    //remove single comments.
                    if ( line.Contains( "//" ) )
                        line = line.Remove( line.IndexOf( "//" ) );

                    line = line.Replace( "\t", " " );

                    if ( !line.Contains( " " ) )
                        continue;

                    while( line.EndsWith( " " ) )
                        line = line.Remove( line.Length - 1, 1 );
                    string[] splits = line.Split( new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries );
                    //TODO: move this.
                    ht.Add( splits[0], line.Substring( line.IndexOf( splits[1] ) ) );
                }
            }
            return ht;
        }
    }

    /// <summary>
    /// This class composes the main part of the webserver.
    /// </summary>
    public class Server : IModuleCreator, IDisposable {

        private Thread requestThread;
        private Thread[] threadPool;
        private readonly uint port = 1337;
        private HttpListener http;
        private FileStream fs;
        private UTF8Encoding utf8;
        private ASCIIEncoding ae;
        private ServerConfig cfg;
        private static readonly int MaxThreads = 16;
        //private byte[] buffer;

        /// <summary>
        /// Used to start threads.
        /// </summary>
        private struct ThreadHttpContextPair {
            public int id;
            public HttpListenerContext hlc;
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        public override void Initialize() {
            cfg = new ServerConfig();
            requestThread = new Thread( new ThreadStart( ListenForRequests ) );
            http = new HttpListener();
            http.Prefixes.Add( "http://+:" + port.ToString() + "/" );

            if ( !Directory.Exists( cfg.ServerPath ) ) {
                Directory.CreateDirectory( cfg.ServerPath );
            }

            fs = new FileStream( cfg.LogFile, FileMode.Append, FileAccess.Write );
            utf8 = new UTF8Encoding();
            ae = new ASCIIEncoding();

            threadPool = new Thread[MaxThreads];
            
            for ( int i = 0; i < MaxThreads; i++ )
                threadPool[i] = null;

            requestThread.Start();
        }

        /// <summary>
        /// Log to the webserver's logfile.
        /// </summary>
        /// <param name="x">The string to log.</param>
        public void Log(string x) {
            lock ( fs ) {
                byte[] b = ae.GetBytes( x + "\r\n" );
                fs.Write( b, 0, x.Length + 2 );
            }
        }

        /// <summary>
        /// Start listening to requests from clients.
        /// </summary>
        public void ListenForRequests() {

        Start:

            try {

                http = new HttpListener();
                http.Prefixes.Add( "http://+:" + port.ToString() + "/" );

                http.Start();

                HttpListenerContext hlc;

                while ( true ) {

                    hlc = http.GetContext(); //Wait for request
                    //Check if we have a thread to handle the request:
                    bool found = false;
                    lock ( threadPool ) {
                        for ( int i = 0; i < MaxThreads; i++ ) {
                            if ( threadPool[i] == null ) {
                                ThreadHttpContextPair thcp = new ThreadHttpContextPair();
                                thcp.hlc = hlc;
                                thcp.id = i;
                                threadPool[i] = new Thread( new ParameterizedThreadStart( this.HandleRequest ) );
                                threadPool[i].Start( thcp );
                                found = true;
                                break;
                            }
                        }
                        if ( !found ) {
                            hlc.Response.StatusCode = 500;
                            hlc.Response.Close();
                        }
                    }

                }
            }
            catch ( ThreadInterruptedException ) {
                for ( int i = 0; i < MaxThreads; i++ ) {
                    if ( threadPool[i] != null ) {
                        threadPool[i].Abort();
                        threadPool[i].Join();
                    }
                }
            }
            catch ( ThreadAbortException ) {
                for ( int i = 0; i < MaxThreads; i++ ) {
                    if ( threadPool[i] != null ) {
                        threadPool[i].Abort();
                        threadPool[i].Join();
                    }
                }
            }
            catch ( HttpListenerException ) {
                try {
                    http.Stop();
                    http.Abort();
                }
                catch ( ObjectDisposedException ) {
                    //Do nothing. We just had to make sure it was dead. This bug comes back every once in a while. Dunno why.
                }
                goto Start;
            }
        }

       

        public void ShowError(ref HttpListenerResponse hlrs, int error) {
            hlrs.StatusCode = error;
            switch (error) {
                case 403: hlrs.StatusDescription = "File Not found.";   break;
                case 404: hlrs.StatusDescription = "Forbidden.";        break;
                default:  hlrs.StatusDescription = "Unknown!.";         break;
            }

            try {
                using ( StreamReader fs = new StreamReader( cfg.ConfigPath + error + ".cfg" ) ) {
                    string content = fs.ReadToEnd();
                    hlrs.OutputStream.Write( utf8.GetBytes( content ), 0, content.Length );
                }
            }
            catch ( FileNotFoundException ) {
                string s = "IRONY! Error grabbing error page ( " + error + " )";
                hlrs.OutputStream.Write( utf8.GetBytes( s ), 0, s.Length );
            }
            hlrs.Close();
        }

        public void HandleRequest(object o) {

            ThreadHttpContextPair thcp = (ThreadHttpContextPair)o;
            HttpListenerContext hlc = thcp.hlc;
            HttpListenerResponse hlrs = hlc.Response;
            HttpListenerRequest hlrq = hlc.Request;
            FileStream fs = null;
            Process cgi = null;

            try {

                string r = hlrq.RawUrl;

                if ( r.Split( '?' )[0].Equals( "/" ) )
                    r = ( cfg.config["IndexPage"] != null ) ? "/" + (string)cfg.config["IndexPage"] : "/index.html";
                else
                    r = hlrq.Url.AbsolutePath;

                string path = Path.Combine( Path.GetFullPath( cfg.ServerPath ), r.Substring( 1 ) );

                Log( "Requested: " + path );

                NameValueCollection nvc = hlrq.QueryString;
                foreach ( string k in nvc.AllKeys ) {
                    Log( " " + k + " : " + nvc[k] );
                }

                //We can't send a file we don't have!
                if ( !File.Exists( path ) ) {
                    ShowError(ref hlrs, 404);
                    return;
                }

                //We are using the log!
                if (path.Equals(cfg.LogFile)) {
                    ShowError(ref hlrs, 403);
                    return;
                }

                fs = new FileStream( path, FileMode.Open, FileAccess.Read );

                bool cgisuccess = false;
                string ext = Path.GetExtension( path ).Substring( 1 );
                if ( cfg.cgi[ext] != null ) {
                    //load CGI crap here.
                    cgi = new Process();
                    cgi.StartInfo.CreateNoWindow = true;
                    cgi.StartInfo.UseShellExecute = false;
                    cgi.StartInfo.FileName = (string)cfg.cgi[ext];
                    cgi.StartInfo.Verb = null;
                    cgi.StartInfo.Arguments = null;
                    cgi.StartInfo.RedirectStandardInput = true;
                    cgi.StartInfo.RedirectStandardOutput = true;
                    cgi.StartInfo.RedirectStandardError = true;
                    cgi.StartInfo.WorkingDirectory = Path.GetDirectoryName( path );

                    // Environment Variables
                    cgi.StartInfo.EnvironmentVariables.Add( "REDIRECT_STATUS", "200" );
                    cgi.StartInfo.EnvironmentVariables.Add( "REDIRECT_URL", "/" + Path.GetFileName( path ) );
                    cgi.StartInfo.EnvironmentVariables.Add( "GATEWAY_INTERFACE", "CGI/1.1" );

                    cgi.StartInfo.EnvironmentVariables.Add( "REQUEST_METHOD", hlrq.HttpMethod );
                    cgi.StartInfo.EnvironmentVariables.Add( "REQUEST_URI", hlrq.RawUrl );
                    cgi.StartInfo.EnvironmentVariables.Add( "REQUEST_URL", "/" + Path.GetFileName( path ) );
                    cgi.StartInfo.EnvironmentVariables.Add( "SCRIPT_URL", "/" + Path.GetFileName( path ) );
                    cgi.StartInfo.EnvironmentVariables.Add( "SCRIPT_FILENAME", (string)cfg.cgi[ext] );
                    if ( hlrq.Url.Query != null && hlrq.Url.Query != string.Empty )
                        cgi.StartInfo.EnvironmentVariables.Add( "QUERY_STRING", hlrq.Url.Query.Remove( 0, 1 ) );
                    else
                        cgi.StartInfo.EnvironmentVariables.Add( "QUERY_STRING", string.Empty );

                    cgi.StartInfo.EnvironmentVariables.Add( "PATH_INFO", Path.GetFileName( path ) );
                    cgi.StartInfo.EnvironmentVariables.Add( "PATH_TRANSLATED", path );

                    cgi.StartInfo.EnvironmentVariables.Add( "DOCUMENT_ROOT", cfg.ServerPath );
                    
                    cgi.StartInfo.EnvironmentVariables.Add( "PWD", cfg.ServerPath );

                    cgi.StartInfo.EnvironmentVariables.Add( "SERVER_NAME", "localhost" );
                    cgi.StartInfo.EnvironmentVariables.Add( "SERVER_PORT", port.ToString() );
                    cgi.StartInfo.EnvironmentVariables.Add( "SERVER_PROTOCOL", "HTTP/1.1" );
                    cgi.StartInfo.EnvironmentVariables.Add( "SERVER_SOFTWARE", "2Q WebServer Module 1.0" );

                    cgi.StartInfo.EnvironmentVariables.Add( "REMOTE_ADDR", hlrq.RemoteEndPoint.Address.ToString() );
                    if ( hlrq.UserHostName != null && hlrq.UserHostName != string.Empty )
                        cgi.StartInfo.EnvironmentVariables.Add( "REMOTE_HOST", hlrq.UserHostName );
                    cgi.StartInfo.EnvironmentVariables.Add( "REMOTE_PORT", hlrq.RemoteEndPoint.Port.ToString() );

                    StringBuilder sb = new StringBuilder();

                    if ( hlrq.Cookies.Count > 0 ) {
                        foreach ( Cookie c in hlrq.Cookies )
                            sb.Append( c.Name + "=" + c.Value + "; " );
                        cgi.StartInfo.EnvironmentVariables.Add( "HTTP_COOKIE", sb.ToString() );
                        sb = new StringBuilder();
                    }

                    if ( hlrq.AcceptTypes.Length > 0 ) {
                        foreach ( string at in hlrq.AcceptTypes )
                            sb.Append( at + ";" );
                        sb.Remove( sb.Length - 1, 1 );
                        cgi.StartInfo.EnvironmentVariables.Add( "HTTP_ACCEPT", sb.ToString() );
                        sb = new StringBuilder();
                    }
                    if ( hlrq.UserLanguages.Length > 0 ) {
                        foreach ( string at in hlrq.UserLanguages )
                            sb.Append( at + ";" );
                        sb.Remove( sb.Length - 1, 1 );
                        cgi.StartInfo.EnvironmentVariables.Add( "HTTP_ACCEPT_LANGUAGE", sb.ToString() );
                        sb = new StringBuilder();
                    }

                    cgi.StartInfo.EnvironmentVariables.Add( "HTTP_REQUEST", hlrq.HttpMethod + " " + hlrq.Url.Query + " " + "HTTP/1.1" );
                    cgi.StartInfo.EnvironmentVariables.Add( "HTTP_ACCEPT_CHARSET", "ISO-8859-1,utf-8;q=0.7,*;q=0.7" );
                    cgi.StartInfo.EnvironmentVariables.Add( "HTTP_ACCEPT_ENCODING", "gzip,deflate" );
                    cgi.StartInfo.EnvironmentVariables.Add( "HTTP_HOST", hlrq.LocalEndPoint.Address.ToString() );
                    cgi.StartInfo.EnvironmentVariables.Add( "HTTP_CONNECTION", "keep-alive" );
                    cgi.StartInfo.EnvironmentVariables.Add( "HTTP_USER_AGENT", hlrq.UserAgent );

                    

                    StringBuilder sb2 = new StringBuilder();
                    StringBuilder sb3 = new StringBuilder();

                    //Server Number
                    //Server Names
                    //Module Number
                    //Module Names
                    //Module Prettynames
                    //Modules Loaded?
                    //Module Parses
                    //Module Events

                    Configuration config = (Configuration)
                    mp.RequestVariable( Request.Configuration, -1 );

                    cgi.StartInfo.EnvironmentVariables.Add( "QQ_ENV_SNUM", config.Servers.Length.ToString() );

                    //THIS WILL ADD:
                    //QQ_ENV_S#_CHANNELS
                    //QQ_ENV_S#_C#_NAMES

                    for ( int i = 0; i < config.Servers.Length; i++ ) {
                        sb.Append( config.Servers[i].Name + ";" );

                        ChannelCollection cc = (ChannelCollection)mp.RequestVariable( Request.ChannelCollection, i );
                        int cn = 0;
                        foreach ( Channel c in cc ) {
                            sb2.Append( c.Name + ";" );

                            foreach ( ChannelUser cu in c ) {
                                if ( cu.InternalUser == null ) continue;
                                else if ( cu.UserFlag != null )
                                    sb3.Append( cu.UserFlag.Value.ToString() + cu.InternalUser.Nickname + ";" );
                                else
                                    sb3.Append( cu.InternalUser.Nickname + ";" );
                            }

                            if ( sb3.Length > 0 ) {
                                sb3.Remove( sb3.Length - 1, 1 );
                                cgi.StartInfo.EnvironmentVariables.Add( "QQ_ENV_S" + i.ToString() + "_C" + cn.ToString() + "_NAMES", sb3.ToString() );
                                sb3 = new StringBuilder();
                            }
                            cn++;
                        }

                        if ( sb2.Length > 0 ) {
                            sb2.Remove( sb2.Length - 1, 1 );
                            cgi.StartInfo.EnvironmentVariables.Add( "QQ_ENV_S" + i.ToString() + "_CHANNELS", sb2.ToString() );
                            sb2 = new StringBuilder();
                        }

                    }

                    foreach ( Configuration.ServerConfig sc in config.Servers ) {
                        sb.Append( sc.Name + ";" );
                    }

                    if ( sb.Length > 0 ) {
                        sb.Remove( sb.Length - 1, 1 );
                        cgi.StartInfo.EnvironmentVariables.Add( "QQ_ENV_SNAMES", sb.ToString() );
                    }

                    IModule[] im = (IModule[])mp.RequestVariable( Request.ModuleList, -1 );

                    sb = new StringBuilder();

                    int nmodules = 0;

                    if ( im != null )
                        foreach ( IModule i in im ) {
                            if ( i == null )
                                continue;
                            nmodules++;
                            sb.Append( i.ModuleConfig.FullName + ";" );
                            sb2.Append( i.IsLoaded + ";" );
                            sb3.Append( i.ModuleConfig.PrettyName + ";" );
                        }

                    cgi.StartInfo.EnvironmentVariables.Add( "QQ_ENV_MNUM", nmodules.ToString() );

                    if ( sb.Length > 0 ) {
                        sb.Remove( sb.Length - 1, 1 );
                        cgi.StartInfo.EnvironmentVariables.Add( "QQ_ENV_MNAMES", sb.ToString() );
                    }
                    if ( sb2.Length > 0 ) {
                        sb2.Remove( sb2.Length - 1, 1 );
                        cgi.StartInfo.EnvironmentVariables.Add( "QQ_ENV_MLOADED", sb2.ToString() );
                    }
                    if ( sb3.Length > 0 ) {
                        sb3.Remove( sb3.Length - 1, 1 );
                        cgi.StartInfo.EnvironmentVariables.Add( "QQ_ENV_MPRETTYNAMES", sb3.ToString() );
                    }

                    try {
                        cgi.Start();
                        cgisuccess = true;
                    }
                    catch ( Win32Exception ) {
                        cgisuccess = false;
                        cgi.Close();
                    }

                    if ( cgisuccess ) {

                        StreamWriter sw = cgi.StandardInput;
                        StreamReader sr = cgi.StandardOutput;
                        StreamReader serr = cgi.StandardError;

                        #region Input/Output From Process

                        //Done writing to standard in.
                        sw.Close();
                        string output = sr.ReadToEnd();
                        
                        int extra_headers_len = 0;

                            string header = output.Split( new string[] { "\r\n\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries )[0].TrimEnd( new char[] { '\r', '\n' } );
                            string[] headers = header.Split( new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );
                            
                            for ( int i = 0; i < headers.Length; i++ ) {
                                string k = headers[i].Remove( headers[i].IndexOf( ' ' ) - 1 );
                                string v = headers[i].Remove( 0, headers[i].IndexOf( ' ' ) + 1 );
                                extra_headers_len += headers[i].Length + 2;
                                if ( hlrs.Headers[k] == null )
                                    hlrs.AddHeader( k, v );
                            }
                            extra_headers_len += 4;
                            output = output.Remove( 0, extra_headers_len );

                        hlrs.OutputStream.Write( utf8.GetBytes( output ), 0, output.Length );

                        string error = serr.ReadToEnd();
                        hlrs.OutputStream.Write( utf8.GetBytes( error ), 0, error.Length );

                        cgi.WaitForExit();

                        //Done reading from standard out.
                        sr.Close();
                        serr.Close();
                        cgi.Close();

                        #endregion

                    }
                }

                if ( cgisuccess == false ) {
                    //Spam the file to the client.
                    bool cached = false;
                    if ( cfg.config["Caching"] != null ) {
                        if ( (string)cfg.config["Caching"] == "1" && cfg.mime[ext] != null ) {
                            if ( hlrq.Headers["If-Modified-Since"] == File.GetLastWriteTime( path ).ToString() ) {
                                hlrs.StatusCode = 304;
                                hlrs.StatusDescription = "Not Modified";
                                hlrs.Headers.Add( "Last-Modified", File.GetLastWriteTime( path ).ToString() );
                                cached = true;
                            }
                            else {
                                hlrs.StatusCode = 200;
                                hlrs.StatusDescription = "OK";
                                hlrs.Headers.Add( "Last-Modified", File.GetLastWriteTime( path ).ToString() );
                                hlrs.Headers.Add( "Content-Type", (string) cfg.mime[ext] );
                            }
                        }
                    }

                    //TODO: fix this: hlrs.Headers.Set("Server", "P2Q Webserver") ~fish.
                    if (!cached) // No need to send the file if it's cached!
                    {
                        byte[] buffer = new byte[fs.Length];
                        while (true)
                        {
                            int n = fs.Read( buffer, 0, (int)fs.Length );
                            if (n == 0) break;
                            hlrs.OutputStream.Write( buffer, 0, (int)fs.Length );
                        }
                    }
                }

                hlrs.Close();
                fs.Close();
            }

            #region catch blocks
            catch ( ThreadInterruptedException ) {
                if ( fs != null ) fs.Close();
                hlrs.Abort();
                cgi.Close();
            }
            catch ( ThreadAbortException ) {
                if ( fs != null ) fs.Close();
                hlrs.Abort();
                cgi.Close();
            }
            catch ( HttpListenerException ) {
                if ( fs != null ) fs.Close();
                hlrs.Abort();
                cgi.Close();
            }
            #endregion

            lock ( threadPool ) {
                threadPool[thcp.id] = null; //Release our thread spot.
            }

        }
        public void resizebytes( ref byte[] ba, int i ) {
            //byte[] oo = new byte[ba.Length - i];
            for ( int x = 0; x < ba.Length - i; x++ ) {
                ba[x] = ba[x + i];
            }
            //return oo;
        }

        #region IDisposable Members

        public void Dispose() {
            http.Stop();
            http.Abort();
            requestThread.Abort();
            requestThread.Join();
            fs.Close();
        }

        #endregion
    }

}