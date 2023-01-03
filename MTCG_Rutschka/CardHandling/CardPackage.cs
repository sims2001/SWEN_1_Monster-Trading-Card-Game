using System.Collections.Generic;

namespace MTCG_Rutschka {
    /// <summary>Class for CardPackage </summary>
    public class CardPackage {
        /// <summary> A queue containing the Cards in the Pack </summary>
        private Queue<ICard> _package = new Queue<ICard>();

        /// <summary> The Cost of the Package </summary>
        public int PackageCost { get; set; }
        /// <summary> The ID of the Package </summary>
        public string PacketId { get; private set; }

        /// <summary> CardPackage Constructor </summary>
        /// <param name="packetId"> Package ID </param>
        /// <param name="firstCard"> First Card </param>
        /// <param name="secondCard"> Second Card </param>
        /// <param name="thirdCard"> Third Card </param>
        /// <param name="fourthCard"> Fourth Card </param>
        /// <param name="fifthCard"> Fifth Card </param>
        /// <param name="cost"> Cost of the Package </param>
        public CardPackage(string packetId, ICard firstCard, ICard secondCard, ICard thirdCard, ICard fourthCard,
            ICard fifthCard, int cost) {
            PacketId = packetId;
            _package.Enqueue(firstCard);
            _package.Enqueue(secondCard);
            _package.Enqueue(thirdCard);
            _package.Enqueue(fourthCard);
            _package.Enqueue(fifthCard);
            PackageCost = cost;
        }

        /// <summary> Get the First Card of the Pack </summary>
        /// <returns> First Card in Pack </returns>
        public ICard GetCard() {
            return _package.Count > 0 ? _package.Dequeue() : null;
        }

        /// <summary> Get all CardIDs in Pack </summary>
        /// <returns> String Array with all Card IDs</returns>
        public string[] GetCardIDs() {
            List<string> ids = new List<string>();
            foreach (var c in _package) {
                ids.Add(c.CardId);
            }
            return ids.ToArray();
        }
    }
}
