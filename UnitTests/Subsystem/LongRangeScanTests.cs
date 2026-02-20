using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.TestObjects;

namespace UnitTests.Subsystem
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
                                          CoordinateDefs = new CoordinateDefs
                                                           {
                                                               new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 0)), CoordinateItem.PlayerShip),
                                                               new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 1)), CoordinateItem.HostileShip),
                                                           },
                                          AddStars = false
                                      }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership);
            Assert.AreEqual(SubsystemType.LongRangeScan, _setup.TestLongRangeScan.Type);
        }

        [TearDown]
        public void TearDown()
        {
            DEFAULTS.COORDINATE_MIN = 0;
            DEFAULTS.COORDINATE_MAX = 0;

            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;

            this.Game.Map = null;
        }

        [Ignore("")]
        [Test]
        public void GetStarInfoFromScannerFailsWithRepeatOnly()
        {
            for (int i = 0; i < 100; i++)
            {
                this.Game.Interact.Output.Write("-");
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
        [Ignore("TODO: placeholder")]
        public void DamagedLRSScannerShowsGarbledData()
        {
            //a possible LRS damaged state.  returns random data in some areas.
            //the stars may be incorrect, or the starbases might be.  You can tell it is damaged as these numbers 
            //will appear off, or the names will be garbled.
            Assert.Fail();
        }

        [Test]
        [Ignore("TODO: placeholder")]
        public void DamagedLRSScannerShowsNoData()
        {
            //a possible LRS damaged state.  returns no data.
            Assert.Fail();
        }

        [Test]
        [Ignore("TODO: placeholder")]
        public void DamagedLRSScannerIsNOTInTheProperOrder()
        {
            //basically, this is the unordered state that LRS first returns in.
            //when LRS is damaged, this can be a state in which it winds up.
            //something will state that the LRSS is damaged onscreen.
            Assert.Fail();
        }

        [Test]
        [Ignore("TODO: placeholder")]
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
                CoordinateDefs = new CoordinateDefs
                    {
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 0)), CoordinateItem.PlayerShip)
                    },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership);
            _setup.TestLongRangeScan.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }

        [Test]
        public void GetInfoFromScanner44()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                CoordinateDefs = new CoordinateDefs
                    {
                        new CoordinateDef(new LocationDef(new Point(4,4), new Point(4, 4)), CoordinateItem.PlayerShip)
                    },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership);
            _setup.TestLongRangeScan.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }

        [Test]
        public void GetInfoFromScannerMaxMax()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                CoordinateDefs = new CoordinateDefs
                    {
                        new CoordinateDef(new LocationDef(new Point(DEFAULTS.SECTOR_MAX - 1, DEFAULTS.SECTOR_MAX - 1), new Point(DEFAULTS.SECTOR_MAX - 1, DEFAULTS.SECTOR_MAX - 1)), CoordinateItem.PlayerShip)
                    },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership);
            _setup.TestLongRangeScan.Controls();

            //todo: mock Write, Pass it in, and test its Output
        }

        [Test]
        public void GetHostileInfoFromScanner()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                CoordinateDefs = new CoordinateDefs
                    {
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 0)), CoordinateItem.PlayerShip),

                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 1)), CoordinateItem.HostileShip),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 2)), CoordinateItem.HostileShip)
                    },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership);

            var x = LongRangeScan.Execute(this.Game.Map.Sectors[0]); //pulls count from Sector object

            Assert.AreEqual(2, x.Hostiles);
        }

        [Ignore("")]
        [Test]
        public void GetStarbaseInfoFromScanner()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                    {
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 0)), CoordinateItem.PlayerShip),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 1)), CoordinateItem.HostileShip),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 2)), CoordinateItem.Starbase),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 3)), CoordinateItem.Starbase),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 4)), CoordinateItem.Starbase),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 5)), CoordinateItem.Starbase),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 6)), CoordinateItem.Starbase)
                    },
                    AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership);

            var x = LongRangeScan.Execute(this.Game.Map.Sectors[0]); //pulls count from Sector object

            Assert.AreEqual(5, x.Starbases);
            Assert.AreEqual(0, x.Stars);
            Assert.AreEqual(1, x.Hostiles);
        }

        [Test(Description = "Fails when run with Fixture")]
        public void GetStarbaseInfoFromScanner2()
        {
            var game = new Game(new StarTrekKGSettings());

            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                    {
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 0)), CoordinateItem.PlayerShip),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 1)), CoordinateItem.HostileShip),

                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 2)), CoordinateItem.Starbase),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 3)), CoordinateItem.Starbase),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 4)), CoordinateItem.Starbase),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 5)), CoordinateItem.Starbase),
                        new CoordinateDef(new LocationDef(new Point(0,0), new Point(0, 6)), CoordinateItem.Starbase)
                    },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            var x = LongRangeScan.Execute(this.Game.Map.Sectors[0]); //pulls count from Sector object

            Assert.AreEqual(5, x.Starbases);
        }

        [Ignore("")]
        [Test]
        public void GetMapInfoForScanner()
        {
            //todo: fix hostiles and starbases and stars to test fully

            var x = LongRangeScan.Execute(this.Game.Map.Sectors[0]); //pulls count from Sector object

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

        [Test]
        public void CheckStarsInRegion()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                AddNebulae = false,
                Initialize = true,
                AddStars = false,
                CoordinateDefs = new CoordinateDefs
                                                    {
                                                        new CoordinateDef(
                                                            new LocationDef(new Point(0, 0), new Point(0, 4)),
                                                            CoordinateItem.Star),
                                                        new CoordinateDef(
                                                            new LocationDef(new Point(0, 0), new Point(0, 5)),
                                                            CoordinateItem.Star),
                                                    }
            }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership);

            Sector Sector = this.Game.Map.Sectors[0, 0];
            int starCount = Sector.GetStarCount();
            Assert.AreEqual(2, starCount);
        }

        private void CheckStarsWithScanner()
        {
            this.Game.Map = new Map(new SetupOptions
            {
                Initialize = true,
                AddStars = false,
                CoordinateDefs = new CoordinateDefs
                                                    {
                                                        new CoordinateDef(
                                                            new LocationDef(new Point(0, 0), new Point(0, 4)),
                                                            CoordinateItem.Star),
                                                        new CoordinateDef(
                                                            new LocationDef(new Point(0, 0), new Point(0, 5)),
                                                            CoordinateItem.Star),
                                                    }
            }, this.Game.Interact, this.Game.Config, this.Game);

            _setup.TestLongRangeScan = new LongRangeScan(this.Game.Map.Playership);
            //int starbaseCount;
            //int starCount;
            //int hostileCount;

            var x = LongRangeScan.Execute(this.Game.Map.Sectors[0]);
            //pulls count from Sector object

            Assert.AreEqual(2, x.Stars);
        }
    }
}


