﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
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
        public bool BlockedByGalacticBarrier { get; set; }

        public Movement(Ship shipConnectedTo, Game game)
        {
            this.Game = game;
            this.ShipConnectedTo = shipConnectedTo;
        }

        public void Execute(MovementType movementType, int direction, int distance, out int lastRegionX, out int lastRegionY)
        {
            Region playershipRegion = this.ShipConnectedTo.GetRegion();

            lastRegionY = playershipRegion.Y;
            lastRegionX = playershipRegion.X;

            //var lastSector = new Coordinate(playerShipSector.X, playerShipSector.Y);

            Sector.GetFrom(this.ShipConnectedTo).Item = SectorItem.Empty;//Clear Old Sector
            Region newLocation = null;

            switch (movementType)
            {
                case MovementType.Impulse:
                    Location newShipLocation = this.TravelThroughSectors(distance, direction, this.ShipConnectedTo);

                    this.Game.Map.SetPlayershipInLocation(this.Game.Map, newShipLocation);
                    //this.ShipConnectedTo.SetLocation(newShipLocation);
                    break;

                case MovementType.Warp:
                    newLocation = this.TravelThroughRegions(Convert.ToInt32(distance), Convert.ToInt32(direction),
                        this.ShipConnectedTo);
                    this.ShipConnectedTo.Coordinate = newLocation;   
                    break;

                //todo:
                //case MovementType.X:
                //    newLocation = this.TravelThroughGalaxies()

                default:
                    this.Game.Write.Line("Unsupported Movement Type");
                    break;
            }

            if (newLocation != null)
            {
                newLocation.SetActive();
                this.Game.Map.SetPlayershipInActiveSector(this.Game.Map); //sets friendly in Active Region 

                this.Game.MoveTimeForward(this.Game.Map, new Coordinate(lastRegionX, lastRegionY), newLocation);
            }
        }

        private Location TravelThroughSectors(int distance, int direction, IShip playership)
        {
            // 4   5   6
            //   \ ↑ /
            //3 ← <*> → 7
            //   / ↓ \
            // 2   1   8

            direction = -direction + 8;

            Location newLocation = null;
            var currentRegion = playership.GetRegion();

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
                    case 0:
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

                
                //if on the edge of a Region, newSector will have negative numbers
                var newSectorCandidate = new Sector(new LocationDef(currentRegion.X, currentRegion.Y, Convert.ToInt32(currentSX), Convert.ToInt32(currentSY), false), false);
                var locationToScan = new Location(this.ShipConnectedTo.GetRegion(), newSectorCandidate);

                //run IRS on sector we are moving into
                IRSResult scanResult = ImmediateRangeScan.For(this.ShipConnectedTo).Scan(locationToScan);

                //If newSectorCandidate had negative numbers, then scanResult will have the newly updated region in it

                //todo: pass in result
                var barrierHit = this.IsGalacticBarrier(ref currentSX, ref currentSY);  //XY will be set to safe value in here
                if (barrierHit)
                {
                    break;
                }
                else
                {
                    bool obstacleEncountered = this.SublightObstacleCheck((Coordinate)playership.Sector, newSectorCandidate, currentRegion.Sectors);
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

                //TODO: VERIFY NEWLOCATION CONTAINS UPDATED SECTOR!
                //TODO: VERIFY NEWLOCATION CONTAINS UPDATED SECTOR!
                //TODO: VERIFY NEWLOCATION CONTAINS UPDATED SECTOR!
                //TODO: VERIFY NEWLOCATION CONTAINS UPDATED SECTOR!
                //TODO: VERIFY NEWLOCATION CONTAINS UPDATED SECTOR!

                //TODO: perhaps make a converter from IRSResult to Location
                newLocation = locationToScan;
                //newLocation.Sector.X = scanResult.Coordinate.X;
                //newLocation.Sector.Y = scanResult.Coordinate.Y;
                newLocation.Region = this.Game.Map.Regions.Where(r => r.Name == scanResult.Name).Single();

                //this.ShipConnectedTo.Coordinate = newLocation;   
            }

            return new Location();
        }

        private Region TravelThroughRegions(int distance, int direction, IShip playership)
        {
            // 4   5   6
            //   \ ↑ /
            //3 ← <*> → 7
            //   / ↓ \
            // 2   1   8

            //todo: get rid of this double-stuff. I'm only doing this so that IsGalacticBarrier can be used by both Region and Sector Navigation.
            int currentQX = playership.GetRegion().X;
            int currentQY = playership.GetRegion().Y;

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

                //todo: check if Region is nebula or out of bounds

                var barrierHit = this.IsGalacticBarrier(ref currentQX, ref currentQY);  //XY will be set to safe value in here
                if (barrierHit)
                {
                    break;
                }
                else
                {
                    bool nebulaEncountered = Regions.IsNebula(ShipConnectedTo.Map, new Coordinate(Convert.ToInt32(currentQX), Convert.ToInt32(currentQY)));
                    if (nebulaEncountered)
                    {
                        this.Game.Write.Line("Nebula Encountered. Navigation stopped to manually recalibrate warp coil");
                        break;
                    }
                }
            }

            return Regions.Get(ShipConnectedTo.Map, new Coordinate(Convert.ToInt32(currentQX), Convert.ToInt32(currentQY)));

            //todo: once we have found Region..
            //is target location blocked?
            //if true, then output that expected location was blocked, and ship's computers have picked a new spot
            
            //while loop
            //   pick a random sector
            //   check it for obstacle
            //if good then jump out of loop
        }

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
        public bool SublightObstacleCheck(Coordinate lastSector, Coordinate sector, Sectors activeSectors)
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
