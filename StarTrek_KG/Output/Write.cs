using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
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

        private int ShieldsDownLevel { get; set; }
        private int LowEnergyLevel { get; set; }

        public Write(int shieldsDownLevel, int lowEnergyLevel)
        {
            this.ShieldsDownLevel = shieldsDownLevel;
            this.LowEnergyLevel = lowEnergyLevel;
        }

        public Write(int totalHostiles, int timeRemaining, int starbases, int stardate, int shieldsDownLevel,
                      int lowEnergyLevel)
        {
            this.TotalHostiles = totalHostiles;
            this.TimeRemaining = timeRemaining;
            this.Starbases = starbases;
            this.Stardate = stardate;

            this.ShieldsDownLevel = shieldsDownLevel;
            this.LowEnergyLevel = lowEnergyLevel;
        }

        //TODO:  Have Game expose and raise an output event
        //Have UI subscribe to it.

        //Output object
        //goal is to output message that a UI can read
        //all *print* mnemonics will be changed to Output
        //UI needs to read this text and display it how it wants

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
            Console.WriteLine();
            Console.WriteLine(StarTrekKGSettings.GetText("CSTimeRemaining"), map.timeRemaining);
            Console.WriteLine(StarTrekKGSettings.GetText("CSHostilesRemaining"), map.Quadrants.GetHostileCount());
            //Map.GetAllHostiles(map).Count
            Console.WriteLine(StarTrekKGSettings.GetText("CSHostilesInQuadrant"), currentQuadrant.GetHostiles().Count);
            Console.WriteLine(StarTrekKGSettings.GetText("CSStarbases"), map.starbases);
            Console.WriteLine(StarTrekKGSettings.GetText("CSWarpEngineDamage"), Navigation.For(ship).Damage);
            Console.WriteLine(StarTrekKGSettings.GetText("CSSRSDamage"), ShortRangeScan.For(ship).Damage);
            Console.WriteLine(StarTrekKGSettings.GetText("CSLRSDamage"), LongRangeScan.For(ship).Damage);
            Console.WriteLine(StarTrekKGSettings.GetText("CSShieldsDamage"), Shields.For(ship).Damage);
            Console.WriteLine(StarTrekKGSettings.GetText("CSComputerDamage"), computerDamage);
            Console.WriteLine(StarTrekKGSettings.GetText("CSPhotonDamage"), Torpedoes.For(ship).Damage);
            Console.WriteLine(StarTrekKGSettings.GetText("CSPhaserDamage"), Phasers.For(ship).Damage);
            Console.WriteLine();

            //foreach (var badGuy in currentQuadrant.Hostiles)
            //{
            //    
            //}

            Console.WriteLine();

            //todo: Display all baddie names in quadrant when encountered.
        }

        //output as KeyValueCollection, and UI will build the string
        public void PrintMission()
        {
            Console.WriteLine(StarTrekKGSettings.GetText("MissionStatement"), this.TotalHostiles, this.TimeRemaining,
                              this.Starbases);
            Console.WriteLine();
        }

        //This needs to be output as an array, List<List>, or KeyValueCollection, and the grid needs to be generated by the UI app
        public static void PrintGalacticRecord(List<Quadrant> Quadrants)
        {
            Console.WriteLine();
            var sb = new StringBuilder();
            ResourceLine("GalacticRecordLine");
            for (var quadrantLB = 0; quadrantLB < Constants.QUADRANT_MAX; quadrantLB++)
            {
                for (var quadrantUB = 0; quadrantUB < Constants.QUADRANT_MAX; quadrantUB++)
                {
                    sb.Append("| ");
                    const int starbaseCount = 0;
                    const int starCount = -1;

                    var quadrant = Playfield.Quadrants.Get(Quadrants, quadrantLB, quadrantUB);
                    if (quadrant.Scanned)
                    {
                        //starbaseCount = quadrant.Starbase ? 1 : 0;
                        //starCount = quadrant.Stars;
                    }

                    sb.Append(String.Format("{0}{1}{2} ", quadrant.GetHostiles().Count, starbaseCount, starCount));
                }

                sb.Append("|");
                Console.WriteLine(sb.ToString());
                sb.Length = 0;
                ResourceLine("GalacticRecordLine");
            }
            Console.WriteLine();
        }

        public void PrintSector(Quadrant quadrant, Map map)
        {
            var condition = this.GetCurrentCondition(quadrant, map);

            Location myLocation = map.Playership.GetLocation();
            int totalHostiles = map.Quadrants.GetHostileCount();
            bool docked = Navigation.For(map.Playership).docked;

            CreateViewScreen(quadrant, map, totalHostiles, condition, myLocation, docked);
            this.OutputWarnings(quadrant, map, docked);
        }

        private static void CreateViewScreen(Quadrant quadrant,
                                             Map map,
                                             int totalHostiles,
                                             string condition,
                                             Location location,
                                             bool docked)
        {
            var sb = new StringBuilder();
            var activeQuadrant = map.Quadrants.GetActive();

            Console.WriteLine(StarTrekKGSettings.GetText("SRSTopBorder", "SRSRegionIndicator"), quadrant.Name);

            ShowSectorRow(sb, 0,
                          String.Format(StarTrekKGSettings.GetText("SRSQuadrantIndicator"), location.Quadrant.X,
                                        location.Quadrant.Y), activeQuadrant.Sectors, totalHostiles);
            ShowSectorRow(sb, 1,
                          String.Format(StarTrekKGSettings.GetText("SRSSectorIndicator"), location.Sector.X,
                                        location.Sector.Y), activeQuadrant.Sectors, totalHostiles);
            ShowSectorRow(sb, 2, String.Format(StarTrekKGSettings.GetText("SRSStardateIndicator"), map.Stardate),
                          activeQuadrant.Sectors, totalHostiles);
            ShowSectorRow(sb, 3,
                          String.Format(StarTrekKGSettings.GetText("SRSTimeRemainingIndicator"), map.timeRemaining),
                          activeQuadrant.Sectors, totalHostiles);
            ShowSectorRow(sb, 4, String.Format(StarTrekKGSettings.GetText("SRSConditionIndicator"), condition),
                          activeQuadrant.Sectors, totalHostiles);
            ShowSectorRow(sb, 5, String.Format(StarTrekKGSettings.GetText("SRSEnergyIndicator"), map.Playership.Energy),
                          activeQuadrant.Sectors, totalHostiles);
            ShowSectorRow(sb, 6,
                          String.Format(StarTrekKGSettings.GetText("SRSShieldsIndicator"),
                                        Shields.For(map.Playership).Energy), activeQuadrant.Sectors, totalHostiles);
            ShowSectorRow(sb, 7,
                          String.Format(StarTrekKGSettings.GetText("SRSTorpedoesIndicator"),
                                        Torpedoes.For(map.Playership).Count), activeQuadrant.Sectors, totalHostiles);

            Console.WriteLine(StarTrekKGSettings.GetText("SRSBottomBorder", "SRSDockedIndicator"), docked);
        }

        private static void ShowSectorRow(StringBuilder sb, int row, string suffix, Sectors sectors, int totalHostiles)
        {
            for (var column = 0; column < Constants.SECTOR_MAX; column++)
            {
                var item = Sector.Get(sectors, row, column).Item;
                switch (item)
                {
                    case SectorItem.Empty:
                        sb.Append(Constants.EMPTY);
                        break;
                    case SectorItem.Friendly:
                        sb.Append(Constants.ENTERPRISE);
                        break;
                    case SectorItem.Hostile:

                        //bug can be viewed (and even tested here)
                        //if last hostile was destroyed, it wontbe removed from array.

                        //todo: resolve if issue
                        //if (this.game.Map.Hostiles.Count < 1)
                        //{
                        //    Console.WriteLine("bug. hostile not removed from display.");
                        //}

                        if (totalHostiles < 1)
                        {
                            Console.WriteLine("bug. hostile not removed from display.");
                        }

                        sb.Append(Constants.HOSTILE);
                        break;
                    case SectorItem.Star:
                        sb.Append(Constants.STAR);
                        break;
                    case SectorItem.Starbase:
                        sb.Append(Constants.STARBASE);
                        break;
                }
            }
            if (suffix != null)
            {
                sb.Append(suffix);
            }
            Console.WriteLine(sb.ToString());
            sb.Length = 0;
        }

        public static void Strings(IEnumerable<string> strings)
        {
            foreach (var str in strings)
            {
                Console.WriteLine(str);
            }
            Console.WriteLine();
        }

        public static void AppTitle()
        {
            const string appTitle = "AppTitle";
            for (int i = 1; i < 13; i++)
            {
                Resource(appTitle + i);
            }
        }

        public static void Panel(string panelHead, IEnumerable<string> strings)
        {
            Console.WriteLine();
            Console.WriteLine(panelHead);
            Console.WriteLine();

            foreach (var str in strings)
            {
                Console.WriteLine(str);
            }

            Console.WriteLine();
        }

        public static void Line(string stringToOutput)
        {
            Console.WriteLine(stringToOutput);
            Console.WriteLine();
        }

        public static void DebugLine(string stringToOutput)
        {
            if (Constants.DEBUG_MODE)
            {
                Console.WriteLine(stringToOutput);
            }
        }

        public static void Resource(string text)
        {
            Console.WriteLine(StarTrekKGSettings.GetText(text) + " ");
        }

        public static void ResourceLine(string text)
        {
            Console.WriteLine(StarTrekKGSettings.GetText(text));
            Console.WriteLine();
        }

        public static void ResourceLine(string prependText, string text)
        {
            Console.WriteLine(prependText + " " + StarTrekKGSettings.GetText(text));
            Console.WriteLine();
        }

        public static void SingleLine(string stringToOutput)
        {
            Console.WriteLine(stringToOutput);
        }

        public static void Prompt(string stringToOutput)
        {
            Console.Write(stringToOutput);
        }

        private void OutputWarnings(Quadrant quadrant, Map map, bool docked)
        {
            if (quadrant.GetHostiles().Count > 0)
            {
                this.ScanHostile(quadrant, map, docked);
            }
            else if (map.Playership.Energy < this.LowEnergyLevel) //todo: setting comes from app.config
            {
                Write.ResourceLine("LowEnergyLevel");
            }
        }

        private void ScanHostile(Quadrant quadrant, Map map, bool docked)
        {
            Console.WriteLine(StarTrekKGSettings.GetText("HostileDetected"),
                              (quadrant.GetHostiles().Count == 1 ? "" : "s"));

            foreach (var hostile in quadrant.GetHostiles())
            {
                Console.WriteLine(StarTrekKGSettings.GetText("IDHostile"), hostile.Name);
            }

            Console.WriteLine("");

            if (Shields.For(map.Playership).Energy == this.ShieldsDownLevel && !docked)
            {
                ResourceLine("ShieldsDown");
            }
        }

        private string GetCurrentCondition(Quadrant quadrant, Map map)
        {
            var condition = "GREEN";

            if (quadrant.GetHostiles().Count > 0)
            {
                condition = "RED";
            }
            else if (map.Playership.Energy < this.LowEnergyLevel)
            {
                condition = "YELLOW";
            }

            return condition;
        }

        public static void DisplayPropertiesOf(object @object)
        {
            if (@object != null)
            {
                var objectPropInfos = @object.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo prop in objectPropInfos)
                {
                    Console.WriteLine("{0} : {1}", prop.Name, prop.GetValue(@object, null));
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
    }
}


