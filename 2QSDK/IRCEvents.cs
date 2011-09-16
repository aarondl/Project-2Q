using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;

namespace Project2Q.SDK {

    /// <summary>
    /// Global internal events for the bot.
    /// </summary>
    public class IRCEvents {

        #region Constructor

        public IRCEvents() {
            parses = new List<Parse>();
            wildcardParses = new List<Parse>();
        }

        #endregion

        #region Parsers

        /// <summary>
        /// An enumeration that helps a module specify which event
        /// should trigger this parse.
        /// </summary>
        [Flags]
        public enum ParseTypes {
            /// <summary>
            /// Message from a user ( not in a channel ).
            /// </summary>
            PrivateMessage = 0x1,
            /// <summary>
            /// Message in a channel.
            /// </summary>
            ChannelMessage = 0x4,
            /// <summary>
            /// Notice from a user ( not in a channel ).
            /// </summary>
            PrivateNotice = 0x8,
            /// <summary>
            /// Notice in a channel.
            /// </summary>
            ChannelNotice = 0x16,
            /// <summary>
            /// Notice from the server.
            /// </summary>
            ServerNotice = 0x32
        }

        /// <summary>
        /// A struct containing the values that are returned from a parse success.
        /// </summary>
        public struct ParseReturns {
            private Channel c;
            private ChannelUser cu;
            private User u;
            private string text;
            private int sid;

            /// <summary>
            /// The server id.
            /// </summary>
            public int ServerID {
                get { return sid; }
                set { sid = value; }
            }

            /// <summary>
            /// Channel that the parse was triggered on.
            /// </summary>
            public Channel Channel {
                get { return c; }
                set { c = value; }
            }

            /// <summary>
            /// ChannelUser that triggered the parse.
            /// </summary>
            public ChannelUser ChannelUser {
                get { return cu; }
                set { cu = value; }
            }

            /// <summary>
            /// Offending text.
            /// </summary>
            public string Text {
                get { return text; }
                set { text = value; }
            }

            /// <summary>
            /// User that triggered the parse.
            /// </summary>
            public User User {
                get { return u; }
                set { u = value; }
            }
        }

        /// <summary>
        /// Readonly struct that holds a parse string, and the flags depicting the events to attach to.
        /// </summary>
        public struct Parse {
            private string parse;
            private ParseTypes ptypes;
            private CrossAppDomainDelegate function;
            private FieldInfo fi;
            private object instanceOf;

            public Parse(string parse, CrossAppDomainDelegate function, ParseTypes ptypes, FieldInfo fi, Object instanceOf) {
                this.parse = parse;
                this.ptypes = ptypes;
                this.function = function;
                this.fi = fi;
                this.instanceOf = instanceOf;
            }

            /// <summary>
            /// Gets the parse string.
            /// </summary>
            public string ParseString {
                get { return parse; }
            }

            /// <summary>
            /// Gets the parse types to attach to.
            /// </summary>
            public ParseTypes ParseTypes {
                get { return ptypes; }
            }

            /// <summary>
            /// Gets the function to call.
            /// </summary>
            public CrossAppDomainDelegate Function {
                get { return function; }
            }

            /// <summary>
            /// Gets the field info to insert the ParseReturns struct into.
            /// </summary>
            public FieldInfo FieldInfo {
                get { return fi; }
            }

            /// <summary>
            /// Gets the instance of the object to send the field info to.
            /// </summary>
            public Object InstanceOf {
                get { return instanceOf; }
            }

            public int CompareTo(object obj) {
                return parse.CompareTo( ((Parse)obj).ParseString );
            }
        }

        private List<Parse> parses;
        private List<Parse> wildcardParses;

        /// <summary>
        /// Gets the parses list.
        /// </summary>
        public List<Parse> Parses {
            get { return parses; }
        }

        /// <summary>
        /// Gets the wildcard parses list.
        /// </summary>
        public List<Parse> WildcardParses {
            get { return wildcardParses; }
        }

        #endregion

        #region Delegates

        public delegate string[] String1(int serverId, string a);
        public delegate string[] String3(int serverId, string a, string b, string c);
        public delegate string[] ServerEvent(int serverId);

        //Possibly recombine this into a general 'message' delegate later.
        public delegate string[] DisconnectType(int serverId, string message);
        public delegate string[] ServerNoticeType( int serverId, string message );
        public delegate string[] PrivateNoticeType( int serverId, User user, string message );
        public delegate string[] ChannelNoticeType( int serverId, Channel c, User u, string message ); // can be sent from outside of a channel.

        public delegate string[] UserMessageType(int serverId, string nick, User user, string message);
        public delegate string[] ChannelMessageType(int serverId, Channel c, ChannelUser user, string message);
        public delegate string[] UserCTCPMessageType(int serverId, string nick, User user, string message);
        public delegate string[] ChannelCTCPMessageType(int serverId, Channel c, ChannelUser user, string message);
        public delegate string[] JoinMessageType(int serverId, Channel c, ChannelUser cu);
        public delegate string[] BotJoinMessageType(int serverId, Channel c);
        public delegate string[] NamesMessageType(int serverId, Channel c);

        public delegate string[] ServerModeMessageType(int serverId, User u, string recipient, string mode);
        
        public delegate string[] NicknameMessageType(int serverId, User u, string oldnick);
        public delegate string[] QuitMessageType(int serverId, User u, string message);
        public delegate string[] PartMessageType(int serverId, ChannelUser u, Channel c, string message);
        public delegate string[] BotPartMessageType(int serverId, Channel c, string message);
        public delegate string[] BotNicknameMessageType(int serverId, string newnick, string oldnick);

        public delegate string[] Err_NickNameInUseType(int serverId, string badnick);

        public delegate string[] WelcomeMessageType( int serverId, string welcome );

        //Hack. This is for bot use only! Therefore it is not availible in modules. DOUBLE CHECK IT'S NOT THUR
        //And the way I've coded the ModuleEvents. They'll get an error for even trying. Mwhaha.
        public delegate string[] WhoMessageType(int serverid, Channel c, IRCHost user);
        #endregion

        #region Events

        //Old style events
        public event String1 Ping;
        public event ServerEvent Connect;

        //New style events.
        public event ChannelNoticeType ChannelNotice;
        public event PrivateNoticeType PrivateNotice;
        public event ServerNoticeType ServerNotice;
        public event ChannelMessageType ChannelMessage;
        public event UserMessageType UserMessage; //Possibly temporary, see above.
        public event ChannelCTCPMessageType ChannelCTCPMessage;
        public event UserCTCPMessageType UserCTCPMessage;
        public event ServerModeMessageType ServerModeMessage;
        public event NamesMessageType Names;
        public event JoinMessageType Join;
        public event BotJoinMessageType BotJoin;
        public event NicknameMessageType NickName;
        public event BotNicknameMessageType BotNickName;
        public event PartMessageType Part;
        public event BotPartMessageType BotPart;
        public event QuitMessageType Quit;
        public event WelcomeMessageType Welcome;
        public event WhoMessageType Who;

        public event DisconnectType Disconnect;
        
        //Errors:
        public event Err_NickNameInUseType Err_NickNameInUse;
        
        
        #endregion
        
        #region Event Raisers


        /// <summary>
        /// Fires the Err_NickNameInUse event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="badnick">The bad nickname.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnErr_NickNameInUse(int sid, string badnick) {
            if ( Err_NickNameInUse != null )
                return InvocationTunnel( this.Err_NickNameInUse.GetInvocationList(),
                    sid, badnick );
            return null;
        }

        /// <summary>
        /// Fires the OnDisconnect event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="message">The reason for disconnection.</param>
        /// <returns>Always null, ignore.</returns>
        public string[][] OnDisconnect(int sid, string message) {
            if ( Disconnect != null )
                InvocationTunnel( this.Disconnect.GetInvocationList(),
                    sid, message );
            return null;
        }

        /// <summary>
        /// Fires the Who event.
        /// </summary>
        /// <param name="sid">The server on which the part happened.</param>
        /// <param name="c">The Channel that was who'd.</param>
        /// <param name="user">The user that was parsed out.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnWho(int sid, Channel c, IRCHost user) {
            if ( Who != null )
                return InvocationTunnel( this.Who.GetInvocationList(), sid, c, user );
            return null;
        }

        /// <summary>
        /// Fires the Part event.
        /// </summary>
        /// <param name="sid">The server on which the part happened.</param>
        /// <param name="cu">The ChannelUser that parted.</param>
        /// <param name="message">The quit message.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnPart(int sid, ChannelUser u, Channel c, string message) {
            if ( Part != null )
                return InvocationTunnel( this.Part.GetInvocationList(),
                    u.InternalUser.UserAttributes == null ?
                    null : u.InternalUser.UserAttributes.Privegeles, sid, u, c, message );
            return null;
        }

        /// <summary>
        /// Fires the BotPart event.
        /// </summary>
        /// <param name="sid">The server on which the part happened.</param>
        /// <param name="message">The quit message.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnBotPart(int sid, Channel c, string message) {
            if ( BotPart != null )
                return InvocationTunnel( this.BotPart.GetInvocationList(),
                    sid, c, message );
            return null;
        }

        /// <summary>
        /// Fires the Quit event.
        /// </summary>
        /// <param name="sid">The server on which the quit happened.</param>
        /// <param name="u">The user that quit.</param>
        /// <param name="message">The quit message.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnQuit(int sid, User u, string message) {
            if ( Quit != null )
                return InvocationTunnel( this.Quit.GetInvocationList(),
                    sid, u, message );
            return null;
        }

        /// <summary>
        /// Fires the NickName event.
        /// </summary>
        /// <param name="sid">The server on which the nick change happened on.</param>
        /// <param name="u">The new user that emerged from the nick change.</param>
        /// <param name="oldnick">The old nickname.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnNickName(int sid, User u, string oldnick) {
            if ( NickName != null )
                return InvocationTunnel( this.NickName.GetInvocationList(),
                    u.UserAttributes == null ?
                    null : u.UserAttributes.Privegeles, sid, u, oldnick );
            return null;
        }

        /// <summary>
        /// Fires the BotNickName event.
        /// </summary>
        /// <param name="sid">The server on which the bot has changed it's nick.</param>
        /// <param name="newnick">The new nickname.</param>
        /// <param name="oldnick">The old nickname.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnBotNickName(int sid, string newnick, string oldnick) {
            if ( BotNickName != null )
                return InvocationTunnel( this.BotNickName.GetInvocationList(),
                    sid, newnick, oldnick );
            return null;
        }

        /// <summary>
        /// Fires the OnNames event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="c">The channel.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnNames(int sid, Channel channel) {
            if ( Names != null )
                return InvocationTunnel( this.Names.GetInvocationList(),
                    sid, channel );
            return null;
        }

        /// <summary>
        /// Fires the OnServerModeMessage event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="c">The channel.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnServerModeMessage(int sid, User u, string recipient, string modes) {
            if ( ServerModeMessage != null )
                return InvocationTunnel( this.ServerModeMessage.GetInvocationList(),
                    sid, u, recipient, modes);
            return null;
        }

        /// <summary>
        /// Fires OnWelcome event.
        /// </summary>
        /// <param name="sid">The server id</param>
        /// <param name="welcome">the message</param>
        /// <returns>raw data sent to server</returns>
        public string[][] OnWelcome( int sid, string welcome ) {
            if ( Welcome != null )
                return InvocationTunnel( this.Welcome.GetInvocationList(),
                    sid, welcome );
            return null;
        }

        /// <summary>
        /// Fires the BotJoin event.
        /// </summary>
        /// <param name="sid">The server on which the bot has joined a channel.</param>
        /// <param name="channel">The channel which the bot has joined.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnBotJoin(int sid, Channel channel) {
            if ( BotJoin != null )
                return InvocationTunnel( this.BotJoin.GetInvocationList(),
                    sid, channel );
            return null;
        }

        /// <summary>
        /// Fires the OnJoin event.
        /// </summary>
        /// <param name="sid">The server on which the user has joined a channel.</param>
        /// <param name="channel">The channel which the user has joined.</param>
        /// <param name="cu">The user which joined the channel.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnJoin(int sid, Channel channel, ChannelUser cu) {
            if ( Join != null )
                return InvocationTunnel( this.Join.GetInvocationList(),
                    cu.InternalUser.UserAttributes == null ?
                    null : cu.InternalUser.UserAttributes.Privegeles, sid, channel, cu );
            return null;
        }
        
        /// <summary>
        /// Fire the ChannelMessage event.
        /// </summary>
        /// <param name="sid">Server ID</param>
        /// <param name="channel">The channel which the message was destined for.</param>
        /// <param name="userhost">The userhost of the sender.</param>
        /// <param name="text">The contents of the message.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnChannelMessage(int sid, Channel channel, ChannelUser user, string text) {
            if ( ChannelMessage != null )
                return InvocationTunnel(this.ChannelMessage.GetInvocationList(),
                    user.InternalUser.UserAttributes == null ? 
                    null : user.InternalUser.UserAttributes.Privegeles, sid, channel, user, text );
            return null;
        }

        /// <summary>
        /// Fires UserMessage Event
        /// </summary>
        /// <param name="sid">Server ID</param>
        /// <param name="nick">The nick who the message was destined for.</param>
        /// <param name="userhost">The userhost of the sender.</param>
        /// <param name="text">The contents of the message.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnUserMessage(int sid, string nick, User user, string text) {
            if ( UserMessage != null ) {
                return InvocationTunnel(this.UserMessage.GetInvocationList(), 
                    user.UserAttributes == null ? null : user.UserAttributes.Privegeles, sid, nick, user, text);
            }
            return null;
        }

        /// <summary>
        /// Fire the ChannelCTCPMessage event.
        /// </summary>
        /// <param name="sid">Server ID</param>
        /// <param name="channel">The channel which the message was destined for.</param>
        /// <param name="userhost">The userhost of the sender.</param>
        /// <param name="text">The contents of the message.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnChannelCTCPMessage(int sid, Channel channel, ChannelUser user, string text) {
            if ( ChannelCTCPMessage != null )
                return InvocationTunnel( this.ChannelCTCPMessage.GetInvocationList(),
                    user.InternalUser.UserAttributes == null ?
                    null : user.InternalUser.UserAttributes.Privegeles, sid, channel, user, text );
            return null;
        }

        /// <summary>
        /// Fires UserCTCPMessage Event
        /// </summary>
        /// <param name="sid">Server ID</param>
        /// <param name="nick">The nick who the message was destined for.</param>
        /// <param name="userhost">The userhost of the sender.</param>
        /// <param name="text">The contents of the message.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnUserCTCPMessage(int sid, string nick, User user, string text) {
            if ( UserCTCPMessage != null ) {
                return InvocationTunnel( this.UserCTCPMessage.GetInvocationList(),
                    user.UserAttributes == null ? null : user.UserAttributes.Privegeles, sid, nick, user, text );
            }
            return null;
        }

        /// <summary>
        /// Fires ChannelNotice Event
        /// </summary>
        /// <param name="sid">Server ID</param>
        /// <param name="channel">Channel which the message was sent on</param>
        /// <param name="user">The user who sent the message</param>
        /// <param name="text">The text the user sent</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnChannelNotice( int sid, Channel channel, User user, string text ) {
            if ( ChannelNotice != null )
                return InvocationTunnel( this.ChannelNotice.GetInvocationList(), 
                    user.UserAttributes == null ?
                    null : user.UserAttributes.Privegeles, sid, channel, user, text );
            return null;
        }

        /// <summary>
        /// Fires UserNotice Event
        /// </summary>
        /// <param name="sid">ServerID</param>
        /// <param name="user">The user who sent the message</param>
        /// <param name="text">The text the user sent</param>
        /// <returns>The raw data to be sent to the server</returns>
        public string[][] OnPrivateNotice( int sid, User user, string text ) {
            if ( PrivateNotice != null )
                return InvocationTunnel( this.PrivateNotice.GetInvocationList(),
                    user.UserAttributes == null ?
                    null : user.UserAttributes.Privegeles, sid, user, text );
            return null;
        }

        /// <summary>
        /// Fires ServerNotice Event
        /// </summary>
        /// <param name="sid">ServerID</param>
        /// <param name="text">The text the server sent</param>
        /// <returns>The raw data to be sent to the server</returns>
        public string[][] OnServerNotice( int sid, string text ) {
            if ( ServerNotice != null )
                return InvocationTunnel( this.ServerNotice.GetInvocationList(), sid, text );
            return null;
        }

        /// <summary>
        /// Fires the Ping event.
        /// </summary>
        /// <param name="sid">Server ID.</param>
        /// <param name="postBack">The expected reply to the server.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnPing(int sid, string postBack) {
            if ( Ping != null )
                return InvocationTunnel( this.Ping.GetInvocationList(), sid, postBack );
            return null;
        }

        /// <summary>
        /// Fires the OnConnect event.
        /// </summary>
        /// <param name="sid">Server ID.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[][] OnConnect(int sid) {
            if ( Connect != null )
                return InvocationTunnel( this.Connect.GetInvocationList(), sid );
            return null;
        }

        /// <summary>
        /// Invokes a list of functions with specified arguments.
        /// (Created to avoid code re-usage in the event-firers.)
        /// </summary>
        /// <param name="invocList">A list of functions to invoke.</param>
        /// <param name="args">The arguments to pass to the functions.</param>
        /// <returns>A compilation of the returns from all the functions.</returns>
        private string[][] InvocationTunnel(Delegate[] invocList, params object[] args) {
            int n = invocList.Length;
            string[][] returns = new string[n][];
            for ( int i = 0; i < n; i++ ) {
                Delegate invoc = invocList[i];
                returns[i] = (string[])invoc.Method.Invoke( invoc.Target, args );
            }
            return returns;
        }

        private string[][] InvocationTunnel(Delegate[] invocList, PrivelegeContainer p, params object[] args) {

            int n = invocList.Length;
            string[][] returns = new string[n][];
            for ( int i = 0; i < n; i++ ) {

                bool continueWithCall = true;

                Delegate invoc = invocList[i];

                if ( p == null || !p.HasPrivelege( Priveleges.SuperUser ) ) {

                    object[] privreq = invoc.Method.GetCustomAttributes( false );

                    for ( int j = 0; j < privreq.Length; j++ ) {
                        if ( continueWithCall && privreq[j].GetType().Equals( typeof( PrivelegeRequiredAttribute ) ) ) {
                            PrivelegeRequiredAttribute pra = (PrivelegeRequiredAttribute)privreq[j];
                            continueWithCall = p != null ? p.HasPrivelege( pra.Required ) : ( (ulong)pra.Required == 0 );
                        }
                        if ( continueWithCall && privreq[j].GetType().Equals( typeof( UserLevelRequiredAttribute ) ) ) {
                            UserLevelRequiredAttribute ura = (UserLevelRequiredAttribute)privreq[j];
                            continueWithCall = p != null ? p.NumericalLevel >= ura.Required : ( ura.Required == 0 );
                        }

                        if ( !continueWithCall )
                            break;
                    }

                }

                if ( continueWithCall )
                    returns[i] = (string[])invoc.Method.Invoke( invoc.Target, args );
                else {
                    returns[i] = null;
                }

            }
            return returns;
        }

        #endregion

    }

}
