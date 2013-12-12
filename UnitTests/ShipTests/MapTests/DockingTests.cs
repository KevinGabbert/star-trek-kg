using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

namespace UnitTests.ShipTests.MapTests
{
    [TestFixture]
    public class DockingTests
    {
        //Todo: docking energy tests

        Map _testMapNoObjects;

        [SetUp]
        public void Setup()
        {
            Constants.SECTOR_MIN = StarTrekKGSettings.GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = StarTrekKGSettings.GetSetting<int>("SECTOR_MAX");

            Constants.QUADRANT_MIN = StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = StarTrekKGSettings.GetSetting<int>("QuadrantMax");

            _testMapNoObjects = (new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly)
                            }
            }));

        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
        }

        [Test]
        public void IsDockingLocation_Left()
        {
            Assert.IsTrue(IsDockingLocation(1, 1, 0, 0));
        }

        [Test]
        public void IsDockingLocation_Right()
        {
            Assert.IsTrue(IsDockingLocation(1, 1, 0, 1));
        }

        [Test]
        public void IsDockingLocation_Top()
        {
            Assert.IsTrue(IsDockingLocation(0, 0, 1, 0));
        }

        [Test]
        public void IsDockingLocation_Bottom()
        {
            Assert.IsTrue(IsDockingLocation(1, 1, 1, 0));
        }

        [Test]
        public void IsDockingLocation_BottomLeft()
        {
            Assert.IsTrue(IsDockingLocation(1, 0, 0, 1));
        }

        [Test]
        public void NOTDockingLocation()
        {
            Assert.IsFalse(IsDockingLocation(3, 3, 0, 0));
        }

        [Test]
        public void NotDockingLocation2()
        {
            var map = (new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(4,4), new Coordinate(4, 4)), SectorItem.Friendly)
                            }
            }));

            Assert.IsFalse(IsDockingLocation(map, 4, 4, 4, 4));
        }

        /// <summary>
        /// The code steers away from this, but it is possible to occupy the same space and it be a docking location
        /// TODO: Change this to be false, so we don't have to have other code prevent this bug
        /// </summary>
        [Test]
        public void StarbaseIsADockingLocation_BugVerification()
        {
            Assert.IsFalse(IsDockingLocation(2, 0, 2, 0));
        }


        /// <summary>
        /// TODO: why is this a bug?
        /// </summary>
        [Test]
        public void StarbaseIsADockingLocation_BugVerification2()
        {
            Assert.IsTrue(IsDockingLocation(1, 0, 0, 1));
        }   
     
        private bool IsDockingLocation(int sectorX, int sectorY, int locationX, int locationY)
        {
            return this.IsDockingLocation(_testMapNoObjects, sectorX, sectorY, locationX, locationY);
        }

        private bool IsDockingLocation(Map map, int sectorX, int sectorY, int locationX, int locationY)
        {
            Sector.Get(map.Quadrants.GetActive().Sectors, sectorX, sectorY).Item = SectorItem.Starbase;

            var isDockingLocation = map.IsDockingLocation(locationX, locationY, _testMapNoObjects.Quadrants.GetActive().Sectors);
            return isDockingLocation;
        }
    }
}
