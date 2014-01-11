using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Subsystem;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class NavigationTests: TestClass_Base
    {
        [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();

            _setup.SetupMapWith1Hostile();

            _setup.TestNavigation = new Navigation(_setup.TestMap, _setup.TestMap.Playership, this.Write); 
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
            _setup.TestNavigation.MaxWarpFactor = 8;
            _setup.TestNavigation.Damage = 47;
            _setup.TestNavigation.Controls("AHHHHHHHH");
            Assert.IsTrue(_setup.TestNavigation.Damaged());
        }

        /// <summary>
        /// TODO: For this test to pass, a course needs to be set (user is prompted)
        /// </summary>
        [Ignore]
        [Test]
        public void WarpDriveDamaged()
        {
            _setup.TestNavigation.MaxWarpFactor = 8;
            _setup.TestNavigation.Damage = 47;
            _setup.TestNavigation.Controls("AHHHHHHHH");

            Assert.Less(_setup.TestNavigation.MaxWarpFactor, 8);
            Assert.Greater(_setup.TestNavigation.MaxWarpFactor, 0);
        }

        //For this test to work, InvalidCourseCheck needs to be mocked
        [Ignore]
        [Test]
        public void ControlsInvalid()
        {
            _setup.TestNavigation.Controls("XXXXX");
        }

        [Test]
        public void Repair()
        {
            _setup.TestNavigation.Damage = 47;
            var repaired = _setup.TestNavigation.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.AreEqual(46, _setup.TestNavigation.Damage);
        }

        [Test]
        public void DamageRepaired()
        {
            _setup.TestNavigation.Damage = 1;
            var repaired = _setup.TestNavigation.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.IsFalse(_setup.TestNavigation.Damaged());
        }

        [Test]
        public void NoNeedForDamageRepair()
        {
            _setup.TestNavigation.Damage = 0;
            var repaired = _setup.TestNavigation.PartialRepair();

            Assert.IsFalse(repaired);
        }

        [Test]
        public void Damaged()
        {
            _setup.TestNavigation.Damage = 47;
            Assert.IsTrue(_setup.TestNavigation.Damaged());
        }

        [Test]
        public void NotDamaged()
        {
            _setup.TestNavigation.Damage = 0;
            Assert.IsFalse(_setup.TestNavigation.Damaged());
        }
    }
}
