using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Commands;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Playfield
{
    [TestFixture]
    public class AdvancedHazardsTests
    {
        [Test]
        public void TemporalRift_Generates_WhenPrevalenceIsHundredPercent()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"TemporalRiftSectorPercent", "100"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"},
                {"GaseousAnomalySectorPercent", "0"},
                {"DeuteriumSectorSpawnPercent", "0"},
                {"DeuteriumCloudSectorPercent", "0"}
            }));

            var total = game.Map.Sectors.Sum(s => s.Coordinates.Count(c => c.Item == CoordinateItem.TemporalRift));
            Assert.Greater(total, 0);
        }

        [Test]
        public void SporeField_Generates_WhenPrevalenceIsHundredPercent()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "100"},
                {"BlackHoleSectorPercent", "0"},
                {"GaseousAnomalySectorPercent", "0"},
                {"DeuteriumSectorSpawnPercent", "0"},
                {"DeuteriumCloudSectorPercent", "0"}
            }));

            var total = game.Map.Sectors.Sum(s => s.Coordinates.Count(c => c.Item == CoordinateItem.SporeField));
            Assert.Greater(total, 0);
        }

        [Test]
        public void BlackHole_Generates_WhenPrevalenceIsHundredPercent_AndCappedToOnePerSector()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "100"},
                {"GaseousAnomalySectorPercent", "0"},
                {"DeuteriumSectorSpawnPercent", "0"},
                {"DeuteriumCloudSectorPercent", "0"}
            }));

            var sectorsWithBlackHoles = game.Map.Sectors
                .Where(s => s.Coordinates.Any(c => c.Item == CoordinateItem.BlackHole))
                .ToList();

            Assert.IsNotEmpty(sectorsWithBlackHoles);
            Assert.That(sectorsWithBlackHoles.All(s => s.Coordinates.Count(c => c.Item == CoordinateItem.BlackHole) <= 1));
        }

        [Test]
        public void CRS_Sector_Indicator_Shows_BlackHole_Marker_After_Sector_Visited()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"},
                {"GaseousAnomalySectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            sector.Coordinates[1, 0].Item = CoordinateItem.BlackHole;
            sector.Coordinates[1, 0].Object = new StarTrek_KG.Playfield.BlackHole();
            sector.Scanned = true;

            var x = game.Map.Playership.Point.X;
            var y = game.Map.Playership.Point.Y;
            var expected = $"Sec: §{x}.{y}°";

            game.SubscriberSendAndGetResponse("crs");

            Assert.IsTrue(game.Map.Playership.OutputQueue().Any(line =>
                line.Contains($"\u00A7{x}.{y}") && line.Contains("\u00B0")));
        }

        [Test]
        public void IRS_Identifies_NewHazards()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"},
                {"GaseousAnomalySectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            sector.Coordinates[1, 0].Item = CoordinateItem.TemporalRift;
            sector.Coordinates[1, 0].Object = new TemporalRift();
            sector.Coordinates[1, 1].Item = CoordinateItem.SporeField;
            sector.Coordinates[1, 1].Object = new SporeField();
            sector.Coordinates[0, 1].Item = CoordinateItem.BlackHole;
            sector.Coordinates[0, 1].Object = new StarTrek_KG.Playfield.BlackHole();

            var irsData = game.Map.Playership.GetLocation().Sector.GetIRSFullData(game.Map.Playership.GetLocation(), game).ToList();
            Assert.AreEqual("Temporal Rift", irsData.Single(r => r.Point != null && r.Point.X == 1 && r.Point.Y == 0).ToScanString());
            Assert.AreEqual("Spore Field", irsData.Single(r => r.Point != null && r.Point.X == 1 && r.Point.Y == 1).ToScanString());
            Assert.AreEqual("Black Hole", irsData.Single(r => r.Point != null && r.Point.X == 0 && r.Point.Y == 1).ToScanString());
        }

        [Test]
        public void IRS_Hostile_Intel_Is_Range_Bounded_For_Name_Energy_And_Shields()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"IRSHostileEnergyVisibleDistance", "2"},
                {"IRSHostileShieldsVisibleDistance", "4"},
                {"IRSHostileNameVisibleDistance", "6"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var hostileCoordinate = sector.Coordinates[1, 0];
            var hostile = new Ship(FactionName.Klingon, "IKC Test", hostileCoordinate, game.Map);
            hostile.Energy = 300;
            StarTrek_KG.Subsystem.Shields.For(hostile).Energy = 200;
            sector.AddShip(hostile, hostileCoordinate);
            var playerLocation = game.Map.Playership.GetLocation();

            var near = new StarTrek_KG.Types.IRSResult
            {
                Item = CoordinateItem.HostileShip,
                Object = hostile,
                Point = new Point(1, 0)
            };
            sector.ApplyHostileScanResolution(near, playerLocation, game);
            Assert.AreEqual(hostile.Name, near.ToScanString());
            Assert.AreEqual($"(E:{hostile.Energy}/S:{StarTrek_KG.Subsystem.Shields.For(hostile).Energy})", near.DetailLine);

            var mid = new StarTrek_KG.Types.IRSResult
            {
                Item = CoordinateItem.HostileShip,
                Object = hostile,
                Point = new Point(3, 0)
            };
            sector.ApplyHostileScanResolution(mid, playerLocation, game);
            Assert.AreEqual(hostile.Name, mid.ToScanString());
            Assert.AreEqual($"(E:?/S:{StarTrek_KG.Subsystem.Shields.For(hostile).Energy})", mid.DetailLine);

            var far = new StarTrek_KG.Types.IRSResult
            {
                Item = CoordinateItem.HostileShip,
                Object = hostile,
                Point = new Point(5, 0)
            };
            sector.ApplyHostileScanResolution(far, playerLocation, game);
            Assert.AreEqual(hostile.Name, far.ToScanString());
            Assert.AreEqual("(E:?/S:?)", far.DetailLine);

            var tooFar = new StarTrek_KG.Types.IRSResult
            {
                Item = CoordinateItem.HostileShip,
                Object = hostile,
                Point = new Point(7, 0)
            };
            sector.ApplyHostileScanResolution(tooFar, playerLocation, game);
            Assert.AreEqual("?", tooFar.ToScanString());
            Assert.AreEqual("(E:?/S:?)", tooFar.DetailLine);
        }

        [Test]
        public void IRSPlus_Renders_Hostile_Detail_Line()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"IRSHostileEnergyVisibleDistance", "10"},
                {"IRSHostileShieldsVisibleDistance", "10"},
                {"IRSHostileNameVisibleDistance", "10"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var hostileCoordinate = sector.Coordinates[1, 0];
            var hostile = new Ship(FactionName.Klingon, "IKC Detail", hostileCoordinate, game.Map);
            hostile.Energy = 333;
            StarTrek_KG.Subsystem.Shields.For(hostile).Energy = 222;
            sector.AddShip(hostile, hostileCoordinate);

            var output = game.SubscriberSendAndGetResponse("irs++");
            Assert.IsTrue(output.Any(line => line.Contains("(E:333/S:222)")));
        }

        [Test]
        public void TemporalRift_Collision_Rewinds_To_Prior_Turn_Position()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftRewindTurns", "1"}
            }));

            var ship = (Ship)game.Map.Playership;
            ship.TurnHistory = new List<LocationDef>
            {
                new LocationDef(new Point(0, 0), new Point(0, 2)),
                new LocationDef(new Point(0, 0), new Point(0, 0))
            };

            var sector = game.Map.Sectors.GetActive();
            sector.Coordinates[1, 0].Item = CoordinateItem.TemporalRift;
            sector.Coordinates[1, 0].Object = new TemporalRift
            {
                Coordinate = sector.Coordinates[1, 0]
            };

            var movement = new Movement(game.Map.Playership);
            movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.AreEqual(0, ship.Coordinate.X);
            Assert.AreEqual(2, ship.Coordinate.Y);
        }

        [Test]
        public void Spore_Contamination_Drains_Energy_And_Can_Cure_On_Roll()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"SporeDrainPerTurn", "30"},
                {"SporeCureRollSides", "1"},
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"}
            }));

            var ship = (Ship)game.Map.Playership;
            ship.SporeContaminated = true;
            var before = ship.Energy;

            game.SubscriberSendAndGetResponse("irs");

            Assert.AreEqual(before - 30, ship.Energy);
            Assert.IsFalse(ship.SporeContaminated);
        }

        [Test]
        public void BlackHole_Proximity_Pulls_Ship_Each_Turn()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BlackHolePullRadius", "2"},
                {"BlackHolePullEnergyDrain", "10"},
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            sector.Coordinates[1, 0].Item = CoordinateItem.BlackHole;
            sector.Coordinates[1, 0].Object = new StarTrek_KG.Playfield.BlackHole
            {
                Coordinate = sector.Coordinates[1, 0]
            };

            var startSectorX = game.Map.Playership.Point.X;
            var startEnergy = game.Map.Playership.Energy;
            var startTime = game.Map.timeRemaining;

            game.SubscriberSendAndGetResponse("irs");

            Assert.AreEqual(startSectorX + 1, game.Map.Playership.Point.X);
            Assert.AreEqual(startEnergy - 10, game.Map.Playership.Energy);
            Assert.AreEqual(startTime - 1, game.Map.timeRemaining);
        }

        [Test]
        public void BlackHole_Blocks_Impulse_And_Warp()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BlackHolePullRadius", "2"},
                {"TemporalRiftSectorPercent", "0"},
                {"SporeSectorPercent", "0"},
                {"BlackHoleSectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            sector.Coordinates[1, 0].Item = CoordinateItem.BlackHole;
            sector.Coordinates[1, 0].Object = new StarTrek_KG.Playfield.BlackHole
            {
                Coordinate = sector.Coordinates[1, 0]
            };

            var movement = new Movement(game.Map.Playership);
            movement.Execute(MovementType.Impulse, NavDirection.Right, 3, out _, out _);
            Assert.AreEqual(0, game.Map.Playership.Coordinate.X);
            Assert.AreEqual(0, game.Map.Playership.Coordinate.Y);

            var warp = new WarpActor(game.Interact);
            var success = warp.Engage(NavDirection.Right, 1, out _, out _, game.Map);
            Assert.IsFalse(success);
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
