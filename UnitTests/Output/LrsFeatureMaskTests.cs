using System.Linq;
using System.Reflection;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class LrsFeatureMaskTests
    {
        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
        }

        [Test]
        public void RunLrsScan_Renders_Compact_Feature_Code_In_Three_Characters()
        {
            var game = new Game(new StarTrekKGSettings(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                AddDeuterium = false,
                AddGraviticMines = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(1, 1), new Point(0, 0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 1)), CoordinateItem.HostileShip),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 2)), CoordinateItem.HostileShip),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 3)), CoordinateItem.Star),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 4)), CoordinateItem.Star),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(1, 1)), CoordinateItem.Starbase),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(1, 2)), CoordinateItem.Wormhole),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(1, 3)), CoordinateItem.Deuterium)
                }
            });

            var output = LongRangeScan.For(game.Map.Playership).RunLRSScan(game.Map.Playership.GetLocation());

            Assert.IsTrue(output.Any(line => line.Contains("227")));
        }

        [Test]
        public void Obj_Decodes_Feature_Code_To_Configured_Names()
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

            var output = game.Interact.ReadAndOutput(game.Map.Playership, game.Map.Text, "obj 7");

            Assert.IsTrue(output.Any(line => line.Contains("starbase wormhole deuterium")));
        }

        [Test]
        public void Obj_Decodes_G_To_Technology_Cache()
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
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 1)), CoordinateItem.TechnologyCache)
                }
            });

            var sectorResult = LongRangeScan.Execute(game.Map.Sectors[0, 0]);

            Assert.That((sectorResult.FeatureMask & LrsFeatureMask.TechnologyCache) != 0, Is.True);
        }

        [Test]
        public void RunLrsScan_Hazard_Features_Do_Not_Render_Question_Mark()
        {
            var game = new Game(new StarTrekKGSettings(), new SetupOptions
            {
                Initialize = true,
                StrictDeterministic = true,
                AddStars = false,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(1, 1), new Point(0, 0)), CoordinateItem.PlayerShip),
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 1)), CoordinateItem.BlackHole)
                }
            });

            var output = LongRangeScan.For(game.Map.Playership).RunLRSScan(game.Map.Playership.GetLocation());

            Assert.IsFalse(output.Any(line => line.Contains("?")));
        }

        [Test]
        public void Obj_Sector_Name_Outputs_Hazard_Details()
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
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 1)), CoordinateItem.BlackHole)
                }
            });

            game.Map.Sectors[0, 0].Name = "Zane";

            var decodeMethod = typeof(Interaction).GetMethod("DecodeSectorFeatureMaskCommand", BindingFlags.Instance | BindingFlags.NonPublic);
            var output = ((System.Collections.IEnumerable)decodeMethod.Invoke(game.Interact, new object[] { game.Map.Sectors[0, 0] }))
                .Cast<string>()
                .ToList();

            Assert.IsTrue(output.Any(line => line.Contains("Zane:")));
            Assert.IsTrue(output.Any(line => line.Contains("black hole")));
            Assert.IsTrue(output.Any(line => line.Contains("Hazards")));
        }

        [Test]
        public void LrsResult_ToScanString_Uses_Bullet_Separators()
        {
            var result = new LRSResult
            {
                Hostiles = 1,
                Starbases = 2,
                Stars = 3
            };

            Assert.AreEqual("1 \u2022 2 \u2022 3", result.ToScanString());
        }
    }
}
