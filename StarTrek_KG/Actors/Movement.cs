using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;

namespace StarTrek_KG.Actors
{
    public class Movement : System
    {
        //todo: to fully abstract this out, this could be a Blocked by property, set to whatever stops us from moving.
        public bool BlockedByObstacle { get; set; }
        public bool BlockedByGalacticBarrier { get; private set; }

        private readonly string NEBULA_ENCOUNTERED = "Nebula Encountered. Navigation stopped to manually recalibrate warp coil";

        public Movement(Ship shipConnectedTo, IGame game)
        {
            base.Game = game;
            base.ShipConnectedTo = shipConnectedTo;
        }

        public void Execute(MovementType movementType, NavDirection direction, int distance, out int lastRegionX, out int lastRegionY)
        {
            Region playershipRegion = this.ShipConnectedTo.GetRegion();

            lastRegionY = playershipRegion.Y;
            lastRegionX = playershipRegion.X;

            //var lastSector = new Coordinate(playerShipSector.X, playerShipSector.Y);

            Sector.GetFrom(this.ShipConnectedTo).Item = SectorItem.Empty;//Clear Old Sector

            switch (movementType)
            {
                case MovementType.Impulse:
                    this.TravelThroughSectors(distance, direction, this.ShipConnectedTo);
                    break;

                case MovementType.Warp:
                    Region newLocation = this.TravelThroughRegions(Convert.ToInt32(distance), Convert.ToInt32(direction),
                        this.ShipConnectedTo);

                    this.ShipConnectedTo.Coordinate = newLocation;

                    if (newLocation != null)
                    {
                        newLocation.SetActive();
                        this.Game.Map.SetPlayershipInActiveSector(this.Game.Map); //sets friendly in Active Region 
                        this.Game.MoveTimeForward(this.Game.Map, new Coordinate(lastRegionX, lastRegionY), newLocation);
                    }

                    break;
                    //todo:
                    //case MovementType.X:
                    //    newLocation = this.TravelThroughGalaxies()
                default:
                    this.Game.Interact.Line("Unsupported Movement Type");
                    break;
            }
        }

        #region Sectors

        private void TravelThroughSectors(int distance, NavDirection direction, IShip travellingShip)
        {
            // 4   5   6
            //   \ ↑ /
            //3 ← <*> → 7
            //   / ↓ \
            // 2   1   8

            //int navDirection = -(int)direction + 8;

            var currentRegion = travellingShip.GetRegion();

            int currentSectorX = travellingShip.Sector.X;
            int currentSectorY = travellingShip.Sector.Y;

            for (int sector = 0; sector < distance; sector++)
            {
                switch (direction)
                {
                    case NavDirection.Left:
                        this.GoLeft(ref currentSectorX);
                        break;

                    case NavDirection.LeftUp:
                        this.GoLeft(ref currentSectorX);
                        this.GoUp(ref currentSectorY);
                        break;

                    case NavDirection.Up:
                        this.GoUp(ref currentSectorY);
                        break;

                    case NavDirection.RightUp:
                        this.GoRight(ref currentSectorX);
                        this.GoUp(ref currentSectorY);
                        break;

                    case NavDirection.Right:
                        this.GoRight(ref currentSectorX);
                        break;

                    case NavDirection.RightDown:
                        this.GoRight(ref currentSectorX);
                        this.GoDown(ref currentSectorY);
                        break;

                    case NavDirection.Down:
                        this.GoDown(ref currentSectorY);
                        break;

                    case NavDirection.LeftDown:
                        this.GoLeft(ref currentSectorX);
                        this.GoDown(ref currentSectorY);
                        break;
                }

                //if on the edge of a Region, newSector will have negative numbers
                var newSectorCandidate = new Sector(new LocationDef(currentRegion.X, currentRegion.Y, currentSectorX, currentSectorY), false);

                var locationToScan = new Location(this.ShipConnectedTo.GetRegion(), newSectorCandidate);

                //run IRS on sector we are moving into
                IRSResult scanResult = ImmediateRangeScan.For(this.ShipConnectedTo).Scan(locationToScan);

                //If newSectorCandidate had negative numbers, then scanResult will have the newly updated region in it

                if (scanResult.GalacticBarrier)
                {
                    this.Game.Interact.Line("All Stop. Cannot cross Galactic Barrier.");
                    return;
                }
                else
                {
                    //throw new NotImplementedException(); //how we gonna check for obstacles if scanresult has bad numbers in it?

                    //AdjustSectorToNewRegion may need to be called here

                    bool obstacleEncountered = this.SublightObstacleCheck((Coordinate) travellingShip.Sector, newSectorCandidate, currentRegion.Sectors);
                    if (obstacleEncountered)
                    {
                        this.Game.Interact.Line("All Stop.");
                        return;
                    }

                    //bool nebulaEncountered = Sectors.IsNebula(ShipConnectedTo.Map, new Coordinate(Convert.ToInt32(currentSX), Convert.ToInt32(currentSY)));
                    //if (nebulaEncountered)
                    //{
                    //    this.Game.Write.Line("Nebula Encountered. Navigation stopped to manually recalibrate warp coil");
                    //    return;
                    //}
                }

                Location newLocation = locationToScan;
                newLocation.Region = this.Game.Map.Regions.Where(r => r.Name == scanResult.RegionName).Single();


                this.Game.Map.SetPlayershipInLocation(travellingShip, this.Game.Map, newLocation);

                //todo:  this.Game.MoveTimeForward(this.Game.Map, new Coordinate(lastRegionX, lastRegionY), newLocation);
            }
        }

        private void GoLeft(ref int x)
        {
            x--;
        }
        private void GoRight(ref int x)
        {
            x++;
        }
        private void GoUp(ref int y)
        {
            y--;
        }
        private void GoDown(ref int y)
        {
            y++;
        }

        #endregion

        #region Regions

        private Region TravelThroughRegions(int distance, int direction, IShip playership)
        {
            // 4   5   6
            //   \ ↑ /
            //3 ← <*> → 7
            //   / ↓ \
            // 2   1   8

            Region currentRegion = playership.GetRegion();
            Region newRegion = currentRegion;

            //todo: get rid of this double-stuff. I'm only doing this so that IsGalacticBarrier can be used by both Region and Sector Navigation.
            int futureShipRegionX = currentRegion.X;
            int futureShipRegionY = currentRegion.Y;

            for (int i = 0; i < distance; i++)
            {
                switch (direction)
                {
                    case 3:
                        futureShipRegionX--; //left
                        break;
                    case 4:
                        futureShipRegionX--; //left
                        futureShipRegionY--; //up
                        break;
                    case 5:
                        futureShipRegionY--; //up
                        break;
                    case 6:
                        futureShipRegionX++; //right
                        futureShipRegionY--; //up
                        break;
                    case 7:
                        futureShipRegionX++; //right
                        break;
                    case 8:
                        futureShipRegionX++; //right
                        futureShipRegionY++; //down
                        break;
                    case 1:
                        futureShipRegionY++; //down
                        break;
                    case 2:
                        futureShipRegionX--; //left
                        futureShipRegionY++; //down
                        break;
                }

                //todo: check if Region is nebula or out of bounds

                bool barrierHit = this.Game.Map.Regions.IsGalacticBarrier(futureShipRegionX, futureShipRegionY);  //XY will be set to safe value in here
                this.BlockedByGalacticBarrier = barrierHit;

                if (barrierHit)
                {
                    //ship location is not updated. this means the ship will stop right before the barrier
                    //todo: later, a config option could be that the ship can be thrown to an adjacent region.
                    break;
                }
                else
                {
                    //set ship location to the new location
                    newRegion = Regions.Get(this.ShipConnectedTo.Map, new Coordinate(futureShipRegionX, futureShipRegionY));

                    bool nebulaEncountered = Regions.IsNebula(this.ShipConnectedTo.Map, newRegion);
                    if (nebulaEncountered)
                    {
                        base.Game.Interact.Line(this.NEBULA_ENCOUNTERED);
                        break;
                    }
                }

            } //for loop end

            return newRegion;

            //todo: once we have found Region..
            //is target location blocked?
            //if true, then output that expected location was blocked, and ship's computers have picked a new spot
            
            //while loop
            //   pick a random sector
            //   check it for obstacle
            //if good then jump out of loop
        }

        #endregion

        //todo: for warp-to-Region
        public void Execute(string destinationRegionName, out int lastRegionX, out int lastRegionY)
        {
            Region playershipRegion = this.ShipConnectedTo.GetRegion();

            lastRegionY = playershipRegion.Y;
            lastRegionX = playershipRegion.X;

            Region destinationRegion = this.Game.Map.Regions.Get(destinationRegionName);

            //destinationRegion.Active = true;
            destinationRegion.SetActive();

            this.Game.Map.SetPlayershipInActiveSector(this.Game.Map); //sets friendly in Active Region  

            this.Game.MoveTimeForward(this.Game.Map, new Coordinate(lastRegionX, lastRegionY), destinationRegion);
        }

        /// <summary>
        /// TODO:  this function needs to go away.  If we strike an obstactle, we should know what it is. 
        /// This looks like a bandaid at best.
        /// Looks for an Obstacle at the sector passed. 
        /// Places Friendly on map at LastSector if Obstacle detected
        /// </summary>
        /// <param name="sector"> </param>
        /// <param name="activeSectors"> </param>
        /// <param name="lastSector"> </param>
        /// <returns></returns>
        private bool SublightObstacleCheck(ICoordinate lastSector, ICoordinate sector, Sectors activeSectors)
        {
            //todo:  I think I destroyed a star and appeared in its place when navigating to a new Region.  (That or LRS is broken, or maybe it is working fine!)
            try
            {
                var mySector = this.ShipConnectedTo.Sector;
                var currentItem = Sector.Get(activeSectors, sector.X, sector.Y).Item;
                var currentObject = Sector.Get(activeSectors, sector.X, sector.Y).Object;

                if (currentItem != SectorItem.Empty)
                {
                    mySector.X = lastSector.X;
                    mySector.Y = lastSector.Y;

                    //todo: move this to XXX label.  run tests.  should work.
                    Sector.Get(activeSectors, mySector.X, mySector.Y).Item = SectorItem.PlayerShip;

                    this.IdentifyObstacle(sector, currentObject, currentItem);

                    this.BlockedByObstacle = true;

                    return true;
                }
            }
            catch
            {
                //Console.Write("error while checking for obstacle.");
            }

            return false;
        }

        private void IdentifyObstacle(ICoordinate sector, ISectorObject currentObject, SectorItem currentItem)
        {
            switch (currentItem)
            {
                case SectorItem.Star:
                    var star = currentObject;
                    this.Game.Interact.Line("Stellar body " + star.Name.ToUpper() + " encountered while navigating at sector: [" + sector.X + "," +
                                      sector.Y + "]");
                    break;

                case SectorItem.HostileShip:
                    var hostile = currentObject;
                    this.Game.Interact.Line("Ship " + hostile.Name + " encountered while navigating at sector: [" + sector.X + "," +
                                      sector.Y + "]");
                    break;

                    
                case SectorItem.Starbase:
                    this.Game.Interact.Line("Starbase encountered while navigating at sector: [" + sector.X + "," + sector.Y + "]");
                    break;

                default:
                    this.Game.Interact.Line("Detected an unidentified obstacle while navigating at sector: [" + sector.X + "," + sector.Y + "]");
                    break;
            }
        }

        //private Region SetShipLocation(double vectorX, double vectorY)
        //{
        //    var shipSector = this.ShipConnectedTo.Sector;
        //    var shipRegion = this.ShipConnectedTo.Coordinate;

        //    shipSector.X = ((int)Math.Round(vectorX)) % 8; //sector info is in the vector
        //    shipSector.Y = ((int)Math.Round(vectorY)) % 8;

        //    var RegionX = ((int)Math.Round(vectorX)) / 8; //Region info is in the vector
        //    var RegionY = ((int)Math.Round(vectorY)) / 8;

        //    shipRegion.X = RegionX;
        //    shipRegion.Y = RegionY;

        //    Region newActiveRegion = this.ShipConnectedTo.GetRegion();

        //    if (newActiveRegion == null)
        //    {
        //        throw new GameException("No Region to make active");
        //    }

        //    newActiveRegion.SetActive();

        //    this.Game.Map.SetActiveSectorAsFriendly(this.Game.Map); //sets friendly in Active Region  

        //    return newActiveRegion; //contains the newly set sector in it
        //}

        //This prompt needs to be exposed to the user as an event
        public bool PromptAndCheckCourse(out NavDirection direction)
        {
            //var course = this.Game.Write.Course() + "Enter Course: ";
            string userDirection = "";
            List<int> availableDirections = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();

            bool userEnteredCourse = this.Game.Prompt.Invoke($"{this.Game.Interact.RenderCourse()} Enter Course: ", out userDirection);

            if (userEnteredCourse)
            {
                //todo: check to see if number is higher than 8

                if (!userDirection.IsNumeric() || userDirection.Contains("."))
                {
                    this.Game.Interact.Line("Invalid course.");
                    direction = NavDirection.Up;

                    return true;
                }

                int directionToCheck = Convert.ToInt32(userDirection);

                if (directionToCheck > availableDirections.Max() || directionToCheck < availableDirections.Min())
                {
                    this.Game.Interact.Line("Invalid course..");
                    direction = NavDirection.Up;

                    return true;
                }
            }

            direction = (NavDirection)availableDirections.Where(d => d == Convert.ToInt32(userDirection)).SingleOrDefault();

            return false;
        }

        ////This prompt needs to be exposed to the user as an event
        //public bool PromptAndCheckCourse(out int direction)
        //{
        //    var course = this.Game.Write.Course() + "Enter Course: ";
        //    string userDirection;

        //    if (this.Game.Write.PromptUser(course, out userDirection))
        //    {
        //        //todo: check to see if number is higher than 8

        //        if (!userDirection.IsNumeric() || userDirection.Contains("."))
        //        {
        //            this.Game.Write.Line("Invalid course.");
        //            direction = 0;

        //            return true;
        //        }

        //        var directionToCheck = Convert.ToInt32(userDirection);

        //        if (directionToCheck > 8 || directionToCheck < 0)
        //        {
        //            this.Game.Write.Line("Invalid course..");
        //            direction = 0;

        //            return true;
        //        }
        //    }

        //    direction = Convert.ToInt32(userDirection);

        //    return false;
        //}
    }
} 
