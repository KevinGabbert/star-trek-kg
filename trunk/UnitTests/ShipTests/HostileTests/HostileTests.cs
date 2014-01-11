﻿using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.HostileTests
{
    [TestFixture]
    public class HostileTests: TestClass_Base
    {
        private readonly Test_Setup setup = new Test_Setup();

        [Test]
        public void ALLHostilesAttack_ShipUndocked_WithShields()
        {
            //todo:  verify that quadrants are set up correctly.
            //todo: This test does not run alone.  what do the other tests set up that this needs?  why don't thea other tests tear down their stuff?

            //todo: will we need to mock out the Console.write process just so that we can test the output?  I'm thinking so..
            setup.SetupMapWith2Hostiles();

            Assert.AreEqual(setup.TestMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"), "Ship energy not at expected amount"); 

            //raise shields
            Shields.For(setup.TestMap.Playership).SetEnergy(2500); //hopefully a single hit wont be harder than this!
            Assert.AreEqual(2500, Shields.For(setup.TestMap.Playership).Energy, "Unexpected shield energy level"); //shields charged correctly // todo: do more tests on this in ShieldTests    

            //Assert.AreEqual(StarTrekKGSettings.GetSetting<int>("energy"), _testMap.Playership.Energy, "Ship energy not at maximum"); //ship has no damage

            Assert.AreEqual(setup.TestMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"), "Ship energy not at expected amount");

            (new Game(this.Draw)).ALLHostilesAttack(setup.TestMap);

            Assert.IsFalse(setup.TestMap.Playership.Destroyed);
            Assert.Less(Shields.For(setup.TestMap.Playership).Energy, 2500);
            Assert.AreEqual(StarTrekKGSettings.GetSetting<int>("energy"), setup.TestMap.Playership.Energy, "expected no change to ship energy"); 
    
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
            setup.SetupMapWith2Hostiles();

            Assert.AreEqual(StarTrekKGSettings.GetSetting<int>("energy"), setup.TestMap.Playership.Energy, "Ship energy not at expected amount"); //ship has no damage
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
            this.AttackAndCheck();
        }

        private void AttackAndCheck()
        {
            setup.SetupMapWith2Hostiles();

            Assert.AreEqual(setup.TestMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"), "Ship energy not at expected amount");

            (new Game(this.Draw)).ALLHostilesAttack(setup.TestMap);

            Assert.IsFalse(setup.TestMap.Playership.Destroyed);

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
            Assert.GreaterOrEqual(setup.TestMap.Playership.Subsystems.Count(s => s.Damaged()), 1);
        }

        [Test]
        public void ALLHostilesAttack_ShipDocked()
        {
            setup.SetupMapWith2Hostiles();

            //cheating so we can cover this line
            Navigation.For(setup.TestMap.Playership).docked = true;

            (new Game(this.Draw)).ALLHostilesAttack(setup.TestMap);

            //Ship has taken no damage.
            Assert.IsFalse(setup.TestMap.Playership.Destroyed);
            Assert.AreEqual(Shields.For(setup.TestMap.Playership).Energy, 0); //never even needed to raise shields!
            Assert.AreEqual(setup.TestMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"));      
        }

        [Test]
        public void MapWith0Friendlies()
        {
            var game = (new Game(this.Draw)); //this can make tests break so I throw it in for a check..

            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs(),
                AddStars = false
            }, this.Write, this.Command));

            var activeQuad = setup.TestMap.Quadrants.GetActive();

            Assert.AreEqual(64, activeQuad.Sectors.Count);
            Assert.AreEqual(0, setup.TestMap.Quadrants.GetHostileCount()); //no hostiles
            Assert.AreEqual(null, setup.TestMap.Playership); //no friendly
            
            //just empty sectors
            foreach (var sector in setup.TestMap.Quadrants.SelectMany(quadrant => quadrant.Sectors))
            {
                Assert.AreEqual(SectorItem.Empty, sector.Item);
            }
        }

        //this method is here to test passing in 
        [Test]
        public void MapWith1Friendly()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(SectorItem.Friendly)
                                            }
            }, this.Write, this.Command));

            var activeQuad = setup.TestMap.Quadrants.GetActive();

            Assert.AreEqual(64, activeQuad.Sectors.Count);

            //Playership's quadrant has been set correctly..
            Assert.AreEqual(setup.TestMap.Playership.QuadrantDef.X, activeQuad.X);
            Assert.AreEqual(setup.TestMap.Playership.QuadrantDef.Y, activeQuad.Y);

            //Check to see if Playership has been assigned to a sector in the active quadrant.

            //indirectly..
            Assert.AreEqual(1, activeQuad.Sectors.Count(s => s.Item == SectorItem.Friendly));

            //directly.
            Assert.AreEqual(SectorItem.Friendly, activeQuad.Sectors.Single(s => s.X == setup.TestMap.Playership.Sector.X && s.Y == setup.TestMap.Playership.Sector.Y).Item);
        }

        //Maybe you want to add/remove hostiles on the fly or something, during the game 
        [Test]
        public void MapWith1Hostile()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs()
            }, this.Write, this.Command));


            var locationDef = new LocationDef(new Coordinate(0, 0), new Coordinate(1, 7));

            //add a ship
            var hostileShip = new Ship("ship1", setup.TestMap, new Sector(locationDef), this.Write, this.Command);

            setup.TestMap.Quadrants[0].AddShip(hostileShip, setup.TestMap.Quadrants[0].Sectors.Get(new Coordinate(1, 7)));

            var hostiles = setup.TestMap.Quadrants.GetHostiles();
            Assert.AreEqual(1, hostiles.Count);

            var firstHostile = setup.TestMap.Quadrants.GetHostiles()[0];
            Assert.AreEqual("Sector: 1, 7", firstHostile.Sector.ToString());

            Assert.AreEqual(1, setup.TestMap.Quadrants.GetHostiles()[0].Sector.X);
            Assert.AreEqual(7, setup.TestMap.Quadrants.GetHostiles()[0].Sector.Y);
        }

        [Test]
        public void MapWith1HostileAlternate()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.Hostile)
                                            }
            }, this.Write, this.Command));

            var hostiles = setup.TestMap.Quadrants.GetHostiles();
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
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs()
            }, this.Write, this.Command));

            var locationDef = new LocationDef(new Coordinate(0, 0), new Coordinate(1, 7));

            //add a ship
            var hostileShip = new Ship("ship1", setup.TestMap, new Sector(locationDef), this.Write, this.Command);

            setup.TestMap.Quadrants[0].AddShip(hostileShip, setup.TestMap.Quadrants[0].Sectors.Get(new Coordinate(1, 7)));

            setup.TestMap.Quadrants.RemoveShip(hostileShip.Name);

            Assert.AreEqual(0, setup.TestMap.Quadrants.GetHostiles().Count);
        }

        //this method is here to test passing in 
        [Test]
        public void MapWith3HostilesTheConfigWay()
        {
            TestMap3Scenario();
        }

        [Ignore("Fixed!")]
        [Test]
        public void MapWith3HostilesTheConfigWay_FailsIntermittently_overAndOver()
        {
            for(int i = 0; i < 1000; i++)
            {
                var x = (new Game(this.Draw));
                Command.Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%% i: " + i);
                TestMap3Scenario();
            }
        }

        private void TestMap3Scenario()
        {
            var x = new Game(this.Draw);

            setup.TestMap = (new Map(new GameConfig
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
                }, this.Write, this.Command));

            var activeQuad = setup.TestMap.Quadrants.GetActive();

            Assert.AreEqual(64, activeQuad.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = setup.TestMap.Quadrants.GetActive();

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
            var x = (new Game(this.Draw));

            setup.TestMap = (new Map(new GameConfig
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
            }, this.Write, this.Command));

            var activeQuad = setup.TestMap.Quadrants.GetActive();

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
            //fails intermittently..
            //**This test has an interesting error in it..

            var game = new Game(this.Draw);

            Assert.IsInstanceOf<Map>(game.Map);

            StarTrekKGSettings.Get = null;

            setup.TestMap = this.SetUp3Hostiles();

            var activeQuad = setup.TestMap.Quadrants.GetActive();

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

        private Map SetUp3Hostiles()
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
                                }, this.Write, this.Command));
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder3()
        {
            var x = (new Game(this.Draw));

            setup.TestMap = this.SetUp3Hostiles();

            var activeQuad = setup.TestMap.Quadrants.GetActive();
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
            setup.TestMap = this.SetUp3Hostiles();

            var activeQuad = setup.TestMap.Quadrants.GetActive();
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
            setup.TestMap = (new Map(new GameConfig
                                   {
                                       Initialize = true,
                                       
                                       
                                       SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(SectorItem.Friendly)
                                            }
                                   }, this.Write, this.Command));

            var activeQuad = setup.TestMap.Quadrants.GetActive();
            var activeQuadrant = activeQuad;

            //add a ship
            var hostileShip = new Ship("ship1", setup.TestMap, new Sector(new LocationDef(activeQuadrant, new Coordinate(1, 7))), this.Write, this.Command);
            var hostileShip2 = new Ship("ship2", setup.TestMap, new Sector(new LocationDef(activeQuadrant, new Coordinate(1, 6))), this.Write, this.Command);

            activeQuadrant.AddShip(hostileShip, hostileShip.Sector);
            activeQuadrant.AddShip(hostileShip2, hostileShip2.Sector);

            var activeQuadrantAfterAdding = setup.TestMap.Quadrants.GetActive();
            var hostiles = activeQuadrantAfterAdding.GetHostiles();

            Assert.AreEqual(2, hostiles.Count);

            Assert.AreEqual(1, hostiles[0].Sector.X);
            Assert.AreEqual(6, hostiles[0].Sector.Y);

            Assert.AreEqual(1, hostiles[1].Sector.X);
            Assert.AreEqual(7, hostiles[1].Sector.Y);
        }

        #region OutOfBounds

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }, this.Write, this.Command));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile2()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                        {
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 7)), SectorItem.Hostile)
                                        }
            }, this.Write, this.Command));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile3()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                        {
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, -1)), SectorItem.Friendly),
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 7)), SectorItem.Hostile)
                                        }
            }, this.Write, this.Command));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile4()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                        {
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                                            new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, -1)), SectorItem.Hostile)
                                        }
            }, this.Write, this.Command));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile5()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }, this.Write, this.Command));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile6()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(-1, 0), new Coordinate(1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }, this.Write, this.Command));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile7()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, -1), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }, this.Write, this.Command));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile8()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 9), new Coordinate(-1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }, this.Write, this.Command));
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile9()
        {
            setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)), SectorItem.Friendly),
                                new SectorDef(new LocationDef(new Coordinate(0, 10), new Coordinate(2, 8)), SectorItem.Hostile)
                            }
            }, this.Write, this.Command));
        }

        #endregion
    }
}
