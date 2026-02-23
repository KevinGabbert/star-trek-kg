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

        [SetUp]
        public void SetUp()
        {
            _setup = new Test_Setup();
            _setup.SetupMapWith1Friendly();
        }

        [Test]
        public void IRS_Shows_Deuterium()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var targetCoordinate = activeSector.Coordinates[1, 0];
            targetCoordinate.Item = CoordinateItem.Deuterium;
            targetCoordinate.Object = new Deuterium(10);

            var shipLocation = _setup.TestMap.Playership.GetLocation();
            var irsData = shipLocation.Sector.GetIRSFullData(shipLocation, _setup.Game);

            var result = irsData.Single(r => r.Point.X == 1 && r.Point.Y == 0);

            Assert.AreEqual(CoordinateItem.Deuterium, result.Item);
            Assert.AreEqual("Deuterium", result.ToScanString());
        }

        [Test]
        public void Deuterium_Is_Consumed_And_Increases_Energy()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var targetCoordinate = activeSector.Coordinates[1, 0];
            targetCoordinate.Item = CoordinateItem.Deuterium;
            targetCoordinate.Object = new Deuterium(25);

            var startingEnergy = _setup.TestMap.Playership.Energy;
            var newLocation = new Location(activeSector, targetCoordinate);

            _setup.TestMap.SetPlayershipInLocation(_setup.TestMap.Playership, _setup.TestMap, newLocation);

            Assert.AreEqual(startingEnergy + 25, _setup.TestMap.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip, targetCoordinate.Item);
            Assert.IsNull(targetCoordinate.Object);
        }

        [Test]
        public void GraviticMine_Shows_On_IRS_And_Damages_Ship()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var mineCoordinate = activeSector.Coordinates[1, 0];
            mineCoordinate.Item = CoordinateItem.GraviticMine;
            mineCoordinate.Object = new GraviticMine();

            var shipLocation = _setup.TestMap.Playership.GetLocation();
            var irsData = shipLocation.Sector.GetIRSFullData(shipLocation, _setup.Game);
            var result = irsData.Single(r => r.Point.X == 1 && r.Point.Y == 0);

            Assert.AreEqual(CoordinateItem.GraviticMine, result.Item);
            Assert.AreEqual("Gravitic Mine", result.ToScanString());

            var startingEnergy = _setup.TestMap.Playership.Energy;
            var newLocation = new Location(activeSector, mineCoordinate);
            _setup.TestMap.SetPlayershipInLocation(_setup.TestMap.Playership, _setup.TestMap, newLocation);

            Assert.AreEqual(startingEnergy - 200, _setup.TestMap.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip, mineCoordinate.Item);
            Assert.IsNull(mineCoordinate.Object);
        }

        [Test]
        public void Deuterium_Is_Consumed_By_Hostile_And_Increases_Energy()
        {
            var activeSector = _setup.TestMap.Sectors.GetActive();
            var targetCoordinate = activeSector.Coordinates[1, 0];
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
    }
}
