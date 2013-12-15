using System;
using System.Linq;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG
{
    //Notes on a play through 12-14-13:

    //- when shields are 0, and you want to subtract, amount of energy shows as (1--0).  Need an error message that says you can't subtract energy
    //- Galactic map broken
    //- Torpedoes don't work (can't verify)
    //- stardates are not incrementing
    //- why can I only go warp 3??
    //- last hostile does not show up on SRS Scanner
    //- Some Prompts accept entry on the next line instead of the same

    //test...

    //ship in bottom right of sector tries to go direction 4 (nw) and it does not get across sector.

    //- why are there always 5 stars in each quadrant??
    //WANTS
    //- I want to know how many stardates I have left.
    //- ability to save game to resume later
    //- a rank issued at the end of the game

    public class Game: IDisposable
    {
        #region Properties

            public Output.Write Output { get; set; }
            public Map Map { get; set; }
            public Command Command { get; set; }

            public bool gameOver;

        #endregion

        /// <summary>
            /// todo: all game workflow functions go here (currently, workflow is ensconced within actors)
        /// </summary>
        public Game()
        {
            //The config file is loaded here, and persisted through the rest of the game. 
            //Any settings that are not in the config at this point, will not be updated unless some fault tolerance is built in that
            //might try to reload the file. #NotInThisVersion
            StarTrekKGSettings.Get = StarTrekKGSettings.GetConfig();

            //These constants need to be localized to Game:
            GetConstants();

            this.Output = (new Output.Write(Constants.SHIELDS_DOWN_LEVEL, Constants.LOW_ENERGY_LEVEL));

            var startConfig = (new GameConfig
                                   {
                                       Initialize = true,
                                       SectorDefs = SectorSetup()
                                   });

            this.Map = new Map(startConfig);
            this.Command = new Command(this.Map);

            //We don't want to start game without hostiles
            if (this.HostileCheck(this.Map)) return;  //todo: unless we want to have a mode that allows it for some reason.

            //todo: why are we creating this Output() class a second time??
            this.Output = new Output.Write(this.Map.hostilesToSetUp, Map.timeRemaining, Map.starbases, Map.Stardate, Constants.SHIELDS_DOWN_LEVEL, Constants.LOW_ENERGY_LEVEL);
        }

        private static void GetConstants()
        {
            Constants.DEBUG_MODE = StarTrekKGSettings.GetSetting<bool>("DebugMode");

            if (Constants.DEBUG_MODE)
            {
                StarTrek_KG.Output.Write.Line("// ---------------- Debug Mode ----------------");
            }

            Constants.SECTOR_MIN = StarTrekKGSettings.GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = StarTrekKGSettings.GetSetting<int>("SECTOR_MAX");

            Constants.QUADRANT_MIN = StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = StarTrekKGSettings.GetSetting<int>("QuadrantMax");

            Constants.SHIELDS_DOWN_LEVEL = StarTrekKGSettings.GetSetting<int>("ShieldsDownLevel");
            Constants.LOW_ENERGY_LEVEL = StarTrekKGSettings.GetSetting<int>("LowEnergyLevel");  
        }

        private static SectorDefs SectorSetup()
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
        private static SectorDefs DefaultHardcodedSetup()
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
                StarTrek_KG.Output.Write.Line("ERROR: --- No Hostiles have been set up.");

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
            var keepPlaying = StarTrekKGSettings.GetSetting<bool>("KeepPlaying");

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
            StarTrek_KG.Output.Write.AppTitle(); //Printing the title at this point is really a debug step. (it shows that the game is started.  Otherwise, it could go after initialization)
            
            StarTrek_KG.Output.Write.ResourceLine("UnderConstructionMessage");
            StarTrek_KG.Output.Write.ResourceLine("UnderConstructionMessage2");
            StarTrek_KG.Output.Write.ResourceLine("UnderConstructionMessage3");

            Output.PrintMission();
        }

        /// <summary>
        /// Game ends when user runs out of power, wins, or is destroyed
        /// </summary>
        private void PlayOnce()
        {
            if(gameOver)
            {
                StarTrek_KG.Output.Write.DebugLine("Game Over.");
                return;
            }

            this.PrintOpeningScreen();

            while (!gameOver)
            {
                gameOver = this.NewTurn(); //Shows Command Prompt

                if (gameOver)
                {
                    StarTrek_KG.Output.Write.DebugLine("Game Over.. Restarting.");

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
            this.Command.Prompt(this.Map.Playership.Name);
                //move this to Console app.//Have Game expose and raise a CommandPrompt event.  //Have console subscribe to that event
                ////Map.GetAllHostiles(this.Map).Count

            gameOver = !(this.Map.Playership.Energy > 0 &&
                         !this.Map.Playership.Destroyed &&
                          (this.Map.Quadrants.GetHostileCount() > 0) &&                              
                         this.Map.timeRemaining > 0);

            Output.PrintCommandResult(this.Map.Playership);
            return gameOver;
        }

        public static void MoveTimeForward(Map map, Coordinate lastQuadrant, Coordinate quadrant)
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
        /// TODO: this needs to be changed.  after destruction, it appears to take several method returns to realize that we are dead.
        /// </summary>
        /// <returns></returns>
        public static bool ALLHostilesAttack(Map map)
        {
            //todo:rewrite this.
            //this is called from torpedo control/phaser control, and navigation control

            var activeQuadrant = map.Quadrants.GetActive();
            var hostilesAttacking = activeQuadrant.GetHostiles();

            //temporary
            if (hostilesAttacking != null)//todo: remove this.
            {
                if (hostilesAttacking.Count > 0)
                {
                    foreach (var badGuy in hostilesAttacking)
                    {
                        if (Navigation.For(map.Playership).docked)
                        {
                            StarTrek_KG.Output.Write.Line(String.Format(map.Playership.Name + " hit by " + badGuy.Name + " at sector [{0},{1}].. No damage due to starbase shields.", (badGuy.Sector.X), (badGuy.Sector.Y)));
                        }
                        else
                        {
                            var ship = map.Playership.GetLocation();
                            var distance = Utility.Utility.Distance(ship.Sector.X,
                                                        ship.Sector.Y,
                                                        badGuy.Sector.X,
                                                        badGuy.Sector.Y);

                            var attackingEnergy = Disruptors.Shoot(distance);

                            var shieldsValueBeforeHit = Shields.For(map.Playership).Energy;

                            map.Playership.AbsorbHitFrom(badGuy, attackingEnergy);

                            ReportShieldsStatus(map, shieldsValueBeforeHit);

                        }
                    }
                    return true;
                }
            }

            return false;
        }


        //todo: move this to a report object?
        public static void ReportShieldsStatus(Map map, int shieldsValueBeforeHit)
        {
            var shieldsValueAfterHit = Shields.For(map.Playership).Energy;

            if (shieldsValueAfterHit <= shieldsValueBeforeHit)
            {
                if (shieldsValueAfterHit == 0)
                {
                    StarTrek_KG.Output.Write.SingleLine(" Shields are Down.");
                }
                else
                {
                    StarTrek_KG.Output.Write.SingleLine(String.Format(" Shields dropped to {0}.", Shields.For(map.Playership).Energy));
                }
            }
        }
    }
}
