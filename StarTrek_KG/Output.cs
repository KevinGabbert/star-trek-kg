﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG
{
    public class Output
    {
        int TotalHostiles { get; set; }
        int TimeRemaining { get; set; }
        int Starbases { get; set; }
        int Stardate { get; set; }

        int ShieldsDownLevel { get; set; }
        int LowEnergyLevel { get; set; }

        public Output(int shieldsDownLevel, int lowEnergyLevel)
        {
            this.ShieldsDownLevel = shieldsDownLevel;
            this.LowEnergyLevel = lowEnergyLevel;
        }

        public Output(int totalHostiles, int timeRemaining, int starbases, int stardate, int shieldsDownLevel, int lowEnergyLevel)
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

        public static void DockSuccess()
        {
            Output.Write("Enterprise successfully docked with starbase."); 
        }            
            
        public static void ComputerDamageMessage()
        {
            Output.Write("The main computer is damaged. Repairs are underway.");
        }

        public static void ShortRangeScanDamageMessage()
        {
            Output.Write("Short range scanner is damaged. Repairs are underway.");
        }

        public static void LongRangeScanDamageMessage()
        {
            Output.Write("Long range scanner is damaged. Repairs are underway.");
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

            Output.Write(missionEndResult);
        }

        //output this as KeyValueCollection that the UI can display as it likes.
        public static void PrintCurrentStatus(Map map, int computerDamage, int shieldControlDamage, int navigationDamage, int shortRangeScanDamage, int longRangeScanDamage, int photonDamage, int phaserDamage, Quadrant currentQuadrant)
        {
            Console.WriteLine();
            Console.WriteLine("               Time Remaining: {0}", map.timeRemaining);
            Console.WriteLine("      Hostile Ships Remaining: {0}", map.Quadrants.GetHostileCount()); //Map.GetAllHostiles(map).Count
            Console.WriteLine("         Hostiles in Quadrant: {0}", currentQuadrant.Hostiles);     
            Console.WriteLine("                    Starbases: {0}", map.starbases);
            Console.WriteLine("           Warp Engine Damage: {0}", navigationDamage);
            Console.WriteLine("   Short Range Scanner Damage: {0}", shortRangeScanDamage);
            Console.WriteLine("    Long Range Scanner Damage: {0}", longRangeScanDamage);
            Console.WriteLine("       Shield Controls Damage: {0}", shieldControlDamage);
            Console.WriteLine("         Main Computer Damage: {0}", computerDamage);
            Console.WriteLine("Photon Torpedo Control Damage: {0}", photonDamage);
            Console.WriteLine("                Phaser Damage: {0}", phaserDamage);
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
            Console.WriteLine("Mission: Destroy {0} Hostile ships in {1} stardates with {2} starbases.",
                              this.TotalHostiles, this.TimeRemaining, this.Starbases);

            Console.WriteLine();
        }

        //This needs to be output as an array, List<List>, or KeyValueCollection, and the grid needs to be generated by the UI app
        public static void PrintGalacticRecord(List<Quadrant> Quadrants)
        {
            Console.WriteLine();
            var sb = new StringBuilder();
            Console.WriteLine("-------------------------------------------------");
            for (var x = 0; x < Constants.QUADRANT_MAX; x++)
            {
                for (var y = 0; y < Constants.QUADRANT_MAX; y++)
                {
                    sb.Append("| ");
                    var starbaseCount = 0;
                    var starCount = -1;

                    Quadrant quadrant = StarTrek_KG.Playfield.Quadrants.Get(Quadrants, x, y);
                    if (quadrant.Scanned)
                    {
                        //starbaseCount = quadrant.Starbase ? 1 : 0;
                        //starCount = quadrant.Stars;
                    }

                    sb.Append(String.Format("{0}{1}{2} ", quadrant.Hostiles.Count, starbaseCount, starCount));
                }
                sb.Append("|");
                Console.WriteLine(sb.ToString());
                sb.Length = 0;
                Console.WriteLine("-------------------------------------------------");
            }
            Console.WriteLine();
        }

        public void PrintSector(Quadrant quadrant, Map map)
        {
            var condition = this.GetCurrentConditon(quadrant, map);

            var myLocation = map.Playership.GetLocation();
            var totalHostiles = map.Quadrants.GetHostileCount(); 
            var docked = Navigation.For(map.Playership).docked;

            Output.CreateDisplay(quadrant, map, totalHostiles, condition, myLocation, docked);
            this.OutputWarnings(quadrant, map, docked);
        }

        private static void CreateDisplay(Quadrant quadrant, 
                                          Map map, 
                                          int totalHostiles, 
                                          string condition,
                                          Location location, 
                                          bool docked)
        {
            var sb = new StringBuilder();
            var activeQuadrant = map.Quadrants.GetActive();

            Console.WriteLine("|=--=--=--=--=--=--=--=|             Region: {0}", quadrant.Name);

            Output.PrintSectorRow(sb, 0, String.Format("           Quadrant: [{0},{1}]", location.Quadrant.X, location.Quadrant.Y), activeQuadrant.Sectors, totalHostiles);
            Output.PrintSectorRow(sb, 1, String.Format("             Sector: [{0},{1}]", location.Sector.X, location.Sector.Y), activeQuadrant.Sectors, totalHostiles);
            Output.PrintSectorRow(sb, 2, String.Format("           Stardate: {0}", map.Stardate), activeQuadrant.Sectors, totalHostiles);
            Output.PrintSectorRow(sb, 3, String.Format("     Time remaining: {0}", map.timeRemaining), activeQuadrant.Sectors, totalHostiles);
            Output.PrintSectorRow(sb, 4, String.Format("          Condition: {0}", condition), activeQuadrant.Sectors, totalHostiles);
            Output.PrintSectorRow(sb, 5, String.Format("             Energy: {0}", map.Playership.Energy), activeQuadrant.Sectors, totalHostiles);
            Output.PrintSectorRow(sb, 6, String.Format("            Shields: {0}", Shields.For(map.Playership).Energy), activeQuadrant.Sectors, totalHostiles);
            Output.PrintSectorRow(sb, 7, String.Format("   Photon Torpedoes: {0}", Torpedoes.For(map.Playership).Count), activeQuadrant.Sectors, totalHostiles);

            Console.WriteLine("|=--=--=--=--=--=--=--=|             Docked: {0}", docked);
        }

        private static void PrintSectorRow(StringBuilder sb, int row, string suffix, Sectors sectors, int totalHostiles)
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

        public static void PrintStrings(IEnumerable<string> strings)
        {
            foreach (var str in strings)
            {
                Console.WriteLine(str);
            }
            Console.WriteLine();
        }

        public static void PrintPanel(string panelHead, IEnumerable<string> strings)
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

        public static void Write(string stringToOutput)
        {
            Console.WriteLine(stringToOutput);
            Console.WriteLine();
        }

        public static void WriteSingleLine(string stringToOutput)
        {
            Console.WriteLine(stringToOutput);
        }

        public static void Prompt(string stringToOutput)
        {
            Console.Write(stringToOutput);
        }

        private void OutputWarnings(Quadrant quadrant, Map map, bool docked)
        {
            if (quadrant.Hostiles.Count > 0)
            {
                Console.WriteLine("Condition RED: Hostile{0} detected.", (quadrant.Hostiles.Count == 1 ? "" : "s"));

                foreach (var hostile in quadrant.Hostiles)
                {
                    Console.WriteLine("Hostile identified as: " + hostile.Name);
                }

                Console.WriteLine("");

                if (Shields.For(map.Playership).Energy == this.ShieldsDownLevel && !docked)
                {
                    Output.Write("Warning: Shields are down.");
                }
            }
            else if (map.Playership.Energy < this.LowEnergyLevel) //todo: setting comes from app.config
            {
                Output.Write("Condition YELLOW: Low energy level.");
            }
        }
        private string GetCurrentConditon(Quadrant quadrant, Map map)
        {
            var condition = "GREEN";

            if (quadrant.Hostiles.Count > 0)
            {
                condition = "RED";
            }
            else if (map.Playership.Energy < this.LowEnergyLevel)
            {
                condition = "YELLOW";
            }

            return condition;
        }
    }
}
