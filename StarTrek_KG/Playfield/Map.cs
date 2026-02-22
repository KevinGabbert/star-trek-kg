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
            public Sectors Sectors { get; set; }
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

        public Map(SetupOptions setupOptions, IInteraction write, IStarTrekKGSettings config, IGame game, FactionName defaultHostile = null)
        {
            this.Game = game;
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
                    if (setupOptions.CoordinateDefs == null)
                    {
                        //todo: then use appConfigSectorDefs  
                        throw new GameConfigException(this.Config.GetSetting<string>("NoCoordinateDefsSetUp"));
                    }

                    this.Initialize(setupOptions.CoordinateDefs, setupOptions.AddNebulae); //Playership is set up here.
                }

                //if (setupOptions.GenerateMap)
                //{
                //    //this.Sectors.PopulateSectors(setupOptions.CoordinateDefs, this);
                //}
            }
        }

        public void Initialize(CoordinateDefs sectorDefs, bool generateWithNebulae)
        {
            this.GetGlobalInfo();

            //This list should match baddie type that is created
            List<string> SectorNames = this.Config.GetStarSystems();

            this.Write.DebugLine("Got Starsystems");

            //TODO: if there are less than 64 Sector names then there will be problems..

            var names = new Stack<string>(SectorNames.Shuffle());

            var baddieShipNames = this.Config.ShipNames(this.DefaultHostile);

            this.Write.DebugLine("Got Baddies");

            //todo: modify this to populate with multiple faction types..
            var baddieNames = new Stack<string>(baddieShipNames.Shuffle());

            //todo: this just set up a "friendly"
            this.InitializeSectorsWithBaddies(names, baddieNames, this.DefaultHostile, sectorDefs, generateWithNebulae);

            this.Write.DebugLine("Intialized Sectors with Baddies");

            if (sectorDefs != null)
            {
                this.SetupPlayerShipInSectors(sectorDefs);
            }

            //Modify this to output everything
            if (DEFAULTS.DEBUG_MODE)
            {
                //TODO: write a hidden command that displays everything. (for debug purposes)

                this.Write.DisplayPropertiesOf(this.Playership); //This line may go away as it should be rolled out with a new Sector
                this.Write.Line(this.Config.GetSetting<string>("DebugModeEnd"));
                this.Write.Line("");
            }

            this.Playership?.UpdateDivinedSectors();
            this.HostilesToSetUp = this.Sectors.GetHostileCount();
        }

        public void SetupPlayerShipInSectors(CoordinateDefs sectorDefs)
        {
            //if we have > 0 friendlies with XYs, then we will place them.
            //if we have at least 1 friendly with no XY, then config will be used to generate that type of ship.

            List<CoordinateDef> playerShips = sectorDefs.PlayerShips().ToList();

            if (playerShips.Any())
            {
                try
                {
                    this.SetUpPlayerShip(playerShips.Single()); //todo: this will eventually change

                    Coordinate sectorToPlaceShip = this.Sectors.GetActive().Coordinates[this.Playership.Coordinate.X, this.Playership.Coordinate.Y];

                    //This places our newly created ship into our newly created List of Sectors.
                    sectorToPlaceShip.Item = CoordinateItem.PlayerShip;
                }
                catch (InvalidOperationException ex)
                {
                    throw new GameConfigException(this.Config.GetSetting<string>("InvalidPlayershipSetup") + ex.Message);
                }
                catch (Exception ex){ throw new GameConfigException(this.Config.GetSetting<string>("ErrorPlayershipSetup") + ex.Message); }
            }
            else
            {
                //this.Sectors[0].Active = true;
                this.Sectors[0].SetActive();
            }
        }

        //Creates a 2D array of Sectors.  This is how all of our game pieces will be moving around.
        public void InitializeSectorsWithBaddies(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, CoordinateDefs sectorDefs, bool generateWithNebulae)
        {
            this.Sectors = new Sectors(this, this.Write);

            //Friendlies are added separately
            List<Coordinate> itemsToPopulateThatAreNotPlayerShip = sectorDefs.ToCoordinates(this.Sectors).Where(q => q.Item != CoordinateItem.PlayerShip).ToList();

            this.Write.DebugLine("ItemsToPopulate: " + itemsToPopulateThatAreNotPlayerShip.Count + " Sectors: " + this.Sectors.Count);
            
            //todo: this can be done with a single loop populating a list of XYs
            this.GenerateSquareGalaxy(names, baddieNames, stockBaddieFaction, itemsToPopulateThatAreNotPlayerShip, generateWithNebulae);
        }

        public void GenerateSquareGalaxy(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, List<Coordinate> itemsToPopulate, bool generateWithNebula)
        {
            if (DEFAULTS.SECTOR_MAX == 0)
            {
                throw new GameException("No Sectors to set up.  Sector_MAX set to Zero");
            }

            for (var SectorX = 0; SectorX < DEFAULTS.SECTOR_MAX; SectorX++) //todo: app.config
            {
                for (var SectorY = 0; SectorY < DEFAULTS.SECTOR_MAX; SectorY++)
                {
                    int index;
                    var newSector = new Sector(this);
                    var SectorXY = new Point(SectorX, SectorY);

                    bool isNebulae = false;
                    if (generateWithNebula)
                    {
                        isNebulae = Utility.Utility.Random.Next(11) == 10; //todo pull this setting from config
                    }

                    newSector.Create(names, baddieNames, stockBaddieFaction, SectorXY, out index, itemsToPopulate,
                                       this.GameConfig.AddStars, isNebulae);

                    this.Sectors.Add(newSector);

                    if (DEFAULTS.DEBUG_MODE)
                    {
                        this.Write.SingleLine(this.Config.GetSetting<string>("DebugAddingNewSector"));

                        this.Write.DisplayPropertiesOf(newSector);

                        //TODO: each object within Sector needs a .ToString()

                        this.Write.Line("");
                    }
                }
            }
        }

        public IEnumerable<Coordinate> AddStarbases()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This will eventually be moved into each individual object
        /// </summary>
        public void GetGlobalInfo()
        {
            //this.Hostiles = new Hostiles(); //todo: create an initial size the same as HostilesToSetUp

            this.HostilesToSetUp = this.Config.GetSetting<int>("totalHostiles") + Utility.Utility.Random.Next(6);
            this.Stardate = this.Config.GetSetting<int>("stardate") + Utility.Utility.Random.Next(50);
            this.timeRemaining = this.Config.GetSetting<int>("timeRemaining") + Utility.Utility.Random.Next(10);
            this.starbases = this.Config.GetSetting<int>("starbases") + Utility.Utility.Random.Next(3);

            this.Text = this.Config.GetSetting<string>("CommandPrompt");

            this.Write.DebugLine("HostilesToSetUp: " + this.HostilesToSetUp);
            this.Write.DebugLine("Stardate: " + Stardate);
            this.Write.DebugLine("timeRemaining: " + this.HostilesToSetUp);
            this.Write.DebugLine("starbases: " + this.HostilesToSetUp);
        }

        //todo: refactor these to a setup object
        public void SetUpPlayerShip(CoordinateDef playerShipDef)
        {
            this.Write.DebugLine(this.Config.GetSetting<string>("DebugSettingUpPlayership"));

            //todo: remove this requirement
            if (this.Sectors.IsNullOrEmpty())
            {
                throw new GameException(this.Config.GetSetting<string>("SectorsNeedToBeSetup1"));
            }

            //todo: if playershipDef.GetFromConfig then grab info from config.  else set up with default random numbers.

            string playerShipName = this.Config.GetSetting<string>("PlayerShip");

            var startingSector = new Coordinate(new LocationDef(playerShipDef.SectorDef, new Point(playerShipDef.Coordinate.X, playerShipDef.Coordinate.Y)));

            this.Playership = new Ship(FactionName.Federation, playerShipName, startingSector, this)
            {
                Allegiance = Allegiance.GoodGuy,
                Energy = this.Config.GetSetting<int>("energy")
            };

            this.SetupPlayershipSector(playerShipDef);

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

        public void SetupPlayershipSector(CoordinateDef playerShipDef)
        {
            if (playerShipDef.SectorDef == null)
            {
                playerShipDef.SectorDef = Point.GetRandom();
            }

            if(!this.Sectors.Any())
            {
                throw new ArgumentException(this.Config.GetSetting<string>("SectorsNotSetUp"));
            }

            Sector regionWithPlayershipDef = this.Sectors.Single(q => q.X == playerShipDef.SectorDef.X && q.Y == playerShipDef.SectorDef.Y);
            this.Playership.Point = new Point(regionWithPlayershipDef.X, regionWithPlayershipDef.Y);

            this.Playership.GetSector().SetActive();
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
        public bool IsDockingLocation(int i, int j, Coordinates sectors)
        {
            //http://stackoverflow.com/questions/3150678/using-linq-with-2d-array-select-not-found
            for (int y = i - 1; y <= i + 1; y++)
            {
                for (int x = j - 1; x <= j + 1; x++)
                {
                    Coordinate gotSector = Coordinates.GetNoError(x, y, sectors);

                    if (gotSector?.Item == CoordinateItem.Starbase)
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
        //                    (badGuy.Coordinate.X), (badGuy.Coordinate.Y));
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

        public CoordinateItem GetItem(int SectorX, int SectorY, int sectorX, int sectorY)
        {
            var item = this.Get(SectorX, SectorY, sectorX, sectorY).Item;
            return item;
        }

        public Coordinate Get(int SectorX, int SectorY, int sectorX, int sectorY)
        {
            var item = this.Sectors.Single(q => q.X == SectorX &&
                                                  q.Y == SectorY).Coordinates.Single(s => s.X == sectorX &&
                                                                                        s.Y == sectorY);
            return item;
        }

        public void RemoveAllDestroyedShips(IMap map, IEnumerable<IShip> destroyedShips)
        {
            map.Sectors.Remove(destroyedShips);
            map.Sectors.GetActive().GetHostiles().RemoveAll(s => s.Destroyed);
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
            map.Sectors.Remove(ship);
        }

        /// <summary>
        ///  Removes all friendlies fromevery sector in the entire map.
        ///  zips through all sectors in all Sectors.  remove any friendlies
        ///  This is a bit of a brute force approach, and not preferred, as it disguises any bugs that might have to do with forgetting
        ///  to remove the ship at the right time.  This function will need to go away or stop being used when or if this game is modified
        ///  to have multiple friendlies, as is the eventual plan.
        /// </summary>
        /// <param name="map"></param>
        public void RemovePlayership(IMap map)
        {
            map.Playership.Coordinate.Item = CoordinateItem.Empty;
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.  Sets down a friendly 
        /// </summary>
        /// <param name="map"></param>
        public void SetPlayershipInActiveSector(IMap map)
        {
            this.RemovePlayership(map);

            var activeSector = map.Sectors.GetActive();

            var newActiveSector = activeSector.Coordinates[map.Playership.Coordinate.X, map.Playership.Coordinate.Y].Item = CoordinateItem.PlayerShip;
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

            newLocation.Sector.SetActive();

            Coordinate foundSector = this.LookupSector(shipToSet.GetSector(), newLocation);
            foundSector.Item = CoordinateItem.PlayerShip;

            shipToSet.Point = new Point(newLocation.Sector.X, newLocation.Sector.Y);
            shipToSet.Coordinate = foundSector;
        }

        private Coordinate LookupSector(Sector oldSector, Location newLocation)
        {
            //todo: divine where ship should be with old region and newlocation with negative numbers

            IEnumerable<Coordinate> matchingSectors = newLocation.Sector.Coordinates
                                                  .Where(s => s.X == newLocation.Coordinate.X && 
                                                              s.Y == newLocation.Coordinate.Y);
            Coordinate foundSector;

            try
            {
                foundSector = matchingSectors.Single();
            }
            catch (Exception)
            {
                //todo: if sector not found then this is a bug.
                throw new ArgumentException($"Coordinate {newLocation.Coordinate.X}, {newLocation.Coordinate.Y}");
            }

            return foundSector;
        }

        public void AddACoupleHostileFederationShipsToExistingMap()
        {
            List<string> federationShipNames = this.Config.ShipNames(FactionName.Federation);
            var federaleNames = new Stack<string>(federationShipNames.Shuffle());

            foreach (var region in this.Sectors)
            {
                bool added = this.AddHostileFedToEmptySector(region, federaleNames);
                if (added) break; //we found an empty Sector to dump new fed ships into..
            }
        }

        private bool AddHostileFedToEmptySector(ISector Sector, Stack<string> federaleNames)
        {
            var HostilesInSector = Sector.GetHostiles();
            if (HostilesInSector.Any()) //we don't want to mix with Klingons just yet..
            {
                var klingons = HostilesInSector.Where(h => h.Faction == FactionName.Klingon);

                if (!klingons.Any())
                {
                    this.AddHostilesToSector(Sector, federaleNames);

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
            var federationShipNames = this.Config.ShipNames(FactionName.Federation);
            var federaleNames = new Stack<string>(federationShipNames.Shuffle());

            foreach (var Sector in this.Sectors)
            {
                if (Utility.Utility.Random.Next(5) == 1) //todo: resource this out.
                {
                    if (!Sector.GetHostiles().Any()) //we don't want to mix with Klingons just yet..
                    {
                        this.AddHostilesToSector(Sector, federaleNames);
                    }
                }
            }
        }

        private void AddHostilesToSector(ISector Sector, Stack<string> federaleNames)
        {
            var numberOfHostileFeds = Utility.Utility.Random.Next(2);

            for (int i = 0; i < numberOfHostileFeds; i++)
            {
                //todo: refactor to Sector object
                ICoordinate coordinateToAddTo = this.GetUnoccupiedRandomCoordinate(Sector);
                this.AddHostileFederale(Sector, coordinateToAddTo, federaleNames);
            }
        }

        private ICoordinate GetUnoccupiedRandomCoordinate(ISector Sector)
        {
            Point randomCoordinate = GetRandomCoordinate();

            ICoordinate coordinate = Sector.GetCoordinate(randomCoordinate);
            bool sectorIsEmpty = coordinate.Item == CoordinateItem.Empty;

            if (!sectorIsEmpty)
            {
                coordinate = this.GetUnoccupiedRandomCoordinate(Sector);
            }

            return coordinate;
        }

        private static Point GetRandomCoordinate()
        {
            int x = Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MIN);
            int y = Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MAX);

            var randomCoordinate = new Point(x, y);
            return randomCoordinate;
        }

        private void AddHostileFederale(ISector Sector, ICoordinate coordinate, Stack<string> federaleNames)
        {
            var newPissedOffFederale = new Ship(FactionName.Federation, federaleNames.Pop(), coordinate, this);
            Shields.For(newPissedOffFederale).Energy = Utility.Utility.Random.Next(100, 500); //todo: resource those numbers out

            Sector.AddShip(newPissedOffFederale, coordinate);

            this.Write.Line("Comm Reports a Federation starship has warped into Sector: " + Sector.Name);
        }

        public IEnumerable<IShip> GetAllFederationShips()
        {
            //todo: finish this.
            return new List<Ship>();
        }

        public bool OutOfBounds(Sector region)
        {
            bool result;

            if (region != null)
            {
                bool inTheNegative = region.X < 0 || region.Y < 0;
                bool maxxed = region.X == DEFAULTS.SECTOR_MAX || region.Y == DEFAULTS.SECTOR_MAX;

                bool yOnMap = region.Y >= 0 && region.Y < DEFAULTS.SECTOR_MAX;
                bool xOnMap = region.X >= 0 && region.X < DEFAULTS.SECTOR_MAX;

                result = (inTheNegative || maxxed) && !(yOnMap && xOnMap);
            }
            else
            {
                result = true;
            }

            return result;
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }
    }
}


//private static List<Coordinate> GetSectorObjects(int starbases, int HostilesToSetUp)
//{
//    var SectorObjects = new List<Coordinate>();

//    //get stars for Sector and subtract from parameter (will be subtracted when this is hit next?)
//    //newSector.Stars = 1 + (Utility.Random).Next(Constants.SECTOR_MAX);
//    //get hostiles for Sector and subtract from big list
//    //get starbase T/F and subtract from big list
//    return SectorObjects;
//}


//public override string ToString()
//{
//    string returnVal = null;

//    //if debugMode
//    //returns the location of every single object in the map

//    //roll out every object in:
//    //this.Sectors;
//    //this.GameConfig;
            

//    return returnVal;
//}

//todo: finish this
//public CoordinateItem GetShip(int SectorX, int SectorY, int sectorX, int sectorY)
//{
//    var t = this.Sectors.Where(q => q.X == SectorX &&
//                                      q.Y == SectorY).Single().Coordinates.Where(s => s.X == sectorX &&
//                                                                                    s.Y == sectorY).Single().Item;


//}

///// <summary>
///// Removes all friendlies fromevery sector in the entire map.
///// </summary>
///// <param name="map"></param>
//public void RemoveAllFriendlies(IMap map)
//{
//    var sectorsWithFriendlies = map.Sectors.SelectMany(Sector => Sector.Coordinates.Where(sector => sector.Item == CoordinateItem.Friendly));

//    foreach (Coordinate sector in sectorsWithFriendlies)
//    {
//        sector.Item = CoordinateItem.Empty;
//    }
//}


