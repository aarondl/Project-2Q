using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Blackjack {

    #region Enumerations

    /// <summary>
    /// Various states that can affect a player.
    /// </summary>
    [Flags]
    public enum PlayerState : int {
        Out = 0x0,
        In = 0x1,

        Hit = 0x2,
        Stand = 0x4,
        DoubleDown = 0x8,
        Split = 0x10,
        Surrender = 0x20,
        Bust = 0x40,

        SplitLeftBust = 0x80,
        SplitRightBust = 0x100,
        SplitLeftStand = 0x200,
        SplitRightStand = 0x400,
    }

    #endregion

    /// <summary>
    /// A class designed to store data and state for a player.
    /// </summary>
    [Serializable]
    public class Player {

        /// <summary>
        /// Creates a blank player.
        /// </summary>
        public Player() {
            nick = null;
            money = 0;
            bet = 0;
            
            //Stats
            blackjacks = 0;
            hands = 0;
            wins = 0;
            ties = 0;
            busts = 0;
            highestMoney = Player.StartMoney;
            splits = 0;
            dds = 0;
            surrenders = 0;
            moneyResets = 0;

            state = PlayerState.Out;
            currentCards = new Hand();
            secondHand = new Hand();
            channel = string.Empty;
        }

        /// <summary>
        /// Formats the players money into a comma'd string.
        /// </summary>
        /// <returns>A comma-full string of money.</returns>
        public static string MoneyFormat(ulong money) {
            string convert = money.ToString();

            if ( convert.Length <= 3 )
                return "$" + convert;

            StringBuilder sb = new StringBuilder();
            sb.Append( '$' );

            int offset = convert.Length % 3;

            //Remaining digits is a multiple of 3, put in a thingy

            for ( int i = 0; i < convert.Length; i++ ) {
                if ( ( convert.Length - i ) % 3 == 0 && i != 0 )
                    sb.Append( "," );
                sb.Append( convert[i] );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks to see if the current player has the state flag ps.
        /// </summary>
        /// <param name="ps">The state flag to check for.</param>
        /// <returns>Wether or not the player had the state flag.</returns>
        public bool HasState(PlayerState ps) {
            return (state & ps) != 0;
        }

        #region Eligibility
        public bool Eligible(PlayerState ps, ref string reason) {
            if ( !HasState(PlayerState.In) ) {
                reason = "You're not playing this round!";
                return false;
            }

            if ( HasState( PlayerState.Stand ) ) {
                reason = "You can't do that, you already stood your hand.";
                return false;
            }
            if ( HasState( PlayerState.Bust ) ) {
                reason = "You can't do that, you already busted!";
                return false;
            }
            if ( HasState( PlayerState.Surrender ) ) {
                reason = "You can't do that, you already surrendered!";
                return false;
            }
            if ( HasState( PlayerState.Split ) ) {
                if ( ( HasState( PlayerState.SplitLeftBust ) || HasState( PlayerState.SplitLeftStand ) ) &&
                     ( HasState( PlayerState.SplitRightBust ) || HasState( PlayerState.SplitRightStand ) ) ) {
                    reason = "You can't do that, both of your hands are already finished.";
                    return false;
                }
            }

            switch ( ps ) {
                case PlayerState.Hit:
                    return true;
                case PlayerState.DoubleDown:
                    if ( currentCards.Count > 2 ) {
                        reason = "You can't double down on a hand with more than two cards.";
                        return false;
                    }
                    if ( bet * 2 > money ) {
                        reason = "You can't afford to double down.";
                        return false;
                    }
                    if ( HasState( PlayerState.Hit ) ) {
                        reason = "You can't do that, you already hit!";
                        return false;
                    }
                    if ( HasState( PlayerState.Split ) ) {
                        reason = "You can't do that, you already split!";
                        return false;
                    }
                    break;
                case PlayerState.Split:
                    if ( currentCards.Count > 2 ) {
                        reason = "You can't split a hand with more than two cards.";
                        return false;
                    }
                    if ( bet * 2 > money ) {
                        reason = "You can't afford to split.";
                        return false;
                    }
                    if ( HasState( PlayerState.Hit ) ) {
                        reason = "You can't do that, you already hit!";
                        return false;
                    }
                    if ( HasState( PlayerState.Split ) ) {
                        reason = "You can't do that, you've already split!";
                        return false;
                    }
                    if ( HasState( PlayerState.DoubleDown ) ) {
                        reason = "You can't do that, you already double downed!";
                        return false;
                    }
                    IEnumerator<byte> e = currentCards.GetEnumerator();
                    e.MoveNext();
                    byte firstcard = e.Current;
                    e.MoveNext();
                    byte secondcard = e.Current;
                    e.Dispose();
                    if ( ( firstcard & (byte)Suit.CardMask ) != ( secondcard & (byte)Suit.CardMask ) ) {
                        reason = "You can't do that, your cards are not the same value.";
                        return false;
                    }
                    break;
                case PlayerState.Stand:
                    return true;
                case PlayerState.Surrender:
                    if ( HasState( PlayerState.Split ) ) {
                        reason = "You can't surrender on a split!";
                        return false;
                    }
                    if ( currentCards.Count > 2 ) {
                        reason = "You can't surrender a hand with more than two cards.";
                        return false;
                    }
                    return true;
            }

            return true;
        }
        #endregion

        //Long-term data
        public string nick;
        public ulong money;

        //Stats
        public ulong blackjacks;
        public ulong hands;
        public ulong wins;
        public ulong ties;
        public ulong highestMoney;
        public ulong busts;
        public ulong splits;
        public ulong dds;
        public ulong surrenders;
        public ulong moneyResets;

        public static void Serialize(Stream s, Player p) {
            //Entry:
            //NICK0MONEY
            byte [] data;
            //Serializing with UTF strings
            UTF8Encoding utf = new UTF8Encoding();
            data = utf.GetBytes( p.nick );
            s.Write( data, 0, data.Length );
            s.WriteByte( 0 );
            data = BitConverter.GetBytes( p.money );
            s.Write( data, 0, data.Length ); //Should be 8

            //Write Statistics
            data = BitConverter.GetBytes( p.blackjacks );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.hands );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.wins );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.ties );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.highestMoney );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.busts );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.splits );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.dds );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.surrenders );
            s.Write( data, 0, data.Length ); //Should be 8
            data = BitConverter.GetBytes( p.moneyResets );
            s.Write( data, 0, data.Length ); //Should be 8
        }

        /// <summary>
        /// Deserializes a player from a stream.
        /// </summary>
        /// <param name="s">The stream to extract the player from.</param>
        /// <returns>The player found, or null if EOF</returns>
        public static Player Deserialize(Stream s) {
            Player p = new Player();
            LinkedList<byte> nameMaker = new LinkedList<byte>();
            UTF8Encoding utf = new UTF8Encoding();

            int nextByte = s.ReadByte();
            if ( nextByte == -1 )
                return null;

            //Read in all the bytes until 0 byte.
            while ( nextByte > 0 ) {
                nameMaker.AddLast( (byte)nextByte );
                nextByte = s.ReadByte();
            }

            //Make sure we keep UTF8 compat
            byte[] convert = new byte[nameMaker.Count];
            nameMaker.CopyTo( convert, 0 );
            p.nick = utf.GetString( convert );

            //Read the money
            convert = new byte[88];
            s.Read( convert, 0, 88 );
            p.money = BitConverter.ToUInt64( convert, 0 );

            //Read the stats
            p.blackjacks = BitConverter.ToUInt64( convert, 8 );
            p.hands = BitConverter.ToUInt64( convert, 16 );
            p.wins = BitConverter.ToUInt64( convert, 24 );
            p.ties = BitConverter.ToUInt64( convert, 32 );
            p.highestMoney = BitConverter.ToUInt64( convert, 40 );
            p.busts = BitConverter.ToUInt64( convert, 48 );
            p.splits = BitConverter.ToUInt64( convert, 56 );
            p.dds = BitConverter.ToUInt64( convert, 64 );
            p.surrenders = BitConverter.ToUInt64( convert, 72 );
            p.moneyResets = BitConverter.ToUInt64( convert, 80 );

            return p;
        }

        //State
        [NonSerialized]
        public ulong bet;
        [NonSerialized]
        public Hand secondHand;
        [NonSerialized]
        public Hand currentCards;
        [NonSerialized]
        public PlayerState state;
        [NonSerialized]
        public string channel;
        [NonSerialized]
        public static readonly ulong StartMoney = 50;

    };

}
