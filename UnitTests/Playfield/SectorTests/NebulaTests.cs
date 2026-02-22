using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Playfield.SectorTests
{
    public class NebulaTests: SectorTests_Base
    {
        [SetUp]
        public void Setup()
        {
            _testSector =
                new Sector(this.Game.Map)
                {
                    Map = new Map(null, this.Game.Interact, this.Game.Config, this.Game),
                    Name = "Setup",
                    Scanned = false,
                    Type = SectorType.Nebulae,
                    X = 0,
                    Y = 0
                };


        }

        //todo: These are going to require that output is read

        //todo: verify LRS returns "N N N"

        //todo: verify SRS returns random "+ - + -"
        //todo: verify SRS returns nearby objects resolving correctly
        //todo: verify SRS returns multiple echoes of baddies

        [Test]
        public void New()
        {
            //*************** sector not being created with new Sector
            _testSector = new Sector(this.Game.Map, true);

            Assert.IsInstanceOf<Map>(_testSector.Map);
            
            this.SectorNewAsserts();

            Assert.AreEqual(SectorType.Nebulae, _testSector.Type);
        }

        [Test]
        public void NewWithMap()
        {
            var baddieNames = new Stack<string>(new StarTrekKGSettings().ShipNames(FactionName.Klingon));
            var SectorNames = new Stack<string>(new StarTrekKGSettings().GetStarSystems());

            _setup.SetupMapWith1Friendly();

            int nameIndex;
            _testSector = new Sector(_setup.TestMap, SectorNames, baddieNames, null, out nameIndex, false, true);

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testSector.Map);
            Assert.AreEqual(SectorType.Nebulae, _testSector.Type);

            Assert.AreEqual("Zeta Alpha", _testSector.Name);
            Assert.IsInstanceOf<Coordinates>(_testSector.Coordinates);
            Assert.AreEqual(false, _testSector.Scanned);
            Assert.AreEqual(0, _testSector.X);
            Assert.AreEqual(0, _testSector.Y);
            Assert.AreEqual(true, _testSector.Empty);
            Assert.IsNotNull(_testSector.Coordinates);
        }

        [Test]
        public void LRSOutputWithNebula()
        {
            const bool setupNebula = true;
            this.SetupSector(setupNebula);

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
            //this.SetupSector(setupNebula);

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
            this.SetupSector(setupNebula); 

            Assert.AreEqual(SectorType.Nebulae, _testSector.Type);

            foreach (var sector in _testSector.Coordinates)
            {
                Assert.AreEqual(CoordinateType.Nebula, sector.Type, "Expected Nebula at Coordinate[" + sector.X + "," + sector.Y + "]");
            }

            //expected baddies are zero because LRS can't see how many are in a nebula. it interferes with sensors.
            this.VerifyScanResults(null);
        }

        [Test]
        public void LRSScanNonNebulaCountsOnly()
        {
            const bool setupNebula = false;
            this.SetupSector(setupNebula); 

            Assert.AreEqual(SectorType.GalacticSpace, _testSector.Type);

            foreach (var sector in _testSector.Coordinates)
            {
                Assert.AreEqual(CoordinateType.Space, sector.Type, "Expected Empty Space at Coordinate[" + sector.X + "," + sector.Y + "]");
            }

            this.VerifyScanResults(1);
        }

        private void VerifyScanResults(int? expectedBaddies)
        {
            var hostilesReallyInSector = _testSector.GetHostiles().Count;
            Assert.AreEqual(1, hostilesReallyInSector);

            _setup.TestLongRangeScan = new LongRangeScan(_setup.TestMap.Playership);

            LRSResult scanResult = LongRangeScan.Execute(_testSector);

            Assert.AreEqual(expectedBaddies, scanResult.Hostiles);
        }

        private void SetupSector(bool withNebula)
        {
            _setup.SetupMapWith1Hostile();

            //Assert.AreEqual(1, _setup.TestMap.Sectors.GetActive());

            _testSector = _setup.TestMap.Playership.GetSector();

            if (withNebula)
            {
                _testSector.TransformIntoNebulae();
            }
        }
    }
}
