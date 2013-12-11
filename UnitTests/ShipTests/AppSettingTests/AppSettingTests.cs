using NUnit.Framework;
using StarTrek_KG.Config;

namespace UnitTests.ShipTests.AppSettingTests
{
    [TestFixture]
    public class AppSettingTests
    {
        /// <summary>
        ///  This fixture keeps track of default Settings
        /// </summary>
        [SetUp]
        public void Setup()
        {
            StarTrekKGSettings.Get = StarTrekKGSettings.GetConfig();
        }

        [Test]
        public void repairEnergy()
        {
            Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        }

        [Test]
        public void QuadrantsNoHostileShips()
        {
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("QuadrantsNoHostileShips"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("QuadrantsNotSetUp"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("QuadrantsNeedToBeSetup1"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DebugNoSetUpSectorsInQuadrant"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DebugSettingUpPlayership"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DebugAddingNewQuadrant"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("InvalidPlayershipSetup"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("ErrorPlayershipSetup"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DebugModeEnd"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("NoSectorDefsSetUp"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("NavigationNotSetUp"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("TheCurrentLocation"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("InvalidXCoordinate"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("InvalidYCoordinate"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DestinationQuadrantY"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DestinationQuadrantX"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("LocatedInQuadrant"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("MaxWarpFactorMessage"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("WarpEnginesRepaired"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("WarpEnginesDamaged"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("BoundsHigh"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("BoundsLow"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DebugMode"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("KeepPlaying"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("PlayerShip"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("Hostile"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("stardate"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("energy"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("shieldEnergy"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("photonTorpedoes"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("timeRemaining"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("starbases"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("totalHostiles"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("BadGuy1Name"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("MaxWarpFactor"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("LowEnergyLevel"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("ShieldsDownLevel"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("SECTOR_MIN"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("SECTOR_MAX"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("QuadrantMax"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("CommandPrompt"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("CommandPromptNavigation"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DisruptorShotSeed"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DisruptorShotDeprecationLevel"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DisruptorEnergyAdjustment"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("DamageSeed"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("quadrantX"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("quadrantY"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("sectorX"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("sectorY"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("warpDriveDamage"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("shortRangeScanDamage"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("longRangeScanDamage"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("shieldControlDamage"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("computerDamage"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("photonDamage"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("phaserDamage"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("shieldLevel"));
            //Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("docked")); 
        }

        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, StarTrekKGSettings.GetSetting<int>("repairEnergy"));
        //}
    }
}
