using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using Microsoft.Ajax.Utilities;
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
        }

        #endregion

        #region PageMethods

        [WebMethod]
        public static string AutoComplete(string input)
        {
            //checks the session to see if it is not null.

            //checks the timer to report time left.

            //_Default will have a static system.timers.timer that when it hits a configured number, will clear the session
            //at a certain time.

            string retVal = "";

            switch (input)
            {
                case "aa":
                    retVal = "$aaa$";
                    break;
                case "bb":
                    retVal = "$bbb$";
                    break;
                default:
                    retVal = "";
                    break;
            }

            string json = JsonConvert.SerializeObject(retVal);
            return json;
        }

        [WebMethod]
        public static string Ping()
        {
            //checks the session to see if it is not null.

            //checks the timer to report time left.

            //_Default will have a static system.timers.timer that when it hits a configured number, will clear the session
            //at a certain time.

            string json = JsonConvert.SerializeObject("Time left: ");
            return json;
        }

        [WebMethod]
        public static string Prompt()
        {
            string currentPrompt = Workflow.GetGame()?.Write?.CurrentPrompt ?? "Terminal: ";

            string json = JsonConvert.SerializeObject(currentPrompt);
            return json;
        }

        [WebMethod]
        public static string QueryConsole(string command, string sessionID)
        {
            List<string> responseLines = new List<string>();

            Game game = Workflow.GetGame();
            responseLines = _WebAppWorkflow.ExecuteCommand(command, sessionID, responseLines, game);

            string json = JsonConvert.SerializeObject(responseLines.ToList());
            return json;
        }

        //WebMethod - GetSessionTimeout
        //WebMethod - ChangeConfigSetting

        #endregion
    }
}