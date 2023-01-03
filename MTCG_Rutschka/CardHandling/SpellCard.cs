namespace MTCG_Rutschka {
    /// <summary> Spell Card </summary>
    public class SpellCard :ICard {
        /// <summary> Constructor for SpellCard </summary>
        /// <param name="id">CardID</param>
        /// <param name="name">Name</param>
        /// <param name="dmg">Damage</param>
        public SpellCard(string id, string name, double dmg) {
            CardId = id;
            CardName = name;
            CardDamage = dmg;
            _SetElement();
            Type = CardType.Spell;
        }

        /// <summary> Set the Element of the Card depending on CardName </summary>
        public void _SetElement() {
            switch (CardName.Replace("Spell", "")) {
                case "Fire":
                    Element = ElementType.Fire; break;
                case "Water":
                    Element = ElementType.Water; break;
                case "Regular":
                default:
                    Element = ElementType.Normal; break;
            }
        }

        /// <summary> Calculate the Damage Dealt to the overgiven enemy </summary>
        /// <param name="enemy"> The Enemy to fight against</param>
        /// <returns> Damage against Enemy</returns>
        public double CalcDmgDealt(ICard enemy) {
            if (enemy.Element == Element)
                return CardDamage;

            double retDmg = CardDamage;
            if(enemy.Element == ElementType.Fire)
                retDmg *= Element == ElementType.Normal ? 0.5 : 1.5;
            if(enemy.Element == ElementType.Normal)
                retDmg *= Element == ElementType.Water ? 0.5 : 1.5;
            if (enemy.Element == ElementType.Water)
                retDmg *= Element == ElementType.Fire ? 0.5 : 1.5;

            return retDmg;
        }
        /// <summary> Check if a special effect applies </summary>
        /// <param name="enemy"> Enemy player </param>
        /// <returns>True or false </returns>
        public bool IsSuperior(ICard enemy) {
            if (Element == ElementType.Water && enemy.CardName.Contains("Knight"))
                return true;
            
            return false;
        }

        /// <summary> Get a string Version of the Element </summary>
        /// <returns> Element String </returns>
        public string GetElement() {
            switch (Element)
            {
                case ElementType.Fire: return "fire";
                case ElementType.Water: return "water";
                case ElementType.Normal:
                default: return "regular";
            }
        }
        /// <summary> Get the Type of the Card </summary>
        /// <returns>Type String</returns>
        public string GetType() {
            return "spell";
        }
        /// <summary> ID for CardHandling in DB and Battles </summary>
        public string CardId { get; set; }
        /// <summary> Name of the Card </summary>
        public string CardName { get; set; }
        /// <summary> Damage of the Card </summary>
        public double CardDamage { get; set; }
        /// <summary> Element of the Card (ENUM) </summary>
        public ElementType Element { get; set; }
        /// <summary> Type of the Card (ENUM) </summary>
        public CardType Type { get; set; }
    }

    
}
