using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

namespace UnitTests.ShipTests.Test_Harness_Objects
{
    public class Test_Setup
    {
        public Map TestMap { get; set; }

        public void SetupMapWith2Hostiles()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;

            TestRunner.GetTestConstants();

            Assert.AreEqual(8, Constants.QUADRANT_MAX);

            this.TestMap = (new Map(new GameConfig
            {
                Initialize = true,

                SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                    }
            }));

            //add a ship
            var hostileShip = new Ship("ship1", this.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));
            var hostileShip2 = new Ship("ship2", this.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 5))));

            var activeQuad = this.TestMap.Quadrants.GetActive();
            activeQuad.AddShip(hostileShip, hostileShip.Sector);
            activeQuad.AddShip(hostileShip2, hostileShip2.Sector);
        }
    }
}
