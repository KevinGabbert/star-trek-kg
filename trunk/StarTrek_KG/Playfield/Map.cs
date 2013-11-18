using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG.Playfield
{
    public class Map
    {
        #region Properties

            public Quadrants Quadrants { get; set; }
            public Ship Playership { get; set; } // todo: v2.0 will have a List<StarShip>().
            public GameConfig GameConfig { get; set; }

            public int Stardate { get; set; }
            public int timeRemaining { get; set; }
            public int starbases { get; set; }
            public string Text { get; set; }

        #endregion

        public int hostilesToSetUp;

        public Map()
        {

        }

        public Map(GameConfig setupOptions)
        {
            this.Initialize(setupOptions);
        }

        private void Initialize(GameConfig setupOptions)
        {
            this.GameConfig = setupOptions;

            if (setupOptions != null)
            {
                if (setupOptions.Initialize)
                {
                    if (setupOptions.SectorDefs == null)
                    {
                        //todo: then use appConfigSectorDefs  
                        throw new GameConfigException("No SectorDefs set up.");
                    }

                    this.Initialize(setupOptions.SectorDefs); //Playership is set up here.
                }

                if (setupOptions.GenerateMap)
                {
                    //this.Quadrants.PopulateSectors(setupOptions.SectorDefs, this);
                }
            }
        }

        public void Initialize(SectorDefs sectorDefs)
        {
            this.GetGlobalInfo();

            //Get this list from app.config.  get list that matches baddie type that is created
            var names = new Stack<string>(Constants.QUADRANT_NAMES.ToList().Shuffle());
            var baddieNames = new Stack<string>(Constants.KLINGON_SHIP_NAMES.ToList().Shuffle());

            //todo: this just set up a "friendly"
            this.InitializeQuadrants(names, baddieNames, sectorDefs);

            if (sectorDefs != null)
            {
                this.SetupFriendlies(sectorDefs);
            }
        }

        private void SetupFriendlies(SectorDefs sectorDefs)
        {
            //if we have > 0 friendlies with XYs, then we will place them.
            //if we have at least 1 friendly with no XY, then config will be used to generate that type of ship.

            if (sectorDefs.Friendlies().Any())
            {
                try
                {
                    this.SetUpPlayerShip(sectorDefs.Friendlies().Single());

                    //This places our newly created ship into our newly created List of Quadrants.
                    Sector.Get(this.Quadrants.GetActive().Sectors, this.Playership.Sector.X, this.Playership.Sector.Y).Item = SectorItem.Friendly;
                }
                catch (InvalidOperationException ex)
                {
                    throw new GameConfigException("error setting up PlayerShip. " + ex.Message);
                }
                catch (Exception ex)
                {
                    throw new GameConfigException("error setting up PlayerShip. " + ex.Message);
                }
            }
            else
            {
                this.Quadrants[0].Active = true;
            }
        }

        //Creates a 2D array of quadrants.  This is how all of our game pieces will be moving around.
        public void InitializeQuadrants(Stack<string> names, Stack<string> baddieNames, SectorDefs sectorDefs)
        {
            this.Quadrants = new Quadrants(this);

            //Friendlies are added separately
            var itemsToPopulate = sectorDefs.ToSectors(this.Quadrants).Where(i => i.Item != SectorItem.Friendly).ToList();
            
            //todo: this can be done with a single loop populating a list of XYs

            for (var quadrantX = 0; quadrantX < Constants.QUADRANT_MAX; quadrantX++) //todo: app.config
            {
                for (var quadrantY = 0; quadrantY < Constants.QUADRANT_MAX; quadrantY++)
                {
                    int index;
                    var newQuadrant = new Quadrant();
                    var quadrantXY = new Coordinate(quadrantX, quadrantY);

                    newQuadrant.Create(this, names, baddieNames, quadrantXY, out index, itemsToPopulate, this.GameConfig.AddStars);
                    this.Quadrants.Add(newQuadrant);
                }
            }
        }

        private IEnumerable<Sector> AddStarbases()
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Sector> AddStars(Quadrant quadrant, int totalStarsInQuadrant)
        {
            while (totalStarsInQuadrant > 0)
            {
                var x = (Utility.Random).Next(Constants.SECTOR_MAX);
                var y = (Utility.Random).Next(Constants.SECTOR_MAX);

                //todo: just pass in sector and get its item
                var sector = quadrant.Sectors.Where(s => s.X == x && s.Y == y).Single();
                var sectorEmpty = sector.Item == SectorItem.Empty;

                if (sectorEmpty)
                {
                    if (totalStarsInQuadrant > 0)
                    {
                        sector.Item = SectorItem.Star;
                        totalStarsInQuadrant--;
                    }
                }
            }

            return quadrant.Sectors.Where(s => s.Item == SectorItem.Star);
        }

        private static List<Sector> GetQuadrantObjects(int starbases, int hostilesToSetUp)
        {
            var quadrantObjects = new List<Sector>();

            //get stars for quadrant and subtract from parameter (will be subtracted when this is hit next?)
            //newQuadrant.Stars = 1 + (Utility.Random).Next(Constants.SECTOR_MAX);
            //get hostiles for quadrant and subtract from big list
            //get starbase T/F and subtract from big list
            return quadrantObjects;
        }

        /// <summary>
        /// This will eventually be moved into each individual object
        /// </summary>
        public void GetGlobalInfo()
        {
            //this.Hostiles = new Hostiles(); //todo: create an initial size the same as hostilesToSetUp

            this.hostilesToSetUp = AppConfig.Setting<int>("totalHostiles") + (Utility.Random).Next(6);
            this.Stardate = AppConfig.Setting<int>("stardate") + (Utility.Random).Next(50);
            this.timeRemaining = AppConfig.Setting<int>("timeRemaining") + (Utility.Random).Next(10);
            this.starbases = AppConfig.Setting<int>("starbases") + (Utility.Random).Next(3);

            this.Text = AppConfig.Setting<string>("CommandPrompt");
        }

        //refactor these to a setup object
        private void SetUpPlayerShip(SectorDef playerShipDef)
        {
            //todo: remove this requirement
            if (this.Quadrants == null)
            {
                throw new GameException("Quadrants need to be set up before setting up a playership");
            }

            //todo: if playershipDef.GetFromConfig then grab info from config.  else set up with default random numbers.

            var playerShipName = AppConfig.Setting<string>("playerShip");

            var startingSector = new Sector(new LocationDef(playerShipDef.QuadrantDef, new Coordinate(playerShipDef.Sector.X, playerShipDef.Sector.Y)));
            this.Playership = new Ship(playerShipName, this, startingSector)
                                  {
                                      Allegiance = Allegiance.GoodGuy
                                  };

            this.Playership.Energy = AppConfig.Setting<int>("energy");

            this.SetupPlayershipQuadrant(playerShipDef);

            this.SetupPlayershipNav();
            this.SetupPlayershipShields();

            ShortRangeScan.For(this.Playership).Damage = 0;
            LongRangeScan.For(this.Playership).Damage = 0;

            Computer.For(this.Playership).Damage = 0;

            this.SetupPlayershipTorpedoes();

            Phasers.For(this.Playership).Damage = 0;

            this.Playership.Destroyed = false;
        }

        private void SetupPlayershipQuadrant(SectorDef playerShipDef)
        {
            if (playerShipDef.QuadrantDef == null)
            {
                playerShipDef.QuadrantDef = new Coordinate((Utility.Random).Next(Constants.SECTOR_MAX),
                                                           (Utility.Random).Next(Constants.SECTOR_MAX));
            }

            var m = this.Quadrants.Where(q => q.X == playerShipDef.QuadrantDef.X && q.Y == playerShipDef.QuadrantDef.Y).Single();
            this.Playership.QuadrantDef = new Coordinate(m.X, m.Y);
            this.Playership.GetQuadrant().Active = true;
        }
        private void SetupPlayershipTorpedoes()
        {
            var torpedoes = Torpedoes.For(this.Playership);
            torpedoes.Count = AppConfig.Setting<int>("photonTorpedoes");
            torpedoes.Damage = 0;
        }
        private void SetupPlayershipShields()
        {
            var starshipShields = Shields.For(this.Playership);
            starshipShields.Energy = 0;
            starshipShields.Damage = 0;
        }
        private void SetupPlayershipNav()
        {
            var starshipNAV = Navigation.For(this.Playership);

            starshipNAV.Damage = 0;
            starshipNAV.MaxWarpFactor = AppConfig.Setting<int>("MaxWarpFactor");
            starshipNAV.docked = false;
        }

        /// <summary>
        /// Legacy code. todo: needs to be rewritten.  Checks all sectors around starbase to see if its a good place to dock.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="sectors"></param>
        /// <returns></returns>
        public bool IsDockingLocation(int i, int j, Sectors sectors)
        {
            //http://stackoverflow.com/questions/3150678/using-linq-with-2d-array-select-not-found
            for (int y = i - 1; y <= i + 1; y++)
            {
                for (int x = j - 1; x <= j + 1; x++)
                {
                    var gotSector = Playfield.Sectors.GetNoError(x, y, sectors);

                    if (gotSector != null)
                    {
                        if (gotSector.Item == SectorItem.Starbase)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //calculates distance traveled mostly to be able to have a value for diagonal distance, as straight line calculation doesnt need all this effort.
        public static double Distance(double startX, double startY, double destinationX, double destinationY)
        {
            var distanceTraveledX = destinationX - startX;
            var distanceTraveledY = destinationY - startY;

            //X squared + Y Squared = Z squared. (Pythagorean Theorem), then Square Root to solve: 
            return Math.Sqrt(distanceTraveledX * distanceTraveledX + 
                             distanceTraveledY * distanceTraveledY);
        }

        /// <summary>
        /// This function represents the amount of energy fired by an opposing ship.
        /// The value is a seeded random number that decreases by distance
        /// </summary>
        /// <param name="distanceDeprecationLevel"> </param>
        /// <param name="distance"></param>
        /// <param name="seed"> </param>
        /// <returns></returns>
        public int DisruptorShot(int seed, double distanceDeprecationLevel, double distance)
        {
            //todo: give ship a disruptor weapon type, enable it only on hostileType.Klingon.  delete this.

            //const int seed = 300; //todo: pull from config
            //const double distanceDeprecationLevel = 11.3; //todo: pull deprecationlevel from config
            var x = (1.0 - distance / distanceDeprecationLevel);

            var deliveredEnergy = (int)(seed * (Utility.Random).NextDouble() * x);

            return deliveredEnergy;
        }

        //starbase position info (there needs to be a starbase object)
        public static int starbaseX;
        public static int starbaseY;

        public void StarbaseCalculator()
        {
            var location = Navigation.For(this.Playership);
            //if (StarTrek_KG.Quadrants.Get(this, location.quadrantX, location.quadrantY).Starbase)
            //{
            //    Console.WriteLine("Starbase in sector [{0},{1}].", (starbaseX + 1), (starbaseY + 1));
            //    Console.WriteLine("Direction: {0:#.##}", Map.ComputeDirection(location.sectorX, location.sectorY, starbaseX, starbaseY));
            //    Console.WriteLine("Distance:  {0:##.##}", Distance(location.sectorX, location.sectorY, starbaseX, starbaseY) / Constants.SECTOR_MAX);
            //}
            //else
            //{
            //    Output.Write("There are no starbases in this quadrant.");
            //}
        }

        public static double ComputeDirection(int x1, int y1, int x2, int y2)
        {
            //Todo:  to place this in the right place, resolve StarbaseCalculator

            double direction;
            if (x1 == x2)
            {
                direction = y1 < y2 ? 7 : 3;
            }
            else if (y1 == y2)
            {
                direction = x1 < x2 ? 1 : 5;
            }
            else
            {
                double dy = Math.Abs(y2 - y1);
                double dx = Math.Abs(x2 - x1);
                var angle = Math.Atan2(dy, dx);
                if (x1 < x2)
                {
                    if (y1 < y2)
                    {
                        direction = 9.0 - 4.0 * angle / Math.PI;
                    }
                    else
                    {
                        direction = 1.0 + 4.0 * angle / Math.PI;
                    }
                }
                else
                {
                    if (y1 < y2)
                    {
                        direction = 5.0 + 4.0 * angle / Math.PI;
                    }
                    else
                    {
                        direction = 5.0 - 4.0 * angle / Math.PI;
                    }
                }
            }
            return direction;
        }

        ///// <summary>
        ///// TODO: this needs to be changed.  after destruction, it appears to take several method returns to realize that we are dead.
        ///// </summary>
        ///// <returns></returns>
        //public bool ALLHostilesAttack()
        //{
        //    //this is called from torpedo control/phaser control, and navigation control

        //    if (this.Hostiles.Count > 0)
        //    {
        //        var starship = Navigation.For(this.Playership);
        //        foreach (var badGuy in this.Hostiles)
        //        {
        //            if (starship.docked)
        //            {
        //                Console.WriteLine(
        //                    this.Playership.Name + " hit by ship at sector [{0},{1}]. No damage due to starbase shields.",
        //                    (badGuy.Sector.X), (badGuy.Sector.Y));
        //            }
        //            else
        //            {
        //                if (!Ship.AbsorbHitFrom(badGuy, this)) return true;
        //            }
        //        }
        //        return true;
        //    }

        //    return false;
        //}

        public SectorItem GetItem(int quadrantX, int quadrantY, int sectorX, int sectorY)
        {
            var item = this.Quadrants.Single(q => q.X == quadrantX &&
                                                  q.Y == quadrantY).Sectors.Single(s => s.X == sectorX &&
                                                                                        s.Y == sectorY).Item;
            return item;
        }

        public Sector Get(int quadrantX, int quadrantY, int sectorX, int sectorY)
        {
            var item = this.Quadrants.Single(q => q.X == quadrantX &&
                                                  q.Y == quadrantY).Sectors.Single(s => s.X == sectorX &&
                                                                                        s.Y == sectorY);
            return item;
        }

        public void RemoveAllDestroyedShips(Map map, List<Ship> destroyedShips)
        {
            map.Quadrants.Remove(destroyedShips, map);
            map.Quadrants.GetActive().Hostiles.RemoveAll(s => s.Destroyed);
        }

        //todo: finish this
        //public SectorItem GetShip(int quadrantX, int quadrantY, int sectorX, int sectorY)
        //{
        //    var t = this.Quadrants.Where(q => q.X == quadrantX &&
        //                                      q.Y == quadrantY).Single().Sectors.Where(s => s.X == sectorX &&
        //                                                                                    s.Y == sectorY).Single().Item;


        //}
    }
}
