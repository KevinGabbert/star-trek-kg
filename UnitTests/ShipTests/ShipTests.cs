using NUnit.Framework;
using StarTrek_KG.Config;

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

            for (var i = 1; i < 1000; i++)
            {
                var oneSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 1, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment");
                var twoSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 2, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment");
                var fourSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 4, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment");
                var sevenSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 7, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment");
                var eightSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 8, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment");

                Assert.Less(oneSector, 290, "iteration: " + i);
                Assert.Less(twoSector, 247, "iteration: " + i);
                Assert.Less(fourSector, 194, "iteration: " + i);
                Assert.Less(sevenSector, 115, "iteration: " + i);
                Assert.Less(eightSector, 88, "iteration: " + i);
            }
        }
    }
}
