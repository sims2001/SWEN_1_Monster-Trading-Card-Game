namespace MTCG_Rutschka {
    /// <summary> Schema for Trades to return to User </summary>
    public class TradingDealSchema {
        /// <summary> Trade ID </summary>
        public string Id { get; private set; }
        /// <summary> The CardID offered to Trade </summary>
        public string CardToTrade { get; private set; }
        /// <summary> Requested Type </summary>
        public string Type { get; private set; }
        /// <summary> Requested Element </summary>
        public string Element { get; private set; }
        /// <summary> Requested Minimum Damage </summary>
        public double MinDamage { get; private set; }

        /// <summary> Constructor for the Trading Deal Schema </summary>
        /// <param name="i"> ID </param>
        /// <param name="ctt"> Card ID to Trade</param>
        /// <param name="t"> Type </param>
        /// <param name="e"> Element </param>
        /// <param name="d"> Damage </param>
        public TradingDealSchema(string i, string ctt, string t, string e, double d) {
            Id = i;
            CardToTrade = ctt;
            Type = t;
            Element = e;
            MinDamage = d;
        }
    }
}
