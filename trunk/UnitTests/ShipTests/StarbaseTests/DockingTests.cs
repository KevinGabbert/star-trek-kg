using System;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.StarbaseTests
{
    public class DockingTests: TestClass_Base
    {
        Map _testMapNoObjects;
        Movement _testMovement;
        Coordinate _startingQuadrant;

        int _startingSectorX;
        int _startingSectorY;

        private int _lastQuadX;
        private int _lastQuadY;

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

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
        }


        private void SetupMapWithStarbase()
        {
            _testMapNoObjects = (new Map(new SetupOptions
            {
                Initialize = true,

                SectorDefs = new SectorDefs
                                    {
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 0)), SectorItem.Friendly), //todo: this needs to be in a random spo
                                        new SectorDef(new LocationDef(new Coordinate(0, 0), new Coordinate(0, 2)), SectorItem.Starbase)
                                    }
            }, this.Game.Write, this.Game.Config));

            //Todo: this is how we would like to add a starbase
            ////add a ship
            //var starbase = new Starbase("starbaseAlpha", _testMap, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));

            //var activeQuad = _testMap.Quadrants.GetActive();
            //activeQuad.AddShip(starbase, starbase.Sector);
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

        private void Move_Sector(string direction, double distance)
        {
            var playershipQuad = _testMapNoObjects.Playership.GetQuadrant();

            _startingQuadrant = new Coordinate(playershipQuad.X, playershipQuad.Y);

            _startingSectorX = _testMapNoObjects.Playership.Sector.X;
            _startingSectorY = _testMapNoObjects.Playership.Sector.Y;

            //verify that the ship is where we think it is before we start
            Assert.AreEqual(SectorItem.Friendly, Sector.Get(_testMapNoObjects.Quadrants.GetActive().Sectors,
                                                           _testMapNoObjects.Playership.Sector.X,
                                                           _testMapNoObjects.Playership.Sector.Y).Item);

            _testMovement = new Movement(_testMapNoObjects.Playership, this.Game);
            _testMovement.BlockedByObstacle = false;

            var sectorItem =
                Sector.Get(_testMovement.Game.Map.Quadrants.GetActive().Sectors, _testMovement.Game.Map.Playership.Sector.X,
                                                                       _testMovement.Game.Map.Playership.Sector.Y).Item;
            Assert.AreEqual(SectorItem.Friendly, sectorItem);

            _testMovement.Execute(Convert.ToInt32(direction), distance, distance / 8, out _lastQuadX, out _lastQuadY);

            //EnergySubtracted changes an entered value of .1 to .8
            //todo: measure time passed
        }
    }
}
