using System.Linq;
using System.Text;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class RenderTests : TestClass_Base
    {
        private Render _render;

        [SetUp]
        public void SetUp()
        {
            _setup.SetupMapWith1Friendly();
            _render = new Render(Game.Interact, new StarTrekKGSettings());
            Game.Interact.Output.Clear();
        }

        [Test]
        public void CreateSRSViewScreen_WritesRegionName()
        {
            var sector = Game.Map.Sectors.GetActive();
            var shipLocation = Game.Map.Playership.GetLocation();
            var sb = new StringBuilder();

            _render.CreateSRSViewScreen(sector, Game.Map, shipLocation, 0, "Alpha", false, sb);

            Assert.IsTrue(Game.Interact.Output.Queue.Any(q => q.Contains("Alpha")));
        }

        [Test]
        public void OutputScanWarnings_NoHostiles_DoesNotAddHostileWarning()
        {
            var sector = Game.Map.Sectors.GetActive();

            _render.OutputScanWarnings(sector, Game.Map, false);

            Assert.IsFalse(Game.Interact.Output.Queue.Any(q => q.Contains("Hostile")));
        }
    }
}
