using NUnit.Framework;
using StarTrek_KG.Config;

namespace UnitTests.Tests
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
            Assert.AreEqual(300, new StarTrekKGSettings().GetSetting<int>("DisruptorShotSeed"));
            Assert.AreEqual(11.3, new StarTrekKGSettings().GetSetting<double>("DisruptorShotDeprecationLevel"));
            Assert.AreEqual(1.0, new StarTrekKGSettings().GetSetting<double>("DisruptorEnergyAdjustment"));

            for (var i = 1; i < 1000; i++)
            {
                var oneSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 1, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", false);
                var twoSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 2, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", false);
                var fourSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 4, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", false);
                var sevenSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 7, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", false);
                var eightSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 8, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", false);

                Assert.AreEqual(oneSector, 273, "iteration: " + i);
                Assert.AreEqual(twoSector, 246, "iteration: " + i);
                Assert.AreEqual(fourSector, 193, "iteration: " + i);
                Assert.AreEqual(sevenSector, 114, "iteration: " + i);
                Assert.AreEqual(eightSector, 87, "iteration: " + i);
            }
        }

        /// <summary>
        /// The only point behind this test was to understand the limit.
        /// Apparently, currently using a hardcoded seed of 300, and a distanceDeprecationLevel of 11.3
        /// </summary>
        [Test]
        public void DisruptorShotInNebula()
        {
            Assert.AreEqual(300, new StarTrekKGSettings().GetSetting<int>("DisruptorShotSeed"));
            Assert.AreEqual(11.3, new StarTrekKGSettings().GetSetting<double>("DisruptorShotDeprecationLevel"));
            Assert.AreEqual(1.0, new StarTrekKGSettings().GetSetting<double>("DisruptorEnergyAdjustment"));

            for (var i = 1; i < 1000; i++)
            {
                var oneSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 1, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", true);
                var twoSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 2, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", true);
                var fourSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 4, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", true);
                var sevenSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 7, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", true);
                var eightSector = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(300, 8, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", true);

                Assert.AreEqual(oneSector, 246, "iteration: " + i);
                Assert.AreEqual(twoSector, 193, "iteration: " + i);
                Assert.AreEqual(fourSector, 87, "iteration: " + i);
                Assert.AreEqual(sevenSector, 0, "iteration: " + i);
                Assert.AreEqual(eightSector, 0, "iteration: " + i);
            }
        }
    }
}
