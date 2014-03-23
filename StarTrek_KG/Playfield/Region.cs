using System;
using System.Linq;
using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Playfield
{
    /// <summary>
    /// A Region in this game is a named area of space. It can contain ships, starbases, or stars. 
    /// </summary>
    public class Region : Coordinate, IRegion
    {
        #region Properties

        public RegionType Type { get; set; }
        public string Name { get; set; }

        public IMap Map { get; set; }

        //TODO: This property needs to be changed to a function, and that function needs to count Hostiles in this Region when called

        //for this to work, each sector needs to be able to store a hostile
        //public List<Ship> Hostiles { get; set; } //TODO: this needs to be changed to a List<ship> that have a hostile property=true

        public Sectors Sectors { get; set; }
        public bool Scanned { get; set; }
        public bool Empty { get; set; }

        public bool Active { get; set; }

        #endregion

        public Region(ICoordinate coordinate)
        {
            this.X = coordinate.X;
            this.Y = coordinate.Y;
        }

        public Region(IMap map, bool isNebulae = false)
        {
            this.Type = isNebulae ? RegionType.Nebulae : RegionType.GalacticSpace;

            this.Empty = true;
            this.Name = String.Empty;
            this.Map = map;
        }

        public Region(IMap map, Stack<string> baddieNames, FactionName stockBaddieFaction, bool isNebulae = false)
        {
            this.Empty = true;
            this.Map = map;
            this.Name = String.Empty;

            this.Create(baddieNames, stockBaddieFaction, isNebulae);
        }

        //todo: we might want to avoid passing in baddie names and set up baddies later..
        public Region(IMap map, Stack<string> RegionNames, Stack<string> baddieNames, FactionName stockBaddieFaction,
            out int nameIndex, bool addStars = false, bool makeNebulae = false)
        {
            this.Type = RegionType.GalacticSpace;
            this.Empty = true;

            this.Create(map, RegionNames, baddieNames, stockBaddieFaction, out nameIndex, addStars, makeNebulae);
        }

        public void SetActive()
        {
            this.Map.Regions.ClearActive();

            this.Active = true;
        }

        public void Create(Stack<string> baddieNames, FactionName stockBaddieFaction, bool addStars = true,
            bool makeNebulae = false)
        {
            if (this.Map == null)
            {
                throw new GameException("Set Map before calling this function");
            }

            this.InitializeSectors(this, new List<Sector>(), baddieNames, stockBaddieFaction, addStars, makeNebulae);
        }

        public void Create(IMap map, Stack<string> RegionNames, Stack<string> baddieNames,
            FactionName stockBaddieFaction, out int nameIndex, bool addStars = true, bool makeNebulae = false)
        {
            nameIndex = (Utility.Utility.Random).Next(baddieNames.Count);

            this.Map = map;
            this.InitializeSectors(this, new List<Sector>(), baddieNames, stockBaddieFaction, addStars, makeNebulae);

            if (RegionNames != null)
            {
                this.Name = RegionNames.Pop();
            }
        }

        public void Create(Stack<string> RegionNames, Stack<String> baddieNames, FactionName stockBaddieFaction,
            Coordinate RegionXY, out int nameIndex, IEnumerable<Sector> itemsToPopulate, bool addStars = true,
            bool isNebulae = false)
        {
            nameIndex = (Utility.Utility.Random).Next(RegionNames.Count);

            this.Name = RegionNames.Pop();

            this.X = RegionXY.X;
            this.Y = RegionXY.Y;

            var itemsInRegion = new List<Sector>();

            if (itemsToPopulate != null)
            {
                itemsInRegion =
                    itemsToPopulate.Where(i => i.RegionDef.X == this.X && i.RegionDef.Y == this.Y).ToList();
            }

            this.InitializeSectors(this, itemsInRegion, baddieNames, stockBaddieFaction, addStars, isNebulae);
        }

        public void InitializeSectors(Region Region,
            List<Sector> itemsToPopulate,
            Stack<string> baddieNames,
            FactionName stockBaddieFaction,
            bool addStars,
            bool makeNebulae = false)
        {
            Region.Sectors = new Sectors(); //todo: pull from app.config. initialize with limit

            //This loop creates empty sectors and populates as needed.
            for (var x = 0; x < Constants.SECTOR_MAX; x++) //todo: pull from app.config. initialize with limit
            {
                for (var y = 0; y < Constants.SECTOR_MAX; y++)
                {
                    this.PopulateMatchingItem(Region, itemsToPopulate, x, y, baddieNames, stockBaddieFaction,
                        makeNebulae);
                }
            }

            if (addStars)
            {
                //Randomly throw stars in
                this.AddStars(Region, (Utility.Utility.Random).Next(Constants.SECTOR_MAX));
            }

            if (makeNebulae)
            {
                Region.TransformIntoNebulae();
            }

            ////This is possible only in the test harness, as app code currently does not call this function with a null
            ////This makes the code more error tolerant.
            //if (itemsToPopulate == null)
            //{
            //    itemsToPopulate = new List<Sector>();
            //}

            //itemsToPopulate.AddRange(starsAdded);

            //todo: make this a test
            //    if (itemsToPopulate.Count != (queryOfItems in Region.Sectors)
            //    {
            //        //error.. error.. danger will robinson
            //        //actually, this check should go in a unit test.  dont need to do it here.
            //    }
        }

        public IEnumerable<Sector> AddStars(Region Region, int totalStarsInRegion)
        {
            Utility.Utility.ResetGreekLetterStack();

            this.CreateStars(Region, totalStarsInRegion);

            return Region.Sectors.Where(s => s.Item == SectorItem.Star);
        }

        public Sector AddStar(Region Region)
        {
            Utility.Utility.ResetGreekLetterStack();

            const int totalStarsInRegion = 1;

            var currentStarName = this.CreateStars(Region, totalStarsInRegion);

            return Region.Sectors.Single(s => s.Item == SectorItem.Star && ((Star) s.Object).Name == currentStarName);
        }

        public string CreateStars(Region Region, int totalStarsInRegion,
            SectorType starSectorType = SectorType.StarSystem)
        {
            string currentStarName = "";

            while (totalStarsInRegion > 0)
            {
                var x = (Utility.Utility.Random).Next(Constants.SECTOR_MAX);
                var y = (Utility.Utility.Random).Next(Constants.SECTOR_MAX);

                //todo: just pass in coordinate and get its item
                var sector = Region.Sectors.Single(s => s.X == x && s.Y == y);
                var sectorEmpty = sector.Item == SectorItem.Empty;

                if (sectorEmpty)
                {
                    if (totalStarsInRegion > 0)
                    {
                        var newStar = new Star();
                        bool foundStarName = false;

                        int counter = 0;
                        while (!foundStarName)
                        {
                            //There's a practical max of 9 stars before LRS is broken, so we shouldn't see this while happening more than 9 times for a new star
                            //unless one is adding stars via debug mode..
                            counter++;
                            var newNameLetter = Utility.Utility.RandomGreekLetter.Pop();

                            var starsInRegion =
                                Region.Sectors.Where(s => s.Object != null && s.Object.Type.Name == "Star").ToList();
                            var allStarsDontHaveNewDesignation =
                                starsInRegion.All(s => ((Star) s.Object).Designation != newNameLetter);

                            if (allStarsDontHaveNewDesignation)
                            {
                                foundStarName = true;

                                if (Region.Name == null) //todo: why do we have null Region names???
                                {
                                    Region.Name = "UNKNOWN Region " + newNameLetter + " " + counter;
                                        //todo: this could get dupes
                                }

                                currentStarName = Region.Name.ToUpper() + " " + newNameLetter;

                                newStar.Name = currentStarName;
                                newStar.Designation = newNameLetter;
                                sector.Item = SectorItem.Star;
                                sector.Type = starSectorType;

                                sector.Object = newStar;
                                totalStarsInRegion--;
                            }

                            //Assuming we are using the greek alphabet for star names, we don't want to create a lockup.
                            if (counter > 25)
                            {
                                this.Map.Write.Line("Too Many Stars.  Sorry.  Not gonna create more.");
                                foundStarName = true;
                            }
                        }
                    }
                }
            }

            return currentStarName;
        }

        public void PopulateMatchingItem(Region Region, ICollection<Sector> itemsToPopulate, int x, int y,
            Stack<string> baddieNames, FactionName stockBaddieFaction, bool isNebula)
        {
            var sectorItemToPopulate = SectorItem.Empty;

            try
            {
                if (itemsToPopulate != null)
                {
                    if (itemsToPopulate.Count > 0)
                    {
                        Sector sectorToPopulate = itemsToPopulate.SingleOrDefault(i => i.X == x && i.Y == y);

                        if (sectorToPopulate != null)
                        {
                            if ((Region.Type == RegionType.Nebulae) &&
                                (sectorToPopulate.Item == SectorItem.Starbase))
                            {
                                sectorItemToPopulate = SectorItem.Empty;
                            }
                            else
                            {
                                if (!(isNebula && (sectorToPopulate.Item == SectorItem.Starbase)))
                                {
                                    sectorItemToPopulate = sectorToPopulate.Item;
                                }
                            }
                        }
                        else
                        {
                            sectorItemToPopulate = SectorItem.Empty;
                        }

                        //todo: plop item down on map

                        //what does Output read?  cause output is blank
                        //output reads SectorItem.  Is sectorItem.Hostile being added?
                        //todo: new ship(coordinates)?
                        //todo: add to hostiles? 
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new GameException(ex.Message);
            }

            this.AddSector(Region, x, y, sectorItemToPopulate, baddieNames, stockBaddieFaction);
        }

        public void AddSector(Region Region, int x, int y, SectorItem itemToPopulate, Stack<string> stockBaddieNames,
            FactionName stockBaddieFaction)
        {
            var newlyCreatedSector = Sector.CreateEmpty(Region, new Coordinate(x, y));
            this.Map.Write.DebugLine("Added new Empty Sector to Region: " + Region.Name + " Coordinate: " +
                                     newlyCreatedSector);

            if (itemToPopulate == SectorItem.HostileShip)
            {
                //if a baddie name is passed, then use it.  otherwise
                var newShip = this.CreateHostileShip(newlyCreatedSector, stockBaddieNames, stockBaddieFaction);
                Region.AddShip(newShip, newlyCreatedSector);
            }
            else
            {
                newlyCreatedSector.Item = itemToPopulate;
            }

            Region.Sectors.Add(newlyCreatedSector);
        }

        public void AddShip(IShip ship, ISector toSector)
        {
            if (toSector == null)
            {
                this.Map.Write.DebugLine("No Sector passed. cannot add to Region: " + this.Name);
                throw new GameException("No Sector passed. cannot add to Region: " + this.Name);
            }

            if (ship == null)
            {
                this.Map.Write.DebugLine("No ship passed. cannot add to Region: " + this.Name);
                throw new GameException("No ship passed. cannot add to Region: " + this.Name);
            }

            this.Map.Write.DebugLine("Adding Ship: " + ship.Name + " to Region: " + this.Name + " Sector: " + toSector);

            var addToSector = this.GetSector(toSector) ?? toSector;
                //if we can't retrieve it, then it hasn't been created yet, so add to our new variable and the caller of this function can add it if they want

            try
            {
                addToSector.Object = ship;

                switch (ship.Allegiance)
                {
                    case Allegiance.GoodGuy:
                        addToSector.Item = SectorItem.PlayerShip;
                        break;

                    case Allegiance.BadGuy:
                        addToSector.Item = SectorItem.HostileShip;
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Map.Write.DebugLine("unable to add ship to sector " + toSector + ". " + ex.Message);
                throw new GameException("unable to add ship to sector " + toSector + ". " + ex.Message);
            }
        }

        public void RemoveShip(IShip ship)
        {
            //staple ship to sector passed.
            Sector sectorToAdd = this.Sectors.Single(s => s.X == ship.Sector.X && s.Y == ship.Sector.Y);
            sectorToAdd.Object = ship;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="listOfBaddies"></param>
        /// <param name="stockBaddieFaction"></param>
        /// <returns></returns>
        public Ship CreateHostileShip(ISector position, Stack<string> listOfBaddies, FactionName stockBaddieFaction)
        {
            //todo: modify this to populate more than a single baddie faction

            //todo: this should be a random baddie, from the list of baddies in app.config
            var hostileShip = new Ship(stockBaddieFaction, listOfBaddies.Pop(), position, this.Map);
                //yes.  This code can be misused.  There will be repeats of ship names if the stack isn't managed properly
            hostileShip.Sector.X = position.X;
            hostileShip.Sector.Y = position.Y;

            Shields.For(hostileShip).Energy = 300 + (Utility.Utility.Random).Next(200);

            this.Map.Write.DebugLine("Created Ship: " + hostileShip.Name);

            return hostileShip;
        }

        public void AddEmptySector(Region Region, int x, int y)
        {
            var sector = Sector.CreateEmpty(Region, new Coordinate(x, y));

            Region.Sectors.Add(sector);
        }

        public bool NoHostiles(List<Ship> hostiles)
        {
            if (hostiles.Count == 0)
            {
                this.Map.Write.Line(this.Map.Config.GetSetting<string>("RegionsNoHostileShips"));
                return true;
            }
            return false;
        }

        /// <summary>
        /// goes through each sector in this Region and counts hostiles
        /// </summary>
        /// <returns></returns>
        public List<IShip> GetHostiles()
        {
            var badGuys = new List<IShip>();

            try
            {
                if (this.Sectors != null)
                {
                    foreach (var sector in this.Sectors)
                    {
                        var @object = sector.Object;

                        if (@object != null)
                        {
                            var objectType = @object.Type.Name;
                            if (objectType == "Ship")
                            {
                                var possibleShipToGet = (IShip) @object;
                                if (possibleShipToGet.Allegiance == Allegiance.BadGuy)
                                {
                                    badGuys.Add(possibleShipToGet);
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new GameException(this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInRegion") +
                                            this.Name);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return badGuys;
        }

        /// <summary>
        /// goes through each sector in this Region and clears hostiles
        /// </summary>
        /// <returns></returns>
        public void ClearHostiles()
        {
            if (this.Sectors != null)
            {
                foreach (var sector in this.Sectors)
                {
                    var @object = sector.Object;

                    if (@object != null)
                    {
                        if (@object.Type.Name == "Ship")
                        {
                            var possibleShipToDelete = (IShip) @object;
                            if (possibleShipToDelete.Allegiance == Allegiance.BadGuy)
                            {
                                sector.Object = null;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new GameException(this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInRegion") + this.Name +
                                        ".");
            }
        }

        /// <summary>
        /// goes through each sector in this Region and clears item requested
        /// </summary>
        /// <returns></returns>
        public void ClearSectorsWithItem(SectorItem item)
        {
            if (this.Sectors != null)
            {
                var sectorsWithItem = this.Sectors.Where(sector => sector.Item == item);
                foreach (var sector in sectorsWithItem)
                {
                    sector.Item = SectorItem.Empty;
                    sector.Object = null;
                }
            }
            else
            {
                throw new GameException(this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInRegion") + this.Name +
                                        ".");
            }
        }

        public int GetStarbaseCount()
        {
            return Sectors.Count(sector => sector.Item == SectorItem.Starbase);
        }

        public int GetStarCount()
        {
            return Sectors.Count(sector => sector.Item == SectorItem.Star);
        }

        public ISector GetSector(ICoordinate coordinate)
        {
            return this.Sectors.FirstOrDefault(sector => sector.X == coordinate.X && sector.Y == coordinate.Y);
        }

        public bool IsNebulae()
        {
            return (this.Type == RegionType.Nebulae);
        }

        //todo: refactor these functions with LRS
        //todo: hand LRS a List<LRSResult>(); and then it can build its little grid.
        //or rather.. LongRangeScan subsystem comes up with the numbers, then hands it to the Print

        public IEnumerable<LRSResult> GetLRSFullData(Location location, Game game)
        {
            var scanData = new LRSFullData();

            bool currentlyInNebula = location.Region.Type == RegionType.Nebulae;

            for (var regionY = location.Region.Y - 1;
                regionY <= location.Region.Y + 1;
                regionY++)
            {
                for (var regionX = location.Region.X - 1;
                    regionX <= location.Region.X + 1;
                    regionX++)
                {
                    var outOfBounds = game.Map.OutOfBounds(regionY, regionX);

                    var currentResult = new LRSResult();

                    //todo: breaks here when regionX or regionY is 8
                    currentResult.Coordinate = new Coordinate(regionX, regionY, false);

                    if (!currentlyInNebula)
                    {
                        currentResult = this.GetRegionInfo(regionX, regionY, outOfBounds, game);
                        currentResult.MyLocation = location.Region.X == regionX &&
                                                   location.Region.Y == regionY;
                    }
                    else
                    {
                        //We are in a nebula.  LRS won't work here.
                        currentResult.Unknown = true;
                        currentResult.Coordinate = new Coordinate(regionX, regionY); //because we at least know the coordinate of what we don't know is..
                    }

                    scanData.Add(currentResult);
                }
            }
            return scanData;
        }

        private LRSResult GetRegionInfo(int regionX, int regionY, bool outOfBounds, Game game)
        {
            var currentResult = new LRSResult();

            if (!outOfBounds)
            {
                currentResult = this.GetRegionData(regionX, regionY, game);
            }
            else
            {
                currentResult.GalacticBarrier = true;
            }

            return currentResult;
        }

        private LRSResult GetRegionData(int regionX, int regionY, Game game)
        {
            Region regionToScan = Regions.Get(game.Map, this.CoordinateToScan(regionX, regionY, game.Config));
            var regionResult = new LRSResult();

            if (regionToScan.Type != RegionType.Nebulae)
            {
                regionResult = LongRangeScan.For(game.Map.Playership).Execute(regionToScan);
            }
            else
            {
                regionResult.Coordinate = new Coordinate(regionX, regionY);
                regionResult.Name = regionToScan.Name;
                regionResult.Unknown = true;
            }

            return regionResult;
        }

        private Coordinate CoordinateToScan(int regionX, int regionY, IStarTrekKGSettings config)
        {
            var max = config.GetSetting<int>("RegionMax") - 1;
            var min = config.GetSetting<int>("Region_MIN");

            int divinedRegionX = regionX;
            int divinedRegionY = regionY;

            if (regionX - 1 < min)
            {
                divinedRegionX = min;
            }

            if ((regionX > max))
            {
                divinedRegionX = max;
            }

            if (regionX + 1 > max)
            {
                divinedRegionX = max;
            }

            if (regionY - 1 < min)
            {
                divinedRegionY = min;
            }

            if ((regionY > max))
            {
                divinedRegionY = max;
            }

            var RegionToScan = new Coordinate(divinedRegionX, divinedRegionY);

            return RegionToScan;
        }

        public Location GetSectorNeighbor(Coordinate currentSector, IMap map)
        {//int sectorT, int sectorL
            var locationToGet = new Location();
            var regionCoordinateToGet = new Coordinate(this.X, this.Y);

            //todo: These Numbers are off
            var sectorCoordinateToGet = new Coordinate(currentSector.X, currentSector.Y, false);

            var direction = "";
            int x = 0;
            int y = 0;

            if (currentSector.X < 8 && currentSector.Y == -1)
            {
                direction = "left";
                x = 7;
                y = currentSector.X;

                var decremented = Region.Decrement(regionCoordinateToGet.X);
                regionCoordinateToGet.X = decremented;

                sectorCoordinateToGet.Y = Sector.Decrement(currentSector.Y);
            }

            if (currentSector.X < 8 && currentSector.Y == 8)
            {
                direction = "right";
                x = 0;
                y = currentSector.X;
                regionCoordinateToGet.X = Region.Increment(regionCoordinateToGet.X);
                sectorCoordinateToGet.Y = Sector.Increment(currentSector.Y);
            }

            if (currentSector.X == -1 && currentSector.Y < 8)
            {
                direction = "top";
                x = currentSector.Y;
                y = 7;

                regionCoordinateToGet.Y = Region.Decrement(regionCoordinateToGet.Y);
                sectorCoordinateToGet.X = Sector.Decrement(currentSector.X);
            }

            if (currentSector.X == 8 && currentSector.Y < 8)
            {
                direction = "bottom";
                x = currentSector.Y;
                y = 0;

                regionCoordinateToGet.Y = Region.Increment(regionCoordinateToGet.Y);
                sectorCoordinateToGet.X = Sector.Increment(currentSector.Y);
            }




            if (currentSector.X == -1 && currentSector.Y == 8)
            {
                direction = "topRight";
                x = 0;
                y = 7;

                regionCoordinateToGet.Y = Region.Decrement(regionCoordinateToGet.Y);
                sectorCoordinateToGet.Y = Sector.Decrement(currentSector.Y);
            }

            if (currentSector.X == -1 && currentSector.Y == -1)
            {
                direction = "topLeft";
                x = 7;
                y = 7;

                regionCoordinateToGet.Y = Region.Decrement(regionCoordinateToGet.Y);
                sectorCoordinateToGet.Y = Sector.Decrement(currentSector.Y);
            }

            if (currentSector.X == 8 && currentSector.Y == -1)
            {
                direction = "bottomLeft";
                x = 7;
                y = 0;

                regionCoordinateToGet.Y = Region.Decrement(regionCoordinateToGet.Y);
                sectorCoordinateToGet.Y = Sector.Decrement(currentSector.Y);
            }

            if (currentSector.X == 8 && currentSector.Y == 8)
            {
                direction = "bottomRight";
                x = 0;
                y = 0;

                regionCoordinateToGet.Y = Region.Increment(regionCoordinateToGet.Y);
                sectorCoordinateToGet.Y = Sector.Increment(currentSector.Y);
            }

            Region neighborSectorRegion = Regions.Get(map, new Coordinate(regionCoordinateToGet.X, regionCoordinateToGet.Y, false));

            if (neighborSectorRegion != null)
            {
                //todo: sector needs to be adjusted at this point depending on what direction user was traveling
                //if -1 then 7
                //if 8 then 0
                //etc..

                //todo: if neighbor is the same as me, then scan failed. return ?? or galactic barrier
 

                //todo: look at current XY and compare neighboring XY to get an indication of direction (make a function to do this)
                //string sectorDirection = Sector.GetNeighborDirection(currentSector, sectorCoordinateToGet);

                map.Write.Line("Region to the " + direction + 
                               " = " + neighborSectorRegion.Name + 
                               " [" + neighborSectorRegion.X + "," + neighborSectorRegion.Y +
                               "], Sector: [" + sectorCoordinateToGet.X + "," + sectorCoordinateToGet.Y + "]"); //t,l
            }
            else
            {
                map.Write.Line("Region to the " + direction + " = " + "NULL" + " [" + currentSector.X + "," + currentSector.Y + "]");
            }

            //this.Map.Write.SingleLine(currentResult.Location.Region.Name);

            //map.Write.WithNoEndCR("Region to the " + direction + "= [" + coordinateToGet.X + "," + coordinateToGet.Y +"]: ");
            var gotRegion = Regions.Get(map, regionCoordinateToGet);

            locationToGet.Region = gotRegion;
            locationToGet.Sector = gotRegion.Sectors.Single(s => s.X == x && s.Y == y);

            return locationToGet;
        }

        public Coordinate GetCoordinate()
        {
            return new Coordinate(this.X, this.Y, false);
        }


        //todo: refactor these with sector
        public static int Increment(int coordinateDimension)
        {
            int retVal;

            if (coordinateDimension >= Constants.SECTOR_MAX)
            {
                retVal = 0;
            }
            else
            {
                //todo: write a test for this in particular.  
                if (coordinateDimension < 7)
                {
                    retVal = coordinateDimension + 1;
                }
                else
                {
                    retVal = coordinateDimension;
                }
            }

            return retVal;
        }
        public static int Decrement(int coordinateDimension)
        {
            int retVal;

            if (coordinateDimension < Constants.SECTOR_MIN)
            {
                retVal = 7;
            }

            else
            {
                //todo: write a test for this in particular. 
                if (coordinateDimension > 0)
                {
                    retVal = coordinateDimension - 1;
                }
                else
                {
                    retVal = coordinateDimension;
                }
            }

            return retVal;
        }
    }
}


////todo: use 1 set of hostiles.  Create in sectors, count up for Regions
// public static void CreateHostileX(Region Region, int x, int y, IList<string> listOfBaddies)
//{
//    //todo: this should be a random baddie, from the list of baddies in app.config
//    //todo: note, in leter versions, baddies and allies can fight each other automatically (when they move to within range of each other.  status of the battles can be kept in the ships log (if observed by a friendly)
//
//          var index = (Utility.Random).Next(listOfBaddies.Count);
//        var hostileShip = new Ship(listOfBaddies[index], Region.Map, x, y);
//      hostileShip.Sector = new Sector(x, y);

//    Shields.For(hostileShip).Energy = 300 + (Utility.Random).Next(200);

//  Region.Hostiles.Add(hostileShip);

//listOfBaddies.RemoveAt(index); //remove name from our big list of names so we dont select it again
//}

//public Sector GetItem(int qX, int qY)
//{
//    return this.Sectors.Where(s => s.X == qX && s.Y == qY).Single();
//}
//public static bool OutOfBounds(int x, int y)
//{
//    return x < 0 || y < 0 || x > 7 || y > 7; //todo: this should be deduced from an app.config value
// }

//Loop for each Hostile and starbase.  Each go around pops a hostile
//(up to 3) into a random sector.  Same thing with Starbase, but the limit
//of starbases is 1.
//public static void Populate(Map map)
//{
//    var starbases = map.starbases;  //todo: once starbases are a list, then  
//    var hostiles = map.HostilesToSetUp;

//    while (hostiles > 0 || starbases > 0)
//    {
//        var x = (Utility.Random).Next(Constants.SECTOR_MAX);
//        var y = (Utility.Random).Next(Constants.SECTOR_MAX);

//        var Region = Regions.Get(map, x, y);

//        if (!Region.Starbase)
//        {
//            Region.Starbase = true;
//            starbases--;
//        }

//        if (Region.Hostiles.Count < 3) //todo: put 3 in app.config
//        {
//            Region.CreateHostile(Region, x, y, Map.baddieNames);
//            hostiles--;
//        }
//    }
//}

//////todo: use 1 set of hostiles.  Create in sectors, count up for Regions
//private static void AddHostile(Region Region, int x, int y)
//{
//    //Region.CreateHostile(Region, x, y, Map.baddieNames);

//    ////todo: a hostile was made, but he is not in any Region yet..

//    //List<Ship> allbadGuysInRegion = Region.Hostiles.Where(s => s.Allegiance == Allegiance.BadGuy).ToList();

//    //var k = allbadGuysInRegion[0];

//    ////fixme

//    ////all hostiles have the same XY.  this is rightfully failing

//    //var badGuy = allbadGuysInRegion.Where(s =>  
//    //                            s.Sector.X == x && 
//    //                            s.Sector.Y == y).Single();

//    ////this needs to be linked
//    ////add again?
//    //Region.Hostiles.Add(badGuy);
//}

//Output as enum??