namespace MTCG_Rutschka {
    /// <summary> Schema for the UserStats to return to Client </summary>
    public class UserStatsSchema {
        /// <summary> Username </summary>
        public string Name { get; private set; }
        /// <summary> ELO </summary>
        public int Elo { get; private set; }
        /// <summary> Wins </summary>
        public int Wins { get; private set; }
        /// <summary> Losses </summary>
        public int Losses { get; private set; }
        /// <summary> Ties </summary>
        public int Ties { get; private set; }

        /// <summary> Constructor for the UserStatsSchema </summary>
        /// <param name="n"> Name </param>
        /// <param name="elo"> Elo </param>
        /// <param name="w"> Wins </param>
        /// <param name="l"> Losses </param>
        /// <param name="t"> Ties </param>
        public UserStatsSchema(string n, int elo, int w, int l, int t) {
            Name = n;
            Elo = elo;
            Wins = w;
            Losses = l;
            Ties = t;
        }
    }
}
