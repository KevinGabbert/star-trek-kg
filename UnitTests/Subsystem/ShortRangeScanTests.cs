using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;

namespace UnitTests.Subsystem
{
    [TestFixture]
    public class ShortRangeScanTests
    {
        [Test]
        public void Controls_WhenDamaged_ReturnsOutputQueue_InsteadOfNull()
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
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip)
                }
            });

            ShortRangeScan.For(game.Map.Playership).Damage = 1;

            var output = game.SubscriberSendAndGetResponse("srs");

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Any(line => line.Contains("Short Range Scan Damaged.")));
        }
    }
}
