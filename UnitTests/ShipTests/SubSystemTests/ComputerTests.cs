using Moq;
using NUnit.Framework;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class ComputerTests
    {
        private Computer _testComputer;

        [SetUp]
        public void Setup()
        {
            _testComputer = new Computer(new Map(new GameConfig
                            {
                                Initialize = true,
                                GenerateMap = true,
                                SectorDefs = new SectorDefs
                                {
                                    new SectorDef(new LocationDef(null, new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                }
                            })); 
        }

        [Test]
        public void ControlsDamaged()
        {
            _testComputer.Damage = 47;
            _testComputer.Controls("AHHHHHHHH");
            Assert.IsTrue(_testComputer.Damaged());
        }

        [Test]
        public void ControlsREC()
        {
            _testComputer.Controls("rec");
        }

        [Test]
        public void ControlsSTA()
        {
            _testComputer.Controls("sta");
        }

        [Test]
        public void ControlsTOR()
        {
            _testComputer.Controls("tor");
        }

        [Test]
        public void ControlsBAS()
        {
            _testComputer.Controls("bas");
        }

        [Ignore]
        [Test]
        public void ControlsNAV()
        {
            //PromptUser needs to be mocked out for this one to pass
            _testComputer.Controls("nav");
        }

        [Test]
        public void ControlsInvalid()
        {
            _testComputer.Controls("xxx");
        }

        [Test]
        public void Repair()
        {
            _testComputer.Damage = 47;
            var repaired = _testComputer.Repair();

            Assert.IsTrue(repaired);
            Assert.AreEqual(46, _testComputer.Damage);
        }

        [Test]
        public void DamageRepaired()
        {
            _testComputer.Damage = 1;
            var repaired = _testComputer.Repair();

            Assert.IsTrue(repaired);
            Assert.IsFalse(_testComputer.Damaged());
        }

        [Test]
        public void NoNeedForDamageRepair()
        {
            _testComputer.Damage = 0;
            var repaired = _testComputer.Repair();

            Assert.IsFalse(repaired);
        }

        [Test]
        public void Damaged()
        {
            _testComputer.Damage = 47;
            Assert.IsTrue(_testComputer.Damaged());
        }

        [Test]
        public void NotDamaged()
        {
            _testComputer.Damage = 0;
            Assert.IsFalse(_testComputer.Damaged());
        }
    }
}
