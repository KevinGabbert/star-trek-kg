using System;
using System.Collections.Generic;
using System.Configuration;
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
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class WarGamesStartupTests
    {
        [Test]
        public void ExecuteConfiguredPostStartCommand_DefaultsToSrs()
        {
            var game = CreateGame();
            game.RunSubscriber();

            var lines = WarGamesStartup.ExecuteConfiguredPostStartCommand(game, new ConfigOverrideSettings(missingKeys: new[] { "war-games-start-command" }));

            Assert.That(lines, Is.Not.Empty);
            Assert.AreEqual(SubsystemType.ShortRangeScan, game.Interact.Subscriber.PromptInfo.SubSystem);
        }

        [Test]
        public void ExecuteConfiguredPostStartCommand_UsesConfiguredCommand()
        {
            var game = CreateGame();
            game.RunSubscriber();

            var lines = WarGamesStartup.ExecuteConfiguredPostStartCommand(game,
                new ConfigOverrideSettings(new Dictionary<string, string>
                {
                    {"war-games-start-command", "lrs"}
                }));

            Assert.That(lines, Is.Not.Empty);
            Assert.AreEqual(SubsystemType.LongRangeScan, game.Interact.Subscriber.PromptInfo.SubSystem);
        }

        private static Game CreateGame()
        {
            var config = new StarTrekKGSettings();
            var setup = new SetupOptions
            {
                Initialize = true,
                IsWarGamesMode = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 1)), CoordinateItem.HostileShip)
                }
            };

            return new Game(config, setup);
        }

        private sealed class ConfigOverrideSettings : IStarTrekKGSettings
        {
            private readonly StarTrekKGSettings _inner;
            private readonly IDictionary<string, string> _overrideValues;
            private readonly HashSet<string> _missingKeys;

            public ConfigOverrideSettings(
                IDictionary<string, string> overrideValues = null,
                IEnumerable<string> missingKeys = null)
            {
                _inner = new StarTrekKGSettings();
                _inner.Get = _inner.GetConfig();
                _overrideValues = overrideValues ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _missingKeys = new HashSet<string>(missingKeys ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            }

            public StarTrekKGSettings Get
            {
                get => _inner.Get;
                set => _inner.Get = value;
            }

            public Names StarSystems => _inner.StarSystems;
            public NameValues ConsoleText => _inner.ConsoleText;
            public Factions Factions => _inner.Factions;
            public NameValues GameSettings => _inner.GameSettings;
            public MenusElement Menus => _inner.Menus;

            public List<CommandDef> LoadCommands()
            {
                return _inner.LoadCommands();
            }

            public StarTrekKGSettings GetConfig()
            {
                return _inner.GetConfig();
            }

            public List<string> ShipNames(FactionName faction)
            {
                return _inner.ShipNames(faction);
            }

            public List<FactionThreat> GetThreats(FactionName faction)
            {
                return _inner.GetThreats(faction);
            }

            public MenuItems GetMenuItems(string menuName)
            {
                return _inner.GetMenuItems(menuName);
            }

            public List<string> GetStarSystems()
            {
                return _inner.GetStarSystems();
            }

            public string GetText(string name)
            {
                return _inner.GetText(name);
            }

            public string GetText(string textToGet, string textToGet2)
            {
                return _inner.GetText(textToGet, textToGet2);
            }

            public T GetSetting<T>(string name)
            {
                if (_missingKeys.Contains(name))
                {
                    throw new ConfigurationErrorsException($"Missing setting: {name}");
                }

                if (_overrideValues.TryGetValue(name, out var value))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return _inner.GetSetting<T>(name);
            }

            public string Setting(string name)
            {
                return this.GetSetting<string>(name);
            }

            public T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false)
            {
                return _inner.CheckAndCastValue<T>(name, element, whiteSpaceIsOk);
            }

            public void Reset()
            {
                _inner.Reset();
            }
        }
    }
}
