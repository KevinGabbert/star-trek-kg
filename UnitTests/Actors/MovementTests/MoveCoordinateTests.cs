using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace UnitTests.Actors.MovementTests
{
    public class MoveCoordinateTests : Movement_Base
    {
        #region Coordinates

        [Ignore("")]
        [Test]
        public void InvalidCourseCheck()
        {
            //todo: Needs to be tested once this function is exposed to the user via an event
        }

        [Test]
        public void HitObstacle()
        {
            Assert.AreEqual(4, _testShip.Coordinate.X);
            Assert.AreEqual(4, _testShip.Coordinate.Y);

            Sector activeSector = this.Game.Map.Sectors.GetActive();
            var shipSector = _testShip.GetSector();

            Assert.AreEqual(activeSector.Name, shipSector.Name);

            var _testCoordinate = this.Game.Map.Sectors[activeSector.Name].Coordinates[4, 3];

            //todo: simplify the setting of an object - get rid of "item"
            _testCoordinate.Item = CoordinateItem.Star;
            _testCoordinate.Object = new Star()
            {
                Designation = "Fred"
            };

            this.Move_Coordinate(NavDirection.Up, 3);

            Assert.IsTrue(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");

            this.Game.Map.Sectors.GetActive().Coordinates[4, 3].Item = CoordinateItem.Empty;
        }

        [Test]
        public void MoveCoordinate_North()
        {
            this.Move_Coordinate(NavDirection.Up, 1);
            this.CheckCoordinatesAfterMovement();

            //verify that ship has moved the expected distance from starting Coordinate
            //we test by "reversing" the movement made by the playership
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y + 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveCoordinate_NorthEast()
        {
            this.Move_Coordinate(NavDirection.RightUp, 1);

            this.CheckCoordinatesAfterMovement();

            //verify that ship has moved the expected distance from starting Coordinate
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X - 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y + 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveCoordinate_East()
        {
            this.Move_Coordinate(NavDirection.Right, 1);

            this.CheckCoordinatesAfterMovement();

            //verify that ship has moved the expected distance from starting Coordinate
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X - 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveCoordinate_SouthEast()
        {
            this.Move_Coordinate(NavDirection.RightDown, 1);

            this.CheckCoordinatesAfterMovement();

            //verify that ship has moved the expected distance from starting Coordinate
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X - 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y - 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveCoordinate_South()
        {
            this.Move_Coordinate(NavDirection.Down, 1);

            this.CheckCoordinatesAfterMovement();

            //verify that ship has moved the expected distance from starting Coordinate
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y - 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveCoordinate_SouthWest()
        {
            this.Move_Coordinate(NavDirection.LeftDown, 1);

            this.CheckCoordinatesAfterMovement();

            //verify that ship has moved the expected distance from starting Coordinate
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X + 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y - 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveCoordinate_West()
        {
            this.Move_Coordinate(NavDirection.Left, 1);

            this.CheckCoordinatesAfterMovement();

            //verify that ship has moved the expected distance from starting Coordinate
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X + 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveCoordinate_NorthWest()
        {
            this.Move_Coordinate(NavDirection.LeftUp, 1);

            this.CheckCoordinatesAfterMovement();

            //verify that ship has moved the expected distance from starting Coordinate
            Assert.AreEqual(_startingCoordinateX, _testShip.Coordinate.X + 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingCoordinateY, _testShip.Coordinate.Y + 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveCoordinate_AllDirections()
        {
            List<int> directions = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();

            foreach (int direction in directions)
            {
                this.Move_Coordinate((NavDirection)direction, 1);
                this.reset();
            }
        }

        [Test]
        public void MoveCoordinate_AllDistances()
        {
            List<int> directions = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();

            for (int distance = 1; distance < 4; distance++)
            {
                foreach (int direction in directions)
                {
                    this.Move_Coordinate((NavDirection)direction, distance);
                    base.reset();
                }
            }
        }

        #endregion
    }
}
