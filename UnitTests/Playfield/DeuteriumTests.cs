using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Playfield;
using StarTrek_KG.Types;
using UnitTests.TestObjects;
using StarTrek_KG.Enums;

namespace UnitTests.Playfield
{
    [TestFixture]
    public class DeuteriumTests
    {
        private Test_Setup _setup;

        private Coordinate AdjacentTestCoordinate()
        {
            var ship = _setup.TestMap.Playership.Coordinate;
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var x = ship.X < 7 ? ship.X + 1 : ship.X - 1;
            return activeSector.Coordinates[x, ship.Y];
        }

        [SetUp]
        public void SetUp()
        {
            _setup = new Test_Setup();
            _setup.SetupMapWith1Friendly();
        }

        [Test]
        public void IRS_Shows_Deuterium()
        {
            var targetCoordinate = AdjacentTestCoordinate();
            targetCoordinate.Item = CoordinateItem.Deuterium;
            targetCoordinate.Object = new Deuterium(10);

            var shipLocation = _setup.TestMap.Playership.GetLocation();
            var irsData = shipLocation.Sector.GetIRSFullData(shipLocation, _setup.Game);

            var result = irsData.Single(r => r.Point != null && r.Point.X == targetCoordinate.X && r.Point.Y == targetCoordinate.Y);

            Assert.AreEqual(CoordinateItem.Deuterium, result.Item);
            Assert.AreEqual("Deuterium (10)", result.ToScanString());
        }

        [Test]
        public void Deuterium_Is_Consumed_And_Increases_Energy()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var targetCoordinate = AdjacentTestCoordinate();
            targetCoordinate.Item = CoordinateItem.Deuterium;
            targetCoordinate.Object = new Deuterium(25);

            var startingEnergy = _setup.TestMap.Playership.Energy;
            var newLocation = new Location(activeSector, targetCoordinate);

            _setup.TestMap.SetPlayershipInLocation(_setup.TestMap.Playership, _setup.TestMap, newLocation);

            Assert.AreEqual(startingEnergy + 25, _setup.TestMap.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip, targetCoordinate.Item);
            Assert.AreSame(_setup.TestMap.Playership, targetCoordinate.Object);
        }

        [Test]
        public void DeuteriumCloud_Is_Consumed_And_Increases_Energy()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var targetCoordinate = AdjacentTestCoordinate();
            targetCoordinate.Item = CoordinateItem.DeuteriumCloud;
            targetCoordinate.Object = new DeuteriumCloud(25);

            var shipLocation = _setup.TestMap.Playership.GetLocation();
            var irsData = shipLocation.Sector.GetIRSFullData(shipLocation, _setup.Game);
            var result = irsData.Single(r => r.Point != null && r.Point.X == targetCoordinate.X && r.Point.Y == targetCoordinate.Y);
            Assert.AreEqual("Deuterium Cloud (25)", result.ToScanString());

            var startingEnergy = _setup.TestMap.Playership.Energy;
            var newLocation = new Location(activeSector, targetCoordinate);

            _setup.TestMap.SetPlayershipInLocation(_setup.TestMap.Playership, _setup.TestMap, newLocation);

            Assert.AreEqual(startingEnergy + 25, _setup.TestMap.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip, targetCoordinate.Item);
            Assert.AreSame(_setup.TestMap.Playership, targetCoordinate.Object);
        }

        [Test]
        public void GraviticMine_Shows_On_IRS_And_Damages_Ship()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var mineCoordinate = AdjacentTestCoordinate();
            mineCoordinate.Item = CoordinateItem.GraviticMine;
            mineCoordinate.Object = new GraviticMine();

            var shipLocation = _setup.TestMap.Playership.GetLocation();
            var irsData = shipLocation.Sector.GetIRSFullData(shipLocation, _setup.Game);
            var result = irsData.Single(r => r.Point != null && r.Point.X == mineCoordinate.X && r.Point.Y == mineCoordinate.Y);

            Assert.AreEqual(CoordinateItem.GraviticMine, result.Item);
            Assert.AreEqual("Gravitic Mine", result.ToScanString());

            var startingEnergy = _setup.TestMap.Playership.Energy;
            var newLocation = new Location(activeSector, mineCoordinate);
            _setup.TestMap.SetPlayershipInLocation(_setup.TestMap.Playership, _setup.TestMap, newLocation);

            Assert.AreEqual(startingEnergy - 200, _setup.TestMap.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip, mineCoordinate.Item);
            Assert.AreSame(_setup.TestMap.Playership, mineCoordinate.Object);
            Assert.IsTrue(_setup.TestMap.Playership.OutputQueue().Any(line => line.Contains("Gravitic mine detonated and damaged the playership.")));
        }

        [Test]
        public void Deuterium_Is_Consumed_By_Hostile_And_Increases_Energy()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var targetCoordinate = AdjacentTestCoordinate();
            targetCoordinate.Item = CoordinateItem.Deuterium;
            targetCoordinate.Object = new Deuterium(30);

            var hostile = new StarTrek_KG.Actors.Ship(StarTrek_KG.TypeSafeEnums.FactionName.Klingon, "TestHostile", targetCoordinate, _setup.TestMap)
            {
                Energy = 400
            };

            var startingEnergy = hostile.Energy;
            activeSector.AddShip(hostile, targetCoordinate);

            Assert.AreEqual(startingEnergy + 30, hostile.Energy);
            Assert.AreEqual(CoordinateItem.HostileShip, targetCoordinate.Item);
            Assert.AreSame(hostile, targetCoordinate.Object);
        }

        [Test]
        public void GraviticMine_Damages_Hostile_And_Outputs_Message()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var mineCoordinate = AdjacentTestCoordinate();
            mineCoordinate.Item = CoordinateItem.GraviticMine;
            mineCoordinate.Object = new GraviticMine();

            var hostile = new StarTrek_KG.Actors.Ship(StarTrek_KG.TypeSafeEnums.FactionName.Klingon, "TestHostile", mineCoordinate, _setup.TestMap)
            {
                Energy = 400
            };

            var startingEnergy = hostile.Energy;
            activeSector.AddShip(hostile, mineCoordinate);

            Assert.Less(hostile.Energy, startingEnergy);
            Assert.IsTrue(_setup.TestMap.Playership.OutputQueue().Any(line => line.Contains($"Enemy ship {hostile.Name} has taken damage from a gravitic mine.")));
        }

        [Test]
        public void IRS_Shows_TechnologyCache()
        {
            var targetCoordinate = AdjacentTestCoordinate();
            targetCoordinate.Item = CoordinateItem.TechnologyCache;
            targetCoordinate.Object = new TechnologyCache(750)
            {
                Coordinate = targetCoordinate
            };

            var shipLocation = _setup.TestMap.Playership.GetLocation();
            var irsData = shipLocation.Sector.GetIRSFullData(shipLocation, _setup.Game);

            var result = irsData.Single(r => r.Point != null && r.Point.X == targetCoordinate.X && r.Point.Y == targetCoordinate.Y);

            Assert.AreEqual(CoordinateItem.TechnologyCache, result.Item);
            Assert.AreEqual("Technology Cache (+750 Max Energy)", result.ToScanString());
        }

        [Test]
        public void TechnologyCache_Is_Consumed_And_Tops_Off_Energy_And_MaxEnergy()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var targetCoordinate = AdjacentTestCoordinate();
            targetCoordinate.Item = CoordinateItem.TechnologyCache;
            targetCoordinate.Object = new TechnologyCache(1200)
            {
                Coordinate = targetCoordinate
            };

            var player = (StarTrek_KG.Actors.Ship)_setup.TestMap.Playership;
            player.Energy = 250;
            var startingMaxEnergy = player.MaxEnergy;
            var newLocation = new Location(activeSector, targetCoordinate);

            _setup.TestMap.SetPlayershipInLocation(player, _setup.TestMap, newLocation);

            Assert.AreEqual(startingMaxEnergy + 1200, player.MaxEnergy);
            Assert.AreEqual(player.MaxEnergy, player.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip, targetCoordinate.Item);
            Assert.AreSame(player, targetCoordinate.Object);
            Assert.IsTrue(player.OutputQueue().Any(line => line.Contains("Technology cache recovered.")));
        }
    }
}
