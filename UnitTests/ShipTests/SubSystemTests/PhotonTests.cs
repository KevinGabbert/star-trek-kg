using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class PhotonTests
    {
        private readonly Test_Setup _setup = new Test_Setup();
        private Torpedoes _photonsToTest;
        private Quadrant _testQuadrant;

        [Test]
        public void ShootTorpedoOnly()
        {
            _setup.SetupMapWith1Friendly();

            _photonsToTest = Torpedoes.For(_setup.TestMap.Playership);
            _testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            Assert.AreEqual(10, _photonsToTest.Count);

            _photonsToTest.Shoot(7);

            Assert.AreEqual(9, _photonsToTest.Count);
        }

        [Test]
        public void ShootHostileMultiple()
        {
            for (int i = 1; i < 10; i++)
            {
                System.Console.Write("-");
                this.ShootHostileTest(false);
            }
        }

        [Test]
        public void VerifyRunsOutOfTorpedoes()
        {
            _setup.SetupMapWith1Friendly();

            _photonsToTest = Torpedoes.For(_setup.TestMap.Playership);
            _testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            Constants.DEBUG_MODE = true;

            for (int i = 0; i < 10; i++)
            {
                _photonsToTest.Shoot(7);
            }

            Assert.AreEqual(0, _photonsToTest.Count);


            for (int i = 0; i < 10; i++)
            {
                _photonsToTest.Shoot(7);

                //todo: need test to verify output
                //Cannot fire.  Torpedo Room reports no Torpedoes to fire.
            }

            Assert.AreEqual(0, _photonsToTest.Count);
        }

        [Ignore]
        [Test]
        public void ShootHostileVerifyingReturnFire()
        {
            _setup.SetupMapWith1Hostile();

            _photonsToTest = Torpedoes.For(_setup.TestMap.Playership);
            _testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            IShip hostile = _testQuadrant.GetHostiles().Single();

            //Verify ship's location
            Assert.AreEqual(0, hostile.Sector.X);
            Assert.AreEqual(1, hostile.Sector.Y);

            Constants.DEBUG_MODE = true;

            _photonsToTest.Shoot(7);

            var noMoreHostile = _testQuadrant.GetHostiles();

            //Verify ship's location is no more
            Assert.AreEqual(0, noMoreHostile.Count);
        }

        [Ignore]
        [Test]
        public void ShootHostileWithNoReturnFire()
        {
            _setup.SetupMapWith1Hostile();

            _photonsToTest = Torpedoes.For(_setup.TestMap.Playership);
            _testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            IShip hostile = _testQuadrant.GetHostiles().Single();

            //Verify ship's location
            Assert.AreEqual(0, hostile.Sector.X);
            Assert.AreEqual(1, hostile.Sector.Y);

            Constants.DEBUG_MODE = true;

            _photonsToTest.Shoot(7);
            _photonsToTest.Shoot(1);
            _photonsToTest.Shoot(5);
            _photonsToTest.Shoot(3);

            _photonsToTest.Shoot(7);
            _photonsToTest.Shoot(1);
            _photonsToTest.Shoot(5);
            _photonsToTest.Shoot(3);

            var noMoreHostile = _testQuadrant.GetHostiles();

            //Verify ship's location is no more
            Assert.AreEqual(0, noMoreHostile.Count);
        }

        [Test]
        public void ShootHostile()
        {
            ShootHostileTest(true);
        }

        private void ShootHostileTest(bool debugMode)
        {
            _setup.SetupMapWith1Hostile();
            
            var srs = ShortRangeScan.For(_setup.TestMap.Playership);
            srs.Controls();

            _photonsToTest = Torpedoes.For(_setup.TestMap.Playership);
            _testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            IShip hostile = _testQuadrant.GetHostiles().Single();

            Assert.AreEqual(0, _photonsToTest.ShipConnectedTo.Sector.X, "Playership.X not at 0");
            Assert.AreEqual(0, _photonsToTest.ShipConnectedTo.Sector.Y, "Playership.Y not at 0");

            //Verify Hostile ship's location
            Assert.AreEqual(0, hostile.Sector.X, "Hostile.X not at 0");
            Assert.AreEqual(1, hostile.Sector.Y, "Hostile.Y not at 1");

            Assert.AreEqual(SectorItem.Hostile, hostile.Sector.Item);
            Assert.IsNotNull(hostile.Sector.Object);

            Constants.DEBUG_MODE = debugMode;
            _photonsToTest.Shoot(7);

            var noMoreHostile = _testQuadrant.GetHostiles();

            //Verify ship's location is no more
            Assert.AreEqual(0, noMoreHostile.Count, "Hostile Not destroyed.");
        }

        [Test]
        public void ShootHostileE()
        {
            this.ShootHostileAt(new Coordinate(4, 4), new Coordinate(4, 5), 7, true);
        }

        [Test]
        public void ShootHostileSE()
        {
            this.ShootHostileAt(new Coordinate(4, 4), new Coordinate(5, 5), 8, true);
        }

        [Test]
        public void ShootHostileS()
        {
            this.ShootHostileAt(new Coordinate(4, 4), new Coordinate(6, 4), 1, true);
        }

        [Test]
        public void ShootHostileSW()
        {
            this.ShootHostileAt(new Coordinate(4, 4), new Coordinate(5, 3), 2, true);
        }

        [Test]
        public void ShootHostileW()
        {
            this.ShootHostileAt(new Coordinate(4, 4), new Coordinate(4, 3), 3, true);
        }

        [Test]
        public void ShootHostileNW()
        {
            this.ShootHostileAt(new Coordinate(4, 4), new Coordinate(3, 3), 4, true);
        }

        [Test]
        public void ShootHostileN()
        {
            this.ShootHostileAt(new Coordinate(4, 4), new Coordinate(3, 4), 5, true);
        }

        [Test]
        public void ShootHostileNE()
        {
            this.ShootHostileAt(new Coordinate(4, 4), new Coordinate(3, 5), 6, true);
        }

        public void ShootHostileAt(Coordinate friendlySector, Coordinate hostileSector, int directionToShoot, bool debugMode)
        {
            Constants.DEBUG_MODE = false;

            _setup.SetupMapWith1HostileAtSector(friendlySector, hostileSector);

            var srs = ShortRangeScan.For(_setup.TestMap.Playership);
            srs.Controls();

            _photonsToTest = Torpedoes.For(_setup.TestMap.Playership);
            _testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            IShip hostile = _testQuadrant.GetHostiles().Single();

            Assert.AreEqual(friendlySector.X, _photonsToTest.ShipConnectedTo.Sector.X, "Playership.X not at 0");
            Assert.AreEqual(friendlySector.Y, _photonsToTest.ShipConnectedTo.Sector.Y, "Playership.Y not at 0");

            //Verify Hostile ship's location
            Assert.AreEqual(hostileSector.X, hostile.Sector.X, "Hostile.X not at 0");
            Assert.AreEqual(hostileSector.Y, hostile.Sector.Y, "Hostile.Y not at 1");

            Assert.AreEqual(SectorItem.Hostile, hostile.Sector.Item);
            Assert.IsNotNull(hostile.Sector.Object);

            Constants.DEBUG_MODE = debugMode;
            _photonsToTest.Shoot(directionToShoot);

            var noMoreHostile = _testQuadrant.GetHostiles();

            //Verify ship's location is no more
            Assert.AreEqual(0, noMoreHostile.Count, "Hostile Not destroyed.");
        }
    }
}
