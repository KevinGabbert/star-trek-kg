using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Playfield;
using UnitTests.TestObjects;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class GameTests: TestClass_Base
    {
        [Test]
        public void NewGame()
        {
            //interesting.. this test causes MapWith3HostilesTheConfigWayAddedInDescendingOrder2() to fail when running after this one does

            var game = new Game(new StarTrekKGSettings());
            
            Assert.IsInstanceOf<Map>(game.Map);

            new StarTrekKGSettings().Get = null;
        }

        [Test]
        public void EnemyTaunt()
        {
            _setup.SetupMapWith1Hostile();

            //_testSector = _setup.TestMap.Playership.GetSector();

            Game.EnemiesWillNowTaunt();
        }

        [Test]
        public void EnemyFedTaunt()
        {
            _setup.SetupMapWith1FedHostile();

            //_testSector = _setup.TestMap.Playership.GetSector();

            Game.EnemiesWillNowTaunt();

        }

        [TearDown]
        public void TearDown()
        {
            
        }

        //ctor
        //Initialize
        //Run
        //PlayOnce

        [Test]
        public void MoveGameTimeForward()
        {
            var map = new Map
            {
                Game = new Game(new StarTrekKGSettings())
            };

            //apparently, the only requirement for this is that an observed movement needs to happen
            map.Game.MoveTimeForward(map, new Point(0, 0), new Point(0, 1));

            Assert.AreEqual(-1, map.timeRemaining);
            Assert.AreEqual(1, map.Stardate);
        }

        [Test]
        public void MoveGameTimeForward2()
        {
            var map = new Map
            {
                Game = new Game(new StarTrekKGSettings())
            };

            //apparently, the only requirement for this is that an observed movement needs to happen
            map.Game.MoveTimeForward(map, new Point(0, 0), new Point(0, 0));

            //Time has not moved
            Assert.AreEqual(0, map.timeRemaining);
            Assert.AreEqual(0, map.Stardate);
        }

        /// <summary>
        /// Tests code in context with surrounding code
        /// </summary>
        [Ignore("")]
        [Test]
        public void MoveGameTimeForward3()
        {
            //todo: test this with a full map, and ship set up.  Then tell ship to move.  
            var map = new Map
            {
                Game = new Game(new StarTrekKGSettings())
            };

            //MovementTests.Move_Sector("w", 1*8);

            //Time has moved
            Assert.AreEqual(0, map.timeRemaining);
            Assert.AreEqual(0, map.Stardate);
        }
    }
}
