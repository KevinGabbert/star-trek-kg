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
            switch (command)
            {
                case "test":
                    responseLines = this.SendTestResponse(responseLines);
                    break;

                case "make error":
                    throw new ArgumentException("This is a test error");

                case "end game":
                    //todo: gives final stats and stops game
                    break;

                case "stop":
                    responseLines = this.StopGame(responseLines);
                    break;

                case "start":
                    responseLines = this.StartGame(game, responseLines);
                    break;

                default:
                    responseLines = this.NewWebTurn(responseLines, sessionID, command);
                    break;
            }
            return responseLines;
        }

        public List<string> StartGame(Game game, List<string> responseLines)
        {
            if (game == null || (!game.Started))
            {
                HttpContext.Current.Session["game"] = null;

                this.RunWeb(responseLines, out game);

                if (game != null)
                {
                    game.Started = true;
                    responseLines = game.Write?.Output?.OutputQueue.ToList();
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

        public List<string> RunWeb(List<string> responseLines, out Game game)
        {
            var settingsForWholeGame = (new StarTrekKGSettings());
            game = (new Game(settingsForWholeGame)
            {
                //todo: delete this property setting
                Output =
                {
                    IsSubscriberApp = true //todo: GET should set the app.config setting to true?
                }
            });

            HttpContext.Current.Session["game"] = game;

            this.GetGame().RunWeb();
            this.GetGame().Started = true;

            responseLines = this.Response("Game Started..");
            return responseLines;
        }

        public Game GetGame(string sessionID)
        {
            //todo: implement sessionID;
            return (Game)HttpContext.Current.Session["game"]; //todo:  make this "game" + sessionID
        }

        internal List<string> StopGame(List<string> responseLines)
        {
            HttpContext.Current.Session.Clear(); 
            return this.Response("Session Cleared.."); //todo: resource this out
        }

        public Game GetGame()
        {
            return (Game)HttpContext.Current.Session["game"]; //todo:  make this "game" + sessionID
        }

        public List<string> SendTestResponse(List<string> responseLines)
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

        public List<string> NewWebTurn(List<string> responseLines, string sessionID, string command)
        {
            var myGame = this.GetGame(sessionID);

            if (myGame?.Started == true)
            {
                //pass command and session ID to application
                if (!myGame.GameOver)
                {
                    responseLines = this.Response(myGame.WebSendAndGetResponse(command));
                }
                else
                {
                    responseLines = this.Response("G A M E  O V E R");
                    HttpContext.Current.Session.Clear();
                }
            }
            else
            {
                responseLines = this.Response("Unrecognized Command. Game is not running.  Type 'start' to start game");
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