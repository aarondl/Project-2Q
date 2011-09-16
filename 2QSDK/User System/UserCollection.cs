using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

using Project2Q.SDK.CollectionEnumerators;

namespace Project2Q.SDK.UserSystem {

    public class UserCollection : IEnumerable<User> {

        #region Variables

        private Dictionary<string, User> userdb;
        private List<RegisteredUser> ruserdb;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a user collection.
        /// </summary>
        public UserCollection() {
            userdb = new Dictionary<string, User>( 100 );
            ruserdb = null;
        }

        #endregion

        /// <summary>
        /// Returns the number of users active on the server.
        /// </summary>
        public int ActiveCount {
            get { return userdb.Count; }
        }

        /// <summary>
        /// Returns the number of registered users in the database.
        /// </summary>
        public int RegisteredCount {
            get { return ruserdb.Count; }
        }

        #region Methods

        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        /// <param name="u">The user to add.</param>
        public void AddUser(User u) {
            if ( userdb.ContainsKey( u.Nickname ) )
                throw new Exception( "NICK COLLISION ON AN IRC SERVER? OR POOR MANAGEMENT ON OUR PART?" );
            userdb.Add( u.Nickname, u );
        }

        /// <summary>
        /// Removes a user from the database.
        /// </summary>
        /// <param name="u">The user to remove.</param>
        public void RemoveUser(User u) {
            userdb.Remove( u.Nickname );
        }

        /// <summary>
        /// Removes a user from the database.
        /// </summary>
        /// <param name="nick">The nick of the user to remove.</param>
        public void RemoveUser(string nick) {
            userdb.Remove( nick );
        }

        /// <summary>
        /// Erases all entries in the database.
        /// </summary>
        public void RemoveAll() {
            userdb.Clear();
        }

        /// <summary>
        /// Add a registered user to the database.
        /// </summary>
        /// <param name="ru">The registered user to add.</param>
        public void AddRegisteredUser(RegisteredUser ru) {
            ruserdb.Add( ru );
        }

        /// <summary>
        /// Removes a registered user from the database.
        /// WARNING: This is a sequential lookup, very slow!
        /// </summary>
        /// <param name="u">The user to hostmatch to remove with.</param>
        public void RemoveRegisteredUser(User u) {
            for ( int i = 0; i < ruserdb.Count; i++ ) {
                foreach ( IRCHost irch in ruserdb[i].HostList ) {
                    if ( IRCHost.WildcardCompare( irch.FullHost, u.CurrentHost.FullHost ) == 0 ) {
                        u.UserAttributes = null; //In case someone tries to get it again.
                        ruserdb.RemoveAt( i );
                    }
                }
            }
        }

        /// <summary>
        /// Finds and attaches a RegisteredUser if any that will match the host of the User.
        /// WARNING: This is a sequential lookup, very slow!
        /// </summary>
        /// <param name="u">The user to use to match hosts with.</param>
        /// <returns>The registered user that matched.</returns>
        public RegisteredUser Authenticate(User u) {
            return Authenticate( u, true );
        }
        
        /// <summary>
        /// Finds and conditionally attaches a RegisteredUser if any that will match the host of the User.
        /// WARNING: This is a sequential lookup, very slow!
        /// </summary>
        /// <param name="u">The user to use to match hosts with.</param>
        /// <param name="attach">Attach the RU to the U?</param>
        /// <returns>The registered user that matched.</returns>
        public RegisteredUser Authenticate(User u, bool attach) {
            foreach ( RegisteredUser ru in ruserdb ) {
                foreach ( IRCHost irch in ru.HostList ) {
                    if ( IRCHost.WildcardCompare( irch.FullHost, u.CurrentHost.FullHost ) == 0 ) { //We have a match.
                        if ( attach )
                            u.UserAttributes = ru;
                        return ru;
                    }
                }
            }
            u.UserAttributes = null;
            return null;
        }

        /// <summary>
        /// Checks to see if the host exists inside the registered user database.
        /// </summary>
        /// <param name="irch">IRC Host to check for.</param>
        /// <returns>True or False.</returns>
        public bool HasHost(IRCHost irch) {
            foreach ( RegisteredUser ru in ruserdb )
                foreach ( IRCHost irchc in ru.HostList )
                    if ( irchc.Equals( irch ) )
                        return true;
            return false;
        }

        /// <summary>
        /// Loads the registered users list from a file.
        /// </summary>
        /// <param name="filename">The filename of the database file.</param>
        public void LoadRegisteredUsers(string filename) {
            FileStream fs = null;

            try {
                fs = new FileStream( filename, FileMode.Open, FileAccess.ReadWrite );
                BinaryFormatter bf = new BinaryFormatter();
                ruserdb = (List<RegisteredUser>)bf.Deserialize( fs );   
            }
            catch ( FileNotFoundException ) {
                ruserdb = new List<RegisteredUser>( 5 );
            }
            finally {
                if ( fs != null )
                    fs.Close();
            }
        }

        /// <summary>
        /// Saves the registered users list to a file.
        /// </summary>
        /// <param name="filename">The filename of the database file to write to.</param>
        public void SaveRegisteredUsers(string filename) {
            FileStream fs = new FileStream( filename, FileMode.Create, FileAccess.Write );
            try {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize( fs, ruserdb );
            }
            finally {
                fs.Close();
            }
        }

        /// <summary>
        /// Retrieves a user from the user database.
        /// </summary>
        /// <param name="host">The current host of the user object to return.</param>
        /// <returns>The user object.</returns>
        public User this[string nick] {
            get { return userdb[nick]; }
        }

        #endregion

        #region IEnumerable<User> Members

        IEnumerator<User> IEnumerable<User>.GetEnumerator() {
            return new UserCollectionEnumerator( userdb.GetEnumerator() );
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new UserCollectionEnumerator( userdb.GetEnumerator() );
        }

        #endregion
    }

}
