using System;
using System.Collections.Generic;
using System.Text;

using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;

namespace Project2Q.SDK.Injections {

    /// <summary>
    /// Holds arguments for the NickName in use error.
    /// </summary>
    public struct Err_NickNameInUseEvent {
        public int sid;
        public string badnick;
    }

    /// <summary>
    /// Holds arguments for the Disconnect event.
    /// </summary>
    public struct DisconnectEvent {
        public int sid;
        public string message;
    }

    /// <summary>
    /// Holds Message Arguments!
    /// </summary>
    public struct MessageEvent {
        public int sid;
        public string sender;
        public string receiver;
        public string text;
    }

    /// <summary>
    /// The Nickname event args.
    /// </summary>
    public struct NickNameEvent {
        public int sid;
        public User u;
        public string oldNick;
    }

    /// <summary>
    /// The Welcome event args
    /// </summary>
    public struct WelcomeEvent {
        public int sid;
        public string message;
    }

    /// <summary>
    /// The BotNickName event args.
    /// </summary>
    public struct BotNickNameEvent {
        public int sid;
        public string newNick;
        public string oldNick;
    }

    /// <summary>
    /// The join/part event.
    /// For BotJoin/BotPart the ChannelUser item will be void.
    /// </summary>
    public struct JoinPartEvent {
        public int sid;
        public ChannelUser user;
        public Channel channel;
        public string message;
    }

    /// <summary>
    /// User message events.
    /// </summary>
    public struct UserMessageEvent {
        public int sid;
        public User sender;
        public string receiver;
        public string text;
    }

    /// <summary>
    /// User Modes Message Events.
    /// </summary>
    public struct ServerModeMessageEvent {
        public int sid;
        public User user;
        public string receiver;
        public string modes;
    }

    /// <summary>
    /// Channel message events.
    /// </summary>
    public struct ChannelMessageEvent {
        public int sid;
        public Channel channel;
        public ChannelUser channelUser;
        public string text;
    }

    /// <summary>
    /// Private notice events.
    /// </summary>
    public struct PrivateNoticeEvent {
        public int sid;
        public User user;
        public string text;
    }

    /// <summary>
    /// Channel notice events.
    /// </summary>
    public struct ChannelNoticeEvent {
        public int sid;
        public Channel channel;
        public User user;
        public string text;
    }

    /// <summary>
    /// Server notice events.
    /// </summary>
    public struct ServerNoticeEvent {
        public int sid;
        public string text;
    }

    /// <summary>
    /// Holds Ping arguments.
    /// </summary>
    public struct PingEvent {
        public int serverId;
        public string postback;
    }

    /// <summary>
    /// The Quit message event.
    /// </summary>
    public struct QuitEvent {
        public int serverId;
        public User u;
        public string message;
    }

    /// <summary>
    /// The Connect event.
    /// </summary>
    public struct ConnectEvent {
        public int serverId;
    }

}
