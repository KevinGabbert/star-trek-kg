using System;
using System.Text;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG.Output
{
    public class PrintSector
    {
        private Location Location { get; set; }
        public string Condition { get; set; }
        private int ShieldsDownLevel { get; set; }
        private int LowEnergyLevel { get; set; }

        public PrintSector(int shieldsDownLevel, int lowEnergyLevel)
        {
            this.ShieldsDownLevel = shieldsDownLevel;
            this.LowEnergyLevel = lowEnergyLevel;
        }

        public void Print(Quadrant quadrant, Map map)
        {
            var condition = this.GetCurrentCondition(quadrant, map);

            Location myLocation = map.Playership.GetLocation();
            int totalHostiles = map.Quadrants.GetHostileCount();
            bool docked = Navigation.For(map.Playership).docked;

            CreateViewScreen(quadrant, map, totalHostiles, condition, myLocation, docked);
            this.OutputWarnings(quadrant, map, docked);
        }

        private void CreateViewScreen(Quadrant quadrant,
                                             Map map,
                                             int totalHostiles,
                                             string condition,
                                             Location location,
                                             bool docked)
        {
            var sb = new StringBuilder();
            this.Condition = condition;
            this.Location = location;

            Console.WriteLine("");
            Console.WriteLine(StarTrekKGSettings.GetText("SRSTopBorder", "SRSRegionIndicator"), quadrant.Name);

            for (int i = 0; i < 8; i++ )
            {
                PrintSector.ShowSectorRow(sb, i, this.GetRowIndicator(i, map), quadrant.Sectors, totalHostiles);
            }

            Console.WriteLine(StarTrekKGSettings.GetText("SRSBottomBorder", "SRSDockedIndicator"), docked);
        }

        private string GetRowIndicator(int row, Map map)
        {
            string retVal = " ";

            switch (row)
            {
                case 0:
                    retVal += String.Format(StarTrekKGSettings.GetText("SRSQuadrantIndicator"), Convert.ToString(this.Location.Quadrant.X), Convert.ToString(this.Location.Quadrant.Y));
                    break;
                case 1:
                    retVal += String.Format(StarTrekKGSettings.GetText("SRSSectorIndicator"), Convert.ToString(this.Location.Sector.X), Convert.ToString(this.Location.Sector.Y));
                    break;
                case 2:
                    retVal += String.Format(StarTrekKGSettings.GetText("SRSStardateIndicator"), map.Stardate);
                    break;
                case 3:
                    retVal += String.Format(StarTrekKGSettings.GetText("SRSTimeRemainingIndicator"), map.timeRemaining);
                    break;
                case 4:
                    retVal += String.Format(StarTrekKGSettings.GetText("SRSConditionIndicator"), this.Condition);
                    break;
                case 5:
                    retVal += String.Format(StarTrekKGSettings.GetText("SRSEnergyIndicator"), map.Playership.Energy);
                    break;
                case 6:
                    retVal += String.Format(StarTrekKGSettings.GetText("SRSShieldsIndicator"), Shields.For(map.Playership).Energy);
                    break;
                case 7:
                    retVal += String.Format(StarTrekKGSettings.GetText("SRSTorpedoesIndicator"), Torpedoes.For(map.Playership).Count);
                    break;
            }

            return retVal;
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
                        sb.Append(Constants.PLAYERSHIP);
                        break;
                    case SectorItem.Hostile:

                        //bug can be viewed (and even tested here)
                        //if last hostile was destroyed, it wont be removed from array.

                        //noticed in playthrough that last hostile couldn't be seen.  is it properly labeled?

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
                Write.ResourceLine("ShieldsDown");
            }
        }
    }
}
