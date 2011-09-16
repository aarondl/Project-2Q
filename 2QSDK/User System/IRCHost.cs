using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace Project2Q.SDK.UserSystem {

    /// <summary>
    /// Glorified String class with wildcard matching functionality.
    /// </summary>
    [Serializable]
    public class IRCHost : IComparable<IRCHost> {

        #region Variables + Properties

        private string nick;
        private string hostname;
        private string fullhost;
        private string username;

        /// <summary>
        /// Deals with the nick portion of the nick!user@hostname.
        /// </summary>
        public string Nick {
            get { return nick; }
            set {
                FullHost = value + "!" + username + "@" + hostname;
            }
        }

        /// <summary>
        /// Deals with the hostname portion of the nick!user@hostname.
        /// </summary>
        public string Hostname {
            get { return hostname; }
            set {
                FullHost = nick + "!" + username + "@" + value;
            }
        }

        /// <summary>
        /// Contains the nick!user@hostname host format.
        /// Setting this property sets all properties.
        /// </summary>
        public string FullHost {
            get { return fullhost; }
            set {
                int at = -1;
                int ex = -1;
                int i = 0;
                int n = value.Length;
                while ( i < n && value[i] != '!' ) i++;
                if ( i < n ) ex = i++;
                while ( i < n && value[i] != '@' ) i++;
                if ( i < n ) at = i++;
                if ( ex < 0 || at < 0 )
                    throw new FormatException( "This is an incorrect host format." );
                this.nick = value.Substring( 0, ex );
                this.username = value.Substring( ex + 1, at - ex - 1 );
                this.hostname = value.Substring( at + 1, n - at - 1 );

                this.fullhost = value;
            }
        }

        /// <summary>
        /// Deals with the username portion of the nick!user@hostname.
        /// </summary>
        public string Username {
            get { return username; }
            set {
                FullHost = nick + "!" + value + "@" + hostname;
            }
        }

        /// <summary>
        /// True if this IRCHost contains members with wildcards present.
        /// </summary>
        public bool ContainsWildcards {
            get { return this.fullhost.IndexOfAny( new char[] { '*', '?' } ) > 0 ? true : false; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a totally wildcard host.
        /// </summary>
        public IRCHost() {
            FullHost = "*!*@*";
        }

        /// <summary>
        /// Parses a full host.
        /// </summary>
        /// <param name="fullhost">The full hostname of an IRC User.</param>
        public IRCHost(string fullhost) {
            FullHost = fullhost;
        }

        /// <summary>
        /// Constructs a host based on individual aspects of the host.
        /// </summary>
        /// <param name="nick">Nickname.</param>
        /// <param name="user">Username.</param>
        /// <param name="host">Hostname.</param>
        public IRCHost(string nick, string user, string host) {
            if ( nick == null || nick == string.Empty )
                nick = "*";
            if ( user == null || user == string.Empty )
                user = "*";
            if ( host == null || host == string.Empty )
                host = "*";
            FullHost = nick + "!" + user + "@" + host;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Matches a wildcard string to a normal string.
        /// </summary>
        /// <param name="s1">A wildcard string.</param>
        /// <param name="s2">A normal string.</param>
        /// <returns>Standard compare operator return.</returns>
        public static int WildcardCompare(string ws, string ns) {

            int wn = ws.Length;
            int nn = ns.Length;

            int i = 0, j = 0;

            int wstore = -1;
            bool lastSuccess = false;

            while ( i < wn && j < nn ) {

                char wc = ws[i], nc = ns[j];

                if ( wc == nc ) { i++; j++; lastSuccess = true; } //If a = b or a = ? eats b
                else if ( wc == '?' ) {
                    if ( wstore > 0 ) {
                        if ( !lastSuccess ) {
                            i = wstore;
                            j++;
                        }
                        wstore = -1;
                    }
                    lastSuccess = false;
                    i++; j++;
                }
                else if ( wc == '*' ) {

                    lastSuccess = false;

                    if ( i + 1 == wn ) { //There is no point in the form text*, the * will just eat the rest.
                        i = wn;
                        j = nn; //Sabotage our sanity check at the bottom.
                        break;
                    }

                    wstore = ++i;
                }
                else if ( wstore > 0 ) {

                    lastSuccess = false;

                    j++;
                    i = wstore; //go back to * + 1
                }
                else
                    if ( wc - nc > 0 )
                        return 1;
                    else if ( wc - nc < 0 )
                        return -1;

            }

            //If ws ran out before ns then it is -1
            if ( i == wn && j < nn )
                return -1;
            //If ns ran out before ws then it is 1
            if ( i < wn && j == nn && ws[i] != '*' )
                return 1;

            return 0;

        }

        #endregion

        #region Object class Overrides

        /// <summary>
        /// Uses string comparison on the fullhost.
        /// </summary>
        /// <param name="obj">The host to compare with.</param>
        /// <returns>True or false</returns>
        public override bool Equals(object obj) {
            IRCHost right = (IRCHost)obj;
            return right.FullHost.Equals( this.fullhost );
        }

        public override int GetHashCode() {
            return fullhost.GetHashCode();
        }

        public override string ToString() {
            return fullhost;
        }

        #endregion

        #region IComparable<IRCHost> Members

        /// <summary>
        /// Compare two hostnames to see if a match can be generated.
        /// </summary>
        /// <param name="other">The other hostname to compare to.</param>
        /// <returns>Standard compare operator return.</returns>
        public int CompareTo(IRCHost other) {

            bool thiswc = this.ContainsWildcards;
            bool thatwc = other.ContainsWildcards;

            if ( !thiswc && !thatwc )
                return this.fullhost.CompareTo( other.FullHost );
            if ( thiswc && thatwc )
                throw new ArgumentException( "Both strings cannot contain wildcards." );

            if ( thiswc )
                return WildcardCompare( this.fullhost, other.FullHost );
            else
                return WildcardCompare( other.FullHost, this.fullhost );

        }

        #endregion
    }

}
