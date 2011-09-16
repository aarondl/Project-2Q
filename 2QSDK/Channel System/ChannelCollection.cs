using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using Project2Q.SDK.CollectionEnumerators;
using Project2Q.SDK.UserSystem;

namespace Project2Q.SDK.ChannelSystem {

    /// <summary>
    /// Contains a collection of channels on a given network.
    /// </summary>
    public class ChannelCollection : IEnumerable<Channel> {

        private Dictionary<string, Channel> chandb;

        public ChannelCollection() {
            chandb = new Dictionary<string, Channel>(5);
        }

        /// <summary>
        /// Returns the number of channels in the database.
        /// </summary>
        public int Count {
            get { return chandb.Count; }
        }

        /// <summary>
        /// Adds a channel to the collection.
        /// </summary>
        /// <param name="c">The channel to add.</param>
        public void AddChannel(Channel c) {
            chandb.Add( c.Name, c );
        }

        /// <summary>
        /// Removes a channel from the collection.
        /// </summary>
        /// <param name="c">The channel to remove.</param>
        public void RemoveChannel(Channel c) {
            chandb.Remove( c.Name );
        }

        /// <summary>
        /// Removes a channel from the collection.
        /// </summary>
        /// <param name="c">The name of the channel to remove.</param>
        public void RemoveChannel(string name) {
            chandb.Remove( name );
        }

        /// <summary>
        /// Erases all entries in the database.
        /// </summary>
        public void RemoveAll() {
            chandb.Clear();
        }

        /// <summary>
        /// Gets the Channel associated with the name.
        /// </summary>
        /// <param name="name">Name of channel.</param>
        /// <returns>The channel associated with the name.</returns>
        public Channel this[string name] {
            get { return chandb[name]; }
        }

        #region IEnumerable<Channel> Members

        IEnumerator<Channel> IEnumerable<Channel>.GetEnumerator() {
            return new ChannelCollectionEnumerator( chandb.GetEnumerator() );
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return new ChannelCollectionEnumerator( chandb.GetEnumerator() );
        }

        #endregion
    }

}
