using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
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
    public class GaseousAnomalyTests
    {
        [Test]
        public void GaseousAnomalies_DoNotGenerate_WhenPrevalenceIsZero()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"GaseousAnomalySectorPercent", "0"}
            });

            var game = CreateGame(overrides);
            var total = game.Map.Sectors.Sum(s => s.Coordinates.Count(c => c.Item == CoordinateItem.GaseousAnomaly));

            Assert.AreEqual(0, total);
        }

        [Test]
        public void GaseousAnomalies_Generate_WhenPrevalenceIsHundredPercent()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"GaseousAnomalySectorPercent", "100"},
                {"GaseousAnomalyGroupMin", "6"},
                {"GaseousAnomalyGroupMax", "6"}
            });

            var game = CreateGame(overrides);
            var total = game.Map.Sectors.Sum(s => s.Coordinates.Count(c => c.Item == CoordinateItem.GaseousAnomaly));

            Assert.Greater(total, 0);
        }

        [Test]
        public void Impulse_EnteringGaseousAnomaly_StopsMovementAndConsumesExtraTurn()
        {
            var settings = new StarTrekKGSettings();
            var interaction = new StarTrek_KG.Output.Interaction(settings);
            var game = new Game(settings, false)
            {
                Interact = interaction
            };

            game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                AddStars = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip)
                }
            }, interaction, game.Config, game);

            var movement = new Movement(game.Map.Playership);
            var startTime = game.Map.timeRemaining;
            var startDate = game.Map.Stardate;
            var startX = game.Map.Playership.Coordinate.X;
            var startY = game.Map.Playership.Coordinate.Y;
            var direction = startX < DEFAULTS.COORDINATE_MAX - 1 ? NavDirection.Right : NavDirection.Left;
            var anomalyX = direction == NavDirection.Right ? startX + 1 : startX - 1;
            var anomalyCoordinate = game.Map.Sectors.GetActive().Coordinates[anomalyX, startY];
            anomalyCoordinate.Item = CoordinateItem.GaseousAnomaly;
            anomalyCoordinate.Object = new GaseousAnomaly
            {
                Coordinate = anomalyCoordinate
            };

            movement.Execute(MovementType.Impulse, direction, 5, out _, out _);

            Assert.AreEqual(anomalyX, game.Map.Playership.Coordinate.X);
            Assert.AreEqual(startY, game.Map.Playership.Coordinate.Y);
            Assert.AreEqual(startTime - 1, game.Map.timeRemaining);
            Assert.AreEqual(startDate + 1, game.Map.Stardate);
            Assert.IsTrue(game.Map.Playership.OutputQueue().Any(s => s.Contains("Gaseous anomaly encountered. Impulse movement halted.")));
            Assert.AreEqual(CoordinateItem.PlayerShip, game.Map.Sectors.GetActive().Coordinates[anomalyX, startY].Item);
        }

        [Test]
        public void IRS_Shows_GaseousAnomaly()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"GaseousAnomalySectorPercent", "0"}
            }));

            var sector = game.Map.Playership.GetSector();
            var coordinate = sector.Coordinates[1, 0];
            coordinate.Item = CoordinateItem.GaseousAnomaly;
            coordinate.Object = new GaseousAnomaly();

            var shipLocation = game.Map.Playership.GetLocation();
            var irsData = shipLocation.Sector.GetIRSFullData(shipLocation, game);
            var result = irsData.Single(r => r.Point != null && r.Point.X == 1 && r.Point.Y == 0);

            Assert.AreEqual("Gaseous Anomaly", result.ToScanString());
        }

        private static Game CreateGame(IStarTrekKGSettings config)
        {
            return new Game(config, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip)
                }
            });
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
