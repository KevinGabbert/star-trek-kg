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
            base._testRegion =
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
            _testRegion = new Sector(this.Game.Map);

            Assert.IsInstanceOf<Map>(_testRegion.Map);
            base.SectorNewAsserts();
            Assert.IsNull(_testRegion.Coordinates);
        }

        [Test]
        public void NewWithMap()
        {
            var baddieNames = new Stack<string>(new StarTrekKGSettings().ShipNames(FactionName.Klingon));

            _setup.SetupMapWith1Friendly();

            //you need to set ItemsToPopulat
            _testRegion = new Sector(this.Game.Map, baddieNames, null, false);

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testRegion.Map);

            //UnitTests.ShipTests.MapTests.MapTests.NoInitializeAsserts(_testRegion.Map);
            Assert.AreEqual(string.Empty, _testRegion.Name);
            Assert.IsInstanceOf<Coordinates>(_testRegion.Coordinates);
            Assert.AreEqual(false, _testRegion.Scanned);
            Assert.AreEqual(0, _testRegion.X);
            Assert.AreEqual(0, _testRegion.Y);
            Assert.AreEqual(true, _testRegion.Empty);

            Assert.IsNotNull(_testRegion.Coordinates);
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
            var newRegion = new Sector(_setup.Game.Map);
            newRegion.Create(names,
                               new Stack<string>(klingonShipNames), null, 
                               new Point(1, 1), out index, null);

            Assert.IsInstanceOf(typeof (Map), _testRegion.Map);

            //todo: make sure that map is not set up with anyting

            Assert.AreEqual(0, newRegion.GetHostiles().Count);
            Assert.IsInstanceOf<Coordinates>(newRegion.Coordinates);
            Assert.AreEqual(false, newRegion.Scanned);
            Assert.AreEqual(1, newRegion.X);
            Assert.AreEqual(1, newRegion.Y);
            Assert.AreEqual(true, newRegion.Empty);
            Assert.AreEqual("Ariel", newRegion.Name);
        }

        //When implemented:
        //INitialize
        //AddHostile - get rid of sector.addhostile
        //CreateHostileX
        //Populate
        //AddEmptySector
        //GetItem
        //OutOfBounds

        public void SetUpNewRegionWith(CoordinateItem item)
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
