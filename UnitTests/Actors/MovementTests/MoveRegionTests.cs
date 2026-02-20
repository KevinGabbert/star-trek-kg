using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace UnitTests.Actors.MovementTests.MoveRegionTests
{
    public class MoveRegionTests : Movement_Base
    {
        #region Sectors

        #region Verify sector obstacles are missed when travelling at higher than sublight speeds
        [Test]
        public void MoveRegion_MissObstacleSouth()
        {
            //This is the star that would be hit without fix
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            this.Game.Map.Sectors.GetActive().Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            this.Game.Map.Sectors.GetActive().Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 5].Item = CoordinateItem.Star; //northeast

            this.Move_Region("7", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");

            //revert for next test
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Empty;
        }

        [Test]
        public void MoveRegion_MissObstacleNorth()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            this.Game.Map.Sectors.GetActive().Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Star; //to the south
            this.Game.Map.Sectors.GetActive().Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            this.Game.Map.Sectors.GetActive().Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 5].Item = CoordinateItem.Star; //northeast

            this.Move_Region("3", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleWest()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            this.Game.Map.Sectors.GetActive().Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Star; //to the south
            this.Game.Map.Sectors.GetActive().Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            this.Game.Map.Sectors.GetActive().Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 5].Item = CoordinateItem.Star; //northeast

            this.Move_Region("5", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleEast()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            this.Game.Map.Sectors.GetActive().Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Star; //to the south
            this.Game.Map.Sectors.GetActive().Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            this.Game.Map.Sectors.GetActive().Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 5].Item = CoordinateItem.Star; //northeast

            this.Move_Region("1", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleSouthWest()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            this.Game.Map.Sectors.GetActive().Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            this.Game.Map.Sectors.GetActive().Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            this.Game.Map.Sectors.GetActive().Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            this.Game.Map.Sectors.GetActive().Coordinates[3, 5].Item = CoordinateItem.Star; //northeast
            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            this.Game.Map.Sectors.GetActive().Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Move_Region("6", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleSouthEast()
        {
            //friendly ship is on 4,4

            //strange.  Going this direction, you actually don't get the "obstacle error" verified by the other "missobstacle" tests
            this.Game.Map.Sectors.GetActive().Coordinates[3, 3].Item = CoordinateItem.Star; //northwest           
            this.Game.Map.Sectors.GetActive().Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            this.Game.Map.Sectors.GetActive().Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            this.Game.Map.Sectors.GetActive().Coordinates[3, 5].Item = CoordinateItem.Star; //northeast
            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            this.Game.Map.Sectors.GetActive().Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Move_Region("4", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleNorthEast()
        {
            //friendly ship is on 4,4

            //This is the obstacle that gets hit
            this.Game.Map.Sectors.GetActive().Coordinates[3, 5].Item = CoordinateItem.Star; //northeast
            this.Game.Map.Sectors.GetActive().Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            this.Game.Map.Sectors.GetActive().Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            this.Game.Map.Sectors.GetActive().Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            this.Game.Map.Sectors.GetActive().Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Move_Region("2", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleNorthWest()
        {
            //friendly ship is on 4,4

            //strange.  Going this direction, you actually don't get the "obstacle error" verified by the other "missobstacle" tests
            this.Game.Map.Sectors.GetActive().Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            this.Game.Map.Sectors.GetActive().Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            this.Game.Map.Sectors.GetActive().Coordinates[5, 3].Item = CoordinateItem.Star; //southwest       
            this.Game.Map.Sectors.GetActive().Coordinates[3, 5].Item = CoordinateItem.Star; //northeast
            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            this.Game.Map.Sectors.GetActive().Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Move_Region("8", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        #endregion

        [Test]
        public void MoveRegion_East()
        {
            //todo: fails from hitting galactic barrier.  map init error??

            this.Move_Region(((int)NavDirection.Right).ToString(), 1);

            //TODO: verify that "Friendly" has been set back down on map after movement (because console app is showing
            //TODO: it dissappearing.) -- verify sector

            //TODO: function call to verify that it shows up in the correct place in output (separate test harness)
            //todo: moving forward .1 from the previous sector pops you in the middle of the next sector
            this.CheckRegionsAfterMovement(true);

            var newRegion = this.Game.Map.Playership.GetSector();

            Assert.AreEqual(_startingRegion.X, newRegion.X - 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, newRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void MoveRegion_SouthEast()
        {
            this.Move_Region(((int)NavDirection.RightDown).ToString(), 1);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X - 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y - 1, "(c)startingRegionY");

            //verify that ship has not changed its sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveRegion_South()
        {
            this.Move_Region(((int)NavDirection.Down).ToString(), 1);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y - 1, "(c)startingRegionY");
        }

        [Test]
        public void MoveRegion_SouthWest()
        {
            this.Move_Region(((int)NavDirection.LeftDown).ToString(), 1);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetSector();

            //todo: why is this +- 2?
            Assert.AreEqual(_startingRegion.X, playershipRegion.X + 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y - 1, "(c)startingRegionY");

            //verify that ship has not changed its sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveRegion_West()
        {
            //todo: this test does not work when run by itself

            Assert.AreEqual(4, _startingRegion.X);
            Assert.AreEqual(4, _startingRegion.Y);

            this.Move_Region(((int)NavDirection.Left).ToString(), 1);

            Assert.AreEqual(4, _startingRegion.X);
            Assert.AreEqual(4, _startingRegion.Y);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X + 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void MoveRegion_NorthWest()
        {
            this.Move_Region(((int)NavDirection.LeftUp).ToString(), 1);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X + 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y + 1, "(c)startingRegionY");

            //verify that ship has not changed its sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveRegion_North()
        {
            this.Move_Region(((int)NavDirection.Up).ToString(), 1);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y + 1, "(c)startingRegionY");
        }

        [Test]
        public void MoveRegion_NorthEast()
        {
            this.Move_Region(((int)NavDirection.RightUp).ToString(), 1);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X - 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y + 1, "(c)startingRegionY");

            //verify that ship has not changed its sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Ignore("")]
        [Test]
        public void MoveRegion_AllDistances()
        {
            for (int direction = 1; direction < 8; direction++)
            {
                for (double i = 1; i < 8; i++)
                {
                    reset();
                    //this.Move_Region(direction, i*8);

                    //todo: for this to be the best test, then the ship would need to move back to its
                    //starting location so it can move the max distance again.
                    //without reset() this test breaks another test.
                    //I wonder if it is becuase when ship hits the edge of the galaxy, (current functionality
                    //negates the last sector of movement), the ship keeps getting knocked back 1 sector location

                    //CheckAfterMovement();
                }
            }
        }

        [Test]
        public void HitGalacticBarrierNorth()
        {
            this.Move_Region(((int)NavDirection.Up).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckRegionsAfterMovement(false);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(0, playershipRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void HitGalacticBarrierSouth()
        {
            this.Move_Region(((int)NavDirection.Down).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckRegionsAfterMovement(false);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(7, playershipRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void HitGalacticBarrierEast()
        {
            this.Move_Region(((int)NavDirection.Right).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckRegionsAfterMovement(false);

            var endingRegion = this.Game.Map.Playership.GetSector();

            Assert.AreEqual(7, endingRegion.X, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, endingRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void HitGalacticBarrierWest()
        {
            this.Move_Region(((int)NavDirection.Left).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckRegionsAfterMovement(false);

            var playershipRegion = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(0, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y, "(c)startingRegionY");
        }

        #endregion

        protected void CheckRegionsAfterMovement(bool checkForGalacticBarrierHit)
        {
            Sector playershipRegion = this.Game.Map.Playership.GetSector();
            Sector activeRegion = this.Game.Map.Sectors.GetActive();

            Assert.AreEqual(64, this.Game.Map.Sectors.Count); //I'd certainly hope that this hasnt changed..
            Assert.IsFalse(_testMovement.BlockedByObstacle, "Blocked by Obstacle");

            if (checkForGalacticBarrierHit)
            {
                Assert.IsFalse(_testMovement.BlockedByGalacticBarrier, "Blocked by Galactic Barrier");
            }

            //ensure there is still only 1 of original Sector (used to address a bug early on)
            Assert.IsInstanceOf<Sector>(playershipRegion);

            //ship is in active sector
            Coordinate found = activeRegion.Coordinates[
                                    this.Game.Map.Playership.Coordinate.X,
                                    this.Game.Map.Playership.Coordinate.Y];

            Assert.IsInstanceOf<Coordinate>(found);

            //starting location is empty
            Sector startingRegionT = this.Game.Map.Sectors[_startingRegion];
            Assert.AreEqual(CoordinateItem.Empty, startingRegionT.Coordinates[_startingSectorX, _startingSectorY].Item);

            //We moved from our original Sector, right?
            Assert.AreNotEqual(startingRegionT.X.ToString() + startingRegionT.Y, activeRegion.X +
                                                                                            activeRegion.Y.ToString(), "Starting Sector");

            //Friendly was set in new location
            //Playership current sector has the ship set in it 
            Assert.AreEqual(CoordinateItem.PlayerShip, playershipRegion.Coordinates[this.Game.Map.Playership.Coordinate.X, this.Game.Map.Playership.Coordinate.Y].Item);

            //is ship in expected location in new Sector?
            ////indirectly..
            int found2 = playershipRegion.Coordinates.Count(s => s.Item == CoordinateItem.PlayerShip);
            Assert.AreEqual(1, found2, "expected to find 1 friendly, not " + found2 + ".   ");

            //directly
            //Verifying Coordinate. Look up sector by playership's coordinates. see if a friendly is there.
            Assert.AreEqual(CoordinateItem.PlayerShip, playershipRegion.Coordinates.Single(s => s.X == this.Game.Map.Playership.Coordinate.X &&
                                                                                                                s.Y == this.Game.Map.Playership.Coordinate.Y).Item);

            //Check Ship Sector against active. (this really just tests the GetActive() function - this should be a separate test as well)
            Assert.AreEqual(this.Game.Map.Playership.Point.X, activeRegion.X, "this.Game.Map.Playership.Sector.X");
            Assert.AreEqual(this.Game.Map.Playership.Point.Y, activeRegion.Y, "this.Game.Map.Playership.Sector.Y");
        }
    }
}
