using System;
using System.Collections.Generic;
using System.Text;

using Injections = Project2Q.SDK.Injections;
using Project2Q.SDK;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;
using Project2Q.SDK.ModuleSupport;

namespace UserManagment {

    public class Main : IModuleCreator {

        public override void  ActivationComplete() {
            mp.RegisterEvent( "ChannelMessage", new CrossAppDomainDelegate( Methods ) );
            mp.RegisterEvent( "ChannelMessage", new CrossAppDomainDelegate( HostsMethods ) );
        }

        public Injections.ChannelMessageEvent channelMessageData;

        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        /// <param name="splits">The raw data to parse.</param>
        public void AddUser( string[] splits ) {

            string text = channelMessageData.text;
            string user = channelMessageData.channelUser.InternalUser.Nickname;
            string channel = channelMessageData.channel.Name;

            //Parse format:
            //*adduser (nickname) [privlist] [numericallevel]
            //  [0]        [1]       [2]           [3]

            if ( splits.Length < 3 || splits.Length > 4 ) {
                BoldNickNotice( user, "Error(Malformed command):",
                " Arguments expected: 2-3, arguments given " + ( splits.Length - 1 ).ToString() + "." );
                return;
            }

            LinkedList<string> ll = new LinkedList<string>();
            StringBuilder errprivs = new StringBuilder();
            StringBuilder neaprivs = new StringBuilder();

            uint numlevel = 0;
            ulong addprivs = 0;
            bool privthur = false, numthur = false;
            int chn;

            User u = null;
            UserCollection uc = (UserCollection)mp.RequestVariable( Request.UserCollection, channelMessageData.sid );

            try {
                u = uc[splits[1]];
            }
            catch ( KeyNotFoundException ) {
                BoldNickNotice( user, "Error(Invalid User):",
                " User " + splits[1] + " not found in active user database." );
                return;
            }

            if ( u.UserAttributes != null ) {
                BoldNickNotice( user, "Error(Invalid User):",
                " User " + splits[1] + " already authenticated. (Did you mean " + Configuration.ModuleConfig.ModulePrefix + "chusr?)" );
                return;
            }

            //first arg - mandatory
            try {
                numlevel = uint.Parse( splits[2] );
                if ( !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( Priveleges.SuperUser ) &&
                    numlevel >= channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.NumericalLevel ) {
                    ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Numerical Level):",
                        " The numerical level: " + splits[2] + " is greater or equal to your own." ) );
                }
                else
                    numthur = true;
            }
            catch ( FormatException ) {
                foreach ( char c in splits[2] ) {
                    chn = (int)c;
                    if ( ( chn > (int)'A' && chn < (int)'z' ) && ( chn > (int)'a' || chn < (int)'Z' ) ) {
                        ulong modemask = PrivelegeContainer.GetModeMask( c );
                        if ( !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( Priveleges.SuperUser ) &&
                            !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( modemask ) ) {
                            neaprivs.Append( c );
                        }
                        else {
                            privthur = true;
                            addprivs |= modemask;
                        }
                    }
                    else
                        errprivs.Append( c );
                }
            }

            //second arg - optional, check for length
            if ( splits.Length > 3 )
                //If the priveleges is already there. We can only look for a numerical userlevel.
                if ( privthur )
                    try {
                        numlevel = uint.Parse( splits[3] );
                        if ( numlevel >= channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.NumericalLevel &&
                            !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( Priveleges.SuperUser ) ) {
                            ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Numerical Level):",
                                " The numerical level: " + splits[3] + " is greater or equal to your own." ) );
                        }
                        else
                            numthur = true;
                    }
                    catch ( FormatException ) {
                        ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Argument):",
                            " The argument: " + splits[3] + " is not a valid numerical user level." ) );
                    }
                //If the priveleges aren't there, we can only look for a privelege set.
                else {
                    foreach ( char c in splits[3] ) {
                        chn = (int)c;
                        if ( ( chn > (int)'A' && chn < (int)'z' ) && ( chn > (int)'a' || chn < (int)'Z' ) ) {
                            ulong modemask = PrivelegeContainer.GetModeMask( c );
                            if ( !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( Priveleges.SuperUser ) &&
                                !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( modemask ) ) {
                                neaprivs.Append( c );
                            }
                            else {
                                privthur = true;
                                addprivs |= modemask;
                            }
                        }
                        else
                            errprivs.Append( c );
                    }
                }

            if ( errprivs.Length != 0 ) {
                ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Argument):",
                    " The arguments: " + errprivs.ToString() + " are not valid mode characters." ) );
            }
            if ( neaprivs.Length != 0 ) {
                ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Argument):",
                    " You must have the following modes: " + neaprivs.ToString() + " to give those modes to others." ) );
            }


            RegisteredUser ru = new RegisteredUser();
            ru.AddHost( u.CurrentHost );

            uc.AddRegisteredUser( ru );
            u.UserAttributes = ru;

            if ( privthur ) {
                u.UserAttributes.Privegeles.AddPriveleges( addprivs );
            }
            if ( numthur ) {
                u.UserAttributes.Privegeles.NumericalLevel = numlevel;
            }

            if ( privthur || numthur ) {
                StringBuilder sb = new StringBuilder();
                sb.Append( " [" );
                if ( privthur ) {
                    sb.Append( u.UserAttributes.Privegeles.PrivelegeString );
                    if ( numthur )
                        sb.Append( ":" + u.UserAttributes.Privegeles.NumericalLevel );
                }
                else if ( numthur )
                    sb.Append( u.UserAttributes.Privegeles.NumericalLevel );
                sb.Append( "]" );

                ll.AddLast( BoldNickNoticeHelper( user, "User Added: ", u.Nickname + sb.ToString() ) );
            }

            if ( ll.Count != 0 ) {
                returns = new string[ll.Count];
                ll.CopyTo( returns, 0 );
            }

        }

        /// <summary>
        /// Removes a user from the database.
        /// </summary>
        /// <param name="splits">Raw data to parse.</param>
        public void DelUser( string[] splits ) {

            string text = channelMessageData.text;
            string user = channelMessageData.channelUser.InternalUser.Nickname;
            string channel = channelMessageData.channel.Name;

            //?deluser nick
            //  [0]     [1]
            if ( splits.Length < 2 ) {
                BoldNickNotice( user, "Error(Invalid User):",
                    "User not found in active user database." );
                return;
            }

            UserCollection uc = (UserCollection)mp.RequestVariable( Request.UserCollection, channelMessageData.sid );

            User u = null;
            try {
                u = uc[splits[1]];
            }
            catch ( KeyNotFoundException ) {
                BoldNickNotice( user, "Error(Invalid User):",
                    "User " + splits[1] + " not found in user database." );
                return;
            }

            if ( u.UserAttributes == null ) {
                BoldNickNotice( user, "Error(Not Authed): ", "User " + splits[1] + " is not authenticated." );
                return;
            }

            //TODO : If user already exists
            //BoldNickNotice( user, "Error: ", "User " + splits[1] + " is already on the userlist with access 100+cU." );
            //Delete him -- this disconnects his registereduser object too. No worries there ^_^
            uc.RemoveRegisteredUser( u );
            BoldNickNotice( user, "Deleted: ", u.Nickname );
        }

        /// <summary>
        /// Modifies the privelege and numerical user levels of a user.
        /// </summary>
        /// <param name="splits">Raw data to parse.</param>
        public void LevelUser( string[] splits ) {
            string text = channelMessageData.text;
            string user = channelMessageData.channelUser.InternalUser.Nickname;
            string channel = channelMessageData.channel.Name;

            //Parse format:
            //*adduser (nickname) [privlist] [numericallevel]
            //  [0]        [1]       [2]           [3]

            if ( splits.Length < 3 || splits.Length > 4 ) {
                BoldNickNotice( user, "Error(Malformed command):",
                " Arguments expected: 2-3, arguments given " + ( splits.Length - 1 ).ToString() + "." );
                return;
            }

            User u = null;
            UserCollection uc = (UserCollection)mp.RequestVariable( Request.UserCollection, channelMessageData.sid );

            try {
                u = uc[splits[1]];
            }
            catch ( KeyNotFoundException ) {
                BoldNickNotice( user, "Error(Invalid User):",
                " User " + splits[1] + " not found in active user database." );
                return;
            }

            if ( u.UserAttributes == null ) {
                BoldNickNotice( user, "Error(Invalid User):",
                " User " + splits[1] + " not authenticated. (Did you mean " + Configuration.ModuleConfig.ModulePrefix + "ausr?)" );
                return;
            }

            string privs = null;
            
            LinkedList<string> ll = new LinkedList<string>();
            StringBuilder errprivs = new StringBuilder();
            StringBuilder neaprivs = new StringBuilder();

            uint numlevel = 0;
            ulong addprivs = 0, subprivs = 0;
            bool privthur = false, numthur = false;
            int chn;

            //first arg - mandatory
            try {
                numlevel = uint.Parse( splits[2] );
                if ( numlevel >= channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.NumericalLevel &&
                    !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( Priveleges.SuperUser ) ) {
                    ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Numerical Level):",
                        " The numerical level: " + splits[2] + " is greater or equal to your own." ) );
                }
                else
                    numthur = true;
            }
            catch ( FormatException ) {
                privs = splits[2];
                bool adding = true;
                foreach ( char c in privs ) {
                    chn = (int)c;
                    if ( c == '+' )
                        adding = true;
                    else if ( c == '-' )
                        adding = false;
                    else if ( ( chn > (int)'A' && chn < (int)'z' ) && ( chn > (int)'a' || chn < (int)'Z' ) ) {
                        ulong modemask = PrivelegeContainer.GetModeMask( c );
                        if ( !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( Priveleges.SuperUser ) &&
                            !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( modemask ) ) {
                            neaprivs.Append( c );
                            continue;
                        }
                        privthur = true;
                        if ( adding )
                            addprivs |= modemask;
                        else
                            subprivs |= modemask;
                    }
                    else
                        errprivs.Append( c );
                }
            }

            //second arg - optional, check for length
            if ( splits.Length > 3 )
                //If the priveleges is already there. We can only look for a numerical userlevel.
                if ( privthur )
                    try {
                        numlevel = uint.Parse( splits[3] );
                        if ( numlevel >= channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.NumericalLevel &&
                            !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( Priveleges.SuperUser ) ) {
                            ll.AddLast(BoldNickNoticeHelper( user, "Error(Invalid Numerical Level):",
                                " The numerical level: " + splits[3] + " is greater or equal to your own." ));
                        }
                        else
                            numthur = true;
                    }
                    catch ( FormatException ) {
                        ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Argument):",
                            " The argument: " + splits[3] + " is not a valid numerical user level." ));
                    }
                //If the priveleges aren't there, we can only look for a privelege set.
                else {
                    privs = splits[3];
                    bool adding = true;
                    foreach ( char c in privs ) {
                        chn = (int)c;
                        if ( c == '+' )
                            adding = true;
                        else if ( c == '-' )
                            adding = false;
                        else if ( ( chn > (int)'A' && chn < (int)'z' ) && ( chn > (int)'a' || chn < (int)'Z' ) ) {
                            ulong modemask = PrivelegeContainer.GetModeMask( c );
                            if ( !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( Priveleges.SuperUser ) &&
                                !channelMessageData.channelUser.InternalUser.UserAttributes.Privegeles.HasPrivelege( modemask ) ) {
                                neaprivs.Append( c );
                                continue;
                            }
                            privthur = true;
                            if ( adding )
                                addprivs |= modemask;
                            else
                                subprivs |= modemask;
                        }
                        else
                            errprivs.Append( c );
                    }
                }

            if ( errprivs.Length != 0 ) {
                ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Argument):",
                    " The arguments: " + errprivs.ToString() + " are not valid mode characters." ) );
            }
            if ( neaprivs.Length != 0 ) {
                ll.AddLast( BoldNickNoticeHelper( user, "Error(Invalid Argument):",
                    " You must have the following modes: " + neaprivs.ToString() + " to give those modes to others." ) );
            }

            if ( privthur ) {
                u.UserAttributes.Privegeles.AddPriveleges( addprivs );
                u.UserAttributes.Privegeles.RemovePriveleges( subprivs );
            }
            if ( numthur ) {
                u.UserAttributes.Privegeles.NumericalLevel = numlevel;
            }

            if ( privthur || numthur ) {
                StringBuilder sb = new StringBuilder();
                sb.Append( " [" );
                if ( privthur ) {
                    sb.Append( u.UserAttributes.Privegeles.PrivelegeString );
                    if ( numthur )
                        sb.Append( ":" + u.UserAttributes.Privegeles.NumericalLevel );
                }
                else if ( numthur )
                    sb.Append( u.UserAttributes.Privegeles.NumericalLevel );
                sb.Append( "]" );

                ll.AddLast( BoldNickNoticeHelper( user, "User Modified: ", u.Nickname + sb.ToString() ) );
            }

            if ( ll.Count != 0 ) {
                returns = new string[ll.Count];
                ll.CopyTo( returns, 0 );
            }

        }

        /// <summary>
        /// Retrieves the userlist.
        /// </summary>
        public void UserList() {
            BoldNickNotice( channelMessageData.channelUser.InternalUser.Nickname, "User List: ", GetUsers() );
            return;
        }

        /// <summary>
        /// Sets the modules return state to notice nick on the network. Bolding the boldtxt
        /// and leaving the txt regular.
        /// </summary>
        /// <param name="nick">The nickname to notice.</param>
        /// <param name="boldtxt">The text to bold.</param>
        /// <param name="txt">The text to send regularily.</param>
        public void BoldNickNotice(string nick, string boldtxt, string txt) {
            returns = new string[] {
                //":" + nick + " NOTICE " + nick + " :" + (char)2 + boldtxt + (char)2 + txt
                BoldNickNoticeHelper(nick, boldtxt, txt),
            };
        }

        /// <summary>
        /// Returns a string set up to notice nick on the network. Bolding the boldtxt
        /// and leaving the txt regular.
        /// </summary>
        /// <param name="nick">The nickname to notice.</param>
        /// <param name="boldtxt">The text to bold.</param>
        /// <param name="txt">The text to send regularily.</param>
        public string BoldNickNoticeHelper(string nick, string boldtxt, string txt) {
            return ":" + nick + " NOTICE " + nick + " :" + (char)2 + boldtxt + (char)2 + txt;
        }

        /// <summary>
        /// List the hosts of a user.
        /// </summary>
        /// <param name="repnick">The user that initiated the request.</param>
        /// <param name="user">The user to list the hosts of.</param>
        public void ListHosts(string repnick, string user) {
            User u = null;
            UserCollection uc = (UserCollection)mp.RequestVariable( Request.UserCollection, channelMessageData.sid );

            try {
                u = uc[user];
            }
            catch ( KeyNotFoundException ) {
                BoldNickNotice( repnick, "Error(Invalid User):", " User not in active user database." );
                return;
            }

            if ( u.UserAttributes == null ) {
                BoldNickNotice( repnick, "Error(Invalid User):", " User not in user database." );
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append( ' ' );
            foreach ( IRCHost irch in u.UserAttributes.HostList ) {
                sb.Append( "[" + irch.ToString() + "]" );
            }

            BoldNickNotice( repnick, "Hosts for " + u.Nickname + ":", sb.ToString() );

        }

        /// <summary>
        /// Adds a host to a user.
        /// </summary>
        /// <param name="repnick">The user that initiated the command.</param>
        /// <param name="host">The host to add.</param>
        /// <param name="user">The user to add the host to.</param>
        public void AddHost(string repnick, string host, string user) {

            User u = null;
            UserCollection uc = (UserCollection)mp.RequestVariable( Request.UserCollection, channelMessageData.sid );

            try {
                u = uc[user];
            }
            catch ( KeyNotFoundException ) {
                BoldNickNotice( repnick, "Error(Invalid User):", " User not in active user database." );
                return;
            }

            if ( u.UserAttributes == null ) {
                BoldNickNotice( repnick, "Error(Invalid User):", " User not in user database." );
                return;
            }

            IRCHost irch = null;
            try {
                irch = new IRCHost( host );
            }
            catch ( FormatException ) {
                BoldNickNotice( repnick, "Error(Invalid Host):", " The host must be in the form [nick]![username]@[host] where all fields can contain wildcard characters '?' and '*'." );
                return;
            }

            if ( uc.HasHost( irch ) ) {
                BoldNickNotice( repnick, "Error(Host Exists):", " The host specified already exists. Please use a wider subset of the host you require." );
                return;
            }

            if ( !u.UserAttributes.AddHost( irch ) ) {
                BoldNickNotice( repnick, "Error(Invalid Host):", " The host specified was rejected. Try a more specific subset of the host you require." );
                return;
            }

            BoldNickNotice( repnick, "Host Added:", " " + irch.ToString() + " to " + ( ( repnick.Equals( u.Nickname ) ) ? "your account." : u.Nickname ) );

        }

        /// <summary>
        /// Removes a host from a user.
        /// </summary>
        /// <param name="repnick">The user that initiated the command.</param>
        /// <param name="host">The host to remove.</param>
        /// <param name="user">The user to remove the host from.</param>
        public void RemoveHost(string repnick, string host, string user) {
            User u = null;
            UserCollection uc = (UserCollection)mp.RequestVariable( Request.UserCollection, channelMessageData.sid );

            try {
                u = uc[user];
            }
            catch ( KeyNotFoundException ) {
                BoldNickNotice( repnick, "Error(Invalid User):", " User not in active user database." );
                return;
            }

            if ( u.UserAttributes == null ) {
                BoldNickNotice( repnick, "Error(Invalid User):", " User not in user database." );
                return;
            }

            if ( u.UserAttributes.HostList.Count <= 1 ) {
                BoldNickNotice( repnick, "Error(Invalid Operation):", " User will have no hosts remaining if you remove this one. (Try " + Configuration.ModuleConfig.ModulePrefix + "rmusr." );
                return;
            }

            IRCHost irch = null;
            try {
                irch = new IRCHost( host );
            }
            catch ( FormatException ) {
                BoldNickNotice( repnick, "Error(Invalid Host):", " The host must be in the form [nick]![username]@[host] where all fields can contain wildcard characters '?' and '*'." );
                return;
            }

            if ( !u.UserAttributes.RemoveHost( irch ) ) {
                BoldNickNotice( repnick, "Error(Host not Found):", " The host specified was not found on this user." );
                return;
            }

            BoldNickNotice( repnick, "Host Removed:", " " + irch.ToString() + " from " + ( ( repnick.Equals( u.Nickname ) ) ? "your account." : u.Nickname ) );
        }

        [PrivelegeRequired( Priveleges.UserSystem )]
        public void Methods() {

            lock ( this ) {

                string[] splits = channelMessageData.text.Split( ' ' );
                if ( channelMessageData.text.Equals( Configuration.ModuleConfig.ModulePrefix + "lusrs" ) ) {
                    UserList(); //Shows users (?users)
                }
                else if ( channelMessageData.text.StartsWith( Configuration.ModuleConfig.ModulePrefix + "ausr" ) ) {
                    AddUser( splits ); //Adds user (?ausr nick flag access)
                }
                else if ( channelMessageData.text.StartsWith( Configuration.ModuleConfig.ModulePrefix + "rmusr" ) ) {
                    DelUser( splits ); //Deletes user (?rmusr nick)
                }
                else if ( channelMessageData.text.StartsWith( Configuration.ModuleConfig.ModulePrefix + "chusr" ) ) {
                    LevelUser( splits ); //Deletes user (?chusr nick access)
                }
                else if ( channelMessageData.text.StartsWith( Configuration.ModuleConfig.ModulePrefix + "lhosts " ) ) {
                    if ( splits.Length < 2 ) {
                        BoldNickNotice( channelMessageData.channelUser.InternalUser.Nickname, "Error(Malformed command):",
                        " Arguments expected: 1, arguments given " + ( splits.Length - 1 ).ToString() + "." );
                        return;
                    }
                    ListHosts( channelMessageData.channelUser.InternalUser.Nickname, splits[1] );
                }
                else if ( channelMessageData.text.StartsWith( Configuration.ModuleConfig.ModulePrefix + "ahost" ) ) {
                    if ( splits.Length < 2 ) {
                        BoldNickNotice( channelMessageData.channelUser.InternalUser.Nickname, "Error(Malformed command):",
                        " Arguments expected: 2, arguments given " + ( splits.Length - 1 ).ToString() + "." );
                        return;
                    }
                    AddHost( channelMessageData.channelUser.InternalUser.Nickname, splits[1], splits[2] );
                }
                else if ( channelMessageData.text.StartsWith( Configuration.ModuleConfig.ModulePrefix + "rmhost" ) ) {
                    if ( splits.Length < 2 ) {
                        BoldNickNotice( channelMessageData.channelUser.InternalUser.Nickname, "Error(Malformed command):",
                        " Arguments expected: 2, arguments given " + ( splits.Length - 1 ).ToString() + "." );
                        return;
                    }
                    RemoveHost( channelMessageData.channelUser.InternalUser.Nickname, splits[1], splits[2] );
                }

            }

        }

        [PrivelegeRequired( Priveleges.UserSystem )]
        public void HostsMethods() {

            lock ( this ) {

                if ( channelMessageData.text.StartsWith( "+host" ) ) {
                    string[] splits = channelMessageData.text.Split( ' ' );
                    if ( splits.Length < 2 ) {
                        BoldNickNotice( channelMessageData.channelUser.InternalUser.Nickname, "Error(Malformed command):",
                        " Arguments expected: 1, arguments given " + ( splits.Length - 1 ).ToString() + "." );
                        return;
                    }
                    AddHost( channelMessageData.channelUser.InternalUser.Nickname, splits[1], channelMessageData.channelUser.InternalUser.Nickname );
                }
                else if ( channelMessageData.text.StartsWith( "-host" ) ) {
                    string[] splits = channelMessageData.text.Split( ' ' );
                    if ( splits.Length < 2 ) {
                        BoldNickNotice( channelMessageData.channelUser.InternalUser.Nickname, "Error(Malformed command):",
                        " Arguments expected: 1, arguments given " + ( splits.Length - 1 ).ToString() + "." );
                        return;
                    }
                    RemoveHost( channelMessageData.channelUser.InternalUser.Nickname, splits[1], channelMessageData.channelUser.InternalUser.Nickname );
                }
                else if ( channelMessageData.text.Equals( "?hosts" ) ) {
                    ListHosts( channelMessageData.channelUser.InternalUser.Nickname, channelMessageData.channelUser.InternalUser.Nickname );
                }

            }

        }

        /// <summary>
        /// Random thing. Returns a string with a user list.
        /// </summary>
        /// <returns>See above.</returns>
        public string GetUsers() {
            UserCollection uc = (UserCollection)mp.RequestVariable( Request.UserCollection, channelMessageData.sid );
            StringBuilder sb = new StringBuilder();
            foreach ( User u in uc ) {
                sb.Append( "[" );
                sb.Append( u.Nickname );
                if ( u.UserAttributes != null ) {
                    sb.Append( " (" );
                    sb.Append( u.UserAttributes.Privegeles.NumericalLevel.ToString() );
                    sb.Append( "|" );
                    sb.Append( u.UserAttributes.Privegeles.PrivelegeString );
                    sb.Append( ")" );
                    foreach ( IRCHost irch in u.UserAttributes.HostList ) {
                        sb.Append( " " );
                        sb.Append( irch.FullHost );
                    }
                }
                sb.Append( "] " );
            }
            return sb.ToString();
        }
    }
}