using System;
using System.Linq;
using System.Collections.Generic;
using StarTrek_KG.Enums;
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
            public List<Ship> Hostiles { get; set; } //TODO: this needs to be changed to a List<ship> that have a hostile property=true

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
            this.Hostiles = new List<Ship>();
            this.Name = String.Empty;
        }

        public Quadrant(Map map, Stack<string> names)
        {
            this.Empty = true;
            this.Hostiles = new List<Ship>();
            this.Map = map;
            this.Create(names);
            this.Name = String.Empty;
        }

        public Quadrant(Map map, Stack<string> names, out int nameIndex)
        {
            this.Empty = true;
            this.Hostiles = new List<Ship>();
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
            nameIndex = (Utility.Random).Next(names.Count);

            var newQuadrant = new Quadrant(this.Map, names);
            newQuadrant.Name = names.Pop();

            //fix: error here:
            var itemsInQuadrant = new List<Sector>();

            //fix: sectors are not being created for all quadrants
            Quadrant.InitializeSectors(newQuadrant, new List<Sector>(), names, this.Map, addStars);

            return newQuadrant;
        }

        public Quadrant Create(Map map, Stack<string> quadrantNames, Stack<String> baddieNames, Coordinate quadrantXY, out int nameIndex, IEnumerable<Sector> itemsToPopulate, bool addStars = true)
        {
            nameIndex = (Utility.Random).Next(quadrantNames.Count);

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
                starsToAdd = Map.AddStars(quadrant, (new Random(Convert.ToInt32(DateTime.Today.Millisecond + DateTime.Today.Minute))).Next(Constants.SECTOR_MAX)); 
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
            var sector = Sector.CreateEmpty(quadrant, new Coordinate(x, y));

            if(itemToPopulate == SectorItem.Hostile)
            {
                //if a baddie name is passed, then use it.  otherwise
                quadrant.Hostiles.Add(Quadrant.CreateHostileShip(sector, baddieNames, map));
            }

            sector.Item = itemToPopulate;

            quadrant.Sectors.Add(sector);
        }

        public static void AddEmptySector(Quadrant quadrant, int x, int y)
        {
            var sector = Sector.CreateEmpty(quadrant, new Coordinate(x, y));

            quadrant.Sectors.Add(sector);
        }

        private static Ship CreateHostileShip(Sector position, Stack<string> listOfBaddies, Map map)
        {
            //todo: this should be a random baddie, from the list of baddies in app.config
            //todo: note, in leter versions, baddies and allies can fight each other automatically (when they move to within range of each other.  status of the battles can be kept in the ships log (if observed by a friendly)

            var hostileShip = new Ship(listOfBaddies.Pop(), map, position);
            hostileShip.Sector.X = position.X; 
            hostileShip.Sector.Y = position.Y;

            Shields.For(hostileShip).Energy = 300 + (Utility.Random).Next(200);

            return hostileShip;
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
                Output.WriteLine("There are no Hostile ships in this quadrant.");
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
        public int GetHostileCount()
        {
            return Sectors.Count(sector => sector.Item == SectorItem.Hostile);
        }

        /// <summary>
        /// goes through each sector in this quadrant and counts hostiles
        /// </summary>
        /// <returns></returns>
        public List<Ship> GetHostiles()
        {
            //var x =  Sectors.Where(sector => sector.Item == SectorItem.Hostile);

            //List<Ship> baddies = this.Sectors.Where()
            throw new NotImplementedException();
        }

        internal int GetStarbaseCount()
        {
            return Sectors.Count(sector => sector.Item == SectorItem.Starbase);
        }

        internal int GetStarCount()
        {
            return Sectors.Count(sector => sector.Item == SectorItem.Star);
        }
    }
}
