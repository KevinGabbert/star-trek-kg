using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Playfield.SectorTests
{
    [TestFixture]
    public class SectorTests: SectorTests_Base
    {
        [SetUp]
        public void Setup()
        {
            base._testSector =
                new Sector(this.Game.Map)
                {
                    Map = new Map(null, this.Game.Interact, this.Game.Config, this.Game),
                    Name = "Setup",
                    Scanned = false,
                    X = 0,
                    Y = 0
                };

        }
    
        [Test]
        public void New()
        {
            //*************** sector not being created with new Sector
            _testSector = new Sector(this.Game.Map);

            Assert.IsInstanceOf<Map>(_testSector.Map);
            base.SectorNewAsserts();
            Assert.IsNull(_testSector.Coordinates);
        }

        [Test]
        public void NewWithMap()
        {
            var baddieNames = new Stack<string>(new StarTrekKGSettings().ShipNames(FactionName.Klingon));

            _setup.SetupMapWith1Friendly();

            //you need to set ItemsToPopulat
            _testSector = new Sector(this.Game.Map, baddieNames, null, false);

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testSector.Map);

            //UnitTests.ShipTests.MapTests.MapTests.NoInitializeAsserts(_testSector.Map);
            Assert.AreEqual(string.Empty, _testSector.Name);
            Assert.IsInstanceOf<Coordinates>(_testSector.Coordinates);
            Assert.AreEqual(false, _testSector.Scanned);
            Assert.AreEqual(0, _testSector.X);
            Assert.AreEqual(0, _testSector.Y);
            Assert.AreEqual(true, _testSector.Empty);

            Assert.IsNotNull(_testSector.Coordinates);
        }

        [Test]
        public void Create()
        {
            var name = new List<string>();
            var systemNames = new StarTrekKGSettings().GetStarSystems();
            var klingonShipNames = new StarTrekKGSettings().ShipNames(FactionName.Klingon);
            name.Add(systemNames[0]);

            var names = new Stack<string>(name);

            int index;
            var newSector = new Sector(_setup.Game.Map);
            newSector.Create(names,
                               new Stack<string>(klingonShipNames), null, 
                               new Point(1, 1), out index, null);

            Assert.IsInstanceOf(typeof (Map), _testSector.Map);

            //todo: make sure that map is not set up with anyting

            Assert.AreEqual(0, newSector.GetHostiles().Count);
            Assert.IsInstanceOf<Coordinates>(newSector.Coordinates);
            Assert.AreEqual(false, newSector.Scanned);
            Assert.AreEqual(1, newSector.X);
            Assert.AreEqual(1, newSector.Y);
            Assert.AreEqual(true, newSector.Empty);
            Assert.AreEqual("Ariel", newSector.Name);
        }

        //When implemented:
        //INitialize
        //AddHostile - get rid of sector.addhostile
        //CreateHostileX
        //Populate
        //AddEmptySector
        //GetItem
        //OutOfBounds

        public void SetUpNewSectorWith(CoordinateItem item)
        {
            //throws in a random number of CoordinateItem
        }

        //todo: call InitializeSectors() and pass empty list<sector>
        //todo: call InitializeSectors() and put in duplicate sectors
        //todo: test AddSector() item XY mismatch

        [Ignore("")]
        [Test]
        public void VerifyNumberingSystem()
        {
            //go through all Sectors and verify that 1. it is numbered right, 2. that there is 1 of each.
        }

    }
}

//todo: size of List<Coordinate> this can be derived from the list.count above.
