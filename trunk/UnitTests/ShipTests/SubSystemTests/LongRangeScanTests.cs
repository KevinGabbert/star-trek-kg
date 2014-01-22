﻿using System;
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
    public class LongRangeScanTests: TestClass_Base
    {
        private LongRangeScan _testLongRangeScanner;

        [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();

            this.Game.Map = new Map(new SetupOptions
                                      {
                                          Initialize = true,
                                          SectorDefs = new SectorDefs
                                                           {
                                                               new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly),
                                                               new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.Hostile),
                                                           },
                                          AddStars = false
                                      }, this.Game.Write, this.Game.Config);

            _testLongRangeScanner = new LongRangeScan(this.Game.Map.Playership, this.Game);
            Assert.AreEqual(SubsystemType.LongRangeScan, _testLongRangeScanner.Type);
        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;

            this.Game.Map = null;
        }

        [Ignore]
        [Test]
        public void GetStarInfoFromScannerFailsWithRepeatOnly()
        {
            for (int i = 0; i < 100; i++)
            {
                this.Game.Write.Console.Write("-");
                this.CheckStarsInQuadrant();
            }
        }

        [Test]
        public void GetStarInfoFromScanner()
        {
            this.CheckStarsWithScanner();
            this.CheckStarsInQuadrant();
        }

        private void CheckStarsInQuadrant()
        {
            this.Game.Map = new Map(new SetupOptions
                                      {
                                          Initialize = true,
                                          AddStars = false,
                                          SectorDefs = new SectorDefs
                                                    {
                                                        new SectorDef(
                                                            new LocationDef(new Coordinate(0, 0), new Coordinate(0, 4)),
                                                            SectorItem.Star),
                                                        new SectorDef(
                                                            new LocationDef(new Coordinate(0, 0), new Coordinate(0, 5)),
                                                            SectorItem.Star),
                                                    }
                                      }, this.Game.Write, this.Game.Config);

            _testLongRangeScanner = new LongRangeScan(this.Game.Map.Playership, this.Game);

            Quadrant quadrant = Quadrants.Get(this.Game.Map, new Coordinate(0, 0));
            int starCount = quadrant.GetStarCount();
            Assert.AreEqual(2, starCount);
        }

        private void CheckStarsWithScanner()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                AddStars = false,
                SectorDefs = new SectorDefs
                                                    {
                                                        new SectorDef(
                                                            new LocationDef(new Coordinate(0, 0), new Coordinate(0, 4)),
                                                            SectorItem.Star),
                                                        new SectorDef(
                                                            new LocationDef(new Coordinate(0, 0), new Coordinate(0, 5)),
                                                            SectorItem.Star),
                                                    }
            }, this.Game.Write, this.Game.Config);

            _testLongRangeScanner = new LongRangeScan(this.Game.Map.Playership, this.Game);

            int starbaseCount;
            int starCount;
            int hostileCount;
            _testLongRangeScanner.Execute(this.Game.Map.Quadrants[0], out hostileCount, out starbaseCount, out starCount);
            //pulls count from Quadrant object

            Assert.AreEqual(2, starCount);
        }


        [Test]
        public void GetInfoFromScanner00()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            _testLongRangeScanner = new LongRangeScan(this.Game.Map.Playership, this.Game);
            _testLongRangeScanner.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }

        [Test]
        public void GetInfoFromScanner44()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(4,4), new Coordinate(4, 4)), SectorItem.Friendly)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            _testLongRangeScanner = new LongRangeScan(this.Game.Map.Playership, this.Game);
            _testLongRangeScanner.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }

        [Test]
        public void GetInfoFromScannerMaxMax()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(Constants.QUADRANT_MAX - 1, Constants.QUADRANT_MAX - 1), new Coordinate(Constants.QUADRANT_MAX - 1, Constants.QUADRANT_MAX - 1)), SectorItem.Friendly)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            _testLongRangeScanner = new LongRangeScan(this.Game.Map.Playership, this.Game);
            _testLongRangeScanner.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }


        [Test]
        public void GetHostileInfoFromScanner()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly),

                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.Hostile),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 2)), SectorItem.Hostile)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            _testLongRangeScanner = new LongRangeScan(this.Game.Map.Playership, this.Game);

            var starbaseCount = 0;
            var starCount = 0;
            var hostileCount = 0;
            _testLongRangeScanner.Execute(this.Game.Map.Quadrants[0], out hostileCount, out starbaseCount, out starCount); //pulls count from Quadrant object

            Assert.AreEqual(2, hostileCount);
        }

        [Ignore]
        [Test]
        public void GetStarbaseInfoFromScanner()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.Hostile),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 2)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 3)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 4)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 5)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 6)), SectorItem.Starbase)
                    },
                    AddStars = false
            }, this.Game.Write, this.Game.Config);

            _testLongRangeScanner = new LongRangeScan(this.Game.Map.Playership, this.Game);

            var starbaseCount = 0;
            var starCount = 0;
            var hostileCount = 0;
            _testLongRangeScanner.Execute(this.Game.Map.Quadrants[0], out hostileCount, out starbaseCount, out starCount); //pulls count from Quadrant object

            Assert.AreEqual(5, starbaseCount);
            Assert.AreEqual(0, starCount);
            Assert.AreEqual(1, hostileCount);
        }

        [Test(Description = "Fails when run with Fixture")]
        public void GetStarbaseInfoFromScanner2()
        {
            var game = new Game((new StarTrekKGSettings()));

            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.Hostile),

                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 2)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 3)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 4)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 5)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 6)), SectorItem.Starbase)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            int starbaseCount;
            int starCount;
            int hostileCount;
            LongRangeScan.For(this.Game.Map.Playership).Execute(this.Game.Map.Quadrants[0], out hostileCount, out starbaseCount, out starCount); //pulls count from Quadrant object

            Assert.AreEqual(5, starbaseCount);
        }

        [Ignore]
        [Test]
        public void GetMapInfoForScanner()
        {
            //todo: fix hostiles and starbases and stars to test fully

            var starbaseCount = 0;
            var starCount = 0;
            var hostileCount = 0;
            LongRangeScan.For(this.Game.Map.Playership).Execute(this.Game.Map.Quadrants[0], out hostileCount, out starbaseCount, out starCount); //pulls count from Quadrant object

            Assert.Greater(0, hostileCount);
            Assert.Greater(0, starbaseCount);
            Assert.Greater(0, starCount);
        }

        [Test]
        public void ControlsDamaged()
        {
            _testLongRangeScanner.Damage = 47;
            _testLongRangeScanner.Controls();
            Assert.IsTrue(_testLongRangeScanner.Damaged());
        }
    }
}
