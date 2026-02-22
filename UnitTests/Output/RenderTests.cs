using System.Linq;
using System.Text;
using NUnit.Framework;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class RenderTests
    {
        [Test]
        public void SRS_Render_UsesYAxisForRows()
        {
            var setup = new Test_Setup();
            setup.SetupMapWith1FriendlyAtSector(new Point(2, 1));

            var map = setup.TestMap;
            var render = new Render(map.Game.Interact, map.Game.Config);
            var sector = map.Sectors.GetActive();
            var location = map.Playership.GetLocation();
            var sb = new StringBuilder();

            map.Game.Interact.Output.Clear();

            render.CreateSRSViewScreen(sector, map, location, 0, sector.Name, false, sb);

            var lines = map.Game.Interact.Output.Queue.ToList();

            var expectedRowIndex = 1 + location.Coordinate.Y;
            Assert.IsTrue(lines[expectedRowIndex].Contains(DEFAULTS.PLAYERSHIP), "Playership not rendered on expected row.");

            if (location.Coordinate.X != location.Coordinate.Y)
            {
                var wrongRowIndex = 1 + location.Coordinate.X;
                Assert.IsFalse(lines[wrongRowIndex].Contains(DEFAULTS.PLAYERSHIP), "Playership rendered on X row instead of Y row.");
            }
        }
    }
}
