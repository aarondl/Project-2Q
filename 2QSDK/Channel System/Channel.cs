using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using Project2Q.SDK.UserSystem;
using Project2Q.SDK.CollectionEnumerators;

namespace Project2Q.SDK.ChannelSystem {

    /// <summary>
    /// Contains a list of modes, users, bans for a given channel on a network.
    /// </summary>
    public class Channel : IEnumerable<ChannelUser> {

        private Dictionary<string, ChannelUser> userlist;
        private string modestring; //Unused for now.
        private List<IRCHost> bans; //Unused for now.
        private string name;
        private string topic;

        /// <summary>
        /// Constructs a channel by providing a name.
        /// </summary>
        /// <param name="name">Channel name.</param>
        public Channel(string name) 
        : this(name, null, null) {
        }

        /// <summary>
        /// Constructs a channel by providing a name, userlist, and associated modes.
        /// </summary>
        /// <param name="name">Channel name.</param>
        /// <param name="userlist">A list of users to add.</param>
        /// <param name="userModes">The user channel-modes in a parallel array to the userlist.</param>
        public Channel(string name, User[] userlist, Nullable<char>[] userModes) {

            this.userlist = new Dictionary<string, ChannelUser>();

            this.name = name;

            if ( userlist != null ) {
                for ( int i = 0; i < userlist.Length; i++ ) {
                    ChannelUser cu = new ChannelUser( userlist[i], userModes[i] );
                    this.userlist.Add( userlist[i].Nickname, cu );
                }
            }

        }

        /// <summary>
        /// Returns the number of users on the channel.
        /// </summary>
        public int Count {
            get { return userlist.Count; }
        }

        /// <summary>
        /// Retrieves a user from the database.
        /// </summary>
        /// <param name="nick">The nickname of the user to lookup.</param>
        /// <returns>The user associated with the nick.</returns>
        public ChannelUser this[string nick] {
            get { return userlist[nick]; }
        }

        /// <summary>
        /// Is this user on this channel?
        /// </summary>
        /// <param name="nick">The nick to check for.</param>
        public bool HasNick(string nick) {
            return userlist.ContainsKey( nick );
        }

        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        /// <param name="u">User to add</param>
        /// <param name="modeChar">The mode character for the user in this channel.</param>
        public void AddUser(User u, Nullable<char> modeChar) {
            ChannelUser cu = new ChannelUser( u, modeChar );
            this.userlist.Add( u.Nickname, cu );
        }

        /// <summary>
        /// Adds a user to the database.
        /// For use with the /names event for adding users void of internalusers to the channel.
        /// </summary>
        /// <param name="nick">The nick of the user to add.</param>
        /// <param name="cu">User to add</param>
        public void AddUser(string nick, ChannelUser cu) {
            this.userlist.Add( nick, cu );
        }

        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        /// <param name="nick">The nick of the user to add.</param>
        /// <param name="cu">User to add</param>
        public void AddUser(ChannelUser cu) {
            this.userlist.Add( cu.InternalUser.Nickname, cu );
        }

        /// <summary>
        /// Replaces a users nickname within the user database.
        /// </summary>
        /// <param name="cu">The user nickname to replace.</param>
        /// <param name="u">The user to replace it with.</param>
        public void ReplaceUser(string cu, ChannelUser u) {
            this.userlist.Remove( cu );
            this.userlist.Add( u.InternalUser.Nickname, u );
        }

        /// <summary>
        /// Removes a user from the channel.
        /// </summary>
        /// <param name="cu">The channeluser to remove.</param>
        public void RemoveUser(ChannelUser cu) {
            this.userlist.Remove( cu.InternalUser.Nickname );
        }

        /// <summary>
        /// Removes a user from the channel.
        /// </summary>
        /// <param name="nick">The nickname of the user to remove.</param>
        public void RemoveUser(string nick) {
            this.userlist.Remove( nick );
        }

        /// <summary>
        /// Removes a user from the channel.
        /// </summary>
        /// <param name="u">The user to remove.</param>
        public void RemoveUser(User u) {
            this.userlist.Remove( u.Nickname );
        }

        /// <summary>
        /// Erases all entries in the database.
        /// </summary>
        public void RemoveAll() {
            userlist.Clear();
        }

        /// <summary>
        /// Gets or Sets the channel name.
        /// </summary>
        public string Name {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or Sets the topic.
        /// </summary>
        public string Topic {
            get { return topic; }
            set { topic = value; }
        }

        #region IEnumerable<User> Members

        IEnumerator<ChannelUser> IEnumerable<ChannelUser>.GetEnumerator() {
            return new ChannelEnumerator( userlist.GetEnumerator() );
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return new ChannelEnumerator( userlist.GetEnumerator() );
        }

        #endregion
    }



}
