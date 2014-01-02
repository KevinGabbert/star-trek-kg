using System;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Actors
{
    public class Movement : System
    {
        //todo: to fully abstract this out, this could be a Blocked by property, set to whatever stops us from moving.
        public bool BlockedByObstacle { get; set; }
        public bool BlockedByGalacticBarrier { get; set; }

        public Movement(Map map, Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
            this.Map = map;
        }

        public void Execute(string direction, double distance, double distanceEntered, out int lastQuadX, out int lastQuadY)
        {
            Sector playerShipSector = this.ShipConnectedTo.Sector;
            Quadrant playershipQuadrant = this.ShipConnectedTo.GetQuadrant();

            lastQuadY = playershipQuadrant.Y;
            lastQuadX = playershipQuadrant.X;

            //todo: fix direction with sector to match quadrant direction

            //hack: bandaid fix. inelegant code
            //todo: Fix the mathematical need for a different numerical direction for sectors and quadrants.
            //todo: GetSectorDirection() and GetQuadrantDirection() need to return the same numbers
            var movementDirection = Convert.ToDouble(direction);

            double numericDirection = distanceEntered < 1 ? movementDirection : Movement.GetQuadrantDirection(direction);

            double vectorLocationX = playershipQuadrant.X * 8 + playerShipSector.X;
            double vectorLocationY = playershipQuadrant.Y * 8 + playerShipSector.Y;

            var lastSector = new Coordinate(playerShipSector.X, playerShipSector.Y);

            //Clear Old Sector
            Sector.GetFrom(this.ShipConnectedTo).Item = SectorItem.Empty;

            Quadrant newLocation = null;

            if (this.TravelThroughSectors(distanceEntered, 
                                          distance, 
                                          numericDirection, 
                                          ref vectorLocationX, 
                                          ref vectorLocationY, 
                                          playershipQuadrant, 
                                          lastSector))
            {
                newLocation = playershipQuadrant; //We are staying in the same quadrant
                goto EndNavigation;
            }

            this.CheckForGalacticBarrier(ref vectorLocationX, ref vectorLocationY);

            //todo: if quadrant hasnt changed because ship cant move off map, then output a message that the galactic barrier has been hit

            //todo: Map Friendly was set in obstacle check (move that here)
            newLocation = this.SetShipLocation(vectorLocationX, vectorLocationY);//Set new Sector

        EndNavigation:

            Game.MoveTimeForward(this.Map, new Coordinate(lastQuadX, lastQuadY), newLocation);  
        }


        /// <summary>
        /// todo: do we need to modify this algorithm?
        /// </summary>
        /// <param name="distanceEntered"></param>
        /// <param name="distance"></param>
        /// <param name="numericDirection"></param>
        /// <param name="vectorLocationX"></param>
        /// <param name="vectorLocationY"></param>
        /// <param name="playershipQuadrant"></param>
        /// <param name="lastSector"></param>
        /// <returns></returns>
        private bool TravelThroughSectors(double distanceEntered, double distance, double numericDirection, 
                                          ref double vectorLocationX, ref double vectorLocationY, 
                                          Quadrant playershipQuadrant, Coordinate lastSector)
        {
            double angle = -(Math.PI * (numericDirection - 1.0) / 4.0);

            var distanceX = (distance*Math.Cos(angle));
            var distanceY = (distance*Math.Sin(angle));

            var vector = new Vector(distanceX / Constants.MOVEMENT_PRECISION,
                                    distanceY / Constants.MOVEMENT_PRECISION);

            var activeSectors = playershipQuadrant.Sectors;

            for (var unit = 0; unit < Constants.MOVEMENT_PRECISION; unit++)
            {
                vectorLocationX += vector.X;
                vectorLocationY += vector.Y;

                int quadX = ((int) Math.Round(vectorLocationX)) / 8;
                int quadY = ((int) Math.Round(vectorLocationY)) / 8;

                //todo: this line causes galactic barrier out of bounds error? (becuase tries to assign 8 to X)

                if (quadX == playershipQuadrant.X && quadY == playershipQuadrant.Y)
                {
                    var closestSector = new Coordinate((int) Math.Round(vectorLocationX) % 8, 
                                                       (int) Math.Round(vectorLocationY) % 8);

                    if (distanceEntered < 1) //Check for obstacles only when navigating at sublight speeds.  i.e. Warp .1, etc
                    {
                        if (this.SublightObstacleCheck(lastSector, closestSector, activeSectors))
                        {
                            //vector_div is so you can get right up to an obstacle before hitting it.
                            Output.Write.Line("All Stop.");
                            return true;
                        }
                    }

                    //todo: verify playershipXY is right next to obstacle to if obstacle hit         
                    lastSector = closestSector;
                }
            }
            return false;
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
                    Sector.Get(activeSectors, mySector.X, mySector.Y).Item = SectorItem.Friendly;

                    Movement.IdentifyObstacle(sector, currentObject, currentItem);

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

        private static void IdentifyObstacle(Coordinate sector, ISectorObject currentObject, SectorItem currentItem)
        {
            switch (currentItem)
            {
                case SectorItem.Star:
                    var star = (Star) currentObject;
                    Output.Write.Line("Stellar body " + star.Name.ToUpper() + " encountered while navigating at sector: [" + sector.X + "," +
                                      sector.Y + "]");
                    break;

                case SectorItem.Hostile:
                    var hostile = (Ship) currentObject;
                    Output.Write.Line("Ship " + hostile.Name + " encountered while navigating at sector: [" + sector.X + "," +
                                      sector.Y + "]");
                    break;

                default:
                    Output.Write.Line("Detected an unidentified obstacle while navigating at sector: [" + sector.X + "," + sector.Y + "]");
                    break;
            }
        }

        private static int GetQuadrantDirection(string direction)
        {
            //this function exists to correct a directional disparity
            //todo: correct this numerical disparity..

            // 6   7   8
            //   \ ↑ /  
            //5 ← <*> → 1
            //   / ↓ \  
            // 4   3   2

            var returnVal = 0;

            switch (direction)
            {
                case "7":
                    returnVal = 1;
                    break;
                case "6":
                    returnVal = 2;
                    break;
                case "5": //correct
                    returnVal = 3; 
                    break;
                case "4":
                    returnVal = 4;
                    break;
                case "3": //correct
                    returnVal = 5;
                    break;
                case "2":
                    returnVal = 6;
                    break;
                case "1": //correct?
                    returnVal = 7;
                    break;
                case "8":
                    returnVal = 8;
                    break;
            }

            return returnVal;
        }

        private Quadrant SetShipLocation(double x, double y)
        {
            var shipSector = this.ShipConnectedTo.Sector;
            var shipQuadrant = this.ShipConnectedTo.QuadrantDef;

            shipSector.X = ((int)Math.Round(x)) % 8;
            shipSector.Y = ((int)Math.Round(y)) % 8;

            var quadX = ((int)Math.Round(x)) / 8;
            var quadY = ((int)Math.Round(y)) / 8;

            shipQuadrant.X = quadX;
            shipQuadrant.Y = quadY;

            Quadrant newActiveQuadrant = this.ShipConnectedTo.GetQuadrant();

            if (newActiveQuadrant == null)
            {
                throw new GameException("No quadrant to make active");
            }

            newActiveQuadrant.Active = true;

            Map.SetFriendly(this.Map); //sets friendly in Active Quadrant  

            return newActiveQuadrant; //contains the newly set sector in it
        }

        //todo: put these values in app.config
        private void CheckForGalacticBarrier(ref double x, ref double y)
        {
            //todo: this barrier needs to be a computed size based upon app.config xy settings of how big the galaxy is.
            //Star Trek lore gives us a good excuse to limit playfield size.

            this.BlockedByGalacticBarrier = false;

            if (x < 0)
            {
                x = 0; //todo: gridXLowerBound in app.config or calculated
                this.BlockedByGalacticBarrier = true;
            }
            else if (x > 63)
            {
                x = 63; //todo: gridXUpperBound in app.config or calculated
                this.BlockedByGalacticBarrier = true;
            }

            if (y < 0)
            {
                y = 0; //todo: gridYLowerBound in app.config or calculated
                this.BlockedByGalacticBarrier = true;
            }
            else if (y > 63)
            {
                y = 63; //todo: gridYUpperBound in app.config or calculated
                this.BlockedByGalacticBarrier = true;
            }

            if (this.BlockedByGalacticBarrier)
            {
                //todo: which one?  name it.
                Output.Write.Line("Galactic Barrier hit. Navigation stopped.");
            }
        }

        //This prompt needs to be exposed to the user as an event
        public bool InvalidCourseCheck(out string direction)
        {
            var course = Output.Draw.Course() + "Enter Course: ";
            string userDirection = "0";

            if (Command.PromptUser(course, out userDirection))
            {

                //todo: check to see if is numeric
                //todo: check to see if number is higher than 8

                //if (!Constants.MAP_DIRECTION.Contains(userDirection))
                //{
                //    Output.Write.Line("Invalid course.");
                //    return true;
                //}

                //todo: convert to double
            }

            direction = userDirection;

            return false;
        }
    }
} 
