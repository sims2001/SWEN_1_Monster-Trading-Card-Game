namespace MTCG_Rutschka {
    /// <summary> MonsterCard </summary>
    public class MonsterCard : ICard{
        /// <summary> Constructor for Monster Card </summary>
        /// <param name="id">CardID</param>
        /// <param name="name">Name</param>
        /// <param name="dmg">Damage</param>
        public MonsterCard(string id, string name, double dmg) {
            CardId = id;
            CardName = name;
            CardDamage = dmg;
            _SetElement();
            Type = CardType.Monster;
        }

        /// <summary> Set the Element of the Card depending on CardName </summary>
        public void _SetElement() {
            switch (CardName) {
                case "Dragon":
                case "FireElf":
                case "FireGoblin":
                case "FireTroll":
                case "Wizard":
                    Element = ElementType.Fire; break;
                case "WaterGoblin":
                case "WaterElf":
                case "WaterTroll":
                case "Kraken":
                    Element = ElementType.Water; break;
                case "Knight":
                case "Ork":
                case "RegularGoblin":
                case "RegularElf":
                case "RegularTroll":
                default:
                    Element = ElementType.Normal; break;
            }
        }

        /// <summary> Calculate the Damage Dealt to the overgiven enemy </summary>
        /// <param name="enemy"> The Enemy to fight against</param>
        /// <returns> Damage against Enemy</returns>
        public double CalcDmgDealt(ICard enemy) {
            if (!enemy.CardName.Contains("Spell") || enemy.Element == Element)
                return CardDamage;

            double retDmg = CardDamage;
            if (enemy.Element == ElementType.Fire)
                retDmg *= Element == ElementType.Normal ? 0.5 : 1.5;
            if (enemy.Element == ElementType.Normal)
                retDmg *= Element == ElementType.Water ? 0.5 : 1.5;
            if (enemy.Element == ElementType.Water)
                retDmg *= Element == ElementType.Fire ? 0.5 : 1.5;

            return retDmg;
        }

        /// <summary> Check if a special effect applies </summary>
        /// <param name="enemy"> Enemy player </param>
        /// <returns>True or false </returns>
        public bool IsSuperior(ICard enemy) {
            if (CardName.Contains("Dragon") && enemy.CardName.Contains("Goblin"))
                return true;
            if (CardName.Contains("Wizard") && enemy.CardName.Contains("Ork"))
                return true;
            if (CardName.Contains("Kraken") && enemy.CardName.Contains("Spell"))
                return true;
            if (CardName.Contains("FireElf") && enemy.CardName.Contains("Dragon"))
                return true;

            return false;
        }

        /// <summary> Get a string Version of the Element </summary>
        /// <returns> Element String </returns>
        public string GetElement() {
            switch (Element) {
                case ElementType.Fire: return "fire";
                case ElementType.Water: return "water";
                case ElementType.Normal:
                default: return "regular";
            }
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
        /// <summary> Get the Type of the Card </summary>
        /// <returns>Type String</returns>
        public string GetType() {
            return "monster";
        }
    }
}
