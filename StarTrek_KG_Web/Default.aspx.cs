using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using Newtonsoft.Json;
using StarTrek_KG;
using StarTrek_KG_Web.WebApp;

namespace StarTrek_KG_Web
{
    public partial class _Default : Page
    {
        private static Workflow _WebAppWorkflow = new Workflow();

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.EnableViewState = false;  //don't need it cause we are not using it..

            //todo: create a turn system for the web app.  Then abstract as much as possible and put it in Star_Trek_KG
            //todo: it might just be that the web app "turn system" might have to stay in the web app, but I'm thinking 
            //todo: we could juts rewrite the one for the console app.
            //todo:  or I could just be safe and not touch the existing system and implement the new one in the web app :D

            ////todo: put all this BELOW in a business layer in StarTrek_KG_Web:
        }
            


        #endregion

        #region PageMethods

        [WebMethod]
        public static string QueryConsole(string command, string sessionID)
        {
            List<string> responseLines = new List<string>();

            Game game = _WebAppWorkflow.GetGame();

            switch (command)
            {
                case "test":
                    responseLines = _Default._WebAppWorkflow.SendTestResponse(responseLines);
                    break;

                case "make error":
                    throw new ArgumentException("This is a test error");

                case "stop":
                    responseLines = _Default._WebAppWorkflow.StopGame(responseLines);
                    break;

                case "start":
                    responseLines = _Default._WebAppWorkflow.StartGame(game, responseLines);
                    break;

                default:
                    responseLines = _Default._WebAppWorkflow.NewTurn(responseLines,  sessionID, command);
                    break;
            }

            string json = JsonConvert.SerializeObject(responseLines.ToList());
            return json;
        }

        #endregion
    }
}