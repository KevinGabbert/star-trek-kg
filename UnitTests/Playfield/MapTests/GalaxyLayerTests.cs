using NUnit.Framework;
using UnitTests.TestObjects;

namespace UnitTests.MapTests
{
    [TestFixture]
    public class GalaxyLayerTests : TestClass_Base
    {
        [Test]
        public void Map_Initializes_With_Single_Primary_Galaxy()
        {
            _setup.SetupMapWith1Friendly();
            var map = _setup.TestMap as StarTrek_KG.Playfield.Map;

            Assert.IsNotNull(map);
            Assert.IsNotNull(map.Galaxies);
            Assert.AreEqual(1, map.Galaxies.Count);
            Assert.AreEqual(0, map.CurrentGalaxyId);
            Assert.IsNotNull(map.CurrentGalaxy);
            Assert.AreEqual(0, map.CurrentGalaxy.Id);
            Assert.AreEqual("NGC-100", map.CurrentGalaxy.Name);
        }

        [Test]
        public void MapSectors_Is_Shortcut_To_CurrentGalaxySectors()
        {
            _setup.SetupMapWith1Friendly();
            var map = _setup.TestMap as StarTrek_KG.Playfield.Map;

            Assert.IsNotNull(map);
            Assert.AreSame(map.Sectors, map.CurrentGalaxy.Sectors);
        }
    }
}
