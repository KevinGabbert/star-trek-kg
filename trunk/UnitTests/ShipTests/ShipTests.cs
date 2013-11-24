﻿using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

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
        /// ** This test will break once those values are pulled from config **
        /// (lets hope that the config settings never get pulled *in* this function, as then the test will go very slow)
        /// </summary>
        [Test]
        public void DisruptorShot()
        {
            Assert.AreEqual(300, AppConfig.Setting<int>("DisruptorShotSeed"));
            Assert.AreEqual(11.3, AppConfig.Setting<double>("DisruptorShotDeprecationLevel"));
            Assert.AreEqual(1.0, AppConfig.Setting<double>("DisruptorEnergyAdjustment"));

            for (var i = 1; i < 100000; i++)
            {
                var oneSector = Ship.DisruptorShot(1);
                var twoSector = Ship.DisruptorShot(2);
                var fourSector = Ship.DisruptorShot(4);
                var sevenSector = Ship.DisruptorShot(7);
                var eightSector = Ship.DisruptorShot(8);

                Assert.Less(oneSector, 290, "iteration: " + i);
                Assert.Less(twoSector, 247, "iteration: " + i);
                Assert.Less(fourSector, 194, "iteration: " + i);
                Assert.Less(sevenSector, 115, "iteration: " + i);
                Assert.Less(eightSector, 88, "iteration: " + i);
            }
        }
    }
}
