using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.ShipTests
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
            var hostileShip = new Ship(FactionName.Klingon, "ship1", new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))), this.TestMap, this.Game);
            var hostileShip2 = new Ship(FactionName.Klingon, "ship2", new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 5))), this.TestMap, this.Game);

            var activeRegion = this.TestMap.Regions.GetActive();
            activeRegion.AddShip(hostileShip, hostileShip.Sector);
            activeRegion.AddShip(hostileShip2, hostileShip2.Sector);

            this.VerifyMap();
        }

        public void SetupMapWith1Friendly()
        {
            this.TestMap = (new Map(new SetupOptions
                                        {
                                            AddNebulae = false,
                                            AddStars = false,
                                            Initialize = true,
                                            SectorDefs = new SectorDefs
                                                             {
                                                                 new SectorDef(
                                                                     new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)),
                                                                     SectorItem.PlayerShip),
                                                                 //todo: this needs to be in a random spo
                                                             }
                                        }, this.Game.Interact, this.Game.Config));

            this.VerifyMap();
        }

        public void SetupEmptyMap()
        {
            this.TestMap = (new Map(new SetupOptions
            {
                AddNebulae = false,
                AddStars = false,
                Initialize = true,
                SectorDefs = new SectorDefs
                {
                    new SectorDef(
                        new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Empty),
                    //todo: this needs to be in a random spo
                }
            }, this.Game.Interact, this.Game.Config));

            this.VerifyMap();
        }

        public void SetupMapWith1Hostile()
        {
            this.TestMap = new Map(new SetupOptions
                                  {
                                      AddNebulae = false,
                                      AddStars = false,
                                      Initialize = true,
                                      SectorDefs = new SectorDefs
                                                       {
                                                           new SectorDef(
                                                               new LocationDef(new Coordinate(0, 0),
                                                                               new Coordinate(0, 0)),
                                                               SectorItem.PlayerShip),
                                                           new SectorDef(
                                                               new LocationDef(new Coordinate(0, 0),
                                                                               new Coordinate(0, 1)), SectorItem.HostileShip),
                                                       }
                                  }, this.Game.Interact, this.Game.Config);
            this.VerifyMap();

            var hostileShip = this.TestMap.Regions.GetHostiles().Single();

            Assert.IsTrue(hostileShip.Energy > 0);
        }

        public void SetupMapWith1FedHostile()
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = true,
                SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(
                                                    new LocationDef(new Coordinate(0, 0),
                                                                    new Coordinate(0, 0)),
                                                    SectorItem.PlayerShip),
                                                new SectorDef(
                                                    new LocationDef(new Coordinate(0, 0),
                                                                    new Coordinate(0, 1)), SectorItem.HostileShip),
                                            }
            }, this.Game.Interact, this.Game.Config, FactionName.Federation);
            this.VerifyMap();
        }

        //public void SetupMapWith1HostileInNebula()
        //{
        //    this.TestMap = new Map(new SetupOptions
        //    {
        //        AddStars = false,
        //        Initialize = true,
        //        SectorDefs = new SectorDefs
        //                                               {
        //                                                   new SectorDef(
        //                                                       new LocationDef(new Coordinate(0, 0),
        //                                                                       new Coordinate(0, 0)),
        //                                                       SectorItem.Friendly),
        //                                                   new SectorDef(
        //                                                       new LocationDef(new Coordinate(0, 0),
        //                                                                       new Coordinate(0, 1)), SectorItem.Hostile),
        //                                               }
        //    }, this.Game.Write, this.Game.Config);
        //    this.VerifyMap();
        //}

        public void SetupMapWith1HostileAtSector(Coordinate friendlySector, Coordinate hostileSector, bool initialize = true)
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = initialize,
                SectorDefs = new SectorDefs
                                        {
                                            new SectorDef(
                                                new LocationDef(new Coordinate(0, 0),
                                                                friendlySector),
                                                SectorItem.PlayerShip),
                                            new SectorDef(
                                                new LocationDef(new Coordinate(0, 0),
                                                                hostileSector), SectorItem.HostileShip),
                                        }
            }, this.Game.Interact, this.Game.Config);
            this.VerifyMap();
        }

        public void SetupMapWith1FriendlyAtSector(Coordinate friendlySector)
        {
            this.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                {
                   new SectorDef(new LocationDef(new Coordinate(0, 0),friendlySector),SectorItem.PlayerShip)},
              AddStars = false
            }, this.Game.Interact, this.Game.Config);
            this.VerifyMap();
        }

        public void SetupMapWithStarbase()
        {
            this.TestMap = (new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = true,

                SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.PlayerShip), //todo: this needs to be in a random spo
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 5)), SectorItem.Starbase)
                                    }
            }, this.Game.Interact, this.Game.Config));

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));

            //var activeRegion = _testMap.Regions.GetActive();
            //activeRegion.AddShip(starbase, starbase.Sector);
            this.VerifyMap();
        }

        public void SetupNewMapOnly()
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = true,
                //GenerateMap = true
            }, this.Game.Interact, this.Game.Config);
            this.VerifyMap();
        }

        public void SetupBaseMap()
        {
            this.TestMap = new Map(null, this.Game.Interact, this.Game.Config);
            this.VerifyMap();
        }

        private void Startup()
        {
            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;

            DEFAULTS.REGION_MIN = 0;
            DEFAULTS.REGION_MAX = 0;

            this.Game = new Game((new StarTrekKGSettings()), false);
            this.Config = Game.Config;

            TestRunner.GetTestConstants();

            Assert.AreEqual(8, DEFAULTS.REGION_MAX);
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }
    }
}
