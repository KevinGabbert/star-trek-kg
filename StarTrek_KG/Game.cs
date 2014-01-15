using System;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG
{
    public class Game : IDisposable, IWrite, IConfig
    {
        #region Properties

            public IStarTrekKGSettings Config { get; set; }
            public Write Write { get; set; }

            public Output.Write Output { get; set; }
            public Output.PrintSector PrintSector { get; set; }
            public Map Map { get; set; }

            public bool gameOver;

        #endregion

        /// <summary>
            /// todo: all game workflow functions go here (currently, workflow is ensconced within actors)
            /// and some unsorted crap at the moment..
        /// </summary>
        public Game(IStarTrekKGSettings config, bool startup = true)
        {
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

                //These constants need to be localized to Game:
                this.GetConstants();

                this.PrintSector =
                    (new Output.PrintSector(Constants.SHIELDS_DOWN_LEVEL, Constants.LOW_ENERGY_LEVEL, this.Write, this.Config));

                var startConfig = (new SetupOptions
                                       {
                                           Initialize = true,
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
                this.Output = new Output.Write(this.Map.hostilesToSetUp, Map.starbases, Map.Stardate, Map.timeRemaining, this.Config);   
                this.PrintSector = new PrintSector(Constants.SHIELDS_DOWN_LEVEL, Constants.LOW_ENERGY_LEVEL, this.Write, this.Config);
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

            Constants.QUADRANT_MIN = this.Config.GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = this.Config.GetSetting<int>("QuadrantMax");

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
            return new SectorDefs
                       {
                           //This tells us what Types of items will be generated at start.  if Coordinates are passed, that is an
                           //indicator that an individual object needs to be placed, istead of generated objects from config file.

                           //todo: get rid of that second, stupid parameter.
                           new SectorDef(SectorItem.Friendly),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
                           new SectorDef(SectorItem.Hostile),
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

        private bool HostileCheck(Map map)
        {
            if (!map.Quadrants.GetHostiles().Any())
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
            var keepPlaying = this.Config.GetSetting<bool>("KeepPlaying");

            while (keepPlaying)
            {
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
            
            this.Write.ResourceLine("UnderConstructionMessage");

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
            if(gameOver)
            {
                this.Write.DebugLine("Game Over.");
                return;
            }

            this.PrintOpeningScreen();

            while (!gameOver)
            {
                gameOver = this.NewTurn(); //Shows Command Prompt

                if (gameOver)
                {
                    this.Write.DebugLine("Game Over.. Restarting.");

                    //TODO:  we can possibly reorder the baddies in this.Map.GameConfig..
                    this.Map.Initialize(this.Map.GameConfig.SectorDefs); //we gonna start over

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

                //move this to Console app.//Have Game expose and raise a CommandPrompt event.  //Have console subscribe to that event
                ////Map.GetAllHostiles(this.Map).Count

            gameOver = !(this.Map.Playership.Energy > 0 &&
                         !this.Map.Playership.Destroyed &&
                          (this.Map.Quadrants.GetHostileCount() > 0) &&                              
                         this.Map.timeRemaining > 0);

            Output.PrintCommandResult(this.Map.Playership);
            return gameOver;
        }

        public void MoveTimeForward(Map map, Coordinate lastQuadrant, Coordinate quadrant)
        {
            if (lastQuadrant.X != quadrant.X || lastQuadrant.Y != quadrant.Y)
            {
                map.timeRemaining--;
                map.Stardate++;
            }
        }

        public void Dispose()
        {
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
        }

        /// <summary>
        /// At the end of each turn, if a hostile is in the same sector with Playership, it will attack.  If there are 37, then all 37 will..
        /// TODO: this needs to be changed.  after destruction, it appears to take several method returns to realize that we are dead.
        /// </summary>
        /// <returns></returns>
        public bool ALLHostilesAttack(Map map)
        {
            //todo:rewrite this.
            //this is called from torpedo control/phaser control, and navigation control

            var activeQuadrant = map.Quadrants.GetActive();
            var hostilesAttacking = activeQuadrant.GetHostiles();

            if (hostilesAttacking != null)//todo: remove this.
            {
                if (hostilesAttacking.Count > 0)
                {
                    foreach (var badGuy in hostilesAttacking)
                    {
                        this.HostileAttacks(map, badGuy);
                    }
                    return true;
                }
            }

            return false;
        }

        private void HostileAttacks(Map map, IShip badGuy)
        {
            if (Navigation.For(map.Playership).docked)
            {
                this.AttackDockedPlayership(badGuy);
            }
            else
            {
                this.AttackNonDockedPlayership(map, badGuy);
            }
        }

        private void AttackNonDockedPlayership(Map map, IShip badGuy)
        {
            var playerShipLocation = map.Playership.GetLocation();
            var distance = Utility.Utility.Distance(playerShipLocation.Sector.X,
                                                    playerShipLocation.Sector.Y,
                                                    badGuy.Sector.X,
                                                    badGuy.Sector.Y);

            var seedEnergyToPowerWeapon = this.Config.GetSetting<int>("DisruptorShotSeed")*
                                          (Utility.Utility.Random).NextDouble();

            //Todo: this should be Disruptors.For(this.ShipConnectedTo).Shoot()
            //todo: the -1 should be the ship energy you want to allocate
            var attackingEnergy = (int)Utility.Utility.ShootBeamWeapon(seedEnergyToPowerWeapon, distance, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment"); 

            var shieldsValueBeforeHit = Shields.For(map.Playership).Energy;

            map.Playership.AbsorbHitFrom(badGuy, attackingEnergy);

            this.ReportShieldsStatus(map, shieldsValueBeforeHit);
        }

        private void AttackDockedPlayership(IShip badGuy)
        {
            this.Write.Line(
                String.Format(
                    "Your Ship has been hit by " + badGuy.Name + " at sector [{0},{1}]. No damage due to starbase shields.",
                    (badGuy.Sector.X), (badGuy.Sector.Y)));
        }


        //todo: move this to a report object?
        public void ReportShieldsStatus(Map map, int shieldsValueBeforeHit)
        {
            var shieldsValueAfterHit = Shields.For(map.Playership).Energy;

            if (shieldsValueAfterHit <= shieldsValueBeforeHit)
            {
                if (shieldsValueAfterHit == 0)
                {
                    this.Write.SingleLine(" Shields are Down.");
                }
                else
                {
                    this.Write.SingleLine(String.Format(" Shields dropped to {0}.", Shields.For(map.Playership).Energy));
                }
            }
        }
    }
}
