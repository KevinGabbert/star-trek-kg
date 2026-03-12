using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class ShipActionCommandCoverageTests
    {
        private Game _game;
        private Interaction _interact;

        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
            var config = new StarTrekKGSettings();
            _game = new Game(config, new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(0,0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,1)), CoordinateItem.HostileShip),
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(2,2)), CoordinateItem.Starbase)
                }
            });

            _interact = (Interaction)_game.Interact;
            _interact.Output.Clear();
        }

        [TestCase("?")]
        [TestCase("imp")]
        [TestCase("wrp")]
        [TestCase("nto")]
        [TestCase("irs")]
        [TestCase("irs+")]
        [TestCase("irs++")]
        [TestCase("irs+++")]
        [TestCase("srs")]
        [TestCase("lrs")]
        [TestCase("crs")]
        [TestCase("pha")]
        [TestCase("tor")]
        [TestCase("toq")]
        [TestCase("she")]
        [TestCase("com")]
        [TestCase("dmg")]
        public void TopLevelShipCommands_DoNotThrow_AndReturnOutput(string command)
        {
            List<string> output = null;

            Assert.DoesNotThrow(() =>
            {
                output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, command);
            }, $"Command '{command}' should not throw.");

            Assert.IsNotNull(output, $"Command '{command}' returned null output.");
        }

        [Test]
        public void Impulse_CommandPath_Completes_WithDirectionAndDistance()
        {
            var before = new Point(_game.Map.Playership.Coordinate.X, _game.Map.Playership.Coordinate.Y);

            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "imp");
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "7");
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "1");

            var after = _game.Map.Playership.Coordinate;
            Assert.IsTrue(before.X != after.X || before.Y != after.Y, "Impulse path should move the ship.");
        }

        [Test]
        public void Warp_CommandPath_Completes_WithDirectionAndFactor()
        {
            var beforeSector = new Point(_game.Map.Playership.Point.X, _game.Map.Playership.Point.Y);

            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "wrp");
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "7");
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "1");

            var afterSector = _game.Map.Playership.Point;
            Assert.IsTrue(beforeSector.X != afterSector.X || beforeSector.Y != afterSector.Y, "Warp path should move sector.");
        }

        [Test]
        public void Shield_CommandPath_AddAndSubtract_DoNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "she");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "add");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "100");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "she");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "sub");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "50");
            });
        }

        [Test]
        public void ScanCommands_ReturnOutput_AfterMovement()
        {
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "imp");
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "7");
            _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "1");

            var commands = new[] { "irs", "irs+", "irs++", "irs+++", "srs", "lrs", "crs" };
            foreach (var command in commands)
            {
                var output = _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, command);
                Assert.IsNotNull(output, $"{command} returned null output.");
                Assert.IsTrue(output.Any(), $"{command} should return at least one line of output.");
            }
        }
    }
}
