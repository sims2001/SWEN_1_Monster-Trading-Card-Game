using System.Collections.Generic;
using System.Linq;
using ConsoleTables;
using Newtonsoft.Json;

namespace MTCG_Rutschka {
    /// <summary> Player Class </summary>
    public class Player {
        /// <summary> Constructor </summary>
        /// <param name="n"> Player Name </param>
        /// <param name="c"> Player Coins </param>
        /// <param name="elo"> Player Elo </param>
        /// <param name="gc"> Player GameCount </param>
        /// <param name="w"> Player Wins </param>
        /// <param name="l"> Player Losses </param>
        /// <param name="t"> Player Ties </param>
        public Player(string n, int c, int elo, int gc, int w, int l, int t) {
            PlayerName = n;
            PlayerCoins = c;
            PlayerStack = new Dictionary<string, ICard>();
            PlayerDeck = new Dictionary<string, ICard>();
            BattlesFought = new Dictionary<string, BattleResult>();
            Elo = elo;
            PlayerGc = gc;
            PlayerWin = w;
            PlayerLoss = l;
            PlayerTie = t;
        }

        /// <summary> Add All Cards from a Pack to the DB and the PlayerStack </summary>
        /// <param name="pack"> The Package bought </param>
        public void OpenPack(CardPackage pack) {
            ICard newCard = pack.GetCard();
            do {
                Program.MyDb.PackToStack(PlayerName, newCard);
                PlayerStack.Add(newCard.CardId, newCard);
                newCard = pack.GetCard();
            } while (newCard != null);
            Program.MyDb.PackageSold(pack);
        }

        /// <summary> Add A Card Instance to The Stack </summary>
        /// <param name="card"> ICard Instance </param>
        public void AddCardToStack(ICard card) {
            PlayerStack.Add(card.CardId, card);
        }

        /// <summary> Add A Card Instance to The Deck </summary>
        /// <param name="card"> ICard Instance </param>
        public void AddCardToDeck(ICard card) {
            PlayerDeck.Add(card.CardId, card);
        }

        /// <summary> Remove a Card from the Deck and return itt </summary>
        /// <param name="id"> Card ID to remove </param>
        /// <returns> Card Instance to put into enemy deck </returns>
        public ICard GiveOpponendCard(string id) {
            ICard rCard = PlayerDeck[id];
            PlayerDeck.Remove(id);
            return rCard;
        }

        /// <summary>
        /// Cleanup Deck after Fight (max 4 Cards in Deck)
        /// Also Update DB
        /// </summary>
        public void CleanupDeck() {
            while (PlayerDeck.Count > 4) {
                var toClean = PlayerDeck.ElementAt( PlayerDeck.Count - 1 );
                PlayerStack.Add(toClean.Key, toClean.Value);
                Program.MyDb.DeckToStack(PlayerName, toClean.Value);
                PlayerDeck.Remove(toClean.Key );
            }
        }

        /// <summary> CleanUp the Deck For Tests (No DBHandling) </summary>
        public void CleanupDeckForTest() {
            while (PlayerDeck.Count > 4) {
                var toClean = PlayerDeck.ElementAt(PlayerDeck.Count - 1);
                PlayerStack.Add(toClean.Key, toClean.Value);
                PlayerDeck.Remove(toClean.Key);
            }
        }

        /// <summary> Move Card from Stack to Deck </summary>
        /// <param name="cardId"> The CardID to Move </param>
        /// <returns> Bool if successfull </returns>
        public bool MoveToDeck(string cardId) {
            if (PlayerStack.ContainsKey(cardId)) {
                ICard transfer = PlayerStack[cardId];
                PlayerDeck.Add(cardId, transfer);
                PlayerStack.Remove(cardId);
                Program.MyDb.StackToDeck(PlayerName, transfer);
                return true;
            }
            return false;
        }


        /// <summary> Returns JsonString with all Cards in Stack </summary>
        /// <returns> JSon String with all Cards in Stack </returns>
        public string GetStackCards() {
            if (PlayerStack.Count == 0)
                throw new NoCardsException();

            List<CardSchema> playerCards = new List<CardSchema>();
            foreach(var c in PlayerStack.Values)
                playerCards.Add(new CardSchema(c.CardId, c.CardName, c.CardDamage));

            return JsonConvert.SerializeObject(playerCards); //JsonSerializer.Serialize(PlayerStack);
        }

        /// <summary> Returns Table-String with all Cards in Deck </summary>
        /// <returns> Table String with all Cards in Deck </returns>
        public string GetDeckCards() {
            var table = new ConsoleTable("CARD-NAME", "CARD-TYPE", "CARD-DAMAGE");
            foreach (var card in PlayerDeck) {
                table.AddRow(card.Value.CardName, card.Value.GetElement(), card.Value.CardDamage);
            }

            string playerCards = table.Rows.Count == 0 ? "THERE ARE CURRENTLY NO CARDS IN YOUR DECK!\n" : table.ToString();
            return playerCards;
        }

        /// <summary> Returns JSonString with all Cards in Deck </summary>
        /// <returns> JSonString with all Cards in Deck </returns>
        public string GetDeckCardsJson() {
            if (PlayerDeck.Count == 0)
                throw new NoCardsException();

            List<CardSchema> playerDeck = new List<CardSchema>();
            foreach(var c in PlayerDeck.Values)
                playerDeck.Add(new CardSchema(c.CardId, c.CardName, c.CardDamage));

            return JsonConvert.SerializeObject(playerDeck);
        }

        /// <summary> Remove a card offered for a Trade </summary>
        /// <param name="cardId"> CardID offered to Trade </param>
        /// <returns> Card Instance to Trade </returns>
        public ICard RemoveTradeCard(string cardId) {
            if (PlayerStack.ContainsKey(cardId)) {
                ICard rtCard = PlayerStack[cardId];
                PlayerStack.Remove(cardId);
                return rtCard;
            }

            return null;
        }

        /// <summary> Return a Card from Stack without removing it </summary>
        /// <param name="cardId"> CardID to offer as Trade </param>
        /// <returns> Card Instance </returns>
        public ICard OfferTradeCard(string cardId) {
            if (PlayerStack.ContainsKey(cardId))
                return PlayerStack[cardId];

            return null;
        }

        /// <summary> Add Card To Stack received from Trade </summary>
        /// <param name="nc"> Card Instance from Trade </param>
        public void AddCardFromTrade(ICard nc) {
            PlayerStack.Add(nc.CardId, nc);
        }

        /// <summary> Checks if the Cards are in the Stack </summary>
        /// <param name="karten"> String array with cardIDs </param>
        /// <returns> True or False if all Cards are in the Stack </returns>
        public bool NotAllInStack(string[] karten) {
            foreach(var karte in karten)
                if (!PlayerStack.ContainsKey(karte))
                    return true;

            return false;
        }

        /// <summary> Dictionary with all Cards in the Players Stack </summary>
        public Dictionary<string, ICard> PlayerStack { get; set; }
        /// <summary> Dictionary with all Cards in the Players Deck </summary>
        public Dictionary<string, ICard> PlayerDeck { get; set; }
        /// <summary> Dictionary with all Battles Fought </summary>
        public Dictionary<string, BattleResult> BattlesFought { get; set; }

        /// <summary> The Players Name </summary>
        public string PlayerName { get; private set; }
        /// <summary> The Players Coins </summary>
        public int PlayerCoins { get; set; }
        /// <summary> The Players ELO Points </summary>
        public int Elo { get; set; }
        /// <summary> The Players GameCount</summary>
        public int PlayerGc { get; set; }
        /// <summary> The Players Wins </summary>
        public int PlayerWin { get; set; }
        /// <summary> The Players Losses </summary>
        public int PlayerLoss { get; set; }
        /// <summary> The Players Ties </summary>
        public int PlayerTie { get; set; }
        /// <summary> How many Cards are in the Deck </summary>
        public int CardsInDeck => PlayerDeck.Count;
        /// <summary> How many Cards are in the Stack </summary>
        public int CardsInStack => PlayerStack.Count;
    }

}
