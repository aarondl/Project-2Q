using System;
using System.Collections.Generic;
using System.Text;

using Project2Q.SDK.UserSystem;

namespace Project2Q.SDK.ChannelSystem {
    
    /// <summary>
    /// Holds additional information for a user on a channel.
    /// </summary>
    public class ChannelUser {

        /// <summary>
        /// Creates a channeluser with an internal user and a flag.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="flag"></param>
        public ChannelUser(User u, Nullable<char> flag) {
            internalUser = u;
            userFlag = flag;
        }

        /// <summary>
        /// Creates a channeluser with everything nulled.
        /// </summary>
        public ChannelUser() {
            internalUser = null;
            userFlag = null;
        }

        private User internalUser;
        private Nullable<char> userFlag;

        /// <summary>
        /// Gets or sets the internal user related to the channeluser.
        /// </summary>
        public User InternalUser {
            get { return internalUser; }
            set { internalUser = value; }
        }

        /// <summary>
        /// Gets or sets the user flag.
        /// </summary>
        public Nullable<char> UserFlag {
            get { return userFlag; }
            set { userFlag = value; }
        }

        /// <summary>
        /// Checks if the user is voiced on the channel.
        /// </summary>
        public bool IsVoice {
            get { return userFlag == '@' || userFlag == '+'; }
        }

        /// <summary>
        /// Checks if the user is opped on the channel.
        /// </summary>
        public bool IsOp {
            get { return userFlag == '@'; }
        }

    }

}
