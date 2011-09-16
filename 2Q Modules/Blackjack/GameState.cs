using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using Project2Q.SDK.ModuleSupport;

namespace Blackjack {

    #region Enumerations

    /// <summary>
    /// Various stages of the game.
    /// </summary>
    [Flags]
    public enum GameStage : byte {
        Stopped = 0x0,
        Started = 0x1,
        InitialBetting = 0x2,
        Dealing = 0x4,
        OpenedForAction = 0x8,
    }

    #endregion

    /// <summary>
    /// A struct to keep game state on various servers.
    /// </summary>
    public class GameState {

        #region Constructor

        public GameState() {
            channel = null;
            sd = null;
            sid = 0;
            players = new Dictionary<string, bool>();
            stage = GameStage.Stopped;
            deck = new CardDeck();
            deck.Shuffle(); //Just in case
            dealerCards = new Hand();
        }

        #endregion

        #region Variables + Properties

        //2Q Stuff to communicate
        public ModuleProxy.SendDataDelegate sd;
        public string channel;
        public int sid;

        //The list of players.
        public Dictionary<string, bool> players;

        //The game State
        public GameStage stage;
        public CardDeck deck;
        public Hand dealerCards;

        //Other
        public Timer gameTimer;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sends a message to the GameState's channel.
        /// </summary>
        /// <param name="x">The message to send.</param>
        public void SayChannel(string x) {
            sd.Invoke( "PRIVMSG " + channel + " :\u001FBJ:\u001F " + x );
        }

        /// <summary>
        /// Sends a message to a user.
        /// </summary>
        /// <param name="x">The message to send.</param>
        /// <param name="u">The user to send it to.</param>
        public void SayUser(string x, string u) {
            sd.Invoke( "NOTICE " + u + " :\u001FBJ:\u001F " + x );
        }

        public void StartGame() {
            stage = GameStage.Started | GameStage.InitialBetting;
            //deck.ResetDeck();
            //deck.Shuffle();
            dealerCards.ResetHand();
            players.Clear();
            SayChannel( "Place your initial bets now!" );
            gameTimer = new Timer( 20000 );
            gameTimer.Enabled = true;
            gameTimer.AutoReset = false;
            gameTimer.Elapsed += new ElapsedEventHandler( this.InitialBetExpire );
            gameTimer.Start();
        }

        /// <summary>
        /// Checks to see if the game is currently in gs stage.
        /// </summary>
        /// <param name="gs">The gamestage to check for.</param>
        /// <returns>Wether or not we're in this stage.</returns>
        public bool InStage(GameStage gs) {
            return (stage & gs) != 0;
        }

        #endregion

        #region InitialBetExpire
        public void InitialBetExpire(object sender, ElapsedEventArgs e) {

            stage &= ~GameStage.InitialBetting; //Turn off the flag, so no one can keep trying.

            if ( players.Count == 0 ) {
                SayChannel( "Huh.. I guess no one wanted to play after all." );
                stage = GameStage.Stopped;
                return;
            }

            /*if ( deck.ValueLeft < 21 * ( players.Count + 2 ) ) {
                SayUser( "Deck Value: " + deck.ValueLeft + ", Safe value calculated: " + (21 * (players.Count + 2 )).ToString() + ".. Resetting deck.", "Aaron" );
                deck.ResetDeck();
                deck.Shuffle();
            }
            else {
                SayUser( "Deck Value: " + deck.ValueLeft, "Aaron" );
            }*/

            LinkedList<string> blackJackPlayers = new LinkedList<string>(); //Ughh
            byte[] ctemp;
            Player play = null;
            string card = null, output;
            char suit = '\0';
            foreach ( string player in players.Keys ) {
                try {
                    play = BJMain.serverData[sid].p[player];
                }
                catch ( KeyNotFoundException ) {
                    SayChannel( "Something horrendously wrong just happened, please alert the bot manager." );
                    break;
                }
                ctemp = deck.GetCards(2);
                play.currentCards.AddCards( ctemp );
                CardDeck.TranslateCard( ctemp[0], ref card, ref suit );
                //if ( suit == 'H' || suit == 'D' ) card = "\u00034" + card + "\u0003 " + suit;
                /*else*/
                card = "\u0002 " + card + "\u0002" + suit;
                output = "Your cards are:" + card;
                CardDeck.TranslateCard( ctemp[1], ref card, ref suit );
                //if ( suit == 'H' || suit == 'D' ) card = "\u00034 " + card + "\u0003" + suit;
                /*else*/
                card = "\u0002 " + card + "\u0002" + suit;
                int lowsum = play.currentCards.GetSum( SumType.Low );
                int hisum = play.currentCards.GetSum( SumType.High );
                output += "," + card +
                    " (" + ( ( lowsum == hisum ) ? hisum.ToString() : lowsum.ToString() + "/" + hisum.ToString() ) + ")";
                if ( hisum == 21 ) {
                    output += ".. \u0002BlackJack!\u0002";
                    play.state |= PlayerState.Stand;
                    blackJackPlayers.AddLast( player );
                    //players[play.nick] = true; //He's not gonna wanna change his blackjack.
                }
                SayUser( output, player );
            }

            foreach ( string s in blackJackPlayers ) {
                players[s] = true;
            }

            ctemp = deck.GetCards(2);
            dealerCards.AddCards( ctemp );
            CardDeck.TranslateCard( ctemp[0], ref card, ref suit );
            if ( suit == 'H' || suit == 'D' ) card = "\u0002\u00034 " + card + "\u0003\u0002" + suit;
            else card = "\u0002 " + card + "\u0002" + suit;
            output = "House's visible card is:" + card + ", play your hand now!";
            SayChannel( output );

            //Wait for action now, give people 30s to play their hand.
            stage = GameStage.Started | GameStage.OpenedForAction;
            if ( players.Count == blackJackPlayers.Count )
                gameTimer = new Timer( 500 );
            else
                gameTimer = new Timer( 60000 );
            gameTimer.Elapsed += new ElapsedEventHandler( PlayerStageOver );
            gameTimer.AutoReset = false;
            gameTimer.Enabled = true;
            gameTimer.Start();

        }

        #endregion

        #region PlayerStageOver
        public void PlayerStageOver(object sender, ElapsedEventArgs e) {

            stage &= ~GameStage.OpenedForAction; //Turn off the flag so no one can keep trying.

            if ( sender != null && e != null ) {
                //Trim players that did not play
                LinkedList<string> trimPlayers = new LinkedList<string>(); //Ughh
                Dictionary<string, bool>.Enumerator iter = players.GetEnumerator();
                while ( iter.MoveNext() ) {
                    if ( !iter.Current.Value ) {
                        //This person did not submit their bet, trim him.
                        trimPlayers.AddLast( iter.Current.Key );
                    }
                }
                iter.Dispose();

                foreach ( string p in trimPlayers ) {
                    SayUser( "You failed to finish your hand within the time limit, you have been trimmed from this game.", p );
                    Player pz = BJMain.serverData[sid].p[p];
                    pz.state = PlayerState.Out;
                    pz.bet = 0;
                    pz.currentCards.ResetHand();
                    pz.secondHand.ResetHand();
                    players.Remove( p );
                }
            }

            if ( players.Count == 0 ) {
                SayChannel( "Huh.. I guess no one wanted to play after all." );
                stage = GameStage.Stopped;
                return;
            }

            byte[] ctemp = new byte[10];
            string card = null, output;
            char suit = '\0';
            byte tcard;
            int sum;
            bool houseBust;
            bool houseBJ;

            //Reveal second card
            tcard = dealerCards.First;
            CardDeck.TranslateCard( tcard, ref card, ref suit );
            if ( suit == 'H' || suit == 'D' ) card = "\u0002\u00034 " + card + "\u0003\u0002" + suit;
            else card = "\u0002 " + card + "\u0002" + suit;
            output = "House hand is:" + card;
            tcard = dealerCards.Last;
            CardDeck.TranslateCard( tcard, ref card, ref suit );
            if ( suit == 'H' || suit == 'D' ) card = "\u0002\u00034 " + card + "\u0003\u0002" + suit;
            else card = "\u0002 " + card + "\u0002" + suit;
            sum = dealerCards.GetSum( SumType.Smart );
            output += "," + card + " (" + sum.ToString() + ")";
            //SayChannel( output );
            
            //AI Hits until hard 17

            sum = dealerCards.GetSum( SumType.Smart );

            while ( sum < 17 ) {
                tcard = deck.NextCard;
                dealerCards.AddCard( tcard );
                CardDeck.TranslateCard( tcard, ref card, ref suit );
                if ( suit == 'H' || suit == 'D' ) card = "\u0002\u00034 " + card + "\u0003\u0002" + suit;
                else card = "\u0002 " + card + "\u0002" + suit;
                sum = dealerCards.GetSum( SumType.Smart );
                output += ".." + card + " (" + sum.ToString() + ")";
            }

            if ( sum > 21 ) {
                output += ".. Bust:\u0002 " + sum.ToString() + "\u0002.";
                houseBust = true;
                houseBJ = false;
            }
            else if ( sum == 21 && dealerCards.Count == 2 ) {
                output += ".. House Blackjack!";
                houseBJ = true;
                houseBust = false;
            }
            else {
                output += ".. Stand:\u0002 " + sum.ToString() + "\u0002.";
                houseBust = false;
                houseBJ = false;
            }

            SayChannel( output );

            int playerSum;
       
            Dictionary<string, bool>.Enumerator itera = players.GetEnumerator();
            while ( itera.MoveNext() ) {
                Player p = BJMain.serverData[sid].p[itera.Current.Key];

                #region HouseBust
                if ( houseBust ) {

                    if ( p.HasState( PlayerState.Surrender ) ) {
                        p.surrenders++;
                    }
                    else if ( p.HasState( PlayerState.Bust ) ) {
                        p.money -= p.bet / 2;
                        SayUser( "House busted, you keep half your bet: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]",
                            p.nick );
                        p.busts++;
                    }
                    #region SplitHouseBust
                    else if ( p.HasState( PlayerState.Split ) ) {

                        if ( p.HasState( PlayerState.SplitLeftBust ) ) {
                            p.money -= p.bet / 4;
                            SayUser( "House busted, your left deck gets half it's bet back: \u0002" + Player.MoneyFormat( p.bet / 4 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]",
                                p.nick );
                            p.busts++;
                        }
                        else {
                            p.money += p.bet / 2;
                            SayUser( "House busted, your left deck wins: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]",
                                p.nick );
                            p.wins++;
                        }

                        if ( p.HasState( PlayerState.SplitRightBust ) ) {
                            p.money -= p.bet / 4;
                            SayUser( "House busted, your right deck gets half it's bet back: \u0002" + Player.MoneyFormat( p.bet / 4 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]",
                                p.nick );
                            p.busts++;
                        }
                        else {
                            p.money += p.bet / 2;
                            SayUser( "House busted, your right deck wins: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]",
                                p.nick );
                            p.wins++;
                        }

                    }
                    #endregion
                    #region NoSplitHouseBust
                    else {
                        if ( p.currentCards.GetSum( SumType.Smart ) == 21 && p.currentCards.Count == 2 ) {
                            p.money += p.bet * 3;
                            SayUser( "You BlackJacked this hand! House pays out 3:1!! \u0002" + Player.MoneyFormat( p.bet * 3 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            p.wins++;
                            p.blackjacks++;
                        }
                        else {
                            p.money += p.bet;
                            SayUser( "House busted, you win: \u0002" + Player.MoneyFormat( p.bet ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]",
                                p.nick );
                            p.wins++;
                        }
                    }

                }
                #endregion
                #endregion
                #region No House Bust
                else {

                    //Examine if this player won against the house.
                    if ( p.HasState( PlayerState.Surrender ) ) {
                        p.surrenders++;
                    }
                    else if ( p.HasState( PlayerState.Bust ) ) {
                        p.money -= p.bet;
                        SayUser( "You lost your bet of: \u0002" + Player.MoneyFormat( p.bet ) + "\u0002." + " [" + Player.MoneyFormat( p.money ) + "]",
                            p.nick );
                        p.busts++;
                    }
                    #region SplitNoHouseBust
                    else if ( p.HasState( PlayerState.Split ) ) {

                        if ( !p.HasState( PlayerState.SplitLeftBust ) ) {
                            //Check the first hand.
                            playerSum = p.currentCards.GetSum( SumType.Smart );

                            if ( houseBJ ) {
                                p.money -= p.bet / 2;
                                SayUser( "You lost your left hand bet of: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            }
                            else if ( playerSum > sum ) {
                                p.money += p.bet / 2;
                                SayUser( "You won on the left hand! You win: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002!" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                                p.wins++;
                            }
                            else if ( playerSum == sum ) {
                                SayUser( "You tied the house on the left hand! You keep its bet: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002!" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                                p.ties++;
                            }
                            else {
                                p.money -= p.bet / 2;
                                SayUser( "You lose on the left hand! You've lost: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002!" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            }
                        }
                        else {
                            p.money -= p.bet / 2;
                            SayUser( "You lost your left hand bet of: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002." + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            p.busts++;
                        }

                        if ( !p.HasState( PlayerState.SplitRightBust ) ) {

                            playerSum = p.secondHand.GetSum( SumType.Smart );

                            if ( houseBJ ) {
                                p.money -= p.bet / 2;
                                SayUser( "You lost your right hand bet of: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            }
                            else if ( playerSum > sum ) {
                                p.money += p.bet / 2;
                                SayUser( "You won on the right hand! You win: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                                p.wins++;
                            }
                            else if ( playerSum == sum ) {
                                SayUser( "You tied the house on the right hand! You keep its bet: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                                p.ties++;
                            }
                            else {
                                p.money -= p.bet / 2;
                                SayUser( "You lose on the right hand! You've lost: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            }
                        }
                        else {
                            p.money -= p.bet / 2;
                            SayUser( "You lost your right hand bet of: \u0002" + Player.MoneyFormat( p.bet / 2 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            p.busts++;
                        }

                    }
                    #endregion
                    #region NoSplitNoHouseBust
                    else {
                        playerSum = p.currentCards.GetSum( SumType.Smart );

                        if ( houseBJ ) {
                            if ( playerSum == 21 && p.currentCards.Count == 2 ) {
                                SayUser( "You tied the house this hand! You keep your bet: \u0002" + Player.MoneyFormat( p.bet ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                                p.ties++;
                            }
                            else {
                                p.money -= p.bet;
                                SayUser( "You lose this hand! You've lost: \u0002" + Player.MoneyFormat( p.bet ) + "\u0002!" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            }
                        }
                        else if ( playerSum == 21 && p.currentCards.Count == 2 ) {
                            p.money += p.bet * 3;
                            SayUser( "You BlackJacked this hand! House pays out 3:1!! \u0002" + Player.MoneyFormat( p.bet * 3 ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            p.wins++;
                            p.blackjacks++;
                        }
                        else if ( playerSum > sum ) {
                            p.money += p.bet;
                            SayUser( "You won this hand! You win: \u0002" + Player.MoneyFormat( p.bet ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            p.wins++;
                        }
                        else if ( playerSum == sum ) {
                            SayUser( "You tied the house this hand! You keep your bet: \u0002" + Player.MoneyFormat( p.bet ) + "\u0002" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                            p.ties++;
                        }
                        else {
                            p.money -= p.bet;
                            SayUser( "You lose this hand! You've lost: \u0002" + Player.MoneyFormat( p.bet ) + "\u0002!" + " [" + Player.MoneyFormat( p.money ) + "]", p.nick );
                        }
                    }
                    #endregion
                }
                #endregion

                if ( p.money < 10 ) {
                    SayUser( "House feels bad for horrible player, you've graciously been given another \u0002" + Player.MoneyFormat( 50 - p.money ) + "\u0002 [" + Player.MoneyFormat( 50 ) + "]",
                        p.nick );
                    p.money = 50;
                    p.moneyResets++;
                }

                p.hands += ( p.HasState( PlayerState.Split ) ) ? (ulong)2 : (ulong)1;
                if ( p.highestMoney < p.money ) p.highestMoney = p.money;
                if ( p.HasState( PlayerState.DoubleDown ) )
                    p.dds++;
                if ( p.HasState( PlayerState.Split ) )
                    p.splits++;

                p.state = PlayerState.Out;
                p.bet = 0;
                p.channel = string.Empty;
                p.currentCards.ResetHand();
                p.secondHand.ResetHand();

            }
            itera.Dispose();

            //All players should be processed. Start a new game.
            StartGame();
        }

        #endregion
    };

}
