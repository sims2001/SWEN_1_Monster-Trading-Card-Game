namespace MTCG_Rutschka {
    /// <summary> Schema returned to User </summary>
    public class CardSchema {
        /// <summary> Card ID </summary>
        public string Id { get; private set; }
        /// <summary> Card Name </summary>
        public string Name { get; private set; }
        /// <summary> Card Damage </summary>
        public double Damage { get; private set; }

        /// <summary> Constructor for CardSchema </summary>
        /// <param name="i">ID</param>
        /// <param name="n">Name</param>
        /// <param name="d">Damage</param>
        public CardSchema(string i, string n, double d) {
            Id = i;
            Name = n;
            Damage = d;
        }
    }
}
