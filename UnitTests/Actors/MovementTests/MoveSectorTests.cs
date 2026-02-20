using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace UnitTests.Actors.MovementTests.MoveSectorTests
{
    public class MoveSectorTests : Movement_Base
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

            Sector activeRegion = this.Game.Map.Sectors.GetActive();
            Sector shipRegion = _testShip.GetSector();

            Assert.AreEqual(activeRegion.Name, shipRegion.Name);

            var _testSector = this.Game.Map.Sectors[activeRegion.Name].Coordinates[3, 4];

            //todo: simplify the setting of an object - get rid of "item"
            _testSector.Item = CoordinateItem.Star;
            _testSector.Object = new Star()
            {
                Designation = "Fred"
            };

            this.Move_Sector(NavDirection.Up, 3);

            Assert.IsTrue(_testMovement.BlockedByObstacle, "Failed to hit Obstacle");

            this.Game.Map.Sectors.GetActive().Coordinates[3, 4].Item = CoordinateItem.Empty;
        }

        [Test]
        public void MoveSector_North()
        {
            this.Move_Sector(NavDirection.Up, 1);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            //we test by "reversing" the movement made by the playership
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X + 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_NorthEast()
        {
            this.Move_Sector(NavDirection.RightUp, 1);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X + 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y - 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_East()
        {
            this.Move_Sector(NavDirection.Right, 1);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y - 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_SouthEast()
        {
            this.Move_Sector(NavDirection.RightDown, 1);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X - 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y - 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_South()
        {
            this.Move_Sector(NavDirection.Down, 1);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X - 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_SouthWest()
        {
            this.Move_Sector(NavDirection.LeftDown, 1);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X - 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y + 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_West()
        {
            this.Move_Sector(NavDirection.Left, 1);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y + 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_NorthWest()
        {
            this.Move_Sector(NavDirection.LeftUp, 1);

            this.CheckSectorsAfterMovement();

            //verify that ship has moved the expected distance from starting sector
            Assert.AreEqual(_startingSectorX, _testShip.Coordinate.X + 1, "this.Game.Map.Playership.Coordinate.X");
            Assert.AreEqual(_startingSectorY, _testShip.Coordinate.Y + 1, "this.Game.Map.Playership.Coordinate.Y");
        }

        [Test]
        public void MoveSector_AllDirections()
        {
            List<int> directions = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();

            foreach (int direction in directions)
            {
                this.Move_Sector((NavDirection)direction, 1);
                this.reset();
            }
        }

        [Test]
        public void MoveSector_AllDistances()
        {
            List<int> directions = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();

            for (int distance = 1; distance < 4; distance++)
            {
                foreach (int direction in directions)
                {
                    this.Move_Sector((NavDirection)direction, distance);
                    base.reset();
                }
            }
        }

        #endregion
    }
}
