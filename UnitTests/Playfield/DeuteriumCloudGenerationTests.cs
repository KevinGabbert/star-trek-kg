using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Commands;
using StarTrek_KG.Config;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Playfield
{
    [TestFixture]
    public class DeuteriumCloudGenerationTests
    {
        [Test]
        public void DeuteriumClouds_Create_Guards_And_Mines_In_Sector()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"DeuteriumSectorPercent", "0"},
                {"DeuteriumCloudSectorPercent", "100"},
                {"DeuteriumCloudMinSize", "1"},
                {"DeuteriumCloudMaxSize", "1"},
                {"DeuteriumCloudGuardCount", "2"},
                {"DeuteriumCloudMineChancePercent", "100"}
            });

            var game = new Game(overrides, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = true,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip)
                }
            });

            var sector = game.Map.Playership.GetSector();
            var cloudCount = sector.Coordinates.Count(c => c.Item == CoordinateItem.DeuteriumCloud);
            var guardCount = sector.GetHostiles().Count;
            var mineCount = sector.Coordinates.Count(c => c.Item == CoordinateItem.GraviticMine);

            Assert.GreaterOrEqual(cloudCount, 1);
            Assert.GreaterOrEqual(guardCount, 2);
            Assert.GreaterOrEqual(mineCount, 1);
        }

        [Test]
        public void DeuteriumClouds_Respect_Minimum_Per_Cell_Amount()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"DeuteriumSectorPercent", "0"},
                {"DeuteriumCloudSectorPercent", "100"},
                {"DeuteriumCloudMinSize", "2"},
                {"DeuteriumCloudMaxSize", "2"},
                {"DeuteriumCloudPointMin", "50"}
            });

            var game = new Game(overrides, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = true,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip)
                }
            });

            var cloudCells = game.Map.Sectors
                .SelectMany(s => s.Coordinates)
                .Where(c => c.Item == CoordinateItem.DeuteriumCloud)
                .ToList();

            Assert.IsNotEmpty(cloudCells);
            Assert.That(cloudCells.All(c => ((DeuteriumCloud)c.Object).Amount >= 50));
        }

        [Test]
        public void DeuteriumSectors_Have_Total_Within_Configured_Sector_Budget()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"DeuteriumSectorPercent", "100"},
                {"DeuteriumCloudSectorPercent", "0"},
                {"DeuteriumSectorTotalMin", "200"},
                {"DeuteriumSectorTotalMax", "800"}
            });

            var game = new Game(overrides, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = true,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip)
                }
            });

            var sectorsWithDeuterium = game.Map.Sectors
                .Where(s => s.Coordinates.Any(c => c.Item == CoordinateItem.Deuterium || c.Item == CoordinateItem.DeuteriumCloud))
                .ToList();

            Assert.IsNotEmpty(sectorsWithDeuterium);

            foreach (var sector in sectorsWithDeuterium)
            {
                var total = sector.Coordinates
                    .Where(c => c.Item == CoordinateItem.Deuterium || c.Item == CoordinateItem.DeuteriumCloud)
                    .Sum(c =>
                    {
                        if (c.Item == CoordinateItem.Deuterium)
                        {
                            return (c.Object as Deuterium)?.Amount ?? 0;
                        }

                        return (c.Object as DeuteriumCloud)?.Amount ?? 0;
                    });

                Assert.GreaterOrEqual(total, 200, $"Sector [{sector.X},{sector.Y}] deuterium total below minimum.");
                Assert.LessOrEqual(total, 800, $"Sector [{sector.X},{sector.Y}] deuterium total above maximum.");
            }
        }

        [Test]
        public void DeuteriumSectorSpawnPercent_Config_Controls_Deuterium_Frequency()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"DeuteriumSectorSpawnPercent", "100"},
                {"DeuteriumSectorPercent", "0"},
                {"DeuteriumCloudSectorPercent", "0"},
                {"DeuteriumSectorTotalMin", "200"},
                {"DeuteriumSectorTotalMax", "200"}
            });

            var game = new Game(overrides, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = true,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip)
                }
            });

            var sectorsWithDeuterium = game.Map.Sectors.Count(s => s.Coordinates.Any(c => c.Item == CoordinateItem.Deuterium));
            Assert.Greater(sectorsWithDeuterium, 0);
        }

        [Test]
        public void Mines_Are_Not_Added_To_Starbase_Sectors_When_Config_Disables_It()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"AllowMinesInStarbaseSectors", "false"}
            });

            var game = new Game(overrides, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = true,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,0)), CoordinateItem.HostileShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(2,0)), CoordinateItem.Starbase)
                }
            });

            var sector = game.Map.Sectors[new Point(0, 0)];
            var mineCount = sector.Coordinates.Count(c => c.Item == CoordinateItem.GraviticMine);
            Assert.AreEqual(0, mineCount);
        }

        [Test]
        public void Mines_Can_Be_Added_To_Starbase_Sectors_When_Config_Enables_It()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"AllowMinesInStarbaseSectors", "true"}
            });

            var game = new Game(overrides, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = true,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,0)), CoordinateItem.HostileShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(2,0)), CoordinateItem.Starbase)
                }
            });

            var sector = game.Map.Sectors[new Point(0, 0)];
            var mineCount = sector.Coordinates.Count(c => c.Item == CoordinateItem.GraviticMine);
            Assert.GreaterOrEqual(mineCount, 1);
        }

        private sealed class ConfigOverrideSettings : IStarTrekKGSettings
        {
            private readonly StarTrekKGSettings _inner = new StarTrekKGSettings();
            private readonly IDictionary<string, string> _overrideValues;

            public ConfigOverrideSettings(IDictionary<string, string> overrideValues)
            {
                _overrideValues = overrideValues ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            public StarTrekKGSettings Get { get => _inner.Get; set => _inner.Get = value; }
            public Names StarSystems => _inner.StarSystems;
            public NameValues ConsoleText => _inner.ConsoleText;
            public Factions Factions => _inner.Factions;
            public NameValues GameSettings => _inner.GameSettings;
            public MenusElement Menus => _inner.Menus;
            public List<CommandDef> LoadCommands() => _inner.LoadCommands();
            public StarTrekKGSettings GetConfig() => _inner.GetConfig();
            public List<string> ShipNames(FactionName faction) => _inner.ShipNames(faction);
            public List<FactionThreat> GetThreats(FactionName faction) => _inner.GetThreats(faction);
            public MenuItems GetMenuItems(string menuName) => _inner.GetMenuItems(menuName);
            public List<string> GetStarSystems() => _inner.GetStarSystems();
            public string GetText(string name) => _inner.GetText(name);
            public string GetText(string textToGet, string textToGet2) => _inner.GetText(textToGet, textToGet2);
            public T GetSetting<T>(string name)
            {
                if (_overrideValues.TryGetValue(name, out var value))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return _inner.GetSetting<T>(name);
            }
            public string Setting(string name) => this.GetSetting<string>(name);
            public T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false) => _inner.CheckAndCastValue<T>(name, element, whiteSpaceIsOk);
            public void Reset() => _inner.Reset();
        }
    }
}
