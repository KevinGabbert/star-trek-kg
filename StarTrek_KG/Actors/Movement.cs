using System;
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

        public Movement(Ship shipConnectedTo, Game game)
        {
            base.Game = game;
            base.ShipConnectedTo = shipConnectedTo;
        }

        public void Execute(MovementType movementType, int direction, int distance, out int lastRegionX, out int lastRegionY)
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
                    this.Game.Write.Line("Unsupported Movement Type");
                    break;
            }
        }

        private void TravelThroughSectors(int distance, int direction, IShip travellingShip)
        {
            // 4   5   6
            //   \ ↑ /
            //3 ← <*> → 7
            //   / ↓ \
            // 2   1   8

            direction = -direction + 8;

            var currentRegion = travellingShip.GetRegion();

            int currentSX = travellingShip.Sector.X;
            int currentSY = travellingShip.Sector.Y;

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
                var newSectorCandidate = new Sector(new LocationDef(currentRegion.X, currentRegion.Y, currentSX, currentSY), false);

                var locationToScan = new Location(this.ShipConnectedTo.GetRegion(), newSectorCandidate);

                //run IRS on sector we are moving into
                IRSResult scanResult = ImmediateRangeScan.For(this.ShipConnectedTo).Scan(locationToScan);

                //If newSectorCandidate had negative numbers, then scanResult will have the newly updated region in it

                if (scanResult.GalacticBarrier)
                {
                    this.Game.Write.Line("All Stop. Cannot cross Galactic Barrier.");
                    return;
                }
                else
                {
                    //throw new NotImplementedException(); //how we gonna check for obstacles if scanresult has bad numbers in it?

                    //AdjustSectorToNewRegion may need to be called here

                    bool obstacleEncountered = this.SublightObstacleCheck((Coordinate) travellingShip.Sector, newSectorCandidate, currentRegion.Sectors);
                    if (obstacleEncountered)
                    {
                        this.Game.Write.Line("All Stop.");
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

                //todo: finish this.
                var shipRegion = travellingShip.GetRegion();

                if (newLocation.Region != shipRegion)
                {
                    newLocation.Sector = this.AdjustSectorToNewRegion(newLocation, this.Game.Map, shipRegion);
                }

                this.Game.Map.SetPlayershipInLocation(travellingShip, this.Game.Map, newLocation);

                //todo:  this.Game.MoveTimeForward(this.Game.Map, new Coordinate(lastRegionX, lastRegionY), newLocation);
            }
        }

        /// <summary>
        /// This will take any segative or > max values and reset them to the new region
        /// </summary>
        /// <param name="locationWithNewRegionButBadSectorNumbers"></param>
        /// <param name="map"></param>
        /// <param name="currentRegion"></param>
        /// <returns></returns>
        private ISector AdjustSectorToNewRegion(Location locationWithNewRegionButBadSectorNumbers, IMap map, Region currentRegion)
        {
            //you have the old region, and you have a sector with negative values. 
            var result = currentRegion.DivineSectorOnMap(locationWithNewRegionButBadSectorNumbers, map);  //.  This should tell you what sector should be

            //We are ignoring result.Region because it will be wrong.  
            //TODO: Refactor .DivineSectorOnMap to take just a sector (or create overload)

            //not sure why, but using .DivineSectorOnMap() this way switches the axes.  (another reason to refactor!) 


            ////HACK: This is for test only.  Sector.Item/.Object etc will not be updated.
            //var newY = result.Sector.X;
            //var newX = result.Sector.Y;

            //result.Sector.X = newX;
            //result.Sector.Y = newY;

            return result.Sector;
        }

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
                        base.Game.Write.Line(this.NEBULA_ENCOUNTERED);
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
        private bool SublightObstacleCheck(Coordinate lastSector, Coordinate sector, Sectors activeSectors)
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

        //This prompt needs to be exposed to the user as an event
        public bool PromptAndCheckCourse(Game.PromptFunc<string, bool> promptCourse, out int direction)
        {
            //var course = this.Game.Write.Course() + "Enter Course: ";
            string userDirection = "";

            bool userEnteredCourse = promptCourse.Invoke($"{this.Game.Write.Course()} Enter Course: ", out userDirection);

            if (userEnteredCourse)
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
