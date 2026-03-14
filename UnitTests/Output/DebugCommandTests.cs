using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class DebugCommandTests
    {
        private Game _game;
        private Interaction _interact;

        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
            _game = new Game(new StarTrekKGSettings(), new SetupOptions
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
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(1,1)), CoordinateItem.HostileShip)
                }
            });

            _interact = (Interaction)_game.Interact;
            _interact.Output.Clear();
        }

        [Test]
        public void DbgGod_Toggles_GodMode_On_Playership()
        {
            var ship = (Ship)_game.Map.Playership;

            var output = _interact.ReadAndOutput(ship, _game.Map.Text, "dbg god");

            Assert.IsTrue(output.Any(line => line.Contains("God mode set to: True")));
            Assert.IsTrue(ship.GodMode);
        }

        [Test]
        public void DbgDadd_Adds_Energy_To_Playership()
        {
            var ship = (Ship)_game.Map.Playership;
            ship.Energy = 50;

            var output = _interact.ReadAndOutput(ship, _game.Map.Text, "dbg dadd 1000");

            Assert.IsTrue(output.Any(line => line.Contains("Added 1000 energy")));
            Assert.AreEqual(1050, ship.Energy);
        }
    }
}
