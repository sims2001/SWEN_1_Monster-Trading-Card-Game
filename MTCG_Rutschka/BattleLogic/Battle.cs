using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MTCG_Rutschka {
    class Battle {
        /// <summary> Object to Lock Thread</summary>
        private object _battleLock = new object();
        /// <summary> Queue with Players wanting to fight </summary>
        private Queue<Player> _players;
        /// <summary> Dictionary for Players with Responding BattleResults </summary>
        private Dictionary<Player, BattleResult> _resultDict;

        /// <summary> Creates a new BattleHub </summary>
        public Battle() {
            _players = new Queue<Player>();
            _resultDict = new Dictionary<Player, BattleResult>();
        }

        /// <summary> Adds a Player to the Queue and waits for BattleResult, which is returned </summary>
        /// <param name="p"> Player to add to queue </param>
        /// <exception cref="WrongCardCountException"> Thrown when there are not four Cards in the Players Deck</exception>
        /// <exception cref="AlreadyInQueueException"> Thrown when the Player's already in the Queue </exception>
        /// <returns> String with the BattleResult as Table </returns>
        public string WantsToFight(Player p) {
            if (p.CardsInDeck != 4) 
                throw new WrongCardCountException();

            Player p1 = null;
            Player p2 = null;
            //Lock Thread (Only one thread can do this)
            lock (_battleLock) {
                if (_players.Contains(p)) throw new AlreadyInQueueException();
                
                //Enqueue Player
                _players.Enqueue(p);

                //If Enough Players Then Dequeue
                if (_players.Count >= 2) {
                    p1 = _players.Dequeue(); // get player 1
                    p2 = _players.Dequeue(); // get player 2
                }
            }

            //If set Battle Players
            if (p1 != null && p2 != null) {
                var battleResult = FightBattle(p1, p2);
                _resultDict.Add(p1, battleResult);
                _resultDict.Add(p2, battleResult);
            }

            //Wait for BattleResult
            while(! _resultDict.ContainsKey(p)) Thread.Sleep(400);

            var returnResult = _resultDict[p];
            _resultDict.Remove(p);

            return returnResult.StringifyResult();
        }

        /// <summary> Fight Battle between two players </summary>
        /// <param name="pOne"> First Player </param>
        /// <param name="pTwo"> Second Player</param>
        /// <returns> BattleResult Instance of the Battle </returns>
        private BattleResult FightBattle(Player pOne, Player pTwo) {
            //Declare necessary Variables
            int roundCount = 1, pOneC, pTwoC;
            ICard oneCard, twoCard, changeCard;
            double dmgOne, dmgTwo;
            var cardIndex = new Random();

            var thisBattleResult = new BattleResult(pOne, pTwo);

            Player winnerPlayer = null, loserPlayer = null, roundWinner;

            //Update GameCount for both players
            pOne.PlayerGc++; pTwo.PlayerGc++;

            //While there are less than 100 Rounds fought
            while (roundCount <= 100) {
                //Check WinCondition (No Cards in Deck)
                if (pOne.CardsInDeck == 0) {
                    winnerPlayer = pTwo;
                    loserPlayer = pOne;
                    break;
                }
                if (pTwo.CardsInDeck == 0) {
                    winnerPlayer = pOne;
                    loserPlayer = pTwo;
                    break;
                }

                //Get Cards and Damage
                roundWinner = null;
                pOneC = cardIndex.Next(0, pOne.CardsInDeck);
                pTwoC = cardIndex.Next(0, pTwo.CardsInDeck);
                oneCard = pOne.PlayerDeck.ElementAt(pOneC).Value;
                twoCard = pTwo.PlayerDeck.ElementAt(pTwoC).Value;
                dmgOne = oneCard.CalcDmgDealt(twoCard);
                dmgTwo = twoCard.CalcDmgDealt(oneCard);

                //check for specials
                if (oneCard.IsSuperior(twoCard))
                    roundWinner = pOne;
                else if (twoCard.IsSuperior((oneCard)))
                    roundWinner = pTwo;

                //Check for Damage for winner
                if (roundWinner == null) {
                    if (dmgOne > dmgTwo)
                        roundWinner = pOne;
                    else if (dmgTwo > dmgOne)
                        roundWinner = pTwo;
                }

                //Add Round to BattleResult
                if (roundWinner == pOne) {
                    changeCard = pTwo.GiveOpponendCard(twoCard.CardId);
                    pOne.AddCardToDeck(changeCard);
                    thisBattleResult.NewRound( new BattleRound(roundCount, pOne, pTwo, oneCard, twoCard, false) );
                }
                else if (roundWinner == pTwo) {
                    changeCard = pOne.GiveOpponendCard(oneCard.CardId);
                    pTwo.AddCardToDeck(changeCard);
                    thisBattleResult.NewRound(new BattleRound(roundCount, pTwo, pOne, twoCard, oneCard, false));
                } else {
                    changeCard = null;
                    thisBattleResult.NewTieRound(roundCount, oneCard, twoCard);
                }
                
                //Update DB
                if(changeCard != null)
                    Program.MyDb.ChangeDeckPlayers(roundWinner.PlayerName, changeCard);

                roundCount++;
            }

            //If the Winner isn't null it's Deck gets cleaned up (max. 4 cards)
            winnerPlayer?.CleanupDeck();

            if (winnerPlayer == null) {
                pOne.PlayerTie++;
                pTwo.PlayerTie++;
                thisBattleResult.TieBattle = true;
            } else {
                thisBattleResult.FinalResult(winnerPlayer, loserPlayer);
                winnerPlayer.Elo += 3;
                winnerPlayer.PlayerWin++;
                //MANDATORY UNIQUE FEATURE
                winnerPlayer.PlayerCoins += 3;

                loserPlayer.Elo = loserPlayer.Elo >= 5 ? loserPlayer.Elo - 5 : 0;
                loserPlayer.PlayerLoss++;
            }
            pOne.BattlesFought.Add(thisBattleResult.BattleId, thisBattleResult);
            pTwo.BattlesFought.Add(thisBattleResult.BattleId, thisBattleResult);
            
            UpdateStats(pOne, pTwo, thisBattleResult);

            return thisBattleResult;
        }

        /// <summary> Update all Stats changed during the battle </summary>
        /// <param name="pone">First Player</param>
        /// <param name="ptwo">second Player </param>
        /// <param name="result">The BattleResult to be saved in the DB</param>
        private void UpdateStats(Player pone, Player ptwo, BattleResult result) {
            //DB Update
            pone.CleanupDeck();
            ptwo.CleanupDeck();

            Program.MyDb.UpdateUserStats(pone);
            Program.MyDb.UpdateUserStats(ptwo);
            Program.MyDb.CreateBattle(result);
        }
        

    }
}
