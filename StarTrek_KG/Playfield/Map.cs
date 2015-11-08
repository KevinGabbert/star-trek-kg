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

            public IGame Game { get; set; }
            public Regions Regions { get; set; }
            public Ship Playership { get; set; } // todo: v2.0 will have a List<StarShip>().
            public SetupOptions GameConfig { get; set; }
            public IInteraction Write { get; set; }
            public IStarTrekKGSettings Config { get; set; }
            public FactionName DefaultHostile { get; set; }

            public int Stardate { get; set; }
            public int timeRemaining { get; set; }
            public int starbases { get; set; }
            public string Text { get; set; }
            public int HostilesToSetUp { get; set; }

        #endregion

        public Map()
        {

        }

        public Map(SetupOptions setupOptions, IInteraction write, IStarTrekKGSettings config, FactionName defaultHostile = null)
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
                //    //this.Regions.PopulateSectors(setupOptions.SectorDefs, this);
                //}
            }
        }

        public void Initialize(SectorDefs sectorDefs, bool generateWithNebulae)
        {
            this.GetGlobalInfo();

            //This list should match baddie type that is created
            List<string> RegionNames = this.Config.GetStarSystems();

            this.Write.DebugLine("Got Starsystems");

            //TODO: if there are less than 64 Region names then there will be problems..

            var names = new Stack<string>(RegionNames.Shuffle());

            var baddieShipNames = this.Config.FactionShips(this.DefaultHostile);

            this.Write.DebugLine("Got Baddies");

            //todo: modify this to populate with multiple faction types..
            var baddieNames = new Stack<string>(baddieShipNames.Shuffle());

            //todo: this just set up a "friendly"
            this.InitializeRegionsWithBaddies(names, baddieNames, this.DefaultHostile, sectorDefs, generateWithNebulae);

            this.Write.DebugLine("Intialized Regions with Baddies");

            if (sectorDefs != null)
            {
                this.SetupPlayerShipInSectors(sectorDefs);
            }

            //Modify this to output everything
            if (DEFAULTS.DEBUG_MODE)
            {
                //TODO: write a hidden command that displays everything. (for debug purposes)

                this.Write.DisplayPropertiesOf(this.Playership); //This line may go away as it should be rolled out with a new Region
                this.Write.Line(this.Config.GetSetting<string>("DebugModeEnd"));
                this.Write.Line("");
            }

            this.Playership?.UpdateDivinedSectors();
        }

        public void SetupPlayerShipInSectors(SectorDefs sectorDefs)
        {
            //if we have > 0 friendlies with XYs, then we will place them.
            //if we have at least 1 friendly with no XY, then config will be used to generate that type of ship.

            List<SectorDef> playerShips = sectorDefs.PlayerShips().ToList();

            if (playerShips.Any())
            {
                try
                {
                    this.SetUpPlayerShip(playerShips.Single()); //todo: this will eventually change

                    Sector sectorToPlaceShip = Sector.Get(this.Regions.GetActive().Sectors, this.Playership.Sector.X, this.Playership.Sector.Y);

                    //This places our newly created ship into our newly created List of Regions.
                    sectorToPlaceShip.Item = SectorItem.PlayerShip;
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
                //this.Regions[0].Active = true;
                this.Regions[0].SetActive();
            }
        }

        //Creates a 2D array of Regions.  This is how all of our game pieces will be moving around.
        public void InitializeRegionsWithBaddies(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, SectorDefs sectorDefs, bool generateWithNebulae)
        {
            this.Regions = new Regions(this, this.Write);

            //Friendlies are added separately
            List<Sector> itemsToPopulateThatAreNotPlayerShip = sectorDefs.ToSectors(this.Regions).Where(q => q.Item != SectorItem.PlayerShip).ToList();

            this.Write.DebugLine("ItemsToPopulate: " + itemsToPopulateThatAreNotPlayerShip.Count + " Regions: " + this.Regions.Count);
            
            //todo: this can be done with a single loop populating a list of XYs
            this.GenerateSquareGalaxy(names, baddieNames, stockBaddieFaction, itemsToPopulateThatAreNotPlayerShip, generateWithNebulae);
        }

        public void GenerateSquareGalaxy(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, List<Sector> itemsToPopulate, bool generateWithNebula)
        {
            if (DEFAULTS.Region_MAX == 0)
            {
                throw new GameException("No Regions to set up.  Region_MAX set to Zero");
            }

            for (var RegionX = 0; RegionX < DEFAULTS.Region_MAX; RegionX++) //todo: app.config
            {
                for (var RegionY = 0; RegionY < DEFAULTS.Region_MAX; RegionY++)
                {
                    int index;
                    var newRegion = new Region(this);
                    var RegionXY = new Coordinate(RegionX, RegionY);

                    bool isNebulae = false;
                    if (generateWithNebula)
                    {
                        isNebulae = Utility.Utility.Random.Next(11) == 10; //todo pull this setting from config
                    }

                    newRegion.Create(names, baddieNames, stockBaddieFaction, RegionXY, out index, itemsToPopulate,
                                       this.GameConfig.AddStars, isNebulae);

                    newRegion.Game = this.Game;

                    this.Regions.Add(newRegion);

                    if (DEFAULTS.DEBUG_MODE)
                    {
                        this.Write.SingleLine(this.Config.GetSetting<string>("DebugAddingNewRegion"));

                        this.Write.DisplayPropertiesOf(newRegion);

                        //TODO: each object within Region needs a .ToString()

                        this.Write.Line("");
                    }
                }
            }
        }

        public IEnumerable<Sector> AddStarbases()
        {
            throw new NotImplementedException();
        }

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

        //todo: refactor these to a setup object
        public void SetUpPlayerShip(SectorDef playerShipDef)
        {
            this.Write.DebugLine(this.Config.GetSetting<string>("DebugSettingUpPlayership"));

            //todo: remove this requirement
            if (this.Regions.IsNullOrEmpty())
            {
                throw new GameException(this.Config.GetSetting<string>("RegionsNeedToBeSetup1"));
            }

            //todo: if playershipDef.GetFromConfig then grab info from config.  else set up with default random numbers.

            string playerShipName = this.Config.GetSetting<string>("PlayerShip");

            var startingSector = new Sector(new LocationDef(playerShipDef.RegionDef, new Coordinate(playerShipDef.Sector.X, playerShipDef.Sector.Y)));

            this.Playership = new Ship(FactionName.Federation, playerShipName, startingSector, this, this.Game)
            {
                Allegiance = Allegiance.GoodGuy,
                Energy = this.Config.GetSetting<int>("energy")
            };

            this.SetupPlayershipRegion(playerShipDef);

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

        public void SetupPlayershipRegion(SectorDef playerShipDef)
        {
            if (playerShipDef.RegionDef == null)
            {
                playerShipDef.RegionDef = Coordinate.GetRandom();
            }

            if(!this.Regions.Any())
            {
                throw new ArgumentException(this.Config.GetSetting<string>("RegionsNotSetUp"));
            }

            Region regionWithPlayershipDef = this.Regions.Single(q => q.X == playerShipDef.RegionDef.X && q.Y == playerShipDef.RegionDef.Y);
            this.Playership.Coordinate = new Coordinate(regionWithPlayershipDef.X, regionWithPlayershipDef.Y);

            this.Playership.GetRegion().SetActive();
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
                    Sector gotSector = Sectors.GetNoError(x, y, sectors);

                    if (gotSector?.Item == SectorItem.Starbase)
                    {
                        return true;
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

        public SectorItem GetItem(int RegionX, int RegionY, int sectorX, int sectorY)
        {
            var item = this.Get(RegionX, RegionY, sectorX, sectorY).Item;
            return item;
        }

        public Sector Get(int RegionX, int RegionY, int sectorX, int sectorY)
        {
            var item = this.Regions.Single(q => q.X == RegionX &&
                                                  q.Y == RegionY).Sectors.Single(s => s.X == sectorX &&
                                                                                        s.Y == sectorY);
            return item;
        }

        public void RemoveAllDestroyedShips(IMap map, IEnumerable<IShip> destroyedShips)
        {
            map.Regions.Remove(destroyedShips);
            map.Regions.GetActive().GetHostiles().RemoveAll(s => s.Destroyed);
        }
        public void RemoveDestroyedShipsAndScavenge(List<IShip> destroyedShips)
        {
            this.RemoveAllDestroyedShips(this, destroyedShips); //remove from Hostiles collection

            foreach (var destroyedShip in destroyedShips)
            {
                this.Playership.Scavenge(destroyedShip.Faction == FactionName.Federation
                    ? ScavengeType.FederationShip
                    : ScavengeType.OtherShip);

                //todo: add else if for starbase when the time comes
            }

            this.Playership.UpdateDivinedSectors();
        }

        public void RemoveTargetFromSector(IMap map, IShip ship)
        {
            map.Regions.Remove(ship);
        }

        /// <summary>
        ///  Removes all friendlies fromevery sector in the entire map.
        ///  zips through all sectors in all Regions.  remove any friendlies
        ///  This is a bit of a brute force approach, and not preferred, as it disguises any bugs that might have to do with forgetting
        ///  to remove the ship at the right time.  This function will need to go away or stop being used when or if this game is modified
        ///  to have multiple friendlies, as is the eventual plan.
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
        public void SetPlayershipInActiveSector(IMap map)
        {
            this.RemovePlayership(map);

            var activeRegion = map.Regions.GetActive();

            var newActiveSector = Sector.Get(activeRegion.Sectors, map.Playership.Sector.X, map.Playership.Sector.Y);
            newActiveSector.Item = SectorItem.PlayerShip;
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.  Sets down a friendly 
        /// </summary>
        /// <param name="shipToSet"></param>
        /// <param name="map"></param>
        /// <param name="newLocation"></param>
        public void SetPlayershipInLocation(IShip shipToSet, IMap map, Location newLocation)
        {
            this.RemovePlayership(map);

            newLocation.Region.SetActive();

            var foundSector = this.LookupSector(shipToSet.GetRegion(), newLocation);
            foundSector.Item = SectorItem.PlayerShip;

            shipToSet.Coordinate = newLocation.Region;
            shipToSet.Sector = foundSector;
        }

        private Sector LookupSector(Region oldRegion, Location newLocation)
        {
            //todo: divine where ship should be with old region and newlocation with negative numbers

            var foundSector = newLocation.Region.Sectors.Single(s => s.X == newLocation.Sector.X && s.Y == newLocation.Sector.Y);
            return foundSector;
        }

        public void AddACoupleHostileFederationShipsToExistingMap()
        {
            var federationShipNames = this.Config.FactionShips(FactionName.Federation);
            var federaleNames = new Stack<string>(federationShipNames.Shuffle());

            foreach (var Region in this.Regions)
            {
                var added = AddHostileFedToEmptyRegion(Region, federaleNames);
                if (added) break; //we found an empty Region to dump new fed ships into..
            }
        }

        private bool AddHostileFedToEmptyRegion(IRegion Region, Stack<string> federaleNames)
        {
            var hostilesInRegion = Region.GetHostiles();
            if (hostilesInRegion.Any()) //we don't want to mix with Klingons just yet..
            {
                var klingons = hostilesInRegion.Where(h => h.Faction == FactionName.Klingon);

                if (!klingons.Any())
                {
                    this.AddHostilesToRegion(Region, federaleNames);

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

            foreach (var Region in this.Regions)
            {
                if (Utility.Utility.Random.Next(5) == 1) //todo: resource this out.
                {
                    if (!Region.GetHostiles().Any()) //we don't want to mix with Klingons just yet..
                    {
                        this.AddHostilesToRegion(Region, federaleNames);
                    }
                }
            }
        }

        private void AddHostilesToRegion(IRegion Region, Stack<string> federaleNames)
        {
            var numberOfHostileFeds = Utility.Utility.Random.Next(2);

            for (int i = 0; i < numberOfHostileFeds; i++)
            {
                //todo: refactor to Region object
                ISector sectorToAddTo = this.GetUnoccupiedRandomSector(Region);
                this.AddHostileFederale(Region, sectorToAddTo, federaleNames);
            }
        }

        private ISector GetUnoccupiedRandomSector(IRegion Region)
        {
            var randomCoordinate = GetRandomCoordinate();

            ISector sector = Region.GetSector(randomCoordinate);
            var sectorIsEmpty = (sector.Item == SectorItem.Empty);

            if (!sectorIsEmpty)
            {
                sector = this.GetUnoccupiedRandomSector(Region);
            }

            return sector;
        }

        private static Coordinate GetRandomCoordinate()
        {
            var x = Utility.Utility.Random.Next(DEFAULTS.SECTOR_MIN);
            var y = Utility.Utility.Random.Next(DEFAULTS.SECTOR_MAX);

            var randomCoordinate = new Coordinate(x, y);
            return randomCoordinate;
        }

        private void AddHostileFederale(IRegion Region, ISector sector, Stack<string> federaleNames)
        {
            var newPissedOffFederale = new Ship(FactionName.Federation, federaleNames.Pop(), sector, this, this.Game);
            Shields.For(newPissedOffFederale).Energy = Utility.Utility.Random.Next(100, 500); //todo: resource those numbers out

            Region.AddShip(newPissedOffFederale, sector);

            this.Write.Line("Comm Reports a Federation starship has warped into Region: " + Region.Name);
        }

        public IEnumerable<IShip> GetAllFederationShips()
        {
            //todo: finish this.
            return new List<Ship>();
        }

        public bool OutOfBounds(Region region)
        {
            var inTheNegative = region.X < 0 || region.Y < 0;
            var maxxed = region.X == DEFAULTS.Region_MAX || region.Y == DEFAULTS.Region_MAX;

            var yOnMap = region.Y >= 0 && region.Y < DEFAULTS.Region_MAX;
            var xOnMap = region.X >= 0 && region.X < DEFAULTS.Region_MAX;

            return (inTheNegative || maxxed) && !(yOnMap && xOnMap);
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }
    }
}


//private static List<Sector> GetRegionObjects(int starbases, int HostilesToSetUp)
//{
//    var RegionObjects = new List<Sector>();

//    //get stars for Region and subtract from parameter (will be subtracted when this is hit next?)
//    //newRegion.Stars = 1 + (Utility.Random).Next(Constants.SECTOR_MAX);
//    //get hostiles for Region and subtract from big list
//    //get starbase T/F and subtract from big list
//    return RegionObjects;
//}


//public override string ToString()
//{
//    string returnVal = null;

//    //if debugMode
//    //returns the location of every single object in the map

//    //roll out every object in:
//    //this.Regions;
//    //this.GameConfig;
            

//    return returnVal;
//}

//todo: finish this
//public SectorItem GetShip(int RegionX, int RegionY, int sectorX, int sectorY)
//{
//    var t = this.Regions.Where(q => q.X == RegionX &&
//                                      q.Y == RegionY).Single().Sectors.Where(s => s.X == sectorX &&
//                                                                                    s.Y == sectorY).Single().Item;


//}

///// <summary>
///// Removes all friendlies fromevery sector in the entire map.
///// </summary>
///// <param name="map"></param>
//public void RemoveAllFriendlies(IMap map)
//{
//    var sectorsWithFriendlies = map.Regions.SelectMany(Region => Region.Sectors.Where(sector => sector.Item == SectorItem.Friendly));

//    foreach (Sector sector in sectorsWithFriendlies)
//    {
//        sector.Item = SectorItem.Empty;
//    }
//}