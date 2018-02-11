using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.ShipTests.RegionTests
{
    public class NebulaTests: RegionTests_Base
    {
        [SetUp]
        public void Setup()
        {
            _testRegion = new Region(this.Game.Map);

            _testRegion.Map = new Map(null, this.Game.Interact, this.Game.Config, this.Game);
            _testRegion.Name = "Setup";
            _testRegion.Scanned = false;
            _testRegion.Type = RegionType.Nebulae;

            _testRegion.X = 0;
            _testRegion.Y = 0;
        }

        //todo: These are going to require that output is read

        //todo: verify LRS returns "N N N"

        //todo: verify SRS returns random "+ - + -"
        //todo: verify SRS returns nearby objects resolving correctly
        //todo: verify SRS returns multiple echoes of baddies

        [Test]
        public void New()
        {
            //*************** sector not being created with new Region
            _testRegion = new Region(this.Game.Map, true);

            Assert.IsInstanceOf<Map>(_testRegion.Map);
            
            this.RegionNewAsserts();

            Assert.AreEqual(RegionType.Nebulae, _testRegion.Type);
        }

        [Test]
        public void NewWithMap()
        {
            var baddieNames = new Stack<string>(new StarTrekKGSettings().ShipNames(FactionName.Klingon));
            var RegionNames = new Stack<string>(new StarTrekKGSettings().GetStarSystems());

            _setup.SetupMapWith1Friendly();

            int nameIndex;
            _testRegion = new Region(_setup.TestMap, RegionNames, baddieNames, null, out nameIndex, false, true);

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testRegion.Map);
            Assert.AreEqual(RegionType.Nebulae, _testRegion.Type);

            Assert.AreEqual("Zeta Alpha", _testRegion.Name);
            Assert.IsInstanceOf<Sectors>(_testRegion.Sectors);
            Assert.AreEqual(false, _testRegion.Scanned);
            Assert.AreEqual(0, _testRegion.X);
            Assert.AreEqual(0, _testRegion.Y);
            Assert.AreEqual(true, _testRegion.Empty);
            Assert.IsNotNull(_testRegion.Sectors);
        }

        [Test]
        public void LRSOutputWithNebula()
        {
            const bool setupNebula = true;
            this.SetupRegion(setupNebula);

            _setup.TestLongRangeScan = new LongRangeScan(_setup.Game.Map.Playership);

            _setup.TestLongRangeScan.Controls();

            ////Verifications of Output to User
            //_mockWrite.Verify(i => i.ShipHitMessage(It.IsAny<IShip>(), It.IsAny<int>()), Times.Exactly(1));
            //_mockWrite.Verify(i => i.ConfigText("NoDamage"), Times.Exactly(1));

            //mockedWrite.Verify(s => s.SingleLine("Long Range Scan inoperative while in Nebula."), Times.Exactly(1));
        }

        [Ignore("")]
        [Test]
        public void LRSOutputWithNNN()
        {
            //to fix this we might need to do the pattern in test_shipobject

            //const bool setupNebula = true;
            //this.SetupRegion(setupNebula);

            //_setup.TestLongRangeScan = new LongRangeScan(_setup.Game.Map.Playership, _setup.Game);

            //var mockedWrite = new Mock<IOutputWrite>();
            //_setup.Game.Write = mockedWrite.Object;

            //_setup.TestLongRangeScan.Controls();

            //mockedWrite.Verify(s => s.RenderNebula(It.IsAny<bool>()), Times.Exactly(1));
        }

        [Test]
        public void LRSScanNebulaCountsOnly()
        {
            const bool setupNebula = true;
            this.SetupRegion(setupNebula); 

            Assert.AreEqual(RegionType.Nebulae, _testRegion.Type);

            foreach (var sector in _testRegion.Sectors)
            {
                Assert.AreEqual(SectorType.Nebula, sector.Type, "Expected Nebula at Sector[" + sector.X + "," + sector.Y + "]");
            }

            this.VerifyScanResults(null);
        }

        [Test]
        public void LRSScanNonNebulaCountsOnly()
        {
            const bool setupNebula = false;
            this.SetupRegion(setupNebula); 

            Assert.AreEqual(RegionType.GalacticSpace, _testRegion.Type);

            foreach (var sector in _testRegion.Sectors)
            {
                Assert.AreEqual(SectorType.Space, sector.Type, "Expected Empty Space at Sector[" + sector.X + "," + sector.Y + "]");
            }

            this.VerifyScanResults(1);
        }

        private void VerifyScanResults(int? expectedBaddies)
        {
            var hostilesReallyInRegion = _testRegion.GetHostiles().Count;
            Assert.AreEqual(1, hostilesReallyInRegion);

            _setup.TestLongRangeScan = new LongRangeScan(_setup.TestMap.Playership);

            LRSResult scanResult = LongRangeScan.Execute(_testRegion);

            Assert.AreEqual(expectedBaddies, scanResult.Hostiles);
        }

        private void SetupRegion(bool withNebula)
        {
            _setup.SetupMapWith1Hostile();

            //Assert.AreEqual(1, _setup.TestMap.Regions.GetActive());

            _testRegion = _setup.TestMap.Playership.GetRegion();

            if (withNebula)
            {
                _testRegion.TransformIntoNebulae();
            }
        }
    }
}
