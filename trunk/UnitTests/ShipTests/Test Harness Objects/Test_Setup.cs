using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.Test_Harness_Objects
{
    public class Test_Setup
    {
        public Map TestMap { get; set; }
        public Navigation TestNavigation { get; set; }
        public Torpedoes TestPhotons { get; set; }
        public Computer TestComputer { get; set; }
        public Computer TestPhasers { get; set; }

        public Write Write { get; set; }

        public Test_Setup()
        {
            Startup();
        }

        private void VerifyMap()
        {
            Assert.IsInstanceOf(typeof(Map), this.TestMap);
            Assert.IsInstanceOf(typeof(Write), this.TestMap.Write);
        }

        public void SetupMapWith2Hostiles()
        {
            this.SetupMapWith1Friendly();

            //add a ship
            var hostileShip = new Ship("ship1", this.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))), this.Write);
            var hostileShip2 = new Ship("ship2", this.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 5))), this.Write);

            var activeQuad = this.TestMap.Quadrants.GetActive();
            activeQuad.AddShip(hostileShip, hostileShip.Sector);
            activeQuad.AddShip(hostileShip2, hostileShip2.Sector);

            this.VerifyMap();
        }

        public void SetupMapWith1Friendly()
        {
            this.TestMap = (new Map(new GameConfig
                                        {
                                            Initialize = true,
                                            SectorDefs = new SectorDefs
                                                             {
                                                                 new SectorDef(
                                                                     new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)),
                                                                     SectorItem.Friendly),
                                                                 //todo: this needs to be in a random spo
                                                             }
                                        }, this.Write));

            this.VerifyMap();
        }

        public void SetupMapWith1Hostile()
        {
            this.TestMap = new Map(new GameConfig
                                  {
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
                                  }, this.Write);
            this.VerifyMap();
        }

        public void SetupMapWith1HostileAtSector(Coordinate friendlySector, Coordinate hostileSector)
        {
            this.TestMap = new Map(new GameConfig
            {
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
            }, this.Write);
            this.VerifyMap();
        }

        public void SetupMapWith1FriendlyAtSector(Coordinate friendlySector)
        {
            this.TestMap = new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                {
                   new SectorDef(new LocationDef(new Coordinate(0, 0),friendlySector),SectorItem.Friendly)},
              AddStars = false
            }, this.Write);
            this.VerifyMap();
        }

        public void SetupMapWithStarbase()
        {
            this.TestMap = (new Map(new GameConfig
            {
                Initialize = true,

                SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 5)), SectorItem.Starbase)
                                    }
            }, this.Write));

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));

            //var activeQuad = _testMap.Quadrants.GetActive();
            //activeQuad.AddShip(starbase, starbase.Sector);
            this.VerifyMap();
        }

        public void SetupNewMapOnly()
        {
            this.TestMap = new Map(new GameConfig
            {
                Initialize = true,
                //GenerateMap = true
            }, this.Write);
            this.VerifyMap();
        }

        public  void SetupBaseMap()
        {
            this.TestMap = new Map(null, this.Write);
            this.VerifyMap();
        }

        private void Startup()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;

            this.Write = new Write(this.TestMap);

            TestRunner.GetTestConstants();

            Assert.AreEqual(8, Constants.QUADRANT_MAX);
        }
    }
}
