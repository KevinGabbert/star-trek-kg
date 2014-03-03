using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.ShipTests.QuadrantTests
{
    public class NebulaTests: QuadrantTests_Base
    {
        [SetUp]
        public void Setup()
        {
            _testQuadrant = new Quadrant(this.Game.Map);

            _testQuadrant.Map = new Map(null, this.Game.Write, this.Game.Config);
            _testQuadrant.Name = "Setup";
            _testQuadrant.Scanned = false;
            _testQuadrant.Type = QuadrantType.Nebulae;

            _testQuadrant.X = 0;
            _testQuadrant.Y = 0;
        }

        //todo: These are going to require that output is read

        //todo: verify LRS returns "N N N"

        //todo: verify SRS returns random "+ - + -"
        //todo: verify SRS returns nearby objects resolving correctly
        //todo: verify SRS returns multiple echoes of baddies

        [Test]
        public void New()
        {
            //*************** sector not being created with new quadrant
            _testQuadrant = new Quadrant(this.Game.Map, true);

            Assert.IsInstanceOf<Map>(_testQuadrant.Map);
            
            this.QuadrantNewAsserts();

            Assert.AreEqual(QuadrantType.Nebulae, _testQuadrant.Type);
        }

        [Test]
        public void NewWithMap()
        {
            var baddieNames = new Stack<string>((new StarTrekKGSettings()).FactionShips(FactionName.Klingon));
            var quadrantNames = new Stack<string>((new StarTrekKGSettings()).GetStarSystems());

            _setup.SetupMapWith1Friendly();

            int nameIndex;
            _testQuadrant = new Quadrant(_setup.TestMap, quadrantNames, baddieNames, null, out nameIndex, false, true);

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testQuadrant.Map);
            Assert.AreEqual(QuadrantType.Nebulae, _testQuadrant.Type);

            Assert.AreEqual("Zeta Alpha", _testQuadrant.Name);
            Assert.IsInstanceOf<Sectors>(_testQuadrant.Sectors);
            Assert.AreEqual(false, _testQuadrant.Scanned);
            Assert.AreEqual(0, _testQuadrant.X);
            Assert.AreEqual(0, _testQuadrant.Y);
            Assert.AreEqual(true, _testQuadrant.Empty);
            Assert.IsNotNull(_testQuadrant.Sectors);
        }

        [Test]
        public void LRSOutputWithNebula()
        {
            const bool setupNebula = true;
            this.SetupQuadrant(setupNebula);

            var mockedWrite = new Mock<IOutputWrite>();
            _setup.Game.Write = mockedWrite.Object;

            _setup.TestLongRangeScan = new LongRangeScan(_setup.Game.Map.Playership, _setup.Game);

            _setup.TestLongRangeScan.Controls();

            mockedWrite.Verify(s => s.SingleLine("Long Range Scan inoperative while in Nebula."), Times.Exactly(1));
        }

        [Ignore]
        [Test]
        public void LRSOutputWithNNN()
        {
            //to fix this we might need to do the pattern in test_shipobject

            //const bool setupNebula = true;
            //this.SetupQuadrant(setupNebula);

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
            this.SetupQuadrant(setupNebula); 

            Assert.AreEqual(QuadrantType.Nebulae, _testQuadrant.Type);

            foreach (var sector in _testQuadrant.Sectors)
            {
                Assert.AreEqual(SectorType.Nebula, sector.Type, "Expected Nebula at Sector[" + sector.X + "," + sector.Y + "]");
            }

            this.VerifyScanResults(0);
        }

        [Test]
        public void LRSScanNonNebulaCountsOnly()
        {
            const bool setupNebula = false;
            this.SetupQuadrant(setupNebula); 

            Assert.AreEqual(QuadrantType.GalacticSpace, _testQuadrant.Type);

            foreach (var sector in _testQuadrant.Sectors)
            {
                Assert.AreEqual(SectorType.Space, sector.Type, "Expected Empty Space at Sector[" + sector.X + "," + sector.Y + "]");
            }

            this.VerifyScanResults(1);
        }

        private void VerifyScanResults(int expectedBaddies)
        {
            var hostilesReallyInQuadrant = _testQuadrant.GetHostiles().Count;
            Assert.AreEqual(1, hostilesReallyInQuadrant);

            _setup.TestLongRangeScan = new LongRangeScan(_setup.TestMap.Playership, _setup.Game);

            var x = _setup.TestLongRangeScan.Execute(_testQuadrant);
                //pulls count from Quadrant object

            Assert.AreEqual(expectedBaddies, x.Hostiles);
        }

        private void SetupQuadrant(bool withNebula)
        {
            _setup.SetupMapWith1Hostile();

            //Assert.AreEqual(1, _setup.TestMap.Quadrants.GetActive());

            _testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            if (withNebula)
            {
                _testQuadrant.TransformIntoNebulae();
            }
        }
    }
}
