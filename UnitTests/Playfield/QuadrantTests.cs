using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.TestObjects;

namespace UnitTests.Playfield
{
    [TestFixture]
    public class QuadrantTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
        }

        [Test]
        public void QuadrantRules_Assigns_Expected_Quadrants()
        {
            var game = new Game(new StarTrekKGSettings(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip)
                }
            });

            Assert.AreEqual("Alpha", QuadrantRules.GetQuadrantName(game.Map, 0, 0));
            Assert.AreEqual("Beta", QuadrantRules.GetQuadrantName(game.Map, 7, 0));
            Assert.AreEqual("Gamma", QuadrantRules.GetQuadrantName(game.Map, 0, 7));
            Assert.AreEqual("Delta", QuadrantRules.GetQuadrantName(game.Map, 7, 7));
        }

        [Test]
        public void LongRangeScan_Execute_Sets_Quadrant_Name()
        {
            var game = new Game(new StarTrekKGSettings(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip)
                }
            });

            var result = LongRangeScan.Execute(game.Map.Sectors[0, 0]);

            Assert.AreEqual("Alpha", result.QuadrantName);
        }

        [Test]
        public void QuadrantRules_Returns_Greek_Symbols()
        {
            var game = new Game(new StarTrekKGSettings(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip)
                }
            });

            Assert.AreEqual("\u0391", QuadrantRules.GetQuadrantSymbol(game.Map, 0, 0));
            Assert.AreEqual("\u0392", QuadrantRules.GetQuadrantSymbol(game.Map, 7, 0));
            Assert.AreEqual("\u0393", QuadrantRules.GetQuadrantSymbol(game.Map, 0, 7));
            Assert.AreEqual("\u0394", QuadrantRules.GetQuadrantSymbol(game.Map, 7, 7));
        }

        [Test]
        public void Federation_And_Vulcan_Ships_Are_Friendly_From_Config()
        {
            var game = new Game(new StarTrekKGSettings().GetConfig(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip)
                }
            });

            var sector = game.Map.Sectors[0, 0];
            var federationCoord = sector.Coordinates.First(c => c.Item == CoordinateItem.Empty);
            var vulcanCoord = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).Skip(1).First();

            var federation = new Ship(FactionName.Federation, "U.S.S. Test", federationCoord, game.Map);
            var vulcan = new Ship(FactionName.Vulcan, "VSS Test", vulcanCoord, game.Map);

            Assert.AreEqual(Allegiance.GoodGuy, federation.Allegiance);
            Assert.AreEqual(Allegiance.GoodGuy, vulcan.Allegiance);
        }

        [Test]
        public void AllHostilesAttack_FriendlyShips_Support_Player_After_Attack()
        {
            var game = new Game(new StarTrekKGSettings(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(1, 0)), CoordinateItem.HostileShip)
                }
            });

            var sector = game.Map.Sectors[0, 0];
            var friendlyCoord = sector.Coordinates.First(c => c.Item == CoordinateItem.Empty);
            var federation = new Ship(FactionName.Federation, "U.S.S. Escort", friendlyCoord, game.Map);
            sector.AddShip(federation, friendlyCoord);

            game.Interact.Output.Clear();
            game.ALLHostilesAttack(game.Map);

            Assert.IsTrue(game.Interact.Output.Queue.Any(line => line.Contains("U.S.S. Escort fires on")));
        }
    }
}
