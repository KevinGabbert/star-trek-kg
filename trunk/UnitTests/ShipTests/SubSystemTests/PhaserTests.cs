using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class PhaserTests: TestClass_Base
    {
        private Map _testMap;


        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();

            _testMap = (new Map(new GameConfig
                                    {
                                        Initialize = true,
                                        
                                        
                                        SectorDefs = new SectorDefs
                                                         {
                                                             new SectorDef(
                                                                 new LocationDef(new Coordinate(0, 0),
                                                                                 new Coordinate(0, 1)),
                                                                 SectorItem.Friendly),
                                                             new SectorDef(
                                                                 new LocationDef(new Coordinate(0, 0),
                                                                                 new Coordinate(0, 3)),
                                                                 SectorItem.Hostile)
                                                         },
                                        AddStars = false
                                    }, this.Write));
        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
        }

        [Test]
        public void PhaserFireSubtractsEnergyFromShip()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                    },
                AddStars = false
            }, this.Write));

            var startingEnergy = StarTrekKGSettings.GetSetting<double>("energy");;
            Assert.AreEqual(startingEnergy, _testMap.Playership.Energy);

            const double testBoltEnergy = 89.6829;

            //This action will hit every single hostile in the quadrant.  In this case, it will hit no one  :D
            Phasers.For(_testMap.Playership).Fire(testBoltEnergy, _testMap.Playership); 

            //Verifies energy subtracted from firing ship.
            Assert.AreEqual(startingEnergy - testBoltEnergy, _testMap.Playership.Energy);
        }

        [Test]
        public void PhasersWontFireWhenToldTooMuchEnergy()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                    },
                AddStars = false
            }, this.Write));

            var startingEnergy = StarTrekKGSettings.GetSetting<double>("energy"); 
            Assert.AreEqual(startingEnergy, _testMap.Playership.Energy);

            const double testBoltEnergy = 4000;

            //This action will hit every single hostile in the quadrant.  In this case, it will hit no one  :D
            Phasers.For(_testMap.Playership).Fire(testBoltEnergy, _testMap.Playership);

            //Todo: Mock up Output so we can see the text result
            //Without a mock for Output, we can't see the output, but the conclusion we can draw here is that the phasers didn't fire, and no energy was expended

            Assert.AreEqual(startingEnergy, _testMap.Playership.Energy);
        }

        [Test]
        public void PhasersWontFireWhenToldNotEnoughEnergy()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                    },
                AddStars = false
            }, this.Write));

            var startingEnergy = StarTrekKGSettings.GetSetting<double>("energy"); ;
            Assert.AreEqual(startingEnergy, _testMap.Playership.Energy);

            const double testBoltEnergy = -1;

            //This action will hit every single hostile in the quadrant.  In this case, it will hit no one  :D
            Phasers.For(_testMap.Playership).Fire(testBoltEnergy, _testMap.Playership);

            //Todo: Mock up Output so we can see the text result
            //Without a mock for Output, we can't see the output, but the conclusion we can draw here is that the phasers didn't fire, and no energy was expended

            Assert.AreEqual(startingEnergy, _testMap.Playership.Energy);
        }


        [Test]
        public void HitThatDestroys()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 6)), SectorItem.Hostile)
                    },
                AddStars = false
            }, this.Write));

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = _testMap.Quadrants.GetActive();

            Assert.AreEqual(1, activeQuadrant.GetHostiles().Count);

            //Verify ship's location
            Assert.AreEqual(2, activeQuadrant.GetHostiles()[0].Sector.X);
            Assert.AreEqual(6, activeQuadrant.GetHostiles()[0].Sector.Y);

            //verify position on map.
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[22].Item);

            //set its energy
            Shields.For(activeQuadrant.GetHostiles()[0]).Energy = 50;

            //todo: verify firing ship's starting energy.

            var startingEnergy = StarTrekKGSettings.GetSetting<double>("energy");

            Assert.AreEqual(startingEnergy, _testMap.Playership.Energy);

            const double testBoltEnergy = 89.6829;

            //This action will hit every single hostile in the quadrant
            Phasers.For(_testMap.Playership).Fire(testBoltEnergy, _testMap.Playership); //due to the distance between the 2 ships, this is how much power it takes to knock the hostile's shield level of 50 down to nothing.

            //Verifies energy subtracted from firing ship.
            Assert.AreEqual(startingEnergy - testBoltEnergy, _testMap.Playership.Energy);

            //in space. no one can hear you scream.
            Assert.AreEqual(0, activeQuadrant.GetHostiles().Count);
            Assert.AreEqual(null, activeQuadrant.Sectors[22].Object);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[22].Item);
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
            //_testMap.Quadrants.GetHostile(0).Shields = 20

            //todo: ensure that baddie has less than 50 (from config?)
            Phasers.For(_testMap.Playership).Fire(50, _testMap.Playership);
        }

        public void HitThatWoundsMultipleHostiles()
        {
            //add a hostile before starting test
        }
    }
}
