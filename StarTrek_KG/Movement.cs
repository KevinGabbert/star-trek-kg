﻿using System;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG
{
    public class Movement : System
    {
        //todo: to fully abstract this out, this could be a Blocked by property, set to whatever stops us from moving.
        public bool BlockedByObstacle { get; set; }
        public bool BlockedByGalacticBarrier { get; set; }

        public Movement(Map map)
        {
            this.Map = map;
        }

        public void Execute(string direction, double distance, double distanceEntered, out int lastQuadX, out int lastQuadY)
        {
            var playerShipSector = this.Map.Playership.Sector;
            var playershipQuadrant = this.Map.Playership.GetQuadrant();

            lastQuadY = playershipQuadrant.Y;
            lastQuadX = playershipQuadrant.X;

            //todo: fix direction with sector to match quadrant direction

            //hack: bandaid fix. inelegant code
            //todo: Fix the mathematical need for a different numerical direction for sectors and quadrants.
            //todo: GetSectorDirection() and GetQuadrantDirection() need to return the same numbers
            int numericDirection = distanceEntered < 1 ? Movement.GetSectorDirection(direction) : Movement.GetQuadrantDirection(direction);

            double vectorLocationX = playershipQuadrant.X * 8 + playerShipSector.X;
            double vectorLocationY = playershipQuadrant.Y * 8 + playerShipSector.Y;

            var lastSector = new Coordinate(playerShipSector.X, playerShipSector.Y);

            //Clear Old Sector
            Sector.Get(this.Map.Quadrants.GetActive().Sectors, playerShipSector.X, playerShipSector.Y).Item = SectorItem.Empty;

            if (this.TravelThroughSectors(distanceEntered, distance, numericDirection, ref vectorLocationX, ref vectorLocationY, playershipQuadrant, lastSector)) goto EndNavigation;

            this.CheckForGalacticBarrier(ref vectorLocationX, ref vectorLocationY);

            //todo: if quadrant hasnt changed because ship cant move off map, then output a message that the galactic barrier has
            //been hit

            //todo: Map Friendly was set in obstacle check (move that here)
            this.SetShipLocation(vectorLocationX, vectorLocationY);//Set new Sector

        EndNavigation:

            Game.MoveTimeForward(this.Map, new Coordinate(lastQuadX, lastQuadY), new Coordinate(playershipQuadrant.X, playershipQuadrant.Y));  
        }

        private bool TravelThroughSectors(double distanceEntered, double distance, int numericDirection, 
                                          ref double vectorLocationX, ref double vectorLocationY, 
                                          Coordinate playershipQuadrant, Coordinate lastSector)
        {
            double angle = -(Math.PI * (numericDirection - 1.0) / 4.0);

            var distanceX = (distance*Math.Cos(angle));
            var distanceY = (distance*Math.Sin(angle));

            var vector = new Vector(distanceX / Constants.MOVEMENT_PRECISION,
                                    distanceY / Constants.MOVEMENT_PRECISION);

            var activeSectors = this.Map.Quadrants.GetActive().Sectors;

            for (var unit = 0; unit < Constants.MOVEMENT_PRECISION; unit++)
            {
                //todo: we might need to switch back to this line in the loop if quadrants are off
                //var activeSectors = this.Map.Quadrants.GetActive().Sectors;

                vectorLocationX += vector.X;
                vectorLocationY += vector.Y;

                int quadX = ((int) Math.Round(vectorLocationX)) / 8;
                int quadY = ((int) Math.Round(vectorLocationY)) / 8;

                //todo: this line causes galactic barrier out of bounds error? (becuase tries to assign 8 to X)
                //var quad = new Coordinate(quadX, quadY);

                if (quadX == playershipQuadrant.X && quadY == playershipQuadrant.Y)
                {
                    var closestSector = new Coordinate((int) Math.Round(vectorLocationX) % 8, 
                                                       (int) Math.Round(vectorLocationY) % 8);

                    if (distanceEntered < 1) //Check for obstacles only when navigating at sublight speeds.  i.e. Warp .1, etc
                    {
                        if (this.SublightObstacleCheck(lastSector, closestSector, activeSectors))
                        {
                            //vector_div is so you can get right up to an obstacle before hitting it.
                            Output.Write("For safety purposes, " + this.Map.Playership.Name + " has been brought to a stop.");
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
            try
            {
                var mySector = this.Map.Playership.Sector;
                var currentItem = Sector.Get(activeSectors, sector.X, sector.Y).Item;

                if (currentItem != SectorItem.Empty)
                {
                    mySector.X = lastSector.X;
                    mySector.Y = lastSector.Y;

                    //todo: move this to XXX label.  run tests.  should work.
                    Sector.Get(activeSectors, mySector.X, mySector.Y).Item = SectorItem.Friendly;

                    Output.Write("Detected an obstacle while navigating: " + currentItem.ToString() + " at sector: [" + sector.X + "," + sector.Y + "]");
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
        private static int GetSectorDirection(string direction)
        {
            // 4   5   6
            //   `.:.'
            // 3--<*>--7
            //   .':`.
            // 2   1   8

            var returnVal = 0;

            switch (direction)
            {
                case "e":
                    returnVal = 7;
                    break;
                case "ne":
                    returnVal = 6;
                    break;
                case "n":
                    returnVal = 5;
                    break;
                case "nw":
                    returnVal = 4;
                    break;
                case "w":
                    returnVal = 3;
                    break;
                case "sw":
                    returnVal = 2;
                    break;
                case "s":
                    returnVal = 1;
                    break;
                case "se":
                    returnVal = 8;
                    break;
            }

            return returnVal;
        }
        private static int GetQuadrantDirection(string direction)
        {
            // 6   7   8
            //   `.:.'
            // 5--<*>--1
            //   .':`.
            // 4   3   2

            var returnVal = 0;

            switch (direction)
            {
                case "e":
                    returnVal = 1;
                    break;
                case "ne":
                    returnVal = 2;
                    break;
                case "n": //correct
                    returnVal = 3; 
                    break;
                case "nw":
                    returnVal = 4;
                    break;
                case "w": //correct
                    returnVal = 5;
                    break;
                case "sw":
                    returnVal = 6;
                    break;
                case "s": //correct?
                    returnVal = 7;
                    break;
                case "se":
                    returnVal = 8;
                    break;
            }

            return returnVal;
        }

        private void SetShipLocation(double x, double y)
        {
            var shipSector = this.Map.Playership.Sector;
            var shipQuadrant = this.Map.Playership.QuadrantDef;

            shipSector.X = ((int)Math.Round(x)) % 8;
            shipSector.Y = ((int)Math.Round(y)) % 8;

            var quadX = ((int)Math.Round(x)) / 8;
            var quadY = ((int)Math.Round(y)) / 8;

            shipQuadrant.X = quadX;
            shipQuadrant.Y = quadY;

            this.Map.Playership.GetQuadrant().Active = true;
            Sector.SetFriendly(this.Map); //sets friendly in Active Quadrant  
        }

        //todo: put these values in app.config
        private void CheckForGalacticBarrier(ref double x, ref double y)
        {
            //todo: this barrier needs to be a computed size based upon app.config xy settings of how big the galaxy is.
            //Star Trek lore gives us a good excuse to limit playfield size.

            this.BlockedByGalacticBarrier = false;

            if (x < 0)
            {
                x = 0;
                this.BlockedByGalacticBarrier = true;
            }
            else if (x > 63)
            {
                x = 63;
                this.BlockedByGalacticBarrier = true;
            }

            if (y < 0)
            {
                y = 0;
                this.BlockedByGalacticBarrier = true;
            }
            else if (y > 63)
            {
                y = 63;
                this.BlockedByGalacticBarrier = true;
            }

            if (this.BlockedByGalacticBarrier)
            {
                //todo: which one?  name it.
                Output.Write("Galactic Barrier hit. Navigation stopped.");
            }
        }

        //This prompt needs to be exposed to the user as an event
        public bool InvalidCourseCheck(out string direction)
        {
            if (Command.PromptUser("Enter course (e,w,n,s): ", out direction))
            {
                if (!Constants.MAP_DIRECTION.Contains(direction))
                {
                    Output.Write("Invalid course.");
                    return true;
                }
            }

            return false;
        }
    }
}
