using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using System.IO;

using Project2Q.SDK.ModuleSupport;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;
using Injections = Project2Q.SDK.Injections;

namespace Blackjack {

    #region Helper Classes

    /// <summary>
    /// For storage purposes only.
    /// </summary>
    public class PlayersChannels {
        //Nick . Player
        public Dictionary<string, Player> p;
        //Channelname . Gamestate
        public Dictionary<string, GameState> g;
    };

    public class PlayerComparer : IComparer<Player> {

        #region IComparer<Player> Members

        public int Compare(Player x, Player y) {
            if ( x.highestMoney < y.highestMoney )
                return 1;
            else if ( x.highestMoney > y.highestMoney )
                return -1;
            return 0;
        }

        #endregion

        #region IComparer<Player> Members

        int IComparer<Player>.Compare(Player x, Player y) {
            if ( x.highestMoney < y.highestMoney )
                return 1;
            else if ( x.highestMoney > y.highestMoney )
                return -1;
            return 0;
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// The main class that drives the game.
    /// </summary>
    public class BJMain : IModuleCreator, IDisposable {

        #region 2Q Stuff

        public Injections.ChannelMessageEvent channelMessageData;
        public Injections.UserMessageEvent userMessageData;
        //private RelayServer bjs;

        /// <summary>
        /// Constructor
        /// </summary>
        public BJMain() {
            serverData = new PlayersChannels[IModule.MaxServers];
        }

        public override void Initialize() {
            if ( !Directory.Exists( @"modules\BlackJack" ) )
                Directory.CreateDirectory( @"modules\BlackJack" );
        }

        public override void Activated(int sid) {
            mp.RegisterEvent( "ChannelMessage", sid, new CrossAppDomainDelegate( Dispatch ) );
            mp.RegisterEvent( "UserMessage", sid, new CrossAppDomainDelegate( Control ) );

            //Load up file here.
            if ( serverData[sid] != null ) return;

            serverData[sid] = new PlayersChannels();
            serverData[sid].p = new Dictionary<string, Player>();

            string serverFile = @"modules\BlackJack\" +
                ( (Project2Q.SDK.Configuration.ServerConfig)mp.RequestVariable( Request.ServerConfiguration, sid ) ).Name + ".dat";

            if ( File.Exists( serverFile ) ) {
                FileStream fs = new FileStream( serverFile, FileMode.Open, FileAccess.Read );


                Player toAdd = Player.Deserialize( fs );
                while ( toAdd != null ) {
                    serverData[sid].p.Add( toAdd.nick, toAdd );
                    toAdd = Player.Deserialize( fs );
                }

                /*BinaryFormatter bf = new BinaryFormatter();
                serverData[sid].p = (Dictionary<string, Player>)bf.Deserialize( fs );*/
                
                fs.Close();
            }
        }

        public override void Deactivated(int sid) {
            mp.UnregisterEvent( "ChannelMessage", sid, new CrossAppDomainDelegate( Dispatch ) );
            mp.UnregisterEvent( "UserMessage", sid, new CrossAppDomainDelegate( Control ) );

            string serverFile = @"modules\BlackJack\" +
                ( (Project2Q.SDK.Configuration.ServerConfig)mp.RequestVariable( Request.ServerConfiguration, sid ) ).Name + ".dat";

            FileStream fs = new FileStream( serverFile, FileMode.Create, FileAccess.Write );

            foreach ( Player p in serverData[sid].p.Values ) {
                Player.Serialize( fs, p );
            }

            fs.Close();
        }

        [PrivelegeRequired( Priveleges.SuperUser )]
        public void Control() {

            string[] cmd = userMessageData.text.Split();
            if ( cmd.Length < 2 )
                return;
            switch ( cmd[0] ) {
                case "bj.join":
                    returns = new string[] { "JOIN :" + cmd[1] };
                    break;
                case "bj.part":
                    returns = new string[] { "PART :" + cmd[1] };
                    break;
                case "bj.append":

                    if ( cmd.Length < 3 ) return;
                    
                    PlayersChannels pc = serverData[userMessageData.sid];

                    bool playingFlag = false;
                    string channelPlaying = null;
                    if ( pc.g != null )
                        foreach ( GameState gs in pc.g.Values ) {
                            if ( gs.stage != GameStage.Stopped ) {
                                playingFlag = true;
                                channelPlaying = gs.channel;
                            }
                            break;
                        }

                    if ( playingFlag ) {
                        Output( "Cannot append, game in session on: " + channelPlaying, userMessageData.sender.Nickname );
                        return;
                    }
                   
                    Player source = null, destination = null;

                    try {
                        source = pc.p[cmd[1]];
                    }
                    catch ( KeyNotFoundException ) {
                        Output( "Cannot append, source: " + cmd[1] + " not found in database.", userMessageData.sender.Nickname );
                        return;
                    }

                    try {
                        destination = pc.p[cmd[2]];
                    }
                    catch ( KeyNotFoundException ) {
                        Output( "Cannot append, destination: " + cmd[2] + " not found in database.", userMessageData.sender.Nickname );
                        return;
                    }

                    destination.money += source.money;

                    destination.blackjacks += source.blackjacks;
                    destination.hands += source.hands;
                    destination.wins += source.wins;
                    destination.ties += source.ties;
                    destination.splits += source.splits;
                    destination.moneyResets += source.moneyResets;
                    destination.highestMoney = ( destination.highestMoney > source.highestMoney ) ? destination.highestMoney : source.highestMoney;
                    destination.dds += source.dds;
                    destination.busts += source.busts;
                    destination.surrenders += source.surrenders;

                    //Destroy the source.
                    pc.p.Remove( source.nick );

                    Output( "Successfully merged: " + cmd[1] + " into: " + cmd[2], userMessageData.sender.Nickname );

                    break;                    
            }

        }

        public void Dispose() {
            //? Do we need this ?
        }

        #endregion

        public static PlayersChannels[] serverData;

        #region Methods

        /// <summary>
        /// Returns a formatted message to a channel or user.
        /// </summary>
        /// <param name="x">The string to say</param>
        /// <param name="userchan">The user or channel to message.</param>
        public void Output(string x, string userchan) {
            //PRIVMSG
            //return ( userchan[0] == '#' ? "PRIVMSG " : "NOTICE " ) + userchan + " :\u001FBJ:\u001F " + x;
            if ( userchan[0] == '#' || userchan[0] == '&' ) {
                returns = new string[] {
                    "PRIVMSG " + userchan + " :\u001FBJ:\u001F " + x,
                };
            }
            else {
                returns = new string[] {
                    "NOTICE " + userchan + " :\u001FBJ:\u001F " + x,
                };
            }
        }

        #endregion

        /// <summary>
        /// This command controls the regular game commands.
        /// </summary>
        public void Dispatch() {

            string[] command = channelMessageData.text.Split();

            GameState state = null;
            Player p = null;
            Dictionary<string, Player> players = serverData[channelMessageData.sid].p;
            try {
                state = serverData[channelMessageData.sid].g[channelMessageData.channel.Name];
            }
            catch ( KeyNotFoundException ) { state = null; }
            catch ( NullReferenceException ) { state = null; }

            bool finished;
            string reason = null;
            byte card;
            string visualCard = null;
            char visualSuit = '1';
            int sum = 0;
            ulong betCalc;
            string output;

            #region Bet
            if ( command[0].Equals( "bet" ) ) {
                if ( state == null || !state.InStage( GameStage.Started ) ||
                    command.Length < 2 || !state.InStage( GameStage.InitialBetting ) ) return;

                //Verify that the second variable is indeed a monetary amount
                ulong betAmount = 0;
                try {
                    if ( command[1][0] == '$' )
                        command[1] = command[1].Remove( 0, 1 );
                    betAmount = ulong.Parse( command[1] );
                }
                catch ( FormatException ) {
                    Output( "That is not a valid bet.", channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }
                catch ( OverflowException ) {
                    Output( "That is not a valid bet.", channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }

                //Check if the player exists. If not, add him with $10 if this is the InitialBetting stage, ignore him if
                //it's the OpenedForAction stage.
                p = null;
                finished = true;
                try {
                    p = serverData[channelMessageData.sid].p[channelMessageData.channelUser.InternalUser.Nickname];
                }
                catch ( KeyNotFoundException ) {
                    finished = false;
                    p = new Player();
                    p.nick = channelMessageData.channelUser.InternalUser.Nickname;
                    p.money = Player.StartMoney;
                    serverData[channelMessageData.sid].p.Add(
                        channelMessageData.channelUser.InternalUser.Nickname, p );
                }

                if ( p.HasState( PlayerState.In ) ) {
                    Output( "You already bet \u0002" + Player.MoneyFormat( p.bet ) + "\u0002!", channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }

                if ( betAmount < 10 ) {
                    Output( "Minimum bet is: \u0002" + Player.MoneyFormat( 10 ) + "\u0002.", channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }

                //Bound the bet
                if ( betAmount > p.money ) {
                    if ( finished )
                        Output( "You only have: \u0002" + Player.MoneyFormat( p.money ) + "\u0002.", channelMessageData.channelUser.InternalUser.Nickname );
                    else {
                        Output( "You have been added to the game database with: \u0002" + Player.MoneyFormat( p.money ) + "\u0002.",
                            channelMessageData.channelUser.InternalUser.Nickname );
                        Output( "You only have \u0002" + Player.MoneyFormat( p.money ) + "\u0002.", channelMessageData.channelUser.InternalUser.Nickname );
                    }
                    return;
                }

                //Set bet
                p.bet = betAmount;
                //Change state to signify he's playing in this channel.
                p.state = PlayerState.In;
                p.channel = channelMessageData.channel.Name;
                state.players.Add( p.nick, false );

                if ( !finished ) {
                    Output( "You have been added to the game database with: \u0002" + Player.MoneyFormat( p.money ) + "\u0002.",
                    channelMessageData.channelUser.InternalUser.Nickname );
                    Output( "You are sitting in with a bet of: \u0002" + Player.MoneyFormat( betAmount ) + "\u0002" + " [" + Player.MoneyFormat( p.money - p.bet ) + "]",
                    channelMessageData.channelUser.InternalUser.Nickname );
                }
                else
                    Output( "You are sitting in with a bet of: \u0002" + Player.MoneyFormat( betAmount ) + "\u0002" + " [" + Player.MoneyFormat( p.money - p.bet ) + "]",
                    channelMessageData.channelUser.InternalUser.Nickname );
                return;

            }
            #endregion
            #region Money
            else if ( command[0].Equals( "money" ) || command[0].Equals( "$" ) ) {
                try {
                    p = serverData[channelMessageData.sid].p[channelMessageData.channelUser.InternalUser.Nickname];
                }
                catch ( KeyNotFoundException ) {
                    Output( "You currently have: \u0002" + Player.MoneyFormat( Player.StartMoney ) + "\u0002",
                    channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }
                Output( "You currently have: \u0002" + Player.MoneyFormat( p.money ) + "\u0002",
                channelMessageData.channelUser.InternalUser.Nickname );
                return;
            }
            #endregion
            #region bj_stats
            else if ( command[0].Equals( "bj.stats" ) ) {

                string nickToFind = channelMessageData.channelUser.InternalUser.Nickname;

                if ( command.Length >= 2 && command[1] != null && !command[1].Equals( string.Empty ) )
                    nickToFind = command[1];

                try {
                    p = serverData[channelMessageData.sid].p[nickToFind];
                }
                catch ( KeyNotFoundException ) {
                    Output( ( nickToFind.Equals( channelMessageData.channelUser.InternalUser.Nickname ) ? "You are" : nickToFind + " is" ) + " not in the database.",
                    channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }

                string outputomfg = "Stats for \u001F\u0002" + p.nick + "\u0002\u001F:";
                outputomfg += " \u0002Hands Played\u0002: " + p.hands;
                outputomfg += " \u0002Most/Current Money\u0002: [" + Player.MoneyFormat( p.highestMoney ) + "/" + Player.MoneyFormat( p.money ) + "]";
                outputomfg += " \u0002Win/Lossed/Tied Hands\u0002: [" + p.wins + "/" + ( p.hands - p.wins - p.ties ) + "/" + p.ties + "]" +
                    "[" + Math.Round( ( (float)p.wins / (float)p.hands ) * 100, 2 ) + "%/" + Math.Round( ( (float)( p.hands - p.wins - p.ties ) / (float)p.hands ) * 100, 2 )
                    + "%/" + Math.Round( ( (float)p.ties / (float)p.hands ) * 100, 2 ) + "%]";
                outputomfg += " \u0002Blackjacks\u0002: " + p.blackjacks + " (" + Math.Round( ( (float)p.blackjacks / (float)p.hands ) * 100, 2 ) + "%)";
                outputomfg += " \u0002Busts\u0002: " + p.busts + " (" + Math.Round( ( (float)p.busts / (float)p.hands ) * 100, 2 ) + "%)";
                outputomfg += " \u0002Double Downs\u0002: " + p.dds + " (" + Math.Round( ( (float)p.dds / (float)p.hands ) * 100, 2 ) + "%)";
                outputomfg += " \u0002Splits\u0002: " + p.splits + " (" + Math.Round( ( (float)p.splits / (float)p.hands ) * 100, 2 ) + "%)";
                outputomfg += " \u0002Surrenders\u0002: " + p.surrenders + " (" + Math.Round( ( (float)p.surrenders / (float)p.hands ) * 100, 2 ) + "%)";
                outputomfg += " \u0002Money Resets\u0002: " + p.moneyResets;

                Output( outputomfg, channelMessageData.channelUser.InternalUser.Nickname );
                return;

            }
            #endregion
            #region bj_top5
            else if ( command[0].Equals( "bj.top5" ) ) {

                int lolcount = serverData[channelMessageData.sid].p.Count;

                if ( lolcount != 0 ) {

                    Player[] moneys = new Player[lolcount];
                    PlayerComparer pc = new PlayerComparer();
                    serverData[channelMessageData.sid].p.Values.CopyTo( moneys, 0 );
                    Array.Sort<Player>( moneys, pc );

                    string outputstr = "Server." + ( (Project2Q.SDK.Configuration.ServerConfig)mp.RequestVariable( Request.ServerConfiguration, channelMessageData.sid ) ).Name + " Blackjack Top 5!";

                    for ( int i = 0; i < lolcount && i < 5; i++ )
                        outputstr += " \u0002" + ( i + 1 ).ToString() + ". " + moneys[i].nick + "\u0002 [" + Player.MoneyFormat( moneys[i].highestMoney ) + "]";

                    Output( outputstr, channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }
                else {
                    Output( "There is nobody on \u0002Server." + ( (Project2Q.SDK.Configuration.ServerConfig)mp.RequestVariable( Request.ServerConfiguration, channelMessageData.sid ) ).Name + "'s\u0002 Blackjack Top 5 yet!", channelMessageData.channel.Name );
                    return;
                }
            }
            #endregion
            #region bj_start
            else if ( command[0].Equals( "bj.start" ) ) {
                if ( state != null && state.InStage( GameStage.Started ) ) {
                    Output( "Blackjack is already started on: " + channelMessageData.channel.Name + ".", channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }

                if ( state == null ) {
                    serverData[channelMessageData.sid].g = new Dictionary<string, GameState>();
                    GameState gs = new GameState();
                    gs.channel = channelMessageData.channel.Name;
                    gs.sid = channelMessageData.sid;
                    gs.sd = (ModuleProxy.SendDataDelegate)mp.RequestVariable( Request.SendData, channelMessageData.sid );
                    serverData[channelMessageData.sid].g.Add( channelMessageData.channel.Name, gs );
                    state = serverData[channelMessageData.sid].g[channelMessageData.channel.Name];
                }

                state.StartGame();
                return;
            }
            #endregion
            #region bj_help
            else if ( command[0].Equals( "bj.help" ) ) {
                if ( command.Length > 1 ) {
                    switch ( command[1] ) {
                        case "bj.start":
                            Output( "\u0002bj.start\0002: Starts the game, if you've never played before, you automatically have $25, so start up a game then type bet $0 through $25 to start playing!",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                        case "bj.stop":
                            Output( "\u0002bj.stop\u0002: Stops the game.",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                        case "bj.top5":
                            Output( "\u0002bj.top5\u0002: Shows the top 5 users across the server.",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                        case "hit":
                            Output( "\u0002Hit\u0002: In play, hit makes the dealer give you another card. You cannot hit if you have busted, standed, double downed, or surrendered.",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                        case "stay": goto case "stand";
                        case "stand":
                            Output( "\u0002Stand\u0002: In play, stand tells the dealer that you do not wish to take anymore cards and that you're done your turn. Don't forget to use this command at the end of play or you will be trimmed from the current game!",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                        case "split":
                            Output( "\u0002Split\u0002: Useable only when you have two cards in your hand, and they are the same card. This command splits your cards into two seperate hands each with one card, you must also double your bet, half resting on each hand. You play them one at a time after that, using hit and stand, the bot assumes you start with the first hand, and after you've stood/busted on that hand, lets you hit/stand on the second. In this version, you can only split a hand once.",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                        case "dd": goto case "doubledown";
                        case "doubledown":
                            Output( "\u0002Double Down\u0002: Useable only when you have two cards in your hand. You double your bet, and take exactly one card more from the dealer. Your turn is over as soon as you double down.",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                        case "surrender":
                            Output( "\u0002Surrender\u0002: Useable only when you have two cards in your hand. This option allows you to take half of your bet back and discontinue the current play.",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                        default:
                            Output( "No help availible on that command.",
                            channelMessageData.channelUser.InternalUser.Nickname );
                            break;
                    }
                }
                else {
                    Output( "\u0002bj.start\u0002: Starts the game! \u0002bj.stop\u0002: Ends the game! \u0002hit, bet, split, dd/doubledown, stand, surrender, money\u0002: The game commands, consult a rulebook for Blackjack to learn to play. Type bj.help [command] for help on any command!",
                    channelMessageData.channelUser.InternalUser.Nickname );
                    Output( "When the house asks to place your bets, simply use the bet command with a monetary amount to begin playing. If you are a new player you will be given $10 to start with. After the house tells you to play your hand you just use the commands availible to you. Be sure to stand after you're done playing your hand or you'll be thrown out of the game!",
                    channelMessageData.channelUser.InternalUser.Nickname );
                }
                return;
            }
            #endregion
            #region bj_stop
            else if ( command[0].Equals( "bj.stop" ) ) {
                if ( state == null && !state.InStage( GameStage.Started ) ) {
                    Output( "Blackjack is not started on: " + channelMessageData.channel.Name + ". Type bj.start to start!", channelMessageData.channelUser.InternalUser.Nickname );
                    return;
                }
                Output( "Blackjack stopped!", channelMessageData.channel.Name );
                state.stage = GameStage.Stopped;

                //Kill all state
                foreach ( string playz in state.players.Keys ) {
                    Player zx = serverData[channelMessageData.sid].p[playz];
                    zx.bet = 0;
                    zx.state = PlayerState.Out;
                    zx.channel = null;
                }
                state.dealerCards.ResetHand();
                state.gameTimer.Stop();
                state.players.Clear();
                return;
            }
            #endregion

            if ( state == null || !state.InStage( GameStage.Started ) ||
                !state.InStage( GameStage.OpenedForAction ) ) return;

            //Check the player thoroughly
            try {
                p = serverData[channelMessageData.sid].p[channelMessageData.channelUser.InternalUser.Nickname];
            }
            catch ( KeyNotFoundException ) {
                //He wasn't playing anyways, go figure.
                return;
            }

            if ( !p.HasState( PlayerState.In ) || p.channel != channelMessageData.channel.Name ) {
                //He's not playing, or he's not playing on this channel.
                return;
            }

            switch ( command[0] ) {
                #region hit
                case "hit":

                    if ( !p.Eligible( PlayerState.Hit, ref reason ) ) {
                        Output( reason, channelMessageData.channelUser.InternalUser.Nickname );
                        return;
                    }

                    if ( p.HasState( PlayerState.Split ) ) {

                        bool righthand = false;

                        if ( p.HasState( PlayerState.SplitLeftStand ) || p.HasState( PlayerState.SplitLeftBust ) ) {
                            righthand = true;
                        }

                        card = state.deck.NextCard;
                        if ( righthand ) {
                            p.secondHand.AddCard( card );
                            sum = p.secondHand.GetSum( SumType.Smart );
                        }
                        else {
                            p.currentCards.AddCard( card );
                            sum = p.currentCards.GetSum( SumType.Smart );
                        }

                        CardDeck.TranslateCard( card, ref visualCard, ref visualSuit );

                        if ( visualSuit == 'H' || visualSuit == 'D' ) visualCard = "\u0002 " + visualCard + "\u0002" + visualSuit;
                        else visualCard = " \u0002" + visualCard + "\u0002" + visualSuit;
                        output = "Your next card for your " + ( righthand ? "right hand" : "left hand" ) + " deck is:" + visualCard + " (" + sum.ToString() + ")";

                        if ( sum > 21 ) {
                            //p.money = p.money - (p.bet/2);
                            p.state |= ( righthand ) ? PlayerState.SplitRightBust : PlayerState.SplitLeftBust;
                            output += ".. you bust!";
                            if ( righthand )
                                state.players[channelMessageData.channelUser.InternalUser.Nickname] = true;
                            Output( output, channelMessageData.channelUser.InternalUser.Nickname );
                        }
                        else {
                            Output( output, channelMessageData.channelUser.InternalUser.Nickname );
                        }

                    }

                    else {

                        card = state.deck.NextCard;
                        p.currentCards.AddCard( card );
                        sum = p.currentCards.GetSum( SumType.Smart );

                        CardDeck.TranslateCard( card, ref visualCard, ref visualSuit );

                        if ( visualSuit == 'H' || visualSuit == 'D' ) visualCard = "\u0002 " + visualCard + "\u0002" + visualSuit;
                        else visualCard = " \u0002" + visualCard + "\u0002" + visualSuit;
                        output = "Your next card is:" + visualCard + " (" + sum.ToString() + ")";

                        if ( sum > 21 ) {
                            p.state |= PlayerState.Bust;
                            output += ".. you bust!";
                            state.players[channelMessageData.channelUser.InternalUser.Nickname] = true;
                            Output( output, channelMessageData.channelUser.InternalUser.Nickname );
                        }
                        else {
                            p.state |= PlayerState.Hit;
                            Output( output, channelMessageData.channelUser.InternalUser.Nickname );
                        }

                    }

                    break;
                #endregion
                #region stand
                case "stay": goto case "stand";
                case "stand":

                    if ( !p.Eligible( PlayerState.Stand, ref reason ) ) {
                        Output( reason, channelMessageData.channelUser.InternalUser.Nickname );
                        return;
                    }

                    if ( p.HasState( PlayerState.Split ) ) {

                        if ( p.HasState( PlayerState.SplitLeftStand ) || p.HasState(PlayerState.SplitLeftBust) ) {
                            //This means we're dealing with the right hand now.
                            p.state |= PlayerState.SplitRightStand;
                            Output( "You've stood your right hand.", channelMessageData.channelUser.InternalUser.Nickname );
                            state.players[channelMessageData.channelUser.InternalUser.Nickname] = true;
                        }
                        else {
                            //Stand the left hand
                            p.state |= PlayerState.SplitLeftStand;
                            Output( "You've stood your left hand.", channelMessageData.channelUser.InternalUser.Nickname );
                        }

                    }
                    else {
                        p.state |= PlayerState.Stand;
                        Output( "You've stood your hand.", channelMessageData.channelUser.InternalUser.Nickname );
                        state.players[channelMessageData.channelUser.InternalUser.Nickname] = true;
                    }

                    break;
                #endregion
                #region doubledown
                case "dd": goto case "doubledown";
                case "doubledown":

                    if ( !p.Eligible( PlayerState.DoubleDown, ref reason ) ) {
                        Output( reason, channelMessageData.channelUser.InternalUser.Nickname );
                        return;
                    }

                    p.bet *= 2;
                    p.state |= PlayerState.DoubleDown;
                    p.state |= PlayerState.Stand;

                    card = state.deck.NextCard;
                    p.currentCards.AddCard( card );
                    sum = p.currentCards.GetSum( SumType.Smart );

                    CardDeck.TranslateCard( card, ref visualCard, ref visualSuit );

                    if ( visualSuit == 'H' || visualSuit == 'D' ) visualCard = "\u0002 " + visualCard + "\u0002" + visualSuit;
                    else visualCard = " \u0002" + visualCard + "\u0002" + visualSuit;
                    output = "You've doubled down and your new bet is: \u0002" + Player.MoneyFormat(p.bet) + "\u0002 [" + Player.MoneyFormat(p.money-p.bet) + "], your next card is:" + visualCard + " (" + sum.ToString() + ")";

                    if ( sum > 21 ) {
                        //p.money = p.money - p.bet;
                        p.state |= PlayerState.Bust;
                        output += ".. you bust!";
                        state.players[channelMessageData.channelUser.InternalUser.Nickname] = true;
                        Output( output, channelMessageData.channelUser.InternalUser.Nickname );
                        return;
                    }
                    else {
                        Output( output, channelMessageData.channelUser.InternalUser.Nickname );
                        //returns = new string[] { SayUserChan( "You've double downed you've stood your hand and your bet is now:\u0002 " + Player.MoneyFormat( p.bet ) + "\u0002.", channelMessageData.channelUser.InternalUser.Nickname ) };
                    }

                    state.players[channelMessageData.channelUser.InternalUser.Nickname] = true;

                    break;
                #endregion
                #region split
                case "split":

                    if ( !p.Eligible( PlayerState.Split, ref reason ) ) {
                        Output( reason, channelMessageData.channelUser.InternalUser.Nickname );
                        return;
                    }

                    p.secondHand.AddCard( p.currentCards.Last );
                    p.currentCards.RemoveLast();
                    p.state |= PlayerState.Split;
                    p.bet *= 2;

                    Output( "You've split your hand into left and right decks with a total bet of: \u0002" + Player.MoneyFormat( p.bet ) + "\u0002 [" + Player.MoneyFormat( p.money - p.bet ) + "]"
                    , channelMessageData.channelUser.InternalUser.Nickname );

                    break;
                case "surrender":

                    if ( !p.Eligible( PlayerState.Surrender, ref reason ) ) {
                        Output( reason, channelMessageData.channelUser.InternalUser.Nickname );
                        return;
                    }

                    betCalc = ( p.bet + 1 ) / 2;
                    p.state |= PlayerState.Surrender;
                    p.money -= betCalc;
                    Output( "You've surrendered and lost: \u0002" + Player.MoneyFormat( betCalc ) + "\u0002 [" + Player.MoneyFormat( p.money ) + "]", channelMessageData.channelUser.InternalUser.Nickname );
                    state.players[channelMessageData.channelUser.InternalUser.Nickname] = true;

                    break;
                #endregion
                default:
                    return;
            }

            finished = true;
            foreach ( bool b in state.players.Values )
                if ( !b ) { finished = false; break; }

            if ( finished ) {
                state.gameTimer.Stop();
                state.gameTimer.Interval = 1000;
                state.gameTimer.Start();
                //state.PlayerStageOver( null, null );
            }

        }

        #region Constants

        public static readonly int MaxPlayers = 5;

        #endregion
    }

}
