using System;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

namespace UnitTests.Tests.MovementTests
{
    public class Movement_Base: TestClass_Base
    {
        //todo: write error messages for no sector set up and no Region set up.
        #region Setup

        #region Setup variables
        protected Movement _testMovement;
        protected Ship _testShip;

        protected Coordinate _startingRegion;

        protected int _startingSectorX;
        protected int _startingSectorY;

        protected int _lastRegionX;
        protected int _lastRegionY;

        #endregion

        [SetUp]
        public void Setup()
        {
            TestRunner.GetTestConstants();

            reset();
            this.CheckBeforeMovement();
        }

        [TearDown]
        public void TearDown()
        {
            reset();
            this.ClearAllSectors();

            this.Game.Map.Playership = null;
            this.Game.Map = null;
            _testMovement = null;

            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;

            DEFAULTS.REGION_MIN = 0;
            DEFAULTS.REGION_MAX = 0;
        }


        //todo: test feature.. Moving the ship requires an expenditure of energy
        //this feature can be turned off by config setting

        protected void reset()
        {
            this.Game.Map = new Map(new SetupOptions
            {

                Initialize = true,
                AddNebulae = false,
                SectorDefs = new SectorDefs
                {
                    new SectorDef(
                        new LocationDef(new Coordinate(4, 4),
                            new Coordinate(4, 4)),
                        SectorItem.PlayerShip),
                    //todo: this needs to be in a random spot
                },
                AddStars = false
            }, this.Game.Interact, this.Game.Config, this.Game);

            _testMovement = new Movement(this.Game.Map.Playership) { BlockedByObstacle = false };
            _testShip = this.Game.Map.Playership; //synctactic sugar

            #region "Manually set ship. todo: write test to ensure that this method works too"
            ////Moves ship to new place in map - updates map
            //Sector.SetFriendly(this.Game.Map.Playership.Sector.X,
            //                    this.Game.Map.Playership.Sector.Y,
            //                    this.Game.Map);

            ////sets ship
            //this.Game.Map.Playership.Coordinate.X = 4;
            //this.Game.Map.Playership.Coordinate.Y = 4;


            //this.Game.Map.Playership.Sector.X = 4;
            //this.Game.Map.Playership.Sector.Y = 4;

            #endregion

            _startingRegion = new Coordinate(this.Game.Map.Playership.Coordinate.X, this.Game.Map.Playership.Coordinate.Y); //random

            _startingSectorX = this.Game.Map.Playership.Sector.X; //4;
            _startingSectorY = this.Game.Map.Playership.Sector.Y; //4;

            _lastRegionX = 0;
            _lastRegionY = 0;
        }

        #endregion

        //todo: this needs to be refactored into a ship setup testfixture or something.
        private void CheckBeforeMovement()
        {
            var activeRegion = this.Game.Map.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count);

            Assert.IsInstanceOf<Region>(activeRegion);

            Assert.AreEqual(_startingRegion.X, activeRegion.X);
            Assert.AreEqual(_startingRegion.Y, activeRegion.Y);

            //Check to see if Playership has been assigned to a sector in the active Region.

            //indirectly..
            Assert.AreEqual(1, activeRegion.Sectors.Count(s => s.Item == SectorItem.PlayerShip));

            //directly.
            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors.Single(s => s.X == this.Game.Map.Playership.Sector.X && s.Y == this.Game.Map.Playership.Sector.Y).Item);

            var x = (from Sector s in activeRegion.Sectors
                where s.Item == SectorItem.PlayerShip
                select s).Count();

            Assert.AreEqual(1, x);

            Assert.AreEqual(_startingRegion.X, this.Game.Map.Playership.Coordinate.X, "startingRegionX");
            Assert.AreEqual(_startingRegion.Y, this.Game.Map.Playership.Coordinate.Y, "startingRegionY");

            Assert.AreEqual(4, this.Game.Map.Playership.Sector.X, "startingShipSectorX");
            Assert.AreEqual(4, this.Game.Map.Playership.Sector.Y, "startingShipSectorY");
        }

        protected void CheckSectorsAfterMovement()
        {
            var activeRegion = this.Game.Map.Regions.GetActive();

            Assert.AreEqual(64, activeRegion.Sectors.Count); //I'd certainly hope that this hasnt changed..
            Assert.IsFalse(_testMovement.BlockedByObstacle, "Blocked by Obstacle");

            //Ensure starting location is empty

            //indirectly
            Assert.AreNotEqual("44", this.Game.Map.Playership.Sector.X + this.Game.Map.Playership.Sector.Y.ToString(), "startingShipSectorX");

            //directly

            //Check location on map

            //todo: this needs to be be flipped back. flip the increment variable in the test instead
            //originating sector is empty
            Assert.AreEqual(SectorItem.Empty, activeRegion.Sectors[_startingSectorX, _startingSectorY].Item);

            //indirectly..
            var found = this.Game.Map.Regions.GetActive().Sectors.Where(s => s.Item == SectorItem.PlayerShip).Count();
            Assert.AreEqual(1, found, "expected to find 1 friendly, not " + found + ".   ");

            //Look up sector by playership's coordinates. see if a friendly is there.
            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors.Single(s => s.X == this.Game.Map.Playership.Sector.X &&
                                                                                s.Y == this.Game.Map.Playership.Sector.Y).Item);
            //same thing.  uses sector.Get functionality to check.
            Assert.AreEqual(SectorItem.PlayerShip, activeRegion.Sectors[this.Game.Map.Playership.Sector.X, this.Game.Map.Playership.Sector.Y].Item);

            Assert.AreEqual(_startingRegion.X, _lastRegionX, "(c)startingRegionX");
            Assert.AreEqual(_startingRegion.Y, _lastRegionY, "(c)startingRegionY");
        }

        protected void ClearAllSectors()
        {
            var activeRegion = this.Game.Map.Regions.GetActive();

            //Clear everything. //Remember, when a map is set up, a ship is generated in a random location, and
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    activeRegion.Sectors[i, j].Item = SectorItem.Empty;
                }
            }
        }

  
        protected void Move_Sector(NavDirection direction, int distance)
        {
            //todo: Modify this to move to next region

            var playershipRegion = this.Game.Map.Playership.GetRegion();

            _startingRegion = new Coordinate(playershipRegion.X, playershipRegion.Y);

            _startingSectorX = this.Game.Map.Playership.Sector.X;
            _startingSectorY = this.Game.Map.Playership.Sector.Y;

            //verify that the ship is where we think it is before we start
            Assert.AreEqual(SectorItem.PlayerShip, this.Game.Map.Regions.GetActive().Sectors[this.Game.Map.Playership.Sector.X, this.Game.Map.Playership.Sector.Y].Item);
            var sectorItem =
                _testMovement.ShipConnectedTo.Map.Regions.GetActive().Sectors[_testMovement.ShipConnectedTo.Map.Playership.Sector.X,
                    _testMovement.ShipConnectedTo.Map.Playership.Sector.Y].Item;
            Assert.AreEqual(SectorItem.PlayerShip, sectorItem);

            _testMovement.Execute(MovementType.Impulse, direction, distance, out _lastRegionX, out _lastRegionY);

            //EnergySubtracted changes an entered value of .1 to .8
            //todo: measure time passed
        }

        protected void Move_Region(string direction, int distance)
        {
            _testMovement.Execute(MovementType.Warp, (NavDirection)Convert.ToInt32(direction), distance, out _lastRegionX, out _lastRegionY);
        }
    }
}
