﻿using System;
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

        public static void DockSuccess(string shipName)
        {
            ResourceLine(shipName, "SuccessfullDock");
        }

        public static void ComputerDamageMessage()
        {
            Resource("ComputerDamaged");
            ResourceLine("RepairsUnderway");
        }

        public static void ShortRangeScanDamageMessage()
        {
            Resource("SRSDamaged");
            ResourceLine("RepairsUnderway");
        }

        public static void LongRangeScanDamageMessage()
        {
            Resource("LRSDamaged");
            ResourceLine("RepairsUnderway");
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

        //output this as KeyValueCollection that the UI can display as it likes.
        public static void PrintCurrentStatus(Map map, int computerDamage, Ship ship, Quadrant currentQuadrant)
        {
            Command.Console.WriteLine("");
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSTimeRemaining"), map.timeRemaining);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSHostilesRemaining"), map.Quadrants.GetHostileCount());
            //Map.GetAllHostiles(map).Count
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSHostilesInQuadrant"), currentQuadrant.GetHostiles().Count);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSStarbases"), map.starbases);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSWarpEngineDamage"), Navigation.For(ship).Damage);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSSRSDamage"), ShortRangeScan.For(ship).Damage);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSLRSDamage"), LongRangeScan.For(ship).Damage);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSShieldsDamage"), Shields.For(ship).Damage);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSComputerDamage"), computerDamage);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSPhotonDamage"), Torpedoes.For(ship).Damage);
            Command.Console.WriteLine(StarTrekKGSettings.GetText("CSPhaserDamage"), Phasers.For(ship).Damage);
            Command.Console.WriteLine();

            //foreach (var badGuy in currentQuadrant.Hostiles)
            //{
            //    
            //}

            Command.Console.WriteLine();

            //todo: Display all baddie names in quadrant when encountered.
        }

        //output as KeyValueCollection, and UI will build the string
        public void PrintMission()
        {
            Command.Console.WriteLine(StarTrekKGSettings.GetText("MissionStatement"), this.TotalHostiles, this.TimeRemaining, this.Starbases);
            Command.Console.WriteLine();
        }

        //This needs to be output as an array, List<List>, or KeyValueCollection, and the grid needs to be generated by the UI app
        public static void PrintGalacticRecord(List<Quadrant> Quadrants)
        {
            Command.Console.WriteLine();
            var sb = new StringBuilder();
            Write.ResourceLine("GalacticRecordLine");

            for (var quadrantLB = 0; quadrantLB < Constants.QUADRANT_MAX; quadrantLB++)
            {
                for (var quadrantUB = 0; quadrantUB < Constants.QUADRANT_MAX; quadrantUB++)
                {
                    sb.Append("|");
                    string starbaseCount = "*";
                    string starCount = "*";
                    string hostileCount = "*";

                    var quadrant = Playfield.Quadrants.Get(Quadrants, new Coordinate(quadrantUB, quadrantLB));
                    if (quadrant.Scanned)
                    {
                        starbaseCount = quadrant.GetStarbaseCount().ToString();
                        starCount = quadrant.GetStarCount().ToString();
                        hostileCount = quadrant.GetHostiles().Count.ToString();
                    }

                    sb.Append(String.Format("{0}{1}{2}", hostileCount, starbaseCount, starCount));
                }

                sb.Append("|");
                Command.Console.WriteLine(sb.ToString());
                sb.Length = 0;
                ResourceLine("GalacticRecordLine");
            }
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

        public static void Panel(string panelHead, IEnumerable<string> strings)
        {
            Command.Console.WriteLine();
            Command.Console.WriteLine(panelHead);
            Command.Console.WriteLine();

            foreach (var str in strings)
            {
                Command.Console.WriteLine(str);
            }

            Command.Console.WriteLine();
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

        public static void ResourceLine(string prependText, string text)
        {
            Command.Console.WriteLine(prependText + " " + StarTrekKGSettings.GetText(text));
            Command.Console.WriteLine();
        }

        public static void SingleLine(string stringToOutput)
        {
            Command.Console.WriteLine(stringToOutput);
        }

        public static void Prompt(string stringToOutput)
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


