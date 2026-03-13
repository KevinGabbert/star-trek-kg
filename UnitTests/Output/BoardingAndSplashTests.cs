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
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class BoardingAndSplashTests
    {
        private Game _game;
        private Interaction _interact;

        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
            var config = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BoardingSuccessMinRoll", "1"}
            });
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
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,0)), CoordinateItem.HostileShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(2,0)), CoordinateItem.Starbase)
                }
            });

            _interact = (Interaction)_game.Interact;
            _interact.Output.Clear();
        }

        [Test]
        public void Boarding_Succeeds_When_Adjacent_And_Weapons_Are_Down()
        {
            var hostile = _game.Map.Sectors.GetActive().GetHostiles().First();
            Torpedoes.For(hostile).Damage = 1;
            Phasers.For(hostile).Damage = 1;
            hostile.Subsystems.Single(s => s.Type == StarTrek_KG.TypeSafeEnums.SubsystemType.Disruptors).Damage = 1;

            Torpedoes.For(hostile).Count = 4;
            hostile.Energy = 250;

            var player = (Ship)_game.Map.Playership;
            player.Prisoners = 0;
            var startEnergy = player.Energy;
            var startTorpedoes = Torpedoes.For(player).Count;

            var output = _interact.ReadAndOutput(player, _game.Map.Text, "brd");

            Assert.IsTrue(output.Any(l => l.Contains("Boarding successful")));
            Assert.IsTrue(output.Any(l => l.Contains("Captured hostile crew")));
            Assert.AreEqual(1, _game.Map.Sectors.GetActive().GetHostiles().Count);
            Assert.Greater(player.Energy, startEnergy);
            Assert.Greater(Torpedoes.For(player).Count, startTorpedoes);
            Assert.Less(hostile.Energy, 250);
            Assert.Less(Torpedoes.For(hostile).Count, 4);
            Assert.Greater(player.Prisoners, 0);
        }

        [Test]
        public void Boarding_Fails_When_Target_Weapons_Are_Still_Active()
        {
            var player = _game.Map.Playership;
            var output = _interact.ReadAndOutput(player, _game.Map.Text, "brd");
            Assert.IsTrue(output.Any(l => l.Contains("still has active weapons")));
        }

        [Test]
        public void DestroyedHostile_Emits_SplashDamage_Message()
        {
            var active = _game.Map.Sectors.GetActive();
            var doomed = active.GetHostiles().First();
            doomed.Destroyed = true;
            var player = _game.Map.Playership;
            var shields = Shields.For(player);
            shields.Energy = 500;
            var shieldsBefore = shields.Energy;

            _game.Map.RemoveDestroyedShipsAndScavenge(new List<IShip> { doomed });

            Assert.Less(Shields.For(player).Energy, shieldsBefore);
        }

        [Test]
        public void Docking_Transfers_Prisoners_And_Triggers_Energy_Bonuses()
        {
            var player = (Ship)_game.Map.Playership;
            var hostile = (Ship)_game.Map.Sectors.GetActive().GetHostiles().First();
            player.Prisoners = 300;

            var playerMaxBefore = player.MaxEnergy;
            var hostileMaxBefore = hostile.MaxEnergy;

            var nav = Navigation.For(player);
            var dockMethod = typeof(Navigation).GetMethod("SuccessfulDockWithStarbase", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(dockMethod);
            dockMethod.Invoke(nav, null);

            Assert.AreEqual(0, player.Prisoners);
            Assert.Greater(player.MaxEnergy, playerMaxBefore);
            Assert.Greater(hostile.MaxEnergy, hostileMaxBefore);
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
