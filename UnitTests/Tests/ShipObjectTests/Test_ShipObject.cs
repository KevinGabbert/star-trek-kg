using Moq;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.ShipTests.ShipObjectTests
{
    [TestFixture]
    public class Test_ShipObject
    {
        private Mock<IGame> _mockGame;
        private Mock<IMap> _mockMap;
        private Mock<IInteraction> _mockWrite;
        private Mock<ISector> _mockSector;
        private Mock<IStarTrekKGSettings> _mockSettings;
        private Mock<ICoordinate> _mockCoordinate;

        [SetUp]
        public void Setup()
        {
            _mockGame = new Mock<IGame>();
            _mockMap = new Mock<IMap>();
            _mockWrite = new Mock<IInteraction>();
            _mockSector = new Mock<ISector>();
            _mockSettings = new Mock<IStarTrekKGSettings>();
            _mockCoordinate = new Mock<ICoordinate>();
            _mockMap.Setup(m => m.Regions).Returns(new Regions(_mockMap.Object, _mockWrite.Object));

            var regions = new Regions(_mockMap.Object, _mockWrite.Object)
            {
                new Region(new Coordinate())
            };

            _mockGame.Setup(m => m.Interact).Returns(_mockWrite.Object);

            _mockMap.Setup(m => m.Game).Returns(_mockGame.Object);
            _mockMap.Setup(m => m.Regions).Returns(regions);
            _mockMap.Setup(m => m.Write).Returns(_mockWrite.Object);

            _mockSector.Setup(c => c.RegionDef).Returns(new Coordinate());

            //todo: write needs an interact set up
            _mockWrite.Setup(m => m.ShipHitMessage(It.IsAny<IShip>(), It.IsAny<int>()));

            _mockMap.Setup(m => m.Config).Returns(_mockSettings.Object);
        }

        [Test]
        public void Basic_Instantiation()
        {
            _mockSettings.Setup(u => u.GetSetting<string>("Hostile")).Returns("GoodGuy");

            var shipToTest = new Ship(FactionName.Klingon, "TestShip", _mockSector.Object, _mockMap.Object, _mockMap.Object.Game);

            Assert.IsInstanceOf<Ship>(shipToTest);
            Assert.AreEqual(Allegiance.GoodGuy, shipToTest.Allegiance);
        }

        [Test]
        public void Basic_Instantiation32()
        {
            var shipToTest = new Ship(FactionName.Klingon, "TestShip", _mockSector.Object, _mockMap.Object, _mockMap.Object.Game)
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

            var shipToTest = new Ship(FactionName.Klingon, "TestShip", _mockSector.Object, _mockMap.Object, _mockMap.Object.Game);

            Assert.IsInstanceOf<Ship>(shipToTest);
            Assert.AreEqual(Allegiance.Indeterminate, shipToTest.Allegiance);
        }

        [Test]
        public void Test_AbsorbHit_NoDamage()
        {
            _mockSettings.Setup(u => u.GetSetting<string>("Hostile")).Returns("blah Blah Blah!!!");

            var shipToTest = new Ship(FactionName.Klingon, "TestShip", _mockSector.Object, _mockMap.Object, _mockMap.Object.Game);

            Shields.For(shipToTest).Energy = 100;  //this is syntactic sugar for: ship.Subsystems.Single(s => s.Type == SubsystemType.Shields);

            Assert.AreEqual(100, Shields.For(shipToTest).Energy);

            _mockWrite.Setup(w => w.Line(It.IsAny<string>()));

            _mockMap.Object.Write = _mockWrite.Object;
            _mockSector.Setup(s => s.X).Returns(-2);
            _mockSector.Setup(s => s.Y).Returns(-3);

            var attacker = new Ship(FactionName.Klingon, "The attacking Ship", _mockSector.Object, _mockMap.Object, _mockMap.Object.Game);
            shipToTest.AbsorbHitFrom(attacker, 50);

            Assert.AreEqual(50, Shields.For(shipToTest).Energy);

            //Verifications of Output to User
            _mockWrite.Verify(i => i.ShipHitMessage(It.IsAny<IShip>(), It.IsAny<int>()), Times.Exactly(1));
            _mockWrite.Verify(i => i.ConfigText("NoDamage"), Times.Exactly(1));
        }

        [Ignore]
        [Test]
        public void Test_AbsorbHit_SubsystemDamaged()
        {

        }

        [Ignore]
        [Test]
        public void Test_AbsorbHit_ShipDEstroyed_NoEnergy()
        {

        }

        [Ignore]
        [Test]
        public void Test_AbsorbHit_ShipDestroyed_AllSubsystemsDestroyed()
        {

        }

        //GetRegion()
        //GetLocation()
        //RepairEverything()
    }
}
