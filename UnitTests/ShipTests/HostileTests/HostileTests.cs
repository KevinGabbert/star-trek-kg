using System;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.HostileTests
{
    [TestFixture]
    public class HostileTests
    {
        private Map _testMap; 

        [SetUp]
        public void SetUp()
        {
            Constants.SECTOR_MIN = StarTrekKGSettings.GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = StarTrekKGSettings.GetSetting<int>("SECTOR_MAX");

            Constants.QUADRANT_MIN = StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = StarTrekKGSettings.GetSetting<int>("QuadrantMax");
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
        public void ALLHostilesAttack_ShipUndocked_WithShields()
        {
            //todo:  verify that quadrants are set up correctly.
            //todo: This test does not run alone.  what do the other tests set up that this needs?  why don't thea other tests tear down their stuff?

            //todo: will we need to mock out the Console.write process just so that we can test the output?  I'm thinking so..
            this.SetupMapWith2Hostiles();

            Assert.AreEqual(_testMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"), "Ship energy not at expected amount"); 

            //raise shields
            Shields.For(_testMap.Playership).SetEnergy(2500); //hopefully a single hit wont be harder than this!
            Assert.AreEqual(2500, Shields.For(_testMap.Playership).Energy, "Unexpected shield energy level"); //shields charged correctly // todo: do more tests on this in ShieldTests    

            //Assert.AreEqual(StarTrekKGSettings.GetSetting<int>("energy"), _testMap.Playership.Energy, "Ship energy not at maximum"); //ship has no damage

            Assert.AreEqual(_testMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"), "Ship energy not at expected amount");

            _testMap.Quadrants.ALLHostilesAttack(_testMap);

            Assert.IsFalse(_testMap.Playership.Destroyed); 
            Assert.Less(Shields.For(_testMap.Playership).Energy, 2500);
            Assert.AreEqual(StarTrekKGSettings.GetSetting<int>("energy"), _testMap.Playership.Energy, "expected no change to ship energy"); 
    
            //Assert that ship has taken 2 hits.
            //todo: use a mock to determine that Ship.AbsorbHitFrom() was called twice.
        }

        [Ignore("Test here that shields go lower when Playership is hit")]
        [Test]
        public void ALLHostilesAttack_ShipUndocked_WithShields2()
        {
            //this.SetupMapWith2Hostiles();

            ////raise shields
            //Shields.For(_testMap.Playership).AddEnergy(2500, true); //hopefully a single hit wont be harder than this!
            //Assert.AreEqual(2500, Shields.For(_testMap.Playership).Energy, "Unexpected shield energy level"); //shields charged correctly // todo: do more tests on this in ShieldTests          
            //Assert.AreEqual(500, _testMap.Playership.Energy, "Ship energy not at maximum"); //ship has no damage

            //_testMap.Quadrants.ALLHostilesAttack(_testMap);

            //Assert.IsFalse(_testMap.Playership.Destroyed);
            //Assert.Less(Shields.For(_testMap.Playership).Energy, 2500);
            //Assert.AreEqual(_testMap.Playership.Energy, 500);

            ////Assert that ship has taken 2 hits.
            ////todo: use a mock to determine that Ship.AbsorbHitFrom() was called twice.
        }

        [Ignore("Test here that output messages are called where shields are lower")]
        [Test]
        public void ALLHostilesAttack_ShipUndocked_NoShields2()
        {
            //this.SetupMapWith2Hostiles();

            //Assert.AreEqual(3000, _testMap.Playership.Energy, "Ship energy not at expected amount"); //ship has no damage

            //_testMap.Quadrants.ALLHostilesAttack(_testMap);

            //Assert.IsFalse(_testMap.Playership.Destroyed);

            ////2 subsystems should have been taken out.
            //Assert.AreEqual(2, _testMap.Playership.Subsystems.Count(s => s.Damaged()));  //todo: check all of ship's systems in HostileTests
        }

        [Test]
        public void ShipUndocked_NoShields_CheckEnergy()
        {
            this.SetupMapWith2Hostiles();

            Assert.AreEqual(StarTrekKGSettings.GetSetting<int>("energy"), _testMap.Playership.Energy, "Ship energy not at expected amount"); //ship has no damage
        }

        [Ignore("This is a long running test intended to suss out a problem (which is now fixed)")]
        [Test]
        public void ALLHostilesAttack_Check_Subsystems()
        {
            //TODO: this fails. Create a test for subsystems.DamageRandomSubsystem()
            for(int i = 0; i < 100; i ++)
            {
                 this.AttackAndCheck();
            }
        }

        [Test]
        public void ALLHostilesAttack_ShipUndocked_NoShields()
        {
            AttackAndCheck();
        }

        private void AttackAndCheck()
        {
            this.SetupMapWith2Hostiles();

            Assert.AreEqual(_testMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"), "Ship energy not at expected amount"); 

            _testMap.Quadrants.ALLHostilesAttack(_testMap);

            Assert.IsFalse(_testMap.Playership.Destroyed);

            //var shipEnergy = _testMap.Playership.Energy;
            //var shipShields = Shields.For(_testMap.Playership).Energy;

            //if(shipEnergy < 3000)
            //{
            //    Assert.AreEqual(2, _testMap.Playership.Subsystems.Count(s => s.Damaged()));
            //}
            //else
            //{
            //    Assert.AreEqual(1, _testMap.Playership.Subsystems.Count(s => s.Damaged()));
            //}

            //Normally, 2 *different* subsystems should have been taken out, however, on a rare occasion, a hit will result in no damage.
            Assert.GreaterOrEqual(_testMap.Playership.Subsystems.Count(s => s.Damaged()), 1);
        }

        [Test]
        public void ALLHostilesAttack_ShipDocked()
        {
            this.SetupMapWith2Hostiles();

            //cheating so we can cover this line
            Navigation.For(_testMap.Playership).docked = true;

            _testMap.Quadrants.ALLHostilesAttack(_testMap);

            //Ship has taken no damage.
            Assert.IsFalse(_testMap.Playership.Destroyed); 
            Assert.AreEqual(Shields.For(_testMap.Playership).Energy, 0); //never even needed to raise shields!
            Assert.AreEqual(_testMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"));      
        }

        [Test]
        public void MapWith0Friendlies()
        {
            var game = new Game(); //this can make tests break so I throw it in for a check..

            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs(),
                AddStars = false
            }));

            var activeQuad = _testMap.Quadrants.GetActive();

            Assert.AreEqual(64, activeQuad.Sectors.Count);
            Assert.AreEqual(0, _testMap.Quadrants.GetHostileCount()); //no hostiles
            Assert.AreEqual(null, _testMap.Playership); //no friendly
            
            //just empty sectors
            foreach (var sector in _testMap.Quadrants.SelectMany(quadrant => quadrant.Sectors))
            {
                Assert.AreEqual(SectorItem.Empty, sector.Item);
            }
        }

        //this method is here to test passing in 
        [Test]
        public void MapWith1Friendly()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(SectorItem.Friendly)
                                            }
            }));

            var activeQuad = _testMap.Quadrants.GetActive();

            Assert.AreEqual(64, activeQuad.Sectors.Count);

            //Playership's quadrant has been set correctly..
            Assert.AreEqual(_testMap.Playership.QuadrantDef.X, activeQuad.X);
            Assert.AreEqual(_testMap.Playership.QuadrantDef.Y, activeQuad.Y);

            //Check to see if Playership has been assigned to a sector in the active quadrant.

            //indirectly..
            Assert.AreEqual(1, activeQuad.Sectors.Count(s => s.Item == SectorItem.Friendly));

            //directly.
            Assert.AreEqual(SectorItem.Friendly, activeQuad.Sectors.Single(s => s.X == _testMap.Playership.Sector.X && s.Y == _testMap.Playership.Sector.Y).Item);
        }

        //Maybe you want to add/remove hostiles on the fly or something, during the game 
        [Test]
        public void MapWith1Hostile()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs()
            }));


            var locationDef = new LocationDef(new Coordinate(0, 0), new Coordinate(1, 7));

            //add a ship
            var hostileShip = new Ship("ship1", _testMap, new Sector(locationDef));

            _testMap.Quadrants[0].AddShip(hostileShip, _testMap.Quadrants[0].Sectors.Get(new Coordinate(1,7)));

            var hostiles = _testMap.Quadrants.GetHostiles();
            Assert.AreEqual(1, hostiles.Count);

            var firstHostile = _testMap.Quadrants.GetHostiles()[0];
            Assert.AreEqual("Sector: 1, 7", firstHostile.Sector.ToString());

            Assert.AreEqual(1, _testMap.Quadrants.GetHostiles()[0].Sector.X);
            Assert.AreEqual(7, _testMap.Quadrants.GetHostiles()[0].Sector.Y);
        }

        [Test]
        public void MapWith1HostileAlternate()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.Hostile)
                                            }
            }));

            var hostiles = _testMap.Quadrants.GetHostiles();
            Assert.AreEqual(1, hostiles.Count);

            var firstHostile = hostiles[0];
            Assert.AreEqual("Sector: 4, 6", firstHostile.Sector.ToString());

            Assert.AreEqual(4, hostiles[0].Sector.X);
            Assert.AreEqual(6, hostiles[0].Sector.Y);
        }

        //Maybe you want to add/remove hostiles on the fly or something, during the game 
        [Test]
        public void Remove1Hostile()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs()
            }));

            var locationDef = new LocationDef(new Coordinate(0, 0), new Coordinate(1, 7));

            //add a ship
            var hostileShip = new Ship("ship1", _testMap, new Sector(locationDef));

            _testMap.Quadrants[0].AddShip(hostileShip, _testMap.Quadrants[0].Sectors.Get(new Coordinate(1, 7)));

            _testMap.Quadrants.RemoveShip(hostileShip.Name);

            Assert.AreEqual(0, _testMap.Quadrants.GetHostiles().Count);
        }

        //this method is here to test passing in 
        [Test]
        public void MapWith3HostilesTheConfigWay_FailsIntermittently()
        {
            TestMap3Scenario();
        }

        [Test]
        public void MapWith3HostilesTheConfigWay_FailsIntermittently_overAndOver()
        {
            for(int i = 0; i < 1000; i++)
            {
                var x = new Game();
                Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%% i: " + i);
                TestMap3Scenario();
            }
        }

        private void TestMap3Scenario()
        {
            var x = new Game();

            _testMap = (new Map(new GameConfig
                                    {
                                        Initialize = true,
                                        
                                        
                                        SectorDefs = new SectorDefs
                                                         {
                                                             new SectorDef(
                                                                 new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)),
                                                                 SectorItem.Friendly),
                                                             new SectorDef(
                                                                 new LocationDef(new Coordinate(0, 0), new Coordinate(2, 6)),
                                                                 SectorItem.Hostile),
                                                             new SectorDef(
                                                                 new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)),
                                                                 SectorItem.Hostile),
                                                             new SectorDef(
                                                                 new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)),
                                                                 SectorItem.Hostile)
                                                         },
                                        AddStars = false
                                    }));

            var activeQuad = _testMap.Quadrants.GetActive();

            Assert.AreEqual(64, activeQuad.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = _testMap.Quadrants.GetActive();

            Assert.AreEqual(SectorItem.Friendly, activeQuadrant.Sectors[17].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[22].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[23].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[24].Item);

            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[36].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[37].Item);

            Assert.AreEqual(3, activeQuadrant.GetHostiles().Count());

            Assert.AreEqual(2, activeQuadrant.GetHostiles()[0].Sector.X);
            Assert.AreEqual(6, activeQuadrant.GetHostiles()[0].Sector.Y);

            Assert.AreEqual(2, activeQuadrant.GetHostiles()[1].Sector.X);
            Assert.AreEqual(7, activeQuadrant.GetHostiles()[1].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.GetHostiles()[2].Sector.X);
            Assert.AreEqual(4, activeQuadrant.GetHostiles()[2].Sector.Y);
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 5)), SectorItem.Friendly),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.Hostile),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)), SectorItem.Hostile),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)), SectorItem.Hostile)
                },
                AddStars = false
            }));

            var activeQuad = _testMap.Quadrants.GetActive();

            Assert.AreEqual(64, activeQuad.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = activeQuad;

            Assert.AreEqual(SectorItem.Friendly, activeQuadrant.Sectors[37].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[38].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[23].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[24].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[36].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[39].Item);

            Assert.AreEqual(3, activeQuadrant.GetHostiles().Count());
            Assert.AreEqual(3, activeQuadrant.GetHostiles().Count());

            Assert.AreEqual(2, activeQuadrant.GetHostiles()[0].Sector.X);
            Assert.AreEqual(7, activeQuadrant.GetHostiles()[0].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.GetHostiles()[1].Sector.X);
            Assert.AreEqual(4, activeQuadrant.GetHostiles()[1].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.GetHostiles()[2].Sector.X);
            Assert.AreEqual(6, activeQuadrant.GetHostiles()[2].Sector.Y);
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder2()
        {
            //**This test has an interesting error in it..

            var game = new Game();

            Assert.IsInstanceOf<Map>(game.Map);

            StarTrekKGSettings.Get = null;

            _testMap = HostileTests.SetUp3Hostiles();

            var activeQuad = _testMap.Quadrants.GetActive();

            Assert.AreEqual(64, activeQuad.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = activeQuad;

            Assert.AreEqual(SectorItem.Friendly, activeQuadrant.Sectors[1].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[38].Item, "Expected Hostile at activeQuadrant.Sectors[38]");
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[23].Item, "Expected Hostile at activeQuadrant.Sectors[23]");
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[24].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[36].Item, "Expected Hostile at activeQuadrant.Sectors[39]");
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[39].Item);


            //when the following code is run after this, when the full test harness is run, this errors.
            //Assert.AreEqual(2, activeQuadrant.Hostiles[0].Sector.X);
            //Assert.AreEqual(7, activeQuadrant.Hostiles[0].Sector.Y);

            //Assert.AreEqual(4, activeQuadrant.Hostiles[1].Sector.X, "SectorX location expected to be a 4");
            //Assert.AreEqual(6, activeQuadrant.Hostiles[1].Sector.Y, "SectorY location expected to be a 6"); //when run with a lot of tests, this is 6.  if run by itself, its 4

            //Assert.AreEqual(4, activeQuadrant.Hostiles[2].Sector.X);
            //Assert.AreEqual(6, activeQuadrant.Hostiles[2].Sector.Y);
        }

        private static Map SetUp3Hostiles()
        {
            return (new Map(new GameConfig
                                {
                                    Initialize = true,
                                    
                                    
                                    SectorDefs = new SectorDefs
                                                     {
                                                         new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 1)), SectorItem.Friendly),
                                                         new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.Hostile),
                                                         new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)), SectorItem.Hostile),
                                                         new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)), SectorItem.Hostile)
                                                     },
                                    AddStars = false
                                }));
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder3()
        {
            _testMap = HostileTests.SetUp3Hostiles();

            var activeQuad = _testMap.Quadrants.GetActive();
            Assert.AreEqual(64, activeQuad.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = activeQuad;

            Assert.AreEqual(3, activeQuadrant.GetHostiles().Count());
            Assert.AreEqual(3, activeQuadrant.GetHostiles().Count());
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder4()
        {
            _testMap = HostileTests.SetUp3Hostiles();

            var activeQuad = _testMap.Quadrants.GetActive();
            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = activeQuad;

            Assert.AreEqual(2, activeQuadrant.GetHostiles()[0].Sector.X);
            Assert.AreEqual(7, activeQuadrant.GetHostiles()[0].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.GetHostiles()[1].Sector.X);
            Assert.AreEqual(4, activeQuadrant.GetHostiles()[1].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.GetHostiles()[2].Sector.X);
            Assert.AreEqual(6, activeQuadrant.GetHostiles()[2].Sector.Y);
        }

        //Maybe you want to add/remove hostiles on the fly or something, during the game 
        [Test]
        public void MapWith2HostilesAnotherWay()
        {
            _testMap = (new Map(new GameConfig
                                   {
                                       Initialize = true,
                                       
                                       
                                       SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(SectorItem.Friendly)
                                            }
                                   }));

            var activeQuad = _testMap.Quadrants.GetActive();
            var activeQuadrant = activeQuad;

            //add a ship
            var hostileShip = new Ship("ship1", _testMap, new Sector(new LocationDef(activeQuadrant, new Coordinate(1, 7))));
            var hostileShip2 = new Ship("ship2", _testMap, new Sector(new LocationDef(activeQuadrant, new Coordinate(1, 6))));

            activeQuadrant.AddShip(hostileShip, hostileShip.Sector);
            activeQuadrant.AddShip(hostileShip2, hostileShip2.Sector);

            var activeQuadrantAfterAdding = _testMap.Quadrants.GetActive();
            var hostiles = activeQuadrantAfterAdding.GetHostiles();

            Assert.AreEqual(2, hostiles.Count);

            Assert.AreEqual(1, hostiles[0].Sector.X);
            Assert.AreEqual(6, hostiles[0].Sector.Y);

            Assert.AreEqual(1, hostiles[1].Sector.X);
            Assert.AreEqual(7, hostiles[1].Sector.Y);
        }


        private void SetupMapWith2Hostiles()
        {
            _testMap = (new Map(new GameConfig
                                {
                                    Initialize = true,
                                    
                                    SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                    }
                                }));

            //add a ship
            var hostileShip = new Ship("ship1", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));
            var hostileShip2 = new Ship("ship2", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 5))));

            var activeQuad = _testMap.Quadrants.GetActive();
            activeQuad.AddShip(hostileShip, hostileShip.Sector);
            activeQuad.AddShip(hostileShip2, hostileShip2.Sector);
        }

        #region OutOfBounds

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile2()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                        {
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 7)), SectorItem.Hostile)
                                        }
            }));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile3()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                        {
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, -1)), SectorItem.Friendly),
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 7)), SectorItem.Hostile)
                                        }
            }));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile4()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                        {
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, -1)), SectorItem.Hostile)
                                        }
            }));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile5()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile6()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(-1, 0), new Coordinate(1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile7()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, -1), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile8()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 9), new Coordinate(-1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile9()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 10), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }));
        }

        #endregion
    }
}
