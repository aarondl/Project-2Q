using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Project2Q.SDK.ChannelSystem;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK;

namespace Project2Q.Core {

    /// <summary>
    /// Provides methods and delegates for dealing
    /// with IRC Protocol.
    /// </summary>
    static class IRCProtocol {

        #region Static Constructor + Statics

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static IRCProtocol() {
            ascii = new ASCIIEncoding();
        }

        public static ASCIIEncoding ascii;

        /// <summary>
        /// Returns a static ascii encoding.
        /// </summary>
        public static ASCIIEncoding Ascii {
            get { return ascii; }
        }

        #endregion

        #region Constants

        public static readonly string CRLF = "\r\n";

        #endregion

        #region Tokenization Functions

        /// <summary>
        /// Trims a byte array to the first 0 byte.
        /// </summary>
        /// <param name="data">The data in.</param>
        /// <returns>The new shortened array.</returns>
        public static byte[] TrimNull(byte[] data) {
            int i = 0;
            for ( i = 0; i < data.Length; i++ )
                if ( data[i] == 0 )
                    break;

            if ( i == data.Length )
                return data;
            byte[] ret = new byte[i];
            for ( int j = 0; j < i; j++ )
                ret[j] = data[j];
            return ret;
        }

        /// <summary>
        /// Trims a byte array to the first crlf.
        /// </summary>
        /// <param name="data">The data in.</param>
        /// <returns>The new shortened array.</returns>
        public static byte[] TrimToCrlf(byte[] data) {
            int i = 0;
            for ( i = 1; i < data.Length - 1; i++ )
                if ( data[i - 1] == 13 && data[i] == 10 )
                    break;

            if ( i == data.Length )
                return data;
            byte[] ret = new byte[i + 1];
            for ( int j = 0; j <= i; j++ )
                ret[j] = data[j];
            return ret;
        }

        /// <summary>
        /// Trims a byte array of it's crlf and any trailing 0 bytes.
        /// </summary>
        /// <param name="data">The data to trim.</param>
        /// <returns>The trimmed array.</returns>
        public static byte[] TrimCrlf(byte[] data) {
            int i = 0;
            for ( i = 0; i < data.Length - 2; i++ )
                if ( data[i + 1] == 13 && data[i + 2] == 10 )
                    break;

            byte[] ret = new byte[i + 1];
            for ( int j = 0; j <= i; j++ )
                ret[j] = data[j];
            return ret;
        }

        /// <summary>
        /// Trims a string of it's crlf.
        /// </summary>
        /// <param name="s">The string to trim.</param>
        /// <returns>The trimmed string.</returns>
        public static string TrimCrlf(string s) {
            if ( s.EndsWith( CRLF ) )
                return s.Remove( s.Length - 2, 2 );
            else return s;
        }

        #endregion

        #region IRC Handling

        /// <summary>
        /// This method parses all raw data for each packet of data that
        /// a Server object might receive. It then uses the events defined
        /// in IRCEvents and fires them through the appropriate server object.
        /// </summary>
        /// <param name="s">The Server which the data was received on.</param>
        /// <param name="raw">The raw text from the socket.</param>
        public static void Parse(Server s, string raw) {
            //TODO: IMPLEMENT THE REST :D

            int spacestring = 0;
            int lastchar = 0;
            int firstchar = 0;

            //Remove excessive spaces from everything.
            StringBuilder sb = new StringBuilder();

            for ( int i = 0; i < raw.Length; i++ ) {
                if ( raw[i] == ' ' ) {
                    if ( spacestring++ == 0 )
                        lastchar = i;
                }
                else if ( spacestring > 1 ) {
                    sb.Append( raw, firstchar, lastchar - firstchar + 1 );
                    spacestring = 0;
                    firstchar = i;
                    lastchar = i;
                }
                else {
                    spacestring = 0;
                    lastchar = i;
                }
            }

            if ( sb.Length > 0 || ( lastchar != firstchar && firstchar != 0 && lastchar != raw.Length - 1 ) ) {
                sb.Append( raw, firstchar, lastchar - firstchar + 1 );
            }
            if ( sb.Length > 0 )
                raw = sb.ToString();

            //Make sure this excessive space is handled with care. (For all instances where text is preceeded by a space)
            raw = raw.Replace( ": ", ":" );

            //Replies to ping handler
            if ( raw.StartsWith( "PING :" ) ) {
                string[] parsed = raw.Split( ':' );
                s.QueueData( s.EventObject.OnPing( s.ServerID, parsed[1] ) );
            }

            if ( raw.StartsWith( "ERROR :" ) ) {
                Console.WriteLine( "[Error: whatsit]" );
            }

            //All that work above for this?
            string[] spaceSplit = raw.Split( ' ' );

            if ( spaceSplit.Length <= 1 ) return; //Nothing below here takes only one spaceSplit arg.
            switch ( spaceSplit[1] ) {
                case "001": {
                        string welcome = raw.Substring( spaceSplit[0].Length + spaceSplit[1].Length + 4 + spaceSplit[2].Length );
                        s.QueueData( s.EventObject.OnWelcome( s.ServerID, welcome ) );
                    }
                    break;
                case "352": {
                        //spaceSplit[5] = spaceSplit[5].Substring( 1 ); //Get rid of the : on the first name.
                        //
                        //                0                  1   2         3    4               5                        6          7  8  9   10
                        //":TechConnect.NL.EU.GameSurge.net 352 P2Q #phishcave ~2q d75-155-184-58.bchsia.telus.net *.GameSurge.net P2Q H :0 2Q Beta"
                        IRCHost ir = new IRCHost(spaceSplit[7], spaceSplit[4], spaceSplit[5]); //fix on debug.
                        Channel c = UpdateChanDB(s, spaceSplit[3]);
                        UpdateUserDB(s, ir);
                        //UpdateBothDB(s, spaceSplit[3], spaceSplit[7]);

                        //This should blank add all the users

                        if (spaceSplit[8].Contains("@"))
                            UpdateBothDB(s, spaceSplit[3], spaceSplit[7], '@');  //NO Simple. hah
                        else if (spaceSplit[8].Contains("+"))
                            UpdateBothDB(s, spaceSplit[3], spaceSplit[7], '+');
                        else
                            UpdateBothDB(s, spaceSplit[3], spaceSplit[7], null);

                        //Invoke the event.
                        s.QueueData(s.EventObject.OnNames(s.ServerID, c));
                    }
                    break;
                case "353": {
                        spaceSplit[5] = spaceSplit[5].Substring( 1 ); //Get rid of the : on the first name.

                        //if ( spaceSplit[2] == s.Config.NickName ) //Do we need to parse ourself? ~fish
                        //    break;

                        Channel c = UpdateChanDB( s, spaceSplit[4] );

                        //This should blank add all the users
                        for ( int i = 5; i < spaceSplit.Length; i++ ) {
                            if ( spaceSplit[i][0] == '@' || spaceSplit[i][0] == '+' )
                                UpdateBothDB( s, spaceSplit[4], spaceSplit[i].Substring( 1 ), spaceSplit[i][0] );
                            else
                                UpdateBothDB( s, spaceSplit[4], spaceSplit[i], null );
                        }

                        //Invoke the event.
                        s.QueueData( s.EventObject.OnNames( s.ServerID, c ) );
                    }
                    break;
                case "433": {
                        //:SERVER 433 * THENICK :Nickname is already in use.
                        s.QueueData( s.EventObject.OnErr_NickNameInUse( s.ServerID, spaceSplit[3] ) );
                    }
                    break;
                case "MODE": {

                        //SERVERMODE:
                        //:USERHOST MODE RECIPIENT :MODESTRING
                        if ( spaceSplit[3][0] == ':' ) {
                            IRCHost irch = new IRCHost( spaceSplit[0].Substring( 1 ) );
                            User u = new User( irch );
                            s.QueueData( s.EventObject.OnServerModeMessage( s.ServerID, u, spaceSplit[2], spaceSplit[3].TrimStart( ':' ) ) );
                        }
                        else {
                            //:USERMODE //TODO: HANDLE THIS
                            //:USERHOST MODE MODESTRING MODEAFFECTUSER MODEAFFECTUSER MODEAFFECTUSER
                        }

                    }
                    break;
                case "JOIN": {
                        IRCHost irch = new IRCHost( spaceSplit[0].Substring( 1 ) );
                        Channel c = UpdateChanDB( s, spaceSplit[2] );
                        if ( irch.Nick.Equals( s.CurrentNickName ) ) {
                            //We joined the channel.
                            s.QueueData( s.EventObject.OnBotJoin( s.ServerID, c ) );
                        }
                        else {
                            //Someone joined the channel.
                            //Update the user first.
                            UpdateUserDB( s, irch );
                            //Now update the channel.
                            ChannelUser cu = UpdateBothDB( s, spaceSplit[2], irch.Nick );
                            s.QueueData( s.EventObject.OnJoin( s.ServerID, c, cu ) );
                        }
                    }
                    break;
                case "PRIVMSG": {
                        string privmsgtxt = raw.Substring(
                        spaceSplit[0].Length + spaceSplit[1].Length + spaceSplit[2].Length + 4 );
                        //:USERHOST PRIVMSG RECIPIENT :TEXT
                        //  length    length  length + 4 (3 spaces & a :)

                        IRCHost irch = new IRCHost( spaceSplit[0].Substring( 1 ) );
                        User u = UpdateUserDB( s, irch );

                        //Replies to Channel/User CTCP Messages.
                        if ( spaceSplit.Length > 4 && spaceSplit[3].Length > 2 && 
                            spaceSplit[3][1] == (char)1 && raw.EndsWith( ( (char)1 ).ToString() ) ) {

                            if ( spaceSplit[2].Length >= 1 && ( spaceSplit[2][0] == '#' || spaceSplit[2][0] == '&' ) ) {
                                Channel c = UpdateChanDB( s, spaceSplit[2] );
                                ChannelUser cu = UpdateBothDB( s, spaceSplit[2], irch.Nick );
                                s.QueueData( s.EventObject.OnChannelCTCPMessage( s.ServerID, c, cu, privmsgtxt.Trim( (char)1 ) ) );
                            }
                            else
                                s.QueueData( s.EventObject.OnUserCTCPMessage( s.ServerID, spaceSplit[2], u, privmsgtxt.Trim( (char)1 ) ) );

                        }

                        //TODO: Evaluate if these can be moved up above the CTCP messages once it's converted.

                        else if ( spaceSplit[2][0] == '#' || spaceSplit[2][0] == '&' ) {
                            Channel c = UpdateChanDB( s, spaceSplit[2] );
                            ChannelUser cu = UpdateBothDB( s, spaceSplit[2], irch.Nick );
                            InvokeParsers( s, c, u, cu, privmsgtxt, IRCEvents.ParseTypes.ChannelMessage );
                            s.QueueData( s.EventObject.OnChannelMessage( s.ServerID, c, cu, privmsgtxt ) );
                        }
                        else {
                            InvokeParsers( s, null, u, null, privmsgtxt, IRCEvents.ParseTypes.PrivateMessage );
                            s.QueueData( s.EventObject.OnUserMessage( s.ServerID, spaceSplit[2], u, privmsgtxt ) );
                        }
                    }
                    break;
                case "NOTICE": {
                        // user notice
                        // :HOST NOTICE ME :MSG
                        // channel notice
                        // :HOST NOTICE CHANNEL :MSG
                        IRCHost irch;
                        string text = raw.Substring(
                        spaceSplit[0].Length + spaceSplit[1].Length + spaceSplit[2].Length + 4 );

                        try { irch = new IRCHost( spaceSplit[0].Substring( 1 ) ); }
                        catch ( FormatException ) { // looks like this is a server notice.
                            InvokeParsers( s, null, null, null, text, IRCEvents.ParseTypes.ServerNotice );
                            s.QueueData( s.EventObject.OnServerNotice( s.ServerID, text ) );
                            break;
                        }

                        User u = UpdateUserDB( s, irch );
                        if ( spaceSplit[2][0] == '#' || spaceSplit[2][0] == '&' ) {
                            Channel c = UpdateChanDB( s, spaceSplit[2] );
                            InvokeParsers( s, c, u, null, text, IRCEvents.ParseTypes.ChannelNotice );
                            s.QueueData( s.EventObject.OnChannelNotice( s.ServerID, c, u, text ) );
                        }
                        else {
                            InvokeParsers( s, null, u, null, text, IRCEvents.ParseTypes.PrivateNotice );
                            s.QueueData( s.EventObject.OnPrivateNotice( s.ServerID, u, text ) );
                        }
                    }
                    break;
                case "QUIT": {
                        //:USERHOST QUIT :MESSAGE
                        IRCHost irch = new IRCHost( spaceSplit[0].Substring( 1 ) );
                        User u = UpdateUserDB( s, irch );
                        //Premature call of event.
                        s.QueueData( s.EventObject.OnQuit( s.ServerID, u, raw.Substring(
                            spaceSplit[0].Length + spaceSplit[1].Length + 3 ) ) );
                        foreach ( Channel c in s.ChannelDatabase )
                            c.RemoveUser( u.Nickname );
                        s.UserDatabase.RemoveUser( u );
                    }
                    break;
                case "PART": {
                        //:USERHOST PART CHANNELNAME :MESSAGE
                        IRCHost irch = new IRCHost( spaceSplit[0].Substring( 1 ) );
                        if ( irch.Nick.Equals( s.CurrentNickName ) ) {
                            s.QueueData( s.EventObject.OnBotPart( s.ServerID, s.ChannelDatabase[spaceSplit[2]], spaceSplit.Length > 3 ? spaceSplit[3] : null ) );
                            s.ChannelDatabase.RemoveChannel( spaceSplit[2] );
                        }
                        else {
                            User u = UpdateUserDB( s, irch );
                            ChannelUser cu = UpdateBothDB( s, spaceSplit[2], u.Nickname );
                            //Premature call of event.
                            s.QueueData( s.EventObject.OnPart( s.ServerID, cu, s.ChannelDatabase[spaceSplit[2]], spaceSplit.Length > 3 ? spaceSplit[3] : null ) );
                            s.ChannelDatabase[spaceSplit[2]].RemoveUser( u );
                            bool stillOnAChannel = false;
                            foreach ( Channel c in s.ChannelDatabase )
                                if ( c.HasNick( u.Nickname ) )
                                    stillOnAChannel = true;
                            if ( stillOnAChannel == false )
                                s.UserDatabase.RemoveUser( u );
                        }
                    }
                    break;
                case "NICK": {
                        //:USERHOST NICK :NEWNICK
                        IRCHost irch = new IRCHost( spaceSplit[0].Substring( 1 ) );

                        string newNick = spaceSplit[2].Substring( 1 );
                        string oldNick = irch.Nick;

                        if ( oldNick == s.CurrentNickName ) { //We need to update.
                            s.CurrentNickName = newNick;
                            s.QueueData( s.EventObject.OnBotNickName( s.ServerID, newNick, oldNick ) );
                        }
                        else {
                            User oldUser = null;
                            try {
                                oldUser = s.UserDatabase[irch.Nick];//Get the old user. Remove him from the DB.
                            }
                            catch ( KeyNotFoundException ) {
                                //see below.
                            }

                            if ( oldUser == null ) {
                                //This means he wasn't in our user database.. WEIRD. Add him, add him to the db, but still
                                //broadcast it as a nick message.
                                oldUser = new User( irch );
                                s.UserDatabase.AddUser( oldUser );
                                s.UserDatabase.Authenticate( oldUser );
                            }
                            else {
                                s.UserDatabase.RemoveUser( oldUser ); //Removed him.
                                oldUser.CurrentHost.Nick = newNick; //Change his nick in the local User variable.
                                s.UserDatabase.AddUser( oldUser ); //Re-Add him to the database.
                                s.UserDatabase.Authenticate( oldUser );
                            }

                            ChannelUser cu = null;
                            foreach ( Channel c in s.ChannelDatabase ) { //Replace nick in all channels that had the old nick identifier.
                                try {
                                    cu = c[oldNick]; //Preserve his channel settings by retrieving this piece of info.
                                }
                                catch ( KeyNotFoundException ) { continue; }
                                cu.InternalUser = oldUser; //Set the new internal user. (nick matches might change authentication)
                                c.ReplaceUser( oldNick, cu ); //Replace the user in the database, remove oldNick, add the channeluser.
                            }

                            s.QueueData( s.EventObject.OnNickName( s.ServerID, oldUser, oldNick ) );
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Parse Invokers and helpers

        /// <summary>
        /// Invokes the Parsers.
        /// </summary>
        /// <param name="s">The server object.</param>
        /// <param name="c">The channel triggered on.</param>
        /// <param name="u">The user triggered from.</param>
        /// <param name="cu">The channeluser triggered from.</param>
        /// <param name="text">The offending text.</param>
        /// <param name="pt">The Parse Types to invoke on.</param>
        private static void InvokeParsers(Server s, Channel c, User u, ChannelUser cu, string text, IRCEvents.ParseTypes pt) {
            bool pcount = s.EventObject.Parses.Count > 0;
            bool wcount = s.EventObject.WildcardParses.Count > 0;
            if ( pcount || wcount ) {
                IRCEvents.ParseReturns pr = CreateParseReturns( s.ServerID, c, u, cu, text );
                if ( pcount )
                    s.QueueData( ParseInvocation( pr.Text, s.EventObject, pr, pt, false ) );
                if ( wcount )
                    s.QueueData( ParseInvocation( pr.Text, s.EventObject, pr, pt, true ) );
            }

        }

        /// <summary>
        /// Creates a parse returns struct.
        /// </summary>
        /// <param name="serverId">The server id.</param>
        /// <param name="c">The channel triggered on.</param>
        /// <param name="u">The user triggered from.</param>
        /// <param name="cu">The channeluser triggered from.</param>
        /// <param name="text">The offending text.</param>
        /// <returns>A new parse returns struct.</returns>
        private static IRCEvents.ParseReturns CreateParseReturns(int serverId, Channel c, User u, ChannelUser cu, string text) {
            IRCEvents.ParseReturns pr = new IRCEvents.ParseReturns();
            pr.Channel = c;
            pr.User = u;
            pr.ChannelUser = cu;
            pr.ServerID = serverId;
            pr.Text = text;
            return pr;
        }

        /// <summary>
        /// Invokes the parses (if match exists) with a specific ParseReturns struct.
        /// </summary>
        /// <param name="toParse">The string to parse.</param>
        /// <param name="irce">The IRCEvents object to retrieve parses from.</param>
        /// <param name="pr">The parse returns struct.</param>
        /// <param name="pt">The parse type of the firing event.</param>
        /// <param name="doWildcards">True if we are doing the wildcard parses. False if not.</param>
        /// <returns>The raw data to queue into the server.</returns>
        private static string[][] ParseInvocation(string toParse, IRCEvents irce, IRCEvents.ParseReturns pr,
            IRCEvents.ParseTypes pt, bool doWildCards) {

            List<IRCEvents.Parse> parses = doWildCards ? irce.WildcardParses : irce.Parses;
            int n = parses.Count;
            string[][] returns = new string[n][];

            string[] splits = doWildCards ? null : toParse.Split( ' ' );

            /*if ( splits.Length > 1 ) TODO: Evaluate if this is a good feature.
                pr.Text = toParse.Substring( toParse.IndexOf( ' ' ) + 1 );*/

            //TODO: You set it up so that parses could be bsearched then you linearly search them.

            for ( int i = 0; i < n; i++ ) {

                if ( ( pt & parses[i].ParseTypes ) == 0 )
                    continue;

                bool continueWithCall = true; //True right now.

                if ( doWildCards )
                    continueWithCall = IRCHost.WildcardCompare( parses[0].ParseString, toParse ) == 0;
                else
                    continueWithCall = splits[0].Equals( parses[i].ParseString );

                if ( !continueWithCall ) //True if the parse went through correctly.
                    continue;

                PrivelegeContainer pc = pr.User.UserAttributes != null ? pr.User.UserAttributes.Privegeles : null;

                if ( pc == null || !pc.HasPrivelege( Priveleges.SuperUser ) ) {

                    object[] privreq = parses[i].Function.Method.GetCustomAttributes( false );

                    for ( int j = 0; continueWithCall && j < privreq.Length; j++ ) {
                        if ( privreq[j].GetType().Equals( typeof( PrivelegeRequiredAttribute ) ) ) {
                            PrivelegeRequiredAttribute pra = (PrivelegeRequiredAttribute)privreq[j];
                            continueWithCall = pc != null && pc.HasPrivelege( pra.Required );
                        }
                        if ( continueWithCall && privreq[j].GetType().Equals( typeof( UserLevelRequiredAttribute ) ) ) {
                            UserLevelRequiredAttribute ura = (UserLevelRequiredAttribute)privreq[j];
                            continueWithCall = pc != null && pc.NumericalLevel >= ura.Required;
                        }
                    }
                }

                if ( !continueWithCall )
                    continue;

                parses[i].FieldInfo.SetValue( parses[i].InstanceOf, pr );
                parses[i].Function.Invoke();
                FieldInfo fi = parses[i].InstanceOf.GetType().GetField( "returns",
                    BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
                returns[i] = (string[])fi.GetValue( parses[i].InstanceOf );
            }

            return returns;

        }

        #endregion

        #region Database Checkers and Updaters

        /// <summary>
        /// Determines if a user is known by the internal databases. If not
        /// it will create and authenticate the user. If so, it will simply look up the user.
        /// </summary>
        /// <param name="s">The server to check for the user on.</param>
        /// <param name="irch">The irc host to check for.</param>
        /// <returns>The new or old user.</returns>
        private static User UpdateUserDB(Server s, IRCHost irch) {
            User u;
            try {
                u = s.UserDatabase[irch.Nick];

                if ( !u.CurrentHost.Equals( irch ) )
                    u.CurrentHost = irch;
            }
            catch ( KeyNotFoundException ) {
                u = new User( irch );
                s.UserDatabase.AddUser( u );
                s.UserDatabase.Authenticate( u );
            }

            return u;
        }

        /// <summary>
        /// Determines if a channel is known by the internal databases. If not
        /// it will create the channel. If so, it will simply look up the channel..
        /// </summary>
        /// <param name="s">The server to check for the channel on.</param>
        /// <param name="channelname">The channel name to check for.</param>
        /// <returns>The new or old user.</returns>
        private static Channel UpdateChanDB(Server s, string channelname) {
            Channel c;
            try {
                c = s.ChannelDatabase[channelname];
            }
            catch ( KeyNotFoundException ) {
                c = new Channel( channelname );
                s.ChannelDatabase.AddChannel( c );
            }
            return c;
        }

        /// <summary>
        /// Retrieves a ChannelUser from the database.
        /// </summary>
        /// <param name="s">The server to check for the ChannelUser.</param>
        /// <param name="channelname">The channel to check.</param>
        /// <param name="usernick">The nick of a definetely existing user.</param>
        /// <returns>The new or old ChannelUser.</returns>
        private static ChannelUser UpdateBothDB(Server s, string channelname, string usernick) {
            return UpdateBothDB( s, channelname, usernick, null );
        }

        /// <summary>
        /// Retrieves a ChannelUser from the database.
        /// </summary>
        /// <param name="s">The server to check for the ChannelUser.</param>
        /// <param name="channelname">The channel to check.</param>
        /// <param name="usernick">The nick of a definetely existing user.</param>
        /// <param name="uflag">If the channel user does not exist he must be created with a userflag. This is that flag.</param>
        /// <returns>The new or old ChannelUser.</returns>
        private static ChannelUser UpdateBothDB(Server s, string channelname, string usernick, Nullable<char> uflag) {
            ChannelUser cu;
            try {
                cu = s.ChannelDatabase[channelname][usernick]; //Check if ChannelUser exists.
            }
            catch ( KeyNotFoundException ) {
                //ChannelUser does not exist.
                cu = new ChannelUser();

                User internalu;
                try {
                    internalu = s.UserDatabase[usernick];
                }
                catch ( KeyNotFoundException ) {
                    cu.UserFlag = uflag;
                    s.ChannelDatabase[channelname].AddUser( usernick, cu );
                    return cu;
                }

                cu.InternalUser = internalu;
                cu.UserFlag = uflag;
                s.ChannelDatabase[channelname].AddUser( cu );
                return cu;
            }

            //Make certain that if the usernick exists in the database that we attach it to it's user counterpart.
            if ( cu.InternalUser == null ) {
                User internalu;
                try {
                    internalu = s.UserDatabase[usernick];
                    cu.InternalUser = internalu;
                }
                catch ( KeyNotFoundException ) {
                    //throw new ExecutionEngineException( "PLACEHOLDER FOR DESTRUCTION THIS SHOULD NEVER HAPPEN" );
                }
            }

            return cu;
        }

        #endregion

        #region Raw Helpers

        /// <summary>
        /// Returns a Nick message defined in IRC Protocol.
        /// </summary>
        /// <param name="s">The server object to derive the nick string from.</param>
        /// <returns>The nick string.</returns>
        public static string CreateNickString(int sid) {
            Server s = Server.GetServer( sid );
            return "NICK :" + s.Nickname;
        }

        /// <summary>
        /// Returns a Nick message defined in IRC Protocol using the alternate nickname.
        /// </summary>
        /// <param name="s">The server object to derive the nick string from.</param>
        /// <returns>The nick string.</returns>
        public static string CreateAltNickString(int sid) {
            Server s = Server.GetServer( sid );
            return "NICK :" + s.AlternateNick;
        }

        /// <summary>
        /// Returns a User message defined in IRC Protocol.
        /// </summary>
        /// <param name="s">The server object to derive the user message from.</param>
        /// <returns>The user message.</returns>
        public static string CreateUserString(int sid) {
            Server s = Server.GetServer( sid );
            return "USER " + s.Username.ToLower() + " \"" + s.Username.ToLower() + ".com\" \"" + s.HostNames[0] + "\" :" + s.Info;
        }

        /// <summary>
        /// Returns a PRIVMSG message defined in IRC Protocol.
        /// </summary>
        /// <param name="target">The target receiver of the text (Nick/Channel).</param>
        /// <param name="text">The text to send.</param>
        /// <returns>The privmsg.</returns>
        public static string CreateMessageString(string target, string text) {
            return "PRIVMSG " + target + " :" + text;
        }

        #endregion

    }
}
