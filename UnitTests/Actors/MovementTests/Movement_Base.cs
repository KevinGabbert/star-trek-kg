using System;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Actors.MovementTests
{
    public class Movement_Base : TestClass_Base
    {
        //todo: write error messages for no Coordinate set up and no Coordinate set up.
        #region Setup

        #region Setup variables
        protected Movement _testMovement;
        protected Ship _testShip;

        protected Point _startingSector;

        protected int _startingCoordinateX;
        protected int _startingCoordinateY;

        protected int _lastCoordinateX;
        protected int _lastCoordinateY;

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
            this.ClearAllCoordinates();

            this.Game.Map.Playership = null;
            this.Game.Map = null;
            _testMovement = null;

            DEFAULTS.COORDINATE_MIN = 0;
            DEFAULTS.COORDINATE_MAX = 0;

            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;
        }


        //todo: test feature.. Moving the ship requires an expenditure of energy
        //this feature can be turned off by config setting

        protected void reset()
        {
            this.Game.Map = new Map(new SetupOptions
            {

                Initialize = true,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(
                        new LocationDef(new Point(4, 4),
                            new Point(4, 4)),
                        CoordinateItem.PlayerShip),
                    //todo: this needs to be in a random spot
                },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            _testMovement = new Movement(this.Game.Map.Playership) { BlockedByObstacle = false };
            _testShip = this.Game.Map.Playership; //synctactic sugar

            #region "Manually set ship. todo: write test to ensure that this method works too"
            ////Moves ship to new place in map - updates map
            //Coordinate.SetFriendly(this.Game.Map.Playership.Coordinate.X,
            //                    this.Game.Map.Playership.Coordinate.Y,
            //                    this.Game.Map);

            ////sets ship
            //this.Game.Map.Playership.Point.X = 4;
            //this.Game.Map.Playership.Point.Y = 4;


            //this.Game.Map.Playership.Coordinate.X = 4;
            //this.Game.Map.Playership.Coordinate.Y = 4;

            #endregion

            _startingSector = new Point(this.Game.Map.Playership.Point.X, this.Game.Map.Playership.Point.Y); //random

            _startingCoordinateX = this.Game.Map.Playership.Coordinate.X; //4;
            _startingCoordinateY = this.Game.Map.Playership.Coordinate.Y; //4;

            _lastCoordinateX = 0;
            _lastCoordinateY = 0;
        }

        #endregion

        //todo: this needs to be refactored into a ship setup testfixture or something.
        private void CheckBeforeMovement()
        {
            var activeSector = this.Game.Map.Sectors.GetActive();

            Assert.AreEqual(64, activeSector.Coordinates.Count);

            Assert.IsInstanceOf<Sector>(activeSector);

            Assert.AreEqual(_startingSector.X, activeSector.X);
            Assert.AreEqual(_startingSector.Y, activeSector.Y);

            //Check to see if Playership has been assigned to a sector in the active Sector.

            //indirectly..
            Assert.AreEqual(1, activeSector.Coordinates.Count(s => s.Item == CoordinateItem.PlayerShip));

            //directly.
            Assert.AreEqual(CoordinateItem.PlayerShip, activeSector.Coordinates.Single(s => s.X == this.Game.Map.Playership.Coordinate.X && s.Y == this.Game.Map.Playership.Coordinate.Y).Item);

            var x = (from Coordinate s in activeSector.Coordinates
                     where s.Item == CoordinateItem.PlayerShip
                     select s).Count();

            Assert.AreEqual(1, x);

            Assert.AreEqual(_startingSector.X, this.Game.Map.Playership.Point.X, "startingSectorX");
            Assert.AreEqual(_startingSector.Y, this.Game.Map.Playership.Point.Y, "startingSectorY");

            Assert.AreEqual(4, this.Game.Map.Playership.Coordinate.X, "startingShipSectorX");
            Assert.AreEqual(4, this.Game.Map.Playership.Coordinate.Y, "startingShipSectorY");
        }

        protected void CheckCoordinatesAfterMovement()
        {
            var activeSector = this.Game.Map.Sectors.GetActive();

            Assert.AreEqual(64, activeSector.Coordinates.Count); //I'd certainly hope that this hasnt changed..
            Assert.IsFalse(_testMovement.BlockedByObstacle, "Blocked by Obstacle");

            //Ensure starting location is empty

            //indirectly
            Assert.AreNotEqual("44", this.Game.Map.Playership.Coordinate.X + this.Game.Map.Playership.Coordinate.Y.ToString(), "startingShipSectorX");

            //directly

            //Check location on map

            //todo: this needs to be be flipped back. flip the increment variable in the test instead
            //originating sector is empty
            Assert.AreEqual(CoordinateItem.Empty, activeSector.Coordinates[_startingCoordinateX, _startingCoordinateY].Item);

            //indirectly..
            var found = this.Game.Map.Sectors.GetActive().Coordinates.Where(s => s.Item == CoordinateItem.PlayerShip).Count();
            Assert.AreEqual(1, found, "expected to find 1 friendly, not " + found + ".   ");

            //Look up sector by playership's coordinates. see if a friendly is there.
            Assert.AreEqual(CoordinateItem.PlayerShip, activeSector.Coordinates.Single(s => s.X == this.Game.Map.Playership.Coordinate.X &&
                                                                                s.Y == this.Game.Map.Playership.Coordinate.Y).Item);
            //same thing.  uses sector.Get functionality to check.
            Assert.AreEqual(CoordinateItem.PlayerShip, activeSector.Coordinates[this.Game.Map.Playership.Coordinate.X, this.Game.Map.Playership.Coordinate.Y].Item);

            Assert.AreEqual(_startingSector.X, _lastCoordinateX, "(c)startingCoordinateX");
            Assert.AreEqual(_startingSector.Y, _lastCoordinateY, "(c)startingCoordinateY");
        }

        protected void ClearAllCoordinates()
        {
            var activeSector = this.Game.Map.Sectors.GetActive();

            //Clear everything. //Remember, when a map is set up, a ship is generated in a random location, and
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    activeSector.Coordinates[i, j].Item = CoordinateItem.Empty;
                }
            }
        }


        protected void Move_Sector(NavDirection direction, int distance)
        {
            //todo: Modify this to move to next (sector)region

            _startingSector = new Point(this.Game.Map.Playership.Point.X, this.Game.Map.Playership.Point.Y);

            _startingCoordinateX = this.Game.Map.Playership.Coordinate.X;
            _startingCoordinateY = this.Game.Map.Playership.Coordinate.Y;

            //verify that the ship is where we think it is before we start
            Assert.AreEqual(CoordinateItem.PlayerShip, this.Game.Map.Sectors.GetActive().Coordinates[this.Game.Map.Playership.Coordinate.X, this.Game.Map.Playership.Coordinate.Y].Item);
            var coordinateItem =
                _testMovement.ShipConnectedTo.Map.Sectors.GetActive().Coordinates[_testMovement.ShipConnectedTo.Map.Playership.Coordinate.X,
                    _testMovement.ShipConnectedTo.Map.Playership.Coordinate.Y].Item;
            Assert.AreEqual(CoordinateItem.PlayerShip, coordinateItem);

            _testMovement.Execute(MovementType.Impulse, direction, distance, out _lastCoordinateX, out _lastCoordinateY);

            //EnergySubtracted changes an entered value of .1 to .8
            //todo: measure time passed
        }

        protected void Move_Coordinate(NavDirection direction, int distance)
        {
            _testMovement.Execute(MovementType.Impulse, direction, distance, out _lastCoordinateX, out _lastCoordinateY);
        }

        protected void Move_Coordinate(string direction, int distance)
        {
            _testMovement.Execute(MovementType.Warp, (NavDirection)Convert.ToInt32(direction), distance, out _lastCoordinateX, out _lastCoordinateY);
        }
    }
}
