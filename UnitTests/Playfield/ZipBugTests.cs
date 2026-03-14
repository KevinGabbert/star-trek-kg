using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Commands;
using StarTrek_KG.Config;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Playfield
{
    [TestFixture]
    public class ZipBugTests
    {
        [Test]
        public void ZipBugs_Generate_Using_Configured_Count()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ZipBugCount", "2"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"}
            }));

            var zipBugs = game.Map.Sectors
                .SelectMany(s => s.Coordinates)
                .Count(c => c.Item == CoordinateItem.ZipBug);

            Assert.AreEqual(2, zipBugs);
        }

        [Test]
        public void ZipBug_Appearance_Increases_Player_MaxEnergy_On_Spawn_And_Relocation()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ZipBugCount", "1"},
                {"ZipBugMaxEnergyAppearanceBonus", "100"},
                {"BlackHoleSectorPercent", "0"},
                {"TemporalRiftSectorPercent", "0"},
                {"WormholeSectorPercent", "0"}
            }));

            var player = (Ship)game.Map.Playership;
            var baselineMaxEnergy = game.Config.GetSetting<int>("energy");
            Assert.AreEqual(baselineMaxEnergy + 100, player.MaxEnergy);

            var zipBugCoordinate = game.Map.Sectors.SelectMany(s => s.Coordinates).Single(c => c.Item == CoordinateItem.ZipBug);
            Assert.IsTrue(game.HandleZipBugShot(zipBugCoordinate, "test"));

            Assert.AreEqual(baselineMaxEnergy + 200, player.MaxEnergy);
        }

        [Test]
        public void ZipBug_Moving_Adjacent_And_FigureEight_Form_Grant_Energy()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ZipBugCount", "0"},
                {"ZipBugAdjacentEnergyBonus", "1000"},
                {"ZipBugFigureEightEnergyPerTurn", "200"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var coordinate = sector.Coordinates[2, 0];
            coordinate.Item = CoordinateItem.ZipBug;
            coordinate.Object = new ZipBug
            {
                Coordinate = coordinate,
                Form = ZipBug.ZipBugForm.FigureEight,
                Name = "Renewal"
            };

            var startingEnergy = game.Map.Playership.Energy;
            game.SubscriberSendAndGetResponse("irs");

            var movedZipBug = sector.Coordinates.Single(c => c.Item == CoordinateItem.ZipBug).Object as ZipBug;
            Assert.AreEqual(startingEnergy + 1200, game.Map.Playership.Energy);
            Assert.That(movedZipBug, Is.Not.Null);
            Assert.LessOrEqual(Math.Abs(movedZipBug.Coordinate.X - game.Map.Playership.Coordinate.X), 1);
            Assert.LessOrEqual(Math.Abs(movedZipBug.Coordinate.Y - game.Map.Playership.Coordinate.Y), 1);
        }

        [Test]
        public void Adjacent_ZipBug_Protects_Player_And_Depletes_Hostile_Weapons()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ZipBugCount", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var zipBugCoordinate = sector.Coordinates[1, 0];
            zipBugCoordinate.Item = CoordinateItem.ZipBug;
            zipBugCoordinate.Object = new ZipBug
            {
                Coordinate = zipBugCoordinate,
                Name = "Harold"
            };

            var hostileCoordinate = sector.Coordinates[2, 2];
            var hostile = new Ship(FactionName.Klingon, "IKC Test", hostileCoordinate, game.Map)
            {
                Energy = 300,
                MaxEnergy = 300
            };
            sector.AddShip(hostile, hostileCoordinate);
            Torpedoes.For(hostile).Count = 4;

            var startingEnergy = game.Map.Playership.Energy;
            game.ALLHostilesAttack(game.Map);

            Assert.AreEqual(startingEnergy, game.Map.Playership.Energy);
            Assert.AreEqual(0, Torpedoes.For(hostile).Count);
            Assert.Greater(Phasers.For(hostile).Damage, 0);
            Assert.Greater(hostile.Subsystems.Single(s => s.Type == SubsystemType.Disruptors).Damage, 0);
        }

        [Test]
        public void Shot_ZipBug_Relocates_Instead_Of_Being_Destroyed()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ZipBugCount", "0"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var zipBugCoordinate = sector.Coordinates[1, 0];
            zipBugCoordinate.Item = CoordinateItem.ZipBug;
            zipBugCoordinate.Object = new ZipBug
            {
                Coordinate = zipBugCoordinate,
                Name = "Deluge"
            };

            Assert.IsTrue(game.HandleZipBugShot(zipBugCoordinate, "test"));
            Assert.AreNotEqual(CoordinateItem.ZipBug, sector.Coordinates[1, 0].Item);
            Assert.AreEqual(1, game.Map.Sectors.SelectMany(s => s.Coordinates).Count(c => c.Item == CoordinateItem.ZipBug));
        }

        [Test]
        public void ZipBug_Presence_Reveals_Hidden_Items_With_Configured_Glyph()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ZipBugCount", "0"},
                {"ZipBugRevealGlyph", "#"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var zipBugCoordinate = sector.Coordinates[7, 7];
            zipBugCoordinate.Item = CoordinateItem.ZipBug;
            zipBugCoordinate.Object = new ZipBug
            {
                Coordinate = zipBugCoordinate,
                Name = "Harold"
            };

            sector.Coordinates[1, 0].Item = CoordinateItem.GraviticMine;
            sector.Coordinates[1, 0].Object = new GraviticMine();
            sector.Coordinates[2, 0].Item = CoordinateItem.SporeField;
            sector.Coordinates[2, 0].Object = new SporeField();

            var output = game.SubscriberSendAndGetResponse("srs");

            Assert.IsTrue(output.Any(line => line.Contains("#")));
        }

        [Test]
        public void HostileForm_ZipBug_Renders_Full_Configured_Glyph_In_Srs()
        {
            var game = CreateGame(new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"ZipBugCount", "0"},
                {"ZipBugHostileGlyph", "+?+"}
            }));

            var sector = game.Map.Sectors.GetActive();
            var zipBugCoordinate = sector.Coordinates[1, 0];
            zipBugCoordinate.Item = CoordinateItem.ZipBug;
            zipBugCoordinate.Object = new ZipBug
            {
                Coordinate = zipBugCoordinate,
                Form = ZipBug.ZipBugForm.HostileMimic,
                Name = "Regeneration"
            };

            var output = game.SubscriberSendAndGetResponse("srs");

            Assert.IsTrue(output.Any(line => line.Contains("+?+")));
        }

        [Test]
        public void ZipBug_AliasPool_Includes_RandomLetter_Observer_Names()
        {
            var aliasesField = typeof(ZipBug).GetField("Aliases", BindingFlags.NonPublic | BindingFlags.Static);
            var aliases = ((string[])aliasesField.GetValue(null)).ToList();

            Assert.That(aliases, Does.Contain("ZZZZFTT"));
            Assert.That(aliases, Does.Contain("FFFFIPBG"));
            Assert.That(aliases, Does.Contain("FFFFHBBTT"));
            Assert.That(aliases, Does.Contain("FFTTHXLT"));
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
