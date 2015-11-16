using NUnit.Framework;
using StarTrek_KG.Subsystem;
using UnitTests.ShipTests;

namespace UnitTests.Tests.SubSystemTests
{
    [TestFixture]
    public class ComputerTests: TestClass_Base
    {
        [SetUp]
        public void Setup()
        {
            _setup.SetupMapWith1Friendly();
            _setup.TestComputer = new Computer(this.Game.Map.Playership); 
        }

        [Test]
        public void ControlsDamaged()
        {
            _setup.TestComputer.Damage = 47;
            _setup.TestComputer.Controls("AHHHHHHHH");
            Assert.IsTrue(_setup.TestComputer.Damaged());
        }

        [Test]
        public void ControlsREC()
        {
            _setup.TestComputer.Controls("rec");
        }

        [Test]
        public void ControlsSTA()
        {
            _setup.TestComputer.Controls("sta");
        }

        [Test]
        public void ControlsTOR()
        {
            _setup.TestComputer.Controls("tor");
        }

        [Test]
        public void ControlsBAS()
        {
            _setup.TestComputer.Controls("bas");
        }

        [Ignore]
        [Test]
        public void ControlsNAV()
        {
            //PromptUser needs to be mocked out for this one to pass
            _setup.TestComputer.Controls("nav");
        }

        [Test]
        public void ControlsInvalid()
        {
            _setup.TestComputer.Controls("xxx");
        }

        [Test]
        public void Repair()
        {
            _setup.TestComputer.Damage = 47;
            var repaired = _setup.TestComputer.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.AreEqual(46, _setup.TestComputer.Damage);
        }

        [Test]
        public void DamageRepaired()
        {
            _setup.TestComputer.Damage = 1;
            var repaired = _setup.TestComputer.PartialRepair();

            Assert.IsTrue(repaired);
            Assert.IsFalse(_setup.TestComputer.Damaged());
        }

        [Test]
        public void NoNeedForDamageRepair()
        {
            _setup.TestComputer.Damage = 0;
            var repaired = _setup.TestComputer.PartialRepair();

            Assert.IsFalse(repaired);
        }

        [Test]
        public void Damaged()
        {
            _setup.TestComputer.Damage = 47;
            Assert.IsTrue(_setup.TestComputer.Damaged());
        }

        [Test]
        public void NotDamaged()
        {
            _setup.TestComputer.Damage = 0;
            Assert.IsFalse(_setup.TestComputer.Damaged());
        }
    }
}
