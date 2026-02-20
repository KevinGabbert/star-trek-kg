using NUnit.Framework;
using StarTrek_KG.Playfield;
using UnitTests.Playfield.SectorTests;

namespace UnitTests.Playfield.SectorTests
{
    [TestFixture]
    public class SectorsTests: SectorTests_Base
    {
        [SetUp]
        public void Setup()
        {
            _setup.SetupMapWith1Friendly();
        }

        [Test]
        public void IsGalacticBarrier()
        {
            Sectors galaxy = this.Game.Map.Sectors;

            Assert.IsNotNull(galaxy);

            Assert.IsFalse(galaxy.IsGalacticBarrier(new Point(1, 2)));
            Assert.IsFalse(galaxy.IsGalacticBarrier(new Point(0, 0)));
            Assert.IsTrue(galaxy.IsGalacticBarrier(new Point(9, 9)));
            Assert.IsTrue(galaxy.IsGalacticBarrier(new Point(99, 99)));
        }
    }
}

//todo: size of List<Coordinate> this can be derived from the list.count above.
