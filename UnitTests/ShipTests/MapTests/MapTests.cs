﻿using System.Collections.Generic;
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
using StarTrek_KG.Utility;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.MapTests
{
    [TestFixture]
    public class MapTests: TestClass_Base
    {
        [SetUp]
        public void Setup()
        {
             Assert.IsInstanceOf(typeof(Map), _setup.TestMap);
            //todo: call VerifyGlobalInfoSettings

             TestRunner.GetTestConstants();
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
        public void InitializeSectors()
        {
            _setup.TestMap.Initialize(new SectorDefs
                                            {
                                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly),
                                                //todo: this needs to be in a random spo
                                            });

            //More comprehensive Quadrant tests is in QuadrantTests
            Assert.AreEqual(64, _setup.TestMap.Quadrants.Count); //currently these reside in constants, but will be moving to app.config

            foreach (var quadrant in _setup.TestMap.Quadrants)
            {
                Assert.AreEqual(64, quadrant.Sectors.Count, "Unexpected sector count: Quadrant X: " + quadrant.X + " Y: " + quadrant.Y);
            }
            
            this.VerifyInitializeSettings();
        }

        [Ignore]
        [Test]
        public void CreateTooManyStars()
        {
                //Todo: test this
        }


        [Test]
        public void InitializeQuadrants()
        {
            var klingonShipNames =  StarTrekKGSettings.GetShips("Klingon");
            var systemNames = StarTrekKGSettings.GetStarSystems();
            _setup.TestMap.InitializeQuadrantsWithBaddies(new Stack<string>(systemNames),
                                         new Stack<string>(klingonShipNames),
                                         new SectorDefs());

            Assert.IsInstanceOf(typeof(Map), _setup.TestMap);
            Assert.Greater(_setup.TestMap.Quadrants.Count, 63); //todo: currently these reside in constants, but will be moving to app.config

            ////todo: Assert that no baddies or friendlies are set up.
        }

        [Ignore]
        [Test]
        public void PopulateWithHostilesAndStarbases()
        {
            var klingonShipNames = StarTrekKGSettings.GetShips("Klingon");
            var systemNames = StarTrekKGSettings.GetStarSystems();

            _setup.TestMap.InitializeQuadrantsWithBaddies(new Stack<string>(systemNames),
                                         new Stack<string>(klingonShipNames),
                                         null);
            //Quadrant.Populate(_setup.TestMap);

            Assert.Greater(_setup.TestMap.Quadrants.Count, 63); //currently these reside in constants, but will be moving to app.config


            //todo: query quadrants for number of starbases (change quadrants to a collection!) and get rid of "starbases" variable)
        }
        
        [Test]
        public void GetGlobalInfo()
        {
            _setup.TestMap.GetGlobalInfo(); 
            this.VerifyGlobalInfoSettings();
        }

        [Ignore]
        [Test]
        public void VerifyCommandLineText()
        {
            _setup.TestMap.GetGlobalInfo(); 
            Assert.AreEqual("Enter command: ", _setup.TestMap.Text); 
        }

        [Test]
        public void Generate_NoIntitialize()
        {
            _setup.TestMap = new Map(null, this.Write, this.Command);

            MapTests.NoInitializeAsserts(_setup.TestMap);
        }

        public static void NoInitializeAsserts(Map testMap)
        {
            Assert.IsInstanceOf(typeof (Map), testMap);
            Assert.IsNull(testMap.Quadrants);
            Assert.IsNotInstanceOf(typeof (Ship), testMap.Playership);
            Assert.IsNotInstanceOf(typeof (Quadrant), testMap.Quadrants);

            Assert.AreEqual(testMap.hostilesToSetUp, 0);
            Assert.AreEqual(testMap.Stardate, 0);
            Assert.AreEqual(testMap.timeRemaining, 0);
            Assert.AreEqual(testMap.starbases, 0);
        }

        /// <summary>
        /// this is really very similar to GenerateEmptySectors.  Just a different path.
        /// </summary>
        [Ignore]
        [Test]
        public void Generate_WithNoObjects()
        {
            _setup.TestMap = new Map(new GameConfig()
                        {
                            
                            Initialize = true,
                            SectorDefs = new SectorDefs()
                        }, this.Write, this.Command);

            //_setup.TestMap.Quadrants.PopulateSectors(null, _setup.TestMap);

            Assert.AreEqual(64, _setup.TestMap.Quadrants.GetActive().Sectors.Count);       
        }

        /// <summary>
        /// this is really very similar to GenerateEmptySectors.  Just a different path.
        /// </summary>
        [Test]
        public void Generate_WithNoObjectsDefaultCount()
        {
            _setup.TestMap = new Map(new GameConfig()
            {
                
                Initialize = true,
                SectorDefs = new SectorDefs()
            }, this.Write, this.Command);

            Assert.AreEqual(64, _setup.TestMap.Quadrants.GetActive().Sectors.Count);
        }

        [Ignore]
        [Test]
        public void Generate_WithPlayerShip()
        {
            _setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs
                            {
                                new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                            }
            }, this.Write, this.Command)); 

            //_setup.TestMap.Quadrants.PopulateSectors(_setup.TestMap.GameConfig.SectorDefs, _setup.TestMap);

            this.VerifyPlayerShipSettings();

            //More comprehensive tests of Sectors will be in SectorTests
            Assert.AreEqual(64, _setup.TestMap.Quadrants.GetActive().Sectors.Count);
            Assert.Greater(_setup.TestMap.Quadrants.GetActive().GetHostiles().Count, -1); 
        }

        [Ignore]
        [Test]
        public void Generate_WithHostiles()
        {
            _setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                    }
            }, this.Write, this.Command));
            //_setup.TestMap.Quadrants.PopulateSectors(null, _setup.TestMap);

            this.VerifyPlayerShipSettings();

            //More comprehensive tests of Sectors will be in SectorTests
            Assert.AreEqual(64, _setup.TestMap.Quadrants.GetActive().Sectors.Count);
            Assert.AreEqual(64, _setup.TestMap.Quadrants.GetActive().Sectors.Count);

            Assert.Greater(_setup.TestMap.Quadrants.GetActive().GetHostiles().Count, 0);
        }
        
        [Test]
        public void CreateEmptySectors()
        {
            _setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs(),
                AddStars = false
            }, this.Write, this.Command));

            var systemNames = StarTrekKGSettings.GetStarSystems();
            _setup.TestMap.Quadrants.GetActive().InitializeSectors(_setup.TestMap.Quadrants.GetActive(), null, new Stack<string>(systemNames), _setup.TestMap, false);

            Assert.AreEqual(64, _setup.TestMap.Quadrants.GetActive().Sectors.Count);  

            this.VerifyAllEmpty();
        }

        [ExpectedException(typeof(GameConfigException))]
        [Test]
        public void EmptyActive()
        {
            _setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs()
            }, this.Write, this.Command));

            _setup.TestMap.Quadrants.ClearActive();

            _setup.TestMap.Quadrants.GetActive();
        }

        [Ignore] //should be: CreateSectorObjects
        [Test]
        public void CreateSectorItems()
        {
            _setup.TestMap = new Map(null, this.Write, this.Command);
            Assert.IsNull(_setup.TestMap.Quadrants.GetActive().Sectors);

            _setup.TestMap.Quadrants.GetActive().Sectors = new Sectors(); //[8, 8];


            //_setup.TestMap.CreateSectorItems(1); //CreateSectorObjects

            Assert.IsNotNull(_setup.TestMap.Quadrants.GetActive().Sectors);
            Assert.AreEqual(64, _setup.TestMap.Quadrants.GetActive().Sectors.Count);   
        }
        
        [Ignore]
        [Test]
        public void ReadSector()
        {
            

        }

        //IsDockingLocation
            //has its own test fixture

        [Ignore] //todo: fix this.
        [Test]
        public void IsSectorRegionEmpty()
        {
            _setup.TestMap = new Map(null, this.Write, this.Command);

            Assert.IsNull(_setup.TestMap.Quadrants.GetActive().Sectors);

            //Quadrant.InitializeSectors(_setup.TestMap.Quadrants.GetActive(), null);

            //They should all be empty
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //_setup.TestMap.IsSectorRegionEmpty(i, j, _setup.TestMap.Quadrants.Active.Sectors);
                }
            }
        }
        
        [Test]
        public void Remove()
        {
            _setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                    }
            }, this.Write, this.Command));

            //Assert.AreEqual(SectorItem.Friendly, _setup.TestMap.GetItem(0, 1, 0, 0)); //verify our newly added friendly ship is on the map

            //todo: refactor this into CreateHostile  //-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0

            const int testQuadX = 0;
            const int testQuadY = 1;
            const int testSectX = 2;
            const int testSectY = 7;

            //add a ship to remove
            var sector = new Sector(new LocationDef(new Coordinate(testQuadX, testQuadY), new Coordinate(testSectX, testSectY)));
            var hostileShip = new Ship("this is the ship", _setup.TestMap, sector, this.Write, this.Command);

            _setup.TestMap.Quadrants.Single(q => q.X == hostileShip.QuadrantDef.X &&
                                           q.Y == hostileShip.QuadrantDef.Y).AddShip(hostileShip, sector);

            _setup.TestMap.Quadrants.Single(q => q.X == testQuadX &&
                                           q.Y == testQuadY).Sectors.Single(s => s.X == testSectX &&
                                                                                 s.Y == testSectY).Item = SectorItem.Hostile;
            //-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0

            var hostiles1 = _setup.TestMap.Quadrants.Single(q => q.X == hostileShip.QuadrantDef.X &&
                                                           q.Y == hostileShip.QuadrantDef.Y).GetHostiles();

            var verifiedShip = hostiles1.Single(s => s.Sector.X == sector.X && s.Sector.Y == sector.Y && s.Name == "this is the ship");
            Assert.IsNotNull(verifiedShip);

            var sectorItemBefore = _setup.TestMap.GetItem(testQuadX, testQuadY, testSectX, testSectY);
            Assert.AreEqual(SectorItem.Hostile, sectorItemBefore); //verify our newly added ship is on the map

            //verify our newly added ship is in the Hostiles list
            var shipToRemove = _setup.TestMap.Quadrants.Single(q => q.X == hostileShip.QuadrantDef.X &&
                                                              q.Y == hostileShip.QuadrantDef.Y).GetHostiles().Single(h => 
                                                                                   h.Sector.X == hostileShip.Sector.X &&
                                                                                   h.Sector.Y == hostileShip.Sector.Y &&
                                                                                   h.Allegiance == Allegiance.BadGuy &&
                                                                                   h.Name == hostileShip.Name);
            Assert.IsInstanceOf<Ship>(shipToRemove);

            //todo: verify with assert that map is set up with sectors and quadrants
            _setup.TestMap.Quadrants.Remove(hostileShip, _setup.TestMap);

            var sectorItemAfter = _setup.TestMap.GetItem(testQuadX, testQuadY, testSectX, testSectY);
            Assert.AreEqual(SectorItem.Empty, sectorItemAfter);

            //todo: Quadrant.Hostiles not sync'd or something?
            var hostilesAfter = _setup.TestMap.Quadrants.GetActive().GetHostiles();
            Assert.IsEmpty(hostilesAfter);

            //todo: finish map.GetShip()
        }
 
        [Test]
        public void Remove2()
        {
            _setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                }
            }, this.Write, this.Command));

            //add a ship
            var ship = new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7)));
            var hostileShip = new Ship("this is the ship", _setup.TestMap, ship, this.Write, this.Command);

            _setup.TestMap.Quadrants.GetActive().AddShip(hostileShip, hostileShip.Sector);

            var verifiedShip = _setup.TestMap.Quadrants.GetActive().GetHostiles().Single(s => s.Sector.X == ship.X && s.Sector.Y == ship.Y && s.Name == "this is the ship");

            Assert.IsNotNull(verifiedShip);

            //todo: verify with assert that map is set up with sectors and quadrants
            //_setup.TestMap.Remove(_setup.TestMap.Quadrants.Active.Hostiles);// get rid of all hostiles on map

            _setup.TestMap.Quadrants.GetActive().ClearHostiles();

            var verifiedGone = _setup.TestMap.Quadrants.GetActive().GetHostiles().SingleOrDefault(s => s.Sector.X == ship.X && s.Sector.Y == ship.Y && s.Name == "this is the ship");

            Assert.IsEmpty(_setup.TestMap.Quadrants.GetActive().GetHostiles());
        }

        [Test]
        public void Remove3()
        {
            _setup.TestMap = (new Map(new GameConfig
            {
                Initialize = true,
                
                
                SectorDefs = new SectorDefs
                {
                    new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                }
            }, this.Write, this.Command));

            //add a ship
            var hostileShip = new Ship("ship1", _setup.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))), this.Write, this.Command);
            var hostileShip2 = new Ship("ship2", _setup.TestMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 2))), this.Write, this.Command);

            _setup.TestMap.Quadrants.GetActive().AddShip(hostileShip, hostileShip.Sector);
            _setup.TestMap.Quadrants.GetActive().AddShip(hostileShip2, hostileShip2.Sector);

            var x = _setup.TestMap.Quadrants.GetActive().GetHostiles();

            Assert.IsNotNull(_setup.TestMap.Quadrants.GetActive().GetHostiles().Single(s => s.Name == "ship1"));
            Assert.IsNotNull(_setup.TestMap.Quadrants.GetActive().GetHostiles().Single(s => s.Name == "ship2"));

            _setup.TestMap.Quadrants.RemoveShip("ship1");

            //var verifiedGone = _setup.TestMap.Quadrants.Active.Hostiles.SingleOrDefault(s => s.Name == "this is the ship");

            Assert.AreEqual(1, _setup.TestMap.Quadrants.GetActive().GetHostiles().Count);

            _setup.TestMap.Quadrants.RemoveShip("ship2");
            Assert.AreEqual(0, _setup.TestMap.Quadrants.GetActive().GetHostiles().Count);
        }

        [Test]
        public void Distance()
        {
            const double oneDiagonalSector = 1.4142135623730951;
            const double twoDiagonalSectors = 2.8284271247461903;

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

        [Ignore]
        [Test]
        public void StarbaseCalculator()
        {
            Assert.Fail();
        }
        public void VerifyGlobalInfoSettings()
        {
            //todo: Assert for each item set

            Assert.IsInstanceOf(typeof(List<IShip>), _setup.TestMap.Quadrants.GetActive().GetHostiles());
            Assert.AreEqual(0, _setup.TestMap.Quadrants.GetActive().GetHostiles().Count);

            Assert.Greater(_setup.TestMap.hostilesToSetUp, -1);
            Assert.Greater(_setup.TestMap.Stardate, -1);
            Assert.Greater(_setup.TestMap.timeRemaining, -1);
            Assert.Greater(_setup.TestMap.starbases, -1);        
        }
        public void VerifyInitializeSettings()
        {
            this.VerifyGlobalInfoSettings();
            
            this.InitializeQuadrants();

            this.VerifyPlayerShipSettings();
        }
        private void VerifyPlayerShipSettings()
        {
            Assert.IsInstanceOf(typeof (Ship), _setup.TestMap.Playership);

            Assert.Greater(_setup.TestMap.Playership.Sector.X, -1);
            Assert.Greater(_setup.TestMap.Playership.Sector.Y, -1);

            Assert.AreEqual(_setup.TestMap.Playership.Allegiance, Allegiance.GoodGuy);
        }
        private void VerifyAllEmpty()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Assert.AreEqual(SectorItem.Empty, Sector.Get(_setup.TestMap.Quadrants.GetActive().Sectors, i, j).Item);
                }
            }
        }
    }
}
