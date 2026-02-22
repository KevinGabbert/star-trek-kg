using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.TestObjects;

namespace UnitTests.Actors.MovementTests
{
    [TestFixture]
    public class MoveCoordinateEdgeWrapTests
    {
        private Game _game;
        private Movement _movement;

        [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();
        }

        private void CreateMovementAt(int sectorX, int sectorY, int coordX, int coordY)
        {
            var settings = new StarTrekKGSettings();
            var interaction = new StarTrek_KG.Output.Interaction(settings);
            _game = new Game(settings, false)
            {
                Interact = interaction
            };

            var map = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                AddStars = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(
                        new LocationDef(new Point(sectorX, sectorY), new Point(coordX, coordY)),
                        CoordinateItem.PlayerShip)
                }
            }, interaction, _game.Config, _game);

            _game.Map = map;
            map.Sectors[new Point(sectorX, sectorY)].SetActive();
            map.SetPlayershipInActiveSector(map);

            _movement = new Movement(map.Playership) { BlockedByObstacle = false };
        }

        [Test]
        public void Impulse_EdgeWrap_Right()
        {
            CreateMovementAt(1, 1, 7, 3);
            _movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.AreEqual(2, _game.Map.Playership.Point.X);
            Assert.AreEqual(1, _game.Map.Playership.Point.Y);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(3, _game.Map.Playership.Coordinate.Y);
            Assert.IsFalse(_movement.BlockedByGalacticBarrier);
        }

        [Test]
        public void Impulse_EdgeWrap_Left()
        {
            CreateMovementAt(1, 1, 0, 3);
            _movement.Execute(MovementType.Impulse, NavDirection.Left, 1, out _, out _);

            Assert.AreEqual(0, _game.Map.Playership.Point.X);
            Assert.AreEqual(1, _game.Map.Playership.Point.Y);
            Assert.AreEqual(7, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(3, _game.Map.Playership.Coordinate.Y);
            Assert.IsFalse(_movement.BlockedByGalacticBarrier);
        }

        [Test]
        public void Impulse_EdgeWrap_Up()
        {
            CreateMovementAt(1, 1, 3, 0);
            _movement.Execute(MovementType.Impulse, NavDirection.Up, 1, out _, out _);

            Assert.AreEqual(1, _game.Map.Playership.Point.X);
            Assert.AreEqual(0, _game.Map.Playership.Point.Y);
            Assert.AreEqual(3, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(7, _game.Map.Playership.Coordinate.Y);
            Assert.IsFalse(_movement.BlockedByGalacticBarrier);
        }

        [Test]
        public void Impulse_EdgeWrap_Down()
        {
            CreateMovementAt(1, 1, 3, 7);
            _movement.Execute(MovementType.Impulse, NavDirection.Down, 1, out _, out _);

            Assert.AreEqual(1, _game.Map.Playership.Point.X);
            Assert.AreEqual(2, _game.Map.Playership.Point.Y);
            Assert.AreEqual(3, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.Y);
            Assert.IsFalse(_movement.BlockedByGalacticBarrier);
        }

        [Test]
        public void Impulse_EdgeWrap_RightDown()
        {
            CreateMovementAt(1, 1, 7, 7);
            _movement.Execute(MovementType.Impulse, NavDirection.RightDown, 1, out _, out _);

            Assert.AreEqual(2, _game.Map.Playership.Point.X);
            Assert.AreEqual(2, _game.Map.Playership.Point.Y);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.Y);
            Assert.IsFalse(_movement.BlockedByGalacticBarrier);
        }

        [Test]
        public void Impulse_EdgeWrap_RightUp()
        {
            CreateMovementAt(1, 1, 7, 0);
            _movement.Execute(MovementType.Impulse, NavDirection.RightUp, 1, out _, out _);

            Assert.AreEqual(2, _game.Map.Playership.Point.X);
            Assert.AreEqual(0, _game.Map.Playership.Point.Y);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(7, _game.Map.Playership.Coordinate.Y);
            Assert.IsFalse(_movement.BlockedByGalacticBarrier);
        }

        [Test]
        public void Impulse_EdgeWrap_LeftDown()
        {
            CreateMovementAt(1, 1, 0, 7);
            _movement.Execute(MovementType.Impulse, NavDirection.LeftDown, 1, out _, out _);

            Assert.AreEqual(0, _game.Map.Playership.Point.X);
            Assert.AreEqual(2, _game.Map.Playership.Point.Y);
            Assert.AreEqual(7, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.Y);
            Assert.IsFalse(_movement.BlockedByGalacticBarrier);
        }

        [Test]
        public void Impulse_EdgeWrap_LeftUp()
        {
            CreateMovementAt(1, 1, 0, 0);
            _movement.Execute(MovementType.Impulse, NavDirection.LeftUp, 1, out _, out _);

            Assert.AreEqual(0, _game.Map.Playership.Point.X);
            Assert.AreEqual(0, _game.Map.Playership.Point.Y);
            Assert.AreEqual(7, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(7, _game.Map.Playership.Coordinate.Y);
            Assert.IsFalse(_movement.BlockedByGalacticBarrier);
        }

        [Test]
        public void Impulse_Barrier_Left()
        {
            CreateMovementAt(0, 1, 0, 3);
            var startingEnergy = _game.Map.Playership.Energy;

            _movement.Execute(MovementType.Impulse, NavDirection.Left, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByGalacticBarrier);
            Assert.AreEqual(0, _game.Map.Playership.Point.X);
            Assert.AreEqual(1, _game.Map.Playership.Point.Y);
            Assert.AreEqual(startingEnergy - 1000, _game.Map.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip,
                _game.Map.Playership.GetSector().Coordinates[_game.Map.Playership.Coordinate.X, _game.Map.Playership.Coordinate.Y].Item);
        }

        [Test]
        public void Impulse_Barrier_Right()
        {
            CreateMovementAt(7, 1, 7, 3);
            var startingEnergy = _game.Map.Playership.Energy;

            _movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByGalacticBarrier);
            Assert.AreEqual(7, _game.Map.Playership.Point.X);
            Assert.AreEqual(1, _game.Map.Playership.Point.Y);
            Assert.AreEqual(startingEnergy - 1000, _game.Map.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip,
                _game.Map.Playership.GetSector().Coordinates[_game.Map.Playership.Coordinate.X, _game.Map.Playership.Coordinate.Y].Item);
        }

        [Test]
        public void Impulse_Barrier_Up()
        {
            CreateMovementAt(1, 0, 3, 0);
            var startingEnergy = _game.Map.Playership.Energy;

            _movement.Execute(MovementType.Impulse, NavDirection.Up, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByGalacticBarrier);
            Assert.AreEqual(1, _game.Map.Playership.Point.X);
            Assert.AreEqual(0, _game.Map.Playership.Point.Y);
            Assert.AreEqual(startingEnergy - 1000, _game.Map.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip,
                _game.Map.Playership.GetSector().Coordinates[_game.Map.Playership.Coordinate.X, _game.Map.Playership.Coordinate.Y].Item);
        }

        [Test]
        public void Impulse_Barrier_Down()
        {
            CreateMovementAt(1, 7, 3, 7);
            var startingEnergy = _game.Map.Playership.Energy;

            _movement.Execute(MovementType.Impulse, NavDirection.Down, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByGalacticBarrier);
            Assert.AreEqual(1, _game.Map.Playership.Point.X);
            Assert.AreEqual(7, _game.Map.Playership.Point.Y);
            Assert.AreEqual(startingEnergy - 1000, _game.Map.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip,
                _game.Map.Playership.GetSector().Coordinates[_game.Map.Playership.Coordinate.X, _game.Map.Playership.Coordinate.Y].Item);
        }

        [Test]
        public void Impulse_Barrier_LeftUp_FromCorner()
        {
            CreateMovementAt(0, 0, 0, 0);
            var startingEnergy = _game.Map.Playership.Energy;

            _movement.Execute(MovementType.Impulse, NavDirection.LeftUp, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByGalacticBarrier);
            Assert.AreEqual(0, _game.Map.Playership.Point.X);
            Assert.AreEqual(0, _game.Map.Playership.Point.Y);
            Assert.AreEqual(startingEnergy - 1000, _game.Map.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip,
                _game.Map.Playership.GetSector().Coordinates[_game.Map.Playership.Coordinate.X, _game.Map.Playership.Coordinate.Y].Item);
        }

        [Test]
        public void Impulse_Barrier_RightDown_FromCorner()
        {
            CreateMovementAt(7, 7, 7, 7);
            var startingEnergy = _game.Map.Playership.Energy;

            _movement.Execute(MovementType.Impulse, NavDirection.RightDown, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByGalacticBarrier);
            Assert.AreEqual(7, _game.Map.Playership.Point.X);
            Assert.AreEqual(7, _game.Map.Playership.Point.Y);
            Assert.AreEqual(startingEnergy - 1000, _game.Map.Playership.Energy);
            Assert.AreEqual(CoordinateItem.PlayerShip,
                _game.Map.Playership.GetSector().Coordinates[_game.Map.Playership.Coordinate.X, _game.Map.Playership.Coordinate.Y].Item);
        }
    }
}
