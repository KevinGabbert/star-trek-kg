using NUnit.Framework;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using UnitTests.TestObjects;

namespace UnitTests.Subsystem
{
    [TestFixture]
    public class ShieldTests : TestClass_Base
    {
       [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();
            DEFAULTS.DEBUG_MODE = false;
           _setup.SetupMapWith1Hostile();

           _setup.TestShields = new Shields(_setup.TestMap.Playership);  
        }

        public void TestPhaserHit()
        {

        }

        //todo: needs a mock set up to read interaction output
        [Test]
        public void ControlsDamaged()
        {
            _setup.TestShields.Damage = 47;
            _setup.TestShields.Controls("AHHHHHHHH");

            Assert.IsTrue(_setup.TestShields.Damaged());

            //todo:  read the following message from IInteraction
            //(see Test_ShipObject for example)

            //"Shields are Damaged. DamageLevel: {this.Damage}"
        }

        [Ignore("")]
        [Test]
        public void ControlsADD()
        {
            _setup.TestShields.Controls("add"); 
        }

        //[Ignore("")]
        //[Test]
        //public void ControlsSUB()
        //{
        //    _setup.SetupMapWith1Hostile();

        //    var mockedShields = new Mock<Shields>(_setup.TestMap); 

        //    Assert.AreEqual(0, mockedShields.Object.Energy);

        //    mockedShields.Setup(s => s.GetValueFromUser()).Returns(1001);

        //    mockedShields.Object.Controls("sub");

        //    Assert.AreEqual(1, mockedShields.Object.Energy);
        //}

        [Ignore("")]
        [Test]
        public void ControlsInvalid()
        {
            _setup.TestShields.Controls("XXXXX");
        }

        [Test]
        public void Repair()
        {
            _setup.TestShields.Damage = 47;
            var repaired = _setup.TestShields.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.AreEqual(46, _setup.TestShields.Damage);
        }

        [Test]
        public void DamageRepaired()
        {
            _setup.TestShields.Damage = 1;
            var repaired = _setup.TestShields.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.IsFalse(_setup.TestShields.Damaged());
        }

        [Test]
        public void NoNeedForDamageRepair()
        {
            _setup.TestShields.Damage = 0;
            var repaired = _setup.TestShields.PartialRepair();

            Assert.IsFalse(repaired);
        }

        [Test]
        public void Damaged()
        {
            _setup.TestShields.Damage = 47;
            Assert.IsTrue(_setup.TestShields.Damaged());
        }

        [Test]
        public void NotDamaged()
        {
            _setup.TestShields.Damage = 0;
            Assert.IsFalse(_setup.TestShields.Damaged());
        }
    }
}
