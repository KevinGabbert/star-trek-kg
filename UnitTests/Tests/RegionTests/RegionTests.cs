
using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.ShipTests.RegionTests
{
    [TestFixture]
    public class RegionTests: RegionTests_Base
    {
        [SetUp]
        public void Setup()
        {
            base._testRegion = new Region(this.Game.Map);

            base._testRegion.Map = new Map(null, this.Game.Interact, this.Game.Config, this.Game);
            base._testRegion.Name = "Setup";
            base._testRegion.Scanned = false;
            base._testRegion.X = 0;
            base._testRegion.Y = 0;
        }
    
        [Test]
        public void New()
        {
            //*************** sector not being created with new Region
            _testRegion = new Region(this.Game.Map);

            Assert.IsInstanceOf<Map>(_testRegion.Map);
            base.RegionNewAsserts();
            Assert.IsNull(_testRegion.Sectors);
        }

        [Test]
        public void NewWithMap()
        {
            var baddieNames = new Stack<string>((new StarTrekKGSettings()).FactionShips(FactionName.Klingon));

            _setup.SetupMapWith1Friendly();

            //you need to set ItemsToPopulat
            _testRegion = new Region(this.Game.Map, baddieNames, null, false);

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testRegion.Map);

            //UnitTests.ShipTests.MapTests.MapTests.NoInitializeAsserts(_testRegion.Map);
            Assert.AreEqual(string.Empty, _testRegion.Name);
            Assert.IsInstanceOf<Sectors>(_testRegion.Sectors);
            Assert.AreEqual(false, _testRegion.Scanned);
            Assert.AreEqual(0, _testRegion.X);
            Assert.AreEqual(0, _testRegion.Y);
            Assert.AreEqual(true, _testRegion.Empty);

            Assert.IsNotNull(_testRegion.Sectors);
        }

        [Test]
        public void Create()
        {
            var name = new List<string>();
            var systemNames = (new StarTrekKGSettings()).GetStarSystems();
            var klingonShipNames = (new StarTrekKGSettings()).FactionShips(FactionName.Klingon);
            name.Add(systemNames[0]);

            var names = new Stack<string>(name);

            int index;
            var newRegion = new Region(_setup.Game.Map);
            newRegion.Create(names,
                               new Stack<string>(klingonShipNames), null, 
                               new Coordinate(1, 1), out index, null);

            Assert.IsInstanceOf(typeof (Map), _testRegion.Map);

            //todo: make sure that map is not set up with anyting

            Assert.AreEqual(0, newRegion.GetHostiles().Count);
            Assert.IsInstanceOf<Sectors>(newRegion.Sectors);
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

        public void SetUpNewRegionWith(SectorItem item)
        {
            //throws in a random number of SectorItem
        }

        //todo: call InitializeSectors() and pass empty list<sector>
        //todo: call InitializeSectors() and put in duplicate sectors
        //todo: test AddSector() item XY mismatch

        [Ignore("")]
        [Test]
        public void VerifyNumberingSystem()
        {
            //go through all Regions and verify that 1. it is numbered right, 2. that there is 1 of each.
        }

    }
}

//todo: size of List<Sector> this can be derived from the list.count above.
