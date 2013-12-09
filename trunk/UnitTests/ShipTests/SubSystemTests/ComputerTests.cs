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
    public class ComputerTests
    {
        private Computer _testComputer;

        [SetUp]
        public void Setup()
        {
            Constants.SECTOR_MIN = StarTrekKGSettings.GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = StarTrekKGSettings.GetSetting<int>("SECTOR_MAX");

            Constants.QUADRANT_MIN = StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = StarTrekKGSettings.GetSetting<int>("QuadrantMax");

            var locationDef = new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0));

            var sectorDefs = new SectorDefs
                                 {
                                     new SectorDef(locationDef, SectorItem.Friendly),
                                     //todo: this needs to be in a random spo
                                 };

            var gameConfig = new GameConfig
                                 {
                                     Initialize = true,
                                     //GenerateMap = true,
                                     SectorDefs = sectorDefs
                                 };

            var map = new Map(gameConfig);

            _testComputer = new Computer(map); 
        }

        [TearDownAttribute]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
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
