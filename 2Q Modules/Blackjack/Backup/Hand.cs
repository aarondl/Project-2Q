using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack {

    /// <summary>
    /// Sum types for the getsum method.
    /// </summary>
    public enum SumType {
        /// <summary>
        /// Assumes all aces are high.
        /// </summary>
        High,
        /// <summary>
        /// Assumes all aces are low.
        /// </summary>
        Low,
        /// <summary>
        /// Sums the highest possible, changing aces to get the highest value without going over 21.
        /// </summary>
        Smart,
    }

    public class Hand : IEnumerable<byte> {

        private byte[] cards;
        private int cardIndex;

        public Hand() {
            //In the game of blackjack, no one can ever have more than 10 cards, or they will bust.
            cards = new byte[10];
            cardIndex = 0;
        }

        /// <summary>
        /// Retrieves or Sets a card at index from the hand.
        /// </summary>
        /// <param name="index">The index to get the card from.</param>
        /// <returns>Returns the card at index.</returns>
        public byte this[int index] {
            get {
                if ( index >= cardIndex )
                    throw new Exception( "That index is not part of this deck." );
                return cards[index];
            }
            set {
                if ( index >= cardIndex )
                    throw new Exception( "That index is not part of this deck." );
                cards[index] = value;
            }
        }

        /// <summary>
        /// Returns the first card in the list.
        /// </summary>
        public byte First {
            get {
                if ( cardIndex == 0 )
                    throw new Exception( "Hand is Empty." );
                return cards[0];
            }
        }

        /// <summary>
        /// Returns the last card in the list.
        /// </summary>
        public byte Last {
            get {
                if ( cardIndex == 0 )
                    throw new Exception( "Hand is empty." );
                return cards[cardIndex - 1];
            }
        }

        /// <summary>
        /// Resets this hand.
        /// </summary>
        public void ResetHand() {
            cardIndex = 0;
        }

        /// <summary>
        /// Adds a card to the hand.
        /// </summary>
        /// <param name="cd">The card deck to take the card out of.</param>
        public void AddCard(CardDeck cd) {
            if ( cardIndex >= 10 )
                throw new Exception( "Cannot have more than 10 cards to a hand." );
            cards[cardIndex++] = cd.NextCard;
        }

        /// <summary>
        /// Adds a card to the hand.
        /// </summary>
        /// <param name="cd">The card to add.</param>
        public void AddCard(byte card) {
            if ( cardIndex >= 10 )
                throw new Exception( "Cannot have more than 10 cards to a hand." );
            cards[cardIndex++] = card;
        }

        /// <summary>
        /// Adds cards to the hand.
        /// </summary>
        /// <param name="cards">The cards to add.</param>
        public void AddCards(byte[] cards) {
            if ( cards.Length > ( 10 - cardIndex ) )
                throw new Exception( "Not enough room in hand for this many cards." );
            for ( int i = 0; i < cards.Length; i++ )
                this.cards[cardIndex++] = cards[i];
        }

        /// <summary>
        /// Remove a card from the hand.
        /// </summary>
        /// <param name="index">The index of the card to remove.</param>
        public void RemoveCard(int index) {
            if ( index >= cardIndex )
                throw new Exception( "That index does not point to a card." );
            for ( int i = index; i < cardIndex - 1; i++ )
                cards[i] = cards[i + 1];
            cardIndex--;
        }

        /// <summary>
        /// Remove the last element in the hand.
        /// </summary>
        public void RemoveLast() {
            if ( cardIndex == 0 )
                throw new Exception( "There are no cards in the hand." );
            cardIndex--;
        }

        /// <summary>
        /// Remove the first element in the hand.
        /// </summary>
        public void RemoveFirst() {
            if ( Count == 0 )
                throw new Exception( "There are no cards in the hand." );
            for ( int i = 0; i < cardIndex-1; i++ )
                cards[i] = cards[i + 1];
            cardIndex--;
        }

        /// <summary>
        /// Returns how many cards are in the hand.
        /// </summary>
        public int Count {
            get { return cardIndex; }
        }

        /// <summary>
        /// Summates the cards in the array.
        /// </summary>
        /// <param name="cards">The array of cards to sum.</param>
        /// <param name="s">The summation method.</param>
        /// <returns>The summation.</returns>
        public int GetSum(SumType s) {
            int sum = 0;
            int aces = 0;

            for ( int i = 0; i < cardIndex; i++ ) {
                int cardVal = cards[i] & (byte)Suit.CardMask;
                if ( cardVal >= 10 )
                    sum += 10;
                else if ( cardVal == 1 ) {
                    sum += ( s == SumType.Low ) ? 1 : 11;
                    aces++;
                }
                else if ( cardVal == 0 ) {
                    break;
                }
                else
                    sum += cardVal;
            }

            if ( s == SumType.Smart )
                while ( sum > 21 && aces > 0 ) {
                    sum -= 10;
                    aces--;
                }

            return sum;
        }

        #region IEnumerable<byte> Members

        public IEnumerator<byte> GetEnumerator() {
            return new Hand.Enumerator( ref this.cards, cardIndex );
        }

        #endregion

        #region IEnumerable Members

        public struct Enumerator : IEnumerator<byte> {

            private int i;
            private byte[] data;
            private int limit;

            public Enumerator(ref byte[] data, int limit) {
                this.data = data;
                this.limit = limit;
                i = -1;
            }

            #region IEnumerator<byte> Members

            public byte Current {
                get { return data[i]; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose() {
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current {
                get { return data[i]; }
            }

            public bool MoveNext() {
                if ( ++i >= limit )
                    return false;
                return true;
            }

            public void Reset() {
                i = -1;
            }

            #endregion
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new Hand.Enumerator(ref cards, cardIndex);
        }

        #endregion
    }

}
