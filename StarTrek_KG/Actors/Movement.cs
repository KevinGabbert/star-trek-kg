using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;

namespace StarTrek_KG.Actors
{
    public class Movement : System
    {
        // 4   5   6
        //   \ ? /
        //3 ? <*> ? 7
        //   / ? \
        // 2   1   8

        //todo: to fully abstract this out, this could be a Blocked by property, set to whatever stops us from moving.
        public bool BlockedByObstacle { get; set; }
        public bool BlockedByGalacticBarrier { get; private set; }
        public IInteraction SystemPrompt { get; set; }

        private readonly string NEBULA_ENCOUNTERED = "Nebula Encountered. Navigation stopped to manually recalibrate warp coil"; //todo: resource this.
        private readonly string GALACTIC_BARRIER_ENCOUNTERED = "Navigation auto shutdown. Galactic Barrier encountered. Power drained by {0}.";

        public Movement(IShip shipConnectedTo)
        {
            base.ShipConnectedTo = shipConnectedTo;
            this.SystemPrompt = shipConnectedTo.Map.Game.Interact;
        }

        public void Execute(MovementType movementType, NavDirection direction, int distance, out int lastRegionX, out int lastRegionY)
        {
            Sector playershipRegion = this.ShipConnectedTo.GetSector();

            lastRegionY = playershipRegion.Y;
            lastRegionX = playershipRegion.X;

            Coordinate.GetFrom(this.ShipConnectedTo).Item = CoordinateItem.Empty; //Clear Old Coordinate

            IGame game = this.ShipConnectedTo.Map.Game;

            switch (movementType)
            {
                case MovementType.Impulse:
                    this.TravelThroughSectors(distance, direction, this.ShipConnectedTo);
                    break;

                case MovementType.Warp:
                    Sector newLocation = this.TravelThroughRegions(Convert.ToInt32(distance), direction, this.ShipConnectedTo);

                    this.ShipConnectedTo.Point = new Point(newLocation.X, newLocation.Y);

                    if (newLocation != null)
                    {
                        newLocation.SetActive();
                        game.Map.SetPlayershipInActiveSector(game.Map); //sets friendly in Active Sector 
                        game.MoveTimeForward(game.Map, playershipRegion, newLocation);
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

        //todo: for warp-to-Sector
        public void Execute(string destinationRegionName, out int lastRegionX, out int lastRegionY)
        {
            Sector playershipRegion = this.ShipConnectedTo.GetSector();

            lastRegionY = playershipRegion.Y;
            lastRegionX = playershipRegion.X;

            IMap map = this.ShipConnectedTo.Map;

            Sector destinationRegion = map.Sectors.Get(destinationRegionName);

            //destinationRegion.Active = true;
            destinationRegion.SetActive();

            map.SetPlayershipInActiveSector(map); //sets friendly in Active Sector  

            this.ShipConnectedTo.Map.Game.MoveTimeForward(map, new Point(lastRegionX, lastRegionY), destinationRegion);
        }

        #region Coordinates

        private void TravelThroughSectors(int distance, NavDirection impulseTravelDirection, IShip travellingShip)
        {
            Sector currentRegion = travellingShip.GetSector();

            for (int sector = 0; sector < distance; sector++)
            {
                // Move coordinate by one step in direction
                Point newCoordinate = this.GetNewCoordinate(impulseTravelDirection, new Point(travellingShip.Coordinate.X, travellingShip.Coordinate.Y));

                // --- NEW LOGIC: Check for sector edge crossing ---
                if (newCoordinate.X < 0 || newCoordinate.X >= DEFAULTS.COORDINATE_MAX ||
                    newCoordinate.Y < 0 || newCoordinate.Y >= DEFAULTS.COORDINATE_MAX)
                {
                    // Move to next region
                    Sector nextRegion = Sectors.GetNext(this.ShipConnectedTo.Map, currentRegion, impulseTravelDirection);

                    // If barrier hit, stop
                    if (this.ShipConnectedTo.Map.Sectors.IsGalacticBarrier(nextRegion))
                    {
                        this.ApplyGalacticBarrierPenalty();
                        break;
                    }

                    // Update region and wrap coordinates
                    currentRegion = nextRegion;

                    if (newCoordinate.X < 0) newCoordinate.X = DEFAULTS.COORDINATE_MAX - 1;
                    else if (newCoordinate.X >= DEFAULTS.COORDINATE_MAX) newCoordinate.X = 0;

                    if (newCoordinate.Y < 0) newCoordinate.Y = DEFAULTS.COORDINATE_MAX - 1;
                    else if (newCoordinate.Y >= DEFAULTS.COORDINATE_MAX) newCoordinate.Y = 0;
                }

                // --- Existing logic to build new location and check obstacles ---
                bool stopNavigation;
                Location newLocation = this.GetNewLocation(
                    impulseTravelDirection,
                    travellingShip,
                    currentRegion,
                    newCoordinate,
                    out stopNavigation
                );

                if (stopNavigation)
                {
                    break;
                }
                else
                {
                    this.ShipConnectedTo.Map.SetPlayershipInLocation(travellingShip, this.ShipConnectedTo.Map, newLocation);
                }
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
        private bool SublightObstacleCheck(IPoint lastSector, IPoint sector, Coordinates activeSectors)
        {
            //todo:  I think I destroyed a star and appeared in its place when navigating to a new Sector.  (That or LRS is broken, or maybe it is working fine!)
            try
            {
                ICoordinate mySector = this.ShipConnectedTo.Coordinate;
                CoordinateItem currentItem = activeSectors[sector.X, sector.Y].Item;
                ICoordinateObject currentObject = activeSectors[sector.X, sector.Y].Object;

                if (currentItem != CoordinateItem.Empty)
                {
                    mySector.X = lastSector.X;
                    mySector.Y = lastSector.Y;

                    //todo: move this to XXX label.  run tests.  should work.
                    activeSectors[mySector.X, mySector.Y].Item = CoordinateItem.PlayerShip;

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

        private void IdentifyObstacle(IPoint sector, ICoordinateObject currentObject, CoordinateItem currentItem)
        {
            //todo: this will go away when ".item" is removed
            if (currentObject == null)
            {
                throw new ArgumentException("SectorObject not set.");
            }

            switch (currentItem)
            {
                case CoordinateItem.Star:
                    ICoordinateObject star = currentObject;
                    this.SystemPrompt.Line($"Stellar body {star?.Name?.ToUpper()} encountered while navigating at sector: [{sector.X},{sector.Y}]");
                    break;

                case CoordinateItem.HostileShip:
                    ICoordinateObject hostile = currentObject;
                    this.SystemPrompt.Line($"Ship {hostile?.Name} encountered while navigating at sector: [{sector.X},{sector.Y}]");
                    break;


                case CoordinateItem.Starbase:
                    this.SystemPrompt.Line($"Starbase encountered while navigating at sector: [{sector.X},{sector.Y}]");
                    break;

                default:
                    this.SystemPrompt.Line($"Detected an unidentified obstacle while navigating at sector: [{sector.X},{sector.Y}]");
                    break;
            }
        }

        private void ApplyGalacticBarrierPenalty()
        {
            int damage = this.ShipConnectedTo?.Map?.Game?.Config?.GetSetting<int>("GalacticBarrierDamage") ?? 0;

            if (this.ShipConnectedTo != null)
            {
                this.ShipConnectedTo.OutputLine(string.Format(this.GALACTIC_BARRIER_ENCOUNTERED, damage));
            }

            if (this.ShipConnectedTo == null)
            {
                return;
            }

            this.ShipConnectedTo.Energy -= damage;

            if (this.ShipConnectedTo.Energy <= 0)
            {
                this.ShipConnectedTo.Energy = 0;
                this.ShipConnectedTo.Destroyed = true;
            }
        }

        #endregion

        #region Sectors

        private Sector TravelThroughRegions(int distance, NavDirection warpDirection, IShip playership)
        {
            Sector currentRegion = playership.GetSector();
            Sector newRegion = currentRegion;

            //todo: get rid of this double-stuff. I'm only doing this so that IsGalacticBarrier can be used by both Sector and Coordinate Navigation.
            Sectors regions = this.ShipConnectedTo.Map.Sectors;

            for (int i = 0; i < distance; i++)
            {
                Sector futureShipRegion = this.GetNewCoordinate(warpDirection, currentRegion).ToSector();
                currentRegion = futureShipRegion;

                //todo: refactor this with sector stuff

                //todo: check if Sector is nebula or out of bounds

                bool barrierHit = regions.IsGalacticBarrier(futureShipRegion);  //XY will be set to safe value in here
                this.BlockedByGalacticBarrier = barrierHit;

                if (barrierHit)
                {
                    this.ApplyGalacticBarrierPenalty();
                    //ship location is not updated. this means the ship will stop right before the barrier
                    //todo: later, a config option could be that the ship can be thrown to an adjacent region.
                    break;
                }
                else
                {
                    //set ship location to the new location
                    newRegion = this.ShipConnectedTo.Map.Sectors[futureShipRegion];

                    bool nebulaEncountered = Sectors.IsNebula(this.ShipConnectedTo.Map, newRegion);
                    if (nebulaEncountered)
                    {
                        this.ShipConnectedTo.OutputLine(this.NEBULA_ENCOUNTERED);
                        break;
                    }
                }

            } //for loop end

            return newRegion;

            //todo: once we have found Sector..
            //is target location blocked?
            //if true, then output that expected location was blocked, and ship's computers have picked a new spot
            
            //while loop
            //   pick a random sector
            //   check it for obstacle
            //if good then jump out of loop
        }

        #endregion

        private Point GetNewCoordinate(NavDirection travelDirection, Point currentCoordinate)
        {
            int currentCoordinateX = currentCoordinate.X;
            int currentCoordinateY = currentCoordinate.Y;

            switch (travelDirection)
            {
                case NavDirection.Left:

                    this.GoLeft(ref currentCoordinateY);
                    break;

                case NavDirection.LeftUp:
                    this.GoLeft(ref currentCoordinateY);
                    this.GoUp(ref currentCoordinateX);
                    break;

                case NavDirection.Up:
                    this.GoUp(ref currentCoordinateX);
                    break;

                case NavDirection.RightUp:
                    this.GoRight(ref currentCoordinateY);
                    this.GoUp(ref currentCoordinateX);
                    break;

                case NavDirection.Right:
                    this.GoRight(ref currentCoordinateY);
                    break;

                case NavDirection.RightDown:
                    this.GoRight(ref currentCoordinateY);
                    this.GoDown(ref currentCoordinateX);
                    break;

                case NavDirection.Down:
                    this.GoDown(ref currentCoordinateX);
                    break;

                case NavDirection.LeftDown:
                    this.GoLeft(ref currentCoordinateY);
                    this.GoDown(ref currentCoordinateX);
                    break;

                default:
                    throw new ArgumentException("NavDirection not supported.");
            }

            return new Point(currentCoordinateX, currentCoordinateY);
        }

        private Location GetNewLocation(NavDirection impulseTravelDirection, ICoordinateObject travellingShip, Sector currentRegion, Point newCoordinate, out bool stopNavigation)
        {
            //if on the edge of a Sector, newSector will have negative numbers
            var newSectorCandidate = new Coordinate(new LocationDef(currentRegion.X, currentRegion.Y, newCoordinate.X, newCoordinate.Y));

            Sector newRegionCandidate = null;

            if (newSectorCandidate.Invalid())
            {
                newRegionCandidate = Sectors.GetNext(this.ShipConnectedTo.Map, currentRegion, impulseTravelDirection);
                newSectorCandidate.IncrementForNewRegion();
            }
            else
            {
                newRegionCandidate = this.ShipConnectedTo.GetSector();
            }

            var locationToScan = new Location(newRegionCandidate, newSectorCandidate);

            //run IRS on sector we are moving into
            IRSResult scanResult = ImmediateRangeScan.For(this.ShipConnectedTo).Scan(locationToScan);

            //If newSectorCandidate had negative numbers, then scanResult will have the newly updated region in it

            if (scanResult.GalacticBarrier)
            {
                this.ApplyGalacticBarrierPenalty();
                stopNavigation = true;
            }
            else
            {
                //todo: how we gonna check for obstacles if scanresult has bad numbers in it?
                bool obstacleEncountered = this.SublightObstacleCheck(new Point(travellingShip.Coordinate.X, travellingShip.Coordinate.Y), new Point(newSectorCandidate.X, newSectorCandidate.Y), currentRegion.Coordinates);

                if (obstacleEncountered)
                {
                    this.ShipConnectedTo.OutputLine("All Stop.");
                    stopNavigation = true;
                }
                else
                {
                    stopNavigation = false;
                }

                //bool nebulaEncountered = Coordinates.IsNebula(ShipConnectedTo.Map, new Point(Convert.ToInt32(currentSX), Convert.ToInt32(currentSY)));
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
                newLocation.Sector = this.ShipConnectedTo.Map.Sectors.Where(r => r.Name == scanResult.RegionName).Single();
            }

            return newLocation;
        }

        //private Sector SetShipLocation(double vectorX, double vectorY)
        //{
        //    var shipSector = this.ShipConnectedTo.Coordinate;
        //    var shipRegion = this.ShipConnectedTo.Point;

        //    shipSector.X = ((int)Math.Round(vectorX)) % 8; //sector info is in the vector
        //    shipSector.Y = ((int)Math.Round(vectorY)) % 8;

        //    var RegionX = ((int)Math.Round(vectorX)) / 8; //Sector info is in the vector
        //    var RegionY = ((int)Math.Round(vectorY)) / 8;

        //    shipRegion.X = RegionX;
        //    shipRegion.Y = RegionY;

        //    Sector newActiveRegion = this.ShipConnectedTo.GetSector();

        //    if (newActiveRegion == null)
        //    {
        //        throw new GameException("No Sector to make active");
        //    }

        //    newActiveRegion.SetActive();

        //    this.Game.Map.SetActiveSectorAsFriendly(this.Game.Map); //sets friendly in Active Sector  

        //    return newActiveRegion; //contains the newly set sector in it
        //}

        //This prompt needs to be exposed to the user as an event
        public bool PromptAndCheckCourse(out NavDirection direction)
        {
            //var course = this.Game.Write.Course() + "Enter Course: ";
            string userDirection = "";
            List<int> availableDirections = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();

            bool userEnteredCourse = this.ShipConnectedTo.Map.Game.Prompt.Invoke($"{this.SystemPrompt.RenderCourse()}{Environment.NewLine}Enter Course: ", out userDirection);

            if (!userEnteredCourse || string.IsNullOrWhiteSpace(userDirection))
            {
                direction = NavDirection.Up;
                return true;
            }

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
                this.SystemPrompt.Line("Invalid course.");
                direction = NavDirection.Up;

                return true;
            }

            direction = (NavDirection)availableDirections.Where(d => d == directionToCheck).SingleOrDefault();

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
