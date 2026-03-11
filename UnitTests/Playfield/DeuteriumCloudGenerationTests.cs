using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

namespace UnitTests.Playfield
{
    [TestFixture]
    public class DeuteriumCloudGenerationTests
    {
        [Test]
        public void DeuteriumClouds_Create_Guards_And_Mines_In_Sector()
        {
            var overrides = new ConfigOverrideSettings(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"DeuteriumSectorPercent", "0"},
                {"DeuteriumCloudSectorPercent", "100"},
                {"DeuteriumCloudMinSize", "1"},
                {"DeuteriumCloudMaxSize", "1"},
                {"DeuteriumCloudGuardCount", "2"},
                {"DeuteriumCloudMineChancePercent", "100"}
            });

            var game = new Game(overrides, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = true,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip)
                }
            });

            var sector = game.Map.Playership.GetSector();
            var cloudCount = sector.Coordinates.Count(c => c.Item == CoordinateItem.DeuteriumCloud);
            var guardCount = sector.GetHostiles().Count;
            var mineCount = sector.Coordinates.Count(c => c.Item == CoordinateItem.GraviticMine);

            Assert.GreaterOrEqual(cloudCount, 1);
            Assert.GreaterOrEqual(guardCount, 2);
            Assert.GreaterOrEqual(mineCount, 1);
        }

        private sealed class ConfigOverrideSettings : IStarTrekKGSettings
        {
            private readonly IStarTrekKGSettings _inner = new StarTrekKGSettings();
            private readonly IDictionary<string, string> _overrideValues;

            public ConfigOverrideSettings(IDictionary<string, string> overrideValues)
            {
                _overrideValues = overrideValues ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            public string SettingsFileName { get => _inner.SettingsFileName; set => _inner.SettingsFileName = value; }
            public bool ConfigError { get => _inner.ConfigError; set => _inner.ConfigError = value; }
            public System.Configuration.Configuration Get { get => _inner.Get; set => _inner.Get = value; }
            public string CurrentPrompt { get => _inner.CurrentPrompt; set => _inner.CurrentPrompt = value; }
            public List<string> NameHeaders => _inner.NameHeaders;
            public List<string> NameValues => _inner.NameValues;
            public List<string> SeverityValues => _inner.SeverityValues;
            public string HostileRegistryAndTypeClasses => _inner.HostileRegistryAndTypeClasses;
            public List<string> HostileFactionThreats => _inner.HostileFactionThreats;
            public bool IsError() => _inner.IsError();
            public string Setting(string settingName) => this.GetSetting<string>(settingName);
            public string GetText(string name) => _inner.GetText(name);
            public string GetSetting(string name) => this.GetSetting<string>(name);
            public string Mission() => _inner.Mission();
            public List<string> ShipNames(StarTrek_KG.TypeSafeEnums.FactionName stockBaddieFaction) => _inner.ShipNames(stockBaddieFaction);
            public List<string> SectorNames() => _inner.SectorNames();
            public StarTrek_KG.Config.Collections.MenuItems GetMenuItems(string submenuName) => _inner.GetMenuItems(submenuName);
            public StarTrek_KG.Config.Collections.CommandCollection GetCommands() => _inner.GetCommands();
            public StarTrek_KG.Config.Collections.FactionThreats GetFactionThreats() => _inner.GetFactionThreats();
            public List<string> RegistryAndTypeClasses(StarTrek_KG.TypeSafeEnums.FactionName stockBaddieFaction) => _inner.RegistryAndTypeClasses(stockBaddieFaction);
            public void VerifyNameInConfig(string nameToVerify) => _inner.VerifyNameInConfig(nameToVerify);
            public void RemoveConfigSetting(string settingToDelete) => _inner.RemoveConfigSetting(settingToDelete);
            public T GetSetting<T>(string name)
            {
                if (_overrideValues.TryGetValue(name, out var value))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return _inner.GetSetting<T>(name);
            }
            public System.Configuration.Configuration GetConfig() => _inner.GetConfig();
            public string ToString(ICollection<string> collectionToString) => _inner.ToString(collectionToString);
            public string NamesToString() => _inner.NamesToString();
        }
    }
}
