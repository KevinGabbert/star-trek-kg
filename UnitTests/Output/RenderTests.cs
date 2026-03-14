using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
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

        [Test]
        public void SRS_Render_Colors_Active_Playership_Glyph_Using_Configured_Color()
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
            var expectedColor = map.Game.Config.GetSetting<string>("PlayerShipColor");

            Assert.That(lines.Any(line => line.Contains($"[[;{expectedColor};]{DEFAULTS.PLAYERSHIP}]")), Is.True);
        }

        [Test]
        public void SRS_Header_Shows_Quadrant_Symbol_And_Location_Format()
        {
            var setup = new Test_Setup();
            setup.SetupMapWith1FriendlyAtSector(new Point(6, 5));

            var map = setup.TestMap;
            var render = new Render(map.Game.Interact, map.Game.Config);
            var sector = map.Playership.GetLocation().Sector;
            var location = map.Playership.GetLocation();
            var sb = new StringBuilder();

            map.Game.Interact.Output.Clear();

            render.CreateSRSViewScreen(sector, map, location, 0, sector.Name, false, sb);

            var lines = map.Game.Interact.Output.Queue.ToList();
            Assert.That(lines.Any(line => line.Contains($"Sector: {sector.Name}")), Is.True);
            Assert.That(lines.Any(line => line.Contains($"Coordinate: [{location.Coordinate.X},{location.Coordinate.Y}]")), Is.True);
            Assert.That(lines.Any(line => line.Contains($"\u00A7{location.Sector.X}.{location.Sector.Y}")), Is.True);
        }

        [Test]
        public void Named_Lrs_Render_Uses_Quadrant_Symbol_In_Coordinate_Line()
        {
            var setup = new Test_Setup();
            setup.SetupMapWith1Friendly();

            var map = setup.TestMap;
            var result = new LRSResult
            {
                Point = new Point(6, 4),
                SectorName = "Mariner",
                Name = "Mariner",
                QuadrantName = "Delta",
                Hostiles = 1,
                Starbases = 0,
                Stars = 2
            };

            var rendered = map.Game.Interact.RenderScanWithNames(
                ScanRenderType.SingleLine,
                "*** Long Range Scan ***",
                new List<IScanResult> { result },
                map.Game).ToList();

            var quadrantSymbol = QuadrantRules.GetQuadrantSymbol("Delta");
            Assert.That(rendered.Any(line => line.Contains($"{quadrantSymbol}\u00A76.4")), Is.True);
            Assert.That(rendered.Any(line => line.Contains("[Delta]")), Is.False);
        }

        [Test]
        public void Ccrs_Subsystem_Display_Order_Matches_Rendered_Alphabetical_Order()
        {
            var order = Render.GetCrsSubsystemDisplayOrder().ToList();

            Assert.That(order.First(), Is.EqualTo("Combined Range Scan"));
            Assert.That(order.Last(), Is.EqualTo("Warp Drive"));
            Assert.That(order, Does.Contain("Shields"));
            Assert.That(order, Does.Contain("Immediate Range Scan"));
            Assert.That(order.Count, Is.EqualTo(13));
        }
    }
}
