using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Project2Q.SDK.Injections;

using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;

namespace Project2Q.SDK.ModuleSupport {

    /// <summary>
    /// Holds event data for modules.
    /// </summary>
    internal class ModuleEvents {

        #region Variables and Constructor

        private ModuleProxy moduleProxy;

        public ModuleEvents(ModuleProxy ml) {
            this.moduleProxy = ml;
        }

        #endregion

        #region Events

        public event CrossAppDomainDelegate Welcome;
        public event CrossAppDomainDelegate Disconnect;
        public event CrossAppDomainDelegate Ping;
        public event CrossAppDomainDelegate Connect;
        public event CrossAppDomainDelegate ChannelNotice;
        public event CrossAppDomainDelegate PrivateNotice;
        public event CrossAppDomainDelegate ServerNotice;

        //Converted events:
        public event CrossAppDomainDelegate ChannelMessage;
        public event CrossAppDomainDelegate UserMessage;
        public event CrossAppDomainDelegate BotJoin;
        public event CrossAppDomainDelegate Join;
        public event CrossAppDomainDelegate Names;
        public event CrossAppDomainDelegate ChannelCTCPMessage;
        public event CrossAppDomainDelegate UserCTCPMessage;

        public event CrossAppDomainDelegate ServerModeMessage;

        public event CrossAppDomainDelegate Part;
        public event CrossAppDomainDelegate BotPart;
        public event CrossAppDomainDelegate NickName;
        public event CrossAppDomainDelegate BotNickName;
        public event CrossAppDomainDelegate Quit;

        //Errors:
        public event CrossAppDomainDelegate Err_NickNameInUse;

        #endregion

        #region Event Firers


        // for future refrence
        // this goes in <return></return>
        // The raw data to be sent to the server.


        /// <summary>
        /// Fires the OnErr_NickNameInUse event.
        /// </summary>
        /// <param name="sid">The Server Id.</param>
        /// <param name="badnick">The Nickname that was in use.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnErr_NickNameInUse(int sid, string badnick) {
            if ( Err_NickNameInUse == null )
                return null;
            Err_NickNameInUseEvent en = new Err_NickNameInUseEvent();
            en.sid = sid;
            en.badnick = badnick;
            return InvocationTunnel( Err_NickNameInUse.GetInvocationList(), en, "onNickInUseData" );
        }

        /// <summary>
        /// Fires the OnDisconnect event.
        /// </summary>
        /// <param name="sid">The server that disconnected.</param>
        /// <param name="message">The reason for disconnect.</param>
        /// <returns>The raw data to be sent back to the server (Obviously this will always be null OR SHOULD BE).</returns>
        public string[] OnDisconnect(int sid, string message) {
            if ( Disconnect == null )
                return null;
            DisconnectEvent de = new DisconnectEvent();
            de.sid = sid;
            de.message = message;
            InvocationTunnel( Disconnect.GetInvocationList(), de, "onDisconnectData" );
            return null; //Don't even give them the option to return anything to "send to the server".
        }

        /// <summary>
        /// Fires the OnNickName event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="u">The user that changed their nick (post-change).</param>
        /// <param name="oldnick">The old nickname.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnNickName(int sid, User u, string oldnick) {
            if ( NickName == null )
                return null;
            NickNameEvent ne = new NickNameEvent();
            ne.sid = sid;
            ne.u = u;
            ne.oldNick = oldnick;
            return InvocationTunnel( NickName.GetInvocationList(), ne, "onNickData",
                ( u.UserAttributes == null ) ? null : u.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the OnWelcome event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="message">The message.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnWelcome( int sid, string message ) {
            if ( Welcome == null )
                return null;
            WelcomeEvent we = new WelcomeEvent();
            we.sid = sid;
            we.message = message;
            return InvocationTunnel( Welcome.GetInvocationList(), we, "OnWelcomeData" );
        }

        /// <summary>
        /// Fires the OnBotNickName event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="newnick">The new nickname.</param>
        /// <param name="oldnick">The old nickname.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnBotNickName(int sid, string newnick, string oldnick) {
            if ( BotNickName == null )
                return null;
            BotNickNameEvent ne = new BotNickNameEvent();
            ne.sid = sid;
            ne.newNick = newnick;
            ne.oldNick = oldnick;
            return InvocationTunnel( BotNickName.GetInvocationList(), ne, "onBotNickData" );
        }

        /// <summary>
        /// Fires the OnPart event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="cu">The channel user that parted.</param>
        /// <param name="c">The channel parted.</param>
        /// <param name="message">The message sent to the server.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnPart(int sid, ChannelUser cu, Channel c, string message) {
            if ( Part == null )
                return null;
            JoinPartEvent je = new JoinPartEvent();
            je.sid = sid;
            je.channel = c;
            je.user = cu;
            je.message = message;
            return InvocationTunnel( Part.GetInvocationList(), je, "onPartData",
                ( cu.InternalUser.UserAttributes == null ) ? null : cu.InternalUser.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the OnBartPart event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="c">The channel parted.</param>
        /// <param name="message">The message sent to the server.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnBotPart(int sid, Channel c, string message) {
            if ( BotJoin == null )
                return null;
            JoinPartEvent je = new JoinPartEvent();
            je.sid = sid;
            je.channel = c;
            je.user = null;
            je.message = message;
            return InvocationTunnel( BotPart.GetInvocationList(), je, "onPartData" );
        }

        /// <summary>
        /// Fires the OnQuit event.
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="u">The user that quit.</param>
        /// <param name="message">The message sent to the server.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnQuit(int sid, User u, string message) {
            if ( Quit == null )
                return null;
            QuitEvent qe = new QuitEvent();
            qe.serverId = sid;
            qe.u = u;
            qe.message = message;
            return InvocationTunnel( BotJoin.GetInvocationList(), qe, "onQuitData" );
        }

        /// <summary>
        /// Fires the OnNames event.
        /// </summary>
        /// <param name="sid">The Server the names are coming from.</param>
        /// <param name="channel">The channel the names were filled into.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnNames(int sid, Channel c) { //TODO: Give this it's own injector? PROBABLY NOT LOL
            if ( Names == null )
                return null;
            JoinPartEvent je = new JoinPartEvent();
            je.sid = sid;
            je.channel = c;
            je.user = null;
            return InvocationTunnel( Names.GetInvocationList(), je, "onNamesData" );
        }

        /// <summary>
        /// Fires the OnServerModeMessage event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="c">The channel.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnServerModeMessage(int sid, User u, string recipient, string modes) {
            if ( ServerModeMessage == null )
                return null;
            ServerModeMessageEvent umme = new ServerModeMessageEvent();
            umme.sid = sid;
            umme.user = u;
            umme.modes = modes;
            umme.receiver = recipient;
            return InvocationTunnel( ServerModeMessage.GetInvocationList(), umme, "onServerModeData" );
        }

        /// <summary>
        /// Fires the BotJoin event.
        /// </summary>
        /// <param name="sid">The server on which the bot has joined a channel.</param>
        /// <param name="channel">The channel which the bot has joined.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnBotJoin(int sid, Channel c) {
            if ( BotJoin == null )
                return null;
            JoinPartEvent je = new JoinPartEvent();
            je.sid = sid;
            je.channel = c;
            je.user = null;
            return InvocationTunnel( BotJoin.GetInvocationList(), je, "onJoinData" );
        }

        /// <summary>
        /// Fires the OnJoin event.
        /// </summary>
        /// <param name="sid">The server on which the user has joined a channel.</param>
        /// <param name="channel">The channel which the user has joined.</param>
        /// <param name="cu">The user which joined the channel.</param>
        /// <returns>The raw data to be sent to the server.</returns>
        public string[] OnJoin(int sid, Channel c, ChannelUser cu) {
            if ( Join == null )
                return null;
            JoinPartEvent je = new JoinPartEvent();
            je.sid = sid;
            je.channel = c;
            je.user = cu;
            return InvocationTunnel( Join.GetInvocationList(), je, "onJoinData",
                ( cu.InternalUser.UserAttributes == null ) ? null : cu.InternalUser.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the ChannelMessage event.
        /// </summary>
        /// <param name="sid">Server id.</param>
        /// <param name="channel">The channel the message was sent to.</param>
        /// <param name="channeluser">The userhost of the user who sent it.</param>
        /// <param name="text">The text contained in the message.</param>
        /// <returns>Raw data.</returns>
        public string[] OnChannelMessage(int sid, Channel channel, ChannelUser userhost, string text) {
            if (ChannelMessage == null)
                return null;
            ChannelMessageEvent me = new ChannelMessageEvent();
            me.sid = sid;
            me.channelUser = userhost;
            me.channel = channel;
            me.text = text;
            return InvocationTunnel(ChannelMessage.GetInvocationList(), me, "channelMessageData",
                ( userhost.InternalUser.UserAttributes == null ) ? null : userhost.InternalUser.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the UserMessage event.
        /// </summary>
        /// <param name="sid">Server id.</param>
        /// <param name="nick">The nick the message was sent to.</param>
        /// <param name="userhost">The userhost of the user who sent it.</param>
        /// <param name="text">The text contained in the message.</param>
        /// <returns>Raw data.</returns>
        public string[] OnUserMessage(int sid, string nick, User userhost, string text) {
            if (UserMessage == null)
                return null;
            UserMessageEvent ue = new UserMessageEvent();
            ue.sid = sid;
            ue.sender = userhost;
            ue.receiver = nick;
            ue.text = text;
            return InvocationTunnel(UserMessage.GetInvocationList(), ue, "userMessageData",
                ( userhost.UserAttributes == null ) ? null : userhost.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the ChannelMessage event.
        /// </summary>
        /// <param name="sid">Server id.</param>
        /// <param name="channel">The channel the message was sent to.</param>
        /// <param name="channeluser">The userhost of the user who sent it.</param>
        /// <param name="text">The text contained in the message.</param>
        /// <returns>Raw data.</returns>
        public string[] OnChannelCTCPMessage(int sid, Channel channel, ChannelUser userhost, string text) {
            if ( ChannelCTCPMessage == null )
                return null;
            ChannelMessageEvent me = new ChannelMessageEvent();
            me.sid = sid;
            me.channelUser = userhost;
            me.channel = channel;
            me.text = text;
            return InvocationTunnel( ChannelCTCPMessage.GetInvocationList(), me, "channelCTCPMessageData",
                ( userhost.InternalUser.UserAttributes == null ) ? null : userhost.InternalUser.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the UserMessage event.
        /// </summary>
        /// <param name="sid">Server id.</param>
        /// <param name="nick">The nick the message was sent to.</param>
        /// <param name="userhost">The userhost of the user who sent it.</param>
        /// <param name="text">The text contained in the message.</param>
        /// <returns>Raw data.</returns>
        public string[] OnUserCTCPMessage(int sid, string nick, User userhost, string text) {
            if ( UserCTCPMessage == null )
                return null;
            UserMessageEvent ue = new UserMessageEvent();
            ue.sid = sid;
            ue.sender = userhost;
            ue.receiver = nick;
            ue.text = text;
            return InvocationTunnel( UserCTCPMessage.GetInvocationList(), ue, "userCTCPMessageData",
                ( userhost.UserAttributes == null ) ? null : userhost.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the ChannelNotice event.
        /// </summary>
        /// <param name="sid">Server ID</param>
        /// <param name="channel">Channel on which the notice was sent to</param>
        /// <param name="user">User who sent the notice</param>
        /// <param name="text">/text that was in the notice</param>
        /// <returns>Raw data.</returns>
        public string[] OnChannelNotice( int sid, Channel channel, User user, string text ) {
            if ( ChannelNotice == null )
                return null;
            ChannelNoticeEvent cn = new ChannelNoticeEvent();
            cn.sid = sid;
            cn.channel = channel;
            cn.user = user;
            cn.text = text;
            return InvocationTunnel( ChannelNotice.GetInvocationList(), cn, "channelNoticeData",
                ( user.UserAttributes == null ) ? null : user.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the PrivateNotice event.
        /// </summary>
        /// <param name="sid">Server ID</param>
        /// <param name="user">User who sent the message.</param>
        /// <param name="text">Text in the notice.</param>
        /// <returns>Raw data.</returns>
        public string[] OnPrivateNotice( int sid, User user, string text ) {
            if ( PrivateNotice == null )
                return null;
            PrivateNoticeEvent pn = new PrivateNoticeEvent();
            pn.sid = sid;
            pn.user = user;
            pn.text = text;
            return InvocationTunnel( PrivateNotice.GetInvocationList(), pn, "privateNoticeData",
                ( user.UserAttributes == null ) ? null : user.UserAttributes.Privegeles );
        }

        /// <summary>
        /// Fires the ServerNotice event.
        /// </summary>
        /// <param name="sid">Server ID</param>
        /// <param name="text">Text in the notice.</param>
        /// <returns>Raw data.</returns>
        public string[] OnServerNotice( int sid, string text ) {
            if ( ServerNotice == null )
                return null;
            ServerNoticeEvent sn = new ServerNoticeEvent();
            sn.sid = sid;
            sn.text = text;
            return InvocationTunnel( ServerNotice.GetInvocationList(), sn, "serverNoticeData", null );
        }

        /// <summary>
        /// Fires the OnConnect event.
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <returns>Raw data.</returns>
        public string[] OnConnect( int sid ) {
            if ( Connect == null )
                return null;
            ConnectEvent ce = new ConnectEvent();
            ce.serverId = sid;
            return InvocationTunnel( Connect.GetInvocationList(), ce, "connectData" );
        }

        /// <summary>
        /// Fires OnPing Event
        /// </summary>
        /// <param name="sid">The server id.</param>
        /// <param name="postback">Raw data. ( postback )</param>
        /// <returns></returns>
        public string[] OnPing(int sid, string postback) {
            if (Ping == null)
                return null;
            PingEvent pe = new PingEvent();
            pe.postback = postback;
            pe.serverId = sid;

            return InvocationTunnel( Ping.GetInvocationList(), pe, "pingData" );
        }

        /// <summary>
        /// Invokes a list of functions with specified arguments.
        /// (Created to avoid code re-usage in the event-firers.)
        /// </summary>
        /// <param name="invocList">A list of functions to invoke.</param>
        /// <returns>A compilation of the returns from all the functions.</returns>
        private string[] InvocationTunnel(Delegate[] invocList) {
            return InvocationTunnel( invocList, null, null, null );
        }

        /// <summary>
        /// Invokes a list of functions with specified arguments.
        /// (Created to avoid code re-usage in the event-firers.)
        /// </summary>
        /// <param name="invocList">A list of functions to invoke.</param>
        /// <param name="injector">The injector data to inject.</param>
        /// <param name="fieldName">The field name of the data object to inject.</param>
        /// <returns>A compilation of the returns from all the functions.</returns>
        private string[] InvocationTunnel(Delegate[] invocList, object injector, string fieldName) {
            Type t = moduleProxy.ModuleInstance.GetType();
            FieldInfo injectorData = null;
            if ( injector != null ) {
                injectorData = t.GetField( fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.Static );
                injectorData.SetValue( moduleProxy.ModuleInstance, injector );
            }
            FieldInfo returnString = t.GetField( "returns", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.Static );

            //TODO: Evaluate if these locks were a good idea.
            lock ( moduleProxy.ModuleInstance ) {

                int n = invocList.Length;
                string[][] returns = new string[n][];

                for ( int i = 0; i < n; i++ ) {
                    //Null the return string to prevent multiple spams.
                    returnString.SetValue( moduleProxy.ModuleInstance, null );
                    AppDomain.CurrentDomain.DoCallBack( (CrossAppDomainDelegate)invocList[i] );
                    returns[i] = (string[])returnString.GetValue( moduleProxy.ModuleInstance );
                }

                //TODO:
                //WARNING: Performance issues? Every single event ever needinfg to get unrwapped into a single
                //sized array. Maybe evaluate where the slowdowns are later on.
                List<string> flatreturn = new List<string>( returns.Length );

                for ( int i = 0; i < returns.Length; i++ )
                    if ( returns[i] != null )
                        flatreturn.AddRange( returns[i] );

                return flatreturn.ToArray();

            }
        }

        /// <summary>
        /// Invokes a list of functions with specified arguments.
        /// (Created to avoid code re-usage in the event-firers.)
        /// </summary>
        /// <param name="invocList">A list of functions to invoke.</param>
        /// <param name="injector">The injector data to inject.</param>
        /// <param name="fieldName">The field name of the data object to inject.</param>
        /// <param name="p">A Privelege Container of the user attempting to call the function.</param>
        /// <returns>A compilation of the returns from all the functions.</returns>
        private string[] InvocationTunnel(Delegate[] invocList, object injector, string fieldName, PrivelegeContainer p ) {

            Type t = moduleProxy.ModuleInstance.GetType();
            FieldInfo injectorData = null;
            if ( injector != null ) {
                injectorData = t.GetField( fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetField | BindingFlags.Static );
                injectorData.SetValue( moduleProxy.ModuleInstance, injector );
            }
            FieldInfo returnString = t.GetField( "returns", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.Static | BindingFlags.SetField );

            //TODO: Evaluate if these locks were a good idea.
            lock ( moduleProxy.ModuleInstance ) {

                int n = invocList.Length;
                string[][] returns = new string[n][];

                for ( int i = 0; i < n; i++ ) {

                    bool continueWithCall = true;

                    if ( p == null || !p.HasPrivelege( Priveleges.SuperUser ) ) {

                        //Check method permissions, can we call it?
                        object[] privreq = invocList[i].Method.GetCustomAttributes( false );

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

                    //If we can continue with the call. Let's DO IT!
                    if ( continueWithCall ) {
                        //Null the return string to prevent multiple spams.
                        returnString.SetValue( moduleProxy.ModuleInstance, null );
                        AppDomain.CurrentDomain.DoCallBack( (CrossAppDomainDelegate)invocList[i] );
                        returns[i] = (string[])returnString.GetValue( moduleProxy.ModuleInstance );
                    }
                }

                //TODO:
                //WARNING: Performance issues? Every single event ever needinfg to get unrwapped into a single
                //sized array. Maybe evaluate where the slowdowns are later on.
                List<string> flatreturn = new List<string>( returns.Length );

                for ( int i = 0; i < returns.Length; i++ )
                    if ( returns[i] != null )
                        flatreturn.AddRange( returns[i] );

                return flatreturn.ToArray();

            }
        }

        #endregion

    }

}
