using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.QuadrantTests
{
    public class NebulaTests: TestClass_Base
    {
        Quadrant _testQuadrant;

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

        //todo: verify LRS returns "N N N"
        //todo: verify SRS returns random "+ - + -"
        //todo: verify SRS returns nearby objects resolving correctly
        //todo: verify SRS returns multiple echoes of baddies

        [Test]
        public void New()
        {
            //*************** sector not being created with new quadrant
            _testQuadrant = new Quadrant(this.Game.Map);

            Assert.IsInstanceOf<Map>(_testQuadrant.Map);
            this.QuadrantNewAsserts();
            Assert.IsNull(_testQuadrant.Sectors);
        }

        [Test]
        public void NewWithMap()
        {
            var baddieNames = new Stack<string>((new StarTrekKGSettings()).GetShips("Klingon"));

            _setup.SetupMapWith1Friendly();

            int nameIndex;
            _testQuadrant = new Quadrant(this.Game.Map, baddieNames, out nameIndex, true);

            //todo: make sure that map is not set up with anyting

            Assert.IsInstanceOf(typeof(Map), _testQuadrant.Map);

            Assert.AreEqual(string.Empty, _testQuadrant.Name);
            Assert.IsInstanceOf<Sectors>(_testQuadrant.Sectors);
            Assert.AreEqual(false, _testQuadrant.Scanned);
            Assert.AreEqual(0, _testQuadrant.X);
            Assert.AreEqual(0, _testQuadrant.Y);
            Assert.AreEqual(true, _testQuadrant.Empty);
            Assert.AreEqual(QuadrantType.Nebulae, _testQuadrant.Type);

            Assert.IsNotNull(_testQuadrant.Sectors);
        }

        private void QuadrantNewAsserts()
        {
            Assert.AreEqual(string.Empty, _testQuadrant.Name);
            Assert.IsNull(_testQuadrant.Sectors);
            Assert.AreEqual(false, _testQuadrant.Scanned);
            Assert.AreEqual(0, _testQuadrant.X);
            Assert.AreEqual(0, _testQuadrant.Y);
            Assert.AreEqual(true, _testQuadrant.Empty);
            Assert.AreEqual(QuadrantType.Nebulae, _testQuadrant.Type);

            Assert.AreEqual(0, _testQuadrant.GetHostiles().Count);
        }
    }
}
