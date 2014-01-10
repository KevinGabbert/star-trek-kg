using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
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

        public void SetupMapWith2Hostiles()
        {
            this.SetupMapWith1Friendly();

            //add a ship
            var hostileShip = new Ship("ship1", this.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));
            var hostileShip2 = new Ship("ship2", this.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 5))));

            var activeQuad = this.TestMap.Quadrants.GetActive();
            activeQuad.AddShip(hostileShip, hostileShip.Sector);
            activeQuad.AddShip(hostileShip2, hostileShip2.Sector);
        }

        public void SetupMapWith1Friendly()
        {
            this.Startup();

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
                                        }));
        }

        public void SetupMapWith1Hostile()
        {
            this.Startup();

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
                                  });
        }

        public void SetupMapWith1HostileAtSector(Coordinate friendlySector, Coordinate hostileSector)
        {
            this.Startup();

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
            });
        }


        private void Startup()
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
