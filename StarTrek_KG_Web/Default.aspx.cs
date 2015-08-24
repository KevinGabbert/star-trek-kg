using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
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


            ////todo: put all this BELOW in a business layer in StarTrek_KG_Web:

            //var settingsForWholeGame = (new StarTrekKGSettings());

            //_game = ((Game) Session["game"]);

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
                    responseLines = new List<string>
                    {
                        "Testing..",
                        "Testing..",
                        "1..2..3.."
                    };
                    break;

                case "clear session":
                    
                    break;

                case "make error":
                    throw new ArgumentException("This is a test error");

                default:

                    //pass command and session ID to application

                    responseLines = new List<string>
                    {
                        "Under Construction.."
                    };

                    //todo: Make sure that game.IsSubscriberApp is set by this point.
                    responseLines = _Default.GetGame(sessionID).SendAndGetResponse(command);

                    break;
            }

            string json = JsonConvert.SerializeObject(responseLines.ToList());

            return json;
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

        #endregion
    }
}