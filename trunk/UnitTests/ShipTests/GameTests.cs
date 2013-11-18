using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Playfield;

namespace UnitTests.ShipTests
{
    public class GameTests
    {
        //ctor
        //Initialize
        //Run
        //PlayOnce

        [Test]
        public void MoveGameTimeForward()
        {
            var map = new Map();

            //apparently, the only requirement for this is that an observed movement needs to happen
            Game.MoveTimeForward(map, new Coordinate(0, 0), new Coordinate(0, 1));

            Assert.AreEqual(-1, map.timeRemaining);
            Assert.AreEqual(1, map.Stardate);
        }

        [Test]
        public void MoveGameTimeForward2()
        {
            var map = new Map();

            //apparently, the only requirement for this is that an observed movement needs to happen
            Game.MoveTimeForward(map, new Coordinate(0, 0), new Coordinate(0, 0));

            //Time has not moved
            Assert.AreEqual(0, map.timeRemaining);
            Assert.AreEqual(0, map.Stardate);
        }

        /// <summary>
        /// Tests code in context with surrounding code
        /// </summary>
        [Ignore]
        [Test]
        public void MoveGameTimeForward3()
        {
            //todo: test this with a full map, and ship set up.  Then tell ship to move.  
            var map = new Map();

            //MovementTests.Move_Quadrant("w", 1*8);

            //Time has moved
            Assert.AreEqual(0, map.timeRemaining);
            Assert.AreEqual(0, map.Stardate);
        }
    }
}
