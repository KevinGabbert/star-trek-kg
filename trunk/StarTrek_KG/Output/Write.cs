using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG.Output
{
    /// <summary>
    /// todo: the goal here is to be able to save all output to a file for later printing..
    /// </summary>
    public class Write
    {
        private int TotalHostiles { get; set; }
        private int TimeRemaining { get; set; }
        private int Starbases { get; set; }
        private int Stardate { get; set; }

        //TODO:  Have Game expose and raise an output event
        //Have UI subscribe to it.

        //Output object
        //goal is to output message that a UI can read
        //all *print* mnemonics will be changed to Output
        //UI needs to read this text and display it how it wants

        public Write()
        {

        }

        public Write(int totalHostiles, int starbases, int stardate, int timeRemaining)
        {
            this.TotalHostiles = totalHostiles;
            this.Starbases = starbases;
            this.Stardate = stardate;
            this.TimeRemaining = timeRemaining;
        }

        //missionResult needs to be an enum
        public void PrintCommandResult(Ship ship)
        {
            var missionEndResult = String.Empty;

            if (ship.Destroyed)
            {
                missionEndResult = "MISSION FAILED: " + ship.Name.ToUpper() + " DESTROYED";
            }
            else if (ship.Energy == 0)
            {
                missionEndResult = "MISSION FAILED: " + ship.Name.ToUpper() + " RAN OUT OF ENERGY.";
            }
            else if (this.TotalHostiles == 0)
            {
                missionEndResult = "MISSION ACCOMPLISHED: ALL HOSTILE SHIPS DESTROYED. WELL DONE!!!";
            }
            else if (this.TimeRemaining == 0)
            {
                missionEndResult = "MISSION FAILED: " + ship.Name.ToUpper() + " RAN OUT OF TIME.";
            }

            //else - No status to report.  Game continues

            Line(missionEndResult);
        }

        //output as KeyValueCollection, and UI will build the string
        public void PrintMission()
        {
            Command.Console.WriteLine(StarTrekKGSettings.GetText("MissionStatement"), this.TotalHostiles, this.TimeRemaining, this.Starbases);
            Command.Console.WriteLine();
        }

        public static void Strings(IEnumerable<string> strings)
        {
            foreach (var str in strings)
            {
                Command.Console.WriteLine(str);
            }
            Command.Console.WriteLine();
        }

        public static void HighlightTextBW(bool on)
        {
            Command.Console.HighlightTextBW(on);
        }

        public static void Line(string stringToOutput)
        {
            Command.Console.WriteLine(stringToOutput);
            Command.Console.WriteLine();
        }

        public static void DebugLine(string stringToOutput)
        {
            if (Constants.DEBUG_MODE)
            {
                Command.Console.WriteLine(stringToOutput);
            }
        }

        public static void Resource(string text)
        {
            Command.Console.WriteLine(StarTrekKGSettings.GetText(text) + " ");
        }

        public static void ResourceLine(string text)
        {
            Command.Console.WriteLine(StarTrekKGSettings.GetText(text));
            Command.Console.WriteLine();
        }

        public static void ResourceSingleLine(string text)
        {
            Command.Console.WriteLine(StarTrekKGSettings.GetText(text));
        }

        public static void ResourceLine(string prependText, string text)
        {
            Command.Console.WriteLine(prependText + " " + StarTrekKGSettings.GetText(text));
            Command.Console.WriteLine();
        }

        public static void SingleLine(string stringToOutput)
        {
            Command.Console.WriteLine(stringToOutput);
        }

        public static void WithNoEndCR(string stringToOutput)
        {
            Command.Console.Write(stringToOutput);
        }

        public static void DisplayPropertiesOf(object @object)
        {
            if (@object != null)
            {
                var objectPropInfos = @object.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo prop in objectPropInfos)
                {
                    Command.Console.WriteLine("{0} : {1}", prop.Name, prop.GetValue(@object, null));
                }
            }

            //try
            //{
            //    List<ObjectWalkerEntity> x = ObjectWalker.Walk(@object);
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
        }

        //public void PrintSector(Quadrant quadrant, Map map)
        //{
        //    var condition = this.GetCurrentCondition(quadrant, map);

        //    Location myLocation = map.Playership.GetLocation();
        //    int totalHostiles = map.Quadrants.GetHostileCount();
        //    bool docked = Navigation.For(map.Playership).docked;

        //    Output.PrintSector.CreateViewScreen(quadrant, map, totalHostiles, condition, myLocation, docked);
        //    this.OutputWarnings(quadrant, map, docked);
        //}

        //private string GetRowIndicator(int row, Map map)
        //{
        //    string retVal = "";

        //    switch (row)
        //    {
        //        case 0:
        //            retVal = String.Format(StarTrekKGSettings.GetText("SRSQuadrantIndicator"), location.Quadrant.X, location.Quadrant.Y);
        //            break;
        //        case 1:
        //            retVal = String.Format(StarTrekKGSettings.GetText("SRSSectorIndicator"), location.Sector.X, location.Sector.Y);
        //            break;
        //        case 2:
        //            retVal = String.Format(StarTrekKGSettings.GetText("SRSStardateIndicator"), map.Stardate);
        //            break;
        //        case 3:
        //            retVal = String.Format(StarTrekKGSettings.GetText("SRSTimeRemainingIndicator"), map.timeRemaining);
        //            break;
        //        case 4:
        //            retVal = String.Format(StarTrekKGSettings.GetText("SRSConditionIndicator"), condition);
        //            break;
        //        case 5:
        //            retVal = String.Format(StarTrekKGSettings.GetText("SRSEnergyIndicator"), map.Playership.Energy);
        //            break;
        //        case 6:
        //            retVal = String.Format(StarTrekKGSettings.GetText("SRSShieldsIndicator"), Shields.For(map.Playership).Energy);
        //            break;
        //        case 7:
        //            retVal = String.Format(StarTrekKGSettings.GetText("SRSTorpedoesIndicator"), Torpedoes.For(map.Playership).Count);
        //            break;
        //    }

        //    return retVal;
        //}
    }
}


