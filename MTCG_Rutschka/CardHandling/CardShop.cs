using System.Collections.Generic;

namespace MTCG_Rutschka {
    /// <summary> Card Shop Class to Handle the buying of Cards </summary>
    public class CardShop {
        /// <summary> A Queue with all Packages in the shop </summary>
        private Queue<CardPackage> _shopContent = new Queue<CardPackage>();

        /// <summary> If the Buyer has enough money a Pack will be added to his Stack </summary>
        /// <exception cref="NoCardPackageException"> Thrown when there are no more packs in the Shop </exception>
        /// <exception cref="NotEnoughMoneyException"> Thrown when the Player doesn't have enough Coins</exception>
        /// <param name="buyer"> The player that wants to buy a package </param>
        public void BuyCardPackage(Player buyer) {
            if(buyer.PlayerCoins >= 5){
                if (_shopContent.Count > 0) {
                    buyer.OpenPack(_shopContent.Dequeue());
                    buyer.PlayerCoins -= 5;
                } else 
                    throw new NoCardPackageException();
            } else 
                throw new NotEnoughMoneyException();
        }

        /// <summary> Add a Package to the Shop </summary>
        /// <param name="pack"> the Pack to Enqueue in the Shop </param>
        public void AddPackageToShop(CardPackage pack) {
            _shopContent.Enqueue(pack);
        }

        /// <summary> Return all Pack IDs in shop </summary>
        /// <returns> String Array with IDs</returns>
        public string[] GetPackIDs() {
            List<string> ids = new List<string>();
            foreach (var p in _shopContent) {
                ids.Add(p.PacketId);
            }
            return ids.ToArray();
        }

        /// <summary> How many Packs are in the Shop </summary>
        public int PacksInShop => _shopContent.Count;
    }
}
