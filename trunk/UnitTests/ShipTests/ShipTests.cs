using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests
{
    [TestFixture]
    public class ShipTests
    {
        //todo: verify that all subsystems are set up.
        //todo: verify defaults, such as starting sector
        //todo: verify config settings are pulled.
        //todo: verify RepairEverything values for each subsystem
        //todo: verify that verifySubsystem repairs one subsystem at a time
        //todo: verify allegiance when ship is created as friendly, and hostile
        //todo: 

        /// <summary>
        /// The only point behind this test was to understand the limit.
        /// Apparently, currently using a hardcoded seed of 300, and a distanceDeprecationLevel of 11.3
        /// </summary>
        [Test]
        public void DisruptorShot()
        {
            Assert.AreEqual(300, StarTrekKGSettings.GetSetting<int>("DisruptorShotSeed"));
            Assert.AreEqual(11.3, StarTrekKGSettings.GetSetting<double>("DisruptorShotDeprecationLevel"));
            Assert.AreEqual(1.0, StarTrekKGSettings.GetSetting<double>("DisruptorEnergyAdjustment"));

            var disruptors = new Disruptors(null);

            for (var i = 1; i < 1000; i++)
            {
                var oneSector = disruptors.Shoot(-1, 1);
                var twoSector = disruptors.Shoot(-1, 2);
                var fourSector = disruptors.Shoot(-1, 4);
                var sevenSector = disruptors.Shoot(-1, 7);
                var eightSector = disruptors.Shoot(-1, 8);

                Assert.Less(oneSector, 290, "iteration: " + i);
                Assert.Less(twoSector, 247, "iteration: " + i);
                Assert.Less(fourSector, 194, "iteration: " + i);
                Assert.Less(sevenSector, 115, "iteration: " + i);
                Assert.Less(eightSector, 88, "iteration: " + i);
            }
        }
    }
}
