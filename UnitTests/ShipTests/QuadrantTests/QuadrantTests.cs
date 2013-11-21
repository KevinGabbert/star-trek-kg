﻿
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace UnitTests.ShipTests.QuadrantTests
{
    [TestFixture]
    public class QuadrantTests
    {
        Quadrant _testQuadrant;

        [SetUp]
        public void Setup()
        {
            _testQuadrant = new Quadrant();

            _testQuadrant.Map = new Map(null);
            _testQuadrant.Name = "Setup";
            _testQuadrant.Hostiles = null;
            _testQuadrant.Scanned = false;

            _testQuadrant.X = 0;
            _testQuadrant.Y = 0;
        }
    
        [Test]
        public void New()
        {
            //*************** sector not being created with new quadrant
            _testQuadrant = new Quadrant();

            Assert.AreEqual(null, _testQuadrant.Map);
            this.QuadrantNewAsserts();
            Assert.IsNull(_testQuadrant.Sectors);
        }

        [Test]
        public void NewWithMap()
        {
            _testQuadrant = new Quadrant(new Map(null), new Stack<string>(StarTrekKGSettings.GetConfig().StarSystems.Cast<StarSystem>().Select(system => system.name).ToList()));

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testQuadrant.Map);

            //UnitTests.ShipTests.MapTests.MapTests.NoInitializeAsserts(_testQuadrant.Map);
            this.QuadrantNewAsserts();
            Assert.IsNotNull(_testQuadrant.Sectors);
        }

        [Test]
        public void Create()
        {
            var name = new List<string>();
            name.Add(StarTrekKGSettings.GetConfig().StarSystems.Cast<StarSystem>().Select(system => system.name).ToList()[0]);

            var names = new Stack<string>(name);

            int index;
            var newQuadrant = new Quadrant();
            newQuadrant.Create(new Map(null), names, 
                               new Stack<string>(Constants.KLINGON_SHIP_NAMES.ToList()),
                               new Coordinate(1, 1), out index, null);

            Assert.IsInstanceOf(typeof(Map), _testQuadrant.Map);

            //todo: make sure that map is not set up with anyting

            Assert.AreEqual(0, newQuadrant.Hostiles.Count);
            //Assert.AreEqual(null, newQuadrant.Sectors);
            Assert.AreEqual(false, newQuadrant.Scanned);
            Assert.AreEqual(1, newQuadrant.X);
            Assert.AreEqual(1, newQuadrant.Y);
            Assert.AreEqual(true, newQuadrant.Empty);
            Assert.AreEqual("Aaamazzara", newQuadrant.Name);
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
            Assert.AreEqual(0, _testQuadrant.Hostiles.Count);
            Assert.AreEqual(false, _testQuadrant.Scanned);
            Assert.AreEqual(0, _testQuadrant.X);
            Assert.AreEqual(0, _testQuadrant.Y);
            Assert.AreEqual(true, _testQuadrant.Empty);
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