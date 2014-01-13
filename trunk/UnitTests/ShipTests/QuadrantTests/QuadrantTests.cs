
using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.QuadrantTests
{
    [TestFixture]
    public class QuadrantTests: TestClass_Base
    {
        Quadrant _testQuadrant;

        [SetUp]
        public void Setup()
        {
            _testQuadrant = new Quadrant(this.Game.Map);

            _testQuadrant.Map = new Map(null, this.Game.Write);
            _testQuadrant.Name = "Setup";
            _testQuadrant.Scanned = false;

            _testQuadrant.X = 0;
            _testQuadrant.Y = 0;
        }
    
        [Test]
        public void New()
        {
            //*************** sector not being created with new quadrant
            _testQuadrant = new Quadrant(this.Game.Map);

            Assert.AreEqual(null, _testQuadrant.Map);
            this.QuadrantNewAsserts();
            Assert.IsNull(_testQuadrant.Sectors);
        }

        [Test]
        public void NewWithMap()
        {
            var systemNames = StarTrekKGSettings.GetStarSystems();

            _setup.SetupMapWith1Friendly();

            _testQuadrant = new Quadrant(this.Game.Map);

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
            var systemNames = StarTrekKGSettings.GetStarSystems();
            var klingonShipNames = StarTrekKGSettings.GetShips("Klingon");
            name.Add(systemNames[0]);

            var names = new Stack<string>(name);

            int index;
            var newQuadrant = new Quadrant(_setup.TestMap);
            newQuadrant.Create(new Map(null, _setup.Write), names,
                               new Stack<string>(klingonShipNames),
                               new Coordinate(1, 1), out index, null);

            Assert.IsInstanceOf(typeof(Map), _testQuadrant.Map);

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

        private void QuadrantNewAsserts()
        {
            Assert.AreEqual(string.Empty, _testQuadrant.Name);
            Assert.IsNull(_testQuadrant.Sectors);
            Assert.AreEqual(false, _testQuadrant.Scanned);
            Assert.AreEqual(0, _testQuadrant.X);
            Assert.AreEqual(0, _testQuadrant.Y);
            Assert.AreEqual(true, _testQuadrant.Empty);
            //Assert.AreEqual(0, _testQuadrant.GetHostiles().Count); //This would rightfully return an exception
        }

        [Ignore]
        [Test]
        private void VerifyNumberingSystem()
        {
            //go through all quadrants and verify that 1. it is numbered right, 2. that there is 1 of each.
        }

    }
}

//todo: size of List<Sector> this can be derived from the list.count above.
