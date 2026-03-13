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

namespace UnitTests.GameTests
{
    [TestFixture]
    public class BorgTests
    {
        [Test]
        public void Borg_Generates_Only_In_Delta_Quadrant_Using_Configured_Count()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BorgCubeCount", "2"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"},
                {"HostileOutpostSectorPercent", "0"}
            }));

            var borgShips = game.Map.Sectors.GetHostiles().OfType<Ship>().Where(s => s.Faction == FactionName.Borg).ToList();

            Assert.AreEqual(2, borgShips.Count);
            Assert.That(borgShips.All(ship =>
                string.Equals(QuadrantRules.GetQuadrantName(game.Map, ship.Point.X, ship.Point.Y), "Delta", StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void Borg_Arrives_Two_Turns_After_Player_Changes_Sector()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BorgCubeCount", "0"},
                {"BorgPursuitDelayTurns", "2"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"}
            }));

            var originSector = game.Map.Sectors[0, 0];
            AddManualBorg(game, originSector, 7, 7);
            game.SubscriberSendAndGetResponse("irs");

            var destinationSector = game.Map.Sectors[1, 0];
            var destinationCoordinate = destinationSector.Coordinates[0, 0];
            game.Map.SetPlayershipInLocation(game.Map.Playership, game.Map, new Location(destinationSector, destinationCoordinate));

            game.SubscriberSendAndGetResponse("irs");
            Assert.False(destinationSector.GetHostiles().Any(h => h.Faction == FactionName.Borg));

            game.SubscriberSendAndGetResponse("irs");
            Assert.IsTrue(destinationSector.GetHostiles().Any(h => h.Faction == FactionName.Borg));
        }

        [Test]
        public void Borg_Within_Range_Immobilizes_And_Drains_Percent_Each_Turn()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BorgCubeCount", "0"},
                {"BorgAttackRange", "2"},
                {"BorgPowerDrainPercent", "35"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            AddManualBorg(game, sector, 2, 0);

            var startingEnergy = game.Map.Playership.Energy;
            game.SubscriberSendAndGetResponse("irs");

            Assert.AreEqual(startingEnergy - (int)Math.Ceiling(startingEnergy * 0.35), game.Map.Playership.Energy);

            var movement = new Movement(game.Map.Playership);
            movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.AreEqual(0, game.Map.Playership.Coordinate.X);
            Assert.AreEqual(0, game.Map.Playership.Coordinate.Y);
        }

        [Test]
        public void Borg_Can_Be_Lured_Into_Black_Hole()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BorgCubeCount", "0"},
                {"BorgBlackHoleLurePercent", "100"},
                {"BorgAttackRange", "2"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            AddManualBorg(game, sector, 2, 0);
            sector.Coordinates[1, 0].Item = CoordinateItem.BlackHole;
            sector.Coordinates[1, 0].Object = new StarTrek_KG.Playfield.BlackHole
            {
                Coordinate = sector.Coordinates[1, 0]
            };

            game.SubscriberSendAndGetResponse("irs");

            Assert.IsFalse(sector.GetHostiles().Any(h => h.Faction == FactionName.Borg));
        }

        [Test]
        public void Torpedo_Hit_Repels_Borg_For_Two_Turns()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BorgCubeCount", "0"},
                {"BorgAttackRange", "2"},
                {"BorgPowerDrainPercent", "35"},
                {"BorgTorpedoDamage", "2500"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var borg = AddManualBorg(game, sector, 2, 0);

            Assert.IsTrue(game.TryApplyBorgWeaponDamage(borg, 2500, "torpedo"));

            Assert.AreEqual(2, borg.BorgRepelledTurnsRemaining);
            Assert.IsFalse(game.IsPlayerImmobilizedByBorg(game.Map.Playership));

            var startingEnergy = game.Map.Playership.Energy;
            game.SubscriberSendAndGetResponse("irs");
            Assert.AreEqual(startingEnergy, game.Map.Playership.Energy);
            Assert.IsFalse(game.IsPlayerImmobilizedByBorg(game.Map.Playership));

            game.SubscriberSendAndGetResponse("irs");
            Assert.IsFalse(game.IsPlayerImmobilizedByBorg(game.Map.Playership));

            game.SubscriberSendAndGetResponse("irs");
            Assert.IsTrue(game.IsPlayerImmobilizedByBorg(game.Map.Playership));
        }

        [Test]
        public void FigureEight_ZipBug_Suppresses_Borg_Lockdown()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BorgCubeCount", "0"},
                {"BorgAttackRange", "2"},
                {"BorgPowerDrainPercent", "35"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            AddManualBorg(game, sector, 2, 0);
            var zipCoordinate = sector.Coordinates[3, 0];
            zipCoordinate.Item = CoordinateItem.ZipBug;
            zipCoordinate.Object = new ZipBug
            {
                Coordinate = zipCoordinate,
                Form = ZipBug.ZipBugForm.FigureEight,
                Name = "Observer"
            };

            var startingEnergy = game.Map.Playership.Energy;
            game.SubscriberSendAndGetResponse("irs");

            Assert.AreEqual(startingEnergy, game.Map.Playership.Energy);
            Assert.IsFalse(game.IsPlayerImmobilizedByBorg(game.Map.Playership));
        }

        [Test]
        public void Default_Startup_Places_Player_In_Alpha_Quadrant()
        {
            var game = new Game(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BorgCubeCount", "0"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"},
                {"HostileOutpostSectorPercent", "0"}
            }), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false
            });

            Assert.AreEqual("Alpha", QuadrantRules.GetQuadrantName(game.Map, game.Map.Playership.Point.X, game.Map.Playership.Point.Y));
        }

        private static Ship AddManualBorg(Game game, Sector sector, int x, int y)
        {
            var coordinate = sector.Coordinates[x, y];
            var borg = new Ship(FactionName.Borg, "Borg Cube", coordinate, game.Map)
            {
                Energy = 10000,
                MaxEnergy = 10000
            };

            Shields.For(borg).Energy = 10000;
            Torpedoes.For(borg).Count = 0;
            Torpedoes.For(borg).Damage = 1;
            Phasers.For(borg).Damage = 1;
            var disruptors = borg.Subsystems.Single(s => s.Type == SubsystemType.Disruptors);
            disruptors.Damage = 1;

            sector.AddShip(borg, coordinate);
            return borg;
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
