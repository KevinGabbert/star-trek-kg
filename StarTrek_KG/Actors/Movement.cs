using System;
using System.Runtime.CompilerServices;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Actors
{
    public class Movement : System
    {
        //todo: to fully abstract this out, this could be a Blocked by property, set to whatever stops us from moving.
        public bool BlockedByObstacle { get; set; }
        public bool BlockedByGalacticBarrier { get; set; }

        public Movement(Ship shipConnectedTo, Game game)
        {
            this.Game = game;
            this.ShipConnectedTo = shipConnectedTo;
        }

        public void Execute(MovementType movementType, int direction, int distance, int distanceEntered, out int lastQuadX, out int lastQuadY)
        {
            //ISector playerShipSector = this.ShipConnectedTo.Sector;
            Quadrant playershipQuadrant = this.ShipConnectedTo.GetQuadrant();

            lastQuadY = playershipQuadrant.Y;
            lastQuadX = playershipQuadrant.X;

            //var lastSector = new Coordinate(playerShipSector.X, playerShipSector.Y);

            Sector.GetFrom(this.ShipConnectedTo).Item = SectorItem.Empty;//Clear Old Sector
            Quadrant newLocation = null;

            switch (movementType)
            {
                case MovementType.Impulse:
                    var newSector = this.TravelThroughSectors(distance, direction, this.ShipConnectedTo);
                    newLocation = this.ShipConnectedTo.GetQuadrant();
                    this.ShipConnectedTo.Sector = newSector;
                    break;

                case MovementType.Warp:
                    newLocation = this.TravelThroughQuadrants(Convert.ToInt32(distance), Convert.ToInt32(direction),
                        this.ShipConnectedTo);
                    this.ShipConnectedTo.Coordinate = newLocation;   
                    break;
                default:
                    this.Game.Write.Line("Unsupported Movement Type");
                    break;
            }

            if (newLocation != null)
            {
                newLocation.SetActive();
                this.Game.Map.SetActiveSectorAsFriendly(this.Game.Map); //sets friendly in Active Quadrant 

                this.Game.MoveTimeForward(this.Game.Map, new Coordinate(lastQuadX, lastQuadY), newLocation);
            }
        }

        private ISector TravelThroughSectors(int distance, int direction, IShip playership)
        {
            // 4   5   6
            //   \ ↑ /
            //3 ← <*> → 7
            //   / ↓ \
            // 2   1   8

            direction = -direction + 8;

            ISector returnVal = playership.Sector;
            var currentQuadrant = playership.GetQuadrant();

            int currentSX = playership.Sector.X;
            int currentSY = playership.Sector.Y;

            for (int i = 0; i < distance; i++)
            {
                switch (Convert.ToInt32(direction))
                {
                    case 3:
                        currentSX--; //left
                        break;
                    case 4:
                        currentSX--; //left
                        currentSY--; //up
                        break;
                    case 5:
                        currentSY--; //up
                        break;
                    case 6:
                        currentSX++; //right
                        currentSY--; //up
                        break;
                    case 7:
                        currentSX++; //right
                        break;
                    case 8:
                        currentSX++; //right
                        currentSY++; //down
                        break;
                    case 1:
                        currentSY++; //down
                        break;
                    case 2:
                        currentSX--; //left
                        currentSY++; //down
                        break;
                }

                //TODO: check to see if we have left quadrant - //DO BOUNDS CHECKING

                //**write a function in sector called .GetNeighbors() (returns 9 sectors)
                //**write a function in sector called .GetNeighbor(SectorNeighborDirection.East) (in this case, if the neighbor is outside the quadrant, then it needs to call Quadrant.GetNeighbor(direction), then ask it for the corresponding sectors.
                //**write a function in sector called .GetQuadrant()

                //**write a function in Quadrant called .GetNeighbors() (for LRS - or use what LRS uses)
                //**write a function in Quadrant called .GetNeighbor(QuadrantNeighborDirection.North)
                    //** now of course this neighbor could be a galactic barrier, so .GetNeighbor could then just return a new quadrant, of QuadrantType.Barrier

                //  put currentSX & currentSY together and see if they blow the upper or lower BOUNDS of the sector edges
                //      // Is X < 0 or > 8
                        // is Y < 0 or > 8
                //  if they do, then we need to figure out what quadrant is past those limits and get it
                //  if there is *no* quadrant past those limits, then we need to say that we hit the galactic barrier.

                //We will have to check for the galactic barrier immediately before pulling a new quadrant..
                //var newQuadrant = new Quadrant(currentQuadrant);

                //If we get into a new quadrant, then we need to figure out where we are. (all obstacle checking still applies of course)
                //   1. Reset the variable or variables that blew its bounds  -- either to 0 or 7
                //   2. Both newQuadrant and newSector will be set here.
                // Q. what if there is a star in the other sector? A. then we will stop. as below.  newSector will not be assigned.

                var newSector = new Sector(new LocationDef(currentQuadrant.X, currentQuadrant.Y, Convert.ToInt32(currentSX), Convert.ToInt32(currentSY)));

                var barrierHit = this.IsGalacticBarrier(ref currentSX, ref currentSY);  //XY will be set to safe value in here
                if (barrierHit)
                {
                    break;
                }
                else
                {
                    bool obstacleEncountered = this.SublightObstacleCheck((Coordinate)playership.Sector, newSector, currentQuadrant.Sectors);
                    if (obstacleEncountered)
                    {
                        this.Game.Write.Line("All Stop.");
                        break;
                    }

                    //bool nebulaEncountered = Sectors.IsNebula(ShipConnectedTo.Map, new Coordinate(Convert.ToInt32(currentSX), Convert.ToInt32(currentSY)));
                    //if (nebulaEncountered)
                    //{
                    //    this.Game.Write.Line("Nebula Encountered. Navigation stopped to manually recalibrate warp coil");
                    //    break;
                    //}
                }
                returnVal = newSector;
            }

            return returnVal;
        }

        private Quadrant TravelThroughQuadrants(int distance, int direction, IShip playership)
        {
            // 4   5   6
            //   \ ↑ /
            //3 ← <*> → 7
            //   / ↓ \
            // 2   1   8

            //todo: get rid of this double-stuff. I'm only doing this so that IsGalacticBarrier can be used by both quadrant and Sector Navigation.
            int currentQX = playership.GetQuadrant().X;
            int currentQY = playership.GetQuadrant().Y;

            for (int i = 0; i < distance; i++)
            {
                switch (direction)
                {
                    case 3: 
                        currentQX--; //left
                        break;
                    case 4:
                        currentQX--; //left
                        currentQY--; //up
                        break;
                    case 5:
                        currentQY--; //up
                        break;
                    case 6:
                        currentQX++; //right
                        currentQY--; //up
                        break;
                    case 7:
                        currentQX++; //right
                        break;
                    case 8:
                        currentQX++; //right
                        currentQY++; //down
                        break;
                    case 1:
                        currentQY++; //down
                        break;
                    case 2:
                        currentQX--; //left
                        currentQY++; //down
                        break;
                }

                //todo: check if Quadrant is nebula or out of bounds

                var barrierHit = this.IsGalacticBarrier(ref currentQX, ref currentQY);  //XY will be set to safe value in here
                if (barrierHit)
                {
                    break;
                }
                else
                {
                    bool nebulaEncountered = Quadrants.IsNebula(ShipConnectedTo.Map, new Coordinate(Convert.ToInt32(currentQX), Convert.ToInt32(currentQY)));
                    if (nebulaEncountered)
                    {
                        this.Game.Write.Line("Nebula Encountered. Navigation stopped to manually recalibrate warp coil");
                        break;
                    }
                }
            }

            return Quadrants.Get(ShipConnectedTo.Map, new Coordinate(Convert.ToInt32(currentQX), Convert.ToInt32(currentQY)));

            //todo: once we have found quadrant..
            //is target location blocked?
            //if true, then output that expected location was blocked, and ship's computers have picked a new spot
            
            //while loop
            //   pick a random sector
            //   check it for obstacle
            //if good then jump out of loop
        }

        //todo: for warp-to-quadrant
        public void Execute(string destinationQuadrantName, out int lastQuadX, out int lastQuadY)
        {
            Quadrant playershipQuadrant = this.ShipConnectedTo.GetQuadrant();

            lastQuadY = playershipQuadrant.Y;
            lastQuadX = playershipQuadrant.X;

            Quadrant destinationQuadrant = Quadrants.GetByName(this.Game.Map.Quadrants, destinationQuadrantName);

            //destinationQuadrant.Active = true;
            destinationQuadrant.SetActive();

            this.Game.Map.SetActiveSectorAsFriendly(this.Game.Map); //sets friendly in Active Quadrant  

            this.Game.MoveTimeForward(this.Game.Map, new Coordinate(lastQuadX, lastQuadY), destinationQuadrant);
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
        public bool SublightObstacleCheck(Coordinate lastSector, Coordinate sector, Sectors activeSectors)
        {
            //todo:  I think I destroyed a star and appeared in its place when navigating to a new quadrant.  (That or LRS is broken, or maybe it is working fine!)
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
                    Sector.Get(activeSectors, mySector.X, mySector.Y).Item = SectorItem.FriendlyShip;

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
                    this.Game.Write.Line("Stellar body " + star.Name.ToUpper() + " encountered while navigating at sector: [" + sector.X + "," +
                                      sector.Y + "]");
                    break;

                case SectorItem.HostileShip:
                    var hostile = currentObject;
                    this.Game.Write.Line("Ship " + hostile.Name + " encountered while navigating at sector: [" + sector.X + "," +
                                      sector.Y + "]");
                    break;

                    
                case SectorItem.Starbase:
                    this.Game.Write.Line("Starbase encountered while navigating at sector: [" + sector.X + "," + sector.Y + "]");
                    break;

                default:
                    this.Game.Write.Line("Detected an unidentified obstacle while navigating at sector: [" + sector.X + "," + sector.Y + "]");
                    break;
            }
        }

        //private Quadrant SetShipLocation(double vectorX, double vectorY)
        //{
        //    var shipSector = this.ShipConnectedTo.Sector;
        //    var shipQuadrant = this.ShipConnectedTo.Coordinate;

        //    shipSector.X = ((int)Math.Round(vectorX)) % 8; //sector info is in the vector
        //    shipSector.Y = ((int)Math.Round(vectorY)) % 8;

        //    var quadX = ((int)Math.Round(vectorX)) / 8; //quadrant info is in the vector
        //    var quadY = ((int)Math.Round(vectorY)) / 8;

        //    shipQuadrant.X = quadX;
        //    shipQuadrant.Y = quadY;

        //    Quadrant newActiveQuadrant = this.ShipConnectedTo.GetQuadrant();

        //    if (newActiveQuadrant == null)
        //    {
        //        throw new GameException("No quadrant to make active");
        //    }

        //    newActiveQuadrant.SetActive();

        //    this.Game.Map.SetActiveSectorAsFriendly(this.Game.Map); //sets friendly in Active Quadrant  

        //    return newActiveQuadrant; //contains the newly set sector in it
        //}

        public bool IsGalacticBarrier(ref int x, ref int y)
        {
            //todo: this barrier needs to be a computed size based upon app.config xy settings of how big the galaxy is.
            //Star Trek lore gives us a good excuse to limit playfield size.

            var upperBound = 7;

            this.BlockedByGalacticBarrier = false;

            if (x < 0)
            {
                x = 0; //todo: gridXLowerBound in app.config or calculated
                this.BlockedByGalacticBarrier = true;
            }
            else if (x > upperBound)
            {
                x = upperBound; //todo: gridXUpperBound in app.config or calculated
                this.BlockedByGalacticBarrier = true;
            }

            if (y < 0)
            {
                y = 0; //todo: gridYLowerBound in app.config or calculated
                this.BlockedByGalacticBarrier = true;
            }
            else if (y > upperBound)
            {
                y = upperBound; //todo: gridYUpperBound in app.config or calculated
                this.BlockedByGalacticBarrier = true;
            }

            if (this.BlockedByGalacticBarrier)
            {
                //todo: which one?  name it.
                this.Game.Write.Line("Galactic Barrier hit. Navigation stopped..");
            }

            return this.BlockedByGalacticBarrier;
        }

        //This prompt needs to be exposed to the user as an event
        public bool InvalidCourseCheck(out int direction)
        {
            var course = this.Game.Write.Course() + "Enter Course: ";
            string userDirection;

            if (this.Game.Write.PromptUser(course, out userDirection))
            {

                //todo: check to see if number is higher than 8

                if (!userDirection.IsNumeric() || userDirection.Contains("."))
                {
                    this.Game.Write.Line("Invalid course.");
                    direction = 0;
                     
                    return true;
                }

                var directionToCheck = Convert.ToInt32(userDirection);

                if (directionToCheck > 8 || directionToCheck < 0)
                {
                    this.Game.Write.Line("Invalid course..");
                    direction = 0;

                    return true;
                }
            }

            direction = Convert.ToInt32(userDirection);

            return false;
        }
    }
} 
