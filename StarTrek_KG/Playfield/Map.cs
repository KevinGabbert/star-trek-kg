using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;

namespace StarTrek_KG.Playfield
{
    public class Map: IMap
    {
        #region Properties

            public Quadrants Quadrants { get; set; }
            public Ship Playership { get; set; } // todo: v2.0 will have a List<StarShip>().
            public SetupOptions GameConfig { get; set; }

            public int Stardate { get; set; }
            public int timeRemaining { get; set; }
            public int starbases { get; set; }
            public string Text { get; set; }
            public IOutputWrite Write { get; set; }
            public IStarTrekKGSettings Config { get; set; }
            public int HostilesToSetUp { get; set; }

            public FactionName DefaultHostile { get; set; }

        #endregion

        public Map()
        {

        }

        public Map(SetupOptions setupOptions, IOutputWrite write, IStarTrekKGSettings config, FactionName defaultHostile = null)
        {
            this.Config = config;
            this.Write = write;

            this.DefaultHostile = defaultHostile ?? FactionName.Klingon;

            this.Initialize(setupOptions);
        }

        public void Initialize(SetupOptions setupOptions)
        {
            this.GameConfig = setupOptions;

            if (setupOptions != null)
            {
                if (setupOptions.Initialize)
                {
                    if (setupOptions.SectorDefs == null)
                    {
                        //todo: then use appConfigSectorDefs  
                        throw new GameConfigException(this.Config.GetSetting<string>("NoSectorDefsSetUp"));
                    }

                    this.Initialize(setupOptions.SectorDefs, setupOptions.AddNebulae); //Playership is set up here.
                }

                //if (setupOptions.GenerateMap)
                //{
                //    //this.Quadrants.PopulateSectors(setupOptions.SectorDefs, this);
                //}
            }
        }

        public void Initialize(SectorDefs sectorDefs, bool generateWithNebulae)
        {
            this.GetGlobalInfo();

            //This list should match baddie type that is created
            List<string> quadrantNames = this.Config.GetStarSystems();

            this.Write.DebugLine("Got Starsystems");

            //TODO: if there are less than 64 quadrant names then there will be problems..

            var names = new Stack<string>(quadrantNames.Shuffle());

            var klingonShipNames = this.Config.FactionShips(this.DefaultHostile);

            this.Write.DebugLine("Got Baddies");

            //todo: modify this to populate with multiple faction types..
            var klingonBaddieNames = new Stack<string>(klingonShipNames.Shuffle());

            //todo: this just set up a "friendly"
            this.InitializeQuadrantsWithBaddies(names, klingonBaddieNames, this.DefaultHostile, sectorDefs, generateWithNebulae);

            this.Write.DebugLine("Intialized quadrants with Baddies");

            if (sectorDefs != null)
            {
                this.SetupFriendlies(sectorDefs);
            }

            //Modify this to output everything
            if (Constants.DEBUG_MODE)
            {
                //TODO: write a hidden command that displays everything. (for debug purposes)

                this.Write.DisplayPropertiesOf(this.Playership); //This line may go away as it should be rolled out with a new quadrant
                this.Write.Line(this.Config.GetSetting<string>("DebugModeEnd"));
                this.Write.Line("");
            }
        }

        public void SetupFriendlies(SectorDefs sectorDefs)
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
                    sectorToPlaceShip.Item = SectorItem.FriendlyShip;
                }
                catch (InvalidOperationException ex)
                {
                    throw new GameConfigException(this.Config.GetSetting<string>("InvalidPlayershipSetup") + ex.Message);
                }
                catch (Exception ex)
                {
                    throw new GameConfigException(this.Config.GetSetting<string>("ErrorPlayershipSetup") + ex.Message);
                }
            }
            else
            {
                //this.Quadrants[0].Active = true;
                this.Quadrants[0].SetActive();
            }
        }

        //Creates a 2D array of quadrants.  This is how all of our game pieces will be moving around.
        public void InitializeQuadrantsWithBaddies(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, SectorDefs sectorDefs, bool generateWithNebulae)
        {
            this.Quadrants = new Quadrants(this, this.Write);

            //Friendlies are added separately
            List<Sector> itemsToPopulateThatAreNotPlayerShip = sectorDefs.ToSectors(this.Quadrants).Where(q => q.Item != SectorItem.FriendlyShip).ToList();

            this.Write.DebugLine("ItemsToPopulate: " + itemsToPopulateThatAreNotPlayerShip.Count + " Quadrants: " + this.Quadrants.Count);
            
            //todo: this can be done with a single loop populating a list of XYs
            this.GenerateSquareGalaxy(names, baddieNames, stockBaddieFaction, itemsToPopulateThatAreNotPlayerShip, generateWithNebulae);
        }

        public void GenerateSquareGalaxy(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, List<Sector> itemsToPopulate, bool generateWithNebula)
        {
            if (Constants.QUADRANT_MAX == 0)
            {
                throw new GameException("No quadrants to set up.  QUADRANT_MAX set to Zero");
            }

            for (var quadrantX = 0; quadrantX < Constants.QUADRANT_MAX; quadrantX++) //todo: app.config
            {
                for (var quadrantY = 0; quadrantY < Constants.QUADRANT_MAX; quadrantY++)
                {
                    int index;
                    var newQuadrant = new Quadrant(this);
                    var quadrantXY = new Coordinate(quadrantX, quadrantY);

                    bool isNebulae = false;
                    if (generateWithNebula)
                    {
                        isNebulae = Utility.Utility.Random.Next(11) == 10; //todo pull this setting from config
                    }

                    newQuadrant.Create(names, baddieNames, stockBaddieFaction, quadrantXY, out index, itemsToPopulate,
                                       this.GameConfig.AddStars, isNebulae);

                    this.Quadrants.Add(newQuadrant);

                    if (Constants.DEBUG_MODE)
                    {
                        this.Write.SingleLine(this.Config.GetSetting<string>("DebugAddingNewQuadrant"));

                        this.Write.DisplayPropertiesOf(newQuadrant);

                        //TODO: each object within quadrant needs a .ToString()

                        this.Write.Line("");
                    }
                }
            }
        }

        public IEnumerable<Sector> AddStarbases()
        {
            throw new NotImplementedException();
        }

        //private static List<Sector> GetQuadrantObjects(int starbases, int HostilesToSetUp)
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
            //this.Hostiles = new Hostiles(); //todo: create an initial size the same as HostilesToSetUp

            this.HostilesToSetUp = this.Config.GetSetting<int>("totalHostiles") + (Utility.Utility.Random).Next(6);
            this.Stardate = this.Config.GetSetting<int>("stardate") + (Utility.Utility.Random).Next(50);
            this.timeRemaining = this.Config.GetSetting<int>("timeRemaining") + (Utility.Utility.Random).Next(10);
            this.starbases = this.Config.GetSetting<int>("starbases") + (Utility.Utility.Random).Next(3);

            this.Text = this.Config.GetSetting<string>("CommandPrompt");

            this.Write.DebugLine("HostilesToSetUp: " + this.HostilesToSetUp);
            this.Write.DebugLine("Stardate: " + Stardate);
            this.Write.DebugLine("timeRemaining: " + this.HostilesToSetUp);
            this.Write.DebugLine("starbases: " + this.HostilesToSetUp);
        }

        //refactor these to a setup object
        public void SetUpPlayerShip(SectorDef playerShipDef)
        {
            this.Write.DebugLine(this.Config.GetSetting<string>("DebugSettingUpPlayership"));

            //todo: remove this requirement
            if (this.Quadrants == null)
            {
                throw new GameException(this.Config.GetSetting<string>("QuadrantsNeedToBeSetup1"));
            }

            //todo: if playershipDef.GetFromConfig then grab info from config.  else set up with default random numbers.

            var playerShipName = this.Config.GetSetting<string>("PlayerShip");

            var startingSector = new Sector(new LocationDef(playerShipDef.QuadrantDef, new Coordinate(playerShipDef.Sector.X, playerShipDef.Sector.Y)));

            this.Playership = new Ship(FactionName.Federation, playerShipName, startingSector, this)
                                  {
                                      Allegiance = Allegiance.GoodGuy
                                  };

            this.Playership.Energy = this.Config.GetSetting<int>("energy");

            this.SetupPlayershipQuadrant(playerShipDef);

            this.SetupSubsystems();

            this.Playership.Destroyed = false;
        }

        public void SetupSubsystems()
        {
            this.GetSubsystemSetupFromConfig();

            //todo:  do we pull strings from config and then put a switch statement below to set up individual systems??

            foreach (ISubsystem subSystem in this.Playership.Subsystems)
            {
                subSystem.Damage = 0;
                subSystem.ShipConnectedTo = this.Playership;
            }

            this.SetupPlayershipNav();
            this.SetupPlayershipShields();
            this.SetupPlayershipTorpedoes();

            //ShortRangeScan.For(this.Playership).Damage = 0;
            //ShortRangeScan.For(this.Playership).ShipConnectedTo = this.Playership;

            //LongRangeScan.For(this.Playership).Damage = 0;
            //LongRangeScan.For(this.Playership).ShipConnectedTo = this.Playership;

            //CombinedRangeScan.For(this.Playership).Damage = 0;
            //CombinedRangeScan.For(this.Playership).ShipConnectedTo = this.Playership;

            //Computer.For(this.Playership).Damage = 0;
            //Computer.For(this.Playership).ShipConnectedTo = this.Playership;

            //Phasers.For(this.Playership).Damage = 0;
            //Phasers.For(this.Playership).ShipConnectedTo = this.Playership;

            //DamageControl.For(this.Playership).Damage = 0;
            //DamageControl.For(this.Playership).ShipConnectedTo = this.Playership;
        }

        public void GetSubsystemSetupFromConfig()
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

        public void SetupPlayershipQuadrant(SectorDef playerShipDef)
        {
            if (playerShipDef.QuadrantDef == null)
            {
                playerShipDef.QuadrantDef = Coordinate.GetRandom();
            }

            if(this.Quadrants.Count == 0)
            {
                throw new ArgumentException(this.Config.GetSetting<string>("QuadrantsNotSetUp"));
            }

            var m = this.Quadrants.Single(q => q.X == playerShipDef.QuadrantDef.X && q.Y == playerShipDef.QuadrantDef.Y);
            this.Playership.Coordinate = new Coordinate(m.X, m.Y);

            //this.Playership.GetQuadrant().Active = true;
            this.Playership.GetQuadrant().SetActive();
        }

        public void SetupPlayershipTorpedoes()
        {
            var torpedoes = Torpedoes.For(this.Playership);

            torpedoes.ShipConnectedTo = this.Playership;
            torpedoes.Count = this.Config.GetSetting<int>("photonTorpedoes");
            //torpedoes.Damage = 0;
        }

        public void SetupPlayershipShields()
        {
            var starshipShields = Shields.For(this.Playership);

            //starshipShields.ShipConnectedTo = this.Playership;
            starshipShields.Energy = 0;
            //starshipShields.Damage = 0;
        }

        public void SetupPlayershipNav()
        {
            var starshipNAV = Navigation.For(this.Playership);

            //starshipNAV.ShipConnectedTo = this.Playership;
            starshipNAV.Movement.ShipConnectedTo = this.Playership;
            //starshipNAV.Damage = 0;
            starshipNAV.MaxWarpFactor = this.Config.GetSetting<int>("MaxWarpFactor");
            starshipNAV.Docked = false;
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

        public void RemoveAllDestroyedShips(IMap map, IEnumerable<IShip> destroyedShips)
        {
            map.Quadrants.Remove(destroyedShips);
            map.Quadrants.GetActive().GetHostiles().RemoveAll(s => s.Destroyed);
        }
        public void RemoveDestroyedShipsAndScavenge(List<IShip> destroyedShips)
        {
            this.RemoveAllDestroyedShips(this, destroyedShips); //remove from Hostiles collection

            foreach (var destroyedShip in destroyedShips)
            {
                if (destroyedShip.Faction == FactionName.Federation)
                {
                    this.Playership.Scavenge(ScavengeType.FederationShip);
                }
                else
                {
                    this.Playership.Scavenge(ScavengeType.OtherShip);
                }

                //todo: add else if for starbase when the time comes
            }
        }

        public void RemoveTargetFromSector(IMap map, IShip ship)
        {
            map.Quadrants.Remove(ship);
        }

        //todo: finish this
        //public SectorItem GetShip(int quadrantX, int quadrantY, int sectorX, int sectorY)
        //{
        //    var t = this.Quadrants.Where(q => q.X == quadrantX &&
        //                                      q.Y == quadrantY).Single().Sectors.Where(s => s.X == sectorX &&
        //                                                                                    s.Y == sectorY).Single().Item;


        //}

        ///// <summary>
        ///// Removes all friendlies fromevery sector in the entire map.
        ///// </summary>
        ///// <param name="map"></param>
        //public void RemoveAllFriendlies(IMap map)
        //{
        //    var sectorsWithFriendlies = map.Quadrants.SelectMany(quadrant => quadrant.Sectors.Where(sector => sector.Item == SectorItem.Friendly));

        //    foreach (Sector sector in sectorsWithFriendlies)
        //    {
        //        sector.Item = SectorItem.Empty;
        //    }
        //}

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.
        /// </summary>
        /// <param name="map"></param>
        public void RemovePlayership(IMap map)
        {
            map.Playership.Sector.Item = SectorItem.Empty;
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.  Sets down a friendly 
        /// </summary>
        /// <param name="map"></param>
        public void SetActiveSectorAsFriendly(IMap map)
        {
            //zip through all sectors in all quadrants.  remove any friendlies

            //This is a bit of a brute force approach, and not preferred, as it disguises any bugs that might have to do with forgetting
            //to remove the ship at the right time.  This function will need to go away or stop being used when or if this game is modified
            //to have multiple friendlies, as is the eventual plan.

            this.RemovePlayership(map);

            var activeQuadrant = map.Quadrants.GetActive();

            var newActiveSector = Sector.Get(activeQuadrant.Sectors, map.Playership.Sector.X, map.Playership.Sector.Y);
            newActiveSector.Item = SectorItem.FriendlyShip;
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

        public void AddACoupleHostileFederationShipsToExistingMap()
        {
            var federationShipNames = this.Config.FactionShips(FactionName.Federation);
            var federaleNames = new Stack<string>(federationShipNames.Shuffle());

            foreach (var quadrant in this.Quadrants)
            {
                var added = AddHostileFedToEmptyQuadrant(quadrant, federaleNames);
                if (added) break; //we found an empty quad to dump new fed ships into..
            }
        }

        private bool AddHostileFedToEmptyQuadrant(IQuadrant quadrant, Stack<string> federaleNames)
        {
            var hostilesInQuad = quadrant.GetHostiles();
            if (hostilesInQuad.Any()) //we don't want to mix with Klingons just yet..
            {
                var klingons = hostilesInQuad.Where(h => h.Faction == FactionName.Klingon);

                if (!klingons.Any())
                {
                    this.AddHostilesToQuadrant(quadrant, federaleNames);

                    //we are only doing this once..
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a random number of federation ships to the map
        /// </summary>
        public void AddHostileFederationShipsToExistingMap()
        {
            var federationShipNames = this.Config.FactionShips(FactionName.Federation);
            var federaleNames = new Stack<string>(federationShipNames.Shuffle());

            foreach (var quadrant in this.Quadrants)
            {
                if (Utility.Utility.Random.Next(5) == 1) //todo: resource this out.
                {
                    if (!quadrant.GetHostiles().Any()) //we don't want to mix with Klingons just yet..
                    {
                        this.AddHostilesToQuadrant(quadrant, federaleNames);
                    }
                }
            }
        }

        private void AddHostilesToQuadrant(IQuadrant quadrant, Stack<string> federaleNames)
        {
            var numberOfHostileFeds = Utility.Utility.Random.Next(2);

            for (int i = 0; i < numberOfHostileFeds; i++)
            {
                //todo: refactor to Quadrant object
                ISector sectorToAddTo = this.GetUnoccupiedRandomSector(quadrant);
                this.AddHostileFederale(quadrant, sectorToAddTo, federaleNames);
            }
        }

        private ISector GetUnoccupiedRandomSector(IQuadrant quadrant)
        {
            var randomCoordinate = GetRandomCoordinate();

            ISector sector = quadrant.GetSector(randomCoordinate);
            var sectorIsEmpty = (sector.Item == SectorItem.Empty);

            if (!sectorIsEmpty)
            {
                sector = this.GetUnoccupiedRandomSector(quadrant);
            }

            return sector;
        }

        private static Coordinate GetRandomCoordinate()
        {
            var x = Utility.Utility.Random.Next(Constants.SECTOR_MIN);
            var y = Utility.Utility.Random.Next(Constants.SECTOR_MAX);

            var randomCoordinate = new Coordinate(x, y);
            return randomCoordinate;
        }

        private void AddHostileFederale(IQuadrant quadrant, ISector sector, Stack<string> federaleNames)
        {
            var newPissedOffFederale = new Ship(FactionName.Federation, federaleNames.Pop(), sector, this);
            Shields.For(newPissedOffFederale).Energy = Utility.Utility.Random.Next(100, 500); //todo: resource those numbers out

            quadrant.AddShip(newPissedOffFederale, sector);

            this.Write.Line("Comm Reports a Federation starship has warped into Quadrant: " + quadrant.Name);
        }

        public IEnumerable<IShip> GetAllFederationShips()
        {
            //todo: finish this.
            return new List<Ship>();
        }

        public bool OutOfBounds(int quadrantY, int quadrantX)
        {
            var inTheNegative = quadrantX < 0 || quadrantY < 0;
            var maxxed = quadrantX == Constants.QUADRANT_MAX || quadrantY == Constants.QUADRANT_MAX;

            var yOnMap = quadrantY >= 0 && quadrantY < Constants.QUADRANT_MAX;
            var xOnMap = quadrantX >= 0 && quadrantX < Constants.QUADRANT_MAX;

            return (inTheNegative || maxxed) && !(yOnMap && xOnMap);
        }
    }
}
