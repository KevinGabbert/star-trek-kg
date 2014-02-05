using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

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

        public IOutputWrite Write
        {
            get { return this.Game.Write; }
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
            var hostileShip = new Ship("", "ship1", new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))), this.TestMap);
            var hostileShip2 = new Ship("", "ship2", new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 5))), this.TestMap);

            var activeQuad = this.TestMap.Quadrants.GetActive();
            activeQuad.AddShip(hostileShip, hostileShip.Sector);
            activeQuad.AddShip(hostileShip2, hostileShip2.Sector);

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
                                                                     SectorItem.Friendly),
                                                                 //todo: this needs to be in a random spo
                                                             }
                                        }, this.Game.Write, this.Game.Config));

            this.VerifyMap();
        }

        public void SetupMapWith1Hostile()
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
                                                               SectorItem.Friendly),
                                                           new SectorDef(
                                                               new LocationDef(new Coordinate(0, 0),
                                                                               new Coordinate(0, 1)), SectorItem.Hostile),
                                                       }
                                  }, this.Game.Write, this.Game.Config);
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

        public void SetupMapWith1HostileAtSector(Coordinate friendlySector, Coordinate hostileSector)
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = true,
                SectorDefs = new SectorDefs
                                        {
                                            new SectorDef(
                                                new LocationDef(new Coordinate(0, 0),
                                                                friendlySector),
                                                SectorItem.Friendly),
                                            new SectorDef(
                                                new LocationDef(new Coordinate(0, 0),
                                                                hostileSector), SectorItem.Hostile),
                                        }
            }, this.Game.Write, this.Game.Config);
            this.VerifyMap();
        }

        public void SetupMapWith1FriendlyAtSector(Coordinate friendlySector)
        {
            this.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                {
                   new SectorDef(new LocationDef(new Coordinate(0, 0),friendlySector),SectorItem.Friendly)},
              AddStars = false
            }, this.Game.Write, this.Game.Config);
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
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 5)), SectorItem.Starbase)
                                    }
            }, this.Game.Write, this.Game.Config));

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));

            //var activeQuad = _testMap.Quadrants.GetActive();
            //activeQuad.AddShip(starbase, starbase.Sector);
            this.VerifyMap();
        }

        public void SetupNewMapOnly()
        {
            this.TestMap = new Map(new SetupOptions
            {
                AddStars = false,
                Initialize = true,
                //GenerateMap = true
            }, this.Game.Write, this.Game.Config);
            this.VerifyMap();
        }

        public void SetupBaseMap()
        {
            this.TestMap = new Map(null, this.Game.Write, this.Game.Config);
            this.VerifyMap();
        }

        private void Startup()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;

            this.Game = new Game((new StarTrekKGSettings()), false);
            this.Config = Game.Config;

            TestRunner.GetTestConstants();

            Assert.AreEqual(8, Constants.QUADRANT_MAX);
        }
    }
}
