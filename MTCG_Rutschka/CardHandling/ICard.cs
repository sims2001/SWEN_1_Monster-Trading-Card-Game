namespace MTCG_Rutschka {
    /// <summary> Enum with all available Elements and None for Trade-Handling</summary>
    public enum ElementType {
        Normal,
        Fire,
        Water,
        None
    }

    /// <summary> Enum for card Type handling and None for Trade-Handling </summary>
    public enum CardType {
        Monster,
        Spell,
        None
    }

    /// <summary> Card Interface </summary>
    public interface ICard {
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
        /// <summary> Set the Element of the Card </summary>
        public void _SetElement();
        /// <summary> Calculate the Damage Dealt to the overgiven enemy </summary>
        /// <param name="enemy"> The Enemy to fight against</param>
        /// <returns> Damage against Enemy</returns>
        public double CalcDmgDealt(ICard enemy);
        /// <summary> Get a string Version of the Element </summary>
        /// <returns> Element String </returns>
        public string GetElement();
        /// <summary> Get the Type of the Card </summary>
        /// <returns>Type String</returns>
        public string GetType();
        /// <summary> Check if a special effect applies </summary>
        /// <param name="enemy"> Enemy player </param>
        /// <returns>True or false </returns>
        public bool IsSuperior(ICard enemy);

    }
}
