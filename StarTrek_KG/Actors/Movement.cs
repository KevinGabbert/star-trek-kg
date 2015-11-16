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
        // 4   5   6
        //   \ ↑ /
        //3 ← <*> → 7
        //   / ↓ \
        // 2   1   8

        //todo: to fully abstract this out, this could be a Blocked by property, set to whatever stops us from moving.
        public bool BlockedByObstacle { get; set; }
        public bool BlockedByGalacticBarrier { get; private set; }
        public IInteraction SystemPrompt { get; set; }

        private readonly string NEBULA_ENCOUNTERED = "Nebula Encountered. Navigation stopped to manually recalibrate warp coil"; //todo: resource this.

        public Movement(IShip shipConnectedTo)
        {
            base.ShipConnectedTo = shipConnectedTo;
            this.SystemPrompt = shipConnectedTo.Map.Game.Interact;
        }

        public void Execute(MovementType movementType, NavDirection direction, int distance, out int lastRegionX, out int lastRegionY)
        {
            Region playershipRegion = this.ShipConnectedTo.GetRegion();

            lastRegionY = playershipRegion.Y;
            lastRegionX = playershipRegion.X;

            //var lastSector = new Coordinate(playerShipSector.X, playerShipSector.Y);

            Sector.GetFrom(this.ShipConnectedTo).Item = SectorItem.Empty;//Clear Old Sector

            IGame game = this.ShipConnectedTo.Map.Game;

            switch (movementType)
            {
                case MovementType.Impulse:
                    this.TravelThroughSectors(distance, direction, this.ShipConnectedTo);
                    break;

                case MovementType.Warp:
                    Region newLocation = this.TravelThroughRegions(Convert.ToInt32(distance), direction, this.ShipConnectedTo);

                    this.ShipConnectedTo.Coordinate = newLocation;

                    if (newLocation != null)
                    {
                        newLocation.SetActive();
                        game.Map.SetPlayershipInActiveSector(game.Map); //sets friendly in Active Region 
                        game.MoveTimeForward(game.Map, new Coordinate(lastRegionX, lastRegionY), newLocation);
                    }

                    break;
                    //todo:

                //case MovementType.TransWarp:
                //    newLocation = this.TravelThroughGalaxies()

                default:
                    game.Interact.Line("Unsupported Movement Type");
                    break;
            }
        }

        //todo: for warp-to-Region
        public void Execute(string destinationRegionName, out int lastRegionX, out int lastRegionY)
        {
            Region playershipRegion = this.ShipConnectedTo.GetRegion();

            lastRegionY = playershipRegion.Y;
            lastRegionX = playershipRegion.X;

            IMap map = this.ShipConnectedTo.Map;

            Region destinationRegion = map.Regions.Get(destinationRegionName);

            //destinationRegion.Active = true;
            destinationRegion.SetActive();

            map.SetPlayershipInActiveSector(map); //sets friendly in Active Region  

            this.ShipConnectedTo.Map.Game.MoveTimeForward(map, new Coordinate(lastRegionX, lastRegionY), destinationRegion);
        }

        #region Sectors

        private void TravelThroughSectors(int distance, NavDirection impulseTravelDirection, IShip travellingShip)
        {
            var currentRegion = travellingShip.GetRegion();

            int currentSectorX = travellingShip.Sector.X;
            int currentSectorY = travellingShip.Sector.Y;

            for (int sector = 0; sector < distance; sector++)
            {
                this.GetNewCoordinate(impulseTravelDirection, ref currentSectorX, ref currentSectorY);

                bool stopNavigation;
                Location newLocation = this.GetNewLocation(impulseTravelDirection, travellingShip, currentRegion, new Coordinate(currentSectorX, currentSectorY), out stopNavigation);

                if (stopNavigation)
                {
                    break;
                }
                else
                {
                    this.ShipConnectedTo.Map.SetPlayershipInLocation(travellingShip, this.ShipConnectedTo.Map, newLocation);
                }

                //todo:  this.Game.MoveTimeForward(this.Game.Map, new Coordinate(lastRegionX, lastRegionY), newLocation);
            }
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
                    this.SystemPrompt.Line($"Stellar body {star.Name.ToUpper()} encountered while navigating at sector: [{sector.X},{sector.Y}]");
                    break;

                case SectorItem.HostileShip:
                    var hostile = currentObject;
                    this.SystemPrompt.Line($"Ship {hostile.Name} encountered while navigating at sector: [{sector.X},{sector.Y}]");
                    break;


                case SectorItem.Starbase:
                    this.SystemPrompt.Line($"Starbase encountered while navigating at sector: [{sector.X},{sector.Y}]");
                    break;

                default:
                    this.SystemPrompt.Line($"Detected an unidentified obstacle while navigating at sector: [{sector.X},{sector.Y}]");
                    break;
            }
        }

        #endregion

        #region Regions

        private Region TravelThroughRegions(int distance, NavDirection warpDirection, IShip playership)
        {
            Region currentRegion = playership.GetRegion();
            Region newRegion = currentRegion;

            //todo: get rid of this double-stuff. I'm only doing this so that IsGalacticBarrier can be used by both Region and Sector Navigation.
            int futureShipRegionX = currentRegion.X;
            int futureShipRegionY = currentRegion.Y;

            Regions regions = this.ShipConnectedTo.Map.Regions;

            for (int i = 0; i < distance; i++)
            {
                this.GetNewCoordinate(warpDirection, ref futureShipRegionX, ref futureShipRegionY);

                //todo: refactor this with sector stuff

                //todo: check if Region is nebula or out of bounds

                bool barrierHit = regions.IsGalacticBarrier(futureShipRegionX, futureShipRegionY);  //XY will be set to safe value in here
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
                        this.ShipConnectedTo.Map.Game.Interact.Line(this.NEBULA_ENCOUNTERED);
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

        private void GetNewCoordinate(NavDirection travelDirection, ref int currentCoordinateX, ref int currentCoordinateY)
        {
            switch (travelDirection)
            {
                case NavDirection.Left:
                    this.GoLeft(ref currentCoordinateX);
                    break;

                case NavDirection.LeftUp:
                    this.GoLeft(ref currentCoordinateX);
                    this.GoUp(ref currentCoordinateY);
                    break;

                case NavDirection.Up:
                    this.GoUp(ref currentCoordinateY);
                    break;

                case NavDirection.RightUp:
                    this.GoRight(ref currentCoordinateX);
                    this.GoUp(ref currentCoordinateY);
                    break;

                case NavDirection.Right:
                    this.GoRight(ref currentCoordinateX);
                    break;

                case NavDirection.RightDown:
                    this.GoRight(ref currentCoordinateX);
                    this.GoDown(ref currentCoordinateY);
                    break;

                case NavDirection.Down:
                    this.GoDown(ref currentCoordinateY);
                    break;

                case NavDirection.LeftDown:
                    this.GoLeft(ref currentCoordinateX);
                    this.GoDown(ref currentCoordinateY);
                    break;

                default:
                    throw new ArgumentException("NavDirection not supported.");
            }
        }

        private Location GetNewLocation(NavDirection impulseTravelDirection, ISectorObject travellingShip, Region currentRegion, ICoordinate newCoordinate, out bool stopNavigation)
        {
            //if on the edge of a Region, newSector will have negative numbers
            var newSectorCandidate = new Sector(new LocationDef(currentRegion.X, currentRegion.Y, newCoordinate.X, newCoordinate.Y));

            Region newRegionCandidate = null;

            if (newSectorCandidate.Invalid())
            {
                newRegionCandidate = Regions.GetNext(this.ShipConnectedTo.Map, currentRegion, impulseTravelDirection);
                newSectorCandidate.IncrementForNewRegion();
            }
            else
            {
                newRegionCandidate = this.ShipConnectedTo.GetRegion();
            }

            var locationToScan = new Location(newRegionCandidate, newSectorCandidate);

            //run IRS on sector we are moving into
            IRSResult scanResult = ImmediateRangeScan.For(this.ShipConnectedTo).Scan(locationToScan);

            //If newSectorCandidate had negative numbers, then scanResult will have the newly updated region in it

            if (scanResult.GalacticBarrier)
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("All Stop. Cannot cross Galactic Barrier.");
                stopNavigation = true;
            }
            else
            {
                //todo: how we gonna check for obstacles if scanresult has bad numbers in it?
                bool obstacleEncountered = this.SublightObstacleCheck((Coordinate)travellingShip.Sector, newSectorCandidate, currentRegion.Sectors);

                if (obstacleEncountered)
                {
                    this.ShipConnectedTo.Map.Game.Interact.Line("All Stop.");
                    stopNavigation = true;
                }

                stopNavigation = false;

                //bool nebulaEncountered = Sectors.IsNebula(ShipConnectedTo.Map, new Coordinate(Convert.ToInt32(currentSX), Convert.ToInt32(currentSY)));
                //if (nebulaEncountered)
                //{
                //    this.Game.Write.Line("Nebula Encountered. Navigation stopped to manually recalibrate warp coil");
                //    return;
                //}
            }

            Location newLocation = locationToScan;

            if (stopNavigation)
            {
                newLocation = null;
            }
            else
            {
                newLocation.Region = this.ShipConnectedTo.Map.Regions.Where(r => r.Name == scanResult.RegionName).Single();
            }

            return newLocation;
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

            bool userEnteredCourse = this.ShipConnectedTo.Map.Game.Prompt.Invoke($"{this.SystemPrompt.RenderCourse()} Enter Course: ", out userDirection);

            if (userEnteredCourse)
            {
                //todo: check to see if number is higher than 8

                if (!userDirection.IsNumeric() || userDirection.Contains("."))
                {
                    this.SystemPrompt.Line("Invalid course.");
                    direction = NavDirection.Up;

                    return true;
                }

                int directionToCheck = Convert.ToInt32(userDirection);

                if (directionToCheck > availableDirections.Max() || directionToCheck < availableDirections.Min())
                {
                    this.SystemPrompt.Line("Invalid course..");
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
    }
} 
