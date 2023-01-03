using System;
using NUnit.Framework;

namespace MTCG_Rutschka.Test {
    /// <summary> Testing The PlayerClass </summary>
    class PlayerTests {
        
        [SetUp]
        public void Setup() { }

        /// <summary> Creates a New Player and Checks if it was successfully initiated </summary>
        [Test]
        public void CreatePlayer() {
            Player newPlayer = new Player("peter", 20, 100, 0, 0, 0, 0);

            Assert.NotNull(newPlayer);
            Assert.AreEqual(0, newPlayer.CardsInStack);
            Assert.AreEqual(0, newPlayer.CardsInDeck);
            Assert.AreEqual("peter", newPlayer.PlayerName);
        }

        /// <summary> Creates a New Player and Adds five new Cards to his Stack </summary>
        [Test]
        public void AddToPlayerStack() {
            var rnd = new Random();

            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            for (var i = 0; i < 5; i++) {
                double d = rnd.NextDouble() * (100 - 5) + 5;
                var id = Guid.NewGuid();
                player.AddCardToStack(new MonsterCard(id.ToString(), "FireGoblin", d));
            }

            var count = player.CardsInStack;

            Assert.AreEqual(count, 5);
        }

        /// <summary> Creates a New Player and Adds three new Cards to his Deck </summary>
        [Test]
        public void AddToDeck() {
            var rnd = new Random();

            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            for (var i = 0; i < 3; i++)
            {
                double d = rnd.NextDouble() * (100 - 5) + 5;
                var id = Guid.NewGuid();
                player.AddCardToDeck(new MonsterCard(id.ToString(), "FireGoblin", d));
            }

            var count = player.CardsInDeck;

            Assert.AreEqual(count, 3);
        }

        /// <summary> Creates a New Player and Adds three new Cards to his Deck and Cleans it up afterwards (max 4 Cards in Deck) </summary>
        [Test]
        public void CleanUpDeck() {
            var rnd = new Random();

            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            for (var i = 0; i < 8; i++) {
                var id = Guid.NewGuid();
                player.AddCardToDeck(new MonsterCard(id.ToString(), "FireGoblin", 10));
            }

            player.CleanupDeckForTest();
            var count = player.CardsInDeck;

            Assert.AreEqual(count, 4);
        }

        /// <summary> Creates a New Player and Adds three new Cards to his Stack, afterwards removes it for a Trade </summary>
        [Test]
        public void RemoveCardForTrade() {
            var rnd = new Random();

            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            
            var id = Guid.NewGuid();
            var c = new MonsterCard(id.ToString(), "FireGoblin", 10);
            player.AddCardToStack(c);

            var anotherC = player.RemoveTradeCard(id.ToString());

            Assert.AreEqual(c, anotherC);
        }

        
    }

    
}
