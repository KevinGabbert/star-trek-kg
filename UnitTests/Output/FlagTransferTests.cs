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
        public void Tfg_Promotes_Adjacent_Disarmed_Hostile_And_Moves_Fleet_With_Player()
        {
            var oldFlagship = (Ship)_game.Map.Playership;
            var target = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            target.Energy = 333;
            target.MaxEnergy = 333;
            Torpedoes.For(target).Damage = 1;
            Phasers.For(target).Damage = 1;
            target.Subsystems.Single(s => s.Type == SubsystemType.Disruptors).Damage = 1;

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
