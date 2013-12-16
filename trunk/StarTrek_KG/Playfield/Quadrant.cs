using System;
using System.Linq;
using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG.Playfield
{
    /// <summary>
    /// A Quadrant in this game is a named area of space. It can contain ships, starbases, or stars. 
    /// </summary>
    public class Quadrant: Coordinate
    {
        #region Properties
            public string Name { get; set; }

            //TODO: This property needs to be changed to a function, and that function needs to count Hostiles in this quadrant when called

            //for this to work, each sector needs to be able to store a hostile
            //public List<Ship> Hostiles { get; set; } //TODO: this needs to be changed to a List<ship> that have a hostile property=true

            public Sectors Sectors { get; set; }
            public bool Scanned { get; set; }
            public bool Empty { get; set; }
            public Map Map { get; set; }

            private bool _active;
            public bool Active
            {
                get
                {
                    return _active;
                }
                set
                {
                    if (value)
                    {
                        this.Map.Quadrants.ClearActive();
                    }

                    _active = value;
                }
            }

        #endregion

        public Quadrant()
        {
            this.Empty = true;
            this.Name = String.Empty;
        }

        public Quadrant(Map map, Stack<string> names)
        {
            this.Empty = true;
            this.Map = map;
            this.Create(names);
            this.Name = String.Empty;
        }

        public Quadrant(Map map, Stack<string> names, out int nameIndex)
        {
            this.Empty = true;
            this.Map = map;
            this.Create(names, out nameIndex);
            this.Name = String.Empty;
        }

        public void Create(Stack<string> baddieNames, bool addStars = true)
        {
            Quadrant.InitializeSectors(this, new List<Sector>(), baddieNames, this.Map, addStars);
        }

        public Quadrant Create(Stack<string> names, out int nameIndex, bool addStars = true)
        {
            nameIndex = (Utility.Utility.Random).Next(names.Count);

            var newQuadrant = new Quadrant(this.Map, names);
            newQuadrant.Name = names.Pop();

            //fix: error here:
            //fix: sectors are not being created for all quadrants
            Quadrant.InitializeSectors(newQuadrant, new List<Sector>(), names, this.Map, addStars);

            return newQuadrant;
        }

        public Quadrant Create(Map map, Stack<string> quadrantNames, Stack<String> baddieNames, Coordinate quadrantXY, out int nameIndex, IEnumerable<Sector> itemsToPopulate, bool addStars = true)
        {
            nameIndex = (Utility.Utility.Random).Next(quadrantNames.Count);

            this.Map = map;
            this.Name = quadrantNames.Pop();

            this.X = quadrantXY.X;
            this.Y = quadrantXY.Y;

            var itemsInQuadrant = new List<Sector>();
                
            if (itemsToPopulate != null)
            {
                itemsInQuadrant = itemsToPopulate.Where(i => i.QuadrantDef.X == this.X && i.QuadrantDef.Y == this.Y).ToList();
            }

            Quadrant.InitializeSectors(this, itemsInQuadrant, baddieNames, map, addStars);

            return this;
        }

        public static void InitializeSectors(Quadrant quadrant, List<Sector> itemsToPopulate, Stack<string> baddieNames, Map map, bool addStars)
        {
            quadrant.Sectors = new Sectors(); //todo: pull from app.config. initialize with limit

            //This loop creates empty sectors and populates as needed.
            for (var x = 0; x < Constants.SECTOR_MAX; x++)  //todo: pull from app.config. initialize with limit
            {
                for (var y = 0; y < Constants.SECTOR_MAX; y++)
                {
                    Quadrant.PopulateMatchingItem(quadrant, itemsToPopulate, x, y, baddieNames, map);
                }
            }

            IEnumerable<Sector> starsToAdd = new List<Sector>();

            if(addStars)
            {
                //Randomly throw stars in
                starsToAdd = Map.AddStars(quadrant, (Utility.Utility.Random).Next(Constants.SECTOR_MAX)); 
            }

            //This is possible only in the test harness, as app code currently does not call this function with a null
            //This makes the code more error tolerant.
            if (itemsToPopulate == null)
            {
                itemsToPopulate = new List<Sector>();
            }

            itemsToPopulate.AddRange(starsToAdd);

            //todo: make this a test
            //    if (itemsToPopulate.Count != (queryOfItems in quadrant.Sectors)
            //    {
            //        //error.. error.. danger will robinson
            //        //actually, this check should go in a unit test.  dont need to do it here.
            //    }
        }

        private static void PopulateMatchingItem(Quadrant quadrant, ICollection<Sector> itemsToPopulate, int x, int y, Stack<string> baddieNames, Map map)
        {

            if (quadrant.X == 1 && quadrant.Y == 1)
            {
                var i = 0;
            }

            SectorItem sectorItemToPopulate = SectorItem.Empty;

            try
            {
                if (itemsToPopulate != null)
                {
                    if (itemsToPopulate.Count > 0)
                    {
                        Sector sectorToPopulate = itemsToPopulate.Where(i => i.X == x && i.Y == y).SingleOrDefault();
                        sectorItemToPopulate = sectorToPopulate == null ? SectorItem.Empty : sectorToPopulate.Item;

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

            Quadrant.AddSector(quadrant, x, y, sectorItemToPopulate, baddieNames, map);
        }

        public static void AddSector(Quadrant quadrant, int x, int y, SectorItem itemToPopulate, Stack<string> baddieNames, Map map)
        {
            var newlyCreatedSector = Sector.CreateEmpty(quadrant, new Coordinate(x, y));
            Output.Write.DebugLine("Added new Empty Sector to Quadrant: " + quadrant.Name + " Coordinate: " + newlyCreatedSector);

            if(itemToPopulate == SectorItem.Hostile)
            {
                //if a baddie name is passed, then use it.  otherwise
                var newShip = Quadrant.CreateHostileShip(newlyCreatedSector, baddieNames, map);
                quadrant.AddShip(newShip, newlyCreatedSector);
            }
            else
            {
                newlyCreatedSector.Item = itemToPopulate;
            }

            quadrant.Sectors.Add(newlyCreatedSector);
        }

        public void AddShip(IShip ship, Sector toSector)
        {
            if (toSector == null)
            {
                Output.Write.DebugLine("No Sector passed. cannot add to Quadrant: " + this.Name);
                throw new GameException("No Sector passed. cannot add to Quadrant: " + this.Name);
            }

            if (ship == null)
            {
                Output.Write.DebugLine("No ship passed. cannot add to Quadrant: " + this.Name);
                throw new GameException("No ship passed. cannot add to Quadrant: " + this.Name);
            }

            Output.Write.DebugLine("Adding Ship: " + ship.Name + " to Quadrant: " + this.Name + " Sector: " + toSector);

            var addToSector = this.GetSector(toSector) ?? toSector; //if we can't retrieve it, then it hasn't been created yet, so add to our new variable and the caller of this function can add it if they want

            try
            {
                addToSector.Object = ship;

                switch(ship.Allegiance)
                {
                    case Allegiance.GoodGuy:
                        addToSector.Item = SectorItem.Friendly;
                        break;

                    case Allegiance.BadGuy:
                        addToSector.Item = SectorItem.Hostile;
                        break;
                }        
            }
            catch(Exception ex)
            {
                Output.Write.DebugLine("unable to add ship to sector " + toSector + ". " + ex.Message);
                throw new GameException("unable to add ship to sector " + toSector + ". " + ex.Message);
            }
        }

        public void RemoveShip(IShip ship)
        {
            //staple ship to sector passed.
            Sector sectorToAdd = this.Sectors.Where(s => s.X == ship.Sector.X && s.Y == ship.Sector.Y).Single();
            sectorToAdd.Object = ship;
        }

        private static Ship CreateHostileShip(Sector position, Stack<string> listOfBaddies, Map map)
        {
            //todo: this should be a random baddie, from the list of baddies in app.config
            var hostileShip = new Ship(listOfBaddies.Pop(), map, position); //yes.  This code can be misused.  There will be repeats of ship names if the stack isn't managed properly
            hostileShip.Sector.X = position.X; 
            hostileShip.Sector.Y = position.Y;

            Shields.For(hostileShip).Energy = 300 + (Utility.Utility.Random).Next(200);

            Output.Write.DebugLine("Created Ship: " + hostileShip.Name);

            return hostileShip;
        }

        public static void AddEmptySector(Quadrant quadrant, int x, int y)
        {
            var sector = Sector.CreateEmpty(quadrant, new Coordinate(x, y));

            quadrant.Sectors.Add(sector);
        }


        //Loop for each Hostile and starbase.  Each go around pops a hostile
        //(up to 3) into a random sector.  Same thing with Starbase, but the limit
        //of starbases is 1.
        //public static void Populate(Map map)
        //{
        //    var starbases = map.starbases;  //todo: once starbases are a list, then  
        //    var hostiles = map.hostilesToSetUp;

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

        public static bool NoHostiles(List<Ship> hostiles)
        {
            if (hostiles.Count == 0)
            {
                Output.Write.Line(StarTrekKGSettings.GetSetting<string>("QuadrantsNoHostileShips"));
                return true;
            }
            return false;
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
                    throw new GameException(StarTrekKGSettings.GetSetting<string>("DebugNoSetUpSectorsInQuadrant") + this.Name);
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
                throw new GameException(StarTrekKGSettings.GetSetting<string>("DebugNoSetUpSectorsInQuadrant") + this.Name + ".");
            }
        }

        internal int GetStarbaseCount()
        {
            return Sectors.Count(sector => sector.Item == SectorItem.Starbase);
        }

        public int GetStarCount()
        {
            return Sectors.Count(sector => sector.Item == SectorItem.Star);
        }

        public Sector GetSector(Coordinate coordinate)
        {
            return this.Sectors.FirstOrDefault(sector => sector.X == coordinate.X && sector.Y == coordinate.Y);
        }
    }
}
