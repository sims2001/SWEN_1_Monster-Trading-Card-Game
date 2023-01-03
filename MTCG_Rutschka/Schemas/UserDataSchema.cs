namespace MTCG_Rutschka {
    /// <summary> Schema for UserData to return to client </summary>
    public class UserDataSchema {
        /// <summary> The Players Real Name </summary>
        public string Name { get; private set; }
        /// <summary> The Players Bio </summary>
        public string Bio { get; private set; }
        /// <summary> The Players Image </summary>
        public string Image { get; private set; }

        /// <summary> Constructor for the UserDataSchema </summary>
        /// <param name="n"> Name </param>
        /// <param name="b"> Bio </param>
        /// <param name="i"> Image </param>
        public UserDataSchema(string n, string b, string i) {
            Name = n;
            Bio = b;
            Image = i;
        }
    }
}
