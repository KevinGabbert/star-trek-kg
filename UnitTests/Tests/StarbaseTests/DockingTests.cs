using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

namespace UnitTests.ShipTests.StarbaseTests
{
    public class DockingTests: TestClass_Base
    {
        Map _testMapNoObjects;
        Movement _testMovement;
        Coordinate _startingRegion;

        int _startingSectorX;
        int _startingSectorY;

        private int _lastRegionX;
        private int _lastRegionY;

        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
        }

        [TearDown]
        public void TearDown()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.Region_MIN = 0;
            Constants.Region_MAX = 0;
        }


        private void SetupMapWithStarbase()
        {
            _testMapNoObjects = (new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.PlayerShip), //todo: this needs to be in a random spo
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 2)), SectorItem.Starbase)
                                    }
            }, this.Game.Write, this.Game.Config));

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));

            //var activeRegion = _testMap.Regions.GetActive();
            //activeRegion.AddShip(starbase, starbase.Sector);
        }

        [Test]
        public void ALLHostilesAttack_ShipUndocked_WithShields()
        {
            this.SetupMapWithStarbase();
        }

        [Test]
        public void Dock_To_Starbase()
        {
            this.SetupMapWithStarbase();


            //todo: modify so that prompt value can be passed in
            //Navigation.For(_testMapNoObjects.Playership).Controls("");

            //this.Move_Sector(((int)NavDirection.East).ToString(), .2 * 8);
        }

        //private void Move_Sector(string direction, int distance)
        //{
        //    var playershipRegion = _testMapNoObjects.Playership.GetRegion();

        //    _startingRegion = new Coordinate(playershipRegion.X, playershipRegion.Y);

        //    _startingSectorX = _testMapNoObjects.Playership.Sector.X;
        //    _startingSectorY = _testMapNoObjects.Playership.Sector.Y;

        //    //verify that the ship is where we think it is before we start
        //    Assert.AreEqual(SectorItem.Friendly, Sector.Get(_testMapNoObjects.Regions.GetActive().Sectors,
        //                                                   _testMapNoObjects.Playership.Sector.X,
        //                                                   _testMapNoObjects.Playership.Sector.Y).Item);

        //    _testMovement = new Movement(_testMapNoObjects.Playership, this.Game);
        //    _testMovement.BlockedByObstacle = false;

        //    var sectorItem =
        //        Sector.Get(_testMovement.Game.Map.Regions.GetActive().Sectors, _testMovement.Game.Map.Playership.Sector.X,
        //                                                               _testMovement.Game.Map.Playership.Sector.Y).Item;
        //    Assert.AreEqual(SectorItem.Friendly, sectorItem);

        //    _testMovement.Execute(Convert.ToInt32(direction), distance, distance / 8, out _lastRegionX, out _lastRegionY);

        //    //EnergySubtracted changes an entered value of .1 to .8
        //    //todo: measure time passed
        //}
    }
}
