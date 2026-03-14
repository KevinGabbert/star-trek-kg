using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Commands;
using StarTrek_KG.Config;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.TestObjects;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class GameTests: TestClass_Base
    {
        [Test]
        public void NewGame()
        {
            //interesting.. this test causes MapWith3HostilesTheConfigWayAddedInDescendingOrder2() to fail when running after this one does

            var game = new Game(new StarTrekKGSettings());
            
            Assert.IsInstanceOf<Map>(game.Map);

            new StarTrekKGSettings().Get = null;
        }

        [Test]
        public void EnemyTaunt()
        {
            _setup.SetupMapWith1Hostile();

            //_testSector = _setup.TestMap.Playership.GetSector();

            Game.EnemiesWillNowTaunt();
        }

        [Test]
        public void EnemyFedTaunt()
        {
            _setup.SetupMapWith1FedHostile();

            //_testSector = _setup.TestMap.Playership.GetSector();

            Game.EnemiesWillNowTaunt();

        }

        [TearDown]
        public void TearDown()
        {
            
        }

        //ctor
        //Initialize
        //Run
        //PlayOnce

        [Test]
        public void MoveGameTimeForward()
        {
            var map = new Map
            {
                Game = new Game(new StarTrekKGSettings())
            };

            //apparently, the only requirement for this is that an observed movement needs to happen
            map.Game.MoveTimeForward(map, new Point(0, 0), new Point(0, 1));

            Assert.AreEqual(-1, map.timeRemaining);
            Assert.AreEqual(1, map.Stardate);
        }

        [Test]
        public void MoveGameTimeForward2()
        {
            var map = new Map
            {
                Game = new Game(new StarTrekKGSettings())
            };

            //apparently, the only requirement for this is that an observed movement needs to happen
            map.Game.MoveTimeForward(map, new Point(0, 0), new Point(0, 0));

            //Time has not moved
            Assert.AreEqual(0, map.timeRemaining);
            Assert.AreEqual(0, map.Stardate);
        }

        [Test]
        public void StrictDeterministicStartup_AllowsNoHostiles()
        {
            var setup = new StarTrek_KG.Settings.SetupOptions
            {
                Initialize = true,
                AddStars = false,
                AddNebulae = false,
                StrictDeterministic = true,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), StarTrek_KG.Enums.CoordinateItem.PlayerShip)
                }
            };

            var game = new Game(new ConfigOverrideSettings(), setup);

            Assert.False(game.GameOver);
        }

        [Test]
        public void NonDeterministicStartup_WithNoHostiles_IsGameOver()
        {
            var setup = new StarTrek_KG.Settings.SetupOptions
            {
                Initialize = true,
                AddStars = false,
                AddNebulae = false,
                StrictDeterministic = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), StarTrek_KG.Enums.CoordinateItem.PlayerShip)
                }
            };

            var game = new Game(new ConfigOverrideSettings(), setup);

            Assert.True(game.GameOver);
        }

        /// <summary>
        /// Tests code in context with surrounding code
        /// </summary>
        [Ignore("")]
        [Test]
        public void MoveGameTimeForward3()
        {
            //todo: test this with a full map, and ship set up.  Then tell ship to move.  
            var map = new Map
            {
                Game = new Game(new StarTrekKGSettings())
            };

            //MovementTests.Move_Sector("w", 1*8);

            //Time has moved
            Assert.AreEqual(0, map.timeRemaining);
            Assert.AreEqual(0, map.Stardate);
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
            public System.Collections.Generic.List<CommandDef> LoadCommands() => _inner.LoadCommands();
            public StarTrekKGSettings GetConfig() => _inner.GetConfig();
            public System.Collections.Generic.List<string> ShipNames(FactionName faction) => _inner.ShipNames(faction);
            public System.Collections.Generic.List<FactionThreat> GetThreats(FactionName faction) => _inner.GetThreats(faction);
            public MenuItems GetMenuItems(string menuName) => _inner.GetMenuItems(menuName);
            public System.Collections.Generic.List<string> GetStarSystems() => _inner.GetStarSystems();
            public string GetText(string name) => _inner.GetText(name);
            public string GetText(string textToGet, string textToGet2) => _inner.GetText(textToGet, textToGet2);

            public T GetSetting<T>(string name)
            {
                if (name == "QuadrantFriendlyShipsPerFaction" ||
                    name == "BorgCubeCount" ||
                    name == "HostileOutpostSectorPercent" ||
                    name == "BlackHoleSectorPercent" ||
                    name == "TemporalRiftSectorPercent" ||
                    name == "WormholeSectorPercent" ||
                    name == "DeuteriumCloudSectorPercent" ||
                    name == "GaseousAnomalySectorPercent" ||
                    name == "SporeSectorPercent" ||
                    name == "ZipBugCount")
                {
                    return (T)System.Convert.ChangeType("0", typeof(T));
                }

                return _inner.GetSetting<T>(name);
            }

            public string Setting(string name) => this.GetSetting<string>(name);
            public T CheckAndCastValue<T>(string name, NameValue element, bool whiteSpaceIsOk = false) => _inner.CheckAndCastValue<T>(name, element, whiteSpaceIsOk);
            public void Reset() => _inner.Reset();
        }
    }
}
