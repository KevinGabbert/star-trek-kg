using System;
using System.Linq;
using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Constants;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;
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

        public IGame Game { private get; set; }
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
            this.Name = string.Empty;
            this.Map = map;
        }

        public Region(IMap map, int x, int y, RegionType regionType)
        {
            this.Type = regionType;

            this.Name = regionType == RegionType.GalacticBarrier ? "Galactic Barrier" : "";

            this.X = x;
            this.Y = y;
            this.Empty = true;
            this.Map = map;
        }

        public Region(IMap map, Stack<string> baddieNames, FactionName stockBaddieFaction, bool isNebulae = false)
        {
            this.Empty = true;
            this.Map = map;
            this.Name = string.Empty;

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

        public void Create(Stack<string> RegionNames, Stack<string> baddieNames, FactionName stockBaddieFaction,
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
            for (var x = 0; x < DEFAULTS.SECTOR_MAX; x++) //todo: pull from app.config. initialize with limit
            {
                for (var y = 0; y < DEFAULTS.SECTOR_MAX; y++)
                {
                    this.PopulateMatchingItem(Region, itemsToPopulate, x, y, baddieNames, stockBaddieFaction,
                        makeNebulae);
                }
            }

            if (addStars)
            {
                //Randomly throw stars in
                this.AddStars(Region, (Utility.Utility.Random).Next(DEFAULTS.SECTOR_MAX));
            }

            if (makeNebulae)
            {
                Region.TransformIntoNebulae();
            }

            ////This is possible only in the test harness, as app code currently does not call this function with a null
            ////This makes the code more error tolerant.
            //if (itemsToPopulate.IsNullOrEmpty())
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
                var x = (Utility.Utility.Random).Next(DEFAULTS.SECTOR_MAX);
                var y = (Utility.Utility.Random).Next(DEFAULTS.SECTOR_MAX);

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
                if (itemsToPopulate?.Count > 0)
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
            catch (Exception)
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
                var newShip = this.CreateHostileShip(newlyCreatedSector, stockBaddieNames, stockBaddieFaction, this.Game);
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
        /// <param name="game"></param>
        /// <returns></returns>
        public Ship CreateHostileShip(ISector position, Stack<string> listOfBaddies, FactionName stockBaddieFaction, IGame game)
        {
            //todo: modify this to populate more than a single baddie faction

            //todo: this should be a random baddie, from the list of baddies in app.config
            var hostileShip = new Ship(stockBaddieFaction, listOfBaddies.Pop(), position, this.Map, game)
            {
                //yes.  This code can be misused.  There will be repeats of ship names if the stack isn't managed properly
                Sector = {X = position.X, Y = position.Y},
            };
               
            var hostileShipShields = Shields.For(hostileShip);

            //var testinghostileShipShields = hostileShipShields.Game.RandomFactorForTesting;
            int hostileShipShieldsRandom = Utility.Utility.TestableRandom(hostileShipShields.ShipConnectedTo.Game); //testinghostileShipShields == 0 ? Utility.Utility.Random.Next(200) : testinghostileShipShields;

            hostileShipShields.Energy = 300 + hostileShipShieldsRandom; //todo: resource this out

            //int hostileShipEnergyRandom = Utility.Utility.TestableRandom(hostileShipShields.Game);
            hostileShip.Energy = 300; //todo: resource this out// + hostileShipEnergyRandom;

            string shipInfo = hostileShip.ToString();
            this.Map.Write.DebugLine($"Created {shipInfo}");

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

                if (this.Sectors != null)
                {
                    IEnumerable<IShip> hostiles = (from sector in this.Sectors

                                                    let objectToExamine = sector.Object
                                                    
                                                    where objectToExamine != null
                                                    where objectToExamine.Type.Name == OBJECT_TYPE.SHIP

                                                    let shipToGet = (IShip)objectToExamine

                                                    where !shipToGet.Destroyed
                                                    where shipToGet.Allegiance == Allegiance.BadGuy

                                                    select shipToGet);
                    badGuys.AddRange(hostiles);
                }
                else
                {
                    if (this.Type != RegionType.GalacticBarrier && this.Type != RegionType.Unknown)
                    {
                        throw new GameException(this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInRegion") + this.Name);
                    }
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
                IEnumerable<Sector> sectorsWithHostiles = (from sector in this.Sectors

                                                           let sectorObject = sector.Object

                                                           where sectorObject != null
                                                           where sectorObject.Type.Name == OBJECT_TYPE.SHIP

                                                           let possibleShipToDelete = (IShip)sectorObject

                                                           where possibleShipToDelete.Allegiance == Allegiance.BadGuy
                                                           select sector);

                foreach (Sector sector in sectorsWithHostiles)
                {
                    sector.Object = null;
                }
            }
            else
            {
                throw new GameException($"{this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInRegion")}{this.Name}.");
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
                throw new GameException($"{this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInRegion")}{this.Name}.");
            }
        }

        public int GetStarbaseCount()
        {
            return Sectors?.Count(sector => sector.Item == SectorItem.Starbase) ?? 0;
        }

        public int GetStarCount()
        {
            return Sectors?.Count(sector => sector.Item == SectorItem.Star) ?? 0;
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

        //todo: refactor this with GetLRSFullData.  Pay attention to OutOfBounds
        public IEnumerable<IRSResult> GetIRSFullData(Location shipLocation, IGame game)
        {
            var scanData = new List<IRSResult>();

            //todo:  //bool currentlyInNebula = shipLocation.Sector.Type == RegionType.Nebulae;

            for (var sectorX = shipLocation.Sector.X - 1;
                sectorX <= shipLocation.Sector.X + 1;
                sectorX++)
            {
                for (var sectorY = shipLocation.Sector.Y - 1;
                    sectorY <= shipLocation.Sector.Y + 1;
                    sectorY++)
                {
                    var outOfBounds = Region.OutOfBounds(shipLocation.Sector);

                    var currentResult = new IRSResult
                    {
                        Coordinate = new Coordinate(sectorX, sectorY)//todo: breaks here when regionX or regionY is 8
                    };

                    currentResult = this.GetSectorInfo(shipLocation.Region, new Coordinate(sectorX, sectorY), outOfBounds, game);
                    currentResult.MyLocation = shipLocation.Region.X == sectorX &&
                                                shipLocation.Region.Y == sectorY;

                    scanData.Add(currentResult);
                }
            }
            return scanData;
        }

        //todo: refactor this with Game.Map.OutOfBounds
        public static bool OutOfBounds(ISector coordinate)
        {
            var inTheNegative = coordinate.X < 0 || coordinate.Y < 0;
            var maxxed = coordinate.X == DEFAULTS.SECTOR_MAX || coordinate.Y == DEFAULTS.SECTOR_MAX;

            var yInRegion = coordinate.Y >= 0 && coordinate.Y < DEFAULTS.SECTOR_MAX;
            var xInRegion = coordinate.X >= 0 && coordinate.X < DEFAULTS.SECTOR_MAX;

            return (inTheNegative || maxxed) && !(yInRegion && xInRegion);
        }

        /// <summary>
        /// Returns a list of "LRSResult" objects.  These objects show basic information of the area surrounding the passed location.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public IEnumerable<LRSResult> GetLRSFullData(Location location, IGame game)
        {
            bool currentlyInNebula = location.Region.Type == RegionType.Nebulae;

            //todo: rewrite scanning method. this one is not very clear.

            //todo: if you create a coordinate that is not legal, then an exception is thrown.  this is bad.

            //1. get all regions next to location.Region

            //-1 -1
            //-1 +1
            //-1 0
            //0 -1
            //0 0
            //0 +1
            //+1 0
            //+1 -1
            //+1 +1

            int locationX = location.Region.X;
            int locationY = location.Region.Y;
            List<Region> thisRegionAndThoseNextToThisOne = new List<Region>
            {
                this.Item(currentlyInNebula, game, locationX, locationY),
                this.Item(currentlyInNebula, game, locationX - 1, locationY),
                this.Item(currentlyInNebula, game, locationX, locationY - 1),
                this.Item(currentlyInNebula, game, locationX + 1, locationY),
                this.Item(currentlyInNebula, game, locationX, locationY + 1),
                this.Item(currentlyInNebula, game, locationX + 1, locationY - 1),
                this.Item(currentlyInNebula, game, locationX - 1, locationY + 1),
                this.Item(currentlyInNebula, game, locationX + 1, locationY + 1),
                this.Item(currentlyInNebula, game, locationX - 1, locationY - 1)
            };

            //build map
            IEnumerable<LRSResult> scanData = thisRegionAndThoseNextToThisOne.Select(this.GetRegionData).ToList();

            //todo: set up galactic barrier space orr
            scanData.Single(r => r.Coordinate.X == location.Region.X && r.Coordinate.Y == location.Region.Y).MyLocation = true;

            return scanData;
        }

        private Region Item(bool currentlyInNebula, IGame game, int locationX, int locationY)
        {
            Region region;

            if (!currentlyInNebula)
            {
                region = game.Map.Regions.SingleOrDefault(r => r.X == locationX && r.Y == locationY) ??
                             new Region(new Coordinate(locationX, locationY))
                             { 
                                 Game = game,
                                 Empty = true,
                                 Type = RegionType.GalacticBarrier,
                                 Name = "Galactic Barrier",
                                 Scanned = true
                             };
            }
            else
            {
                region = new Region(new Coordinate(locationX, locationY))
                {
                    Game = game,
                    Type = RegionType.Unknown
                };
            }

            return region;
        }

        public IRSResult GetSectorInfo(Region currentRegion, ICoordinate sector, bool outOfBounds, IGame game)
        {
            var currentResult = new IRSResult();

            if (!outOfBounds)
            {
                if (!currentRegion.IsNebulae())
                {
                    currentResult = this.GetSectorData(currentRegion, sector, game);
                }
                else
                {
                    //currentResult.RegionName = "Unknown";
                    currentResult.Unknown = true;
                    currentResult.Coordinate = new Coordinate(currentRegion.X, currentRegion.Y);
                }
            }
            else
            {
                currentResult.GalacticBarrier = true;
                //todo: set region to Galactic barrier?
            }

            return currentResult;
        }

        private LRSResult GetRegionData(Region region)
        {
            //Region regionToScan; // = Regions.Get(game.Map, this.CoordinateToScan(region.X, region.Y, game.Config));
            var regionResult = new LRSResult();

            if (region.Type != RegionType.Nebulae)
            {
                regionResult = LongRangeScan.Execute(region);
            }
            else
            {
                regionResult.Coordinate = new Coordinate(region.X, region.Y);
                regionResult.Name = region.Name;
                //regionResult.Unknown = true; //todo: is it?
            }

            return regionResult;
        }

        private IRSResult GetSectorData(Region currentRegion, ICoordinate sector, IGame game)
        {
            Sector sectorToScan = this.Sectors.GetNoError(sector);

            Coordinate xx = sectorToScan ?? new Coordinate(sector.X, sector.Y);

            ISector sectorToExamine = new Sector(new LocationDef(currentRegion, xx));
            var locationToExamine = new Location(currentRegion, sectorToExamine);

            Location divinedLocationOnMap = currentRegion.DivineSectorOnMap(locationToExamine, this.Map);

            if (divinedLocationOnMap.Region.Type != RegionType.GalacticBarrier)
            {
                //int i;
            }
            else
            {
                
            }

            IRSResult sectorResult = ImmediateRangeScan.For(game.Map.Playership).Execute(divinedLocationOnMap);

            return sectorResult;
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

        /// <summary>
        /// Returns map Location info on the location Requested.
        /// If passed an area not in region passed, example: {ThisRegion}.[-1, 0], then the proper location will be divined & returned.
        /// </summary>
        /// <param name="locationToExamine"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public Location DivineSectorOnMap(Location locationToExamine, IMap map)
        {
            var result = new DivinedSectorResult
            {
                RegionCoordinateToGet = locationToExamine.Region,
                SectorCoordinateToGet = (Coordinate) locationToExamine.Sector,
                CurrentLocationX = locationToExamine.Sector.X,
                CurrentLocationY = locationToExamine.Sector.Y,
                Direction = "Here"
            };

            //todo: perhaps this code needs to be in the constructor -------------
            //todo: perhaps this code needs to be in the constructor -------------

            var locationToGet = new Location();
            var regionCoordinateToGet = new Coordinate(locationToExamine.Region.X, locationToExamine.Region.Y);
            var sectorCoordinateToGet = new Coordinate(locationToExamine.Sector.X, locationToExamine.Sector.Y);

            result = this.GetLeftEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);
            result = this.GetRightEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);
            result = this.GetTopEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);
            result = this.GetBottomEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);

            result = this.GetBottomRightEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);
            result = this.GetTopRightEdgeResult(locationToExamine, result, regionCoordinateToGet);
            result = this.GetTopLeftEdgeResult(locationToExamine, result, regionCoordinateToGet);
            result = this.GetBottomLeftEdgeResult(locationToExamine, result, regionCoordinateToGet);

            if (result.RegionCoordinateToGet == null)
            {
                throw new ArgumentException();
            }

            locationToGet.Region = Regions.Get(map, new Coordinate(result.RegionCoordinateToGet.X, result.RegionCoordinateToGet.Y));

            if (locationToGet.Region.Type != RegionType.GalacticBarrier)
            {
                //todo: moving to the 7th column in sector breaks things?
                locationToGet.Sector = locationToGet.Region.Sectors.Single(s => s.X == result.CurrentLocationX && s.Y == result.CurrentLocationY);
            }

            return locationToGet;
        }

        #region GetNeighbor Code

        private DivinedSectorResult GetBottomRightEdgeResult(Location locationToExamine, 
                                                              DivinedSectorResult result,
                                                              ICoordinate regionCoordinateToGet, 
                                                              ICoordinate sectorCoordinateToGet)
        {
            if (this.OffBottomRightEdge(locationToExamine))
            {
                result = result.Get("bottomRight",
                    regionCoordinateToGet.X += 1,
                    regionCoordinateToGet.Y += 1,
                    Sector.SIncrement(locationToExamine.Sector.X),
                    sectorCoordinateToGet.Y,   //todo:  This shows as 8, but current position needs to be fixed.  don't fix here.
                    0,
                    0);
            }
            return result;
        }

        private DivinedSectorResult GetBottomLeftEdgeResult(Location locationToExamine, 
                                                             DivinedSectorResult result,
                                                             ICoordinate regionCoordinateToGet)
        {
            if (this.OffBottomLeftEdge(locationToExamine))
            {
                result = result.Get("bottomLeft",
                    regionCoordinateToGet.X -= 1,
                    regionCoordinateToGet.Y, // -= 1
                    Sector.SIncrement(locationToExamine.Sector.X),
                    Sector.SDecrement(locationToExamine.Sector.Y),
                    7,
                    0);
            }
            return result;
        }

        private DivinedSectorResult GetTopLeftEdgeResult(Location locationToExamine, 
                                                          DivinedSectorResult result,
                                                          ICoordinate regionCoordinateToGet)
        {
            if (this.OffTopLeftEdge(locationToExamine))
            {
                result = result.Get("topLeft",
                    regionCoordinateToGet.X,
                    regionCoordinateToGet.Y, //-= 1
                    Sector.SDecrement(locationToExamine.Sector.X),
                    Sector.SDecrement(locationToExamine.Sector.Y),
                    7,
                    7);
            }
            return result;
        }

        private DivinedSectorResult GetTopRightEdgeResult(Location locationToExamine, 
                                                           DivinedSectorResult result,
                                                           ICoordinate regionCoordinateToGet)
        {
            if (this.OffTopRightEdge(locationToExamine))
            {
                result = result.Get("topRight",
                    regionCoordinateToGet.X,
                    regionCoordinateToGet.Y -= 1,
                    Sector.SDecrement(locationToExamine.Sector.X),
                    Sector.SIncrement(locationToExamine.Sector.Y),
                    0,
                    7);
            }
            return result;
        }

        private DivinedSectorResult GetBottomEdgeResult(Location locationToExamine, 
                                                         DivinedSectorResult result,
                                                         ICoordinate regionCoordinateToGet, 
                                                         ICoordinate sectorCoordinateToGet)
        {
            if (this.OffBottomEdge(locationToExamine))
            {
                result = result.Get("bottom",
                    regionCoordinateToGet.X,
                    regionCoordinateToGet.Y += 1,
                    Sector.SIncrement(locationToExamine.Sector.X),
                    sectorCoordinateToGet.Y,
                    locationToExamine.Sector.Y,
                    0);
            }
            return result;
        }

        private DivinedSectorResult GetTopEdgeResult(Location locationToExamine, 
                                                      DivinedSectorResult result,
                                                      ICoordinate regionCoordinateToGet, 
                                                      ICoordinate sectorCoordinateToGet)
        {
            if (this.OffTopEdge(locationToExamine))
            {
                result = result.Get("top",
                    regionCoordinateToGet.X,
                    regionCoordinateToGet.Y -= 1,
                    Sector.SDecrement(locationToExamine.Sector.X),
                    sectorCoordinateToGet.Y,
                    locationToExamine.Sector.Y,
                    7);
            }
            return result;
        }

        private DivinedSectorResult GetRightEdgeResult(Location locationToExamine, 
                                                        DivinedSectorResult result,
                                                        ICoordinate regionCoordinateToGet, 
                                                        ICoordinate sectorCoordinateToGet)
        {
            if (this.OffRightEdge(locationToExamine))
            {
                result = result.Get("right",
                    regionCoordinateToGet.X += 1,
                    regionCoordinateToGet.Y,
                    sectorCoordinateToGet.X,
                    Sector.SIncrement(locationToExamine.Sector.Y),
                    0,
                    locationToExamine.Sector.X);
            }
            return result;
        }

        private DivinedSectorResult GetLeftEdgeResult(Location locationToExamine, 
                                                       DivinedSectorResult result,
                                                       ICoordinate regionCoordinateToGet, 
                                                       ICoordinate sectorCoordinateToGet)
        {
            if (this.OffLeftEdge(locationToExamine))
            {
                result = result.Get("left",
                    regionCoordinateToGet.X -= 1,
                    regionCoordinateToGet.Y,
                    sectorCoordinateToGet.X,
                    Sector.SDecrement(locationToExamine.Sector.Y),
                    7,
                    locationToExamine.Sector.X);
            }
            return result;
        }

        private bool OffBottomRightEdge(Location locationToExamine)
        {
            return locationToExamine.Sector.X == 8 && locationToExamine.Sector.Y == 8;
        }

        private bool OffBottomLeftEdge(Location locationToExamine)
        {
            return locationToExamine.Sector.X == 8 && locationToExamine.Sector.Y == -1;
        }

        private bool OffTopLeftEdge(Location locationToExamine)
        {
            return locationToExamine.Sector.X == -1 && locationToExamine.Sector.Y == -1;
        }

        private bool OffTopRightEdge(Location locationToExamine)
        {
            return locationToExamine.Sector.X == -1 && locationToExamine.Sector.Y == 8;
        }

        private bool OffBottomEdge(Location locationToExamine)
        {
            return locationToExamine.Sector.X == 8 && locationToExamine.Sector.Y < 8;
        }

        private bool OffTopEdge(Location locationToExamine)
        {
            return locationToExamine.Sector.X == -1 && locationToExamine.Sector.Y < 8;
        }

        private bool OffRightEdge(Location locationToExamine)
        {
            return locationToExamine.Sector.X < 8 && locationToExamine.Sector.Y == 8;
        }

        private bool OffLeftEdge(Location locationToExamine)
        {
            return locationToExamine.Sector.X < 8 && locationToExamine.Sector.Y == -1;
        }

        #endregion

        public Coordinate GetCoordinate()
        {
            return new Coordinate(this.X, this.Y);
        }

        public bool Invalid()
        {
            bool invalid = this.X < DEFAULTS.REGION_MIN ||
                           this.X >= DEFAULTS.REGION_MAX ||
                           this.Y < DEFAULTS.REGION_MIN ||
                           this.Y >= DEFAULTS.REGION_MAX;

            return invalid;
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