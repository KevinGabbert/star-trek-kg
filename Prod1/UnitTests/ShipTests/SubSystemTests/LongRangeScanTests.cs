using System;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class LongRangeScanTests
    {
        private LongRangeScan _testLongRangeScanner;
        private Map _testLRSMap;

        [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();

            _testLRSMap = new Map(new GameConfig
                                      {
                                          Initialize = true,
                                          SectorDefs = new SectorDefs
                                                           {
                                                               new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly),
                                                               new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.Hostile),
                                                           },
                                          AddStars = false
                                      });

            _testLongRangeScanner = new LongRangeScan(_testLRSMap, _testLRSMap.Playership);
            Assert.AreEqual(SubsystemType.LongRangeScan, _testLongRangeScanner.Type);
        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;

            _testLRSMap = null;
        }

        [Ignore]
        [Test]
        public void GetStarInfoFromScannerFailsWithRepeatOnly()
        {
            for (int i = 0; i < 100; i++)
            {
                Command.Console.Write("-");
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
            _testLRSMap = new Map(new GameConfig
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
                                      });

            _testLongRangeScanner = new LongRangeScan(_testLRSMap, _testLRSMap.Playership);

            Quadrant quadrant = Quadrants.Get(_testLRSMap, new Coordinate(0,0));
            int starCount = quadrant.GetStarCount();
            Assert.AreEqual(2, starCount);
        }

        private void CheckStarsWithScanner()
        {
            _testLRSMap = new Map(new GameConfig
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
            });

            _testLongRangeScanner = new LongRangeScan(_testLRSMap, _testLRSMap.Playership);

            int starbaseCount;
            int starCount;
            int hostileCount;
            LongRangeScan.GetMapInfoForScanner(_testLRSMap, new Coordinate(0, 0), out hostileCount, out starbaseCount, out starCount);
            //pulls count from Quadrant object

            Assert.AreEqual(2, starCount);
        }


        [Test]
        public void GetInfoFromScanner00()
        {
            _testLRSMap = new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly)
                    },
                AddStars = false
            });

            _testLongRangeScanner = new LongRangeScan(_testLRSMap, _testLRSMap.Playership);
            _testLongRangeScanner.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }

        [Test]
        public void GetInfoFromScanner44()
        {
            _testLRSMap = new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(4,4), new Coordinate(4, 4)), SectorItem.Friendly)
                    },
                AddStars = false
            });

            _testLongRangeScanner = new LongRangeScan(_testLRSMap, _testLRSMap.Playership);
            _testLongRangeScanner.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }

        [Test]
        public void GetInfoFromScannerMaxMax()
        {
            _testLRSMap = new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(Constants.QUADRANT_MAX - 1, Constants.QUADRANT_MAX - 1), new Coordinate(Constants.QUADRANT_MAX - 1, Constants.QUADRANT_MAX - 1)), SectorItem.Friendly)
                    },
                AddStars = false
            });

            _testLongRangeScanner = new LongRangeScan(_testLRSMap, _testLRSMap.Playership);
            _testLongRangeScanner.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }


        [Test]
        public void GetHostileInfoFromScanner()
        {
            _testLRSMap = new Map(new GameConfig
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.Friendly),

                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.Hostile),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 2)), SectorItem.Hostile)
                    },
                AddStars = false
            });

            _testLongRangeScanner = new LongRangeScan(_testLRSMap, _testLRSMap.Playership);

            var starbaseCount = 0;
            var starCount = 0;
            var hostileCount = 0;
            LongRangeScan.GetMapInfoForScanner(_testLRSMap, new Coordinate(0, 0), out hostileCount, out starbaseCount, out starCount); //pulls count from Quadrant object

            Assert.AreEqual(2, hostileCount);
        }

        [Ignore]
        [Test]
        public void GetStarbaseInfoFromScanner()
        {
            _testLRSMap = new Map(new GameConfig
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
            });

            _testLongRangeScanner = new LongRangeScan(_testLRSMap, _testLRSMap.Playership);

            var starbaseCount = 0;
            var starCount = 0;
            var hostileCount = 0;
            LongRangeScan.GetMapInfoForScanner(_testLRSMap, new Coordinate(0, 0), out hostileCount, out starbaseCount, out starCount); //pulls count from Quadrant object

            Assert.AreEqual(5, starbaseCount);
            Assert.AreEqual(0, starCount);
            Assert.AreEqual(1, hostileCount);
        }

        [Test(Description = "Fails when run with Fixture")]
        public void GetStarbaseInfoFromScanner2()
        {
            var game = new Game();

            _testLRSMap = new Map(new GameConfig
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
            });

            int starbaseCount;
            int starCount;
            int hostileCount;
            LongRangeScan.GetMapInfoForScanner(_testLRSMap, new Coordinate(0, 0), out hostileCount, out starbaseCount, out starCount); //pulls count from Quadrant object

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
            LongRangeScan.GetMapInfoForScanner(_testLRSMap, new Coordinate(0, 0), out hostileCount, out starbaseCount, out starCount); //pulls count from Quadrant object

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
