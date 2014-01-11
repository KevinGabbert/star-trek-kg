using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG.Output
{
    /// <summary>
    /// todo: the goal here is to be able to save all output to a file for later printing..
    /// </summary>
    public class Write: ICommand
    {
        private int TotalHostiles { get; set; }
        private int TimeRemaining { get; set; }
        private int Starbases { get; set; }
        private int Stardate { get; set; }

        public Command Command { get; set; } 

        //TODO:  Have Game expose and raise an output event
        //Have UI subscribe to it.

        //Output object
        //goal is to output message that a UI can read
        //all *print* mnemonics will be changed to Output
        //UI needs to read this text and display it how it wants

        public Write()
        {

        }

        public Write(Command commandObject)
        {
            if(commandObject == null)
            {
                throw new GameException("null dependency");
            }
            this.Command = commandObject;
        }

        public Write(int totalHostiles, int starbases, int stardate, int timeRemaining, Command commandObject)
        {
            this.Command = Command;
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
            this.Command.Console.WriteLine(StarTrekKGSettings.GetText("MissionStatement"), this.TotalHostiles, this.TimeRemaining, this.Starbases);
            this.Command.Console.WriteLine();
        }

        public void Strings(IEnumerable<string> strings)
        {
            foreach (var str in strings)
            {
                this.Command.Console.WriteLine(str);
            }
            this.Command.Console.WriteLine();
        }

        public void HighlightTextBW(bool on)
        {
            this.Command.Console.HighlightTextBW(on);
        }

        public void Line(string stringToOutput)
        {
            this.Command.Console.WriteLine(stringToOutput);
            this.Command.Console.WriteLine();
        }

        public void DebugLine(string stringToOutput)
        {
            if (Constants.DEBUG_MODE)
            {
                this.Command.Console.WriteLine(stringToOutput);
            }
        }

        public void Resource(string text)
        {
            this.Command.Console.WriteLine(StarTrekKGSettings.GetText(text) + " ");
        }

        public void ResourceLine(string text)
        {
            this.Command.Console.WriteLine(StarTrekKGSettings.GetText(text));
            this.Command.Console.WriteLine();
        }

        public void ResourceSingleLine(string text)
        {
            this.Command.Console.WriteLine(StarTrekKGSettings.GetText(text));
        }

        public void ResourceLine(string prependText, string text)
        {
            this.Command.Console.WriteLine(prependText + " " + StarTrekKGSettings.GetText(text));
            this.Command.Console.WriteLine();
        }

        public void SingleLine(string stringToOutput)
        {
            this.Command.Console.WriteLine(stringToOutput);
        }

        public void WithNoEndCR(string stringToOutput)
        {
            this.Command.Console.Write(stringToOutput);
        }

        public void DisplayPropertiesOf(object @object)
        {
            if (@object != null)
            {
                var objectPropInfos = @object.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo prop in objectPropInfos)
                {
                    this.Command.Console.WriteLine("{0} : {1}", prop.Name, prop.GetValue(@object, null));
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


