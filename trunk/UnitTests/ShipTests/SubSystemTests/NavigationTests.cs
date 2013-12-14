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
    public class NavigationTests
    {
        private Navigation _testNavigation;

        [SetUp]
        public void Setup()
        {
            Constants.SECTOR_MIN = StarTrekKGSettings.GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = StarTrekKGSettings.GetSetting<int>("SECTOR_MAX");

            Constants.QUADRANT_MIN = StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = StarTrekKGSettings.GetSetting<int>("QuadrantMax");

            _testNavigation = new Navigation(new Map(new GameConfig
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

        //For this test to work, InvalidCourseCheck needs to be mocked
        [Ignore]
        [Test]
        public void ControlsDamaged()
        {
            _testNavigation.MaxWarpFactor = 8;
            _testNavigation.Damage = 47;
            _testNavigation.Controls("AHHHHHHHH");
            Assert.IsTrue(_testNavigation.Damaged());
        }

        /// <summary>
        /// TODO: For this test to pass, a course needs to be set (user is prompted)
        /// </summary>
        [Ignore]
        [Test]
        public void WarpDriveDamaged()
        {
            _testNavigation.MaxWarpFactor = 8;
            _testNavigation.Damage = 47;
            _testNavigation.Controls("AHHHHHHHH");

            Assert.Less(_testNavigation.MaxWarpFactor, 8);
            Assert.Greater(_testNavigation.MaxWarpFactor, 0);
        }

        //For this test to work, InvalidCourseCheck needs to be mocked
        [Ignore]
        [Test]
        public void ControlsInvalid()
        {
            _testNavigation.Controls("XXXXX");
        }

        [Test]
        public void Repair()
        {
            _testNavigation.Damage = 47;
            var repaired = _testNavigation.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.AreEqual(46, _testNavigation.Damage);
        }

        [Test]
        public void DamageRepaired()
        {
            _testNavigation.Damage = 1;
            var repaired = _testNavigation.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.IsFalse(_testNavigation.Damaged());
        }

        [Test]
        public void NoNeedForDamageRepair()
        {
            _testNavigation.Damage = 0;
            var repaired = _testNavigation.PartialRepair();

            Assert.IsFalse(repaired);
        }

        [Test]
        public void Damaged()
        {
            _testNavigation.Damage = 47;
            Assert.IsTrue(_testNavigation.Damaged());
        }

        [Test]
        public void NotDamaged()
        {
            _testNavigation.Damage = 0;
            Assert.IsFalse(_testNavigation.Damaged());
        }
    }
}
