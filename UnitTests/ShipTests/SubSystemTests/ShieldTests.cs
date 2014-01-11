using Moq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Subsystem;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class ShieldTests
    {
        private readonly Test_Setup _setup = new Test_Setup();
        private Shields _testShields;

       [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();

           _setup.SetupMapWith1Hostile();

           _testShields = new Shields(_setup.TestMap, _setup.TestMap.Playership, _setup.Write);  
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
            _setup.SetupMapWith1Hostile();

            var mockedShields = new Mock<Shields>(_setup.TestMap); 

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
