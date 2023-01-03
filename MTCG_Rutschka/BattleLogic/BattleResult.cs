using System;
using System.Collections.Generic;
using ConsoleTables;

namespace MTCG_Rutschka {
    /// <summary> Class to safe BattleResults </summary>
    public class BattleResult {
        /// <summary> List with all Rounds in the Battle </summary>
        public List<BattleRound> BattleRounds;
        /// <summary> Did the Battle Result in a Tie? </summary>
        public bool TieBattle;
        /// <summary> First Player in Fight </summary>
        public Player PlayerOne;
        /// <summary> Second Player in Fight </summary>
        public Player PlayerTwo;
        /// <summary> The ID of the Battle </summary>
        public string BattleId;

        /// <summary> Constructor with only two Players (During activ Battle </summary>
        /// <param name="pone"> First Player </param>
        /// <param name="ptwo"> Second Player </param>
        public BattleResult(Player pone, Player ptwo) {
            BattleId = Guid.NewGuid().ToString();
            PlayerOne = pone;
            PlayerTwo = ptwo;
            BattleRounds = new List<BattleRound>();
            TieBattle = false;
            Winner = null;
            Loser = null;
        }

        /// <summary> Constructor to create battle from DB </summary>
        /// <param name="bid"> BattleID </param>
        /// <param name="pone"> First Player </param>
        /// <param name="ptwo"> Second Player </param>
        /// <param name="tie"> Tie or not? </param>
        /// <param name="winner"> Winner </param>
        /// <param name="loser"> Loser </param>
        public BattleResult(string bid, Player pone, Player ptwo, bool tie, Player winner, Player loser) {
            BattleId = bid;
            PlayerOne = pone;
            PlayerTwo = ptwo;
            TieBattle = tie;
            Winner = winner;
            Loser = loser;
            BattleRounds = new List<BattleRound>();
        }

        /// <summary> Add new round to the Battle </summary>
        /// <param name="round"> a new BattleRound Instance </param>
        public void NewRound(BattleRound round) {
            BattleRounds.Add(round);
        }

        /// <summary> New Tie-Round (no Winner) </summary>
        /// <param name="roundCount"> No. of the Round fought </param>
        /// <param name="oneCard"> First Card </param>
        /// <param name="twoCard"> Second Card </param>
        public void NewTieRound(int roundCount, ICard oneCard, ICard twoCard) {
            BattleRounds.Add(new BattleRound(roundCount, PlayerTwo, PlayerTwo, oneCard, twoCard, true));
        }

        /// <summary> Function to Set Winner and Loser </summary>
        /// <param name="win">Winner </param>
        /// <param name="los"> Loser </param>
        public void FinalResult(Player win, Player los) {
            Winner = win;
            Loser = los;
        }

        /// <summary> The Winner of the Battle </summary>
        public Player Winner { get; private set; }
        /// <summary> The Loser of the Battle </summary>
        public Player Loser { get; private set; }

        /// <summary> Create a Console-Table with the Result of the Battle </summary>
        /// <returns> String with Battle Result </returns>
        public string StringifyResult() {
            var str = TieBattle ? "NO WINNER! BATTLE RESULTED IN A TIE!\n" : "WINNER: " + Winner.PlayerName + "! \nLOSER: " + Loser.PlayerName + "!\n";
            var players = PlayerOne.PlayerName + " vs " + PlayerTwo.PlayerName;
            var table = new ConsoleTable("ROUND", "PLAYERS", PlayerOne.PlayerName + " CARD", PlayerTwo.PlayerName + " CARD", "RESULT", "ROUNDWINNER");
            foreach (var round in BattleRounds) {
                var combiOne = "";
                if (round.RoundWinner == PlayerOne)
                    combiOne = round.WinnerCard.CardName + "(" + round.WinnerCard.CardDamage + "Dmg)";
                else
                    combiOne = round.LoserCard.CardName + "(" + round.LoserCard.CardDamage + "Dmg)";

                var combitTwo = "";
                if (round.RoundWinner == PlayerTwo)
                    combitTwo = round.WinnerCard.CardName + "(" + round.WinnerCard.CardDamage + "Dmg)";
                else
                    combitTwo = round.LoserCard.CardName + "(" + round.LoserCard.CardDamage + "Dmg)";

                var result = round.Tie ? round.WinnerCard.CardName + " equal with " + round.LoserCard.CardName : round.WinnerCard.CardName + " beats " + round.LoserCard.CardName;
                var winner = round.Tie ? "TIE" : round.RoundWinner.PlayerName;
                table.AddRow(round.RoundNum, players, combiOne, combitTwo, result, winner);
            }
            str = str + table.ToString();
            return str.Remove(str.LastIndexOf(Environment.NewLine));
        }
    }
}
