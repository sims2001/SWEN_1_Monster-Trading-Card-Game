using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace MTCG_Rutschka.Test {
    /// <summary> Testing Cards and Corresponding Functions </summary>
    public class CardTests {
        [SetUp]
        public void Setup() {}

        /// <summary> Initiates a new MonsterCard and checks the validity </summary>
        [Test]
        public void CreateMonsterCard() {
            ICard waterGoblin = new MonsterCard(Guid.NewGuid().ToString(), "WaterGoblin", 10);

            Assert.NotNull(waterGoblin);
            Assert.IsTrue(waterGoblin.Element == ElementType.Water);
            Assert.IsTrue(waterGoblin.Type == CardType.Monster);
            Assert.AreEqual("monster", waterGoblin.GetType());
            Assert.AreEqual("water", waterGoblin.GetElement());
        }

        /// <summary> Initiates a new SpellCard and checks the validity </summary>
        [Test]
        public void CreateFireSpellCard() {
            ICard fireSpell = new SpellCard(Guid.NewGuid().ToString(), "FireSpell", 10);

            Assert.NotNull(fireSpell);
            Assert.IsTrue(fireSpell.Element == ElementType.Fire);
            Assert.IsTrue(fireSpell.Type == CardType.Spell);
            Assert.AreEqual("spell", fireSpell.GetType());
            Assert.AreEqual("fire", fireSpell.GetElement());
        }

        /// <summary> Initiates two MonsterCards and calculates the Damages they would Deal to each other </summary>
        [Test]
        public void CalcMonsterDamage() {
            ICard waterGoblin = new MonsterCard(new Guid().ToString(), "WaterGoblin", 10.5);
            ICard fireElf = new MonsterCard(new Guid().ToString(), "FireElf", 22.3);

            var goblinDmg = waterGoblin.CalcDmgDealt(fireElf);
            var elfDmg = fireElf.CalcDmgDealt(waterGoblin);

            Assert.AreEqual(goblinDmg, 10.5);
            Assert.AreEqual(elfDmg, 22.3);
        }

        /// <summary> Initiates a Monster- and a SpellCard and calculates the Damages they would Deal to each other </summary>
        [Test]
        public void CalcSpellVsMonsterDmg() {
            ICard waterGoblin = new MonsterCard(new Guid().ToString(), "WaterGoblin", 20);
            ICard fireSpell = new SpellCard(new Guid().ToString(), "FireSpell", 10);

            var goblinDmg = waterGoblin.CalcDmgDealt(fireSpell);
            var spellDmg = fireSpell.CalcDmgDealt(waterGoblin);

            Assert.AreEqual(goblinDmg, 30);
            Assert.AreEqual(spellDmg, 5);
        }

        [Test]
        /// <summary> Initiates two SpellCards and calculates the Damages they would Deal to each other </summary>
        public void CalcSpellVsSpellDmg() {
            ICard regularSpell = new SpellCard(new Guid().ToString(), "RegularSpell", 20);
            ICard fireSpell = new SpellCard(new Guid().ToString(), "FireSpell", 10);

            var regularDmg = regularSpell.CalcDmgDealt(fireSpell);
            var fireDmg = fireSpell.CalcDmgDealt(regularSpell);

            Assert.AreEqual(regularDmg, 10);
            Assert.AreEqual(fireDmg, 15);
        }

        /// <summary> Checks if two equal Cards have a superiority above the other </summary>
        [Test]
        public void NotSuperior() {
            ICard waterSpell = new SpellCard(new Guid().ToString(), "WaterSpell", 10);
            ICard regularGoblin = new MonsterCard(new Guid().ToString(), "RegularGoblin", 20);

            var waterSup = waterSpell.IsSuperior(regularGoblin);
            var regSup = regularGoblin.IsSuperior(waterSpell);

            Assert.AreEqual(waterSup, false);
            Assert.AreEqual(regSup, false);
        }

        /// <summary> Checks if a superior Card has a superiority above the other </summary>
        [Test]
        public void SpellSuperior() {
            ICard waterSpell = new SpellCard(new Guid().ToString(), "WaterSpell", 10);
            ICard knight = new MonsterCard(new Guid().ToString(), "Knight", 20);

            var waterSup = waterSpell.IsSuperior(knight);
            var regSup = knight.IsSuperior(waterSpell);

            Assert.AreEqual(waterSup, true);
            Assert.AreEqual(regSup, false);
        }

        /// <summary> Checks if the Kraken is Superior to SpellCards </summary>
        [Test]
        public void KrakenSuperior() {
            ICard waterSpell = new SpellCard(new Guid().ToString(), "WaterSpell", 10);
            ICard kraken = new MonsterCard(new Guid().ToString(), "Kraken", 20);

            var waterSup = waterSpell.IsSuperior(kraken);
            var regSup = kraken.IsSuperior(waterSpell);

            Assert.AreEqual(waterSup, false);
            Assert.AreEqual(regSup, true);
        }

        /// <summary> Test if a Package can successfully be created </summary>
        [Test]
        public void TestPackageCreation() {
            var rnd = new Random();
            Queue<ICard> toPackage = new Queue<ICard>();

            for (var i = 0; i < 5; i++)
                toPackage.Enqueue(new Mock<ICard>().Object);

            CardPackage pack = new CardPackage(new Guid().ToString(), toPackage.Dequeue(), toPackage.Dequeue(), toPackage.Dequeue(), toPackage.Dequeue(), toPackage.Dequeue(), 5);

            Assert.That(pack.PackageCost, Is.EqualTo(5));
            Assert.That(pack.GetCardIDs(), Is.Not.Empty);
        }

        /// <summary> Tests to Add a freshly created Package to the Shop </summary>
        [Test]
        public void AddPackToShop() {
            Queue<ICard> toPackage = new Queue<ICard>();

            for (var i = 0; i < 5; i++)
                toPackage.Enqueue(new Mock<ICard>().Object);

            var id = Guid.NewGuid().ToString();

            CardPackage pack = new CardPackage(id, toPackage.Dequeue(), toPackage.Dequeue(), toPackage.Dequeue(), toPackage.Dequeue(), toPackage.Dequeue(), 5);

            CardShop shop = new CardShop();
            shop.AddPackageToShop(pack);

            var cnt = shop.PacksInShop;
            var allIDs = shop.GetPackIDs();
            var theId = allIDs[0];

            Assert.AreEqual(1, cnt);
            Assert.AreEqual(id, theId);
        }
    }
}