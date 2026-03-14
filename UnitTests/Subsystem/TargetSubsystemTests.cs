using System;
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
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Subsystem
{
    [TestFixture]
    public class TargetSubsystemTests
    {
        [Test]
        public void Phaser_TargetedShot_Uses_Fallback_When_TssLockStreak_Config_Is_Missing()
        {
            var game = new Game(new MissingLockStreakConfigSettings(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(1, 0)), CoordinateItem.HostileShip)
                }
            });

            var player = (StarTrek_KG.Actors.Ship)game.Map.Playership;
            var hostile = (StarTrek_KG.Actors.Ship)game.Map.Sectors.GetActive().GetHostiles().First();
            hostile.Name = "IKC Test";
            player.TargetShipName = hostile.Name;
            player.TargetSubsystemMnemonic = "tor";
            player.TargetSubsystemLockStreak = 1;

            var output = game.Interact.ReadAndOutput(player, game.Map.Text, "pha 8");

            Assert.IsFalse(output.Any(line => line.Contains("Unable to retrieve 'TSSLockStreakBonusPercent'")));
            Assert.IsTrue(output.Any(line =>
                line.Contains("Targeted hit damaged tor") ||
                line.Contains("Targeted shot on")));
        }

        private sealed class MissingLockStreakConfigSettings : IStarTrekKGSettings
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
                if (string.Equals(name, "TSSLockStreakBonusPercent", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("missing");
                }

                return _inner.GetSetting<T>(name);
            }

            public string Setting(string name) => this.GetSetting<string>(name);
            public T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false) => _inner.CheckAndCastValue<T>(name, element, whiteSpaceIsOk);
            public void Reset() => _inner.Reset();
        }
    }
}
