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

        public Command Command { get; set; }
        public Write Write { get; set; }
        public Draw Draw { get; set; }

        public Test_Setup()
        {
            Startup();
        }

        public void SetupMapWith2Hostiles()
        {
            this.SetupMapWith1Friendly();

            //add a ship
            var hostileShip = new Ship("ship1", this.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))), this.Write, this.Command);
            var hostileShip2 = new Ship("ship2", this.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 5))), this.Write, this.Command);

            var activeQuad = this.TestMap.Quadrants.GetActive();
            activeQuad.AddShip(hostileShip, hostileShip.Sector);
            activeQuad.AddShip(hostileShip2, hostileShip2.Sector);
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
                                        }, this.Write, this.Command));
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
                                  }, this.Write, this.Command);
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
            }, this.Write, this.Command);
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
            }, this.Write, this.Command));

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));

            //var activeQuad = _testMap.Quadrants.GetActive();
            //activeQuad.AddShip(starbase, starbase.Sector);
        }

        public void SetupNewMapOnly()
        {
            this.TestMap = new Map(new GameConfig
            {
                Initialize = true,
                //GenerateMap = true
            }, this.Write, this.Command);
        }

        public  void SetupBaseMap()
        {
            this.TestMap = new Map(null, this.Write, this.Command);
        }


        private static void Startup()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;

            TestRunner.GetTestConstants();

            Assert.AreEqual(8, Constants.QUADRANT_MAX);
        }
    }
}
