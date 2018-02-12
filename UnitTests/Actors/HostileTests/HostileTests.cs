using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.TestObjects;

namespace UnitTests.Actors.HostileTests
{
    [TestFixture]
    public class HostileTests: TestClass_Base
    {
        [Test]
        public void ALLHostilesAttack_ShipUndocked_WithShields()
        {
            //todo:  verify that Regions are set up correctly.
            //todo: This test does not run alone.  what do the other tests set up that this needs?  why don't thea other tests tear down their stuff?

            //todo: will we need to mock out the Console.write process just so that we can test the output?  I'm thinking so..
            _setup.SetupMapWith2Hostiles();

            Assert.AreEqual(_setup.TestMap.Playership.Energy, new StarTrekKGSettings().GetSetting<int>("energy"), "Ship energy not at expected amount"); 

            //raise shields
            Shields.For(_setup.TestMap.Playership).SetEnergy(2500); //hopefully a single hit wont be harder than this!
            Assert.AreEqual(2500, Shields.For(_setup.TestMap.Playership).Energy, "Unexpected shield energy level"); //shields charged correctly // todo: do more tests on this in ShieldTests    

            //Assert.AreEqual((new StarTrekKGSettings()).GetSetting<int>("energy"), _testMap.Playership.Energy, "Ship energy not at maximum"); //ship has no damage

            Assert.AreEqual(_setup.TestMap.Playership.Energy, new StarTrekKGSettings().GetSetting<int>("energy"), "Ship energy not at expected amount");

            new Game(new StarTrekKGSettings()).ALLHostilesAttack(_setup.TestMap);

            Assert.IsFalse(_setup.TestMap.Playership.Destroyed);
            Assert.Less(Shields.For(_setup.TestMap.Playership).Energy, 2500);
            Assert.AreEqual(new StarTrekKGSettings().GetSetting<int>("energy"), _setup.TestMap.Playership.Energy, "expected no change to ship energy. Ship should have been protected by shields."); 
    
            //Assert that ship has taken 2 hits.
            //todo: use a mock to determine that Ship.AbsorbHitFrom() was called twice.
        }

        /// <summary>
        /// This test is very sensitive to the evolving ship damage mechanic.
        /// </summary>
        [Test]
        public void ALLHostilesAttack_ShipUndocked_WithNoShields()
        {
            //todo:  This test needs to be tuned to proper gameplay
            _setup.SetupMapWith1Hostile();

            IShip badGuy = _setup.TestMap.Regions.GetHostiles().Single();
            badGuy.Energy = 2000;
            badGuy.Subsystems.Add(new Disruptors(badGuy) { Energy = 500});
            Disruptors disruptorsForBadGuy = (Disruptors)badGuy.Subsystems.Single(s => s.Type == SubsystemType.Disruptors);

            Assert.AreEqual(_setup.TestMap.Playership.Energy, new StarTrekKGSettings().GetSetting<int>("energy"), "Ship energy not at expected amount");

            Shields.For(_setup.TestMap.Playership).SetEnergy(0);

            _setup.TestMap.Playership.Energy = 100;

            const int expectedHitsTilldestruction = 12;
            for (int i = 0; i < 100; i++)
            {
                var testGame = new Game(new StarTrekKGSettings());

                TakeShots(testGame, expectedHitsTilldestruction);

                if (_setup.TestMap.Playership.Destroyed)
                {
                    break;
                }
                else
                {
                    disruptorsForBadGuy.Energy = 1000;
                    badGuy.Energy = 2000;
                }
            }

            //todo: use a mock to determine that Ship.AbsorbHitFrom() was called twice.
        }

        private void TakeShots(Game testGame, int expectedHitsTilldestruction)
        {
            for (int i = 0; i < 10; i++)
            {
                testGame.ALLHostilesAttack(_setup.TestMap);

                if (_setup.TestMap.Playership.Destroyed)
                {
                    break;
                }
                Assert.LessOrEqual(i, expectedHitsTilldestruction);
            }
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

            //_testMap.Regions.ALLHostilesAttack(_testMap);

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

            //_testMap.Regions.ALLHostilesAttack(_testMap);

            //Assert.IsFalse(_testMap.Playership.Destroyed);

            ////2 subsystems should have been taken out.
            //Assert.AreEqual(2, _testMap.Playership.Subsystems.Count(s => s.Damaged()));  //todo: check all of ship's systems in HostileTests
        }

        [Test]
        public void ShipUndocked_NoShields_CheckEnergy()
        {
            _setup.SetupMapWith2Hostiles();

            Assert.AreEqual(new StarTrekKGSettings().GetSetting<int>("energy"), _setup.TestMap.Playership.Energy, "Ship energy not at expected amount"); //ship has no damage
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
            _setup.SetupMapWith2Hostiles();

            Assert.AreEqual(_setup.TestMap.Playership.Energy, new StarTrekKGSettings().GetSetting<int>("energy"), "Ship energy not at expected amount");

            new Game(new StarTrekKGSettings()).ALLHostilesAttack(_setup.TestMap);

            Assert.IsFalse(_setup.TestMap.Playership.Destroyed);

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
            Assert.GreaterOrEqual(_setup.TestMap.Playership.Subsystems.Count(s => s.Damaged()), 1);
        }

        [Test]
        public void ALLHostilesAttack_ShipDocked()
        {
            _setup.SetupMapWith2Hostiles();

            //cheating so we can cover this line
            Navigation.For(_setup.TestMap.Playership).Docked = true;

            this.Game.ALLHostilesAttack(_setup.TestMap);

            //Ship has taken no damage.
            Assert.IsFalse(_setup.TestMap.Playership.Destroyed);
            Assert.AreEqual(Shields.For(_setup.TestMap.Playership).Energy, 0); //never even needed to raise shields!
            Assert.AreEqual(_setup.TestMap.Playership.Energy, new StarTrekKGSettings().GetSetting<int>("energy"));      
        }

        [Test]
        public void MapWith0Friendlies()
        {
            var game = new Game(new StarTrekKGSettings()); //this can make tests break so I throw it in for a check..

            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs(),
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            var activeRegion = _setup.TestMap.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count);
            Assert.AreEqual(0, _setup.TestMap.Regions.GetHostileCount()); //no hostiles
            Assert.AreEqual(null, _setup.TestMap.Playership); //no friendly
            
            //just empty sectors
            foreach (var sector in _setup.TestMap.Regions.SelectMany(Region => Region.Sectors))
            {
                Assert.AreEqual(SectorItem.Empty, sector.Item);
            }
        }

        //this method is here to test passing in 
        [Test]
        public void MapWith1Friendly()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                {
                    new SectorDef(SectorItem.PlayerShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            var activeRegion = _setup.TestMap.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count);

            //Playership's Region has been set correctly..
            Assert.AreEqual(_setup.TestMap.Playership.Coordinate.X, activeRegion.X);
            Assert.AreEqual(_setup.TestMap.Playership.Coordinate.Y, activeRegion.Y);

            //Check to see if Playership has been assigned to a sector in the active Region.

            //indirectly..
            Assert.AreEqual(1, activeRegion.Sectors.Count(s => s.Item == SectorItem.PlayerShip));

            //directly.
            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors.Single(s => s.X == _setup.TestMap.Playership.Sector.X && s.Y == _setup.TestMap.Playership.Sector.Y).Item);
        }

        //Maybe you want to add/remove hostiles on the fly or something, during the game 
        [Test]
        public void MapWith1Hostile()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs()
            }, this.Game.Interact, this.Game.Config, this.Game);


            var locationDef = new LocationDef(new Coordinate(0, 0), new Coordinate(1, 7));

            //add a ship
            var hostileShip = new Ship(FactionName.Klingon, "ship1", new Sector(locationDef), this.Game.Map);

            _setup.TestMap.Regions[0].AddShip(hostileShip, _setup.TestMap.Regions[0].Sectors.Get(new Coordinate(1, 7)));

            var hostiles = _setup.TestMap.Regions.GetHostiles();
            Assert.AreEqual(1, hostiles.Count);

            var firstHostile = _setup.TestMap.Regions.GetHostiles()[0];
            Assert.AreEqual("Sector: 1, 7", firstHostile.Sector.ToString());

            Assert.AreEqual(1, _setup.TestMap.Regions.GetHostiles()[0].Sector.X);
            Assert.AreEqual(7, _setup.TestMap.Regions.GetHostiles()[0].Sector.Y);
        }

        [Test]
        public void MapWith1HostileAlternate()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                
                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            var hostiles = _setup.TestMap.Regions.GetHostiles();
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
            _setup.TestMap = new Map(new SetupOptions
            {
                AddNebulae = false,
                Initialize = true,
                SectorDefs = new SectorDefs()
            }, this.Game.Interact, this.Game.Config, this.Game);

            var locationDef = new LocationDef(new Coordinate(0, 0), new Coordinate(1, 7));

            //add a ship
            var hostileShip = new Ship(FactionName.Klingon, "ship1", new Sector(locationDef), this.Game.Map);

            _setup.TestMap.Regions[0].AddShip(hostileShip, _setup.TestMap.Regions[0].Sectors.Get(new Coordinate(1, 7)));

            _setup.TestMap.Regions.RemoveShip(hostileShip);

            Assert.AreEqual(0, _setup.TestMap.Regions.GetHostiles().Count);
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
                this.Game.Interact.Output.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%% i: " + i);
                TestMap3Scenario();
            }
        }

        private void TestMap3Scenario()
        {
            var x = new Game(new StarTrekKGSettings());

            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                SectorDefs = new SectorDefs
                {
                    new SectorDef(
                        new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)),
                        SectorItem.PlayerShip),
                    new SectorDef(
                        new LocationDef(new Coordinate(0, 0), new Coordinate(2, 6)),
                        SectorItem.HostileShip),
                    new SectorDef(
                        new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)),
                        SectorItem.HostileShip),
                    new SectorDef(
                        new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)),
                        SectorItem.HostileShip)
                },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            var activeRegion = _setup.TestMap.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            //var activeRegion = _setup.TestMap.Regions.GetActive();

            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors[17].Item);
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[22].Item);
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[23].Item);

            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[24].Item);

            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[36].Item);
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[37].Item);

            Assert.AreEqual(3, activeRegion.GetHostiles().Count);

            Assert.AreEqual(2, activeRegion.GetHostiles()[0].Sector.X);
            Assert.AreEqual(6, activeRegion.GetHostiles()[0].Sector.Y);

            Assert.AreEqual(2, activeRegion.GetHostiles()[1].Sector.X);
            Assert.AreEqual(7, activeRegion.GetHostiles()[1].Sector.Y);

            Assert.AreEqual(4, activeRegion.GetHostiles()[2].Sector.X);
            Assert.AreEqual(4, activeRegion.GetHostiles()[2].Sector.Y);
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder()
        {
            var x = new Game(new StarTrekKGSettings());

            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                
                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 5)), SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.HostileShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)), SectorItem.HostileShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)), SectorItem.HostileShip)
                },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            var activeRegion = _setup.TestMap.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            //var activeRegion = activeRegion;

            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors[37].Item);
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[38].Item);
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[23].Item);
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[24].Item);
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[36].Item);
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[39].Item);

            Assert.AreEqual(3, activeRegion.GetHostiles().Count);
            Assert.AreEqual(3, activeRegion.GetHostiles().Count);

            Assert.AreEqual(2, activeRegion.GetHostiles()[0].Sector.X);
            Assert.AreEqual(7, activeRegion.GetHostiles()[0].Sector.Y);

            Assert.AreEqual(4, activeRegion.GetHostiles()[1].Sector.X);
            Assert.AreEqual(4, activeRegion.GetHostiles()[1].Sector.Y);

            Assert.AreEqual(4, activeRegion.GetHostiles()[2].Sector.X);
            Assert.AreEqual(6, activeRegion.GetHostiles()[2].Sector.Y);
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder2()
        {
            //fails intermittently..
            //**This test has an interesting error in it..

            var game = new Game(new StarTrekKGSettings());

            Assert.IsInstanceOf<Map>(game.Map);

            new StarTrekKGSettings().Get = null;

            _setup.TestMap = this.SetUp3Hostiles();

            var activeRegion = _setup.TestMap.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            //var activeRegion = activeRegion;

            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors[1].Item);
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[38].Item, "Expected Hostile at activeRegion.Sectors[38]");
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[23].Item, "Expected Hostile at activeRegion.Sectors[23]");
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[24].Item);
            Assert.AreEqual(SectorItem.HostileShip, activeRegion.Sectors[36].Item, "Expected Hostile at activeRegion.Sectors[39]");
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[39].Item);


            //when the following code is run after this, when the full test harness is run, this errors.
            //Assert.AreEqual(2, activeRegion.Hostiles[0].Sector.X);
            //Assert.AreEqual(7, activeRegion.Hostiles[0].Sector.Y);

            //Assert.AreEqual(4, activeRegion.Hostiles[1].Sector.X, "SectorX location expected to be a 4");
            //Assert.AreEqual(6, activeRegion.Hostiles[1].Sector.Y, "SectorY location expected to be a 6"); //when run with a lot of tests, this is 6.  if run by itself, its 4

            //Assert.AreEqual(4, activeRegion.Hostiles[2].Sector.X);
            //Assert.AreEqual(6, activeRegion.Hostiles[2].Sector.Y);
        }

        private Map SetUp3Hostiles()
        {
            return new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                                    
                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 1)), SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.HostileShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)), SectorItem.HostileShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)), SectorItem.HostileShip)
                },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder3()
        {
            var x = new Game(new StarTrekKGSettings());

            _setup.TestMap = this.SetUp3Hostiles();

            var activeRegion = _setup.TestMap.Regions.GetActive();
            Assert.AreEqual(64, activeRegion.Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            //var activeRegion = activeRegion;

            Assert.AreEqual(3, activeRegion.GetHostiles().Count);
            Assert.AreEqual(3, activeRegion.GetHostiles().Count);
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder4()
        {
            _setup.TestMap = this.SetUp3Hostiles();

            var activeRegion = _setup.TestMap.Regions.GetActive();
            //todo: why active? are hostiles in the same sector?
            //var activeRegion = activeRegion;

            Assert.AreEqual(2, activeRegion.GetHostiles()[0].Sector.X);
            Assert.AreEqual(7, activeRegion.GetHostiles()[0].Sector.Y);

            Assert.AreEqual(4, activeRegion.GetHostiles()[1].Sector.X);
            Assert.AreEqual(4, activeRegion.GetHostiles()[1].Sector.Y);

            Assert.AreEqual(4, activeRegion.GetHostiles()[2].Sector.X);
            Assert.AreEqual(6, activeRegion.GetHostiles()[2].Sector.Y);
        }

        //Maybe you want to add/remove hostiles on the fly or something, during the game 
        [Test]
        public void MapWith2HostilesAnotherWay()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                                       
                SectorDefs = new SectorDefs
                {
                    new SectorDef(SectorItem.PlayerShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            var activeRegion = _setup.TestMap.Regions.GetActive();
            //var activeRegion = activeRegion;

            //add a ship
            var hostileShip = new Ship(FactionName.Klingon, "ship1", new Sector(new LocationDef(activeRegion, new Coordinate(1, 7))), this.Game.Map);
            var hostileShip2 = new Ship(FactionName.Klingon, "ship2", new Sector(new LocationDef(activeRegion, new Coordinate(1, 6))), this.Game.Map);

            activeRegion.AddShip(hostileShip, hostileShip.Sector);
            activeRegion.AddShip(hostileShip2, hostileShip2.Sector);

            var activeRegionAfterAdding = _setup.TestMap.Regions.GetActive();
            var hostiles = activeRegionAfterAdding.GetHostiles();

            Assert.AreEqual(2, hostiles.Count);

            Assert.AreEqual(1, hostiles[0].Sector.X);
            Assert.AreEqual(6, hostiles[0].Sector.Y);

            Assert.AreEqual(1, hostiles[1].Sector.X);
            Assert.AreEqual(7, hostiles[1].Sector.Y);
        }

        #region OutOfBounds

        [Ignore("Todo: Fix")]
        [Test]
        public void MapCreateOutOfBoundsHostile()
        {
            //todo: the issue with this one is that SECTOR_MAX is set to 8 in the config files, and it would seem that 
            //it should be set to 7.  ssetting it to 7 breaks about 30 tests.  Don't have the brain power to look into
            //that now.

            //Methinks that the navigation needs to be tested completely before this can be fixed

            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)),
                        SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)),
                        SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"
        }

        [Test]
        public void MapCreateOutOfBoundsHostile2()
        {
            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 7)), SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"
        }

        [Test]
        public void MapCreateOutOfBoundsHostile3()
        {
            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, -1)),
                        SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 7)),
                        SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"
        }

        [Test]
        public void MapCreateOutOfBoundsHostile4()
        {
            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)),
                        SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, -1)),
                        SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"
        }

        [Test]
        public void MapCreateOutOfBoundsHostile5()
        {
            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)),
                        SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)),
                        SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"
        }

        [Test]
        public void MapCreateOutOfBoundsHostile6()
        {
            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(-1, 0), new Coordinate(1, 1)),
                        SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)),
                        SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"
        }

        [Test]
        public void MapCreateOutOfBoundsHostile7()
        {
            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)),
                        SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, -1), new Coordinate(2, 8)),
                        SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"

        }

        [Test]
        public void MapCreateOutOfBoundsHostile8()
        {
            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 9), new Coordinate(-1, 1)),
                        SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 8)),
                        SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"
        }

        [Test]
        public void MapCreateOutOfBoundsHostile9()
        {
            Assert.That(() => new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,

                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(-1, 1)),
                        SectorItem.PlayerShip),
                    new SectorDef(new LocationDef(new Coordinate(0, 10), new Coordinate(2, 8)),
                        SectorItem.HostileShip)
                }
            }, this.Game.Interact, this.Game.Config, this.Game), Throws.TypeOf<GameConfigException>()); //"Error Setting up Sector Sector x < 0"
        }

        #endregion
    }
}
