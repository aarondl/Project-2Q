using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;

namespace Project2Q.SDK.CollectionEnumerators {

    #region UserCollection

    /// <summary>
    /// Enumerates through the UserCollection.
    /// </summary>
    public sealed class UserCollectionEnumerator : IEnumerator<User> {

        public UserCollectionEnumerator(Dictionary<string, User>.Enumerator d) {
            dictionary = d;
        }

        private Dictionary<string, User>.Enumerator dictionary;

        #region IEnumerator<User> Members

        public User Current {
            get {
                return dictionary.Current.Value;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            dictionary.Dispose();
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
            get {
                return dictionary.Current.Value;
            }
        }

        public bool MoveNext() {
            return dictionary.MoveNext();
        }

        public void Reset() {
            throw new NotSupportedException();
        }

        #endregion
    }

    #endregion

    #region ChannelCollection

    public sealed class ChannelCollectionEnumerator : IEnumerator<Channel> {

        private Dictionary<string, Channel>.Enumerator cce;

        public ChannelCollectionEnumerator(Dictionary<string, Channel>.Enumerator cce) {
            this.cce = cce;
        }

        #region IEnumerator<Channel> Members

        public Channel Current {
            get { return cce.Current.Value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            cce.Dispose();
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
            get { return cce.Current.Value; }
        }

        public bool MoveNext() {
            return cce.MoveNext();
        }

        public void Reset() {
            throw new NotSupportedException();
        }

        #endregion
    }

    #endregion

    #region Channel

    /// <summary>
    /// Enumerates through a Channel.
    /// </summary>
    public sealed class ChannelEnumerator : IEnumerator<ChannelUser> {

        #region IEnumerator<ChannelUser> Members

        private Dictionary<string, ChannelUser>.Enumerator i;

        public ChannelEnumerator(Dictionary<string, ChannelUser>.Enumerator i) {
            this.i = i;
        }

        public ChannelUser Current {
            get {
                return i.Current.Value;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            i.Dispose();
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
            get { return i.Current.Value; }
        }

        public bool MoveNext() {
            return i.MoveNext();
        }

        public void Reset() {
            throw new NotSupportedException();
        }

        #endregion
    }

    #endregion

}
