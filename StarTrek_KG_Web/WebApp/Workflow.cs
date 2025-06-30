using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StarTrek_KG;
using StarTrek_KG.Config;

namespace StarTrek_KG_Web.WebApp
{
    public class Workflow
    {
        public List<string> ExecuteCommand(string command, string sessionID, List<string> responseLines, Game game)
        {

            //todo: make this into a resource file. KLW
            switch (command.Trim().ToLower())
            {
                case "connect to ncc1701":
                    //todo: connects to ncc1701
                    //todo: flesh this out to connect to any ship needed.
                    break;

                //todo: resource out menu
                case "term menu":
                    responseLines = this.Response(new List<string>() //todo: resource this
                    {
                        " --- Terminal Menu ---",
                        "start - starts a session",
                        "end session - ends the currently running game",
                        "reset config - reloads config",
                        "release notes - see the latest release notes",
                        "clear - clear the screen"
                    });
                    break;

                case "test":
                    responseLines = this.SendTestResponse();
                    break;

                case "make error":
                    throw new ArgumentException("This is a test error");

                case "end session":
                case "stop":
                    //todo: gives final stats and stops game
                    responseLines = this.Response(new List<string>()
                    {
                        "-- Session Terminated --"
                    });

                    this.ClearSession(responseLines);
                    break;

                case "clear session":
                    responseLines = this.ClearSession(responseLines);
                    break;

                case "reset config":
                    responseLines = new List<string>()
                    {
                        "Not Implemented Yet."
                    };

                    break;

                case "start":
                    responseLines = this.StartGame(game, responseLines);
                    break;

                case "release notes":
                    //todo: pull release notes from a file
                    responseLines = this.Response("Under Construction");
                    break;

                default:
                    responseLines = this.NewTurn(responseLines, sessionID, command);
                    break;
            }
            return responseLines;
        }

        private List<string> StartGame(Game game, List<string> responseLines)
        {
            if (game == null || !game.Started)
            {
                HttpContext.Current.Session["game"] = null;

                this.RunWeb(responseLines, out game);

                if (game != null)
                {
                    if (!game.Interact.OutputError)
                    {
                        game.Started = true;

                        //todo: this should animate so that periods type out slowly..  JQuery Terminal can do this.
                        responseLines.Add("Connecting to U.S.S. Enterprise - NCC 1701..");

                        if (game.Interact?.Output?.Queue != null)
                        {
                            responseLines.AddRange(game.Interact?.Output?.Queue.ToList());
                        }
                    }
                    else
                    {
                        responseLines.Insert(0, "Err:"); //todo: resource this
                    }
                }
                else
                {
                    responseLines = this.Response($"Error Starting Game. {this.ReportIssueMessage()}"); //todo: resource this out.
                }
            }
            else
            {  
                responseLines = this.Response("Game already running. Type 'stop' then 'start' again to start over."); //todo: resource this out.
            }
            return responseLines;
        }

        private List<string> RunWeb(List<string> responseLines, out Game game)
        {
            StarTrekKGSettings settingsForWholeGame = new StarTrekKGSettings();
  
            game = new Game(settingsForWholeGame); //todo: GET should set the app.config setting to true?

            HttpContext.Current.Session["game"] = game;

            if (!game.Interact.OutputError)
            {
                Workflow.GetGame().RunSubscriber();

                responseLines = this.Response("Game Started.."); //todo: resource this
            }
            else
            {
                responseLines.Add("Game Initialization failed.  Possible Configuration Error.");
            }

            return responseLines;
        }

        private Game GetGame(string sessionID)
        {
            //todo: implement sessionID;
            return (Game)HttpContext.Current.Session["game"]; //todo:  make this "game" + sessionID
        }

        private List<string> ClearSession(List<string> responseLines)
        {
            HttpContext.Current.Session.Clear(); 
            return this.Response("Session Cleared.."); //todo: resource this out
        }

        public static Game GetGame()
        {
            return (Game)HttpContext.Current.Session["game"]; //todo:  make this "game" + sessionID
        }

        private List<string> SendTestResponse()
        {
            return this.Response(new List<string>
            {
                "Testing..",
                "Testing..",
                "1..2..3.."
            });
        }

        //private static CheckSessionTime()
        //{
        //    // Get the session in minutes and seconds
        //    HttpSessionState session = HttpContext.Current.Session;//Return current session
        //    DateTime? sessionStart = session["SessionStart"] as DateTime?;//Get DateTime object SessionStart
        //                                                                  //Check if session has not expired
        //    if (sessionStart.HasValue)
        //    {

        //    }
        //}

        private List<string> NewTurn(List<string> responseLines, string sessionID, string command)
        {
            var myGame = this.GetGame(sessionID);

            if (myGame?.Started == true)
            {
                //pass command and session ID to application
                if (!myGame.GameOver)
                {
                    var turnResponse = myGame.SubscriberSendAndGetResponse(command);
                    responseLines = this.Response(turnResponse);
                }
                else
                {
                    responseLines = this.Response("** G A M E  O V E R **"); //todo: resource this
                    responseLines.Insert(0, "Err:"); //todo: resource this

                    HttpContext.Current.Session.Clear();
                }
            }
            else
            {
                responseLines = this.Response("Unrecognized Command. Game is not running.  Type 'start' to start game");
                responseLines.Insert(0, "Err:"); //todo: resource this
            }

            return responseLines;
        }

        private List<string> Response(string text)
        {
            var responseLines = new List<string>()
            {
                Environment.NewLine,
                text,
                Environment.NewLine
            };
            return responseLines;
        }
        private List<string> Response(IList<string> text)
        {
            if (text != null)
            {
                text.Insert(0, Environment.NewLine);
                text.Add(Environment.NewLine);
            }
            else
            {
                text = new List<string>(new List<string>());
            }

            var responseLines = new List<string>(text);

            return responseLines;
        }

        private string ReportIssueMessage()
        {
            return "Click <a href=\"https://github.com/KevinGabbert/star-trek-kg/issues\" target=_blank>HERE</a> to report issue (opens in new window)";
        }
    }
}