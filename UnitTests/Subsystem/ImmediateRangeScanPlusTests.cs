using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Output;
using UnitTests.TestObjects;

namespace UnitTests.Subsystem
{
    [TestFixture]
    public class ImmediateRangeScanPlusTests : TestClass_Base
    {
        private Interaction _interact;

        [SetUp]
        public void SetUp()
        {
            _setup.SetupMapWith1Friendly();
            _interact = (Interaction)this.Game.Interact;
            _interact.Output.Clear();
        }

        [Test]
        public void IRSPlus_Consumes_Configured_Energy()
        {
            var ship = this.Game.Map.Playership;
            var startingEnergy = ship.Energy;

            var output = _interact.ReadAndOutput(ship, "map", "irs+");

            Assert.AreEqual(startingEnergy - 100, ship.Energy);
            Assert.True(output.Any(line => line.Contains("*** Immediate Range Scan + ***")));
        }

        [Test]
        public void IRSPlusPlus_Consumes_Configured_Energy()
        {
            var ship = this.Game.Map.Playership;
            var startingEnergy = ship.Energy;

            var output = _interact.ReadAndOutput(ship, "map", "irs++");

            Assert.AreEqual(startingEnergy - 500, ship.Energy);
            Assert.True(output.Any(line => line.Contains("*** Immediate Range Scan ++ ***")));
        }

        [Test]
        public void IRSPlus_And_IRSPlusPlus_Return_Expected_Cell_Counts()
        {
            var shipLocation = this.Game.Map.Playership.GetLocation();

            var plusData = shipLocation.Sector.GetIRSFullData(shipLocation, this.Game, 4).ToList();
            var plusPlusData = shipLocation.Sector.GetIRSFullData(shipLocation, this.Game, 5).ToList();

            Assert.AreEqual(16, plusData.Count);
            Assert.AreEqual(25, plusPlusData.Count);
        }

        [Test]
        public void IRSPlus_When_Insufficient_Energy_Does_Not_Run()
        {
            var ship = this.Game.Map.Playership;
            ship.Energy = 50;

            var output = _interact.ReadAndOutput(ship, "map", "irs+");

            Assert.AreEqual(50, ship.Energy);
            Assert.True(output.Any(line => line.Contains("Insufficient energy for scan")));
        }

        [Test]
        public void IRSPlusPlusPlus_Consumes_Configured_Energy_And_Renders_Header()
        {
            var ship = this.Game.Map.Playership;
            var startingEnergy = ship.Energy;

            var output = _interact.ReadAndOutput(ship, "map", "irs+++");

            Assert.AreEqual(startingEnergy - 1000, ship.Energy);
            Assert.True(output.Any(line => line.Contains("*** Immediate Range Scan +++ ***")));
        }
    }
}
