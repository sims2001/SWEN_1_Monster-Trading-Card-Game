namespace MTCG_Rutschka {
    public class Trade {
        /// <summary> The ID of the Trade </summary>
        public string TradeId { get; set; }
        /// <summary> The Player who offers a Card </summary>
        public Player Vendor { get; set; }
        /// <summary> The Card offered </summary>
        public ICard OfferedCard { get; set; }
        /// <summary> The Card Type Requested (Monster/Spell) </summary>
        public CardType RequestedType { get; set; }
        /// <summary> The Element Type Requested (Fire/Water/Regular) </summary>
        public ElementType RequestedElement { get; set; }
        /// <summary> The Damage Requested </summary>
        public double RequestedDamage { get; set; }

        /// <summary> Constructor for a trade </summary>
        /// <param name="id"> TradeID </param>
        /// <param name="anb"> Vendor </param>
        /// <param name="ang"> Card offered </param>
        /// <param name="type"> Requested CardType </param>
        /// <param name="elem"> Requested Element </param>
        /// <param name="dmg"> Requested Damage </param>
        public Trade(string id, Player anb, ICard ang, CardType type, ElementType elem, double dmg = 0.0) {
            TradeId = id;
            Vendor = anb;
            OfferedCard = ang;
            RequestedType = type;
            RequestedElement = elem;
            RequestedDamage = dmg;
        }

        /// <summary> Returns the  Requested CardType as String </summary>
        /// <returns> CardType as String </returns>
        public string TypeString() {
            switch(RequestedType) {
                case CardType.Monster: return "monster";
                case CardType.Spell:   return "spell";
                case CardType.None:
                default: return "";
            }
        }

        /// <summary> Returns the Requested Element as String </summary>
        /// <returns> Element as String </returns>
        public string ElementString() {
            switch(RequestedElement) {
                case ElementType.Fire: return "fire";
                case ElementType.Normal: return "normal";
                case ElementType.Water: return "water";
                case ElementType.None:
                default: return "";
            }
        }
    }
}
