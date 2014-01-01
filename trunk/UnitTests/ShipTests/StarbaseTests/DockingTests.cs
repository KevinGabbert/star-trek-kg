using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

namespace UnitTests.ShipTests.StarbaseTests
{
    public class DockingTests
    {
        private Map _testMap; 

        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
        }


        private void SetupMapWithStarbase()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,

                SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 5)), SectorItem.Starbase)
                                    }
            }));

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));

            //var activeQuad = _testMap.Quadrants.GetActive();
            //activeQuad.AddShip(starbase, starbase.Sector);
        }

        [Test]
        public void ALLHostilesAttack_ShipUndocked_WithShields()
        {
            this.SetupMapWithStarbase();
        }
    }
}
