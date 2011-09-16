using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace Project2Q.SDK.UserSystem {

    /// <summary>
    /// Represents attributes only a registered user would have.
    /// </summary>
    [Serializable]
    public class RegisteredUser {

        private PrivelegeContainer p;
        private List<IRCHost> hostList;

        /// <summary>
        /// Gets or Sets the priveleges object for this registered user.
        /// </summary>
        public PrivelegeContainer Privegeles {
            get { return p; }
            set { p = value; }
        }

        /// <summary>
        /// Gets or Sets the HostList object for this registered user.
        /// </summary>
        public List<IRCHost> HostList {
            get { return hostList; }
            set { hostList = value; }
        }

        /// <summary>
        /// Creates a Registered User.
        /// </summary>
        public RegisteredUser() {
            hostList = new List<IRCHost>( 2 );
            p = new PrivelegeContainer();
        }

        /// <summary>
        /// Checks to see if this registered user can authenticate by the host.
        /// </summary>
        /// <param name="host">The host to check for.</param>
        /// <returns>Yes or no</returns>
        public bool HasHost(IRCHost host) {
            foreach ( IRCHost i in hostList )
                if ( host.CompareTo( i ) == 0 )
                    return true;
            return false;
        }

        /// <summary>
        /// Adds a host to the registered user's host list.
        /// </summary>
        /// <param name="host">The host to add.</param>
        /// <returns>True or false.</returns>
        public bool AddHost(IRCHost host) {
            if ( host.Equals( new IRCHost( "*!*@*" ) ) )
                return false;
            hostList.Add( host );
            return true;
        }

        /// <summary>
        /// Removes a host from the registered user's host list.
        /// </summary>
        /// <param name="host">The host to remove.</param>
        /// <returns>True or false.</returns>
        public bool RemoveHost(IRCHost host) {
            return hostList.Remove( host );
        }

    }

}
