using Moq;
using NUnit.Framework;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace UnitTests.Actors.ShipObjectTests
{
    [TestFixture]
    public class SubsystemSetup
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

        [Ignore("")]
        [Test]
        public void Basic_Instantiation_Verifying_Subsystems()
        {
            //_mockSettings.Setup(u => u.GetSetting<string>("Hostile")).Returns("GoodGuy");

            //var shipToTest = new Ship("TestShip", _mockSector.Object, _mockMap.Object, _mockSettings.Object);

            //Assert.IsInstanceOf<Ship>(shipToTest);
            //Assert.AreEqual(Allegiance.GoodGuy, shipToTest.Allegiance);
        }
    }
}

//mockMap.Setup(u => u.(123)).Returns(1);
