using Moq;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace UnitTests.ShipTests.ShipObjectTests
{
    [TestFixture]
    public class Test_ShipObject
    {
        private Mock<Sector> _mockSector;
        private Mock<Map> _mockMap;
        private Mock<IStarTrekKGSettings> _mockSettings;

        [SetUp]
        public void Setup()
        {
            _mockSector = new Mock<Sector>();
            _mockMap = new Mock<Map>();
            _mockSettings = new Mock<IStarTrekKGSettings>();
        }

        [Test]
        public void Basic_Instantiation()
        {
            _mockSettings.Setup(u => u.GetSetting<string>("Hostile")).Returns("GoodGuy");

            var shipToTest = new Ship("TestShip", _mockSector.Object, _mockMap.Object, _mockSettings.Object);

            Assert.IsInstanceOf<Ship>(shipToTest);
            Assert.AreEqual(Allegiance.GoodGuy, shipToTest.Allegiance);
        }

        [Test]
        public void Basic_Instantiation32()
        {
            var shipToTest = new Ship("TestShip", _mockSector.Object, _mockMap.Object, _mockSettings.Object)
            {
                Allegiance = Allegiance.GoodGuy
            };

            Assert.IsInstanceOf<Ship>(shipToTest);
            Assert.AreEqual(Allegiance.GoodGuy, shipToTest.Allegiance);
        }

        [Test]
        public void Basic_InstantiationUnknownAllegiance()
        {
            _mockSettings.Setup(u => u.GetSetting<string>("Hostile")).Returns("blah Blah Blah!!!");

            var shipToTest = new Ship("TestShip", _mockSector.Object, _mockMap.Object, _mockSettings.Object);

            Assert.IsInstanceOf<Ship>(shipToTest);
            Assert.AreEqual(Allegiance.Indeterminate, shipToTest.Allegiance);
        }


        //AbsorbHitFrom(IShip attacker, int attackingEnergy)
        //GetQuadrant()
        //GetLocation()
        //RepairEverything()
    }
}
