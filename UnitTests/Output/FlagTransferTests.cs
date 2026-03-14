using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Commands;
using StarTrek_KG.Config.Elements;
using UnitTests.TestObjects;
using System.Text;

namespace UnitTests.Output
{
    [TestFixture]
    public class FlagTransferTests
    {
        private Game _game;
        private Interaction _interact;

        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
            _game = new Game(new ConfigOverrideSettings(), new SetupOptions
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
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,0)), CoordinateItem.HostileShip)
                }
            });

            _interact = (Interaction)_game.Interact;
            _interact.Output.Clear();
        }

        [Test]
        public void Tfg_Promotes_Boarded_Adjacent_Hostile_And_Moves_Fleet_With_Player()
        {
            var oldFlagship = (Ship)_game.Map.Playership;
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            target.Energy = 333;
            target.MaxEnergy = 333;
            Shields.For(target).Energy = 49;

            var boardingOutput = _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "brd");
            Assert.IsTrue(boardingOutput.Any(l => l.Contains("Boarding successful")));
            Assert.IsTrue(target.SecuredByBoarding);

            _interact.Output.Clear();
            var output = _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "tfg");

            Assert.IsTrue(output.Any(l => l.Contains("Command flag transferred")));
            Assert.AreSame(target, _game.Map.Playership);
            Assert.IsTrue(target.InPlayerFleet);
            Assert.IsTrue(oldFlagship.InPlayerFleet);
            Assert.AreEqual(Allegiance.GoodGuy, target.Allegiance);
            Assert.AreEqual(CoordinateItem.PlayerShip, target.Coordinate.Item);
            Assert.AreEqual(CoordinateItem.FriendlyShip, oldFlagship.Coordinate.Item);

            var destinationSector = _game.Map.Sectors[new Point(1, 0)];
            var destinationCoordinate = destinationSector.Coordinates.GetNoError(new Point(2, 2));
            _game.Map.SetPlayershipInLocation(_game.Map.Playership, _game.Map, new Location(destinationSector, destinationCoordinate));

            Assert.IsTrue(oldFlagship.InPlayerFleet);
            Assert.IsNotNull(oldFlagship.Coordinate);
            Assert.IsFalse(_game.Map.Playership.Coordinate.X == oldFlagship.Coordinate.X &&
                           _game.Map.Playership.Coordinate.Y == oldFlagship.Coordinate.Y);
        }

        [Test]
        public void Tfg_Updates_Prompt_To_New_Commanding_Ship_Name()
        {
            var oldFlagship = (Ship)_game.Map.Playership;
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            Shields.For(target).Energy = 49;

            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "brd");
            _interact.Output.Clear();
            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "tfg");

            Assert.AreEqual($"{target.Name} -> ", _interact.CurrentPrompt);
        }

        [Test]
        public void Tfg_Renders_Transferred_Flagship_With_Its_Original_Faction_Glyph()
        {
            var oldFlagship = (Ship)_game.Map.Playership;
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            Shields.For(target).Energy = 49;

            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "brd");
            _interact.Output.Clear();
            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "tfg");

            var render = new Render(_game.Map.Game.Interact, _game.Map.Game.Config);
            var sector = _game.Map.Sectors.GetActive();
            var location = _game.Map.Playership.GetLocation();
            var sb = new StringBuilder();
            _game.Map.Game.Interact.Output.Clear();

            render.CreateSRSViewScreen(sector, _game.Map, location, 1, sector.Name, false, sb);
            var lines = _game.Map.Game.Interact.Output.Queue.ToList();
            var klingonGlyph = _game.Config.Get.FactionDetails(target.Faction).designator;
            var playerRowIndex = 1 + location.Coordinate.Y;

            Assert.IsTrue(lines[playerRowIndex].Contains(klingonGlyph), lines[playerRowIndex]);
        }

        [Test]
        public void Tfg_Renders_Previous_Flagship_As_Friendly_Ship_Not_Null_Marker()
        {
            var oldFlagship = (Ship)_game.Map.Playership;
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            Shields.For(target).Energy = 49;

            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "brd");
            _interact.Output.Clear();
            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "tfg");

            var render = new Render(_game.Map.Game.Interact, _game.Map.Game.Config);
            var sector = _game.Map.Sectors.GetActive();
            var location = _game.Map.Playership.GetLocation();
            var sb = new StringBuilder();
            _game.Map.Game.Interact.Output.Clear();

            render.CreateSRSViewScreen(sector, _game.Map, location, 1, sector.Name, false, sb);
            var lines = _game.Map.Game.Interact.Output.Queue.ToList();
            var nullMarker = DEFAULTS.NULL_MARKER;

            Assert.IsFalse(lines.Any(line => line.Contains(nullMarker)));
        }

        [Test]
        public void Tfg_Fails_When_Adjacent_Hostile_Has_Not_Been_Boarded()
        {
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            Shields.For(target).Energy = 49;

            var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "tfg");

            Assert.IsTrue(output.Any(l => l.Contains("has not been secured by boarding")));
            Assert.AreNotSame(target, _game.Map.Playership);
        }

        [Test]
        public void Tfg_Allows_Borg_Takeover_When_FigureEight_ZipBug_Is_Present()
        {
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            target.Faction = FactionName.Borg;
            target.Name = "Borg Cube";

            var zipCoordinate = _game.Map.Sectors.GetActive().Coordinates.GetNoError(new Point(2, 0));
            var zipBug = new ZipBug
            {
                Coordinate = zipCoordinate,
                Form = ZipBug.ZipBugForm.FigureEight,
                Name = "Observer"
            };
            zipCoordinate.Item = CoordinateItem.ZipBug;
            zipCoordinate.Object = zipBug;

            var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "tfg");

            Assert.IsTrue(output.Any(l => l.Contains("Command flag transferred")));
            Assert.AreSame(target, _game.Map.Playership);
            Assert.AreEqual(Allegiance.GoodGuy, target.Allegiance);
        }

        [Test]
        public void FleetShip_Moves_Out_Of_The_Way_When_Playership_Impulses_Into_It()
        {
            var oldFlagship = (Ship)_game.Map.Playership;
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            Shields.For(target).Energy = 49;

            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "brd");
            _interact.Output.Clear();
            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "tfg");

            var targetXBeforeMove = _game.Map.Playership.Coordinate.X;
            var targetYBeforeMove = _game.Map.Playership.Coordinate.Y;
            var fleetXBeforeMove = oldFlagship.Coordinate.X;
            var fleetYBeforeMove = oldFlagship.Coordinate.Y;
            var movement = new Movement(_game.Map.Playership) { BlockedByObstacle = false };
            movement.Execute(MovementType.Impulse, NavDirection.Left, 1, out _, out _);

            Assert.AreEqual(0, _game.Map.Playership.Coordinate.X, $"player before=[{targetXBeforeMove},{targetYBeforeMove}] fleet before=[{fleetXBeforeMove},{fleetYBeforeMove}]");
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.Y, $"player before=[{targetXBeforeMove},{targetYBeforeMove}] fleet before=[{fleetXBeforeMove},{fleetYBeforeMove}]");
            Assert.AreNotEqual(_game.Map.Playership.Coordinate, oldFlagship.Coordinate);
            Assert.AreEqual(CoordinateItem.FriendlyShip, oldFlagship.Coordinate.Item);
        }

        [Test]
        public void FleetShip_Does_Not_Relocate_During_Normal_IntraSector_Player_Movement()
        {
            var oldFlagship = (Ship)_game.Map.Playership;
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            Shields.For(target).Energy = 49;

            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "brd");
            _interact.Output.Clear();
            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "tfg");

            var fleetXBeforeMove = oldFlagship.Coordinate.X;
            var fleetYBeforeMove = oldFlagship.Coordinate.Y;

            var movement = new Movement(_game.Map.Playership) { BlockedByObstacle = false };
            movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.AreEqual(fleetXBeforeMove, oldFlagship.Coordinate.X);
            Assert.AreEqual(fleetYBeforeMove, oldFlagship.Coordinate.Y);
            Assert.AreEqual(CoordinateItem.FriendlyShip, oldFlagship.Coordinate.Item);
        }

        [Test]
        public void Warp_After_Flag_Transfer_Does_Not_Return_Duplicate_Scan_Blocks()
        {
            var oldFlagship = (Ship)_game.Map.Playership;
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            Shields.For(target).Energy = 49;

            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "brd");
            _interact.Output.Clear();
            _interact.ReadAndOutput(oldFlagship, _game.Map.Text, "tfg");

            var output = _game.SubscriberSendAndGetResponse("warp 1 course 7");
            var sectorLines = output.Where(line => line.StartsWith("Sector:", System.StringComparison.Ordinal)).ToList();

            Assert.LessOrEqual(sectorLines.Count, 1, string.Join(" | ", sectorLines));
        }

        private sealed class ConfigOverrideSettings : IStarTrekKGSettings
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
                if (string.Equals(name, "BoardingSuccessMinRoll", System.StringComparison.OrdinalIgnoreCase))
                {
                    return (T)System.Convert.ChangeType("1", typeof(T));
                }

                return _inner.GetSetting<T>(name);
            }

            public string Setting(string name) => this.GetSetting<string>(name);
            public T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false) => _inner.CheckAndCastValue<T>(name, element, whiteSpaceIsOk);
            public void Reset() => _inner.Reset();
        }
    }
}
