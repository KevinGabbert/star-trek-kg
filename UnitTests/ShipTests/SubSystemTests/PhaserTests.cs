using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
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

            _setup.SetupMapWith1HostileAtSector(new Coordinate(0, 0), new Coordinate(0, 3));
        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.Region_MIN = 0;
            Constants.Region_MAX = 0;
        }

        [Test]
        public void PhaserFireSubtractsEnergyFromShip2()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Coordinate(2, 1));

            var startingEnergy = (new StarTrekKGSettings()).GetSetting<double>("energy"); ;
            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);

            const int testBoltEnergy = 89;

            var phasers = new Phasers(_setup.TestMap.Playership, this.Game);

            //This action will hit every single hostile in the Region.  In this case, it will hit no one  :D
            phasers.Fire(testBoltEnergy, _setup.TestMap.Playership);

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
            Phasers.For(_setup.TestMap.Playership).Fire(testBoltEnergy, _setup.TestMap.Playership); 

            //Verifies energy subtracted from firing ship.
            Assert.AreEqual(startingEnergy - testBoltEnergy, _setup.TestMap.Playership.Energy);
        }

        [Test]
        public void PhasersWontFireWhenToldTooMuchEnergy()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Coordinate(2, 1));

            var startingEnergy = (new StarTrekKGSettings()).GetSetting<double>("energy"); 
            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);

            const int testBoltEnergy = 4000;

            //This action will hit every single hostile in the Region.  In this case, it will hit no one  :D
            Phasers.For(_setup.TestMap.Playership).Fire(testBoltEnergy, _setup.TestMap.Playership);

            //Todo: Mock up Output so we can see the text result
            //Without a mock for Output, we can't see the output, but the conclusion we can draw here is that the phasers didn't fire, and no energy was expended

            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);
        }

        [Test]
        public void PhasersWontFireWhenToldNotEnoughEnergy()
        {
            _setup.SetupMapWith1FriendlyAtSector(new Coordinate(2, 1));

            var startingEnergy = (new StarTrekKGSettings()).GetSetting<double>("energy"); ;
            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);

            const int testBoltEnergy = -1;

            //This action will hit every single hostile in the Region.  In this case, it will hit no one  :D
            Phasers.For(_setup.TestMap.Playership).Fire(testBoltEnergy, _setup.TestMap.Playership);

            //Todo: Mock up Output so we can see the text result
            //Without a mock for Output, we can't see the output, but the conclusion we can draw here is that the phasers didn't fire, and no energy was expended

            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);
        }


        [Test]
        public void HitThatDestroys()
        {
            _setup.SetupMapWith1HostileAtSector(new Coordinate(2, 1), new Coordinate(2,6));

            //todo: why active? are hostiles in the same sector?
            var activeRegion = _setup.TestMap.Regions.GetActive();

            Assert.AreEqual(1, activeRegion.GetHostiles().Count);

            //Verify ship's location
            Assert.AreEqual(2, activeRegion.GetHostiles()[0].Sector.X);
            Assert.AreEqual(6, activeRegion.GetHostiles()[0].Sector.Y);

            //verify position on map.
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[22].Item);

            //set its energy
            Shields.For(activeRegion.GetHostiles()[0]).Energy = 50;

            //todo: verify firing ship's starting energy.

            var startingEnergy = (new StarTrekKGSettings()).GetSetting<double>("energy");

            Assert.AreEqual(startingEnergy, _setup.TestMap.Playership.Energy);

            const int testBoltEnergy = 89;

            //This action will hit every single hostile in the Region
            Phasers.For(_setup.TestMap.Playership).Fire(testBoltEnergy, _setup.TestMap.Playership); //due to the distance between the 2 ships, this is how much power it takes to knock the hostile's shield level of 50 down to nothing.

            //Verifies energy subtracted from firing ship.  (greater, because of auto-salvage operation)
            Assert.GreaterOrEqual(startingEnergy - testBoltEnergy, _setup.TestMap.Playership.Energy);

            //in space. no one can hear you scream.
            Assert.AreEqual(0, activeRegion.GetHostiles().Count);
            Assert.AreEqual(null, activeRegion.Sectors[22].Object);
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[22].Item);
        }

        [Ignore]
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
            Phasers.For(_setup.TestMap.Playership).Fire(50, _setup.TestMap.Playership);
        }

        public void HitThatWoundsMultipleHostiles()
        {
            //add a hostile before starting test
        }
    }
}
