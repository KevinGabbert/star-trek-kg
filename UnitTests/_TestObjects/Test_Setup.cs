using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Commands;
using StarTrek_KG.Config;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Types;

namespace UnitTests.TestObjects
{
    public class Test_Setup: IConfig
    {
        public IStarTrekKGSettings Config { get; set; }

        public Game Game { get; set; }
        public Navigation TestNavigation { get; set; }
        public Torpedoes TestPhotons { get; set; }
        public Computer TestComputer { get; set; }
        public Phasers TestPhasers { get; set; }
        public Shields TestShields { get; set; }
        public LongRangeScan TestLongRangeScan { get; set; }

        public IInteraction Write
        {
            get { return this.Game.Interact; }
        }

        public IMap TestMap
        {
            get
            {
                return this.Game.Map;
            }
            set
            {
                this.Game.Map = value;
            }
        }

        public Test_Setup()
        {
            Startup();
        }

        private void VerifyMap()
        {
            Assert.IsNotNull(this.TestMap);
            Assert.IsNotNull(this.TestMap.Write);
        }

        public void SetupMapWith2Hostiles()
        {
            this.SetupMapWith1Friendly();

            //add a ship
            var hostileShip = new Ship(FactionName.Klingon, "ship1", new Coordinate(new LocationDef(new Point(0, 0), new Point(2, 7))), this.TestMap);
            var hostileShip2 = new Ship(FactionName.Klingon, "ship2", new Coordinate(new LocationDef(new Point(0, 0), new Point(2, 5))), this.TestMap);

            var activeSector = this.TestMap.Sectors.GetActive();
            activeSector.AddShip(hostileShip, hostileShip.Coordinate);
            activeSector.AddShip(hostileShip2, hostileShip2.Coordinate);

            this.VerifyMap();
        }

        public void SetupMapWith1Friendly()
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddNebulae = false,
                AddStars = false,
                Initialize = true,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(
                        new LocationDef(new Point(0, 0), new Point(0, 0)),
                        CoordinateItem.PlayerShip),
                    //todo: this needs to be in a random spo
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            this.VerifyMap();
        }

        public void SetupEmptyMap()
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddNebulae = false,
                AddStars = false,
                Initialize = true,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(
                        new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.Empty),
                    //todo: this needs to be in a random spo
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            this.VerifyMap();
        }

        public void SetupMapWith1Hostile()
        {
            this.TestMap = new Map(new SetupOptions
                                  {
                                      AddNebulae = false,
                                      AddStars = false,
                                      Initialize = true,
                                      CoordinateDefs = new CoordinateDefs
                                                       {
                                                           new CoordinateDef(
                                                               new LocationDef(new Point(0, 0),
                                                                               new Point(0, 0)),
                                                               CoordinateItem.PlayerShip),
                                                           new CoordinateDef(
                                                               new LocationDef(new Point(0, 0),
                                                                               new Point(0, 1)), CoordinateItem.HostileShip),
                                                       }
                                  }, this.Game.Interact, this.Game.Config, this.Game);
            this.VerifyMap();

            var hostileShip = this.TestMap.Sectors.GetHostiles().Single();

            Assert.IsTrue(hostileShip.Energy > 0);
        }

        public void SetupMapWith1FedHostile()
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = true,
                CoordinateDefs = new CoordinateDefs
                                            {
                                                new CoordinateDef(
                                                    new LocationDef(new Point(0, 0),
                                                                    new Point(0, 0)),
                                                    CoordinateItem.PlayerShip),
                                                new CoordinateDef(
                                                    new LocationDef(new Point(0, 0),
                                                                    new Point(0, 1)), CoordinateItem.HostileShip),
                                            }
            }, this.Game.Interact, this.Game.Config, this.Game);
            this.VerifyMap();
        }

        //public void SetupMapWith1HostileInNebula()
        //{
        //    this.TestMap = new Map(new SetupOptions
        //    {
        //        AddStars = false,
        //        Initialize = true,
        //        CoordinateDefs = new CoordinateDefs
        //                                               {
        //                                                   new CoordinateDef(
        //                                                       new LocationDef(new Point(0, 0),
        //                                                                       new Point(0, 0)),
        //                                                       CoordinateItem.Friendly),
        //                                                   new CoordinateDef(
        //                                                       new LocationDef(new Point(0, 0),
        //                                                                       new Point(0, 1)), CoordinateItem.Hostile),
        //                                               }
        //    }, this.Game.Write, this.Game.Config);
        //    this.VerifyMap();
        //}

        public void SetupMapWith1HostileAtSector(Point friendlySector, Point hostileSector, bool initialize = true)
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = initialize,
                CoordinateDefs = new CoordinateDefs
                                        {
                                            new CoordinateDef(
                                                new LocationDef(new Point(0, 0),
                                                                friendlySector),
                                                CoordinateItem.PlayerShip),
                                            new CoordinateDef(
                                                new LocationDef(new Point(0, 0),
                                                                hostileSector), CoordinateItem.HostileShip),
                                        }
            }, this.Game.Interact, this.Game.Config, this.Game);
            this.VerifyMap();
        }

        public void SetupMapWith1FriendlyAtSector(Point friendlySector)
        {
            this.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                CoordinateDefs = new CoordinateDefs
                {
                   new CoordinateDef(new LocationDef(new Point(0, 0),friendlySector),CoordinateItem.PlayerShip)},
              AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);
            this.VerifyMap();
        }

        public void SetupMapWithStarbase()
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = true,

                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip), //todo: this needs to be in a random spo
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 5)), CoordinateItem.Starbase)
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Coordinate(new LocationDef(new Point(0, 0), new Point(2, 7))));

            //var activeSector = _testMap.Sectors.GetActive();
            //activeSector.AddShip(starbase, starbase.Coordinate);
            this.VerifyMap();
        }

        public void SetupNewMapOnly()
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = true,
                //GenerateMap = true
            }, this.Game.Interact, this.Game.Config, this.Game);
            this.VerifyMap();
        }

        public void SetupBaseMap()
        {
            this.TestMap = new Map(null, this.Game.Interact, this.Game.Config, this.Game);
            this.VerifyMap();
        }

        private void Startup()
        {
            DEFAULTS.COORDINATE_MIN = 0;
            DEFAULTS.COORDINATE_MAX = 0;

            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;

            this.Game = new Game(new UnitTestSettings(), false);
            this.Config = Game.Config;

            TestRunner.GetTestConstants();

            Assert.AreEqual(8, DEFAULTS.SECTOR_MAX);
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }

        private sealed class UnitTestSettings : IStarTrekKGSettings
        {
            private readonly StarTrekKGSettings _inner = new StarTrekKGSettings().GetConfig();

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
