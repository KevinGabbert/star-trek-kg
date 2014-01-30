
using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace UnitTests.ShipTests.QuadrantTests
{
    [TestFixture]
    public class QuadrantTests: QuadrantTests_Base
    {
        [SetUp]
        public void Setup()
        {
            base._testQuadrant = new Quadrant(this.Game.Map);

            base._testQuadrant.Map = new Map(null, this.Game.Write, this.Game.Config);
            base._testQuadrant.Name = "Setup";
            base._testQuadrant.Scanned = false;
            base._testQuadrant.X = 0;
            base._testQuadrant.Y = 0;
        }
    
        [Test]
        public void New()
        {
            //*************** sector not being created with new quadrant
            _testQuadrant = new Quadrant(this.Game.Map);

            Assert.IsInstanceOf<Map>(_testQuadrant.Map);
            base.QuadrantNewAsserts();
            Assert.IsNull(_testQuadrant.Sectors);
        }

        [Test]
        public void NewWithMap()
        {
            var baddieNames = new Stack<string>((new StarTrekKGSettings()).GetShips("Klingon"));

            _setup.SetupMapWith1Friendly();

            //you need to set ItemsToPopulat
            _testQuadrant = new Quadrant(this.Game.Map, baddieNames, false);

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testQuadrant.Map);

            //UnitTests.ShipTests.MapTests.MapTests.NoInitializeAsserts(_testQuadrant.Map);
            Assert.AreEqual(string.Empty, _testQuadrant.Name);
            Assert.IsInstanceOf<Sectors>(_testQuadrant.Sectors);
            Assert.AreEqual(false, _testQuadrant.Scanned);
            Assert.AreEqual(0, _testQuadrant.X);
            Assert.AreEqual(0, _testQuadrant.Y);
            Assert.AreEqual(true, _testQuadrant.Empty);

            Assert.IsNotNull(_testQuadrant.Sectors);
        }

        [Test]
        public void Create()
        {
            var name = new List<string>();
            var systemNames = (new StarTrekKGSettings()).GetStarSystems();
            var klingonShipNames = (new StarTrekKGSettings()).GetShips("Klingon");
            name.Add(systemNames[0]);

            var names = new Stack<string>(name);

            int index;
            var newQuadrant = new Quadrant(_setup.Game.Map);
            newQuadrant.Create(names,
                               new Stack<string>(klingonShipNames),
                               new Coordinate(1, 1), out index, null);

            Assert.IsInstanceOf(typeof (Map), _testQuadrant.Map);

            //todo: make sure that map is not set up with anyting

            Assert.AreEqual(0, newQuadrant.GetHostiles().Count);
            Assert.IsInstanceOf<Sectors>(newQuadrant.Sectors);
            Assert.AreEqual(false, newQuadrant.Scanned);
            Assert.AreEqual(1, newQuadrant.X);
            Assert.AreEqual(1, newQuadrant.Y);
            Assert.AreEqual(true, newQuadrant.Empty);
            Assert.AreEqual("Ariel", newQuadrant.Name);
        }

        //When implemented:
        //INitialize
        //AddHostile - get rid of sector.addhostile
        //CreateHostileX
        //Populate
        //AddEmptySector
        //GetItem
        //OutOfBounds

        public void SetUpNewQuadrantWith(SectorItem item)
        {
            //throws in a random number of SectorItem
        }

        //todo: call InitializeSectors() and pass empty list<sector>
        //todo: call InitializeSectors() and put in duplicate sectors
        //todo: test AddSector() item XY mismatch

        [Ignore]
        [Test]
        private void VerifyNumberingSystem()
        {
            //go through all quadrants and verify that 1. it is numbered right, 2. that there is 1 of each.
        }

    }
}

//todo: size of List<Sector> this can be derived from the list.count above.
