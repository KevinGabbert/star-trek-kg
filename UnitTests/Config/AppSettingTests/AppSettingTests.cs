using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Config.AppSettingTests
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
            new StarTrekKGSettings().Get = new StarTrekKGSettings().GetConfig();
        }

        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}


        [Test]
        public void repairEnergy()
        {
            Assert.AreEqual(2001, new StarTrekKGSettings().GetSetting<int>("repairEnergy"));
        }

        [Test]
        public void timeRemaining_Default_Is_72()
        {
            Assert.AreEqual(75, new StarTrekKGSettings().GetSetting<int>("timeRemaining"));
        }

        [Test]
        public void totalHostiles_Default_Is_25()
        {
            Assert.AreEqual(25, new StarTrekKGSettings().GetSetting<int>("totalHostiles"));
        }

        [Test]
        public void Romulan_Faction_Has_Configured_Ship_Names()
        {
            var shipNames = new StarTrekKGSettings().ShipNames(FactionName.Romulan);

            Assert.That(shipNames, Does.Contain("IRW Valdore"));
            Assert.That(shipNames, Does.Contain("IRW Haakona"));
            Assert.That(shipNames, Does.Contain("RIS D'deridex"));
            Assert.That(shipNames, Does.Contain("IRW Intrakhu"));
            Assert.That(shipNames, Does.Contain("IRW T'seren"));
            Assert.That(shipNames.Count, Is.GreaterThanOrEqualTo(75));
        }

        [Test]
        public void SectorsNoHostileShips()
        {
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SectorsNoHostileShips"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SectorsNotSetUp"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SectorsNeedToBeSetup1"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DebugNoSetUpCoordinatesInSector"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DebugSettingUpPlayership"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DebugAddingNewSector"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("InvalidPlayershipSetup"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("ErrorPlayershipSetup"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DebugModeEnd"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("NoCoordinateDefsSetUp"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("NavigationNotSetUp"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("TheCurrentLocation"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("InvalidXCoordinate"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("InvalidYCoordinate"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DestinationSectorY"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DestinationSectorX"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("LocatedInSector"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("MaxWarpFactorMessage"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("WarpEnginesRepaired"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("WarpEnginesDamaged"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("BoundsHigh"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("BoundsLow"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DebugMode"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("KeepPlaying"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("PlayerShip"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("Hostile"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("stardate"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("energy"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("shieldEnergy"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("photonTorpedoes"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("timeRemaining"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("starbases"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("totalHostiles"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("BadGuy1Name"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("MaxWarpFactor"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("LowEnergyLevel"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("ShieldsDownLevel"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SECTOR_MIN"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SECTOR_MAX"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SECTOR_MIN"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SECTOR_MAX"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("CommandPrompt"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("CommandPromptNavigation"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DisruptorShotSeed"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DisruptorShotDeprecationLevel"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DisruptorEnergyAdjustment"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("DamageSeed"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SectorX"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("SectorY"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("sectorX"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("sectorY"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("warpDriveDamage"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("shortRangeScanDamage"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("longRangeScanDamage"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("shieldControlDamage"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("computerDamage"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("photonDamage"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("phaserDamage"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("shieldLevel"));
            //Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("docked")); 
        }

        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
        //[Test]
        //public void repairEnergy()
        //{
        //    Assert.AreEqual(2001, (new StarTrekKGSettings()).GetSetting<int>("repairEnergy"));
        //}
    }
}
