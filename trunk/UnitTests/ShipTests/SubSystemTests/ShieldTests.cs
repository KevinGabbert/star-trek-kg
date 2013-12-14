using Moq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class ShieldTests
    {
        private Shields _testShields;

       [SetUp]
        public void Setup()
        {
            Constants.SECTOR_MIN = StarTrekKGSettings.GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = StarTrekKGSettings.GetSetting<int>("SECTOR_MAX");

            Constants.QUADRANT_MIN = StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = StarTrekKGSettings.GetSetting<int>("QuadrantMax");

            _testShields = new Shields(new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.Hostile),
                            }
            }));  
        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
        }

        public void TestPhaserHit()
        {
            



        }



        [Test]
        public void ControlsDamaged()
        {
            _testShields.Damage = 47;
            _testShields.Controls("AHHHHHHHH");
            Assert.IsTrue(_testShields.Damaged());
        }

        [Ignore]
        [Test]
        public void ControlsADD()
        {
            _testShields.Controls("add");
            
        }

        [Ignore]
        [Test]
        public void ControlsSUB()
        {
            var mockedShields = new Mock<Shields>(new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.Hostile),
                            }
            })); 

            Assert.AreEqual(0, mockedShields.Object.Energy);

            mockedShields.Setup(s => s.TransferredFromUser()).Returns(1001);

            mockedShields.Object.Controls("sub");

            Assert.AreEqual(1, mockedShields.Object.Energy);
        }

        [Ignore]
        [Test]
        public void ControlsInvalid()
        {
            _testShields.Controls("XXXXX");
        }

        [Test]
        public void Repair()
        {
            _testShields.Damage = 47;
            var repaired = _testShields.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.AreEqual(46, _testShields.Damage);
        }

        [Test]
        public void DamageRepaired()
        {
            _testShields.Damage = 1;
            var repaired = _testShields.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.IsFalse(_testShields.Damaged());
        }

        [Test]
        public void NoNeedForDamageRepair()
        {
            _testShields.Damage = 0;
            var repaired = _testShields.PartialRepair();

            Assert.IsFalse(repaired);
        }

        [Test]
        public void Damaged()
        {
            _testShields.Damage = 47;
            Assert.IsTrue(_testShields.Damaged());
        }

        [Test]
        public void NotDamaged()
        {
            _testShields.Damage = 0;
            Assert.IsFalse(_testShields.Damaged());
        }
    }
}
