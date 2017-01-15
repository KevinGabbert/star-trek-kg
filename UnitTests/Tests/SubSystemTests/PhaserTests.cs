using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class PhaserTests: TestClass_Base
    {
        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
        }

        [TearDown]
        public void TearDown()
        {
            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;

            DEFAULTS.REGION_MIN = 0;
            DEFAULTS.REGION_MAX = 0;
        }

        [Test]
        public void PhaserFireSubtractsEnergyFromShip2()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Coordinate(2, 1));

            var startingEnergy = (new StarTrekKGSettings()).GetSetting<double>("energy"); ;
            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);

            const int testBoltEnergy = 89;

            //This action will hit every single hostile in the Region.  In this case, it will hit no one  :D
            Phasers.For(_setup.TestMap.Playership).Fire(testBoltEnergy);

            this.VerifyFiringShipIntegrity(_setup.TestMap.Playership, startingEnergy, testBoltEnergy, 1911);

            //Verifies energy subtracted from firing ship.
            Assert.AreEqual(startingEnergy - testBoltEnergy, _setup.TestMap.Playership.Energy);
        }

        [Test]
        public void PhaserFireSubtractsEnergyFromShip()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Coordinate(2,1));

            var startingEnergy = (new StarTrekKGSettings()).GetSetting<double>("energy");;
            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);

            const int testBoltEnergy = 89;

            //This action will hit every single hostile in the Region.  In this case, it will hit no one  :D
            Phasers.For(_setup.TestMap.Playership).Fire(testBoltEnergy);

            this.VerifyFiringShipIntegrity(_setup.TestMap.Playership, startingEnergy, testBoltEnergy, 1911);

            //Verifies energy subtracted from firing ship.
            Assert.AreEqual(startingEnergy - testBoltEnergy, _setup.TestMap.Playership.Energy);
        }

        [Test]
        public void PhasersWontFireWhenToldTooMuchEnergy()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Coordinate(2, 1));

            var ship = _setup.TestMap.Playership;

            var startingEnergy = (new StarTrekKGSettings()).GetSetting<int>("energy");
            Assert.AreEqual(startingEnergy, ship.Energy);

            const int testBoltEnergy = 4000;
            var beforeEnergy = ship.Energy;

            //This action will hit every single hostile in the Region.  In this case, it will hit no one  :D
            Phasers.For(_setup.TestMap.Playership).Fire(testBoltEnergy);

            Assert.AreEqual(ship.Energy, beforeEnergy); //verify that no energy xfer happened.

            Assert.Greater(Shields.For(ship).Energy, -1);

            //Todo: Mock up Output so we can see the text result
            //Without a mock for Output, we can't see the output, but the conclusion we can draw here is that the phasers didn't fire, and no energy was expended

            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);
        }

        [Test]
        public void PhasersWontFireWhenToldNotEnoughEnergy()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Coordinate(2, 1));

            var startingEnergy = (new StarTrekKGSettings()).GetSetting<int>("energy"); ;
            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);

            const int testBoltEnergy = -1;

            //This action will hit every single hostile in the Region.  In this case, it will hit no one  :D
            Phasers.For(_setup.TestMap.Playership).Fire(testBoltEnergy);

            this.VerifyFiringShipIntegrity(_setup.TestMap.Playership, startingEnergy, testBoltEnergy, startingEnergy);

            //Todo: Mock up Output so we can see the text result
            //Without a mock for Output, we can't see the output, but the conclusion we can draw here is that the phasers didn't fire, and no energy was expended

            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);
        }

        [Test]
        public void HitThatDestroys_NonNebula()
        {
            //TestClass_Base called before this, doing some initialization a second time
            _setup.SetupMapWith1HostileAtSector(new Coordinate(2, 1), new Coordinate(2,6), true);

            //todo: why active? are hostiles in the same sector?
            var activeRegion = _setup.TestMap.Regions.GetActive();
            activeRegion.Type = RegionType.GalacticSpace; // for testing purposes.

            var beforeHostiles = activeRegion.GetHostiles();
            var countOfHostiles = beforeHostiles.Count;

            Assert.AreEqual(1, countOfHostiles);

            //Verify ship's location
            Assert.AreEqual(2, beforeHostiles[0].Sector.X);
            Assert.AreEqual(6, beforeHostiles[0].Sector.Y);

            //verify position on map.
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[22].Item);

            //set badguy energy
            var badGuyShields = Shields.For(beforeHostiles[0]);
            badGuyShields.Energy = 50;

            //todo: verify firing ship's starting energy.
            var startingEnergy = (new StarTrekKGSettings()).GetSetting<int>("energy");

            var playershipBefore = _setup.TestMap.Playership;

            Assert.AreEqual(startingEnergy, playershipBefore.Energy);

            const int testBoltEnergy = 92;

            //todo: wait.. what? why do I need to specify who is firing??

            //Random numbers that are used in this operation:
            var phasers = Phasers.For(playershipBefore);
            phasers.ShipConnectedTo.Map.Game.RandomFactorForTesting = 3;

            playershipBefore.Map.Game = phasers.ShipConnectedTo.Map.Game;
            Torpedoes.For(playershipBefore).ShipConnectedTo.Map.Game.RandomFactorForTesting = 2;

            badGuyShields.ShipConnectedTo.Map.Game.RandomFactorForTesting = 200;

            //This action will hit every single hostile in the Region
            phasers.Fire(testBoltEnergy);  //due to the distance between the 2 ships, this is how much power it takes to knock the hostile's shield level of 50 down to nothing.

            var playershipAfter = _setup.TestMap.Playership;

            this.VerifyFiringShipIntegrity(playershipAfter, startingEnergy, testBoltEnergy, 1743);

            var afterHostiles = activeRegion.GetHostiles();
            var afterHostilesCount = afterHostiles.Count;

            //in space. no one can hear you scream.
            Assert.AreEqual(0, afterHostilesCount);
            Assert.AreEqual(null, activeRegion.Sectors[22].Object);
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[22].Item);
        }

        [Test]
        public void HitThatDestroys_Nebula()
        {
            //TestClass_Base called before this, doing some initialization a second time
            _setup.SetupMapWith1HostileAtSector(new Coordinate(2, 1), new Coordinate(2, 6), true);
            Game.RandomFactorForTesting = 123;

            //todo: why active? are hostiles in the same sector?
            var activeRegion = _setup.TestMap.Regions.GetActive();
            activeRegion.Type = RegionType.Nebulae; // for testing purposes.

            var beforeHostiles = activeRegion.GetHostiles();
            var countOfHostiles = beforeHostiles.Count;

            Assert.AreEqual(1, countOfHostiles);

            //Verify ship's location
            Assert.AreEqual(2, beforeHostiles[0].Sector.X);
            Assert.AreEqual(6, beforeHostiles[0].Sector.Y);

            //verify position on map.
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[22].Item);

            //set badguy energy
            var badGuyShields = Shields.For(beforeHostiles[0]);
            badGuyShields.Energy = 50;

            //todo: verify firing ship's starting energy.
            var startingEnergy = (new StarTrekKGSettings()).GetSetting<int>("energy");

            var playershipBefore = _setup.TestMap.Playership;

            Assert.AreEqual(startingEnergy, playershipBefore.Energy);

            const int testBoltEnergy = 444;

            //Random numbers that are used in this operation:
            var phasers = Phasers.For(playershipBefore);
            phasers.ShipConnectedTo.Map.Game.RandomFactorForTesting = 3;

            playershipBefore.Map.Game = phasers.ShipConnectedTo.Map.Game;
            Torpedoes.For(playershipBefore).ShipConnectedTo.Map.Game.RandomFactorForTesting = 2;

            badGuyShields.ShipConnectedTo.Map.Game.RandomFactorForTesting = 200;

            //This action will hit every single hostile in the Region
            phasers.Fire(testBoltEnergy);  //due to the distance between the 2 ships, this is how much power it takes to knock the hostile's shield level of 50 down to nothing.

            var playershipAfter = _setup.TestMap.Playership;

            this.VerifyFiringShipIntegrity(playershipAfter, startingEnergy, testBoltEnergy, 1524);

            var afterHostiles = activeRegion.GetHostiles();
            var afterHostilesCount = afterHostiles.Count;

            //in space. no one can hear you scream.
            Assert.AreEqual(0, afterHostilesCount);
            Assert.AreEqual(null, activeRegion.Sectors[22].Object);
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[22].Item);
        }

        [Ignore("")]
        [Test]
        public void FirePhasersFromConsole()
        {
            //todo: a mock needed for this.  One that taps into the appropriate event.
            //Phasers.For(_testMap.Playership).Controls(_testMap);
        }

        public void HitThatWounds()
        {
            //_testMap.Regions.GetHostile(0).Shields = 20

            //todo: ensure that baddie has less than 50 (from config?)
            Phasers.For(_setup.TestMap.Playership).Fire(50); 
        }

        public void HitThatWoundsMultipleHostiles()
        {
            //add a hostile before starting test
        }

        public void VerifyFiringShipIntegrity(IShip firingShip, double startingEnergy, int testBoltEnergy, int expectedFiringShipAfterEnergy)
        {
            //Verifies energy subtracted from firing ship.  (greater, because of auto-salvage operation)
            //Assert.GreaterOrEqual(startingEnergy - testBoltEnergy, firingShip.Energy);

            //todo: calculate distance and damage that will be dealt 

            //int seedEnergyToPowerWeapon = this.Config.GetSetting<int>("DisruptorShotSeed") * (Game.RandomFactorForTesting).Next();

            //var attackingEnergy = (int)StarTrek_KG.Utility.Utility.ShootBeamWeapon(seedEnergyToPowerWeapon, distance, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", inNebula); 

            var firingShipEnergy = firingShip.Energy;
            
            Assert.AreEqual(firingShipEnergy, expectedFiringShipAfterEnergy, " Firing Ship: " + firingShip.Name + " expected energy: " + expectedFiringShipAfterEnergy + ". but was " + firingShipEnergy);

            //todo: verify that firing ship was not hit.
            Assert.Greater(Shields.For(firingShip).Energy, -1);
        } 
    }
}
