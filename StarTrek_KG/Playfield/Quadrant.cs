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
    /// A Quadrant in this game is a named area of space. It can contain ships, starbases, or stars. 
    /// </summary>
    public class Quadrant : Coordinate, IQuadrant
    {
        #region Properties

        public QuadrantType Type { get; set; }
        public string Name { get; set; }

        public IMap Map { get; set; }

        //TODO: This property needs to be changed to a function, and that function needs to count Hostiles in this quadrant when called

        //for this to work, each sector needs to be able to store a hostile
        //public List<Ship> Hostiles { get; set; } //TODO: this needs to be changed to a List<ship> that have a hostile property=true

        public Sectors Sectors { get; set; }
        public bool Scanned { get; set; }
        public bool Empty { get; set; }

        public bool Active { get; set; }

        #endregion

        public Quadrant(ICoordinate coordinate)
        {
            this.X = coordinate.X;
            this.Y = coordinate.Y;
        }

        public Quadrant(IMap map, bool isNebulae = false)
        {
            this.Type = isNebulae ? QuadrantType.Nebulae : QuadrantType.GalacticSpace;

            this.Empty = true;
            this.Name = String.Empty;
            this.Map = map;
        }

        public Quadrant(IMap map, Stack<string> baddieNames, FactionName stockBaddieFaction, bool isNebulae = false)
        {
            this.Empty = true;
            this.Map = map;
            this.Name = String.Empty;

            this.Create(baddieNames, stockBaddieFaction, isNebulae);
        }

        //todo: we might want to avoid passing in baddie names and set up baddies later..
        public Quadrant(IMap map, Stack<string> quadrantNames, Stack<string> baddieNames, FactionName stockBaddieFaction,
            out int nameIndex, bool addStars = false, bool makeNebulae = false)
        {
            this.Type = QuadrantType.GalacticSpace;
            this.Empty = true;

            this.Create(map, quadrantNames, baddieNames, stockBaddieFaction, out nameIndex, addStars, makeNebulae);
        }

        public void SetActive()
        {
            this.Map.Quadrants.ClearActive();

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

        public void Create(IMap map, Stack<string> quadrantNames, Stack<string> baddieNames,
            FactionName stockBaddieFaction, out int nameIndex, bool addStars = true, bool makeNebulae = false)
        {
            nameIndex = (Utility.Utility.Random).Next(baddieNames.Count);

            this.Map = map;
            this.InitializeSectors(this, new List<Sector>(), baddieNames, stockBaddieFaction, addStars, makeNebulae);

            if (quadrantNames != null)
            {
                this.Name = quadrantNames.Pop();
            }
        }

        public void Create(Stack<string> quadrantNames, Stack<String> baddieNames, FactionName stockBaddieFaction,
            Coordinate quadrantXY, out int nameIndex, IEnumerable<Sector> itemsToPopulate, bool addStars = true,
            bool isNebulae = false)
        {
            nameIndex = (Utility.Utility.Random).Next(quadrantNames.Count);

            this.Name = quadrantNames.Pop();

            this.X = quadrantXY.X;
            this.Y = quadrantXY.Y;

            var itemsInQuadrant = new List<Sector>();

            if (itemsToPopulate != null)
            {
                itemsInQuadrant =
                    itemsToPopulate.Where(i => i.QuadrantDef.X == this.X && i.QuadrantDef.Y == this.Y).ToList();
            }

            this.InitializeSectors(this, itemsInQuadrant, baddieNames, stockBaddieFaction, addStars, isNebulae);
        }

        public void InitializeSectors(Quadrant quadrant,
            List<Sector> itemsToPopulate,
            Stack<string> baddieNames,
            FactionName stockBaddieFaction,
            bool addStars,
            bool makeNebulae = false)
        {
            quadrant.Sectors = new Sectors(); //todo: pull from app.config. initialize with limit

            //This loop creates empty sectors and populates as needed.
            for (var x = 0; x < Constants.SECTOR_MAX; x++) //todo: pull from app.config. initialize with limit
            {
                for (var y = 0; y < Constants.SECTOR_MAX; y++)
                {
                    this.PopulateMatchingItem(quadrant, itemsToPopulate, x, y, baddieNames, stockBaddieFaction,
                        makeNebulae);
                }
            }

            if (addStars)
            {
                //Randomly throw stars in
                this.AddStars(quadrant, (Utility.Utility.Random).Next(Constants.SECTOR_MAX));
            }

            if (makeNebulae)
            {
                quadrant.TransformIntoNebulae();
            }

            ////This is possible only in the test harness, as app code currently does not call this function with a null
            ////This makes the code more error tolerant.
            //if (itemsToPopulate == null)
            //{
            //    itemsToPopulate = new List<Sector>();
            //}

            //itemsToPopulate.AddRange(starsAdded);

            //todo: make this a test
            //    if (itemsToPopulate.Count != (queryOfItems in quadrant.Sectors)
            //    {
            //        //error.. error.. danger will robinson
            //        //actually, this check should go in a unit test.  dont need to do it here.
            //    }
        }

        public IEnumerable<Sector> AddStars(Quadrant quadrant, int totalStarsInQuadrant)
        {
            Utility.Utility.ResetGreekLetterStack();

            this.CreateStars(quadrant, totalStarsInQuadrant);

            return quadrant.Sectors.Where(s => s.Item == SectorItem.Star);
        }

        public Sector AddStar(Quadrant quadrant)
        {
            Utility.Utility.ResetGreekLetterStack();

            const int totalStarsInQuadrant = 1;

            var currentStarName = this.CreateStars(quadrant, totalStarsInQuadrant);

            return quadrant.Sectors.Single(s => s.Item == SectorItem.Star && ((Star) s.Object).Name == currentStarName);
        }

        public string CreateStars(Quadrant quadrant, int totalStarsInQuadrant,
            SectorType starSectorType = SectorType.StarSystem)
        {
            string currentStarName = "";

            while (totalStarsInQuadrant > 0)
            {
                var x = (Utility.Utility.Random).Next(Constants.SECTOR_MAX);
                var y = (Utility.Utility.Random).Next(Constants.SECTOR_MAX);

                //todo: just pass in coordinate and get its item
                var sector = quadrant.Sectors.Single(s => s.X == x && s.Y == y);
                var sectorEmpty = sector.Item == SectorItem.Empty;

                if (sectorEmpty)
                {
                    if (totalStarsInQuadrant > 0)
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

                            var starsInQuadrant =
                                quadrant.Sectors.Where(s => s.Object != null && s.Object.Type.Name == "Star").ToList();
                            var allStarsDontHaveNewDesignation =
                                starsInQuadrant.All(s => ((Star) s.Object).Designation != newNameLetter);

                            if (allStarsDontHaveNewDesignation)
                            {
                                foundStarName = true;

                                if (quadrant.Name == null) //todo: why do we have null quadrant names???
                                {
                                    quadrant.Name = "UNKNOWN QUADRANT " + newNameLetter + " " + counter;
                                        //todo: this could get dupes
                                }

                                currentStarName = quadrant.Name.ToUpper() + " " + newNameLetter;

                                newStar.Name = currentStarName;
                                newStar.Designation = newNameLetter;
                                sector.Item = SectorItem.Star;
                                sector.Type = starSectorType;

                                sector.Object = newStar;
                                totalStarsInQuadrant--;
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

        public void PopulateMatchingItem(Quadrant quadrant, ICollection<Sector> itemsToPopulate, int x, int y,
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
                            if ((quadrant.Type == QuadrantType.Nebulae) &&
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

            this.AddSector(quadrant, x, y, sectorItemToPopulate, baddieNames, stockBaddieFaction);
        }

        public void AddSector(Quadrant quadrant, int x, int y, SectorItem itemToPopulate, Stack<string> stockBaddieNames,
            FactionName stockBaddieFaction)
        {
            var newlyCreatedSector = Sector.CreateEmpty(quadrant, new Coordinate(x, y));
            this.Map.Write.DebugLine("Added new Empty Sector to Quadrant: " + quadrant.Name + " Coordinate: " +
                                     newlyCreatedSector);

            if (itemToPopulate == SectorItem.HostileShip)
            {
                //if a baddie name is passed, then use it.  otherwise
                var newShip = this.CreateHostileShip(newlyCreatedSector, stockBaddieNames, stockBaddieFaction);
                quadrant.AddShip(newShip, newlyCreatedSector);
            }
            else
            {
                newlyCreatedSector.Item = itemToPopulate;
            }

            quadrant.Sectors.Add(newlyCreatedSector);
        }

        public void AddShip(IShip ship, ISector toSector)
        {
            if (toSector == null)
            {
                this.Map.Write.DebugLine("No Sector passed. cannot add to Quadrant: " + this.Name);
                throw new GameException("No Sector passed. cannot add to Quadrant: " + this.Name);
            }

            if (ship == null)
            {
                this.Map.Write.DebugLine("No ship passed. cannot add to Quadrant: " + this.Name);
                throw new GameException("No ship passed. cannot add to Quadrant: " + this.Name);
            }

            this.Map.Write.DebugLine("Adding Ship: " + ship.Name + " to Quadrant: " + this.Name + " Sector: " + toSector);

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

        public void AddEmptySector(Quadrant quadrant, int x, int y)
        {
            var sector = Sector.CreateEmpty(quadrant, new Coordinate(x, y));

            quadrant.Sectors.Add(sector);
        }

        public bool NoHostiles(List<Ship> hostiles)
        {
            if (hostiles.Count == 0)
            {
                this.Map.Write.Line(this.Map.Config.GetSetting<string>("QuadrantsNoHostileShips"));
                return true;
            }
            return false;
        }

        /// <summary>
        /// goes through each sector in this quadrant and counts hostiles
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
                    throw new GameException(this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInQuadrant") +
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
        /// goes through each sector in this quadrant and clears hostiles
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
                throw new GameException(this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInQuadrant") + this.Name +
                                        ".");
            }
        }

        /// <summary>
        /// goes through each sector in this quadrant and clears item requested
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
                throw new GameException(this.Map.Config.GetSetting<string>("DebugNoSetUpSectorsInQuadrant") + this.Name +
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
            return (this.Type == QuadrantType.Nebulae);
        }

        //todo: refactor these functions with LRS
        //todo: hand LRS a List<LRSResult>(); and then it can build its little grid.
        //or rather.. LongRangeScan subsystem comes up with the numbers, then hands it to the Print

        public LRSData GetLRSData(Location location, Game game)
        {
            var scanData = new LRSData();

            bool currentlyInNebula = location.Quadrant.Type == QuadrantType.Nebulae;

            for (var quadrantY = location.Quadrant.Y - 1;
                quadrantY <= location.Quadrant.Y + 1;
                quadrantY++)
            {
                for (var quadrantX = location.Quadrant.X - 1;
                    quadrantX <= location.Quadrant.X + 1;
                    quadrantX++)
                {
                    var outOfBounds = game.Map.OutOfBounds(quadrantY, quadrantX);

                    var currentResult = new LRSResult();
                    if (!currentlyInNebula)
                    {
                        currentResult = this.GetQuadrantInfo(quadrantY, outOfBounds, quadrantX, game);
                        currentResult.MyLocation = location.Quadrant.X == quadrantX &&
                                                   location.Quadrant.Y == quadrantY;
                    }
                    else
                    {
                        //We are in a nebula.  LRS won't work here.
                        currentResult.Unknown = true;
                    }
                    scanData.Add(currentResult);
                }
            }
            return scanData;
        }

        private LRSResult GetQuadrantInfo(int quadrantY, bool outOfBounds, int quadrantX, Game game)
        {
            var currentResult = new LRSResult();

            if (!outOfBounds)
            {
                currentResult = GetQuadrantData(quadrantY, quadrantX, game);
            }
            else
            {
                currentResult.GalacticBarrier = true;
            }

            return currentResult;
        }

        private LRSResult GetQuadrantData(int quadrantY, int quadrantX, Game game)
        {
            Quadrant quadrantToScan = Quadrants.Get(game.Map, this.CoordinateToScan(quadrantY, quadrantX, game.Config));
            var quadrantResult = new LRSResult();

            if (quadrantToScan.Type != QuadrantType.Nebulae)
            {
                quadrantResult = LongRangeScan.For(game.Map.Playership).Execute(quadrantToScan);
            }
            else
            {
                quadrantResult.Unknown = true;
            }

            return quadrantResult;
        }

        private Coordinate CoordinateToScan(int quadrantY, int quadrantX, IStarTrekKGSettings config)
        {
            var max = config.GetSetting<int>("QuadrantMax") - 1;
            var min = config.GetSetting<int>("QUADRANT_MIN");

            int divinedQuadX = quadrantX;
            int divinedQuadY = quadrantY;

            if (quadrantX - 1 < min)
            {
                divinedQuadX = min;
            }

            if ((quadrantX > max))
            {
                divinedQuadX = max;
            }

            if (quadrantX + 1 > max)
            {
                divinedQuadX = max;
            }

            if (quadrantY - 1 < min)
            {
                divinedQuadY = min;
            }

            if ((quadrantY > max))
            {
                divinedQuadY = max;
            }

            var quadrantToScan = new Coordinate(divinedQuadX, divinedQuadY);

            return quadrantToScan;
        }

        internal Location GetNeighbor(int sectorT, int sectorL, IMap map)
        {
            var locationToGet = new Location();
            var coordinateToGet = new Coordinate(this.X, this.Y);
            var direction = "";
            int x = -2;
            int y = -2;

            if (sectorT < 8 && sectorL == -1)
            {
                direction = "left";
                x = 7;
                y = sectorT;
                coordinateToGet.X -= 1;
            }

            if (sectorT == -1 && sectorL == -1)
            {
                direction = "topLeft";
                x = 7;
                y = 7;
                coordinateToGet.Y -= 1;
                coordinateToGet.X -= 1;
            }

            if (sectorT == -1 && sectorL < 8)
            {
                direction = "top";
                x = sectorL;
                y = 7;
                coordinateToGet.Y -= 1;
            }

            if (sectorT == -1 && sectorL == 8)
            {
                direction = "topRight";
                x = 0;
                y = 7;
                coordinateToGet.Y -= 1;
                coordinateToGet.X += 1;
            }

            if (sectorT < 8 && sectorL == 8)
            {
                direction = "right";
                x = 0;
                y = 0;
                coordinateToGet.X += 1;
            }

            //---------------
            if (sectorT == 8 && sectorL < 8)
            {
                direction = "bottom";
                x = sectorL;
                y = 0;
                coordinateToGet.Y = Sector.Increment(coordinateToGet.Y);
            }

            if (sectorT == 8 && sectorL == -1)
            {
                direction = "bottomLeft";
                x = 7;
                y = 0;
                coordinateToGet.Y = Sector.Decrement(coordinateToGet.Y);
                coordinateToGet.X += 1;
            }

            //-----------------

            if (sectorT == 8 && sectorL == 8)
            {
                direction = "bottomRight";
                x = 0;
                y = 0;
                coordinateToGet.Y += 1;
                coordinateToGet.X += 1;
            }

            map.Write.SingleLine("Quadrant to the " + direction);
            var gotQuadrant = Quadrants.Get(map, coordinateToGet);

            locationToGet.Quadrant = gotQuadrant;
            locationToGet.Sector = gotQuadrant.Sectors.Single(s => s.X == x && s.Y == y);

            return locationToGet;
        }
    }
}


////todo: use 1 set of hostiles.  Create in sectors, count up for quadrants
// public static void CreateHostileX(Quadrant quadrant, int x, int y, IList<string> listOfBaddies)
//{
//    //todo: this should be a random baddie, from the list of baddies in app.config
//    //todo: note, in leter versions, baddies and allies can fight each other automatically (when they move to within range of each other.  status of the battles can be kept in the ships log (if observed by a friendly)
//
//          var index = (Utility.Random).Next(listOfBaddies.Count);
//        var hostileShip = new Ship(listOfBaddies[index], quadrant.Map, x, y);
//      hostileShip.Sector = new Sector(x, y);

//    Shields.For(hostileShip).Energy = 300 + (Utility.Random).Next(200);

//  quadrant.Hostiles.Add(hostileShip);

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

//        var quadrant = Quadrants.Get(map, x, y);

//        if (!quadrant.Starbase)
//        {
//            quadrant.Starbase = true;
//            starbases--;
//        }

//        if (quadrant.Hostiles.Count < 3) //todo: put 3 in app.config
//        {
//            Quadrant.CreateHostile(quadrant, x, y, Map.baddieNames);
//            hostiles--;
//        }
//    }
//}

//////todo: use 1 set of hostiles.  Create in sectors, count up for quadrants
//private static void AddHostile(Quadrant quadrant, int x, int y)
//{
//    //Quadrant.CreateHostile(quadrant, x, y, Map.baddieNames);

//    ////todo: a hostile was made, but he is not in any QUADRANT yet..

//    //List<Ship> allbadGuysInQuadrant = quadrant.Hostiles.Where(s => s.Allegiance == Allegiance.BadGuy).ToList();

//    //var k = allbadGuysInQuadrant[0];

//    ////fixme

//    ////all hostiles have the same XY.  this is rightfully failing

//    //var badGuy = allbadGuysInQuadrant.Where(s =>  
//    //                            s.Sector.X == x && 
//    //                            s.Sector.Y == y).Single();

//    ////this needs to be linked
//    ////add again?
//    //quadrant.Hostiles.Add(badGuy);
//}

//Output as enum??