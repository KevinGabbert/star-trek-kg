using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Actors.MovementTests
{
    [TestFixture]
    public class MoveCoordinateCollisionTests
    {
        private Game _game;
        private Movement _movement;

        [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();
        }

        private void CreateMovementAt(CoordinateDefs coordinateDefs)
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
                CoordinateDefs = coordinateDefs
            }, interaction, _game.Config, _game);

            _game.Map = map;
            map.Sectors[new Point(0, 0)].SetActive();
            map.SetPlayershipInActiveSector(map);

            _movement = new Movement(map.Playership) { BlockedByObstacle = false };
        }

        [Test]
        public void Impulse_CollisionWithStarbase_AppliesConfiguredDamage_AndDoesNotOverwrite()
        {
            CreateMovementAt(new CoordinateDefs
            {
                new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip),
                new CoordinateDef(new LocationDef(new Point(0, 0), new Point(1, 0)), CoordinateItem.Starbase)
            });

            var startingEnergy = _game.Map.Playership.Energy;
            var expectedDamage = _game.Config.GetSetting<int>("StarbaseCollisionDamage");

            _movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByObstacle);
            Assert.AreEqual(startingEnergy - expectedDamage, _game.Map.Playership.Energy);
            Assert.IsFalse(_game.Map.Playership.Destroyed);

            Assert.AreEqual(0, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.Y);
            Assert.AreEqual(CoordinateItem.PlayerShip, _game.Map.Playership.GetSector().Coordinates[0, 0].Item);
            Assert.AreEqual(CoordinateItem.Starbase, _game.Map.Playership.GetSector().Coordinates[1, 0].Item);
        }

        [Test]
        public void Impulse_CollisionWithHostile_AppliesConfiguredDamage_AndDoesNotOverwrite()
        {
            CreateMovementAt(new CoordinateDefs
            {
                new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip),
                new CoordinateDef(new LocationDef(new Point(0, 0), new Point(1, 0)), CoordinateItem.HostileShip)
            });

            var startingEnergy = _game.Map.Playership.Energy;
            var expectedDamage = _game.Config.GetSetting<int>("HostileCollisionDamage");

            _movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByObstacle);
            Assert.AreEqual(startingEnergy - expectedDamage, _game.Map.Playership.Energy);
            Assert.IsFalse(_game.Map.Playership.Destroyed);

            Assert.AreEqual(0, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.Y);
            Assert.AreEqual(CoordinateItem.PlayerShip, _game.Map.Playership.GetSector().Coordinates[0, 0].Item);
            Assert.AreEqual(CoordinateItem.HostileShip, _game.Map.Playership.GetSector().Coordinates[1, 0].Item);
        }

        [Test]
        public void Impulse_CollisionWithStar_DestroysShip_AndDoesNotOverwrite()
        {
            CreateMovementAt(new CoordinateDefs
            {
                new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip),
                new CoordinateDef(new LocationDef(new Point(0, 0), new Point(1, 0)), CoordinateItem.Star)
            });

            _movement.Execute(MovementType.Impulse, NavDirection.Right, 1, out _, out _);

            Assert.IsTrue(_movement.BlockedByObstacle);
            Assert.IsTrue(_game.Map.Playership.Destroyed);
            Assert.AreEqual(0, _game.Map.Playership.Energy);

            Assert.AreEqual(0, _game.Map.Playership.Coordinate.X);
            Assert.AreEqual(0, _game.Map.Playership.Coordinate.Y);
            Assert.AreEqual(CoordinateItem.PlayerShip, _game.Map.Playership.GetSector().Coordinates[0, 0].Item);
            Assert.AreEqual(CoordinateItem.Star, _game.Map.Playership.GetSector().Coordinates[1, 0].Item);
        }
    }
}
