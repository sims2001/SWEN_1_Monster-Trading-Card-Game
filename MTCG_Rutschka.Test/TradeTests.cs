using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;

namespace MTCG_Rutschka.Test {
    /// <summary> Implements Tests for the Trade and TradeBank class </summary>
    class TradeTests {
        
        [SetUp]
        public void Setup() { }

        /// <summary> Registers a new Trade in the TradeBank </summary>
        [Test]
        public void RegisterTrade() {
            TradeBank bank = new TradeBank();

            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            var id = Guid.NewGuid();
            var c = new MonsterCard(id.ToString(), "FireGoblin", 5);
            player.AddCardToStack(c);
            var tid = Guid.NewGuid().ToString();

            Trade trade = null;
            try {
                bank.RegisterTestTrade(tid, player, player.RemoveTradeCard(id.ToString()), CardType.None,
                    ElementType.Fire, 2);
                trade = bank.GetTrade(tid);

                Assert.AreEqual(trade.OfferedCard, c);
            }
            catch (NoTradingDealException exc) {
                Assert.AreEqual("Trade does not exist", exc.Message);
            }
            catch (TradeAlreadyExistsException exc) {
                Assert.AreEqual("Trade already exists", exc.Message);
            }

            Assert.NotNull(bank);
        }

        /// <summary> Registers and Deletes a Trade in the TradeBank </summary>
        [Test]
        public void DeleteTrade() {
            TradeBank bank = new TradeBank();

            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            var id = Guid.NewGuid().ToString();
            var c = new MonsterCard(id, "FireGoblin", 5);
            player.AddCardToStack(c);
            var tid = Guid.NewGuid().ToString();

            Trade trade = null;
            try
            {
                bank.RegisterTestTrade(tid, player, player.RemoveTradeCard(id), CardType.None,
                    ElementType.Fire, 2);
                
                bank.DeleteTestTrade(tid, player);

                var cnt = bank.TradeCount;
                Assert.AreEqual(cnt, 0);
                Assert.AreEqual(c, player.OfferTradeCard(id));
            }
            catch (NoTradingDealException exc) {
                Assert.AreEqual("Trade does not exist", exc.Message);
            }
            catch (TradeAlreadyExistsException exc) {
                Assert.AreEqual("Trade already exists", exc.Message);
            }
            catch (InvalidTradeCardException) {
                var cnt = bank.TradeCount;
                Assert.AreEqual(1, cnt);
            }
        }

        /// <summary> Create a new Trade without TradeBank </summary>
        [Test]
        public void CreateTrade() {
            var tid = Guid.NewGuid().ToString();
            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            var id = Guid.NewGuid().ToString();
            var c = new MonsterCard(id, "FireGoblin", 5);
            Trade trd = new Trade(tid, player, c, CardType.Monster, ElementType.Fire, 0);

            Assert.NotNull(trd);
            Assert.AreEqual(player, trd.Vendor);
            Assert.AreEqual(c, trd.OfferedCard);
        }

        /// <summary> Checks if the Element of a Trade is successfully Set </summary>
        [Test]
        public void CheckElement() {
            var tid = Guid.NewGuid().ToString();
            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            var id = Guid.NewGuid().ToString();
            var c = new MonsterCard(id, "FireGoblin", 5);
            Trade trd = new Trade(tid, player, c, CardType.Monster, ElementType.Fire, 0);

            var elem = trd.ElementString();
            Assert.AreEqual("fire", elem);
        }

        /// <summary> Checks if the Type of a Trade is successfully Set </summary>
        [Test]
        public void CheckType() {
            var tid = Guid.NewGuid().ToString();
            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            var id = Guid.NewGuid().ToString();
            var c = new MonsterCard(id, "FireGoblin", 5);
            Trade trd = new Trade(tid, player, c, CardType.Monster, ElementType.Fire, 0);

            var type = trd.TypeString();
            Assert.AreEqual("monster", type);
        }

        /// <summary> Checks if the Damage of a Trade is successfully Set </summary>
        [Test]
        public void CheckDamage() {
            var tid = Guid.NewGuid().ToString();
            var player = new Player("peter", 20, 0, 0, 0, 0, 0);
            var id = Guid.NewGuid().ToString();
            var c = new MonsterCard(id, "FireGoblin", 5);
            Trade trd = new Trade(tid, player, c, CardType.Monster, ElementType.Fire, 25);

            var dmg = trd.RequestedDamage;
            Assert.AreEqual(25, dmg);
        }
    }
}
