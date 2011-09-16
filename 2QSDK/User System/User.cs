using System;
using System.Collections.Generic;
using System.Text;

namespace Project2Q.SDK.UserSystem {

    /// <summary>
    /// Describes an online user.
    /// </summary>
    public class User {

        /// <summary>
        /// Creates a User.
        /// </summary>
        /// <param name="current">The current hostname of this user.</param>
        public User(IRCHost current) {
            ru = null;
            currentHost = current;
        }

        /// <summary>
        /// Creates a User associated with a Registered User.
        /// </summary>
        /// <param name="current">The current hostname of this user.</param>
        /// <param name="ru">The associated registered user.</param>
        public User(IRCHost current, RegisteredUser ru) {
            this.ru = ru;
            currentHost = current;
        }

        #region Variables and Properties

        private RegisteredUser ru;
        private IRCHost currentHost;

        /// <summary>
        /// Returns this users nickname.
        /// </summary>
        public string Nickname {
            get { return currentHost.Nick; }
        }

        /// <summary>
        /// Returns this users username.
        /// </summary>
        public string Username {
            get { return currentHost.Username; }
        }

        /// <summary>
        /// Returns this users hostname.
        /// </summary>
        public string Hostname {
            get { return currentHost.Hostname; }
        }

        /// <summary>
        /// Gets or Sets the Current Host.
        /// </summary>
        public IRCHost CurrentHost {
            get { return currentHost; }
            set { currentHost = value; }
        }

        /// <summary>
        /// Returns the registered user associated with the current user.
        /// </summary>
        public RegisteredUser UserAttributes {
            get { return ru; }
            set { ru = value; }
        }

        #endregion



    }

}
