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
                    responseLines.Add("Error starting Game");
                }
            }
            else
            {
                responseLines.Add("Game already running. Type 'stop' then 'start' again to start over.");
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

            responseLines = new List<string> { "Game Started.." };
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
            return new List<string> { "Session Cleared.." };
        }

        public Game GetGame()
        {
            return (Game)HttpContext.Current.Session["game"] ?? null; //todo:  make this "game" + sessionID
        }

        public List<string> SendTestResponse(List<string> responseLines)
        {
            responseLines = new List<string>
            {
                "Testing..",
                "Testing..",
                "1..2..3.."
            };
            return responseLines;
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
        public List<string> NewTurn(List<string> responseLines, string sessionID, string command)
        {
            var myGame = this.GetGame(sessionID);

            if (myGame?.Started == true)
            {
                //pass command and session ID to application
                if (!myGame.GameOver)
                {
                    //todo: Make sure that game.IsSubscriberApp is set by this point.
                    responseLines = myGame.SendAndGetResponse(command);
                }
                else
                {
                    responseLines.Add("G A M E  O V E R");
                    HttpContext.Current.Session.Clear();

                    //todo: now what?  auto start over? or just do it anyway?
                }
            }
            else
            {
                responseLines.Add("No game running.  Type 'start' to start game");
            }

            return responseLines;
        }
    }
}