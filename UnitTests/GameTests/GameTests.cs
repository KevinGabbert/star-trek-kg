
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using UnitTests.ShipTests;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class GameTests: TestClass_Base
    {
        private Quadrant _testQuadrant;

        [Test]
        public void NewGame()
        {
            //interesting.. this test causes MapWith3HostilesTheConfigWayAddedInDescendingOrder2() to fail when running after this one does

            var game = new Game((new StarTrekKGSettings()));
            
            Assert.IsInstanceOf<Map>(game.Map);

            (new StarTrekKGSettings()).Get = null;
        }

        [Test]
        public void EnemyTaunt()
        {
            _setup.SetupMapWith1Hostile();

            //_testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            Game.EnemiesWillNowTaunt();

        }

        [Test]
        public void EnemyFedTaunt()
        {
            _setup.SetupMapWith1FedHostile();

            //_testQuadrant = _setup.TestMap.Playership.GetQuadrant();

            Game.EnemiesWillNowTaunt();

        }

        [TearDown]
        public void TearDown()
        {
            
        }
    }
}
