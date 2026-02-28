using System;
using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Commands;
using StarTrek_KG.Config;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class FeatureFlagStartupTests
    {
        [Test]
        public void MissingFeatureFlags_DefaultToDisabled()
        {
            var config = new FeatureToggleSettings(
                overrideValues: null,
                missingKeys: new[]
                {
                    "enable-deuterium-sectors",
                    "enable-gravitic-mines"
                });

            var game = new Game(config);

            Assert.False(game.Map.GameConfig.AddDeuterium);
            Assert.False(game.Map.GameConfig.AddGraviticMines);
        }

        [Test]
        public void PresentFeatureFlags_True_EnablesFeatures()
        {
            var config = new FeatureToggleSettings(
                new Dictionary<string, string>
                {
                    { "enable-deuterium-sectors", "true" },
                    { "enable-gravitic-mines", "true" }
                });

            var game = new Game(config);

            Assert.True(game.Map.GameConfig.AddDeuterium);
            Assert.True(game.Map.GameConfig.AddGraviticMines);
        }

        [Test]
        public void PresentFeatureFlags_False_DisablesFeatures()
        {
            var config = new FeatureToggleSettings(
                new Dictionary<string, string>
                {
                    { "enable-deuterium-sectors", "false" },
                    { "enable-gravitic-mines", "false" }
                });

            var game = new Game(config);

            Assert.False(game.Map.GameConfig.AddDeuterium);
            Assert.False(game.Map.GameConfig.AddGraviticMines);
        }

        private sealed class FeatureToggleSettings : IStarTrekKGSettings
        {
            private readonly StarTrekKGSettings _inner;
            private readonly IDictionary<string, string> _overrideValues;
            private readonly HashSet<string> _missingKeys;

            public FeatureToggleSettings(
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
