using NUnit.Framework;
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
            _testMapNoObjects = (new Map(new GameConfig
            {
                Initialize = true,
                GenerateMap = true,
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly)
                            }
            })); 
        }

        [Test]
        public void IsDockingLocation_Left()
        {
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 1, 1).Item = SectorItem.Starbase;
            //_testMapNoObjects.Quadrants.Active.Sectors[1, 1] = SectorItem.Starbase;

            var isDockingLocation = _testMapNoObjects.IsDockingLocation(0, 0, _testMapNoObjects.Quadrants.GetActive().Sectors);

            Assert.IsTrue(isDockingLocation);
        }

        [Test]
        public void IsDockingLocation_Right()
        {
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 1, 1).Item = SectorItem.Starbase;

            var isDockingLocation = _testMapNoObjects.IsDockingLocation(0, 1, _testMapNoObjects.Quadrants.GetActive().Sectors);

            Assert.IsTrue(isDockingLocation);
        }

        [Test]
        public void IsDockingLocation_Top()
        {
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 0, 0).Item = SectorItem.Starbase;

            var isDockingLocation = _testMapNoObjects.IsDockingLocation(1, 0, _testMapNoObjects.Quadrants.GetActive().Sectors);

            Assert.IsTrue(isDockingLocation);
        }

        [Test]
        public void IsDockingLocation_Bottom()
        {
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 1, 1).Item = SectorItem.Starbase;

            var isDockingLocation = _testMapNoObjects.IsDockingLocation(0, 0, _testMapNoObjects.Quadrants.GetActive().Sectors);

            Assert.IsTrue(isDockingLocation);
        }

        [Test]
        public void IsDockingLocation_BottomLeft()
        {
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 1, 0).Item = SectorItem.Starbase;

            var isDockingLocation = _testMapNoObjects.IsDockingLocation(0, 1, _testMapNoObjects.Quadrants.GetActive().Sectors);
  
            Assert.IsTrue(isDockingLocation);
        }

        [Test]
        public void NotDockingLocation()
        {
            var map = (new Map(new GameConfig
            {
                Initialize = true,
                GenerateMap = true,
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly)
                            }
            })); 

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Starbase;

            var isDockingLocation = map.IsDockingLocation(0, 0, map.Quadrants.GetActive().Sectors);

            Assert.IsFalse(isDockingLocation);
        }

        [Test]
        public void NotDockingLocation2()
        {
            var map = (new Map(new GameConfig
            {
                Initialize = true,
                GenerateMap = true,
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(4,4), new Coordinate(4, 4)), SectorItem.Friendly)
                            }
            }));

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Starbase;

            var isDockingLocation = map.IsDockingLocation(0, 0, map.Quadrants.GetActive().Sectors);

            Assert.IsFalse(isDockingLocation);
        }

        /// <summary>
        /// The code steers away from this, but it is possible to occupy the same space and it be a docking location
        /// TODO: Change this to be false, so we don't have to have other code prevent this bug
        /// </summary>
        [Test]
        public void StarbaseIsADockingLocation_BugVerification()
        {
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 2, 0).Item = SectorItem.Starbase;

            var isDockingLocation = _testMapNoObjects.IsDockingLocation(2, 0, _testMapNoObjects.Quadrants.GetActive().Sectors);

            Assert.IsTrue(isDockingLocation); 
        }

        [Test]
        public void StarbaseIsADockingLocation_BugVerification2()
        {
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 1, 0).Item = SectorItem.Starbase;

            var isDockingLocation = _testMapNoObjects.IsDockingLocation(0, 1, _testMapNoObjects.Quadrants.GetActive().Sectors);

            Assert.IsTrue(isDockingLocation);
        }
    }
}
