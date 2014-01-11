﻿using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Utility;

namespace StarTrek_KG.Playfield
{
    public class Map : IWrite
    {
        #region Properties

            public Write Write { get; set; }

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

        public Map(GameConfig setupOptions, Write write)
        {
            this.Write = write;
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

            this.Write.DebugLine("Got Starsystems");

            //TODO: if there are less than 64 quadrant names then there will be problems..

            var names = new Stack<string>(quadrantNames.Shuffle());

            var klingonShipNames = StarTrekKGSettings.GetShips("Klingon");

            this.Write.DebugLine("Got Baddies");

            var baddieNames = new Stack<string>(klingonShipNames.Shuffle());

            //todo: this just set up a "friendly"
            this.InitializeQuadrantsWithBaddies(names, baddieNames, sectorDefs);

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
                this.Write.Line(StarTrekKGSettings.GetSetting<string>("DebugModeEnd"));
                this.Write.Line("");
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
            this.Quadrants = new Quadrants(this, this.Write);

            //Friendlies are added separately
            List<Sector> itemsToPopulateThatAreNotFriendlies = sectorDefs.ToSectors(this.Quadrants).Where(q => q.Item != SectorItem.Friendly).ToList();

            this.Write.DebugLine("ItemsToPopulate: " + itemsToPopulateThatAreNotFriendlies.Count + " Quadrants: " + this.Quadrants.Count);
            
            //todo: this can be done with a single loop populating a list of XYs
            this.GenerateSquareGalaxy(names, baddieNames, itemsToPopulateThatAreNotFriendlies);
        }

        private void GenerateSquareGalaxy(Stack<string> names, Stack<string> baddieNames, List<Sector> itemsToPopulate)
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
                    var newQuadrant = new Quadrant(this.Write);
                    var quadrantXY = new Coordinate(quadrantX, quadrantY);

                    newQuadrant.Create(this, names, baddieNames, quadrantXY, out index, itemsToPopulate,
                                       this.GameConfig.AddStars);
                    this.Quadrants.Add(newQuadrant);

                    if (Constants.DEBUG_MODE)
                    {
                        this.Write.SingleLine(StarTrekKGSettings.GetSetting<string>("DebugAddingNewQuadrant"));

                        this.Write.DisplayPropertiesOf(newQuadrant);

                        //TODO: each object within quadrant needs a .ToString()

                        this.Write.Line("");
                    }
                }
            }
        }

        private IEnumerable<Sector> AddStarbases()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Sector> AddStars(Quadrant quadrant, int totalStarsInQuadrant)
        {
            Utility.Utility.ResetGreekLetterStack();

            this.CreateStar(quadrant, totalStarsInQuadrant);

            return quadrant.Sectors.Where(s => s.Item == SectorItem.Star);
        }

        public Sector AddStar(Quadrant quadrant)
        {
            Utility.Utility.ResetGreekLetterStack();

            const int totalStarsInQuadrant = 1;

            var currentStarName = this.CreateStar(quadrant, totalStarsInQuadrant);

            return quadrant.Sectors.Single(s => s.Item == SectorItem.Star && ((Star)s.Object).Name == currentStarName);
        }

        private string CreateStar(Quadrant quadrant, int totalStarsInQuadrant)
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

                            var starsInQuadrant = quadrant.Sectors.Where( s => s.Object != null && s.Object.Type.Name == "Star").ToList();
                            var allStarsDontHaveNewDesignation = starsInQuadrant.All(s => ((Star) s.Object).Designation != newNameLetter);

                            if (allStarsDontHaveNewDesignation)
                            {
                                foundStarName = true;
                                currentStarName = quadrant.Name.ToUpper() + " " + newNameLetter;

                                newStar.Name = currentStarName;
                                newStar.Designation = newNameLetter;
                                sector.Item = SectorItem.Star;

                                sector.Object = newStar;
                                totalStarsInQuadrant--;
                            }

                            //Assuming we are using the greek alphabet for star names, we don't want to create a lockup.
                            if(counter > 25)
                            {
                               this.Write.Line("Too Many Stars.  Sorry.  Not gonna create more.");
                               foundStarName = true;
                            }
                        }
                    }
                }
            }
            return currentStarName;
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

            this.Write.DebugLine("HostilesToSetUp: " + hostilesToSetUp);
            this.Write.DebugLine("Stardate: " + Stardate);
            this.Write.DebugLine("timeRemaining: " + hostilesToSetUp);
            this.Write.DebugLine("starbases: " + hostilesToSetUp);
        }

        //refactor these to a setup object
        private void SetUpPlayerShip(SectorDef playerShipDef)
        {
            this.Write.DebugLine(StarTrekKGSettings.GetSetting<string>("DebugSettingUpPlayership"));

            //todo: remove this requirement
            if (this.Quadrants == null)
            {
                throw new GameException(StarTrekKGSettings.GetSetting<string>("QuadrantsNeedToBeSetup1"));
            }

            //todo: if playershipDef.GetFromConfig then grab info from config.  else set up with default random numbers.

            var playerShipName = StarTrekKGSettings.GetSetting<string>("PlayerShip");

            var startingSector = new Sector(new LocationDef(playerShipDef.QuadrantDef, new Coordinate(playerShipDef.Sector.X, playerShipDef.Sector.Y)));

            this.Playership = new Ship(playerShipName, this, startingSector, this.Write)
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
            this.SetupPlayershipTorpedoes();

            ShortRangeScan.For(this.Playership).Damage = 0;
            ShortRangeScan.For(this.Playership).ShipConnectedTo = this.Playership;

            LongRangeScan.For(this.Playership).Damage = 0;
            LongRangeScan.For(this.Playership).ShipConnectedTo = this.Playership;

            Computer.For(this.Playership).Damage = 0;
            Computer.For(this.Playership).ShipConnectedTo = this.Playership;

            Phasers.For(this.Playership).Damage = 0;
            Phasers.For(this.Playership).ShipConnectedTo = this.Playership;
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
                playerShipDef.QuadrantDef = Coordinate.GetRandom();
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

            torpedoes.ShipConnectedTo = this.Playership;
            torpedoes.Count = StarTrekKGSettings.GetSetting<int>("photonTorpedoes");
            torpedoes.Damage = 0;
        }
        private void SetupPlayershipShields()
        {
            var starshipShields = Shields.For(this.Playership);

            starshipShields.ShipConnectedTo = this.Playership;
            starshipShields.Energy = 0;
            starshipShields.Damage = 0;
        }
        private void SetupPlayershipNav()
        {
            var starshipNAV = Navigation.For(this.Playership);

            starshipNAV.ShipConnectedTo = this.Playership;
            starshipNAV.Movement.ShipConnectedTo = this.Playership;
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

        public void RemoveTargetFromSector(Map map, IShip ship)
        {
            map.Quadrants.Remove(ship, map);
        }

        //todo: finish this
        //public SectorItem GetShip(int quadrantX, int quadrantY, int sectorX, int sectorY)
        //{
        //    var t = this.Quadrants.Where(q => q.X == quadrantX &&
        //                                      q.Y == quadrantY).Single().Sectors.Where(s => s.X == sectorX &&
        //                                                                                    s.Y == sectorY).Single().Item;


        //}

        //todo: this should be part of the computer object
        public void StarbaseCalculator(Ship shipConnectedTo)
        {
            //var navigation = Navigation.For(shipConnectedTo);

            var mySector = shipConnectedTo.Sector;

            var thisQuadrant = shipConnectedTo.GetQuadrant();

            var starbasesInSector = thisQuadrant.Sectors.Where(s => s.Item == SectorItem.Starbase).ToList();

            if (starbasesInSector.Any())
            {
                foreach (var starbase in starbasesInSector)
                {
                    this.Write.Line("-----------------");
                    this.Write.Line(string.Format("Starbase in sector [{0},{1}].", (starbase.X + 1), (starbase.Y + 1)));

                    this.Write.Line(string.Format("Direction: {0:#.##}", Utility.Utility.ComputeDirection(mySector.X, mySector.Y, starbase.X, starbase.Y)));
                    this.Write.Line(string.Format("Distance:  {0:##.##}", Utility.Utility.Distance(mySector.X, mySector.Y, starbase.X, starbase.Y) / Constants.SECTOR_MAX));
                }
            }
            else
            {
                this.Write.Line("There are no starbases in this quadrant.");
            }
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.
        /// </summary>
        /// <param name="map"></param>
        public void RemoveAllFriendlies(Map map)
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
        public void SetFriendly(Map map)
        {
            //zip through all sectors in all quadrants.  remove any friendlies

            //This is a bit of a brute force approach, and not preferred, as it disguises any bugs that might have to do with forgetting
            //to remove the ship at the right time.  This function will need to go away or stop being used when or if this game is modified
            //to have multiple friendlies, as is the eventual plan.

            this.RemoveAllFriendlies(map);

            var activeQuadrant = map.Quadrants.GetActive();

            var newActiveSector = Sector.Get(activeQuadrant.Sectors, map.Playership.Sector.X, map.Playership.Sector.Y);
            newActiveSector.Item = SectorItem.Friendly;
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
