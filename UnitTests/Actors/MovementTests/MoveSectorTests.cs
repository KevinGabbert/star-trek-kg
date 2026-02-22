using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace UnitTests.Actors.MovementTests.MoveSectorTests
{
    public class MoveSectorTests : Movement_Base
    {
        #region Sectors

        //todo: these obstacle tests only need one star set up. the multiple set up makes them ambiguous

        #region Verify sector obstacles are missed when travelling at higher than sublight speeds
        [Test]
        public void MoveSector_MissObstacleSouth()
        {
            //This is the star that would be hit without fix

            Sector activeSector = this.Game.Map.Sectors.GetActive();
            activeSector.Coordinates[5, 4].Item = CoordinateItem.Star; //to the south
            activeSector.Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            activeSector.Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            activeSector.Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            activeSector.Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            activeSector.Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            activeSector.Coordinates[3, 5].Item = CoordinateItem.Star; //northeast

            this.Move_Coordinate("1", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");

            //revert for next test
            this.Game.Map.Sectors.GetActive().Coordinates[5, 4].Item = CoordinateItem.Empty;
        }

        [Test]
        public void MoveSector_MissObstacleNorth()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector activeSector = this.Game.Map.Sectors.GetActive();
            activeSector.Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            activeSector.Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            activeSector.Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            activeSector.Coordinates[5, 4].Item = CoordinateItem.Star; //to the south
            activeSector.Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            activeSector.Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            activeSector.Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            activeSector.Coordinates[3, 5].Item = CoordinateItem.Star; //northeast

            this.Move_Coordinate("5", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveSector_MissObstacleWest()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector activeSector = this.Game.Map.Sectors.GetActive();
            activeSector.Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            activeSector.Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            activeSector.Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            activeSector.Coordinates[5, 4].Item = CoordinateItem.Star; //to the south
            activeSector.Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            activeSector.Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            activeSector.Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            activeSector.Coordinates[3, 5].Item = CoordinateItem.Star; //northeast

            this.Move_Coordinate("3", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveSector_MissObstacleEast()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector activeSector = this.Game.Map.Sectors.GetActive();
            activeSector.Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            activeSector.Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            activeSector.Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            activeSector.Coordinates[5, 4].Item = CoordinateItem.Star; //to the south
            activeSector.Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            activeSector.Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            activeSector.Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            activeSector.Coordinates[3, 5].Item = CoordinateItem.Star; //northeast

            this.Move_Coordinate("7", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveSector_MissObstacleSouthWest()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector activeSector = this.Game.Map.Sectors.GetActive();
            activeSector.Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            activeSector.Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            activeSector.Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            activeSector.Coordinates[3, 5].Item = CoordinateItem.Star; //northeast
            activeSector.Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            activeSector.Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            activeSector.Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            activeSector.Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Move_Coordinate("2", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveSector_MissObstacleSouthEast()
        {
            //friendly ship is on 4,4

            //strange.  Going this direction, you actually don't get the "obstacle error" verified by the other "missobstacle" tests
            Sector activeSector = this.Game.Map.Sectors.GetActive();
            activeSector.Coordinates[3, 3].Item = CoordinateItem.Star; //northwest           
            activeSector.Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            activeSector.Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            activeSector.Coordinates[3, 5].Item = CoordinateItem.Star; //northeast
            activeSector.Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            activeSector.Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            activeSector.Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            activeSector.Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Move_Coordinate("8", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveSector_MissObstacleNorthEast()
        {
            //friendly ship is on 4,4

            //This is the obstacle that gets hit
            Sector activeSector = this.Game.Map.Sectors.GetActive();
            activeSector.Coordinates[3, 5].Item = CoordinateItem.Star; //northeast
            activeSector.Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            activeSector.Coordinates[5, 3].Item = CoordinateItem.Star; //southwest
            activeSector.Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            activeSector.Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            activeSector.Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            activeSector.Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            activeSector.Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Move_Coordinate("6", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveSector_MissObstacleNorthWest()
        {
            //friendly ship is on 4,4

            //strange.  Going this direction, you actually don't get the "obstacle error" verified by the other "missobstacle" tests
            Sector activeSector = this.Game.Map.Sectors.GetActive();
            activeSector.Coordinates[5, 5].Item = CoordinateItem.Star; //southeast
            activeSector.Coordinates[3, 3].Item = CoordinateItem.Star; //northwest
            activeSector.Coordinates[5, 3].Item = CoordinateItem.Star; //southwest       
            activeSector.Coordinates[3, 5].Item = CoordinateItem.Star; //northeast
            activeSector.Coordinates[3, 4].Item = CoordinateItem.Star; //to the North
            activeSector.Coordinates[4, 5].Item = CoordinateItem.Star; //to the east
            activeSector.Coordinates[4, 3].Item = CoordinateItem.Star; //to the west
            activeSector.Coordinates[5, 4].Item = CoordinateItem.Star; //to the south

            this.Move_Coordinate("4", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        #endregion

        [Test]
        public void MoveSector_East()
        {
            //todo: fails from hitting galactic barrier.  map init error??

            this.Move_Coordinate(((int)NavDirection.Right).ToString(), 1);

            //TODO: verify that "Friendly" has been set back down on map after movement (because console app is showing
            //TODO: it dissappearing.) -- verify sector

            //TODO: function call to verify that it shows up in the correct place in output (separate test harness)
            //todo: moving forward .1 from the previous sector pops you in the middle of the next sector
            this.CheckSectorsAfterMovement(true);

            var newSector = this.Game.Map.Playership.GetSector();

            Assert.AreEqual(_startingSector.X + 1, newSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y, newSector.Y, "(c)startingSectorY");
        }

        [Test]
        public void MoveSector_SouthEast()
        {
            this.Move_Coordinate(((int)NavDirection.RightDown).ToString(), 1);

            this.CheckSectorsAfterMovement(true);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingSector.X + 1, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y + 1, playershipSector.Y, "(c)startingSectorY");

            //verify that ship has not changed its sector
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_South()
        {
            this.Move_Coordinate(((int)NavDirection.Down).ToString(), 1);

            this.CheckSectorsAfterMovement(true);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingSector.X, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y + 1, playershipSector.Y, "(c)startingSectorY");
        }

        [Test]
        public void MoveSector_SouthWest()
        {
            this.Move_Coordinate(((int)NavDirection.LeftDown).ToString(), 1);

            this.CheckSectorsAfterMovement(true);

            var playershipSector = this.Game.Map.Playership.GetSector();

            //todo: why is this +- 2?
            Assert.AreEqual(_startingSector.X - 1, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y + 1, playershipSector.Y, "(c)startingSectorY");

            //verify that ship has not changed its sector
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_West()
        {
            //todo: this test does not work when run by itself

            Assert.AreEqual(4, _startingSector.X);
            Assert.AreEqual(4, _startingSector.Y);

            this.Move_Coordinate(((int)NavDirection.Left).ToString(), 1);

            Assert.AreEqual(4, _startingSector.X);
            Assert.AreEqual(4, _startingSector.Y);

            this.CheckSectorsAfterMovement(true);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingSector.X - 1, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y, playershipSector.Y, "(c)startingSectorY");
        }

        [Test]
        public void MoveSector_NorthWest()
        {
            this.Move_Coordinate(((int)NavDirection.LeftUp).ToString(), 1);

            this.CheckSectorsAfterMovement(true);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingSector.X - 1, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y - 1, playershipSector.Y, "(c)startingSectorY");

            //verify that ship has not changed its sector
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_North()
        {
            this.Move_Coordinate(((int)NavDirection.Up).ToString(), 1);

            this.CheckSectorsAfterMovement(true);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingSector.X, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y - 1, playershipSector.Y, "(c)startingSectorY");
        }

        [Test]
        public void MoveSector_NorthEast()
        {
            this.Move_Coordinate(((int)NavDirection.RightUp).ToString(), 1);

            this.CheckSectorsAfterMovement(true);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingSector.X + 1, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y - 1, playershipSector.Y, "(c)startingSectorY");

            //verify that ship has not changed its sector
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Ignore("")]
        [Test]
        public void MoveSector_AllDistances()
        {
            for (int direction = 1; direction < 8; direction++)
            {
                for (double i = 1; i < 8; i++)
                {
                    reset();
                    //this.Move_Sector(direction, i*8);

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
            this.Move_Coordinate(((int)NavDirection.Up).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckSectorsAfterMovement(false);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingSector.X, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(0, playershipSector.Y, "(c)startingSectorY");
        }

        [Test]
        public void HitGalacticBarrierSouth()
        {
            this.Move_Coordinate(((int)NavDirection.Down).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckSectorsAfterMovement(false);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(_startingSector.X, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(7, playershipSector.Y, "(c)startingSectorY");
        }

        [Test]
        public void HitGalacticBarrierEast()
        {
            this.Move_Coordinate(((int)NavDirection.Right).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckSectorsAfterMovement(false);

            var endingSector = this.Game.Map.Playership.GetSector();

            Assert.AreEqual(7, endingSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y, endingSector.Y, "(c)startingSectorY");
        }

        [Test]
        public void HitGalacticBarrierWest()
        {
            this.Move_Coordinate(((int)NavDirection.Left).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckSectorsAfterMovement(false);

            var playershipSector = this.Game.Map.Playership.GetSector();
            Assert.AreEqual(0, playershipSector.X, "(c)startingSectorX");
            Assert.AreEqual(_startingSector.Y, playershipSector.Y, "(c)startingSectorY");
        }

        #endregion

        protected void CheckSectorsAfterMovement(bool checkForGalacticBarrierHit)
        {
            Sector playershipSector = this.Game.Map.Playership.GetSector();
            Sector activeSector = this.Game.Map.Sectors.GetActive();

            Assert.AreEqual(64, this.Game.Map.Sectors.Count); //I'd certainly hope that this hasnt changed..
            Assert.IsFalse(_testMovement.BlockedByObstacle, "Blocked by Obstacle");

            if (checkForGalacticBarrierHit)
            {
                Assert.IsFalse(_testMovement.BlockedByGalacticBarrier, "Blocked by Galactic Barrier");
            }

            //ensure there is still only 1 of original Sector (used to address a bug early on)
            Assert.IsInstanceOf<Sector>(playershipSector);

            //ship is in active sector
            Coordinate found = activeSector.Coordinates[
                                    this.Game.Map.Playership.Coordinate.X,
                                    this.Game.Map.Playership.Coordinate.Y];

            Assert.IsInstanceOf<Coordinate>(found);

            //starting location is empty
            Sector startingSectorT = this.Game.Map.Sectors[_startingSector];
            Assert.AreEqual(CoordinateItem.Empty, startingSectorT.Coordinates[_startingCoordinateX, _startingCoordinateY].Item);

            //We moved from our original Sector, right?
            Assert.AreNotEqual(startingSectorT.X.ToString() + startingSectorT.Y, activeSector.X +
                                                                                            activeSector.Y.ToString(), "Starting Sector");

            //Friendly was set in new location
            //Playership current sector has the ship set in it 
            Assert.AreEqual(CoordinateItem.PlayerShip, playershipSector.Coordinates[this.Game.Map.Playership.Coordinate.X, this.Game.Map.Playership.Coordinate.Y].Item);

            //is ship in expected location in new Sector?
            ////indirectly..
            int found2 = playershipSector.Coordinates.Count(s => s.Item == CoordinateItem.PlayerShip);
            Assert.AreEqual(1, found2, "expected to find 1 friendly, not " + found2 + ".   ");

            //directly
            //Verifying Coordinate. Look up sector by playership's coordinates. see if a friendly is there.
            Assert.AreEqual(CoordinateItem.PlayerShip, playershipSector.Coordinates.Single(s => s.X == this.Game.Map.Playership.Coordinate.X &&
                                                                                                                s.Y == this.Game.Map.Playership.Coordinate.Y).Item);

            //Check Ship Sector against active. (this really just tests the GetActive() function - this should be a separate test as well)
            Assert.AreEqual(this.Game.Map.Playership.Point.X, activeSector.X, "this.Game.Map.Playership.Sector.X");
            Assert.AreEqual(this.Game.Map.Playership.Point.Y, activeSector.Y, "this.Game.Map.Playership.Sector.Y");
        }
    }
}
