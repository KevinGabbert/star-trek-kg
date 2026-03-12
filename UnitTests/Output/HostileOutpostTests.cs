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
    public class HostileOutpostTests
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
                    new CoordinateDef(new LocationDef(new Point(0,0), new Point(7,7)), CoordinateItem.HostileShip)
                }
            });

            _interact = (Interaction)_game.Interact;
            _interact.Output.Clear();

            var sector = _game.Map.Playership.GetSector();
            var outpostCoordinate = sector.Coordinates[1, 0];
            outpostCoordinate.Item = CoordinateItem.HostileOutpost;
            outpostCoordinate.Object = new HostileOutpost
            {
                Coordinate = outpostCoordinate
            };
        }

        [Test]
        public void HostileOutpost_Requires_Three_Torpedoes_To_Destroy()
        {
            var sector = _game.Map.Playership.GetSector();
            var outpostCoordinate = sector.Coordinates[1, 0];

            for (var i = 0; i < 3; i++)
            {
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "tor");
                _interact.ReadAndOutput(_game.Map.Playership, _game.Map.Text, "7");
            }

            Assert.AreEqual(CoordinateItem.Empty, outpostCoordinate.Item);
            Assert.IsNull(outpostCoordinate.Object);
        }
    }
}
