using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Actors.StarbaseTests
{
    public class DockingTests: TestClass_Base
    {
        Map _testMapNoObjects;
        //Movement _testMovement;
        //Point _startingRegion;

        //int _startingSectorX;
        //int _startingSectorY;

        //private int _lastRegionX;
        //private int _lastRegionY;

        [SetUp]
        public void SetUp()
        {
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


        private void SetupMapWithStarbase()
        {
            _testMapNoObjects = new Map(new SetupOptions
            {
                Initialize = true,
                AddNebulae = false,
                CoordinateDefs = new CoordinateDefs
                {
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 0)), CoordinateItem.PlayerShip), //todo: this needs to be in a random spo
                    new CoordinateDef(new LocationDef(new Point(0, 0), new Point(0, 2)), CoordinateItem.Starbase)
                }
            }, this.Game.Interact, this.Game.Config, this.Game);

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Coordinate(new LocationDef(new Point(0, 0), new Point(2, 7))));

            //var activeRegion = _testMap.Sectors.GetActive();
            //activeRegion.AddShip(starbase, starbase.Coordinate);
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
        //    var playershipRegion = _testMapNoObjects.Playership.GetSector();

        //    _startingRegion = new Point(playershipRegion.X, playershipRegion.Y);

        //    _startingSectorX = _testMapNoObjects.Playership.Coordinate.X;
        //    _startingSectorY = _testMapNoObjects.Playership.Coordinate.Y;

        //    //verify that the ship is where we think it is before we start
        //    Assert.AreEqual(CoordinateItem.Friendly, Coordinate.Get(_testMapNoObjects.Sectors.GetActive().Coordinates,
        //                                                   _testMapNoObjects.Playership.Coordinate.X,
        //                                                   _testMapNoObjects.Playership.Coordinate.Y).Item);

        //    _testMovement = new Movement(_testMapNoObjects.Playership, this.Game);
        //    _testMovement.BlockedByObstacle = false;

        //    var sectorItem =
        //        Coordinate.Get(_testMovement.Game.Map.Sectors.GetActive().Coordinates, _testMovement.Game.Map.Playership.Coordinate.X,
        //                                                               _testMovement.Game.Map.Playership.Coordinate.Y).Item;
        //    Assert.AreEqual(CoordinateItem.Friendly, sectorItem);

        //    _testMovement.Execute(Convert.ToInt32(direction), distance, distance / 8, out _lastRegionX, out _lastRegionY);

        //    //EnergySubtracted changes an entered value of .1 to .8
        //    //todo: measure time passed
        //}
    }
}


