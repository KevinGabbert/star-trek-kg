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
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Playfield
{
    [TestFixture]
    public class WormholeTests
    {
        [Test]
        public void Wormholes_Do_Not_Generate_When_Prevalence_Is_Zero()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "0"}
            }));

            var wormholes = game.Map.Sectors
                .SelectMany(s => s.Coordinates)
                .Count(c => c.Item == CoordinateItem.Wormhole);

            Assert.AreEqual(0, wormholes);
        }

        [Test]
        public void Wormholes_Generate_When_Prevalence_Is_HundredPercent()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "100"},
                {"GaseousAnomalySectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"},
                {"HostileOutpostSectorPercent", "0"}
            }));

            var wormholes = game.Map.Sectors
                .SelectMany(s => s.Coordinates)
                .Where(c => c.Item == CoordinateItem.Wormhole)
                .ToList();

            Assert.IsNotEmpty(wormholes);
            Assert.AreEqual(0, wormholes.Count % 2);
            Assert.That(wormholes.All(c => c.Object is Wormhole w && w.DestinationSector != null));
        }

        [Test]
        public void Generated_Wormholes_Are_Paired_Bidirectionally_By_PairId()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "100"},
                {"GaseousAnomalySectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"},
                {"HostileOutpostSectorPercent", "0"}
            }));

            var wormholes = game.Map.Sectors
                .SelectMany(s => s.Coordinates)
                .Where(c => c.Item == CoordinateItem.Wormhole)
                .Select(c => new
                {
                    Coordinate = c,
                    Wormhole = (Wormhole)c.Object
                })
                .ToList();

            var pairGroups = wormholes.GroupBy(w => w.Wormhole.PairId).ToList();

            Assert.IsNotEmpty(pairGroups);
            Assert.That(pairGroups.All(g => g.Count() == 2));
            Assert.That(pairGroups.All(g =>
            {
                var pair = g.ToList();
                var first = pair[0];
                var second = pair[1];
                return first.Wormhole.DestinationSector.X == second.Coordinate.SectorDef.X &&
                       first.Wormhole.DestinationSector.Y == second.Coordinate.SectorDef.Y &&
                       second.Wormhole.DestinationSector.X == first.Coordinate.SectorDef.X &&
                       second.Wormhole.DestinationSector.Y == first.Coordinate.SectorDef.Y;
            }));
        }

        [Test]
        public void IRS_Shows_Wormhole()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var coordinate = sector.Coordinates[1, 0];
            coordinate.Item = CoordinateItem.Wormhole;
            coordinate.Object = new Wormhole
            {
                Coordinate = coordinate,
                DestinationSector = new Point(1, 0),
                PairId = 1
            };

            var shipLocation = game.Map.Playership.GetLocation();
            var irsData = shipLocation.Sector.GetIRSFullData(shipLocation, game);
            var result = irsData.Single(r => r.Point != null && r.Point.X == 1 && r.Point.Y == 0);

            Assert.AreEqual("Wormhole", result.ToScanString());
        }

        [Test]
        public void Impulse_Entering_Wormhole_Teleports_And_Consumes_Extra_Turn()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "0"}
            }));

            var originSector = game.Map.Sectors[0, 0];
            var destinationSector = game.Map.Sectors[1, 0];
            var wormholeCoordinate = originSector.Coordinates[1, 0];
            wormholeCoordinate.Item = CoordinateItem.Wormhole;
            wormholeCoordinate.Object = new Wormhole
            {
                Coordinate = wormholeCoordinate,
                DestinationSector = destinationSector.GetPoint(),
                PairId = 7
            };

            var exitCoordinate = destinationSector.Coordinates[7, 7];
            RestrictSectorToSingleEmptyCoordinate(destinationSector, exitCoordinate);

            var startTime = game.Map.timeRemaining;
            var startDate = game.Map.Stardate;

            var movement = new Movement(game.Map.Playership);
            movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.AreEqual(destinationSector.X, game.Map.Playership.Point.X);
            Assert.AreEqual(destinationSector.Y, game.Map.Playership.Point.Y);
            Assert.AreEqual(exitCoordinate.X, game.Map.Playership.Coordinate.X);
            Assert.AreEqual(exitCoordinate.Y, game.Map.Playership.Coordinate.Y);
            Assert.AreEqual(startTime - 1, game.Map.timeRemaining);
            Assert.AreEqual(startDate + 1, game.Map.Stardate);
            Assert.AreEqual(CoordinateItem.Wormhole, originSector.Coordinates[1, 0].Item);
            Assert.IsTrue(game.Map.Playership.OutputQueue().Any(line => line.Contains("Wormhole engaged")));
        }

        [Test]
        public void Wormhole_Transit_Is_Bidirectional_When_Paired()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "0"}
            }));

            var originSector = game.Map.Sectors[0, 0];
            var destinationSector = game.Map.Sectors[1, 0];

            var originWormhole = originSector.Coordinates[1, 0];
            originWormhole.Item = CoordinateItem.Wormhole;
            originWormhole.Object = new Wormhole
            {
                Coordinate = originWormhole,
                DestinationSector = destinationSector.GetPoint(),
                PairId = 3
            };

            var destinationWormhole = destinationSector.Coordinates[0, 0];
            destinationWormhole.Item = CoordinateItem.Wormhole;
            destinationWormhole.Object = new Wormhole
            {
                Coordinate = destinationWormhole,
                DestinationSector = originSector.GetPoint(),
                PairId = 3
            };

            var destinationExit = destinationSector.Coordinates[7, 7];
            RestrictSectorToSpecificWalkableCoordinates(destinationSector, destinationWormhole, destinationExit);

            var originExit = originSector.Coordinates[7, 6];
            RestrictSectorToSpecificWalkableCoordinates(originSector, originWormhole, originExit, originSector.Coordinates[0, 0]);

            var movement = new Movement(game.Map.Playership);
            movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.AreEqual(destinationExit.X, game.Map.Playership.Coordinate.X);
            Assert.AreEqual(destinationExit.Y, game.Map.Playership.Coordinate.Y);

            var timeBeforeReturn = game.Map.timeRemaining;
            var dateBeforeReturn = game.Map.Stardate;
            game.Map.SetPlayershipInLocation(game.Map.Playership, game.Map, new Location(destinationSector, destinationWormhole));

            Assert.AreEqual(originSector.X, game.Map.Playership.Point.X);
            Assert.AreEqual(originSector.Y, game.Map.Playership.Point.Y);
            Assert.AreEqual(originExit.X, game.Map.Playership.Coordinate.X);
            Assert.AreEqual(originExit.Y, game.Map.Playership.Coordinate.Y);
            Assert.AreEqual(timeBeforeReturn - 1, game.Map.timeRemaining);
            Assert.AreEqual(dateBeforeReturn + 1, game.Map.Stardate);
        }

        [Test]
        public void Direct_Placement_Into_Wormhole_Consumes_Extra_Turn_And_Teleports()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "0"}
            }));

            var originSector = game.Map.Sectors[0, 0];
            var destinationSector = game.Map.Sectors[1, 0];
            var originWormhole = originSector.Coordinates[1, 0];
            originWormhole.Item = CoordinateItem.Wormhole;
            originWormhole.Object = new Wormhole
            {
                Coordinate = originWormhole,
                DestinationSector = destinationSector.GetPoint(),
                PairId = 9
            };

            var exitCoordinate = destinationSector.Coordinates[7, 7];
            RestrictSectorToSingleEmptyCoordinate(destinationSector, exitCoordinate);

            var startTime = game.Map.timeRemaining;
            var startDate = game.Map.Stardate;

            game.Map.SetPlayershipInLocation(game.Map.Playership, game.Map, new Location(originSector, originWormhole));

            Assert.AreEqual(destinationSector.X, game.Map.Playership.Point.X);
            Assert.AreEqual(destinationSector.Y, game.Map.Playership.Point.Y);
            Assert.AreEqual(exitCoordinate.X, game.Map.Playership.Coordinate.X);
            Assert.AreEqual(exitCoordinate.Y, game.Map.Playership.Coordinate.Y);
            Assert.AreEqual(startTime - 1, game.Map.timeRemaining);
            Assert.AreEqual(startDate + 1, game.Map.Stardate);
        }

        [Test]
        public void Wormhole_Transit_Fails_Gracefully_When_Destination_Sector_Has_No_Empty_Coordinate()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "0"}
            }));

            var originSector = game.Map.Sectors[0, 0];
            var destinationSector = game.Map.Sectors[1, 0];
            var wormholeCoordinate = originSector.Coordinates[1, 0];
            wormholeCoordinate.Item = CoordinateItem.Wormhole;
            wormholeCoordinate.Object = new Wormhole
            {
                Coordinate = wormholeCoordinate,
                DestinationSector = destinationSector.GetPoint(),
                PairId = 13
            };

            foreach (var coordinate in destinationSector.Coordinates)
            {
                coordinate.Item = CoordinateItem.Star;
                coordinate.Object = null;
            }

            var startTime = game.Map.timeRemaining;
            var startDate = game.Map.Stardate;

            var movement = new Movement(game.Map.Playership);
            movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.AreEqual(originSector.X, game.Map.Playership.Point.X);
            Assert.AreEqual(originSector.Y, game.Map.Playership.Point.Y);
            Assert.AreEqual(wormholeCoordinate.X, game.Map.Playership.Coordinate.X);
            Assert.AreEqual(wormholeCoordinate.Y, game.Map.Playership.Coordinate.Y);
            Assert.AreEqual(startTime, game.Map.timeRemaining);
            Assert.AreEqual(startDate, game.Map.Stardate);
            Assert.AreEqual(CoordinateItem.PlayerShip, wormholeCoordinate.Item);
            Assert.IsTrue(game.Map.Playership.OutputQueue().Any(line => line.Contains("Wormhole destabilized")));
        }

        [Test]
        public void LRS_Renders_Wormhole_Marker()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "0"},
                {"WormholeChar", "W"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var coordinate = sector.Coordinates[1, 0];
            coordinate.Item = CoordinateItem.Wormhole;
            coordinate.Object = new Wormhole
            {
                Coordinate = coordinate,
                DestinationSector = new Point(1, 0),
                PairId = 11
            };

            var output = LongRangeScan.For(game.Map.Playership).Controls();

            Assert.IsTrue(output.Any(line => line.Contains("W")));
        }

        [Test]
        public void Short_Range_Scan_Uses_Configured_Wormhole_Glyph()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"WormholeSectorPercent", "0"},
                {"WormholeChar", "W"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var coordinate = sector.Coordinates[1, 0];
            coordinate.Item = CoordinateItem.Wormhole;
            coordinate.Object = new Wormhole
            {
                Coordinate = coordinate,
                DestinationSector = new Point(1, 0),
                PairId = 15
            };

            var output = game.SubscriberSendAndGetResponse("srs");

            Assert.IsTrue(output.Any(line => line.Contains("W")));
        }

        private static void RestrictSectorToSingleEmptyCoordinate(Sector sector, Coordinate emptyCoordinate)
        {
            foreach (var coordinate in sector.Coordinates.Where(c => !(c.X == emptyCoordinate.X && c.Y == emptyCoordinate.Y)))
            {
                coordinate.Item = CoordinateItem.Star;
                coordinate.Object = null;
            }
        }

        private static void RestrictSectorToSpecificWalkableCoordinates(Sector sector, params Coordinate[] walkableCoordinates)
        {
            foreach (var coordinate in sector.Coordinates.Where(c => walkableCoordinates.All(w => w.X != c.X || w.Y != c.Y)))
            {
                coordinate.Item = CoordinateItem.Star;
                coordinate.Object = null;
            }
        }

        private static Game CreateGame(IStarTrekKGSettings settings)
        {
            return new Game(settings, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip)
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
