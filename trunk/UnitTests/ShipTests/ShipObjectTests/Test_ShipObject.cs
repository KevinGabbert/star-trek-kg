﻿using Moq;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.ShipObjectTests
{
    [TestFixture]
    public class Test_ShipObject
    {
        private Mock<Map> _mockMap;
        private Mock<IOutputWrite> _mockWrite;
        private Mock<ISector> _mockSector;
        private Mock<IStarTrekKGSettings> _mockSettings;

        [SetUp]
        public void Setup()
        {
            _mockMap = new Mock<Map>();
            _mockWrite = new Mock<IOutputWrite>();
            _mockSector = new Mock<ISector>();
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

        [Test]
        public void Test_AbsorbHit_NoDamage()
        {
            _mockSettings.Setup(u => u.GetSetting<string>("Hostile")).Returns("blah Blah Blah!!!");

            var shipToTest = new Ship("TestShip", _mockSector.Object, _mockMap.Object, _mockSettings.Object);
            Shields.For(shipToTest).Energy = 100;  //this is syntactic sugar for: ship.Subsystems.Single(s => s.Type == SubsystemType.Shields);

            Assert.AreEqual(100, Shields.For(shipToTest).Energy);

            _mockWrite.Setup(w => w.Line(It.IsAny<string>()));

            _mockMap.Object.Write = _mockWrite.Object;
            _mockSector.Setup(s => s.X).Returns(-2);
            _mockSector.Setup(s => s.Y).Returns(-3);

            var attacker = new Ship("The attacking Ship", _mockSector.Object, _mockMap.Object, _mockSettings.Object);
            shipToTest.AbsorbHitFrom(attacker, 50);

            Assert.AreEqual(50, Shields.For(shipToTest).Energy);

            //Verifications of Output to User

            _mockSector.Verify(s => s.X, Times.Exactly(1));
            _mockSector.Verify(s => s.Y, Times.Exactly(1));

            _mockWrite.Verify(w => w.Line(It.IsAny<string>()), Times.Exactly(2));
            _mockWrite.Verify(w => w.Line("TestShip hit by The attacking Ship at sector [-2,-3].... "), Times.AtLeastOnce());
            _mockWrite.Verify(w => w.Line("No Structural Damage from hit."), Times.Once());
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

        //GetQuadrant()
        //GetLocation()
        //RepairEverything()
    }
}