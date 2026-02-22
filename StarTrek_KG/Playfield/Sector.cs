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
    /// A Sector in this game is a named area of space. It can contain ships, starbases, or stars. 
    /// </summary>
    public class Sector : Point, ISector
    {
        #region Properties

        public SectorType Type { get; set; }
        public string Name { get; set; }

        public IMap Map { get; set; }

        //TODO: This property needs to be changed to a function, and that function needs to count Hostiles in this Sector when called

        //for this to work, each sector needs to be able to store a hostile
        //public List<Ship> Hostiles { get; set; } //TODO: this needs to be changed to a List<ship> that have a hostile property=true

        public Coordinates Coordinates { get; set; }
        public bool Scanned { get; set; }
        public bool Empty { get; set; }

        public bool Active { get; set; }

        #endregion

        public Sector(IPoint point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public Sector(IMap map, bool isNebulae = false)
        {
            this.Type = isNebulae ? SectorType.Nebulae : SectorType.GalacticSpace;

            this.Empty = true;
            this.Name = string.Empty;
            this.Map = map;
        }

        public Sector(IMap map, int x, int y, SectorType regionType)
        {
            this.Type = regionType;

            this.Name = regionType == SectorType.GalacticBarrier ? "Galactic Barrier" : "";

            this.X = x;
            this.Y = y;
            this.Empty = true;
            this.Map = map;
        }

        public Sector(IMap map, Stack<string> baddieNames, FactionName stockBaddieFaction, bool isNebulae = false)
        {
            this.Empty = true;
            this.Map = map;
            this.Name = string.Empty;

            this.Create(baddieNames, stockBaddieFaction, isNebulae);
        }

        //todo: we might want to avoid passing in baddie names and set up baddies later..
        public Sector(IMap map, Stack<string> sectorNames, Stack<string> baddieNames, FactionName stockBaddieFaction,
            out int nameIndex, bool addStars = false, bool makeNebulae = false)
        {
            this.Type = SectorType.GalacticSpace;
            this.Empty = true;

            this.Create(map, sectorNames, baddieNames, stockBaddieFaction, out nameIndex, addStars, makeNebulae);
        }

        public void SetActive()
        {
            this.Map.Sectors.ClearActive();

            this.Active = true;
        }

        public void Create(Stack<string> baddieNames, FactionName stockBaddieFaction, bool addStars = true,
            bool makeNebulae = false)
        {
            if (this.Map == null)
            {
                throw new GameException("Set Map before calling this function");
            }

            this.InitializeCoordinates(this, new List<Coordinate>(), baddieNames, stockBaddieFaction, addStars, makeNebulae);
        }

        public void Create(IMap map, Stack<string> sectorNames, Stack<string> baddieNames,
            FactionName stockBaddieFaction, out int nameIndex, bool addStars = true, bool makeNebulae = false)
        {
            nameIndex = Utility.Utility.Random.Next(baddieNames.Count);

            this.Map = map;
            this.InitializeCoordinates(this, new List<Coordinate>(), baddieNames, stockBaddieFaction, addStars, makeNebulae);

            if (sectorNames != null)
            {
                this.Name = sectorNames.Pop();
            }
        }

        public void Create(Stack<string> sectorNames, Stack<string> baddieNames, FactionName stockBaddieFaction,
            Point sectorPoint, out int nameIndex, IEnumerable<Coordinate> itemsToPopulate, bool addStars = true,
            bool isNebulae = false)
        {
            nameIndex = Utility.Utility.Random.Next(sectorNames.Count);

            this.Name = sectorNames.Pop();

            this.X = sectorPoint.X;
            this.Y = sectorPoint.Y;

            var itemsInSector = new List<Coordinate>();

            if (itemsToPopulate != null)
            {
                itemsInSector =
                    itemsToPopulate.Where(i => i.SectorDef.X == this.X && i.SectorDef.Y == this.Y).ToList();
            }

            this.InitializeCoordinates(this, itemsInSector, baddieNames, stockBaddieFaction, addStars, isNebulae);
        }

        public void InitializeCoordinates(Sector Sector,
            List<Coordinate> itemsToPopulate,
            Stack<string> baddieNames,
            FactionName stockBaddieFaction,
            bool addStars,
            bool makeNebulae = false)
        {
            Sector.Coordinates = new Coordinates(); //todo: pull from app.config. initialize with limit

            //This loop creates empty sectors and populates as needed.
            for (var x = 0; x < DEFAULTS.COORDINATE_MAX; x++) //todo: pull from app.config. initialize with limit
            {
                for (var y = 0; y < DEFAULTS.COORDINATE_MAX; y++)
                {
                    this.PopulateMatchingItem(Sector, itemsToPopulate, x, y, baddieNames, stockBaddieFaction,
                        makeNebulae);
                }
            }

            if (addStars)
            {
                //Randomly throw stars in
                this.AddStars(Sector, Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MAX));
            }

            if (makeNebulae)
            {
                Sector.TransformIntoNebulae();
            }

            ////This is possible only in the test harness, as app code currently does not call this function with a null
            ////This makes the code more error tolerant.
            //if (itemsToPopulate.IsNullOrEmpty())
            //{
            //    itemsToPopulate = new List<Coordinate>();
            //}

            //itemsToPopulate.AddRange(starsAdded);

            //todo: make this a test
            //    if (itemsToPopulate.Count != (queryOfItems in Sector.Coordinates)
            //    {
            //        //error.. error.. danger will robinson
            //        //actually, this check should go in a unit test.  dont need to do it here.
            //    }
        }

        public IEnumerable<Coordinate> AddStars(Sector Sector, int totalStarsInSector)
        {
            Utility.Utility.ResetGreekLetterStack();

            this.CreateStars(Sector, totalStarsInSector);

            return Sector.Coordinates.Where(s => s.Item == CoordinateItem.Star);
        }

        public Coordinate AddStar(Sector Sector)
        {
            Utility.Utility.ResetGreekLetterStack();

            const int totalStarsInSector = 1;

            var currentStarName = this.CreateStars(Sector, totalStarsInSector);

            return Sector.Coordinates.Single(s => s.Item == CoordinateItem.Star && ((Star) s.Object).Name == currentStarName);
        }

        public string CreateStars(Sector Sector, int totalStarsInSector,
            CoordinateType starSectorType = CoordinateType.StarSystem)
        {
            string currentStarName = "";

            while (totalStarsInSector > 0)
            {
                var x = Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MAX);
                var y = Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MAX);

                //todo: just pass in coordinate and get its item
                var sector = Sector.Coordinates.Single(s => s.X == x && s.Y == y);
                var sectorEmpty = sector.Item == CoordinateItem.Empty;

                if (sectorEmpty)
                {
                    if (totalStarsInSector > 0)
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

                            var starsInSector =
                                Sector.Coordinates.Where(s => s.Object != null && s.Object.Type.Name == "Star").ToList();

                            var allStarsDontHaveNewDesignation =
                                starsInSector.All(s => ((Star) s.Object).Designation != newNameLetter);

                            if (allStarsDontHaveNewDesignation)
                            {
                                foundStarName = true;

                                if (Sector.Name == null) //todo: why do we have null Sector names???
                                {
                                    Sector.Name = $"UNKNOWN Sector {newNameLetter} {counter}";
                                        //todo: this could get dupes
                                }

                                currentStarName = $"{Sector.Name.ToUpper()} {newNameLetter}";

                                newStar.Name = currentStarName;
                                newStar.Designation = newNameLetter;
                                sector.Item = CoordinateItem.Star;
                                sector.Type = starSectorType;

                                sector.Object = newStar;
                                totalStarsInSector--;
                            }

                            //Assuming we are using the greek alphabet for star names, we don't want to create a lockup.
                            if (counter > 25)
                            {
                                this.Map.Write.Line("Star Creation limit reached.");
                                foundStarName = true;
                            }
                        }
                    }
                }
            }

            return currentStarName;
        }

        public void PopulateMatchingItem(Sector Sector, ICollection<Coordinate> itemsToPopulate, int x, int y,
            Stack<string> baddieNames, FactionName stockBaddieFaction, bool isNebula)
        {
            var sectorItemToPopulate = CoordinateItem.Empty;

            try
            {
                if (itemsToPopulate?.Count > 0)
                {
                    Coordinate sectorToPopulate = itemsToPopulate.SingleOrDefault(i => i.X == x && i.Y == y);

                    if (sectorToPopulate != null)
                    {
                        if ((Sector.Type == SectorType.Nebulae) &&
                            (sectorToPopulate.Item == CoordinateItem.Starbase))
                        {
                            sectorItemToPopulate = CoordinateItem.Empty;
                        }
                        else
                        {
                            if (!(isNebula && (sectorToPopulate.Item == CoordinateItem.Starbase)))
                            {
                                sectorItemToPopulate = sectorToPopulate.Item;
                            }
                        }
                    }
                    else
                    {
                        sectorItemToPopulate = CoordinateItem.Empty;
                    }

                    //todo: plop item down on map

                    //what does Output read?  cause output is blank
                    //output reads CoordinateItem.  Is sectorItem.Hostile being added?
                    //todo: new ship(coordinates)?
                    //todo: add to hostiles? 
                }
            }
            catch (Exception)
            {
                //throw new GameException(ex.Message);
            }

            this.AddCoordinate(Sector, x, y, sectorItemToPopulate, baddieNames, stockBaddieFaction);
        }

        public void AddCoordinate(Sector Sector, int x, int y, CoordinateItem itemToPopulate, Stack<string> stockBaddieNames, FactionName stockBaddieFaction)
        {
            var newlyCreatedSector = Coordinate.CreateEmpty(Sector, new Point(x, y));
            this.Map.Write.DebugLine($"Added new Empty Coordinate to Sector: {Sector.Name} Point: {newlyCreatedSector}");

            if (itemToPopulate == CoordinateItem.HostileShip)
            {
                //if a baddie name is passed, then use it.  otherwise
                var newShip = this.CreateHostileShip(newlyCreatedSector, stockBaddieNames, stockBaddieFaction, this.Map.Game);
                Sector.AddShip(newShip, newlyCreatedSector);
            }
            else
            {
                newlyCreatedSector.Item = itemToPopulate;
            }

            Sector.Coordinates.Add(newlyCreatedSector);
        }

        public void AddShip(IShip ship, ICoordinate toCoordinate)
        {
            if (toCoordinate == null)
            {
                this.Map.Write.DebugLine("No Coordinate passed. cannot add to Sector: " + this.Name);
                throw new GameException("No Coordinate passed. cannot add to Sector: " + this.Name);
            }

            if (ship == null)
            {
                this.Map.Write.DebugLine("No ship passed. cannot add to Sector: " + this.Name);
                throw new GameException("No ship passed. cannot add to Sector: " + this.Name);
            }

            this.Map.Write.DebugLine("Adding Ship: " + ship.Name + " to Sector: " + this.Name + " Coordinate: " + toCoordinate);

            var addToSector = this.GetCoordinate(toCoordinate) ?? toCoordinate;
                //if we can't retrieve it, then it hasn't been created yet, so add to our new variable and the caller of this function can add it if they want

            try
            {
                addToSector.Object = ship;

                switch (ship.Allegiance)
                {
                    case Allegiance.GoodGuy:
                        addToSector.Item = CoordinateItem.PlayerShip;
                        break;

                    case Allegiance.BadGuy:
                        addToSector.Item = CoordinateItem.HostileShip;
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Map.Write.DebugLine("unable to add ship to sector " + toCoordinate + ". " + ex.Message);
                throw new GameException("unable to add ship to sector " + toCoordinate + ". " + ex.Message);
            }
        }

        public void RemoveShip(IShip ship)
        {
            //staple ship to sector passed.
            Coordinate sectorToAdd = this.Coordinates.Single(s => s.X == ship.Coordinate.X && s.Y == ship.Coordinate.Y);
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
        public Ship CreateHostileShip(ICoordinate position, Stack<string> listOfBaddies, FactionName stockBaddieFaction, IGame game)
        {
            //todo: modify this to populate more than a single baddie faction

            //todo: this should be a random baddie, from the list of baddies in app.config
            var hostileShip = new Ship(stockBaddieFaction, listOfBaddies.Pop(), position, this.Map)
            {
                //yes.  This code can be misused.  There will be repeats of ship names if the stack isn't managed properly
                Coordinate = {X = position.X, Y = position.Y},
            };
               
            var hostileShipShields = Shields.For(hostileShip);

            //var testinghostileShipShields = hostileShipShields.Game.RandomFactorForTesting;
            int hostileShipShieldsRandom = Utility.Utility.TestableRandom(hostileShipShields.ShipConnectedTo.Map.Game); //testinghostileShipShields == 0 ? Utility.Utility.Random.Next(200) : testinghostileShipShields;

            hostileShipShields.Energy = 300 + hostileShipShieldsRandom; //todo: resource this out

            //int hostileShipEnergyRandom = Utility.Utility.TestableRandom(hostileShipShields.Game);
            hostileShip.Energy = 300; //todo: resource this out// + hostileShipEnergyRandom;

            string shipInfo = hostileShip.ToString();
            this.Map.Write.DebugLine($"Created {shipInfo}");

            return hostileShip;
        }

        public void AddEmptyCoordinate(Sector Sector, int x, int y)
        {
            var sector = Coordinate.CreateEmpty(Sector, new Point(x, y));

            Sector.Coordinates.Add(sector);
        }

        public bool NoHostiles(List<Ship> hostiles)
        {
            if (hostiles.Count == 0)
            {
                this.Map.Write.Line(this.Map.Config.GetSetting<string>("SectorsNoHostileShips"));
                return true;
            }
            return false;
        }

        /// <summary>
        /// goes through each sector in this Sector and counts hostiles
        /// </summary>
        /// <returns></returns>
        public List<IShip> GetHostiles()
        {
            var badGuys = new List<IShip>();

                if (this.Coordinates != null)
                {
                    IEnumerable<IShip> hostiles = from sector in this.Coordinates

                        let objectToExamine = sector.Object
                                                    
                        where objectToExamine != null
                        where objectToExamine.Type.Name == OBJECT_TYPE.SHIP

                        let shipToGet = (IShip)objectToExamine

                        where !shipToGet.Destroyed
                        where shipToGet.Allegiance == Allegiance.BadGuy

                        select shipToGet;
                    badGuys.AddRange(hostiles);
                }
                else
                {
                    if (this.Type != SectorType.GalacticBarrier && this.Type != SectorType.Unknown)
                    {
                        throw new GameException(this.Map.Config.GetSetting<string>("DebugNoSetUpCoordinatesInSector") + this.Name);
                    }
                }

            return badGuys;
        }

        /// <summary>
        /// goes through each sector in this Sector and clears hostiles
        /// </summary>
        /// <returns></returns>
        public void ClearHostiles()
        {
            if (this.Coordinates != null)
            {
                IEnumerable<Coordinate> sectorsWithHostiles = from sector in this.Coordinates

                    let sectorObject = sector.Object

                    where sectorObject != null
                    where sectorObject.Type.Name == OBJECT_TYPE.SHIP

                    let possibleShipToDelete = (IShip)sectorObject

                    where possibleShipToDelete.Allegiance == Allegiance.BadGuy
                    select sector;

                foreach (Coordinate sector in sectorsWithHostiles)
                {
                    sector.Object = null;
                }
            }
            else
            {
                throw new GameException($"{this.Map.Config.GetSetting<string>("DebugNoSetUpCoordinatesInSector")}{this.Name}.");
            }
        }

        /// <summary>
        /// goes through each sector in this Sector and clears item requested
        /// </summary>
        /// <returns></returns>
        public void ClearSectorsWithItem(CoordinateItem item)
        {
            if (this.Coordinates != null)
            {
                var sectorsWithItem = this.Coordinates.Where(sector => sector.Item == item);
                foreach (var sector in sectorsWithItem)
                {
                    sector.Item = CoordinateItem.Empty;
                    sector.Object = null;
                }
            }
            else
            {
                throw new GameException($"{this.Map.Config.GetSetting<string>("DebugNoSetUpCoordinatesInSector")}{this.Name}.");
            }
        }

        public int GetStarbaseCount()
        {
            return Coordinates?.Count(sector => sector.Item == CoordinateItem.Starbase) ?? 0;
        }

        public int GetStarCount()
        {
            return Coordinates?.Count(sector => sector.Item == CoordinateItem.Star) ?? 0;
        }

        public ICoordinate GetCoordinate(IPoint coordinate)
        {
            return this.Coordinates.FirstOrDefault(sector => sector.X == coordinate.X && sector.Y == coordinate.Y);
        }

        public bool IsNebulae()
        {
            return this.Type == SectorType.Nebulae;
        }

        //todo: refactor these functions with LRS
        //todo: hand LRS a List<LRSResult>(); and then it can build its little grid.
        //or rather.. LongRangeScan subsystem comes up with the numbers, then hands it to the Print

        //todo: refactor this with GetLRSFullData.  Pay attention to OutOfBounds
        public IEnumerable<IRSResult> GetIRSFullData(Location shipLocation, IGame game)
        {
            var scanData = new List<IRSResult>();

            //todo:  //bool currentlyInNebula = shipLocation.Coordinate.Type == SectorType.Nebulae;

            for (var sectorX = shipLocation.Coordinate.X - 1;
                sectorX <= shipLocation.Coordinate.X + 1;
                sectorX++)
            {
                for (var sectorY = shipLocation.Coordinate.Y - 1;
                    sectorY <= shipLocation.Coordinate.Y + 1;
                    sectorY++)
                {
                    var outOfBounds = Sector.OutOfBounds(shipLocation.Coordinate);

                    var currentResult = new IRSResult
                    {
                        Point = new Point(sectorX, sectorY)//todo: breaks here when regionX or regionY is 8
                    };

                    currentResult = this.GetSectorInfo(shipLocation.Sector, new Point(sectorX, sectorY), outOfBounds, game);
                    currentResult.MyLocation = shipLocation.Sector.X == sectorX &&
                                                shipLocation.Sector.Y == sectorY;

                    scanData.Add(currentResult);
                }
            }
            return scanData;
        }

        //todo: refactor this with Game.Map.OutOfBounds
        public static bool OutOfBounds(ICoordinate coordinate)
        {
            var inTheNegative = coordinate.X < 0 || coordinate.Y < 0;
            var maxxed = coordinate.X == DEFAULTS.COORDINATE_MAX || coordinate.Y == DEFAULTS.COORDINATE_MAX;

            var yInSector = coordinate.Y >= 0 && coordinate.Y < DEFAULTS.COORDINATE_MAX;
            var xInSector = coordinate.X >= 0 && coordinate.X < DEFAULTS.COORDINATE_MAX;

            return (inTheNegative || maxxed) && !(yInSector && xInSector);
        }

        /// <summary>
        /// Returns a list of "LRSResult" objects.  These objects show basic information of the area surrounding the passed location.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public IEnumerable<LRSResult> GetLRSFullData(Location location, IGame game)
        {
            bool currentlyInNebula = location.Sector.Type == SectorType.Nebulae;

            //todo: rewrite scanning method. this one is not very clear.

            //todo: if you create a coordinate that is not legal, then an exception is thrown.  this is bad.

            //1. get all regions next to location.Sector

            //-1 -1
            //-1 +1
            //-1 0
            //0 -1
            //0 0
            //0 +1
            //+1 0
            //+1 -1
            //+1 +1

            int locationX = location.Sector.X;
            int locationY = location.Sector.Y;
            List<Sector> thisSectorAndThoseNextToThisOne = new List<Sector>
            {
                // Row-major order: top-left to bottom-right, so current location is centered.
                this.Item(currentlyInNebula, game, locationX - 1, locationY - 1),
                this.Item(currentlyInNebula, game, locationX,     locationY - 1),
                this.Item(currentlyInNebula, game, locationX + 1, locationY - 1),

                this.Item(currentlyInNebula, game, locationX - 1, locationY),
                this.Item(currentlyInNebula, game, locationX,     locationY),
                this.Item(currentlyInNebula, game, locationX + 1, locationY),

                this.Item(currentlyInNebula, game, locationX - 1, locationY + 1),
                this.Item(currentlyInNebula, game, locationX,     locationY + 1),
                this.Item(currentlyInNebula, game, locationX + 1, locationY + 1)
            };

            //build map
            IEnumerable<LRSResult> scanData = thisSectorAndThoseNextToThisOne.Select(this.GetSectorData).ToList();

            //todo: set up galactic barrier space orr
            scanData.Single(r => r.Point.X == location.Sector.X && r.Point.Y == location.Sector.Y).MyLocation = true;

            return scanData;
        }

        private Sector Item(bool currentlyInNebula, IGame game, int locationX, int locationY)
        {
            Sector region;

            if (!currentlyInNebula)
            {
                region = game.Map.Sectors.SingleOrDefault(r => r.X == locationX && r.Y == locationY) ??
                             new Sector(new Point(locationX, locationY))
                             { 
                                 Map = game.Map,
                                 Empty = true,
                                 Type = SectorType.GalacticBarrier,
                                 Name = "Galactic Barrier",
                                 Scanned = true
                             };
            }
            else
            {
                region = new Sector(new Point(locationX, locationY))
                {
                    Map = game.Map,
                    Type = SectorType.Unknown
                };
            }

            return region;
        }

        public IRSResult GetSectorInfo(Sector currentSector, IPoint sector, bool outOfBounds, IGame game)
        {
            var currentResult = new IRSResult();

            if (!outOfBounds)
            {
                if (!currentSector.IsNebulae())
                {
                    currentResult = this.GetSectorData(currentSector, sector, game);
                }
                else
                {
                    //currentResult.SectorName = "Unknown";
                    currentResult.Unknown = true;
                    currentResult.Point = new Point(currentSector.X, currentSector.Y);
                }
            }
            else
            {
                currentResult.GalacticBarrier = true;
                //todo: set region to Galactic barrier?
            }

            return currentResult;
        }

        private LRSResult GetSectorData(Sector region)
        {
            //Sector regionToScan; // = Sectors.Get(game.Map, this.CoordinateToScan(region.X, region.Y, game.Config));
            var regionResult = new LRSResult();

            if (region.Type == SectorType.Unknown)
            {
                regionResult.Point = new Point(region.X, region.Y);
                regionResult.Name = region.Name;
                regionResult.Unknown = true;
                regionResult.Hostiles = null;
                regionResult.Starbases = null;
                regionResult.Stars = null;
            }
            else if (region.Type != SectorType.Nebulae)
            {
                regionResult = LongRangeScan.Execute(region);
            }
            else
            {
                regionResult.Point = new Point(region.X, region.Y);
                regionResult.Name = region.Name;
                regionResult.Unknown = true;
                regionResult.Hostiles = null;
                regionResult.Starbases = null;
                regionResult.Stars = null;
            }

            return regionResult;
        }

        private IRSResult GetSectorData(Sector currentSector, IPoint sector, IGame game)
        {
            Coordinate sectorToScan = this.Coordinates.GetNoError(sector);

            Point xx = sectorToScan ?? new Point(sector.X, sector.Y);

            ICoordinate coordinateToExamine = new Coordinate(new LocationDef(currentSector, xx));
            var locationToExamine = new Location(currentSector, coordinateToExamine);

            Location divinedLocationOnMap = currentSector.DivineCoordinateOnMap(locationToExamine, this.Map);

            if (divinedLocationOnMap.Sector.Type != SectorType.GalacticBarrier)
            {
                //int i;
            }
            else
            {
                
            }

            IRSResult sectorResult = ImmediateRangeScan.For(game.Map.Playership).Execute(divinedLocationOnMap);

            return sectorResult;
        }

        private Point CoordinateToScan(int regionX, int regionY, IStarTrekKGSettings config)
        {
            var max = config.GetSetting<int>("SECTOR_MAX") - 1;
            var min = config.GetSetting<int>("SECTOR_MIN");

            int divinedSectorX = regionX;
            int divinedSectorY = regionY;

            if (regionX - 1 < min)
            {
                divinedSectorX = min;
            }

            if (regionX > max)
            {
                divinedSectorX = max;
            }

            if (regionX + 1 > max)
            {
                divinedSectorX = max;
            }

            if (regionY - 1 < min)
            {
                divinedSectorY = min;
            }

            if (regionY > max)
            {
                divinedSectorY = max;
            }

            var SectorToScan = new Point(divinedSectorX, divinedSectorY);

            return SectorToScan;
        }

        /// <summary>
        /// Returns map Location info on the location Requested.
        /// If passed an area not in sector passed, example: {ThisCoordinate}.[-1, 0], then the proper location will be divined & returned.
        /// </summary>
        /// <param name="locationToExamine"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public Location DivineCoordinateOnMap(Location locationToExamine, IMap map)
        {
            var result = new DivinedCoordinateResult
            {
                SectorCoordinateToGet = locationToExamine.Sector,
                CoordinatePointToGet = new Point(locationToExamine.Coordinate.X, locationToExamine.Coordinate.Y),
                CurrentLocationX = locationToExamine.Coordinate.X,
                CurrentLocationY = locationToExamine.Coordinate.Y,
                Direction = "Here"
            };

            //todo: perhaps this code needs to be in the constructor -------------
            //todo: perhaps this code needs to be in the constructor -------------

            var locationToGet = new Location();
            var regionCoordinateToGet = new Point(locationToExamine.Sector.X, locationToExamine.Sector.Y);
            var sectorCoordinateToGet = new Point(locationToExamine.Coordinate.X, locationToExamine.Coordinate.Y);

            result = this.GetLeftEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);
            result = this.GetRightEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);
            result = this.GetTopEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);
            result = this.GetBottomEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);

            result = this.GetBottomRightEdgeResult(locationToExamine, result, regionCoordinateToGet, sectorCoordinateToGet);
            result = this.GetTopRightEdgeResult(locationToExamine, result, regionCoordinateToGet);
            result = this.GetTopLeftEdgeResult(locationToExamine, result, regionCoordinateToGet);
            result = this.GetBottomLeftEdgeResult(locationToExamine, result, regionCoordinateToGet);

            if (result.SectorCoordinateToGet == null)
            {
                throw new ArgumentException();
            }

            locationToGet.Sector = map.Sectors[new Point(result.SectorCoordinateToGet.X, result.SectorCoordinateToGet.Y)];

            if (locationToGet.Sector.Type != SectorType.GalacticBarrier)
            {
                //todo: moving to the 7th column in sector breaks things?
                locationToGet.Coordinate = locationToGet.Sector.Coordinates.Single(s => s.X == result.CurrentLocationX && s.Y == result.CurrentLocationY);
            }

            return locationToGet;
        }

        #region GetNeighbor Code

        private DivinedCoordinateResult GetBottomRightEdgeResult(Location locationToExamine, 
                                                              DivinedCoordinateResult result,
                                                              IPoint regionCoordinateToGet, 
                                                              IPoint sectorCoordinateToGet)
        {
            if (this.OffBottomRightEdge(locationToExamine))
            {
                result = result.Get("bottomRight",
                    regionCoordinateToGet.X += 1,
                    regionCoordinateToGet.Y += 1,
                    Coordinate.SIncrement(locationToExamine.Coordinate.X),
                    sectorCoordinateToGet.Y,   //todo:  This shows as 8, but current position needs to be fixed.  don't fix here.
                    0,
                    0);
            }
            return result;
        }

        private DivinedCoordinateResult GetBottomLeftEdgeResult(Location locationToExamine, 
                                                             DivinedCoordinateResult result,
                                                             IPoint regionCoordinateToGet)
        {
            if (this.OffBottomLeftEdge(locationToExamine))
            {
                result = result.Get("bottomLeft",
                    regionCoordinateToGet.X -= 1,
                    regionCoordinateToGet.Y, // -= 1
                    Coordinate.SIncrement(locationToExamine.Coordinate.X),
                    Coordinate.SDecrement(locationToExamine.Coordinate.Y),
                    7,
                    0);
            }
            return result;
        }

        private DivinedCoordinateResult GetTopLeftEdgeResult(Location locationToExamine, 
                                                          DivinedCoordinateResult result,
                                                          IPoint regionCoordinateToGet)
        {
            if (this.OffTopLeftEdge(locationToExamine))
            {
                result = result.Get("topLeft",
                    regionCoordinateToGet.X,
                    regionCoordinateToGet.Y, //-= 1
                    Coordinate.SDecrement(locationToExamine.Coordinate.X),
                    Coordinate.SDecrement(locationToExamine.Coordinate.Y),
                    7,
                    7);
            }
            return result;
        }

        private DivinedCoordinateResult GetTopRightEdgeResult(Location locationToExamine, 
                                                           DivinedCoordinateResult result,
                                                           IPoint regionCoordinateToGet)
        {
            if (this.OffTopRightEdge(locationToExamine))
            {
                result = result.Get("topRight",
                    regionCoordinateToGet.X,
                    regionCoordinateToGet.Y -= 1,
                    Coordinate.SDecrement(locationToExamine.Coordinate.X),
                    Coordinate.SIncrement(locationToExamine.Coordinate.Y),
                    0,
                    7);
            }
            return result;
        }

        private DivinedCoordinateResult GetBottomEdgeResult(Location locationToExamine, 
                                                         DivinedCoordinateResult result,
                                                         IPoint regionCoordinateToGet, 
                                                         IPoint sectorCoordinateToGet)
        {
            if (this.OffBottomEdge(locationToExamine))
            {
                result = result.Get("bottom",
                    regionCoordinateToGet.X,
                    regionCoordinateToGet.Y += 1,
                    Coordinate.SIncrement(locationToExamine.Coordinate.X),
                    sectorCoordinateToGet.Y,
                    locationToExamine.Coordinate.Y,
                    0);
            }
            return result;
        }

        private DivinedCoordinateResult GetTopEdgeResult(Location locationToExamine, 
                                                      DivinedCoordinateResult result,
                                                      IPoint regionCoordinateToGet, 
                                                      IPoint sectorCoordinateToGet)
        {
            if (this.OffTopEdge(locationToExamine))
            {
                result = result.Get("top",
                    regionCoordinateToGet.X,
                    regionCoordinateToGet.Y -= 1,
                    Coordinate.SDecrement(locationToExamine.Coordinate.X),
                    sectorCoordinateToGet.Y,
                    locationToExamine.Coordinate.Y,
                    7);
            }
            return result;
        }

        private DivinedCoordinateResult GetRightEdgeResult(Location locationToExamine, 
                                                        DivinedCoordinateResult result,
                                                        IPoint regionCoordinateToGet, 
                                                        IPoint sectorCoordinateToGet)
        {
            if (this.OffRightEdge(locationToExamine))
            {
                result = result.Get("right",
                    regionCoordinateToGet.X += 1,
                    regionCoordinateToGet.Y,
                    sectorCoordinateToGet.X,
                    Coordinate.SIncrement(locationToExamine.Coordinate.Y),
                    0,
                    locationToExamine.Coordinate.X);
            }
            return result;
        }

        private DivinedCoordinateResult GetLeftEdgeResult(Location locationToExamine, 
                                                       DivinedCoordinateResult result,
                                                       IPoint regionCoordinateToGet, 
                                                       IPoint sectorCoordinateToGet)
        {
            if (this.OffLeftEdge(locationToExamine))
            {
                result = result.Get("left",
                    regionCoordinateToGet.X -= 1,
                    regionCoordinateToGet.Y,
                    sectorCoordinateToGet.X,
                    Coordinate.SDecrement(locationToExamine.Coordinate.Y),
                    7,
                    locationToExamine.Coordinate.X);
            }
            return result;
        }

        private bool OffBottomRightEdge(Location locationToExamine)
        {
            return locationToExamine.Coordinate.X == 8 && locationToExamine.Coordinate.Y == 8;
        }

        private bool OffBottomLeftEdge(Location locationToExamine)
        {
            return locationToExamine.Coordinate.X == 8 && locationToExamine.Coordinate.Y == -1;
        }

        private bool OffTopLeftEdge(Location locationToExamine)
        {
            return locationToExamine.Coordinate.X == -1 && locationToExamine.Coordinate.Y == -1;
        }

        private bool OffTopRightEdge(Location locationToExamine)
        {
            return locationToExamine.Coordinate.X == -1 && locationToExamine.Coordinate.Y == 8;
        }

        private bool OffBottomEdge(Location locationToExamine)
        {
            return locationToExamine.Coordinate.X == 8 && locationToExamine.Coordinate.Y < 8;
        }

        private bool OffTopEdge(Location locationToExamine)
        {
            return locationToExamine.Coordinate.X == -1 && locationToExamine.Coordinate.Y < 8;
        }

        private bool OffRightEdge(Location locationToExamine)
        {
            return locationToExamine.Coordinate.X < 8 && locationToExamine.Coordinate.Y == 8;
        }

        private bool OffLeftEdge(Location locationToExamine)
        {
            return locationToExamine.Coordinate.X < 8 && locationToExamine.Coordinate.Y == -1;
        }

        #endregion

        public Point GetPoint()
        {
            return new Point(this.X, this.Y);
        }

        public new Sector ToSector()
        {
            return new Sector(new Point(this.X, this.Y));
        }

        public bool Invalid()
        {
            bool invalid = this.X < DEFAULTS.SECTOR_MIN ||
                           this.X >= DEFAULTS.SECTOR_MAX ||
                           this.Y < DEFAULTS.SECTOR_MIN ||
                           this.Y >= DEFAULTS.SECTOR_MAX;

            return invalid;
        }
    }
}


////todo: use 1 set of hostiles.  Create in sectors, count up for Sectors
// public static void CreateHostileX(Sector Sector, int x, int y, IList<string> listOfBaddies)
//{
//    //todo: this should be a random baddie, from the list of baddies in app.config
//    //todo: note, in leter versions, baddies and allies can fight each other automatically (when they move to within range of each other.  status of the battles can be kept in the ships log (if observed by a friendly)
//
//          var index = (Utility.Random).Next(listOfBaddies.Count);
//        var hostileShip = new Ship(listOfBaddies[index], Sector.Map, x, y);
//      hostileShip.Coordinate = new Coordinate(x, y);

//    Shields.For(hostileShip).Energy = 300 + (Utility.Random).Next(200);

//  Sector.Hostiles.Add(hostileShip);

//listOfBaddies.RemoveAt(index); //remove name from our big list of names so we dont select it again
//}

//public Coordinate GetItem(int qX, int qY)
//{
//    return this.Coordinates.Where(s => s.X == qX && s.Y == qY).Single();
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

//        var Sector = Sectors.Get(map, x, y);

//        if (!Sector.Starbase)
//        {
//            Sector.Starbase = true;
//            starbases--;
//        }

//        if (Sector.Hostiles.Count < 3) //todo: put 3 in app.config
//        {
//            Sector.CreateHostile(Sector, x, y, Map.baddieNames);
//            hostiles--;
//        }
//    }
//}

//////todo: use 1 set of hostiles.  Create in sectors, count up for Sectors
//private static void AddHostile(Sector Sector, int x, int y)
//{
//    //Sector.CreateHostile(Sector, x, y, Map.baddieNames);

//    ////todo: a hostile was made, but he is not in any Sector yet..

//    //List<Ship> allbadGuysInSector = Sector.Hostiles.Where(s => s.Allegiance == Allegiance.BadGuy).ToList();

//    //var k = allbadGuysInSector[0];

//    ////fixme

//    ////all hostiles have the same XY.  this is rightfully failing

//    //var badGuy = allbadGuysInSector.Where(s =>  
//    //                            s.Coordinate.X == x && 
//    //                            s.Coordinate.Y == y).Single();

//    ////this needs to be linked
//    ////add again?
//    //Sector.Hostiles.Add(badGuy);
//}

//Output as enum??
