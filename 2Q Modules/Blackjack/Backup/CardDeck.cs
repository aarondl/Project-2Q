using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack {

    #region Global Enumerations

    /// <summary>
    /// Describes the suit of a card.
    /// </summary>
    [Flags]
    public enum Suit : byte {
        Hearts = 0x0,
        Diamonds = 0x10,
        Spades = 0x20,
        Clubs = 0x30,
        /// <summary>
        /// Used to extract a suit from a card byte.
        /// </summary>
        SuitMask = Clubs,
        /// <summary>
        /// Used to extract a card number from a card byte.
        /// </summary>
        CardMask = 0x0F,
    }

    #endregion

    #region Exception Types

    /// <summary>
    /// An exception that occurs exclusively inside a deck.
    /// </summary>
    public class DeckException : Exception {
        public DeckException(string message) {
            errmsg = message;
        }
        string errmsg;
    }

    #endregion

    /// <summary>
    /// Represents a Card Deck with no jokers.
    /// </summary>
    public class CardDeck {

        #region Properties and Variables

        private byte[] deck;
        private int deckPtr;
        //private int valueLeft;

        /// <summary>
        /// Gets the total value of the cards left in the deck.
        /// </summary>
        /*public int ValueLeft {
            get { return valueLeft; }
        }*/

        /// <summary>
        /// Gets the next card.
        /// </summary>
        public byte NextCard {
            get {
                if ( NumCards - deckPtr == 0 ) {
                    ResetDeck();
                    Shuffle();
                    Shuffle();
                    Shuffle();
                    Shuffle();
                    Shuffle();
                    Shuffle();
                    Shuffle();
                }
                    //throw new DeckException( "Not enough cards left in the deck for this operation." );
                //valueLeft -= ( deck[deckPtr] & (byte)Suit.CardMask );
                return deck[deckPtr++];
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new deck organized AH, AD, AS, AC, 1H, 1D...
        /// </summary>
        public CardDeck() {
            deckPtr = 0;
            deck = new byte[NumCards];
            FullDeck.CopyTo( deck, 0 );
            //valueLeft = FullDeckValue;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets n cards and returns them.
        /// </summary>
        /// <param name="ncards">The number of cards to retrieve.</param>
        /// <returns>An array full of the cards requested.</returns>
        public byte[] GetCards(int ncards) {
            /*if ( ncards > NumCards )
                throw new DeckException( "Not enough cards in a deck for this operation." );*/
            /*if ( NumCards - deckPtr - 1 < ncards ) {
                ResetDeck();
                Shuffle();
            }*/
            //throw new DeckException( "Not enough cards left in the deck for this operation." );

            byte[] b = new byte[ncards];
            for ( int i = 0; i < ncards; i++ ) {

                if ( NumCards - deckPtr == 0 ) {
                    ResetDeck();
                    Shuffle();
                }

                b[i] = deck[deckPtr++];
                //valueLeft -= ( b[i] & (byte)Suit.CardMask );
            }

            return b;
        }

        /// <summary>
        /// Resets the deck to default.
        /// </summary>
        public void ResetDeck() {
            deckPtr = 0;
            FullDeck.CopyTo( deck, 0 );
            //valueLeft = FullDeckValue;
        }

        /// <summary>
        /// Shuffles the deck.
        /// </summary>
        public void Shuffle() {            
            Random r = new Random();

            //Hopefully that shuffled it well enough :D
            for ( int i = 0; i < NumCards; i++ ) {
                int j = r.Next( 0, NumCards );
                byte temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        #endregion

        #region Statics and Constants

        /// <summary>
        /// The number of cards in a deck.
        /// </summary>
        public static readonly int NumCards = 52;
        public static readonly int FullDeckValue = 364;

        /// <summary>
        /// Translates a card into displayable data.
        /// </summary>
        /// <param name="encodedcard">The encoded card.</param>
        /// <param name="card">The card type.</param>
        /// <param name="suit">The card suit.</param>
        public static void TranslateCard(byte encodedcard, ref string card, ref char suit) {
            switch ( (byte)Suit.SuitMask & encodedcard ) {
                case (byte)Suit.Hearts: suit = 'H'; break;
                case (byte)Suit.Diamonds: suit = 'D'; break;
                case (byte)Suit.Clubs: suit = 'C'; break;
                case (byte)Suit.Spades: suit = 'S'; break;
                default: suit = 'E'; break;
            }

            switch ( (byte)( ~Suit.SuitMask ) & encodedcard ) {
                case 1: card = "A"; break;
                case 2: card = "2"; break;
                case 3: card = "3"; break;
                case 4: card = "4"; break;
                case 5: card = "5"; break;
                case 6: card = "6"; break;
                case 7: card = "7"; break;
                case 8: card = "8"; break;
                case 9: card = "9"; break;
                case 10: card = "10"; break;
                case 11: card = "J"; break;
                case 12: card = "Q"; break;
                case 13: card = "K"; break;
                default: card = "E"; break;
            }
        }

        /// <summary>
        /// A full deck of cards.
        /// </summary>
        public static readonly byte[] FullDeck = {
            1, 17, 33, 49, //Ace of Hearts, Diamonds, Spades, Clubs
            2, 18, 34, 50, //2 of Hearts, Diamonds, Spades, Clubs
            3, 19, 35, 51, //3 of Hearts, Diamonds, Spades, Clubs
            4, 20, 36, 52, //4 of Hearts, Diamonds, Spades, Clubs
            5, 21, 37, 53, //5 of Hearts, Diamonds, Spades, Clubs
            6, 22, 38, 54, //6 of Hearts, Diamonds, Spades, Clubs
            7, 23, 39, 55, //7 of Hearts, Diamonds, Spades, Clubs
            8, 24, 40, 56, //8 of Hearts, Diamonds, Spades, Clubs
            9, 25, 41, 57, //9 of Hearts, Diamonds, Spades, Clubs
            10, 26, 42, 58, //10 of Hearts, Diamonds, Spades, Clubs
            11, 27, 43, 59, //J of Hearts, Diamonds, Spades, Clubs
            12, 28, 44, 60, //Q of Hearts, Diamonds, Spades, Clubs
            13, 29, 45, 61, //K of Hearts, Diamonds, Spades, Clubs
        };

        /* Card Storage outline.
         * 
         * A = 1, K = 13
         * 13 types of cards. 13 combinations can be stored in
         * 4 bits.
         * 
         * 4 suits can be encoded in two bits. 00, 01, 10, 11
         * 
         * uyyx xxxx
         * x = card#
         * u = unused
         * y = suit
         *   00 (0) [00|0000:0] = Hearts
         *   01 (1) [01|0000:16] = Diamonds
         *   10 (3) [10|0000:32] = Spades
         *   11 (4) [11|0000:48] = Clubs
         * 
         * Therefore we can encode this in bytes. Topping out at 7 bits.
         */

        #endregion

    }

}
