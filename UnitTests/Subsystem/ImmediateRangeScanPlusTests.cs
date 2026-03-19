using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Playfield;
using StarTrek_KG.Output;
using StarTrek_KG.Types;
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

        [Test]
        public void IRS_In_Nebula_Reveals_Deuterium_In_Base_Scan()
        {
            var sector = this.Game.Map.Sectors.GetActive();
            sector.TransformIntoNebulae();
            var deuteriumCoordinate = sector.GetCoordinate(new Point(1, 0));
            deuteriumCoordinate.Item = CoordinateItem.Deuterium;
            deuteriumCoordinate.Object = new Deuterium(42);

            var output = _interact.ReadAndOutput(this.Game.Map.Playership, "map", "irs");

            Assert.That(output.Any(line => line.Contains("Deuterium (42)")), Is.True);
        }

        [Test]
        public void IRSPlus_In_Nebula_Runs_With_Inaccuracy_Warning()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Point(3, 3));
            _interact = (Interaction)this.Game.Interact;
            _interact.Output.Clear();

            var ship = this.Game.Map.Playership;
            var sector = ship.GetSector();
            sector.TransformIntoNebulae();
            var startingEnergy = ship.Energy;

            var output = _interact.ReadAndOutput(ship, "map", "irs+");

            Assert.AreEqual(startingEnergy - 100, ship.Energy);
            Assert.That(output.Any(line => line.Contains("*** Immediate Range Scan + ***")), Is.True);
            Assert.That(output.Any(line => line.Contains("this scan is inaccurate")), Is.True);
            Assert.That(output.Any(line => line.Contains("ghost images are possible")), Is.True);
        }

        [Test]
        public void IRSPlus_In_Nebula_Produces_Ghost_Images_And_False_Coordinates()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Point(3, 3));
            _interact = (Interaction)this.Game.Interact;
            _interact.Output.Clear();

            var shipLocation = this.Game.Map.Playership.GetLocation();
            shipLocation.Sector.TransformIntoNebulae();
            var deuteriumCoordinate = shipLocation.Sector.GetCoordinate(new Point(4, 4));
            deuteriumCoordinate.Item = CoordinateItem.Deuterium;
            deuteriumCoordinate.Object = new Deuterium(25);

            var scanData = shipLocation.Sector.GetIRSFullData(shipLocation, this.Game, 4).ToList();

            Assert.That(scanData.Any(result => IsNebulaGhost(result.Item)), Is.True);
            Assert.That(scanData.Any(result => result.Item == CoordinateItem.Deuterium &&
                                              (result.Point.X != 4 || result.Point.Y != 4)), Is.True);
        }

        [Test]
        public void IRSPlusPlus_In_Nebula_Has_Heavy_Artifacts_And_Severe_Warning()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Point(3, 3));
            _interact = (Interaction)this.Game.Interact;
            _interact.Output.Clear();

            var ship = this.Game.Map.Playership;
            var shipLocation = ship.GetLocation();
            shipLocation.Sector.TransformIntoNebulae();

            var scanData = shipLocation.Sector.GetIRSFullData(shipLocation, this.Game, 5).ToList();
            var inBoundsData = scanData.Where(result => !result.GalacticBarrier && !result.MyLocation).ToList();
            var unknownCount = inBoundsData.Count(result => result.Unknown);
            var ghostCount = inBoundsData.Count(result => IsNebulaGhost(result.Item));
            var output = _interact.ReadAndOutput(ship, "map", "irs++");

            Assert.That(unknownCount, Is.GreaterThanOrEqualTo(5));
            Assert.That(ghostCount, Is.GreaterThanOrEqualTo(5));
            Assert.That(output.Any(line => line.Contains("severely degraded")), Is.True);
            Assert.That(output.Any(line => line.Contains("ghost images are likely")), Is.True);
        }

        private static bool IsNebulaGhost(CoordinateItem item)
        {
            return item == CoordinateItem.BlackHole ||
                   item == CoordinateItem.GraviticMine ||
                   item == CoordinateItem.GaseousAnomaly ||
                   item == CoordinateItem.TemporalRift ||
                   item == CoordinateItem.HostileOutpost;
        }

        [Test]
        public void IRSPlusPlusPlus_In_Nebula_Is_Blocked_Without_Consuming_Turn_Or_Energy()
        {
            var ship = this.Game.Map.Playership;
            var sector = this.Game.Map.Sectors.GetActive();
            sector.TransformIntoNebulae();
            var startingEnergy = ship.Energy;
            var startingTime = this.Game.Map.timeRemaining;
            var startingStardate = this.Game.Map.Stardate;

            var output = _interact.ReadAndOutput(ship, "map", "irs+++");

            Assert.AreEqual(startingEnergy, ship.Energy);
            Assert.AreEqual(startingTime, this.Game.Map.timeRemaining);
            Assert.AreEqual(startingStardate, this.Game.Map.Stardate);
            Assert.That(output.Any(line => line.Contains("Nebula interference blocks this enhanced scan mode.")), Is.True);
            Assert.That(output.Any(line => line.Contains("*** Immediate Range Scan +++ ***")), Is.False);
        }
    }
}
