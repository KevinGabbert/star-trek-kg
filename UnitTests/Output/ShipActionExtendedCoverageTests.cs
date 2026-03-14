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
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Actors;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class ShipActionExtendedCoverageTests
    {
        private Game _game;
        private Interaction _interact;

        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();

            var config = new StarTrekKGSettings();
            _game = new Game(config, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,1)), CoordinateItem.HostileShip)
                }
            });

            _interact = (Interaction)_game.Interact;
            _interact.Output.Clear();
        }

        [Test]
        public void NaturalLanguage_RaiseShields_Works()
        {
            var before = Shields.For(_game.Map.Playership).Energy;

            var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "raise shields");

            Assert.IsNotNull(output);
            Assert.GreaterOrEqual(Shields.For(_game.Map.Playership).Energy, before);
        }

        [Test]
        public void NaturalLanguage_AddShieldEnergy_Works()
        {
            var before = Shields.For(_game.Map.Playership).Energy;
            var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "add 100 to shields");

            Assert.IsNotNull(output);
            Assert.GreaterOrEqual(Shields.For(_game.Map.Playership).Energy, before);
        }

        [Test]
        public void NaturalLanguage_WarpAndMove_Commands_DoNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "warp 1 course 7");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "move right 1");
            });
        }

        [Test]
        public void NaturalLanguage_ScanAndWeapons_Commands_DoNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "long range scan");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "fire phasers 100");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "fire torpedo at 1 1");
            });
        }

        [Test]
        public void Computer_And_DamageControl_Commands_Return_Output()
        {
            var computerOutput = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "com");
            var damageOutput = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "dmg");

            Assert.IsNotNull(computerOutput);
            Assert.IsNotNull(damageOutput);
            Assert.IsTrue(computerOutput.Any() || damageOutput.Any());
        }

        [Test]
        public void PowerStatus_Command_Returns_SubsystemRows()
        {
            var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "pwr");

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Any(line => line.Contains("Power / Subsystem Status")));
            Assert.IsTrue(output.Any(line => line.Contains("STATE:")));
        }

        [Test]
        public void Impulse_InvalidDirection_RepromptsWithoutThrowing()
        {
            Assert.DoesNotThrow(() =>
            {
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "imp");
                var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "999");
                Assert.IsTrue(output == null || output.Any());
                Assert.AreEqual(1, _interact.Subscriber.PromptInfo.Level);
            });
        }

        [Test]
        public void Warp_InvalidFactor_RepromptsWithoutThrowing()
        {
            Assert.DoesNotThrow(() =>
            {
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "wrp");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "7");
                var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "99");
                Assert.IsTrue(output.Any(line => line.Contains("Invalid warp factor")));
            });
        }

        [Test]
        public void Subscriber_WarpArrival_Output_Does_Not_Return_Duplicate_Scan_Blocks()
        {
            var output = _game.SubscriberSendAndGetResponse("warp 1 course 7");

            var sectorLines = output.Where(line => line.StartsWith("Sector:", System.StringComparison.Ordinal)).ToList();
            Assert.LessOrEqual(sectorLines.Count, 1);
        }

        [Test]
        public void GaseousAnomaly_ConsumesExtraTurn_OnEachEntry()
        {
            var sector = _game.Map.Playership.GetSector();
            var y = _game.Map.Playership.Coordinate.Y;

            // Force two anomaly entries in two separate impulse commands.
            var firstX = _game.Map.Playership.Coordinate.X < 6 ? _game.Map.Playership.Coordinate.X + 1 : _game.Map.Playership.Coordinate.X - 1;
            var secondX = _game.Map.Playership.Coordinate.X < 6 ? _game.Map.Playership.Coordinate.X + 2 : _game.Map.Playership.Coordinate.X - 2;
            var direction = _game.Map.Playership.Coordinate.X < 6 ? "7" : "3";

            sector.Coordinates[firstX, y].Item = CoordinateItem.GaseousAnomaly;
            sector.Coordinates[firstX, y].Object = new GaseousAnomaly { Coordinate = sector.Coordinates[firstX, y] };
            sector.Coordinates[secondX, y].Item = CoordinateItem.GaseousAnomaly;
            sector.Coordinates[secondX, y].Object = new GaseousAnomaly { Coordinate = sector.Coordinates[secondX, y] };

            var startTime = _game.Map.timeRemaining;

            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "imp");
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, direction);
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "3");

            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "imp");
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, direction);
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "3");

            Assert.AreEqual(startTime - 2, _game.Map.timeRemaining);
        }

        [Test]
        public void IRSPlusPlusPlus_Consumes_One_Turn()
        {
            var startTime = _game.Map.timeRemaining;
            var startStardate = _game.Map.Stardate;

            var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "irs+++");

            Assert.IsNotNull(output);
            Assert.AreEqual(startTime - 1, _game.Map.timeRemaining);
            Assert.AreEqual(startStardate + 1, _game.Map.Stardate);
        }

        [Test]
        public void TacticsManual_Consumes_One_Turn()
        {
            var startTime = _game.Map.timeRemaining;
            var startStardate = _game.Map.Stardate;

            var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "tac");

            Assert.IsNotNull(output);
            Assert.AreEqual(startTime - 1, _game.Map.timeRemaining);
            Assert.AreEqual(startStardate + 1, _game.Map.Stardate);
        }

        [Test]
        public void TssCommand_Sets_Target_Lock()
        {
            var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "tss tor");
            Assert.IsTrue(output.Any(l => l.Contains("Target lock set")));

            var player = (Ship)_game.Map.Playership;
            Assert.AreEqual("tor", player.TargetSubsystemMnemonic);
            Assert.IsFalse(string.IsNullOrWhiteSpace(player.TargetShipName));
        }

        [Test]
        public void Missing_HostileMaxRetreatAttempts_Does_Not_Throw_During_Attack_Turn()
        {
            var config = new MissingRetreatAttemptsConfig();
            var game = new Game(config, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,1)), CoordinateItem.HostileShip)
                }
            });

            var interact = (Interaction)game.Interact;

            Assert.DoesNotThrow(() =>
            {
                interact.ReadAndOutput(game.Map.Playership, game.Map.Text, "imp");
                interact.ReadAndOutput(game.Map.Playership, game.Map.Text, "7");
                interact.ReadAndOutput(game.Map.Playership, game.Map.Text, "1");
            });
        }

        private sealed class MissingRetreatAttemptsConfig : IStarTrekKGSettings
        {
            private readonly StarTrekKGSettings _inner = new StarTrekKGSettings();

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
                if (string.Equals(name, "HostileMaxRetreatAttempts", System.StringComparison.Ordinal))
                {
                    throw new System.Configuration.SettingsPropertyNotFoundException("Missing for test.");
                }

                return _inner.GetSetting<T>(name);
            }
            public string Setting(string name) => this.GetSetting<string>(name);
            public T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false) => _inner.CheckAndCastValue<T>(name, element, whiteSpaceIsOk);
            public void Reset() => _inner.Reset();
        }
    }
}
