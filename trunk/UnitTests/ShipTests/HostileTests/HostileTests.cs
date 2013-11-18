using System.Linq;
using NUnit.Framework;
using StarTrek_KG;
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

        [Test]
        public void ALLHostilesAttack_ShipUndocked_WithShields()
        {
            this.SetupMapWith2Hostiles();

            //raise shields
            Shields.For(_testMap.Playership).AddEnergy(2500, true); //hopefully a single hit wont be harder than this!
            Assert.AreEqual(2500, Shields.For(_testMap.Playership).Energy, "Unexpected shield energy level"); //shields charged correctly // todo: do more tests on this in ShieldTests          
            Assert.AreEqual(500, _testMap.Playership.Energy, "Ship energy not at maximum"); //ship has no damage

            _testMap.Quadrants.ALLHostilesAttack(_testMap);

            Assert.IsFalse(_testMap.Playership.Destroyed); 
            Assert.Less(Shields.For(_testMap.Playership).Energy, 2500);
            Assert.AreEqual(_testMap.Playership.Energy, 500);
    
            //Assert that ship has taken 2 hits.
            //todo: use a mock to determine that Ship.AbsorbHitFrom() was called twice.
        }

        [Test]
        public void ALLHostilesAttack_ShipUndocked_NoShields()
        {
            this.SetupMapWith2Hostiles();
            
            Assert.AreEqual(3000, _testMap.Playership.Energy, "Ship energy not at expected amount"); //ship has no damage

            _testMap.Quadrants.ALLHostilesAttack(_testMap);

            Assert.IsTrue(_testMap.Playership.Destroyed);  //guess you shoulda raised shields first, huh?
            Assert.AreEqual(0, _testMap.Playership.Energy);  //todo: check all of ship's systems in HostileTests
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
            Assert.AreEqual(_testMap.Playership.Energy, 3000);      
        }

        /// <summary>
        /// The only point behind this test was to understand the limit.
        /// Apparently, currently using a hardcoded seed of 300, and a distanceDeprecationLevel of 11.3
        /// ** This test will break once those values are pulled from config **
        /// (lets hope that the config settings never get pulled *in* this function, as then the test will go very slow)
        /// </summary>
        [Test]
        public void DisruptorShot()
        {
            //todo: pull these from config
            const double deprecationLevel = 11.3;
            const int seed = 300;

            for (var i = 1; i < 100000; i++)
            {
                var oneSector = _testMap.DisruptorShot(seed, deprecationLevel, 1);
                var twoSector = _testMap.DisruptorShot(seed, deprecationLevel, 2);
                var fourSector = _testMap.DisruptorShot(seed, deprecationLevel, 4);
                var sevenSector = _testMap.DisruptorShot(seed, deprecationLevel, 7);
                var eightSector = _testMap.DisruptorShot(seed, deprecationLevel, 8);

                Assert.Less(oneSector, 290, "iteration: " + i);
                Assert.Less(twoSector, 247, "iteration: " + i);
                Assert.Less(fourSector, 194, "iteration: " + i);
                Assert.Less(sevenSector, 115, "iteration: " + i);
                Assert.Less(eightSector, 88, "iteration: " + i);
            }
        }

        [Test]
        public void MapWith0Friendlies()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
                SectorDefs = new SectorDefs(),
                AddStars = false
            }));

            Assert.AreEqual(64, _testMap.Quadrants.GetActive().Sectors.Count);
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
                SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(SectorItem.Friendly)
                                            }
            }));

            Assert.AreEqual(64, _testMap.Quadrants.GetActive().Sectors.Count);

            //Playership's quadrant has been set correctly..
            Assert.AreEqual(_testMap.Playership.QuadrantDef.X, _testMap.Quadrants.GetActive().X);
            Assert.AreEqual(_testMap.Playership.QuadrantDef.Y, _testMap.Quadrants.GetActive().Y);

            //Check to see if Playership has been assigned to a sector in the active quadrant.

            //indirectly..
            Assert.AreEqual(1, _testMap.Quadrants.GetActive().Sectors.Count(s => s.Item == SectorItem.Friendly));

            //directly.
            Assert.AreEqual(SectorItem.Friendly, _testMap.Quadrants.GetActive().Sectors.Single(s => s.X == _testMap.Playership.Sector.X && s.Y == _testMap.Playership.Sector.Y).Item);
        }

        //this method is here to test passing in 
        [Test]
        public void MapWith3HostilesTheConfigWay()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 1)), SectorItem.Friendly),
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 6)), SectorItem.Hostile),
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)), SectorItem.Hostile),
                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)), SectorItem.Hostile)
                    },
                AddStars = false
            }));

            Assert.AreEqual(64, _testMap.Quadrants.GetActive().Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = _testMap.Quadrants.GetActive();

            Assert.AreEqual(SectorItem.Friendly, activeQuadrant.Sectors[17].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[22].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[23].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[24].Item);

            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[36].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[37].Item);

            Assert.AreEqual(3, activeQuadrant.Hostiles.Count());
            Assert.AreEqual(3, activeQuadrant.GetHostileCount());

            Assert.AreEqual(2, activeQuadrant.Hostiles[0].Sector.X);
            Assert.AreEqual(6, activeQuadrant.Hostiles[0].Sector.Y);

            Assert.AreEqual(2, activeQuadrant.Hostiles[1].Sector.X);
            Assert.AreEqual(7, activeQuadrant.Hostiles[1].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.Hostiles[2].Sector.X);
            Assert.AreEqual(4, activeQuadrant.Hostiles[2].Sector.Y);
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 5)), SectorItem.Friendly),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.Hostile),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)), SectorItem.Hostile),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)), SectorItem.Hostile)
                },
                AddStars = false
            }));

            Assert.AreEqual(64, _testMap.Quadrants.GetActive().Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = _testMap.Quadrants.GetActive();

            Assert.AreEqual(SectorItem.Friendly, activeQuadrant.Sectors[37].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[38].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[23].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[24].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[36].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[39].Item);

            Assert.AreEqual(3, activeQuadrant.Hostiles.Count());
            Assert.AreEqual(3, activeQuadrant.GetHostileCount());

            Assert.AreEqual(2, activeQuadrant.Hostiles[0].Sector.X);
            Assert.AreEqual(7, activeQuadrant.Hostiles[0].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.Hostiles[1].Sector.X);
            Assert.AreEqual(4, activeQuadrant.Hostiles[1].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.Hostiles[2].Sector.X);
            Assert.AreEqual(6, activeQuadrant.Hostiles[2].Sector.Y);
        }

        /// <summary>
        /// This addresses an issue where ships added in descending order causes the first one not to be entered when all tests are run
        /// </summary>
        [Test]
        public void MapWith3HostilesTheConfigWayAddedInDescendingOrder2()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 1)), SectorItem.Friendly),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 6)), SectorItem.Hostile),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)), SectorItem.Hostile),
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(4, 4)), SectorItem.Hostile)
                },
                AddStars = false
            }));

            Assert.AreEqual(64, _testMap.Quadrants.GetActive().Sectors.Count);

            //todo: why active? are hostiles in the same sector?
            var activeQuadrant = _testMap.Quadrants.GetActive();

            Assert.AreEqual(SectorItem.Friendly, activeQuadrant.Sectors[1].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[38].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[23].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[24].Item);
            Assert.AreEqual(SectorItem.Hostile, activeQuadrant.Sectors[36].Item);
            Assert.AreEqual(SectorItem.Empty, activeQuadrant.Sectors[39].Item);

            Assert.AreEqual(3, activeQuadrant.Hostiles.Count());
            Assert.AreEqual(3, activeQuadrant.GetHostileCount());

            Assert.AreEqual(2, activeQuadrant.Hostiles[0].Sector.X);
            Assert.AreEqual(7, activeQuadrant.Hostiles[0].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.Hostiles[1].Sector.X);
            Assert.AreEqual(4, activeQuadrant.Hostiles[1].Sector.Y);

            Assert.AreEqual(4, activeQuadrant.Hostiles[2].Sector.X);
            Assert.AreEqual(6, activeQuadrant.Hostiles[2].Sector.Y);
        }

        //Maybe you want to add/remove hostiles on the fly or something, during the game 
        [Test]
        public void MapWith2HostilesAnotherWay()
        {
            _testMap = (new Map(new GameConfig
                                   {
                                       Initialize = true,
                                       GenerateMap = true,
                                       UseAppConfigSectorDefs = false,
                                       SectorDefs = new SectorDefs
                                            {
                                                new SectorDef(SectorItem.Friendly)
                                            }
                                   }));

            //this was creates an invisible hostile. why?
            //this.Map.Quadrants.GetActive().Hostiles.Add(new Ship("testbaddie", this.Map, new Sector(new LocationDef(3, 7, 3, 7))));

            //add a ship
            var hostileShip = new Ship("ship1", _testMap, new Sector(new LocationDef(null, new Coordinate(1, 7))));
            var hostileShip2 = new Ship("ship2", _testMap, new Sector(new LocationDef(null, new Coordinate(1, 6))));

            _testMap.Quadrants.GetActive().Hostiles.Add(hostileShip);
            _testMap.Quadrants.GetActive().Hostiles.Add(hostileShip2);

            Assert.AreEqual(2, _testMap.Quadrants.GetActive().Hostiles.Count);

            Assert.AreEqual(1, _testMap.Quadrants.GetActive().Hostiles[0].Sector.X);
            Assert.AreEqual(7, _testMap.Quadrants.GetActive().Hostiles[0].Sector.Y);

            Assert.AreEqual(1, _testMap.Quadrants.GetActive().Hostiles[1].Sector.X);
            Assert.AreEqual(6, _testMap.Quadrants.GetActive().Hostiles[1].Sector.Y);
        }

        private void SetupMapWith2Hostiles()
        {
            _testMap = (new Map(new GameConfig
                                {
                                    Initialize = true,
                                    GenerateMap = true,
                                    SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(null, new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                    }
                                }));

            //add a ship
            var hostileShip = new Ship("ship1", _testMap, new Sector(new LocationDef(null, new Coordinate(2, 7))));
            var hostileShip2 = new Ship("ship2", _testMap, new Sector(new LocationDef(null, new Coordinate(2, 5))));

            _testMap.Quadrants.GetActive().Hostiles.Add(hostileShip);
            _testMap.Quadrants.GetActive().Hostiles.Add(hostileShip2);
        }

        #region OutOfBounds

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void MapCreateOutOfBoundsHostile()
        {
            _testMap = (new Map(new GameConfig
            {
                Initialize = true,
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
                GenerateMap = true,
                UseAppConfigSectorDefs = false,
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
