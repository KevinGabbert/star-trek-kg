using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Config;
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
        public void ShootHostile()
        {
            _setup.SetupMapWith1Hostile();

            _photonsToTest = Torpedoes.For(_setup.TestMap.Playership);
            _testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            IShip hostile = _testQuadrant.GetHostiles().Single();

            //Verify ship's location
            Assert.AreEqual(0, hostile.Sector.X);
            Assert.AreEqual(1, hostile.Sector.Y);

            _photonsToTest.Shoot(7);

            var noMoreHostile = _testQuadrant.GetHostiles();

            //Verify ship's location is no more
            Assert.AreEqual(0, noMoreHostile.Count);
        }
    }
}
