namespace MTCG_Rutschka {
    /// <summary> Individual Round in a Battle </summary>
    public class BattleRound {
        /// <summary> Winner of the Round </summary>
        public Player RoundWinner { get; private set; }
        /// <summary> Loserof the Round </summary>
        public Player RoundLoser { get; private set; }
        /// <summary> Was the Round a Tie?</summary>
        public bool Tie { get; private set; }
        /// <summary> The Card that won the Round </summary>
        public ICard WinnerCard { get; private set; }
        /// <summary> The Card that lost the Round </summary>
        public ICard LoserCard { get; private set; }
        /// <summary> The Round Number </summary>
        public int RoundNum { get; private set; }

        /// <summary> Constructor </summary>
        /// <param name="rn"> RoundNumber </param>
        /// <param name="winner"> RoundWinner </param>
        /// <param name="loser"> RoundLoser </param>
        /// <param name="winCard"> WinnerCard </param>
        /// <param name="lossCard"> LoserCard </param>
        /// <param name="tie"> Tie or not </param>
        public BattleRound(int rn, Player winner, Player loser, ICard winCard, ICard lossCard, bool tie) {
            RoundNum = rn;
            RoundWinner = winner;
            RoundLoser = loser;
            Tie = tie;
            WinnerCard = winCard;
            LoserCard = lossCard;
        }

    }
}
