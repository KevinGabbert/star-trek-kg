using System;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

using System.Linq;

namespace UnitTests.ShipTests.MovementTests
{
    [TestFixture]
    public class MovementTests: TestClass_Base
    {
        //todo: write error messages for no sector set up and no Region set up.
        #region Setup

        #region Setup variables
        Movement _testMovement;

        Coordinate _startingRegion;

        int _startingSectorX;
        int _startingSectorY;

        private int _lastRegionX;
        private int _lastRegionY;

        #endregion

        [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();

            reset();
            this.CheckBeforeMovement();

        }

        [TearDown]
        public void TearDown()
        {
            reset();
            this.ClearAllSectors();

            this.Game.Map.Playership = null;
            this.Game.Map = null;
            _testMovement = null;

            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.Region_MIN = 0;
            Constants.Region_MAX = 0;
        }


        //todo: test feature.. Moving the ship requires an expenditure of energy
        //this feature can be turned off by config setting

        private void reset()
        {
            this.Game.Map = (new Map(new SetupOptions
                                         {

                                             Initialize = true,
                                             AddNebulae = false,
                                             SectorDefs = new SectorDefs
                                                              {
                                                                  new SectorDef(
                                                                      new LocationDef(new Coordinate(4, 4),
                                                                                      new Coordinate(4, 4)),
                                                                      SectorItem.PlayerShip),
                                                                  //todo: this needs to be in a random spot
                                                              },
                                             AddStars = false
                                         }, this.Game.Write, this.Game.Config));

            _testMovement = new Movement(this.Game.Map.Playership, this.Game) {BlockedByObstacle = false};

            #region "Manually set ship. todo: write test to ensure that this method works too"
            ////Moves ship to new place in map - updates map
            //Sector.SetFriendly(this.Game.Map.Playership.Sector.X,
            //                    this.Game.Map.Playership.Sector.Y,
            //                    this.Game.Map);

            ////sets ship
            //this.Game.Map.Playership.Coordinate.X = 4;
            //this.Game.Map.Playership.Coordinate.Y = 4;


            //this.Game.Map.Playership.Sector.X = 4;
            //this.Game.Map.Playership.Sector.Y = 4;

            #endregion

            _startingRegion = new Coordinate(this.Game.Map.Playership.Coordinate.X, this.Game.Map.Playership.Coordinate.Y); //random

            _startingSectorX = this.Game.Map.Playership.Sector.X; //4;
            _startingSectorY = this.Game.Map.Playership.Sector.Y; //4;

            _lastRegionX = 0;
            _lastRegionY = 0;
        }

    #endregion

        #region Sectors

        [Test]
        public void HitGalacticBarrierNorth()
        {
            this.Move_Region(((int)NavDirection.North).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckRegionsAfterMovement(false);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(0, playershipRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void HitGalacticBarrierSouth()
        {
            this.Move_Region(((int)NavDirection.South).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckRegionsAfterMovement(false);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(7, playershipRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void HitGalacticBarrierEast()
        {
            this.Move_Region(((int)NavDirection.East).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckRegionsAfterMovement(false);

            var endingRegion = this.Game.Map.Playership.GetRegion();

            Assert.AreEqual(7, endingRegion.X, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, endingRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void HitGalacticBarrierWest()
        {
            this.Move_Region(((int)NavDirection.West).ToString(), 5 * 8);

            Assert.IsTrue(_testMovement.BlockedByGalacticBarrier, "Expected Galactic Barrier to be hit");

            this.CheckRegionsAfterMovement(false);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(0, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y, "(c)startingRegionY");
        }

        [Ignore]
        [Test]
        public void InvalidCourseCheck()
        {
            //todo: Needs to be tested once this function is exposed to the user via an event
        }
        
        //[Test]
        //public void HitObstacle()
        //{
        //    Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star;

        //    this.Move_Sector(((int)NavDirection.North).ToString(), .1 * 8); 

        //    Assert.IsTrue(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");

        //    Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Empty;
        //}

        //[Test]
        //public void MoveSector_North()
        //{
        //    this.Move_Sector(((int)NavDirection.North).ToString(), .1 * 8);

        //    this.CheckSectorsAfterMovement();

        //    //verify that ship has moved the expected distance from starting sector
        //    Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X + 1, "this.Game.Map.Playership.Sector.X");
        //    Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y, "this.Game.Map.Playership.Sector.Y");
        //}

        //[Test]
        //public void MoveSector_NorthEast()
        //{
        //    this.Move_Sector(((int)NavDirection.NorthEast).ToString(), .1 * 8);

        //    this.CheckSectorsAfterMovement();

        //    //verify that ship has moved the expected distance from starting sector
        //    Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X + 1, "this.Game.Map.Playership.Sector.X");
        //    Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y - 1, "this.Game.Map.Playership.Sector.Y");
        //}

        //[Test]
        //public void MoveSector_East()
        //{
        //    this.Move_Sector(((int)NavDirection.East).ToString(), .1 * 8);

        //    this.CheckSectorsAfterMovement();

        //    //verify that ship has moved the expected distance from starting sector
        //    Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X, "this.Game.Map.Playership.Sector.X");
        //    Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y - 1, "this.Game.Map.Playership.Sector.Y");
        //}

        //[Test]
        //public void MoveSector_SouthEast()
        //{
        //    this.Move_Sector(((int)NavDirection.SouthEast).ToString(), .1 * 8);

        //    this.CheckSectorsAfterMovement();

        //    //verify that ship has moved the expected distance from starting sector
        //    Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X - 1, "this.Game.Map.Playership.Sector.X");
        //    Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y - 1, "this.Game.Map.Playership.Sector.Y");
        //}

        //[Test]
        //public void MoveSector_South()
        //{
        //    this.Move_Sector(((int)NavDirection.South).ToString(), .1 * 8);

        //    this.CheckSectorsAfterMovement();

        //    //verify that ship has moved the expected distance from starting sector
        //    Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X - 1, "this.Game.Map.Playership.Sector.X");
        //    Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y, "this.Game.Map.Playership.Sector.Y");
        //}

        //[Test]
        //public void MoveSector_SouthWest()
        //{
        //    this.Move_Sector(((int)NavDirection.SouthWest).ToString(), .1 * 8);

        //    this.CheckSectorsAfterMovement();

        //    //verify that ship has moved the expected distance from starting sector
        //    Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X - 1, "this.Game.Map.Playership.Sector.X");
        //    Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y + 1, "this.Game.Map.Playership.Sector.Y");
        //}

        //[Test]
        //public void MoveSector_West()
        //{
        //    this.Move_Sector(((int)NavDirection.West).ToString(), .1 * 8);

        //    this.CheckSectorsAfterMovement();

        //    //verify that ship has moved the expected distance from starting sector
        //    Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X, "this.Game.Map.Playership.Sector.X");
        //    Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y + 1, "this.Game.Map.Playership.Sector.Y");
        //}

        //[Test]
        //public void MoveSector_NorthWest()
        //{
        //    this.Move_Sector(((int)NavDirection.NorthWest).ToString(), .1 * 8);

        //    this.CheckSectorsAfterMovement();

        //    //verify that ship has moved the expected distance from starting sector
        //    Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X + 1, "this.Game.Map.Playership.Sector.X");
        //    Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y + 1, "this.Game.Map.Playership.Sector.Y");
        //}

        //[Test]
        //public void MoveSector_AllDirections()
        //{
        //    foreach (string direction in Constants.MAP_DIRECTION)
        //    {
        //        this.Move_Sector(direction, .1 * 8);
        //        this.reset();
        //    }
        //}

        //[Test]
        //public void MoveSector_AllDistances()
        //{
        //    for (double i = .1*8; i < .8*8; i++)
        //    {
        //        foreach (string direction in Constants.MAP_DIRECTION)
        //        {
        //            this.Move_Sector(direction, i);
        //            this.reset();
        //        }
        //    }
        //}

        private void Move_Sector(string direction, int distance)
        {
            var playershipRegion = this.Game.Map.Playership.GetRegion();

            _startingRegion = new Coordinate(playershipRegion.X, playershipRegion.Y);

            _startingSectorX = this.Game.Map.Playership.Sector.X;
            _startingSectorY = this.Game.Map.Playership.Sector.Y;

            //verify that the ship is where we think it is before we start
            Assert.AreEqual(SectorItem.PlayerShip, Sector.Get(this.Game.Map.Regions.GetActive().Sectors,
                                                           this.Game.Map.Playership.Sector.X,
                                                           this.Game.Map.Playership.Sector.Y).Item);
            var sectorItem =
                Sector.Get(_testMovement.Game.Map.Regions.GetActive().Sectors, _testMovement.Game.Map.Playership.Sector.X,
                                                                       _testMovement.Game.Map.Playership.Sector.Y).Item;
            Assert.AreEqual(SectorItem.PlayerShip, sectorItem);

            _testMovement.Execute(MovementType.Impulse, Convert.ToInt32(direction), distance, out _lastRegionX, out _lastRegionY);

            //EnergySubtracted changes an entered value of .1 to .8
            //todo: measure time passed
        }

#endregion
        #region Regions

        #region Verify sector obstacles are missed when travelling at higher than sublight speeds
        [Test]
        public void MoveRegion_MissObstacleSouth()
        {
            //This is the star that would be hit without fix
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            this.Move_Region("7", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");

            //revert for next test
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Empty;
        }

        [Test]
        public void MoveRegion_MissObstacleNorth()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North

            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            this.Move_Region("3", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleWest()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west

            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            this.Move_Region("5", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleEast()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east

            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            this.Move_Region("1", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleSouthWest()
        {
            //friendly ship is on 4,4

            //This is the star that would be hit without fix
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest

            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            this.Move_Region("6", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleSouthEast()
        {
            //friendly ship is on 4,4

            //strange.  Going this direction, you actually don't get the "obstacle error" verified by the other "missobstacle" tests
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            this.Move_Region("4", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleNorthEast()
        {
            //friendly ship is on 4,4

            //This is the obstacle that gets hit
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast

            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            this.Move_Region("2", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        [Test]
        public void MoveRegion_MissObstacleNorthWest()
        {
            //friendly ship is on 4,4

            //strange.  Going this direction, you actually don't get the "obstacle error" verified by the other "missobstacle" tests
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 5).Item = SectorItem.Star; //southeast

            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 3).Item = SectorItem.Star; //northwest
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 3).Item = SectorItem.Star; //southwest       
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 5).Item = SectorItem.Star; //northeast
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 3, 4).Item = SectorItem.Star; //to the North
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 5).Item = SectorItem.Star; //to the east
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 4, 3).Item = SectorItem.Star; //to the west
            Sector.Get(this.Game.Map.Regions.GetActive().Sectors, 5, 4).Item = SectorItem.Star; //to the south

            this.Move_Region("8", 1 * 8);

            Assert.IsFalse(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");
        }

        #endregion

        [Test]
        public void MoveRegion_East()
        {
            this.Move_Region(((int)NavDirection.East).ToString(), 1*8);

            //TODO: verify that "Friendly" has been set back down on map after movement (because console app is showing
            //TODO: it dissappearing.) -- verify sector

            //TODO: function call to verify that it shows up in the correct place in output (separate test harness)
            //todo: moving forward .1 from the previous sector pops you in the middle of the next sector
            this.CheckRegionsAfterMovement(true);

            var newRegion = this.Game.Map.Playership.GetRegion();

            Assert.AreEqual(_startingRegion.X, newRegion.X - 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, newRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void MoveRegion_SouthEast()
        {
            this.Move_Region(((int)NavDirection.SouthEast).ToString(), 1 * 8);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X - 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y - 1, "(c)startingRegionY");

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X + 2, "this.Game.Map.Playership.Sector.X");
            Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y + 2, "this.Game.Map.Playership.Sector.Y");
        }

        [Test]
        public void MoveRegion_South()
        {
            this.Move_Region(((int)NavDirection.South).ToString(), 1 * 8);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y - 1, "(c)startingRegionY");
        }

        [Test]
        public void MoveRegion_SouthWest()
        {
            this.Move_Region(((int)NavDirection.SouthWest).ToString(), 1 * 8);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetRegion();

            //todo: why is this +- 2?
            Assert.AreEqual(_startingRegion.X, playershipRegion.X + 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y - 1, "(c)startingRegionY");

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X - 2, "this.Game.Map.Playership.Sector.X");
            Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y + 2, "this.Game.Map.Playership.Sector.Y");
        }

        [Test]
        public void MoveRegion_West()
        {
            //todo: this test does not work when run by itself

            Assert.AreEqual(4, _startingRegion.X);
            Assert.AreEqual(4, _startingRegion.Y);

            this.Move_Region(((int)NavDirection.West).ToString(), 1 * 8);

            Assert.AreEqual(4, _startingRegion.X);
            Assert.AreEqual(4, _startingRegion.Y);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X + 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y, "(c)startingRegionY");
        }

        [Test]
        public void MoveRegion_NorthWest()
        {
            this.Move_Region(((int)NavDirection.NorthWest).ToString(), 1 * 8);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X + 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y + 1, "(c)startingRegionY");

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X - 2, "this.Game.Map.Playership.Sector.X");
            Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y - 2, "this.Game.Map.Playership.Sector.Y");
        }

        [Test]
        public void MoveRegion_North()
        {
            this.Move_Region(((int)NavDirection.North).ToString(), 1 * 8);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y + 1, "(c)startingRegionY");
        }

        [Test]
        public void MoveRegion_NorthEast()
        {
            this.Move_Region(((int)NavDirection.NorthEast).ToString(), 1 * 8);

            this.CheckRegionsAfterMovement(true);

            var playershipRegion = this.Game.Map.Playership.GetRegion();
            Assert.AreEqual(_startingRegion.X, playershipRegion.X - 1, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, playershipRegion.Y + 1, "(c)startingRegionY");

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, this.Game.Map.Playership.Sector.X + 2, "this.Game.Map.Playership.Sector.X");
            Assert.AreEqual(_startingSectorY, this.Game.Map.Playership.Sector.Y - 2, "this.Game.Map.Playership.Sector.Y");
        }

        [Ignore]
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

#endregion

        [Ignore]// not working yet
        [Test]
        public void TravelAlongCourse_BugVerification()
        {
            var testMovement = new Movement( this.Game.Map.Playership, this.Game);

            double x = 31.5084577259018;
            double y = 31.5084577259018;

            //testMovement.TravelAlongCourse(0, 0, -0.00565685424949238, new Coordinate(), -0.00565685424949238, ref x, ref y );
            //.Encountered obstacle within Region. X:0Y:0 Friendlyvx: -0.00565685424949238 vy: -0.00565685424949238 x: 31.5084577259018 y: 31.5084577259018
            //.Encountered obstacle within Region. X:0Y:0 Friendlyvx: -0.00565685424949238 vy: -0.00565685424949238 x: 31.5084577259018 y: 31.5084577259018 lastsectX: 0 lastSectY: 0
        }

        private void Move_Region(string direction, int distance)
        {
            _testMovement.Execute(MovementType.Warp, Convert.ToInt32(direction), distance, out _lastRegionX, out _lastRegionY);
        }

        //todo: this needs to be refactored into a ship setup testfixture or something.
        private void CheckBeforeMovement()
        {
            var activeRegion = this.Game.Map.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count);

            Assert.IsInstanceOf<Region>(activeRegion);

            Assert.AreEqual(_startingRegion.X, activeRegion.X);
            Assert.AreEqual(_startingRegion.Y, activeRegion.Y);

            //Check to see if Playership has been assigned to a sector in the active Region.

            //indirectly..
            Assert.AreEqual(1, activeRegion.Sectors.Count(s => s.Item == SectorItem.PlayerShip));

            //directly.
            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors.Single(s => s.X == this.Game.Map.Playership.Sector.X && s.Y == this.Game.Map.Playership.Sector.Y).Item);

            var x = (from Sector s in activeRegion.Sectors
                     where s.Item == SectorItem.PlayerShip
                     select s).Count();

            Assert.AreEqual(1, x);

            Assert.AreEqual(_startingRegion.X, this.Game.Map.Playership.Coordinate.X, "startingRegionX");
            Assert.AreEqual(_startingRegion.Y, this.Game.Map.Playership.Coordinate.Y, "startingRegionY");

            Assert.AreEqual(4, this.Game.Map.Playership.Sector.X, "startingShipSectorX");
            Assert.AreEqual(4, this.Game.Map.Playership.Sector.Y, "startingShipSectorY");
        }

        private void CheckRegionsAfterMovement(bool checkForGalacticBarrierHit)
        {
            Region playershipRegion = this.Game.Map.Playership.GetRegion();
            Region activeRegion = this.Game.Map.Regions.GetActive();

            Assert.AreEqual(64, this.Game.Map.Regions.Count); //I'd certainly hope that this hasnt changed..
            Assert.IsFalse(_testMovement.BlockedByObstacle, "Blocked by Obstacle");

            if (checkForGalacticBarrierHit)
            {
                Assert.IsFalse(_testMovement.BlockedByGalacticBarrier, "Blocked by Galactic Barrier");
            }

            //ensure there is still only 1 of original Region (used to address a bug early on)
            Assert.IsInstanceOf<Region>(playershipRegion);

            //ship is in active sector
            Sector found = Sector.Get(activeRegion.Sectors,
                                    this.Game.Map.Playership.Sector.X,
                                    this.Game.Map.Playership.Sector.Y);

            Assert.IsInstanceOf<Sector>(found);

            //starting location is empty
            Region startingRegionT = Regions.Get(this.Game.Map, _startingRegion);
            Assert.AreEqual(SectorItem.Empty, Sector.Get(startingRegionT.Sectors, _startingSectorX, _startingSectorY).Item);

            //We moved from our original Region, right?
            Assert.AreNotEqual(startingRegionT.X.ToString() + startingRegionT.Y, activeRegion.X +
                                                                                            activeRegion.Y.ToString(), "Starting Region");

            //Friendly was set in new location
            //Playership current sector has the ship set in it 
            Assert.AreEqual(SectorItem.PlayerShip, Sector.Get(playershipRegion.Sectors, 
                                                            this.Game.Map.Playership.Sector.X, this.Game.Map.Playership.Sector.Y).Item);

            //is ship in expected location in new Region?
            ////indirectly..
            int found2 = (playershipRegion.Sectors.Where(s => s.Item == SectorItem.PlayerShip)).Count();
            Assert.AreEqual(1, found2, "expected to find 1 friendly, not " + found2 + ".   ");

            //directly
            //Verifying Sector. Look up sector by playership's coordinates. see if a friendly is there.
            Assert.AreEqual(SectorItem.PlayerShip, playershipRegion.Sectors.Single(s => s.X == this.Game.Map.Playership.Sector.X &&
                                                                                                                s.Y == this.Game.Map.Playership.Sector.Y).Item);

            //Check Ship Region against active. (this really just tests the GetActive() function - this should be a separate test as well)
            Assert.AreEqual(this.Game.Map.Playership.Coordinate.X, activeRegion.X, "this.Game.Map.Playership.Region.X");
            Assert.AreEqual(this.Game.Map.Playership.Coordinate.Y, activeRegion.Y, "this.Game.Map.Playership.Region.Y");
        }

        private void CheckSectorsAfterMovement()
        {
            var activeRegion = this.Game.Map.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count); //I'd certainly hope that this hasnt changed..
            Assert.IsFalse(_testMovement.BlockedByObstacle, "Blocked by Obstacle");

            //Ensure starting location is empty

            //indirectly
            Assert.AreNotEqual("44", this.Game.Map.Playership.Sector.X + this.Game.Map.Playership.Sector.Y.ToString(), "startingShipSectorX");

            //directly

            //Check location on map

            //todo: this needs to be be flipped back. flip the increment variable in the test instead
            //originating sector is empty
            Assert.AreEqual(SectorItem.Empty, Sector.Get(activeRegion.Sectors, _startingSectorX, _startingSectorY).Item);

            //indirectly..
            var found = (this.Game.Map.Regions.GetActive().Sectors.Where(s => s.Item == SectorItem.PlayerShip)).Count();
            Assert.AreEqual(1, found, "expected to find 1 friendly, not " + found + ".   ");

            //Look up sector by playership's coordinates. see if a friendly is there.
            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors.Single(s => s.X == this.Game.Map.Playership.Sector.X &&
                                                                                s.Y == this.Game.Map.Playership.Sector.Y).Item);
            //same thing.  uses sector.Get functionality to check.
            Assert.AreEqual(SectorItem.PlayerShip, Sector.Get(activeRegion.Sectors, this.Game.Map.Playership.Sector.X, this.Game.Map.Playership.Sector.Y).Item);

            Assert.AreEqual(_startingRegion.X, _lastRegionX, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, _lastRegionY, "(c)startingRegionY");
        }

        private void ClearAllSectors()
        {
            var activeRegion = this.Game.Map.Regions.GetActive();

            //Clear everything. //Remember, when a map is set up, a ship is generated in a random location, and
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Sector.Get(activeRegion.Sectors, i, j).Item = SectorItem.Empty;
                }
            }
        }
    }
}