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
    public class CombinedRangeScanTests
    {
        [Test]
        public void Controls_WhenShortRangeScanIsDamaged_StillRenders_WhenCombinedRangeScanIsIntact()
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

            var output = game.SubscriberSendAndGetResponse("crs");

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Any(line => line.Contains("Sector:")));
            Assert.IsFalse(output.Any(line => line.Contains("Combined Scan needs SRS Subsystem in order to run.")));
        }
    }
}
 
