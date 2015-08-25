using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.SessionState;
using System.Web.UI;
using Newtonsoft.Json;
using StarTrek_KG;
using StarTrek_KG.Config;

namespace StarTrek_KG_Web
{
    public partial class _Default : Page
    {
        private Game _game;
        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.EnableViewState = false;  //don't need it cause we are not using it..

            //todo: create a turn system for the web app.  Then abstract as much as possible and put it in Star_Trek_KG
            //todo: it might just be that the web app "turn system" might have to stay in the web app, but I'm thinking 
            //todo: we could juts rewrite the one for the console app.
            //todo:  or I could just be safe and not touch the existing system and implement the new one in the web app :D

            ////todo: put all this BELOW in a business layer in StarTrek_KG_Web:

            //var settingsForWholeGame = (new StarTrekKGSettings());

            _game = ((Game) Session["game"]);

            //if (_game == null)
            //{
            //    var game = (new Game(settingsForWholeGame)
            //    {
            //        //todo: delete this property setting
            //        Output =
            //        {
            //            IsSubscriberApp = true //todo: GET should set the app.config setting to true?
            //        }

            //    });

            //    Session["game"] = game;
            //    _game = (Game) Session["game"];

            //    ((Game) Session["game"]).Run();
            //}
            //else
            //{
            //    _game = ((Game) Session["game"]);
            //}
        }


        #endregion

        #region PageMethods

        [WebMethod]
        public static string QueryConsole(string command, string sessionID)
        {
            List<string> responseLines = new List<string>
            {
                "Not Implemented."
            };

            switch (command)
            {
                case "test":
                    responseLines = _Default.SendTestResponse(responseLines);
                    break;

                case "make error":
                    throw new ArgumentException("This is a test error");

                case "stop":
                    HttpContext.Current.Session.Clear();
                    responseLines = new List<string>{"Session Cleared.."};
                    break;

                case "start":
                    HttpContext.Current.Session["game"] = null;
                    responseLines = _Default.RunWeb(responseLines);
                    break;

                case "run":
                    responseLines = _Default.RunWeb(responseLines);
                    break;

                case "test2":
                    _Default.RunWeb(responseLines);
                    Game game2 = _Default.GetGame();

                    game2?.PrintOpeningScreen();

                    responseLines = game2?.Write?.Output?.OutputQueue.ToList();
                    break;

                default:
                    _Default.RunWeb(responseLines);

                    var myGame = _Default.GetGame(sessionID);

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

                    break;
            }

            string json = JsonConvert.SerializeObject(responseLines.ToList());
            return json;
        }

        private static List<string> RunWeb(List<string> responseLines)
        {
            var settingsForWholeGame = (new StarTrekKGSettings());
            var game = (new Game(settingsForWholeGame)
            {
                //todo: delete this property setting
                Output =
                {
                    IsSubscriberApp = true //todo: GET should set the app.config setting to true?
                }
            });

            HttpContext.Current.Session["game"] = game;

            _Default.GetGame().RunWeb();

            responseLines = new List<string> {"Game Started.."};
            return responseLines;
        }

        private static Game GetGame(string sessionID)
        {
            //todo: implement sessionID;
            return (Game)HttpContext.Current.Session["game"]; //todo:  make this "game" + sessionID
        }

        private static Game GetGame()
        {
            return (Game)HttpContext.Current.Session["game"]; //todo:  make this "game" + sessionID
        }

        private static List<string> SendTestResponse(List<string> responseLines)
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

        #endregion
    }
}