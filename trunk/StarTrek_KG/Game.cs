using System;
using System.Configuration;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;

namespace StarTrek_KG
{
    public class Game
    {
        #region Properties

            public Output Output { get; set; }
            public Map Map { get; set; }
            public Command Command { get; set; }

            public bool gameOver;

        #endregion

        public Game()
        {

            Constants.SECTOR_MIN = Convert.ToInt32(ConfigurationManager.AppSettings["SECTOR_MIN"]);
            Constants.SECTOR_MAX = Convert.ToInt32(ConfigurationManager.AppSettings["SECTOR_MAX"]);

            Constants.QUADRANT_MIN = Convert.ToInt32(ConfigurationManager.AppSettings["QUADRANT_MIN"]);
            Constants.QUADRANT_MAX = Convert.ToInt32(ConfigurationManager.AppSettings["QuadrantMax"]);

            this.Output = new Output();

            var startConfig = (new GameConfig
                                   {
                                       Initialize = true,
                                       GenerateMap = true,
                                       SectorDefs = SectorSetup()
                                   });

            this.Map = new Map(startConfig);

            //We don't want to start game without hostiles
            //todo: unless we want to have a mode that allows it for some reason.
            if (this.HostileCheck(startConfig)) return;

            this.Output = new Output(this.Map.hostilesToSetUp, Map.timeRemaining, Map.starbases, Map.Stardate);
            this.Command = new Command(this.Map);
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

        private bool HostileCheck(GameConfig startConfig)
        {
            if (startConfig.SectorDefs.Hostiles().Count() < 1)
            {
                Output.Write("No Hostiles have been set up.");
                    //todo: perhaps we'd have a reason to make a "freeform" option or mode where you could practice shooting things, moving, etc.
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
            bool keepPlaying = Convert.ToBoolean(ConfigurationManager.AppSettings["KeepPlaying"]);

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
        private void Initialize()
        {
            Output.PrintStrings(Constants.APP_TITLE); //Printing the title at this point is really a debug step. (it shows that the game is started.  Otherwise, it could go after initialization)

            Output.PrintMission();

            if(!this.gameOver)
            {
                //this.Map.Quadrants.PopulateSectors(this.Map.GameConfig.SectorDefs, this.Map);
            }
        }

        /// <summary>
        /// Game ends when user runs out of power, wins, or is destroyed
        /// </summary>
        private void PlayOnce()
        {
            if(gameOver)
            {
                return;
            }

            this.Initialize();

            while (!gameOver)
            {
                gameOver = this.NewTurn();

                if (gameOver)
                {
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
    }
}
