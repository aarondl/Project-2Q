using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Project2Q.Core {

    /// <summary>
    /// Reads in and exposes Users from a file in RCon format.
    /// </summary>
    internal sealed class RconUserFile : IEnumerable<RconUserFile.RconUser>, IDisposable {

        #region Statics

        /// <summary>
        /// Converts an integer to a hexadecimal character.
        /// </summary>
        /// <param name="b">An unsigned hexadecimal character. (0-15 range)</param>
        /// <returns>The hexadecimal character equivalent.</returns>
        public static char DecToHex(int b) {
            if ( b > 15 || b < 0 ) throw new FormatException( "Not a hexadecimal value" );
            if ( b >= 10 ) {
                return (char)( 'a' + ( b - 10 ) );
            }
            else
                return (char)( '0' + b );
        }

        /// <summary>
        /// Converts a digest into a hash.
        /// </summary>
        /// <param name="digest">The digest to convert.</param>
        /// <returns>An ascii representation of the digest.</returns>
        public static string GetHashFromDigest(byte[] digest) {
            StringBuilder sb = new StringBuilder();
            for ( int i = 0; i < 16; i++ ) {
                sb.AppendFormat( "{0}{1}",
                    DecToHex( digest[i] / 16 ),
                    DecToHex( digest[i] % 16 ) );
            }
            return sb.ToString();
        }

        #endregion

        #region Variables + Properties

        private FileStream fs;
        private Dictionary<string,RconUser> users;
        private bool hasRootUser;

        /// <summary>
        /// Gets a value depicting if the file has a root user defined.
        /// </summary>
        public bool HasRootUser {
            get { return hasRootUser; }
        }

        #endregion

        #region Constructor
        
        /// <summary>
        /// Creates a users file and adds the first user.
        /// </summary>
        /// <param name="filename">The file to create.</param>
        /// <param name="firstUser">The first user to be put in this file (must be root user).</param>
        public RconUserFile(string filename, RconUserFile.RconUser firstUser) {
            fs = new FileStream( filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None );

            if ( firstUser.UserAccess != RconUser.RootLevel )
                throw new ArgumentException( "First user must be a root user." );

            users = new Dictionary<string, RconUser>( 20 );
            users.Add( firstUser.UserName, firstUser );
        }

        /// <summary>
        /// Reads in an existing users file and exposes the users in code.
        /// </summary>
        /// <param name="filename">The file to read in.</param>
        public RconUserFile(string filename) {
            //This'll blow up in their face if they specify a bad file
            fs = new FileStream( filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None );

            if ( fs.Length == 0 )
                throw new FormatException( "RconUser file not initialized." );

            users = new Dictionary<string, RconUser>( 20 );

            ReadInAllUsers();

        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the classes internal user list.
        /// </summary>
        public void ReadInAllUsers() {
            users.Clear();
            fs.Seek(0, SeekOrigin.Begin );

            //Format:
            //Read Int (Strlen of next str)
            //Read Strlen chars for username
            //Read Int (Access level)
            //Read 16 bytes, password digest.

            //We sure need a lot of memory for this little operation ><
            int recordSize;
            RconUser rc;
            int readBytes = 0;
            int accessLevel = 0;
            string username;
            byte[] intArray = new byte[sizeof( Int32 )];
            byte[] data = new byte[256];

            //This will probably throw an End of Stream if it's an empty file.
            readBytes = fs.Read( intArray, 0, sizeof( Int32 ) );
            if ( readBytes <= 0 )
                throw new FormatException( "RconUser file format error." );
            recordSize = BitConverter.ToInt32( intArray, 0 );

            while ( recordSize > 0 ) {

                readBytes = fs.Read( data, 0, recordSize + sizeof( Int32 ) + 32 ); //20 bytes to get the access level and password digest
                if ( readBytes != recordSize + sizeof( Int32 ) + 32 ) 
                    throw new FormatException( "Something messed up in the User file!" );

                username = IRCProtocol.Ascii.GetString( data, 0, recordSize );
                accessLevel = BitConverter.ToInt32( data, recordSize );

                if ( accessLevel == 11 )
                    if ( this.hasRootUser ) throw new FormatException( "Invalid format, has two root users." );
                    else this.hasRootUser = true;

                rc = new RconUser(
                username, //The username
                IRCProtocol.Ascii.GetString( data, recordSize + sizeof( Int32 ), 32 ), //The password hash digest (16 bytes)
                accessLevel); //The access level

                users.Add( username, rc );

                if ( fs.Length == readBytes ) break; //We've reached the end (I think).

                //Get Next Record
                intArray = new byte[sizeof( Int32 )];
                fs.Read( intArray, 0, sizeof( Int32 ) );
                recordSize = BitConverter.ToInt32( intArray, 0 );
            }
        }

        /// <summary>
        /// Writes all users to the file.
        /// </summary>
        public void Commit() {

            fs.Seek(0, SeekOrigin.Begin );
            fs.SetLength(0);

            Dictionary<string, RconUser>.Enumerator e = users.GetEnumerator();

            int recordSize = 0;

            while ( e.MoveNext() ) {
                recordSize = e.Current.Value.UserName.Length;
                fs.Write( BitConverter.GetBytes( recordSize ), 0, sizeof( Int32 ) );
                fs.Write( IRCProtocol.Ascii.GetBytes( e.Current.Value.UserName ), 0, recordSize );
                fs.Write( BitConverter.GetBytes( e.Current.Value.UserAccess ), 0, sizeof( Int32 ) );
                fs.Write( IRCProtocol.Ascii.GetBytes( e.Current.Value.Password ), 0, 32 );
            }
        }

        /// <summary>
        /// Retrives the user by the name.
        /// </summary>
        /// <param name="username">The username of the user struct to return.</param>
        /// <returns>A nullable version of RconUser. If it's null, the user was not in the dictionary.</returns>
        public Nullable<RconUser> GetUser(string username) {
            Nullable<RconUser> n;
            try {
                n = users[username];
            }
            catch ( KeyNotFoundException ) {
                n = null;
            }
            return n;
        }

        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user, if unhashed specify hashPassword as true.</param>
        /// <param name="userAccess">The users access. This value must be between RConUser.MaxValue/MinValue.</param>
        /// <param name="hashPassword">Do we hash the password we received?</param>
        /// <returns>If the add was successful, failure is a result of a second root user being added.</returns>
        public bool AddUser(string username, string password, int userAccess, bool hashPassword) {

            if ( ( userAccess == RconUser.RootLevel && this.hasRootUser ) || users.ContainsKey(username) )
                return false;

            if ( hashPassword ) {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                md5.Initialize();
                byte[] pass = IRCProtocol.Ascii.GetBytes( password );
                md5.TransformFinalBlock(pass, 0, pass.Length);
                password = GetHashFromDigest( md5.Hash ); //Save as digest.
            }
            
            users.Add( username, new RconUser( username, password, userAccess ) );

            return true;
        }

        /// <summary>
        /// Removes a user from the users file.
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Success?</returns>
        public bool RemoveUser(string username) {
            if ( !users.ContainsKey( username ) )
                return false;
            users.Remove( username );
            return true;
        }

        /// <summary>
        /// Renames a user from the users file.
        /// </summary>
        /// <param name="username">The User to rename.</param>
        /// <param name="newUserName">The new user name.</param>
        /// <returns>Success?</returns>
        public bool RenameUser(string username, string newUserName) {
            if ( !users.ContainsKey( username ) )
                return false;
            RconUser rctoReplace = users[username];
            RconUser rcReplace = new RconUser(
                newUserName,
                rctoReplace.Password,
                rctoReplace.UserAccess );
            users.Remove( username );
            users.Add( newUserName, rcReplace );
            return true;
        }

        /// <summary>
        /// Retrieves the user password hash by the name.
        /// </summary>
        /// <param name="username">The username of whom to get the password hash.</param>
        /// <returns>Password hash of the user requested. Null if user was not found.</returns>
        public string GetUserPassword(string username) {
            try {
                return users[username].Password;
            }
            catch ( KeyNotFoundException ) {
                return null;
            }
        }

        /// <summary>
        /// Retrieves the User Access of the requested user name.
        /// </summary>
        /// <param name="username">The username of whom to get the access level.</param>
        /// <returns>The access level. Returns -1 if user does not exist.</returns>
        public int GetUserAccess(string username) {
            try {
                return users[username].UserAccess;
            }
            catch ( KeyNotFoundException ) {
                return -1;
            }
        }

        #endregion

        #region RConUser Struct

        /// <summary>
        /// Used to define an RCon User.
        /// </summary>
        internal struct RconUser {
            /// <summary>
            /// Creates an RConUser
            /// </summary>
            /// <param name="uname">Username</param>
            /// <param name="passhash">Password Hash</param>
            /// <param name="access">Access</param>
            public RconUser(string uname, string passhash, int access) {
                if ( access < RconUser.MinLevel || access > RconUser.MaxLevel )
                    throw new ArgumentOutOfRangeException( "Access must be between RconUser.MaxLevel and RconUser.MinLevel." );
                this.username = uname;
                this.passhash = passhash;
                this.userlevel = access;
            }
            /// <summary>
            /// Gives a value to the Root user level, this field is readonly.
            /// </summary>
            public readonly static int RootLevel = 11;
            /// <summary>
            /// The minimum level of access a user can have.
            /// </summary>
            public readonly static int MinLevel = 0;
            /// <summary>
            /// The maximum level of access a user can have.
            /// </summary>
            public readonly static int MaxLevel = RootLevel;
            /// <summary>
            /// Returns the users username.
            /// </summary>
            public string UserName {
                get { return username; }
            }
            /// <summary>
            /// Returns a password hash of the user.
            /// </summary>
            public string Password {
                get { return passhash; }
            }
            /// <summary>
            /// Returns an integer depicting their access level inside the RConsole.
            /// </summary>
            public int UserAccess {
                get { return userlevel; }
            }
            private string username;
            private string passhash;
            private int userlevel;
        }

        #endregion

        #region IEnumerable<RconUserFile.RconUser> Members

        /// <summary>
        /// Gets a templated enumerator.
        /// </summary>
        /// <returns>The templated enumerator to use.</returns>
        public IEnumerator<RconUserFile.RconUser> GetEnumerator() {
            return new RconUserFileIterator(users);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)(new RconUserFileIterator( users ));
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases the user file handle.
        /// </summary>
        public void Dispose() {
            fs.Close();
        }

        #endregion
    }

    #region RconUserFileIterator

    /// <summary>
    /// Used to enumerate through an RconUserFile.
    /// </summary>
    internal sealed class RconUserFileIterator : IEnumerator<RconUserFile.RconUser> {

        private Dictionary<string, RconUserFile.RconUser>.Enumerator enumerator;

        public RconUserFileIterator(Dictionary<string,RconUserFile.RconUser> users) {
            enumerator = users.GetEnumerator();
        }

        #region IEnumerator<KeyValuePair<string,string>> Members

        /// <summary>
        /// Returns an RconUser struct.
        /// </summary>
        public RconUserFile.RconUser Current {
            get { return enumerator.Current.Value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Returns an RconUser struct.
        /// </summary>
        object IEnumerator.Current {
            get { return (object)enumerator.Current; }
        }

        /// <summary>
        /// Moves to the next in the list.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext() {
            return enumerator.MoveNext();
        }

        public void Reset() {
            throw new Exception( "Can't reset, jokes on you." );
        }

        #endregion
    }

    #endregion

}
