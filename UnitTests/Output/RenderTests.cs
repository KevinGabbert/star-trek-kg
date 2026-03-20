using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
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
            var playerGlyph = map.Game.Config.GetSetting<string>("PlayerShipGlyph");

            var expectedRowIndex = 1 + location.Coordinate.Y;
            Assert.IsTrue(lines[expectedRowIndex].Contains(playerGlyph), "Playership not rendered on expected row.");

            if (location.Coordinate.X != location.Coordinate.Y)
            {
                var wrongRowIndex = 1 + location.Coordinate.X;
                Assert.IsFalse(lines[wrongRowIndex].Contains(playerGlyph), "Playership rendered on X row instead of Y row.");
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
            var playerGlyph = map.Game.Config.GetSetting<string>("PlayerShipGlyph");

            Assert.That(lines.Any(line => line.Contains($"[[;{expectedColor};]{playerGlyph}]")), Is.True);
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

        [Test]
        public void SRS_Render_Masks_NonPlayer_Objects_In_Nebula()
        {
            var setup = new Test_Setup();
            setup.SetupMapWith1FriendlyAtSector(new Point(2, 1));

            var map = setup.TestMap;
            var render = new Render(map.Game.Interact, map.Game.Config);
            var sector = map.Sectors.GetActive();
            var location = map.Playership.GetLocation();
            var deuteriumCell = sector.GetCoordinate(new Point(1, 1));
            var hostileCell = sector.GetCoordinate(new Point(3, 1));
            var sb = new StringBuilder();

            deuteriumCell.Item = CoordinateItem.Deuterium;
            deuteriumCell.Object = new Deuterium(30);
            hostileCell.Item = CoordinateItem.HostileShip;
            hostileCell.Object = new StarTrek_KG.Actors.Ship(FactionName.Klingon, "Probe", hostileCell, map);
            sector.TransformIntoNebulae();

            map.Game.Interact.Output.Clear();

            render.CreateSRSViewScreen(sector, map, location, 1, sector.Name, true, sb);

            var output = string.Join("\n", map.Game.Interact.Output.Queue);
            var maskedDeuterium = StarTrek_KG.Utility.Utility.DamagedScannerUnit(new Point(1, 1));
            var maskedHostile = StarTrek_KG.Utility.Utility.DamagedScannerUnit(new Point(3, 1));
            var playerGlyph = map.Game.Config.GetSetting<string>("PlayerShipGlyph");
            Assert.That(output.Contains(playerGlyph), Is.True);
            Assert.That(output.Contains(" . "), Is.False);
            Assert.That(output.Contains(maskedDeuterium), Is.True);
            Assert.That(output.Contains(maskedHostile), Is.True);
        }

        [Test]
        public void SRS_Render_Does_Not_Append_Scan_Legend()
        {
            var setup = new Test_Setup();
            setup.SetupMapWith1Friendly();

            var map = setup.TestMap;
            var render = new Render(map.Game.Interact, map.Game.Config);
            var sector = map.Sectors.GetActive();
            var location = map.Playership.GetLocation();
            var sb = new StringBuilder();

            map.Game.Interact.Output.Clear();

            render.CreateSRSViewScreen(sector, map, location, 0, sector.Name, false, sb);

            var output = string.Join("\n", map.Game.Interact.Output.Queue);
            Assert.That(output.Contains("Legend:"), Is.False);
        }

        [Test]
        public void CRS_Render_Does_Not_Append_Scan_Legend()
        {
            var setup = new Test_Setup();
            setup.SetupMapWith1Friendly();

            var map = setup.TestMap;
            var render = new Render(map.Game.Interact, map.Game.Config);
            var sector = map.Sectors.GetActive();
            var location = map.Playership.GetLocation();
            var sb = new StringBuilder();

            map.Game.Interact.Output.Clear();

            render.CreateCRSViewScreen(sector, map, location, 0, sector.Name, false, sb);

            var output = string.Join("\n", map.Game.Interact.Output.Queue);
            Assert.That(output.Contains("Legend:"), Is.False);
        }

        [Test]
        public void CRS_Subsystem_Lights_Are_Centered_In_Top_Border()
        {
            var setup = new Test_Setup();
            setup.SetupMapWith1Friendly();

            var map = setup.TestMap;
            var render = new Render(map.Game.Interact, new CrsBarEnabledSettings());
            var method = typeof(Render).GetMethod("BuildCrsTopBorderDisplay", BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (string)method.Invoke(render, new object[] { "+----------------------+", map.Playership });
            var normalized = Regex.Replace(result, @"\[\[;[^\]]*;\][^\]]\]", "*");
            var firstLight = normalized.IndexOf('*', 1);
            var lastLight = normalized.LastIndexOf('*', normalized.Length - 2);
            var leftPad = normalized.Substring(1, firstLight - 1).Count(ch => ch == '-');
            var rightPad = normalized.Substring(lastLight + 1, normalized.Length - (lastLight + 2)).Count(ch => ch == '-');

            Assert.That(leftPad, Is.GreaterThan(0));
            Assert.That(System.Math.Abs(leftPad - rightPad), Is.LessThanOrEqualTo(1));
        }

        private sealed class CrsBarEnabledSettings : IStarTrekKGSettings
        {
            private readonly StarTrekKGSettings _inner = new StarTrekKGSettings();

            public StarTrekKGSettings Get { get => _inner.Get; set => _inner.Get = value; }
            public StarTrek_KG.Config.Collections.Names StarSystems => _inner.StarSystems;
            public StarTrek_KG.Config.Collections.NameValues ConsoleText => _inner.ConsoleText;
            public StarTrek_KG.Config.Collections.Factions Factions => _inner.Factions;
            public StarTrek_KG.Config.Collections.NameValues GameSettings => _inner.GameSettings;
            public StarTrek_KG.Config.Elements.MenusElement Menus => _inner.Menus;
            public List<StarTrek_KG.Commands.CommandDef> LoadCommands() => _inner.LoadCommands();
            public StarTrekKGSettings GetConfig() => _inner.GetConfig();
            public List<string> ShipNames(FactionName faction) => _inner.ShipNames(faction);
            public List<FactionThreat> GetThreats(FactionName faction) => _inner.GetThreats(faction);
            public StarTrek_KG.Config.Collections.MenuItems GetMenuItems(string menuName) => _inner.GetMenuItems(menuName);
            public List<string> GetStarSystems() => _inner.GetStarSystems();
            public string GetText(string name) => _inner.GetText(name);
            public string GetText(string textToGet, string textToGet2) => _inner.GetText(textToGet, textToGet2);

            public T GetSetting<T>(string name)
            {
                if (name == "enable-crs-subsystem-bar")
                {
                    return (T)(object)true;
                }

                return _inner.GetSetting<T>(name);
            }

            public string Setting(string name) => _inner.Setting(name);
            public T CheckAndCastValue<T>(string name, StarTrek_KG.Config.Elements.NameValue element, bool whiteSpaceIsOk = false) => _inner.CheckAndCastValue<T>(name, element, whiteSpaceIsOk);
            public void Reset() => _inner.Reset();
        }
    }
}
