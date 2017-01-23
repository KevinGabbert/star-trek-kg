using NUnit.Framework;
using StarTrek_KG.Playfield;
using UnitTests.ShipTests.RegionTests;

namespace UnitTests.Tests.RegionsTests
{
    [TestFixture]
    public class RegionsTests: RegionTests_Base
    {
        [SetUp]
        public void Setup()
        {
            _setup.SetupMapWith1Friendly();
        }

        [Test]
        public void IsGalacticBarrier()
        {
            Regions galaxy = this.Game.Map.Regions;

            Assert.IsNotNull(galaxy);

            Assert.IsFalse(galaxy.IsGalacticBarrier(new Coordinate(1, 2)));
            Assert.IsFalse(galaxy.IsGalacticBarrier(new Coordinate(0, 0)));
            Assert.IsTrue(galaxy.IsGalacticBarrier(new Coordinate(9, 9)));
            Assert.IsTrue(galaxy.IsGalacticBarrier(new Coordinate(99, 99)));
        }
    }
}

//todo: size of List<Sector> this can be derived from the list.count above.
