using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Utility;

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
                        throw new GameConfigException(StarTrekKGSettings.GetSetting<string>("NoSectorDefsSetUp"));
                    }

                    this.Initialize(setupOptions.SectorDefs); //Playership is set up here.
                }

                //if (setupOptions.GenerateMap)
                //{
                //    //this.Quadrants.PopulateSectors(setupOptions.SectorDefs, this);
                //}
            }
        }

        public void Initialize(SectorDefs sectorDefs)
        {
            this.GetGlobalInfo();

            //This list should match baddie type that is created
            List<string> quadrantNames = StarTrekKGSettings.GetStarSystems();

            Output.Write.DebugLine("Got Starsystems");

            //TODO: if there are less than 64 quadrant names then there will be problems..

            var names = new Stack<string>(quadrantNames.Shuffle());

            var klingonShipNames = StarTrekKGSettings.GetShips("Klingon");

            Output.Write.DebugLine("Got Baddies");

            var baddieNames = new Stack<string>(klingonShipNames.Shuffle());

            //todo: this just set up a "friendly"
            this.InitializeQuadrantsWithBaddies(names, baddieNames, sectorDefs);

            Output.Write.DebugLine("Intialized quadrants with Baddies");

            if (sectorDefs != null)
            {
                this.SetupFriendlies(sectorDefs);
            }

            //Modify this to output everything
            if (Constants.DEBUG_MODE)
            {
                //TODO: write a hidden command that displays everything. (for debug purposes)

                Output.Write.DisplayPropertiesOf(this.Playership); //This line may go away as it should be rolled out with a new quadrant
                Output.Write.Line(StarTrekKGSettings.GetSetting<string>("DebugModeEnd"));
                Output.Write.Line("");
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

                    var sectorToPlaceShip = Sector.Get(this.Quadrants.GetActive().Sectors, this.Playership.Sector.X, this.Playership.Sector.Y);

                    //This places our newly created ship into our newly created List of Quadrants.
                    sectorToPlaceShip.Item = SectorItem.Friendly;
                }
                catch (InvalidOperationException ex)
                {
                    throw new GameConfigException(StarTrekKGSettings.GetSetting<string>("InvalidPlayershipSetup") + ex.Message);
                }
                catch (Exception ex)
                {
                    throw new GameConfigException(StarTrekKGSettings.GetSetting<string>("ErrorPlayershipSetup") + ex.Message);
                }
            }
            else
            {
                this.Quadrants[0].Active = true;
            }
        }

        //Creates a 2D array of quadrants.  This is how all of our game pieces will be moving around.
        public void InitializeQuadrantsWithBaddies(Stack<string> names, Stack<string> baddieNames, SectorDefs sectorDefs)
        {
            this.Quadrants = new Quadrants(this);

            //Friendlies are added separately
            var itemsToPopulate = sectorDefs.ToSectors(this.Quadrants).Where(i => i.Item != SectorItem.Friendly).ToList();

            //Console.WriteLine("ItemsToPopulate: " + itemsToPopulate.Count + " Quadrants: " + this.Quadrants.Count);
            
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

                    if (Constants.DEBUG_MODE)
                    {
                        Output.Write.SingleLine(StarTrekKGSettings.GetSetting<string>("DebugAddingNewQuadrant"));
                        
                        Output.Write.DisplayPropertiesOf(newQuadrant);

                        //TODO: each object within quadrant needs a .ToString()

                        Output.Write.Line("");
                    }
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
                var x = (Utility.Utility.Random).Next(Constants.SECTOR_MAX);
                var y = (Utility.Utility.Random).Next(Constants.SECTOR_MAX);

                //todo: just pass in sector and get its item
                var sector = quadrant.Sectors.Single(s => s.X == x && s.Y == y);
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

        //private static List<Sector> GetQuadrantObjects(int starbases, int hostilesToSetUp)
        //{
        //    var quadrantObjects = new List<Sector>();

        //    //get stars for quadrant and subtract from parameter (will be subtracted when this is hit next?)
        //    //newQuadrant.Stars = 1 + (Utility.Random).Next(Constants.SECTOR_MAX);
        //    //get hostiles for quadrant and subtract from big list
        //    //get starbase T/F and subtract from big list
        //    return quadrantObjects;
        //}

        /// <summary>
        /// This will eventually be moved into each individual object
        /// </summary>
        public void GetGlobalInfo()
        {
            //this.Hostiles = new Hostiles(); //todo: create an initial size the same as hostilesToSetUp

            this.hostilesToSetUp = StarTrekKGSettings.GetSetting<int>("totalHostiles") + (Utility.Utility.Random).Next(6);
            this.Stardate = StarTrekKGSettings.GetSetting<int>("stardate") + (Utility.Utility.Random).Next(50);
            this.timeRemaining = StarTrekKGSettings.GetSetting<int>("timeRemaining") + (Utility.Utility.Random).Next(10);
            this.starbases = StarTrekKGSettings.GetSetting<int>("starbases") + (Utility.Utility.Random).Next(3);

            this.Text = StarTrekKGSettings.GetSetting<string>("CommandPrompt");

            Output.Write.DebugLine("HostilesToSetUp: " + hostilesToSetUp);
            Output.Write.DebugLine("Stardate: " + Stardate);
            Output.Write.DebugLine("timeRemaining: " + hostilesToSetUp);
            Output.Write.DebugLine("starbases: " + hostilesToSetUp);
        }

        //refactor these to a setup object
        private void SetUpPlayerShip(SectorDef playerShipDef)
        {
            Output.Write.DebugLine(StarTrekKGSettings.GetSetting<string>("DebugSettingUpPlayership"));

            //todo: remove this requirement
            if (this.Quadrants == null)
            {
                throw new GameException(StarTrekKGSettings.GetSetting<string>("QuadrantsNeedToBeSetup1"));
            }

            //todo: if playershipDef.GetFromConfig then grab info from config.  else set up with default random numbers.

            var playerShipName = StarTrekKGSettings.GetSetting<string>("PlayerShip");

            var startingSector = new Sector(new LocationDef(playerShipDef.QuadrantDef, new Coordinate(playerShipDef.Sector.X, playerShipDef.Sector.Y)));

            this.Playership = new Ship(playerShipName, this, startingSector)
                                  {
                                      Allegiance = Allegiance.GoodGuy
                                  };

            this.Playership.Energy = StarTrekKGSettings.GetSetting<int>("energy");

            this.SetupPlayershipQuadrant(playerShipDef);

            this.SetupSubsystems();

            this.Playership.Destroyed = false;
        }

        private void SetupSubsystems()
        {
            this.GetSubsystemSetupFromConfig();

            //todo:  do we pull strings from config and then put a switch statement below to set up individual systems??

            this.SetupPlayershipNav();
            this.SetupPlayershipShields();

            ShortRangeScan.For(this.Playership).Damage = 0;
            LongRangeScan.For(this.Playership).Damage = 0;

            Computer.For(this.Playership).Damage = 0;

            this.SetupPlayershipTorpedoes();

            Phasers.For(this.Playership).Damage = 0;
        }

        private void GetSubsystemSetupFromConfig()
        {
            //TODO: Finish this

            //var subsystemsToSetUp = new List<ISubsystem>();

            ////pull desired subsystem Setup from App.Config

            //foreach (var subsystem in appConfigSubSystem)
            //{
            //    //Set up SubSystem
            //    subsystemsToSetUp.Add(new Shields(this));

            //    //Might have to do a switch if we can't use reflection to create the objcet
            //}
        }

        private void SetupPlayershipQuadrant(SectorDef playerShipDef)
        {
            if (playerShipDef.QuadrantDef == null)
            {
                playerShipDef.QuadrantDef = new Coordinate((Utility.Utility.Random).Next(Constants.SECTOR_MAX),
                                                           (Utility.Utility.Random).Next(Constants.SECTOR_MAX));
            }

            if(this.Quadrants.Count == 0)
            {
                throw new ArgumentException(StarTrekKGSettings.GetSetting<string>("QuadrantsNotSetUp"));
            }

            var m = this.Quadrants.Single(q => q.X == playerShipDef.QuadrantDef.X && q.Y == playerShipDef.QuadrantDef.Y);
            this.Playership.QuadrantDef = new Coordinate(m.X, m.Y);
            this.Playership.GetQuadrant().Active = true;
        }
        private void SetupPlayershipTorpedoes()
        {
            var torpedoes = Torpedoes.For(this.Playership);
            torpedoes.Count = StarTrekKGSettings.GetSetting<int>("photonTorpedoes");
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
            starshipNAV.MaxWarpFactor = StarTrekKGSettings.GetSetting<int>("MaxWarpFactor");
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
                    var gotSector = Sectors.GetNoError(x, y, sectors);

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
            var item = this.Get(quadrantX, quadrantY, sectorX, sectorY).Item;
            return item;
        }

        public Sector Get(int quadrantX, int quadrantY, int sectorX, int sectorY)
        {
            var item = this.Quadrants.Single(q => q.X == quadrantX &&
                                                  q.Y == quadrantY).Sectors.Single(s => s.X == sectorX &&
                                                                                        s.Y == sectorY);
            return item;
        }

        public void RemoveAllDestroyedShips(Map map, List<IShip> destroyedShips)
        {
            map.Quadrants.Remove(destroyedShips, map);
            map.Quadrants.GetActive().GetHostiles().RemoveAll(s => s.Destroyed);
        }

        public static bool DestroyedBaddies(Map map, IEnumerable<IShip> query)
        {
            foreach (var ship in query)
            {
                map.Quadrants.Remove(ship, map);

                return true;
            }

            return false;
        }

        //todo: finish this
        //public SectorItem GetShip(int quadrantX, int quadrantY, int sectorX, int sectorY)
        //{
        //    var t = this.Quadrants.Where(q => q.X == quadrantX &&
        //                                      q.Y == quadrantY).Single().Sectors.Where(s => s.X == sectorX &&
        //                                                                                    s.Y == sectorY).Single().Item;


        //}
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

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.
        /// </summary>
        /// <param name="map"></param>
        public static void RemoveAllFriendlies(Map map)
        {
            var sectorsWithFriendlies = map.Quadrants.SelectMany(quadrant => quadrant.Sectors.Where(sector => sector.Item == SectorItem.Friendly));

            foreach (Sector sector in sectorsWithFriendlies)
            {
                sector.Item = SectorItem.Empty;
            }
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.  Sets down a friendly 
        /// </summary>
        /// <param name="map"></param>
        public static void SetFriendly(Map map)
        {
            //zip through all sectors in all quadrants.  remove any friendlies

            //This is a bit of a brute force approach, and not preferred, as it disguises any bugs that might have to do with forgetting
            //to remove the ship at the right time.  This function will need to go away or stop being used when or if this game is modified
            //to have multiple friendlies, as is the eventual plan.

            Map.RemoveAllFriendlies(map);
            Sector.Get(map.Quadrants.GetActive().Sectors, map.Playership.Sector.X,
                       map.Playership.Sector.Y).Item = SectorItem.Friendly;
        }

        public override string ToString()
        {
            string returnVal = null;

            //if debugMode
            //returns the location of every single object in the map

            //roll out every object in:
            //this.Quadrants;
            //this.GameConfig;
            

            return returnVal;
        }
    }
}
