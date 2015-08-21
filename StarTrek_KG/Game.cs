﻿using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;
using W = StarTrek_KG.Output.Write;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG
{
    public class Game : IDisposable, IWrite, IConfig
    {
        #region Properties

            public IStarTrekKGSettings Config { get; set; }
            public IOutputWrite Write { get; set; }

            public Write Output { get; set; }
            public Render PrintSector { get; set; }
            public IMap Map { get; set; }

            public bool PlayerNowEnemyToFederation { get; set; } //todo: temporary until Starbase object is created
            public List<FactionThreat> LatestTaunts { get; set; } //todo: temporary until proper object is created
            public bool gameOver;
            public int RandomFactorForTesting 
            { 
                get; 
                set; 
            }

            public delegate TResult PromptFunc<T, out TResult>(T input, out T output);

        #endregion

        /// <summary>
        /// todo: all game workflow functions go here (currently, workflow is ensconced within actors)
        /// and some unsorted crap at the moment..
        /// </summary>
        public Game(IStarTrekKGSettings config, bool startup = true)
        {
            this.RandomFactorForTesting = 0;
            this.PlayerNowEnemyToFederation = false;  //todo: resource this out.
            this.Config = config;
            if(this.Write == null)
            {
                this.Write = new Write(config);
            }

            if (startup)
            {
                //The config file is loaded here, and persisted through the rest of the game. 
                //Any settings that are not in the config at this point, will not be updated unless some fault tolerance is built in that
                //might try to reload the file. #NotInThisVersion
                this.Config.Get = this.Config.GetConfig();

                this.LatestTaunts = new List<FactionThreat>();

                //These constants need to be localized to Game:
                this.GetConstants();

                this.PrintSector =
                    (new Render(this.Write, this.Config));

                var startConfig = (new SetupOptions
                                       {
                                           Initialize = true,
                                           AddNebulae = true,
                                           SectorDefs = SectorSetup()
                                       });

                this.Write = new Write(this.Config); 
                this.Map = new Map(startConfig, this.Write, this.Config);
                this.Write = new Write(this.Config);

                //We don't want to start game without hostiles
                if (this.HostileCheck(this.Map))
                    return; //todo: unless we want to have a mode that allows it for some reason.

                //Set initial color scheme
                this.Write.HighlightTextBW(false);

                //todo: why are we creating this PrintSector() class a second time??
                this.Output = new Write(this.Map.HostilesToSetUp, Map.starbases, Map.Stardate, Map.timeRemaining, this.Config);   
                this.PrintSector = new Render(this.Write, this.Config);
            }
        }

        private void GetConstants()
        {
            Constants.DEBUG_MODE = this.Config.GetSetting<bool>("DebugMode");

            if (Constants.DEBUG_MODE)
            {
                this.Write.Line("// ---------------- Debug Mode ----------------");
            }

            Constants.SECTOR_MIN = this.Config.GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = this.Config.GetSetting<int>("SECTOR_MAX");

            Constants.Region_MIN = this.Config.GetSetting<int>("Region_MIN");
            Constants.Region_MAX = this.Config.GetSetting<int>("RegionMax");

            Constants.SHIELDS_DOWN_LEVEL = this.Config.GetSetting<int>("ShieldsDownLevel");
            Constants.LOW_ENERGY_LEVEL = this.Config.GetSetting<int>("LowEnergyLevel");  
        }

        private SectorDefs SectorSetup()
        {

            //todo: these SectorDefs can be computed somewhere
            //todo: this make a GetSectorDefsFromAppConfig()
                //todo: Output a message if GetSectorDefsFromAppConfig() fails, then use hardcoded setup and start game anyway

            return DefaultHardcodedSetup();
        }

        /// <summary>
        /// This is the setup we get if app config can not be read for some reason (or it is buggy)
        /// </summary>
        /// <returns></returns>
        private SectorDefs DefaultHardcodedSetup()
        {

            //todo: get rid of this.  generate on the fly!

            return new SectorDefs
                       {
                           //This tells us what Types of items will be generated at start.  if Coordinates are passed, that is an
                           //indicator that an individual object needs to be placed, istead of generated objects from config file.

                           //todo: get rid of that second, stupid parameter.
                           new SectorDef(SectorItem.PlayerShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.HostileShip),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Starbase),
                           new SectorDef(SectorItem.Star),
                       };
        }

        //private bool HostileCheck(GameConfig startConfig)
        //{
        //    if (startConfig.SectorDefs.GetHostiles().Count() < 1)
        //    {
        //        Output.WriteLine("ERROR: --- No Hostiles have been set up.");
                
        //        //todo: perhaps we'd have a reason to make a "freeform" option or mode where you could practice shooting things, moving, etc.
        //        //todo: in that case, this function would not be called

        //        this.gameOver = true;
        //        return true;
        //    }
        //    return false;
        //}

        private bool HostileCheck(IMap map)
        {
            if (!map.Regions.GetHostiles().Any())
            {
                this.Write.Line("ERROR: --- No Hostiles have been set up.");

                //todo: perhaps we'd have a reason to make a "freeform" option or mode where you could practice shooting things, moving, etc.
                //todo: in that case, this function would not be called

                this.gameOver = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts the game.  Repeats indefinitely (as the original did) if App.config is set to do so.
        /// </summary>
        public void Run()
        {
            bool keepPlaying = true;

            while (keepPlaying)
            {
                keepPlaying = this.Config.GetSetting<bool>("KeepPlaying");

                this.PlayOnce();
                this.gameOver = false;
            }
        }

        /// <summary>
        /// Prints title and sets up the playfield.
        /// This is where the Map is created, and references to it are passed around from here on.
        /// </summary>
        private void PrintOpeningScreen()
        {
            this.RandomAppTitle(); //Printing the title at this point is really a debug step. (it shows that the game is started.  Otherwise, it could go after initialization)

            this.Write.ResourceLine(this.Config.GetText("AppVersion").TrimStart(' '), "UnderConstructionMessage");

            Output.PrintMission();
        }

        public void RandomAppTitle()
        {
            int randomVal = Utility.Utility.Random.Next(3);

            switch (randomVal)
            {
                case 0:
                    this.AppTitleItem("Classic", 7);
                    break;

                case 2:
                    this.AppTitleItem("TNG", 7);
                    break;

                default:
                    this.AppTitleItem("Movie", 7);
                    break;
            }

            this.Write.Resource("AppTitleSpace");

            RandomPicture();

            this.Write.Resource("AppTitleSpace");
        }

        private void RandomPicture()
        {
            Utility.Utility.Random = new Random(Guid.NewGuid().GetHashCode());
            int randomVal = Utility.Utility.Random.Next(150);
            switch (randomVal)
            {
                case 1:
                    AppTitleItem("ExcelsiorMedium", 8);
                    break;

                case 2:
                    AppTitleItem("DaedalusSmall", 8);
                    break;

                case 3:
                    AppTitleItem("Reliant", 8);
                    break;

                case 4:
                    AppTitleItem("D7Front", 6);
                    break;

                case 5:
                    AppTitleItem("D-10-", 6);
                    break;

                case 6:
                    AppTitleItem("D-4-", 7);
                    break;

                case 7:
                    AppTitleItem("D-11-", 6);
                    break;

                case 8:
                    AppTitleItem("D-18-", 6);
                    break;

                case 9:
                    AppTitleItem("D-27-", 7);
                    break;

                case 10:
                    AppTitleItem("AkulaSmall", 7);
                    break;

                case 11:
                    AppTitleItem("BattlecruiserSmall", 6);
                    break;

                case 12:
                    AppTitleItem("SaladinSmall", 6);
                    break;

                case 13:
                    AppTitleItem("EagleSmall", 6);
                    break;

                case 14:
                    AppTitleItem("DreadnaughtSide", 9);
                    break;

                case 15:
                    AppTitleItem("Enterprise-BSmall", 6);
                    break;

                case 16:
                    AppTitleItem("ExcelsiorSmall", 6);
                    break;

                case 17:
                    AppTitleItem("RomulanBOP", 8);
                    break;

                default:
                    AppTitleItem("2ShipsSmall", 7);
                    break;
            }
        }

        private void AppTitleItem(string itemName, int endingLine)
        {
            for (int i = 1; i < endingLine; i++)
            {
                this.Write.Resource("AppTitle" + itemName + i);
            }
        }


        /// <summary>
        /// Game ends when user runs out of power, wins, or is destroyed
        /// </summary>
        private void PlayOnce()
        {
            if(this.gameOver)
            {
                this.Write.DebugLine("Game Over.");
                return;
            }

            this.PrintOpeningScreen();

            while (!gameOver)
            {
                this.gameOver = this.NewTurn(); //Shows Command Prompt

                if (gameOver)
                {
                    this.Write.DebugLine("Game Over.. Restarting.");

                    //TODO:  we can possibly reorder the baddies in this.Map.GameConfig..
                    this.Map.Initialize(this.Map.GameConfig.SectorDefs, this.Map.GameConfig.AddNebulae); //we gonna start over

                    break;
                }
            }
        }

        /// <summary>
        /// A turn is represented as each time the Console requires a new command from the user
        /// This does not include sub-commands, such as adding power to the shields, etc.
        /// </summary>
        /// <returns></returns>
        private bool NewTurn()
        {
            this.Write.Prompt(this.Map.Playership, this.Map.Text, this);

            //todo: move this to Console app.//Have Game expose and raise a CommandPrompt event.  //Have console subscribe to that event

            var starbasesLeft = this.Map.Regions.GetStarbaseCount();

            if (this.PlayerNowEnemyToFederation)
            {
                this.gameOver = (this.Map.timeRemaining < 1 ||
                                 starbasesLeft < 1 ||
                                 this.Map.Playership.Destroyed ||
                                 this.Map.Playership.Energy < 1);
            }
            else
            {
                this.gameOver = !(this.Map.Playership.Energy > 0 &&
                                !this.Map.Playership.Destroyed &&
                                (this.Map.Regions.GetHostileCount() > 0) &&                              
                                this.Map.timeRemaining > 0);
            }

            Output.PrintCommandResult(this.Map.Playership, this.PlayerNowEnemyToFederation, starbasesLeft);
            return gameOver;
        }

        public void MoveTimeForward(IMap map, Coordinate lastRegion, Coordinate Region)
        {
            if (lastRegion.X != Region.X || lastRegion.Y != Region.Y)
            {
                map.timeRemaining--;
                map.Stardate++;
            }
        }

        public void Dispose()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.Region_MIN = 0;
            Constants.Region_MAX = 0;
        }

        /// <summary>
        /// At the end of each turn, if a hostile is in the same sector with Playership, it will attack.  If there are 37, then all 37 will..
        /// TODO: this needs to be changed.  after destruction, it appears to take several method returns to realize that we are dead.
        /// </summary>
        /// <returns></returns>
        public bool ALLHostilesAttack(IMap map)
        {
            //todo: centralize this.
            //this is called from torpedo control/phaser control, and navigation control

            var returnValue = false;
            var activeRegion = map.Regions.GetActive();
            var hostilesAttacking = activeRegion.GetHostiles();

            this.HostileStarbasesAttack(map, activeRegion);

            returnValue = this.HostileShipsAttack(map, hostilesAttacking, returnValue);

            return returnValue;
        }

        private bool HostileShipsAttack(IMap map, ICollection<IShip> hostilesAttacking, bool returnValue)
        {
            if (hostilesAttacking?.Count > 0)
            {
                foreach (var badGuy in hostilesAttacking)
                {
                    int randomHostileAttacksFactor = Utility.Utility.TestableRandom(this); //this.RandomFactorForTesting == 0 ? Utility.Utility.Random.Next() : this.RandomFactorForTesting;

                    this.HostileAttacks(map, badGuy, randomHostileAttacksFactor);
                }

                this.EnemiesWillNowTaunt();

                returnValue = true;
            }
            return returnValue;
        }

        private void HostileStarbasesAttack(IMap map, IRegion activeRegion)
        {
            if (this.PlayerNowEnemyToFederation)
            {
                if (activeRegion.Type != RegionType.Nebulae) //starbases don't belong in Nebulae.  If some dummy put one here intentionally, then it will do no damage.  Why? because if you have no shields, a hostile starbase will disable you with the first shot and kill you with the second. 
                {
                    var starbasesAttacking = activeRegion.GetStarbaseCount();

                    for (int i = 0; i < starbasesAttacking; i++)
                    {
                        int hostileStarbaseAttacksRandom = Utility.Utility.TestableRandom(this);  //this.RandomFactorForTesting == 0 ? Utility.Utility.Random.Next() : this.RandomFactorForTesting;
                        
                        //todo: modify starbase to be its own ship object on the map
                        //HACK: this is a little bit of a cheat, saying that the playership is attacking itself, but until the starbase is its own object, this should be fine
                        this.HostileAttacks(map, map.Playership, hostileStarbaseAttacksRandom);

                        int hostileStarbaseAttacksRandom2 = Utility.Utility.TestableRandom(this); //this.RandomFactorForTesting == 0 ? Utility.Utility.Random.Next() : this.RandomFactorForTesting;

                        //cause starbases are bastards like that.  hey.. You started it!
                        this.HostileAttacks(map, map.Playership, hostileStarbaseAttacksRandom2);

                        //todo: when starbases are their own object, they will fire once.. it will just hurt more.
                    }
                }
                else
                {
                    this.Write.Line("Hostile Starbase fires blindly, unable to get a lock on your position in Nebula.");
                }
            }
        }

        private void HostileAttacks(IMap map, IShip badGuy, int randomFactor)
        {
            if (Navigation.For(map.Playership).Docked && !this.PlayerNowEnemyToFederation)
            {
                this.AttackDockedPlayership(badGuy, 0);
            }
            else
            {
                this.AttackNonDockedPlayership(map, badGuy, randomFactor);
            }
        }

        private void AttackNonDockedPlayership(IMap map, IShip badGuy, int randomFactor)
        {
            var playerShipLocation = map.Playership.GetLocation();
            var distance = Utility.Utility.Distance(playerShipLocation.Sector.X,
                                                    playerShipLocation.Sector.Y,
                                                    badGuy.Sector.X,
                                                    badGuy.Sector.Y);

            int disruptorShotSeed = this.Config.GetSetting<int>("DisruptorShotSeed");

            //todo: randomFactor is blowing out the top of the int
            int seedEnergyToPowerWeapon = disruptorShotSeed; // * (randomFactor/5);

            var inNebula = badGuy.GetRegion().Type == RegionType.Nebulae;

            //Todo: this should be Disruptors.For(this.ShipConnectedTo).Shoot()
            //todo: the -1 should be the ship energy you want to allocate
            var attackingEnergy = (int)Utility.Utility.ShootBeamWeapon(seedEnergyToPowerWeapon, distance, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", inNebula); 

            var shieldsValueBeforeHit = Shields.For(map.Playership).Energy;

            map.Playership.AbsorbHitFrom(badGuy, attackingEnergy);

            this.ReportShieldsStatus(map, shieldsValueBeforeHit);
        }

        private void AttackDockedPlayership(IShip attacker, int attackingEnergy)
        {
            string hitMessage = W.ShipHitMessage(attacker, attackingEnergy);

            this.Write.Line(hitMessage + " No damage due to starbase shields.");
        }

        //todo: move this to a report object?
        public void ReportShieldsStatus(IMap map, int shieldsValueBeforeHit)
        {
            var shieldsValueAfterHit = Shields.For(map.Playership).Energy;

            if (shieldsValueAfterHit <= shieldsValueBeforeHit)
            {
                if (shieldsValueAfterHit == 0)
                {
                    this.Write.SingleLine("** Shields are Down **");
                }
                else
                {
                    this.Write.SingleLine($"Shields dropped to {Shields.For(map.Playership).Energy}.");
                }
            }
        }

        /// <summary>
        /// All enemies in PlayerShip's Region shall now commence to unclog their noses in the general direction of the player.
        /// </summary>
        public void EnemiesWillNowTaunt()
        {
            //todo: move this to communications subsystem eventually
            var currentRegion = this.Map.Playership.GetRegion();
            var hostilesInRegion = currentRegion.GetHostiles();
            string currentThreat = "";

            this.LatestTaunts = new List<FactionThreat>();

            foreach (var ship in hostilesInRegion)
            {
                bool tauntLikely = Utility.Utility.Random.Next(5) == 1; //todo: resource this out.

                if (tauntLikely)
                {
                    currentThreat = SingleEnemyTaunt(ship, currentThreat);
                }
            }
        }

        /// <summary>
        /// this is just a bit inefficient, but the way to fix it is to have a refactor.  It works for now
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="currentThreat"></param>
        /// <returns></returns>
        private string SingleEnemyTaunt(IShip ship, string currentThreat)
        {
            var currentFaction = ship.Faction;

            if (currentFaction == null)
            {
                throw new GameException("null faction for taunt");
            }

            string currentShipName = null;

            this.Write.Line("");

            if (currentFaction == FactionName.Federation)
            {
                //"NCC-500 U.S.S. Saladin  Saladin-class"
                //"NCC-500 U.S.S. FirstName SecondName  Saladin-class"

                currentShipName = ship.Name;
            }
            else if (currentFaction == FactionName.Klingon)
            {
                this.Write.WithNoEndCR(
                    $"Klingon ship at {"[" + ship.Sector.X + "," + ship.Sector.Y + "]"} sends the following message: ");
            }
            else
            {
                this.Write.WithNoEndCR(
                    $"Hostile at {"[" + ship.Sector.X + "," + ship.Sector.Y + "]"} sends the following message: ");
                currentShipName = ship.Name;
            }

            FactionThreat randomThreat = this.Config.GetThreats(currentFaction).Shuffle().First();

            currentThreat += String.Format(randomThreat.Threat, currentShipName);

            this.LatestTaunts.Add(randomThreat);

            this.Write.Line(currentThreat);
            return currentThreat;
        }

        //public static string GetFederationShipName(IShip ship)
        //{
        //    string currentShipName = ship.Name;

        //    if (currentShipName == "Starbase")
        //    {
        //        return currentShipName;
        //    }

        //    if (currentShipName == "Enterprise")
        //    {
        //        return "Starbase";
        //    }

        //    try
        //    {
        //        int USS = ship.Name.IndexOf("U.S.S. ");
        //        int spaceAfterGivenName = 0;

        //        var nameLength = ship.Name.Length;

        //        for (int i = nameLength; i > 0; i--)
        //        {
        //            var currentChar = ship.Name.Substring(i - 1, 1);
        //            if (currentChar == " ")
        //            {
        //                spaceAfterGivenName = i;
        //                break;
        //            }
        //        }

        //        currentShipName = ship.Name.Substring(USS, spaceAfterGivenName - USS).Trim();
        //        return currentShipName;
        //    }
        //    catch (Exception)
        //    {
        //        //HACK: At present, starbase name won't parse because I have it so that you are shooting yourself.  :D
        //        //todo: make starbase its own object
        //        //if (currentShipName == this.Map.Playership.Name)
        //        //{
        //            ship.Name = "Unknown";
        //        //}
        //        //else
        //        //{
        //        //    //yeah, something else broke.  tell the world.
        //        //    throw;
        //        //}
        //    }

        //    return currentShipName;
        //}

        public static string GetFederationShipRegistration(IShip ship)
        {
            int USS = ship.Name.IndexOf("U.S.S.");

            string currentShipName = ship.Name.Substring(0, USS).Trim();
            return currentShipName;
        }

        public static bool Auto_Raise_Shields(IMap map, IRegion Region)
        {
            bool shieldsRaised = false;

            if (Region.Type != RegionType.Nebulae)
            {
                var thisShip = map.Playership;
                var thisShipEnergy = thisShip.Energy;
                var thisShipShields = Shields.For(thisShip);

                if (thisShipShields.Energy == 0) //todo: resource this out
                {
                    if (thisShipEnergy > 500) //todo: resource this out
                    {
                        thisShipShields.Energy = 500; //todo: resource this out
                        thisShip.Energy -= 500;

                        shieldsRaised = true;
                    }
                    else if (thisShipEnergy > 1)
                    {
                        var energyLeft = thisShipEnergy/2; //todo: resource this out

                        thisShipShields.Energy = Convert.ToInt32(energyLeft);
                        thisShip.Energy = energyLeft;
                        shieldsRaised = true;
                    }
                }
            }

            return shieldsRaised;
        }

        public void DestroyStarbase(IMap map, int newY, int newX, Sector qLocation)
        {
            //todo: technically, the script below should leave the Torpedoes class and move to a script class..
            //todo: raise an event that a script can use.

            //At present, a starbase can be destroyed by a single hit
            bool emergencyMessageSuccess = this.StarbaseEmergencyMessageAttempt();

            this.DestroyStarbase(map, newY, newX, (ISector)qLocation);

            if (emergencyMessageSuccess)
            {
                this.Write.Line("Before destruction, the Starbase was able to send an emergency message to Starfleet");
                this.Write.Line("Federation Ships and starbases will now shoot you on sight!");

                this.PlayerNowEnemyToFederation = true;

                //todo: later, the map will be populated with fed ships at startup.. but this should be applicable in both situations :)
                map.AddHostileFederationShipsToExistingMap();
            }
            else
            {
                this.Write.Line("Starbase was destroyed before getting out a distress call.");

                if (!this.PlayerNowEnemyToFederation)
                {
                    this.Write.Line("For now, no one will know of this..");
                }
            }
        }

        private void DestroyStarbase(IMap map, int newY, int newX, ISector qLocation)
        {
            Navigation.For(map.Playership).Docked = false;  //in case you shot it point-blank range..

            map.starbases--;

            qLocation.Object = null;
            qLocation.Item = SectorItem.Empty;

            //yeah. How come a starbase can protect your from baddies but one torpedo hit takes it out?
            this.Write.Line($"You have destroyed A Federation starbase! (at sector [{newX},{newY}])");

            this.Map.Playership.Scavenge(ScavengeType.Starbase);

            //todo: When the Starbase is a full object, then allow the torpedoes to either lower its shields, or take out subsystems.
            //todo: a concerted effort of 4? torpedoes will destroy an unshielded starbase.
            //todo: however, you'd better hit the comms subsystem to prevent an emergency message, then shoot the log bouy
            //todo: it sends out or other starbases will know of your crime.
        }

        private bool StarbaseEmergencyMessageAttempt()
        {
            return (Utility.Utility.Random.Next(2) == 1);
        }
    }
}
