using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.ShipTests;

namespace UnitTests.Tests.SubSystemTests
{
    [TestFixture]
    public class LongRangeScanTests: TestClass_Base
    {
        [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();

            this.Game.Map = new Map(new SetupOptions
                                      {
                                          AddNebulae = true,
                                          Initialize = true,
                                          SectorDefs = new SectorDefs
                                                           {
                                                               new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.PlayerShip),
                                                               new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.HostileShip),
                                                           },
                                          AddStars = false
                                      }, this.Game.Write, this.Game.Config);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership, this.Game);
            Assert.AreEqual(SubsystemType.LongRangeScan, _setup.TestLongRangeScan.Type);
        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.Region_MIN = 0;
            Constants.Region_MAX = 0;

            this.Game.Map = null;
        }

        [Ignore]
        [Test]
        public void GetStarInfoFromScannerFailsWithRepeatOnly()
        {
            for (int i = 0; i < 100; i++)
            {
                this.Game.Write.Console.Write("-");
                this.CheckStarsInRegion();
            }
        }

        [Test]
        public void GetStarInfoFromScanner()
        {
            this.CheckStarsWithScanner();
            this.CheckStarsInRegion();
        }

        [Test]
        public void DamagedLRSScannerShowsGarbledData()
        {
            //a possible LRS damaged state.  returns random data in some areas.
            //the stars may be incorrect, or the starbases might be.  You can tell it is damaged as these numbers 
            //will appear off, or the names will be garbled.
            Assert.Fail();
        }

        [Test]
        public void DamagedLRSScannerShowsNoData()
        {
            //a possible LRS damaged state.  returns no data.
            Assert.Fail();
        }

        [Test]
        public void DamagedLRSScannerIsNOTInTheProperOrder()
        {
            //basically, this is the unordered state that LRS first returns in.
            //when LRS is damaged, this can be a state in which it winds up.
            //something will state that the LRSS is damaged onscreen.
            Assert.Fail();
        }

        [Test]
        public void GetInfoFromScannerIsInTheProperOrder()
        {
            //The returned LRS data is not in its proper order. this will likely require either that the LRS display
            //is ordered properly.
            Assert.Fail();
        }

        [Test]
        public void GetInfoFromScanner00()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.PlayerShip)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership, this.Game);
            _setup.TestLongRangeScan.Controls();

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
                        new SectorDef(new LocationDef(new Coordinate(4,4), new Coordinate(4, 4)), SectorItem.PlayerShip)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership, this.Game);
            _setup.TestLongRangeScan.Controls();

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
                        new SectorDef(new LocationDef(new Coordinate(Constants.Region_MAX - 1, Constants.Region_MAX - 1), new Coordinate(Constants.Region_MAX - 1, Constants.Region_MAX - 1)), SectorItem.PlayerShip)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership, this.Game);
            _setup.TestLongRangeScan.Controls();

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
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.PlayerShip),

                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.HostileShip),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 2)), SectorItem.HostileShip)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership, this.Game);

            var x = LongRangeScan.Execute(this.Game.Map.Regions[0]); //pulls count from Region object

            Assert.AreEqual(2, x.Hostiles);
        }

        [Ignore]
        [Test]
        public void GetStarbaseInfoFromScanner()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.PlayerShip),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.HostileShip),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 2)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 3)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 4)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 5)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 6)), SectorItem.Starbase)
                    },
                    AddStars = false
            }, this.Game.Write, this.Game.Config);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership, this.Game);

            var x = LongRangeScan.Execute(this.Game.Map.Regions[0]); //pulls count from Region object

            Assert.AreEqual(5, x.Starbases);
            Assert.AreEqual(0, x.Stars);
            Assert.AreEqual(1, x.Hostiles);
        }

        [Test(Description = "Fails when run with Fixture")]
        public void GetStarbaseInfoFromScanner2()
        {
            var game = new Game((new StarTrekKGSettings()));

            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                SectorDefs = new SectorDefs
                    {
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 0)), SectorItem.PlayerShip),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 1)), SectorItem.HostileShip),

                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 2)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 3)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 4)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 5)), SectorItem.Starbase),
                        new SectorDef(new LocationDef(new Coordinate(0,0), new Coordinate(0, 6)), SectorItem.Starbase)
                    },
                AddStars = false
            }, this.Game.Write, this.Game.Config);

            var x = LongRangeScan.Execute(this.Game.Map.Regions[0]); //pulls count from Region object

            Assert.AreEqual(5, x.Starbases);
        }

        [Ignore]
        [Test]
        public void GetMapInfoForScanner()
        {
            //todo: fix hostiles and starbases and stars to test fully

            var x = LongRangeScan.Execute(this.Game.Map.Regions[0]); //pulls count from Region object

            Assert.Greater(0, x.Hostiles);
            Assert.Greater(0, x.Starbases);
            Assert.Greater(0, x.Stars);
        }

        [Test]
        public void ControlsDamaged()
        {
            _setup.TestLongRangeScan.Damage = 47;
            _setup.TestLongRangeScan.Controls();
            Assert.IsTrue(_setup.TestLongRangeScan.Damaged());
        }


        private void CheckStarsInRegion()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                AddNebulae = false,
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

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership, this.Game);

            Region Region = Regions.Get(this.Game.Map, new Coordinate(0, 0));
            int starCount = Region.GetStarCount();
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

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership, this.Game);

            int starbaseCount;
            int starCount;
            int hostileCount;

            var x = LongRangeScan.Execute(this.Game.Map.Regions[0]);
            //pulls count from Region object

            Assert.AreEqual(2, x.Stars);
        }
    }
}
