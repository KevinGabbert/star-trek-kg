using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG
{
    /// <summary>
    /// This class consists of methods that have yet to be refactored into separate objects
    /// </summary>
    public class Game : IDisposable, IInteract, IConfig, IGame
    {
        #region Properties
        public delegate TResult _promptFunc<T, out TResult>(T input, out T output);

        public IStarTrekKGSettings Config { get; set; }
        public IInteraction Interact { get; set; }
        public IMap Map { get; set; }
        private Render PrintSector { get; set; }

        public Game._promptFunc<string, bool> Prompt { get; set; }

        public List<FactionThreat> LatestTaunts { get; set; } //todo: temporary until proper object is created
        public bool PlayerNowEnemyToFederation { get; set; } //todo: temporary until Starbase object is created

        public bool Started { get; set; }
        public bool GameOver { get; private set; }

        public int RandomFactorForTesting
        {
            get;
            set;
        }

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
            if (this.Interact == null)
            {
                try
                {
                    this.Interact = new Interaction(config)
                    {
                        CurrentPrompt = "Enter Command:>" //todo: resource this (default prompt)
                    };

                    this.Prompt = (string s, out string output) => this.Interact.PromptUserSubscriber(s, out output);

                }
                catch (Exception ex)
                {
                    this.Interact = new Interaction()
                    {
                        Output = new SubscriberOutput(config),
                        OutputError = true,
                        CurrentPrompt = "Terminal: " //todo: resource this.
                    };

                    return;
                }
            }

            //The config file is loaded here, and persisted through the rest of the game. 
            //Any settings that are not in the config at this point, will not be updated unless some fault tolerance is built in that
            //might try to reload the file. #NotInThisVersion
            this.Config.Get = this.Config.GetConfig();

            if (startup)
            {
                this.LatestTaunts = new List<FactionThreat>();

                //These constants need to be localized to Game:
                this.GetConstants();

                this.PrintSector = (new Render(this.Interact, this.Config));

                var startConfig = (new SetupOptions
                {
                    Initialize = true,
                    AddNebulae = true,
                    SectorDefs = this.SectorSetup()
                });

                this.InitMap(startConfig);

                //We don't want to start game without hostiles
                if (this.HostileCheck(this.Map))
                {
                    return; //todo: unless we want to have a mode that allows it for some reason.
                }

                //Set initial color scheme
                this.Interact.HighlightTextBW(false);

                //todo: why are we creating this PrintSector() class a second time??
                this.Interact = new Interaction(this.Map.HostilesToSetUp, Map.starbases, Map.Stardate, Map.timeRemaining, this.Config);
                this.PrintSector = new Render(this.Interact, this.Config);
            }
        }

        private void InitMap(SetupOptions startConfig)
        {
            this.Interact = new Interaction(this.Config);
            this.Map = new Map(startConfig, this.Interact, this.Config);
            this.Interact = new Interaction(this.Config);
        }

        #region Turn System

        #region Console/Telnet

        /// <summary>
        /// Game ends when user runs out of power, wins, or is destroyed
        /// </summary>
        private void PlayOnce()
        {
            if (this.GameOver)
            {
                this.Interact.DebugLine("Game Over.");
                return;
            }

            this.PrintOpeningScreen();

            while (!this.GameOver)
            {
                this.GameOver = this.NewConsoleTurn(); //Shows Command Prompt

                if (this.GameOver)
                {
                    this.Interact.DebugLine("Game Over.. Restarting.");

                    this.Initialize();

                    break;
                }
            }
        }

        /// <summary>
        /// A turn is represented as each time the Console requires a new command from the user
        /// This does not include sub-commands, such as adding power to the shields, etc.
        /// </summary>
        /// <returns></returns>
        private bool NewConsoleTurn()
        {
            this.Interact.ReadAndOutput(this.Map.Playership, this.Map.Text, this);

            //todo: move this to Console app.//Have Game expose and raise a CommandPrompt event.  //Have console subscribe to that event

            this.ReportGameStatus();

            return this.GameOver;
        }

        /// <summary>
        /// Starts the game in Console or Telnet Mode. Repeats indefinitely (as the original did) if App.config is set to do so.
        /// </summary>
        public void RunConsoleOrTelnet()
        {
            bool keepPlaying = true;

            while (keepPlaying)
            {
                keepPlaying = this.Config.GetSetting<bool>("KeepPlaying");

                this.PlayOnce();
                this.GameOver = false;
            }
        }

        #endregion

        #region Web

        /// <summary>
        /// Starts the game in Web mode.  Shows start screen
        /// </summary>
        public void RunSubscriber()
        {
            this.Start();
            this.PrintOpeningScreen();
        }

        /// <summary>
        /// This method reads the passed command and executes the appropriate  game resources
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public List<string> SubscriberSendAndGetResponse(string command)
        {
            List<string> retVal = null;

            this.Map.Playership.Game = this;

            this.Interact.Output.Clear();

            retVal = this.Interact.ReadAndOutput(this.Map.Playership, this.Map.Text, this, command);

            if (retVal == null)
            {
                //do nothing.  exit out
                retVal = new List<string>();
            }
            else
            {
                //todo: this may need to be added to Queue;
                this.ReportGameStatus();
            }

            //Write.OutputQueue should be filled with everything that just happened.
            return retVal; 
        }

        #endregion

        #endregion

        #region Setup

        private void Start()
        {
            this.Started = true;
            this.Interact.ResetPrompt();
        }

        private void Initialize()
        {
            this.Started = true;
            this.Interact.ResetPrompt();

            //TODO:  we can possibly reorder the baddies in this.Map.GameConfig..
            this.Map.Initialize(this.Map.GameConfig.SectorDefs, this.Map.GameConfig.AddNebulae); //we gonna start over
        }

        private void GetConstants()
        {
            DEFAULTS.DEBUG_MODE = this.Config.GetSetting<bool>("DebugMode");

            if (DEFAULTS.DEBUG_MODE)
            {
                this.Interact.Line("// ---------------- Debug Mode ----------------");
            }

            DEFAULTS.SECTOR_MIN = this.Config.GetSetting<int>("SECTOR_MIN");
            DEFAULTS.SECTOR_MAX = this.Config.GetSetting<int>("SECTOR_MAX");

            DEFAULTS.REGION_MIN = this.Config.GetSetting<int>("Region_MIN");
            DEFAULTS.REGION_MAX = this.Config.GetSetting<int>("RegionMax");

            DEFAULTS.SHIELDS_DOWN_LEVEL = this.Config.GetSetting<int>("ShieldsDownLevel");
            DEFAULTS.LOW_ENERGY_LEVEL = this.Config.GetSetting<int>("LowEnergyLevel");
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
            //todo: this needs to be  in a config file

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

        #endregion

        #region Title

        /// <summary>
        /// Prints title and sets up the playfield.
        /// This is where the Map is created, and references to it are passed around from here on.
        /// </summary>
        private void PrintOpeningScreen()
        {
            this.RandomAppTitle(); //Printing the title at this point is really a debug step. (it shows that the game is started.  Otherwise, it could go after initialization)

            this.Interact.ResourceLine(this.Config.GetText("AppVersion").TrimStart(' '), "UnderConstructionMessage");

            this.Interact.PrintMission();
        }

        private void RandomAppTitle()
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

            this.Interact.Resource("AppTitleSpace");

            this.RandomPicture();

            this.Interact.Resource("AppTitleSpace");
        }

        private void RandomPicture()
        {
            Utility.Utility.Random = new Random(Guid.NewGuid().GetHashCode());
            int randomVal = Utility.Utility.Random.Next(100);
            switch (randomVal)
            {
                case 1:
                    this.AppTitleItem("ExcelsiorMedium", 8);
                    break;

                case 2:
                    this.AppTitleItem("DaedalusSmall", 8);
                    break;

                case 3:
                    this.AppTitleItem("Reliant", 8);
                    break;

                case 4:
                    this.AppTitleItem("D7Front", 6);
                    break;

                case 5:
                    this.AppTitleItem("D-10-", 6);
                    break;

                case 6:
                    this.AppTitleItem("D-4-", 7);
                    break;

                case 7:
                    this.AppTitleItem("D-11-", 6);
                    break;

                case 8:
                    this.AppTitleItem("D-18-", 6);
                    break;

                case 9:
                    this.AppTitleItem("D-27-", 7);
                    break;

                case 10:
                    this.AppTitleItem("AkulaSmall", 7);
                    break;

                case 11:
                    this.AppTitleItem("BattlecruiserSmall", 6);
                    break;

                case 12:
                    this.AppTitleItem("SaladinSmall", 6);
                    break;

                case 13:
                    this.AppTitleItem("EagleSmall", 6);
                    break;

                case 14:
                    this.AppTitleItem("DreadnaughtSide", 9);
                    break;

                case 15:
                    this.AppTitleItem("Enterprise-BSmall", 6);
                    break;

                case 16:
                    this.AppTitleItem("ExcelsiorSmall", 6);
                    break;

                case 17:
                    this.AppTitleItem("RomulanBOP", 8);
                    break;

                default:
                    this.AppTitleItem("2ShipsSmall", 7);
                    break;
            }
        }

        private void AppTitleItem(string itemName, int endingLine)
        {
            for (int i = 1; i < endingLine; i++)
            {
                this.Interact.Resource($"AppTitle{itemName}{i}");
            }
        }

        #endregion

        #region Attacks

        /// <summary>
        /// At the end of each turn, if a hostile is in the same sector with Playership, it will attack.  If there are 37, then all 37 will..
        /// TODO: this needs to be changed.  after destruction, it appears to take several method returns to realize that we are dead.
        /// </summary>
        /// <returns></returns>
        public void ALLHostilesAttack(IMap map)
        {
            //todo: centralize this.
            //this is called from torpedo control/phaser control, and navigation control

            var returnValue = false;
            var activeRegion = map.Regions.GetActive();
            var hostilesAttacking = activeRegion.GetHostiles();

            this.HostileStarbasesAttack(map, activeRegion);

            returnValue = this.HostileShipsAttack(map, hostilesAttacking, returnValue);
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
                    this.Interact.Line("Hostile Starbase fires blindly, unable to get a lock on your position in Nebula.");
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
            string hitMessage = this.Interact.ShipHitMessage(attacker, attackingEnergy);

            this.Interact.Line(hitMessage + " No damage due to starbase shields.");
        }

        #endregion

        #region Taunts

        /// <summary>
        /// All enemies in PlayerShip's Region shall now commence to unclog their noses in the general direction of the player.
        /// </summary>
        public void EnemiesWillNowTaunt()
        {
            //todo: move this to communications subsystem eventually
            var currentRegion = this.Map.Playership.GetRegion();
            var hostilesInRegion = currentRegion.GetHostiles();

            this.LatestTaunts = new List<FactionThreat>();

            IEnumerable<IShip> shipsWithTaunts = from ship in hostilesInRegion
                                                    let tauntLikely = Utility.Utility.Random.Next(5) == 1
                                                    where tauntLikely
                                                    select ship;

            string currentThreat = "";
            // ReSharper disable once LoopCanBeConvertedToQuery //Linq should only be for selecting, not executing.
            foreach (var taunt in shipsWithTaunts)
            {
                currentThreat = this.SingleEnemyTaunt(taunt, currentThreat);
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

            this.Interact.Line("");

            if (currentFaction == FactionName.Federation)
            {
                //"NCC-500 U.S.S. Saladin  Saladin-class"
                //"NCC-500 U.S.S. FirstName SecondName  Saladin-class"

                currentShipName = ship.Name;
            }
            else if (currentFaction == FactionName.Klingon)
            {
                this.Interact.WithNoEndCR($"Klingon ship at {"[" + ship.Sector.X + "," + ship.Sector.Y + "]"} sends the following message: ");
            }
            else
            {
                this.Interact.WithNoEndCR($"Hostile at {"[" + ship.Sector.X + "," + ship.Sector.Y + "]"} sends the following message: ");
                currentShipName = ship.Name;
            }

            FactionThreat randomThreat = this.Config.GetThreats(currentFaction).Shuffle().First();

            currentThreat += string.Format(randomThreat.Threat, currentShipName);

            this.LatestTaunts.Add(randomThreat);

            this.Interact.Line(currentThreat);
            return currentThreat;
        }

        #endregion

        #region Shields

        public bool Auto_Raise_Shields(IMap map, IRegion Region)
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
                        var energyLeft = thisShipEnergy / 2; //todo: resource this out

                        thisShipShields.Energy = Convert.ToInt32(energyLeft);
                        thisShip.Energy = energyLeft;
                        shieldsRaised = true;
                    }
                }
            }

            return shieldsRaised;
        }

        //todo: move this to a report object?
        public void ReportShieldsStatus(IMap map, int shieldsValueBeforeHit)
        {
            var shieldsValueAfterHit = Shields.For(map.Playership).Energy;

            if (shieldsValueAfterHit <= shieldsValueBeforeHit)
            {
                if (shieldsValueAfterHit == 0)
                {
                    this.Interact.SingleLine("** Shields are Down **");
                }
                else
                {
                    this.Interact.SingleLine($"Shields dropped to {Shields.For(map.Playership).Energy}.");
                }
            }
        }

        #endregion

        #region Starbase

        public void DestroyStarbase(IMap map, int newY, int newX, Sector qLocation)
        {
            //todo: technically, the script below should leave the Torpedoes class and move to a script class..
            //todo: raise an event that a script can use.

            //At present, a starbase can be destroyed by a single hit
            bool emergencyMessageSuccess = this.StarbaseEmergencyMessageAttempt();

            this.DestroyStarbase(map, newY, newX, (ISector)qLocation);

            if (emergencyMessageSuccess)
            {
                this.Interact.Line("Before destruction, the Starbase was able to send an emergency message to Starfleet");
                this.Interact.Line("Federation Ships and starbases will now shoot you on sight!");

                this.PlayerNowEnemyToFederation = true;

                //todo: later, the map will be populated with fed ships at startup.. but this should be applicable in both situations :)
                map.AddHostileFederationShipsToExistingMap();
            }
            else
            {
                this.Interact.Line("Starbase was destroyed before getting out a distress call.");

                if (!this.PlayerNowEnemyToFederation)
                {
                    this.Interact.Line("For now, no one will know of this..");
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
            this.Interact.Line($"You have destroyed A Federation starbase! (at sector [{newX},{newY}])");

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

        #endregion

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
            int USS = ship.Name.IndexOf("U.S.S."); //todo: resource this out.

            string currentShipName = ship.Name.Substring(0, USS).Trim();
            return currentShipName;
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
                this.Interact.Line("ERROR: --- No Hostiles have been set up.");

                //todo: perhaps we'd have a reason to make a "freeform" option or mode where you could practice shooting things, moving, etc.
                //todo: in that case, this function would not be called

                this.GameOver = true;
                return true;
            }
            return false;
        }

        private void ReportGameStatus()
        {
            int starbasesLeft = this.Map.Regions.GetStarbaseCount();

            if (this.PlayerNowEnemyToFederation)
            {
                this.GameOver = (this.Map.timeRemaining < 1 ||
                                 starbasesLeft < 1 ||
                                 this.Map.Playership.Destroyed ||
                                 this.Map.Playership.Energy < 1);
            }
            else
            {
                this.GameOver = !(this.Map.Playership.Energy > 0 &&
                                  !this.Map.Playership.Destroyed &&
                                  (this.Map.Regions.GetHostileCount() > 0) &&
                                  this.Map.timeRemaining > 0);
            }

            this.Interact.PrintMissionResult(this.Map.Playership, this.PlayerNowEnemyToFederation, starbasesLeft);
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
            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;

            DEFAULTS.REGION_MIN = 0;
            DEFAULTS.REGION_MAX = 0;
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }
    }
}
