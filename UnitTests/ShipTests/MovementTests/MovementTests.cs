﻿using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

using System.Linq;

namespace UnitTests.ShipTests.MovementTests
{
    [TestFixture]
    public class MovementTests
    {
        //todo: write error messages for no sector set up and no quadrant set up.
        #region Setup

        #region Setup variables
        Map _testMapNoObjects;
        Movement _testMovement;

        int startingQuadrantX;
        int startingQuadrantY;

        int startingSectorX;
        int startingSectorY;

        private int lastQuadX;
        private int lastQuadY;

        #endregion

        [SetUp]
        public void Setup()
        {
            Constants.SECTOR_MIN = StarTrekKGSettings.GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = StarTrekKGSettings.GetSetting<int>("SECTOR_MAX");

            Constants.QUADRANT_MIN = StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = StarTrekKGSettings.GetSetting<int>("QuadrantMax");

            reset();
            this.CheckBeforeMovement();

        }

        [TearDown]
        public void TearDown()
        {
            reset();
            this.ClearAllSectors();

            _testMapNoObjects.Playership = null;
            _testMapNoObjects = null;
            _testMovement = null;

            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
        }

        private void reset()
        {
            var x = new GameConfig
                        {
                            //GenerateMap = true,
                            Initialize = true,
                            UseAppConfigSectorDefs = false,
                            SectorDefs = new SectorDefs
                                                {
                                                    new SectorDef(new LocationDef(new Coordinate(4, 4), new Coordinate(4, 4)), SectorItem.Friendly),
                                                    //todo: this needs to be in a random spot
                                                },
                            AddStars = false
                        };

            _testMapNoObjects = new Map(x);
            _testMovement = new Movement(_testMapNoObjects);
            _testMovement.BlockedByObstacle = false;

            #region "Manually set ship. todo: write test to ensure that this method works too"
            ////Moves ship to new place in map - updates map
            //Sector.SetFriendly(_testMapNoObjects.Playership.Sector.X,
            //                    _testMapNoObjects.Playership.Sector.Y,
            //                    _testMapNoObjects);

            ////sets ship
            //_testMapNoObjects.Playership.Coordinate.X = 4;
            //_testMapNoObjects.Playership.Coordinate.Y = 4;


            //_testMapNoObjects.Playership.Sector.X = 4;
            //_testMapNoObjects.Playership.Sector.Y = 4;

            #endregion

            startingQuadrantX = _testMapNoObjects.Playership.QuadrantDef.X; //random;
            startingQuadrantY = _testMapNoObjects.Playership.QuadrantDef.Y; //random;

            startingSectorX = _testMapNoObjects.Playership.Sector.X; //4;
            startingSectorY = _testMapNoObjects.Playership.Sector.Y; //4;

            lastQuadX = 0;
            lastQuadY = 0;
        }

    #endregion

        #region Sectors

        [Test]
        public void HitGalacticBarrierNorth()
        {
            this.Move_Quadrant("n", 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckQuadrantsAfterMovement(false);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X, "(c)startingQuadrantX");
            Assert.AreEqual(0, _testMapNoObjects.Playership.GetQuadrant().Y, "(c)startingQuadrantY");
        }

        [Test]
        public void HitGalacticBarrierSouth()
        {
            this.Move_Quadrant("s", 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckQuadrantsAfterMovement(false);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X, "(c)startingQuadrantX");
            Assert.AreEqual(7, _testMapNoObjects.Playership.GetQuadrant().Y, "(c)startingQuadrantY");
        }

        [Test]
        public void HitGalacticBarrierEast()
        {
            this.Move_Quadrant("e", 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckQuadrantsAfterMovement(false);

            Assert.AreEqual(7, _testMapNoObjects.Playership.GetQuadrant().X, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y, "(c)startingQuadrantY");
        }

        [Test]
        public void HitGalacticBarrierWest()
        {
            this.Move_Quadrant("w", 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckQuadrantsAfterMovement(false);

            Assert.AreEqual(0, _testMapNoObjects.Playership.GetQuadrant().X, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y, "(c)startingQuadrantY");
        }

        [Ignore]
        [Test]
        public void InvalidCourseCheck()
        {
            //todo: Needs to be tested once this function is exposed to the user via an event
        }
        
        [Test]
        public void HitObstacle()
        {
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star;

            this.Move_Sector("n", .1*8); 

            Assert.IsTrue(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Empty;
        }

        [Test]
        public void MoveSector_North()
        {
            this.Move_Sector("n", .1*8);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X + 1, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveSector_NorthEast()
        {
            this.Move_Sector("ne", .1*8);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X + 1, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y - 1, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveSector_East()
        {
            this.Move_Sector("e", .1*8);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y - 1, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveSector_SouthEast()
        {
            this.Move_Sector("se", .1*8);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X - 1, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y - 1, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveSector_South()
        {
            this.Move_Sector("s", .1*8);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X - 1, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveSector_SouthWest()
        {
            this.Move_Sector("sw", .1*8);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X - 1, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y + 1, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveSector_West()
        {  
            this.Move_Sector("w", .1*8);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y + 1, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveSector_NorthWest()
        {
            this.Move_Sector("nw", .1*8);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X + 1, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y + 1, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveSector_AllDirections()
        {
            foreach (string direction in Constants.MAP_DIRECTION)
            {
                this.Move_Sector(direction, .1 * 8);
                this.reset();
            }
        }

        [Test]
        public void MoveSector_AllDistances()
        {
            for (double i = .1*8; i < .8*8; i++)
            {
                foreach (string direction in Constants.MAP_DIRECTION)
                {
                    this.Move_Sector(direction, i);
                    this.reset();
                }
            }
        }

        private void Move_Sector(string direction, double distance)
        {
            startingQuadrantX = _testMapNoObjects.Playership.GetQuadrant().X;
            startingQuadrantY = _testMapNoObjects.Playership.GetQuadrant().Y;

            startingSectorX = _testMapNoObjects.Playership.Sector.X;
            startingSectorY = _testMapNoObjects.Playership.Sector.Y;

            //verify that the ship is where we think it is before we start
            Assert.AreEqual(SectorItem.Friendly, Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 
                                                           _testMapNoObjects.Playership.Sector.X,
                                                           _testMapNoObjects.Playership.Sector.Y).Item);
            var sectorItem =
                Sector.Get(_testMovement.Map.Quadrants.GetActive().Sectors, _testMovement.Map.Playership.Sector.X,
                                                                       _testMovement.Map.Playership.Sector.Y).Item;
            Assert.AreEqual(SectorItem.Friendly, sectorItem);

            _testMovement.Execute(direction, distance, distance/8, out lastQuadX, out lastQuadY);

            //EnergySubtracted changes an entered value of .1 to .8
            //todo: measure time passed
        }

#endregion
        #region Quadrants

        #region Verify sector obstacles are missed when travelling at higher than sublight speeds
        [Test]
        public void MoveQuadrant_MissObstacleSouth()
        {
            //This is the star that would be hit without fix
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            this.Move_Quadrant("e", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");

            //revert for next test
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Empty;
        }

        [Test]
        public void MoveQuadrant_MissObstacleNorth()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            this.Move_Quadrant("w", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveQuadrant_MissObstacleWest()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            this.Move_Quadrant("n", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveQuadrant_MissObstacleEast()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            this.Move_Quadrant("s", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveQuadrant_MissObstacleSouthWest()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            this.Move_Quadrant("ne", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveQuadrant_MissObstacleSouthEast()
        {
            //friendly ship is on 4,4

            //strange.  Going this direction, you actually don't get the "obstacle error" verified by the other "missobstacle" tests
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            this.Move_Quadrant("nw", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveQuadrant_MissObstacleNorthEast()
        {
            //friendly ship is on 4,4

            //This is the obstacle that gets hit
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            this.Move_Quadrant("sw", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveQuadrant_MissObstacleNorthWest()
        {
            //friendly ship is on 4,4

            //strange.  Going this direction, you actually don't get the "obstacle error" verified by the other "missobstacle" tests
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast

            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest       
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            this.Move_Quadrant("se", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        #endregion

        [Test]
        public void MoveQuadrant_East()
        {
            this.Move_Quadrant("e", 1*8);

            //TODO: verify that "Friendly" has been set back down on map after movement (because console app is showing
            //TODO: it dissappearing.) -- verify sector

            //TODO: function call to verify that it shows up in the correct place in output (separate test harness)
            //todo: moving forward .1 from the previous sector pops you in the middle of the next sector
            this.CheckQuadrantsAfterMovement(true);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X - 1, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y, "(c)startingQuadrantY");
        }

        [Test]
        public void MoveQuadrant_SouthEast()
        {
            this.Move_Quadrant("se", 1*8);

            this.CheckQuadrantsAfterMovement(true);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X - 1, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y - 1, "(c)startingQuadrantY");

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X + 2, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y + 2, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveQuadrant_South()
        {
            this.Move_Quadrant("s", 1*8);

            this.CheckQuadrantsAfterMovement(true);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y - 1, "(c)startingQuadrantY");
        }

        [Test]
        public void MoveQuadrant_SouthWest()
        {
            this.Move_Quadrant("sw", 1*8);

            this.CheckQuadrantsAfterMovement(true);

            //todo: why is this +- 2?
            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X + 1, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y - 1, "(c)startingQuadrantY");

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X - 2, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y + 2, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveQuadrant_West()
        {
            //todo: this test does not work when run by itself

            this.Move_Quadrant("w", 1*8);

            this.CheckQuadrantsAfterMovement(true);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X + 1, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y, "(c)startingQuadrantY");
        }

        [Test]
        public void MoveQuadrant_NorthWest()
        {
            this.Move_Quadrant("nw", 1*8);

            this.CheckQuadrantsAfterMovement(true);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X + 1, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y + 1, "(c)startingQuadrantY");

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X - 2, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y - 2, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Test]
        public void MoveQuadrant_North()
        {
            this.Move_Quadrant("n", 1*8);

            this.CheckQuadrantsAfterMovement(true);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y + 1, "(c)startingQuadrantY");
        }

        [Test]
        public void MoveQuadrant_NorthEast()
        {
            this.Move_Quadrant("ne", 1*8);

            this.CheckQuadrantsAfterMovement(true);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.GetQuadrant().X - 1, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.GetQuadrant().Y + 1, "(c)startingQuadrantY");

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(startingSectorX, _testMapNoObjects.Playership.Sector.X + 2, "_testMapNoObjects.Playership.Sector.X");
            Assert.AreEqual(startingSectorY, _testMapNoObjects.Playership.Sector.Y - 2, "_testMapNoObjects.Playership.Sector.Y");
        }

        [Ignore]
        [Test]
        public void MoveQuadrant_AllDistances()
        {
            for (int direction = 1; direction < 8; direction++)
            {
                for (double i = 1; i < 8; i++)
                {
                    reset();
                    //this.Move_Quadrant(direction, i*8);
                    
                    //todo: for this to be the best test, then the ship would need to move back to its
                    //starting location so it can move the max distance again.
                    //without reset() this test breaks another test.
                    //I wonder if it is becuase when ship hits the edge of the galaxy, (current functionality
                    //negates the last sector of movement), the ship keeps getting knocked back 1 sector location

                    //CheckAfterMovement();
                }
            }
        }

#endregion

        [Ignore]// not working yet
        [Test]
        public void TravelAlongCourse_BugVerification()
        {
            var testMovement = new Movement(_testMapNoObjects);

            double x = 31.5084577259018;
            double y = 31.5084577259018;

            //testMovement.TravelAlongCourse(0, 0, -0.00565685424949238, new Coordinate(), -0.00565685424949238, ref x, ref y );
            //.Encountered obstacle within quadrant. X:0Y:0 Friendlyvx: -0.00565685424949238 vy: -0.00565685424949238 x: 31.5084577259018 y: 31.5084577259018
            //.Encountered obstacle within quadrant. X:0Y:0 Friendlyvx: -0.00565685424949238 vy: -0.00565685424949238 x: 31.5084577259018 y: 31.5084577259018 lastsectX: 0 lastSectY: 0
        }

        private void Move_Quadrant(string direction, double distance)
        {
            _testMovement.Execute(direction, distance, distance, out lastQuadX, out lastQuadY);
        }

        //todo: this needs to be refactored into a ship setup testfixture or something.
        private void CheckBeforeMovement()
        {
            Assert.AreEqual(64, _testMapNoObjects.Quadrants.GetActive().Sectors.Count);

            Assert.IsInstanceOf<Quadrant>(_testMapNoObjects.Quadrants.GetActive());

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Quadrants.GetActive().X);
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Quadrants.GetActive().Y);

            //Check to see if Playership has been assigned to a sector in the active quadrant.

            //indirectly..
            Assert.AreEqual(1, _testMapNoObjects.Quadrants.GetActive().Sectors.Count(s => s.Item == SectorItem.Friendly));

            //directly.
            Assert.AreEqual(SectorItem.Friendly, _testMapNoObjects.Quadrants.GetActive().Sectors.Single(s => s.X == _testMapNoObjects.Playership.Sector.X && s.Y == _testMapNoObjects.Playership.Sector.Y).Item);

            var x = (from Sector s in _testMapNoObjects.Quadrants.GetActive().Sectors
                     where s.Item == SectorItem.Friendly
                     select s).Count();

            Assert.AreEqual(1, x);

            Assert.AreEqual(startingQuadrantX, _testMapNoObjects.Playership.QuadrantDef.X, "startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, _testMapNoObjects.Playership.QuadrantDef.Y, "startingQuadrantY");

            Assert.AreEqual(4, _testMapNoObjects.Playership.Sector.X, "startingShipSectorX");
            Assert.AreEqual(4, _testMapNoObjects.Playership.Sector.Y, "startingShipSectorY");
        }

        private void CheckQuadrantsAfterMovement(bool checkForGalacticBarrierHit)
        {
            Assert.AreEqual(64, _testMapNoObjects.Quadrants.Count); //I'd certainly hope that this hasnt changed..
            Assert.IsFalse(_testMovement.BlockedByObstacle, "Blocked by Obstacle");

            if (checkForGalacticBarrierHit)
            {
                Assert.IsFalse(_testMovement.BlockedByGalacticBarrier, "Blocked by Galactic Barrier");
            }

            //ensure there is still only 1 of original quadrant (used to address a bug early on)
            Assert.IsInstanceOf<Quadrant>(_testMapNoObjects.Playership.GetQuadrant());

            //ship is in active sector
            var found = Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors,
                                    _testMapNoObjects.Playership.Sector.X,
                                    _testMapNoObjects.Playership.Sector.Y);

            Assert.IsInstanceOf<Sector>(found);

            //starting location is empty
            var startingQuadrant = Quadrants.Get(_testMapNoObjects, startingQuadrantX, startingQuadrantY);
            Assert.AreEqual(SectorItem.Empty, Sector.Get(startingQuadrant.Sectors, startingSectorX, startingSectorY).Item);

            //We moved from our original quadrant, right?
            Assert.AreNotEqual(startingQuadrantX.ToString() + startingQuadrantY.ToString(), _testMapNoObjects.Playership.GetQuadrant().X +
                                                                                            _testMapNoObjects.Playership.GetQuadrant().Y.ToString(), "Starting Quadrant");

            //Friendly was set in new location
            //Playership current sector has the ship set in it 
            Assert.AreEqual(SectorItem.Friendly, Sector.Get(_testMapNoObjects.Playership.GetQuadrant().Sectors, 
                                                            _testMapNoObjects.Playership.Sector.X, _testMapNoObjects.Playership.Sector.Y).Item);

            //is ship in expected location in new quadrant?
            ////indirectly..
            var found2 = (_testMapNoObjects.Playership.GetQuadrant().Sectors.Where(s => s.Item == SectorItem.Friendly)).Count();
            Assert.AreEqual(1, found2, "expected to find 1 friendly, not " + found2 + ".   ");

            //directly
            //Verifying Sector. Look up sector by playership's coordinates. see if a friendly is there.
            Assert.AreEqual(SectorItem.Friendly, _testMapNoObjects.Playership.GetQuadrant().Sectors.Single(s => s.X == _testMapNoObjects.Playership.Sector.X &&
                                                                                                                s.Y == _testMapNoObjects.Playership.Sector.Y).Item);

            //Check Ship Quadrant against active. (this really just tests the GetActive() function - this should be a separate test as well)
            Assert.AreEqual(_testMapNoObjects.Playership.QuadrantDef.X, _testMapNoObjects.Quadrants.GetActive().X, "_testMapNoObjects.Playership.Quadrant.X");
            Assert.AreEqual(_testMapNoObjects.Playership.QuadrantDef.Y, _testMapNoObjects.Quadrants.GetActive().Y, "_testMapNoObjects.Playership.Quadrant.Y");
        }

        private void CheckSectorsAfterMovement()
        {
            Assert.AreEqual(64, _testMapNoObjects.Quadrants.GetActive().Sectors.Count); //I'd certainly hope that this hasnt changed..
            Assert.IsFalse(_testMovement.BlockedByObstacle, "Blocked by Obstacle");

            //Ensure starting location is empty

            //indirectly
            Assert.AreNotEqual("44", _testMapNoObjects.Playership.Sector.X + _testMapNoObjects.Playership.Sector.Y.ToString(), "startingShipSectorX");

            //directly

            //Check location on map

            //todo: this needs to be be flipped back. flip the increment variable in the test instead
            //originating sector is empty
            Assert.AreEqual(SectorItem.Empty, Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, startingSectorX, startingSectorY).Item);

            //indirectly..
            var found = (_testMapNoObjects.Quadrants.GetActive().Sectors.Where(s => s.Item == SectorItem.Friendly)).Count();
            Assert.AreEqual(1, found, "expected to find 1 friendly, not " + found + ".   ");

            //Look up sector by playership's coordinates. see if a friendly is there.
            Assert.AreEqual(SectorItem.Friendly, _testMapNoObjects.Quadrants.GetActive().Sectors.Single(s => s.X == _testMapNoObjects.Playership.Sector.X &&
                                                                                                        s.Y == _testMapNoObjects.Playership.Sector.Y).Item);
            //same thing.  uses sector.Get functionality to check.
            Assert.AreEqual(SectorItem.Friendly, Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, _testMapNoObjects.Playership.Sector.X, _testMapNoObjects.Playership.Sector.Y).Item);

            Assert.AreEqual(startingQuadrantX, lastQuadX, "(c)startingQuadrantX");
            Assert.AreEqual(startingQuadrantY, lastQuadY, "(c)startingQuadrantY");
        }

        private void ClearAllSectors()
        {
            //Clear everything. //Remember, when a map is set up, a ship is generated in a random location, and
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors, i, j).Item = SectorItem.Empty;
                }
            }
        }
    }
}