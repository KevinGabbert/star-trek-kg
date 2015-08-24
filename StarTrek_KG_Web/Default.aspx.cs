using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using Newtonsoft.Json;

namespace StarTrek_KG_Web
{
    public partial class _Default : Page
    {
        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.EnableViewState = false;  //don't need it cause we are not using it..
        }

        #endregion

        #region PageMethods

        [WebMethod]
        public static string QueryConsole(string command)
        {
            List<string> responseLines = new List<string>
                    {
                        "Testing..",
                        "Testing..",
                        "1..2..3.."
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

                case "make error":
                    throw new ArgumentException("This is a test error");

                default:

                    responseLines = new List<string>
                    {
                        "Not Implemented Yet."
                    };

                    break;
            }

            string json = JsonConvert.SerializeObject(responseLines.ToList());

            return json;
        }

        #endregion
    }
}