using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;
using UnitTests.TestObjects;

namespace UnitTests.MapTests
{
    [TestFixture]
    public class MapTests: TestClass_Base
    {
        [SetUp]
        public void Setup()
        {
            //todo: call VerifyGlobalInfoSettings

             TestRunner.GetTestConstants();
        }

        [TearDown]
        public void TearDown()
        {
            DEFAULTS.COORDINATE_MIN = 0;
            DEFAULTS.COORDINATE_MAX = 0;

            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;
        }

        [Test]
        public void InitializeSectors()
        {
            _setup.SetupMapWith1Friendly();

            //_setup.TestMap.Initialize(new CoordinateDefs
            //                                {
            //                                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.Friendly),
            //                                    //todo: this needs to be in a random spo
            //                                });

            //More comprehensive Sector tests are in SectorTests
            Assert.AreEqual(64, _setup.TestMap.Sectors.Count); //currently these reside in constants, but will be moving to app.config

            foreach (var Sector in _setup.TestMap.Sectors)
            {
                Assert.AreEqual(64, Sector.Coordinates.Count, "Unexpected sector count: Sector X: " + Sector.X + " Y: " + Sector.Y);
            }
            
            this.VerifyInitializeSettings();
        }

        [Ignore("")]
        [Test]
        public void CreateTooManyStars()
        {
                //Todo: test this
        }


        [Test]
        public void InitializeRegions()
        {
            var klingonShipNames = new StarTrekKGSettings().ShipNames(FactionName.Klingon);
            var systemNames = new StarTrekKGSettings().GetStarSystems();
            _setup.TestMap.InitializeSectorsWithBaddies(new Stack<string>(systemNames),
                                         new Stack<string>(klingonShipNames), null, 
                                         new CoordinateDefs(), false);

            Assert.IsInstanceOf(typeof(Map), _setup.TestMap);
            Assert.Greater(_setup.TestMap.Sectors.Count, 63); //todo: currently these reside in constants, but will be moving to app.config

            ////todo: Assert that no baddies or friendlies are set up.
        }

        [Ignore("")]
        [Test]
        public void PopulateWithHostilesAndStarbases()
        {
            var klingonShipNames = new StarTrekKGSettings().ShipNames(FactionName.Klingon);
            var systemNames = new StarTrekKGSettings().GetStarSystems();

            _setup.TestMap.InitializeSectorsWithBaddies(new Stack<string>(systemNames),
                                         new Stack<string>(klingonShipNames), null, 
                                         null, false);
            //Sector.Populate(_setup.TestMap);

            Assert.Greater(_setup.TestMap.Sectors.Count, 63); //currently these reside in constants, but will be moving to app.config


            //todo: query Sectors for number of starbases (change Sectors to a collection!) and get rid of "starbases" variable)
        }
        
        [Test]
        public void GetGlobalInfo()
        {
            _setup.TestMap.GetGlobalInfo(); 
            this.VerifyGlobalInfoSettings();
        }

        [Ignore("")]
        [Test]
        public void VerifyCommandLineText()
        {
            _setup.TestMap.GetGlobalInfo(); 
            Assert.AreEqual("Enter command: ", _setup.TestMap.Text); 
        }

        [Test]
        public void Generate_NoIntitialize()
        {
            _setup.TestMap = new Map(null, this.Game.Interact, this.Game.Config, this.Game);

            MapTests.NoInitializeAsserts(_setup.TestMap);
        }

        public static void NoInitializeAsserts(IMap testMap)
        {
            Assert.IsInstanceOf(typeof (Map), testMap);
            Assert.IsNull(testMap.Sectors);
            Assert.IsNotInstanceOf(typeof (Ship), testMap.Playership);
            Assert.IsNotInstanceOf(typeof (Sector), testMap.Sectors);

            Assert.AreEqual(testMap.HostilesToSetUp, 0);
            Assert.AreEqual(testMap.Stardate, 0);
            Assert.AreEqual(testMap.timeRemaining, 0);
            Assert.AreEqual(testMap.starbases, 0);
        }

        /// <summary>
        /// this is really very similar to GenerateEmptySectors.  Just a different path.
        /// </summary>
        [Ignore("")]
        [Test]
        public void Generate_WithNoObjects()
        {
            _setup.TestMap = new Map(new SetupOptions()
                        {
                            
                            Initialize = true,
                            CoordinateDefs = new CoordinateDefs()
                        }, this.Game.Interact, this.Game.Config, this.Game);

            //_setup.TestMap.Sectors.PopulateSectors(null, _setup.TestMap);

            Assert.AreEqual(64, _setup.TestMap.Sectors.GetActive().Coordinates.Count);       
        }

        /// <summary>
        /// this is really very similar to GenerateEmptySectors.  Just a different path.
        /// </summary>
        [Test]
        public void Generate_WithNoObjectsDefaultCount()
        {
            _setup.TestMap = new Map(new SetupOptions()
            {
                
                Initialize = true,
                CoordinateDefs = new CoordinateDefs()
            }, this.Game.Interact, this.Game.Config, this.Game);

            Assert.AreEqual(64, _setup.TestMap.Sectors.GetActive().Coordinates.Count);
        }

        [Ignore("")]
        [Test]
        public void Generate_WithPlayerShip()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip), //todo: this needs to be in a random spo
                }
            }, this.Game.Interact, this.Game.Config, this.Game); 

            //_setup.TestMap.Sectors.PopulateSectors(_setup.TestMap.GameConfig.CoordinateDefs, _setup.TestMap);

            this.VerifyPlayerShipSettings();

            //More comprehensive tests of Coordinates will be in SectorTests
            Assert.AreEqual(64, _setup.TestMap.Sectors.GetActive().Coordinates.Count);
            Assert.Greater(_setup.TestMap.Sectors.GetActive().GetHostiles().Count, -1); 
        }

        [Ignore("")]
        [Test]
        public void Generate_WithHostiles()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip), //todo: this needs to be in a random spo
                }
            }, this.Game.Interact, this.Game.Config, this.Game);
            //_setup.TestMap.Sectors.PopulateSectors(null, _setup.TestMap);

            this.VerifyPlayerShipSettings();

            //More comprehensive tests of Coordinates will be in SectorTests
            Assert.AreEqual(64, _setup.TestMap.Sectors.GetActive().Coordinates.Count);
            Assert.AreEqual(64, _setup.TestMap.Sectors.GetActive().Coordinates.Count);

            Assert.Greater(_setup.TestMap.Sectors.GetActive().GetHostiles().Count, 0);
        }
        
        [Test]
        public void CreateEmptySectors()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs(),
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            var systemNames = new StarTrekKGSettings().GetStarSystems();
            var activeSector = _setup.TestMap.Sectors.GetActive();
            activeSector.InitializeCoordinates(activeSector, new List<Coordinate>(), new Stack<string>(systemNames), FactionName.TestFaction, false);

            Assert.AreEqual(64, _setup.TestMap.Sectors.GetActive().Coordinates.Count);  

            this.VerifyAllEmpty();
        }

        [Test]
        public void EmptyActive()
        {
            Map newMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs()
            }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestMap = newMap;

            _setup.TestMap.Sectors.ClearActive();

            Assert.That(() => _setup.TestMap.Sectors.GetActive(), Throws.TypeOf<GameConfigException>(), "No Sector has been set Active - This would happen if there are no friendlies on the map.");
        }

        [Ignore("")] //should be: CreateSectorObjects
        [Test]
        public void CreateSectorItems()
        {
            _setup.TestMap = new Map(null, this.Game.Interact, this.Game.Config, this.Game);
            Assert.IsNull(_setup.TestMap.Sectors.GetActive().Coordinates);

            _setup.TestMap.Sectors.GetActive().Coordinates = new Coordinates(); //[8, 8];


            //_setup.TestMap.CreateSectorItems(1); //CreateSectorObjects

            Assert.IsNotNull(_setup.TestMap.Sectors.GetActive().Coordinates);
            Assert.AreEqual(64, _setup.TestMap.Sectors.GetActive().Coordinates.Count);   
        }
        
        [Ignore("")]
        [Test]
        public void ReadSector()
        {
            

        }

        //IsDockingLocation
            //has its own test fixture

        [Ignore("")] //todo: fix this.
        [Test]
        public void IsSectorRegionEmpty()
        {
            _setup.TestMap = new Map(null, this.Game.Interact, this.Game.Config, this.Game);

            Assert.IsNull(_setup.TestMap.Sectors.GetActive().Coordinates);

            //Sector.InitializeSectors(_setup.TestMap.Sectors.GetActive(), null);

            //They should all be empty
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //_setup.TestMap.IsSectorRegionEmpty(i, j, _setup.TestMap.Sectors.Active.Coordinates);
                }
            }
        }
        
        [Test]
        public void Remove()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip), //todo: this needs to be in a random spo
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            //Assert.AreEqual(CoordinateItem.Friendly, _setup.TestMap.GetItem(0, 1, 0, 0)); //verify our newly added friendly ship is on the map

            //todo: refactor this into CreateHostile  //-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0

            const int testRegionX = 0;
            const int testRegionY = 1;
            const int testSectX = 2;
            const int testSectY = 7;

            //add a ship to remove
            var sector = new Coordinate(new LocationDef(new Point(testRegionX, testRegionY), new Point(testSectX, testSectY)));
            var hostileShip = new Ship(FactionName.Klingon, "this is the ship", sector, this.Game.Map);

            _setup.TestMap.Sectors.Single(q => q.X == hostileShip.Point.X &&
                                           q.Y == hostileShip.Point.Y).AddShip(hostileShip, sector);

            _setup.TestMap.Sectors.Single(q => q.X == testRegionX &&
                                           q.Y == testRegionY).Coordinates.Single(s => s.X == testSectX &&
                                                                                 s.Y == testSectY).Item = CoordinateItem.HostileShip;
            //-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0

            var hostiles1 = _setup.TestMap.Sectors.Single(q => q.X == hostileShip.Point.X &&
                                                           q.Y == hostileShip.Point.Y).GetHostiles();

            var verifiedShip = hostiles1.Single(s => s.Coordinate.X == sector.X && s.Coordinate.Y == sector.Y && s.Name == "this is the ship");
            Assert.IsNotNull(verifiedShip);

            var sectorItemBefore = _setup.TestMap.GetItem(testRegionX, testRegionY, testSectX, testSectY);
            Assert.AreEqual(CoordinateItem.HostileShip, sectorItemBefore); //verify our newly added ship is on the map

            //verify our newly added ship is in the Hostiles list
            var shipToRemove = _setup.TestMap.Sectors.Single(q => q.X == hostileShip.Point.X &&
                                                              q.Y == hostileShip.Point.Y).GetHostiles().Single(h => 
                                                                                   h.Coordinate.X == hostileShip.Coordinate.X &&
                                                                                   h.Coordinate.Y == hostileShip.Coordinate.Y &&
                                                                                   h.Allegiance == Allegiance.BadGuy &&
                                                                                   h.Name == hostileShip.Name);
            Assert.IsInstanceOf<Ship>(shipToRemove);

            //todo: verify with assert that map is set up with sectors and Sectors
            _setup.TestMap.Sectors.Remove(hostileShip);

            var sectorItemAfter = _setup.TestMap.GetItem(testRegionX, testRegionY, testSectX, testSectY);
            Assert.AreEqual(CoordinateItem.Empty, sectorItemAfter);

            //todo: Sector.Hostiles not sync'd or something?
            var hostilesAfter = _setup.TestMap.Sectors.GetActive().GetHostiles();
            Assert.IsEmpty(hostilesAfter);

            //todo: finish map.GetShip()
        }
 
        [Test]
        public void Remove2()
        {
            _setup.SetupMapWith1Friendly();

            //_setup.TestMap = (new Map(new GameConfig
            //{
            //    Initialize = true,
                
            //    CoordinateDefs = new CoordinateDefs
            //    {
            //        new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.Friendly), //todo: this needs to be in a random spo
            //    }
            //}, this.Write));

            //add a ship
            var ship = new Coordinate(new LocationDef(new Point(0, 0), new Point(2, 7)));
            var hostileShip = new Ship(FactionName.Klingon, "this is the ship", ship, this.Game.Map);

            _setup.TestMap.Sectors.GetActive().AddShip(hostileShip, hostileShip.Coordinate);

            var verifiedShip = _setup.TestMap.Sectors.GetActive().GetHostiles().Single(s => s.Coordinate.X == ship.X && s.Coordinate.Y == ship.Y && s.Name == "this is the ship");

            Assert.IsNotNull(verifiedShip);

            //todo: verify with assert that map is set up with sectors and Sectors
            //_setup.TestMap.Remove(_setup.TestMap.Sectors.Active.Hostiles);// get rid of all hostiles on map

            _setup.TestMap.Sectors.GetActive().ClearHostiles();

            var verifiedGone = _setup.TestMap.Sectors.GetActive().GetHostiles().SingleOrDefault(s => s.Coordinate.X == ship.X && s.Coordinate.Y == ship.Y && s.Name == "this is the ship");

            Assert.IsEmpty(_setup.TestMap.Sectors.GetActive().GetHostiles());
        }

        [Test]
        public void Remove3()
        {
            _setup.TestMap = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip), //todo: this needs to be in a random spo
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            //add a ship
            var hostileShip = new Ship(FactionName.Klingon, "ship1", new Coordinate(new LocationDef(new Point(0, 0), new Point(2, 7))), this.Game.Map);
            var hostileShip2 = new Ship(FactionName.Klingon, "ship2", new Coordinate(new LocationDef(new Point(0, 0), new Point(2, 2))), this.Game.Map);

            _setup.TestMap.Sectors.GetActive().AddShip(hostileShip, hostileShip.Coordinate);
            _setup.TestMap.Sectors.GetActive().AddShip(hostileShip2, hostileShip2.Coordinate);

            Assert.IsNotNull(_setup.TestMap.Sectors.GetActive().GetHostiles().Single(s => s.Name == "ship1"));
            Assert.IsNotNull(_setup.TestMap.Sectors.GetActive().GetHostiles().Single(s => s.Name == "ship2"));

            _setup.TestMap.Sectors.RemoveShipFromMap("ship1");

            Assert.AreEqual(1, _setup.TestMap.Sectors.GetActive().GetHostiles().Count);

            _setup.TestMap.Sectors.RemoveShipFromMap("ship2");
            Assert.AreEqual(0, _setup.TestMap.Sectors.GetActive().GetHostiles().Count);
        }

        [Test]
        public void Distance()
        {
            const double oneDiagonalSector = 1; //1.4142135623730951;
            const double twoDiagonalSectors = 3;// 2.8284271247461903;

            Assert.AreEqual(twoDiagonalSectors, Utility.Distance(0, 0, 2, 2));
            Assert.AreEqual(twoDiagonalSectors, Utility.Distance(2, 2, 0, 0));
            Assert.AreEqual(oneDiagonalSector, Utility.Distance(1, 1, 0, 0));
            Assert.AreEqual(twoDiagonalSectors, Utility.Distance(1, 2, 3, 4));

            Assert.AreEqual(1.0, Utility.Distance(0, 0, 0, 1));
            Assert.AreEqual(2.0, Utility.Distance(0, 0, 0, 2));
            Assert.AreEqual(3.0, Utility.Distance(0, 0, 0, 3));
            Assert.AreEqual(4.0, Utility.Distance(0, 0, 0, 4));
            Assert.AreEqual(8.0, Utility.Distance(0, 0, 0, 8));
        }

        [Test]
        public void ComputeDirection()
        {
            Assert.AreEqual(8.0, Utility.ComputeDirection(0, 0, 2, 2));
            Assert.AreEqual(4.0, Utility.ComputeDirection(2, 2, 0, 0));
            Assert.AreEqual(4.0, Utility.ComputeDirection(1, 1, 0, 0));
            Assert.AreEqual(8.0, Utility.ComputeDirection(1, 2, 3, 4));
        }

        [Test]
        public void ComputeDirection2()
        {
            //Covering the rest of the paths

            Assert.AreEqual(7.0, Utility.ComputeDirection(0, 0, 0, 2));
            Assert.AreEqual(1.0, Utility.ComputeDirection(0, 0, 2, 0));

            Assert.AreEqual(7.5903344706017331d, Utility.ComputeDirection(0, 0, 2, 4));
            Assert.AreEqual(1.5903344706017331d, Utility.ComputeDirection(0, 1, 2, 0));

            Assert.AreEqual(6.0, Utility.ComputeDirection(2, 0, 1, 1));
        }

        [Ignore("")]
        [Test]
        public void StarbaseCalculator()
        {
            Assert.Fail();
        }
        public void VerifyGlobalInfoSettings()
        {
            //todo: Assert for each item set

            Assert.IsInstanceOf(typeof(List<IShip>), _setup.TestMap.Sectors.GetActive().GetHostiles());
            Assert.AreEqual(0, _setup.TestMap.Sectors.GetActive().GetHostiles().Count);

            Assert.Greater(_setup.TestMap.HostilesToSetUp, -1);
            Assert.Greater(_setup.TestMap.Stardate, -1);
            Assert.Greater(_setup.TestMap.timeRemaining, -1);
            Assert.Greater(_setup.TestMap.starbases, -1);        
        }
        public void VerifyInitializeSettings()
        {
            this.VerifyGlobalInfoSettings();
            
            this.InitializeRegions();

            this.VerifyPlayerShipSettings();
        }
        private void VerifyPlayerShipSettings()
        {
            Assert.IsInstanceOf(typeof (Ship), _setup.TestMap.Playership);

            Assert.Greater(_setup.TestMap.Playership.Coordinate.X, -1);
            Assert.Greater(_setup.TestMap.Playership.Coordinate.Y, -1);

            Assert.AreEqual(_setup.TestMap.Playership.Allegiance, Allegiance.GoodGuy);
        }
        private void VerifyAllEmpty()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Assert.AreEqual(CoordinateItem.Empty, _setup.TestMap.Sectors.GetActive().Coordinates[i, j].Item);
                }
            }
        }
    }
}
