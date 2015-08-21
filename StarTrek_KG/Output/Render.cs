using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Output
{
    public class Render: IWrite, IConfig
    {
        #region Properties

        public IOutputWrite Write { get; set; }
        public IStarTrekKGSettings Config { get; set; }

        #endregion

        public Render(IOutputWrite write, IStarTrekKGSettings config)
        {
            this.Config = config;
            this.Write = write;
            this.Write.Config = config;
        }

        public void CreateSRSViewScreen(IRegion Region, IMap map, Location shipLocation, int totalHostiles, string RegionDisplayName, bool isNebula, StringBuilder sectorScanStringBuilder)
        {
            this.Write.Console.WriteLine(this.Config.GetText("SRSTopBorder", "SRSRegion"), RegionDisplayName);

            int srsRows = Convert.ToInt32(this.Config.GetText("SRSRows"));
            for (int i = 0; i < srsRows; i++) //todo: resource out
            {
                this.ShowSectorRow(sectorScanStringBuilder, i, this.GetSRSRowIndicator(i, map, shipLocation), Region.Sectors, totalHostiles, isNebula);
            }

            this.Write.Console.WriteLine(this.Config.GetText("SRSBottomBorder", "SRSDockedIndicator"), Navigation.For(map.Playership).Docked);
        }

        public void CreateCRSViewScreen(IRegion Region, IMap map, Location shipLocation, int totalHostiles, string RegionDisplayName, bool isNebula, StringBuilder sectorScanStringBuilder)
        {
            List<string> lrsResults = LongRangeScan.For(map.Playership).RunLRSScan(shipLocation);

            var topBorder = this.Config.GetText("CRSTopBorder");

            this.CRS_Region_ScanLine(RegionDisplayName, topBorder, shipLocation);
            this.ScanLine(topBorder, $" Energy: {map.Playership.Energy}   Shields: {Shields.For(map.Playership).Energy}");

            int crsRows = Convert.ToInt32(this.Config.GetText("CRSRows"));
            for (int i = 0; i < crsRows; i++) 
            {
                var rowIndicator = this.GetCRSRightTextLine(i, map, lrsResults, totalHostiles);
                this.ShowSectorRow(sectorScanStringBuilder, i, rowIndicator, Region.Sectors, totalHostiles, isNebula);
            }

            string lrsBottom = null;
            if (lrsResults.Count() == 7)
            {
                lrsBottom = " " + lrsResults[6];
            }

            this.ScanLine(this.Config.GetText("CRSBottomBorder"), lrsBottom + this.Config.GetText("AppVersion"));
        }

        private string GetSRSRowIndicator(int row, IMap map, Location location)
        {
            string retVal = " ";

            switch (row)
            {
                case 0:
                    retVal += String.Format(this.Config.GetText("SRSRegionIndicator"), Convert.ToString(location.Region.X), Convert.ToString(location.Region.Y));
                    break;
                case 1:
                    retVal += String.Format(this.Config.GetText("SRSSectorIndicator"), Convert.ToString(location.Sector.X), Convert.ToString(location.Sector.Y));
                    break;
                case 2:
                    retVal += String.Format(this.Config.GetText("SRSStardateIndicator"), map.Stardate);
                    break;
                case 3:
                    retVal += String.Format(this.Config.GetText("SRSTimeRemainingIndicator"), map.timeRemaining);
                    break;
                case 4:
                    retVal += String.Format(this.Config.GetText("SRSConditionIndicator"), map.Playership.GetConditionAndSetIcon());
                    break;
                case 5:
                    retVal += String.Format(this.Config.GetText("SRSEnergyIndicator"), map.Playership.Energy);
                    break;
                case 6:
                    retVal += String.Format(this.Config.GetText("SRSShieldsIndicator"), Shields.For(map.Playership).Energy);
                    break;
                case 7:
                    retVal += String.Format(this.Config.GetText("SRSTorpedoesIndicator"), Torpedoes.For(map.Playership).Count);
                    break;
            }

            return retVal;
        }

        private void ScanLine(string srsText, string rightSideText = "")
        {
            int textMeasurement = srsText.Length;

            var srsLine = new StringBuilder(srsText);

            srsLine.Remove(textMeasurement, srsLine.ToString().Length - (textMeasurement));
            srsLine.Insert(textMeasurement, rightSideText);

            this.Write.SingleLine(srsLine.ToString());
        }

        private void CRS_Region_ScanLine(string RegionName, string topBorder, Location location)
        {
            int topBorderAreaMeasurement = topBorder.Length + 1;
            var regionLineBuilder = new StringBuilder($"Region: {RegionName}".PadRight(topBorderAreaMeasurement));

            regionLineBuilder.Remove(topBorderAreaMeasurement, regionLineBuilder.ToString().Length - (topBorderAreaMeasurement));

            var RegionIndicator =
                $" Coord: [{Convert.ToString(location.Region.X)},{Convert.ToString(location.Region.Y)}]  Sec: §{Convert.ToString(location.Sector.X)}.{Convert.ToString(location.Sector.Y)}";

            regionLineBuilder.Insert(topBorderAreaMeasurement, RegionIndicator);

            this.Write.SingleLine(regionLineBuilder.ToString());
        }

        private string GetCRSRightTextLine(int row, IMap map, IList<string> lrsResults, int totalHostiles)
        {
            string retVal = " ";

            switch (row)
            {
                case 0:
                    retVal += $"Torpedoes: {Torpedoes.For(map.Playership).Count}  Hostiles Left: {totalHostiles}";
                    break;

                case 1:
                    retVal += $"Time remaining: {map.timeRemaining}";
                    break;

                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:                 
                    retVal = addLine(row, lrsResults, retVal);
                    break;
            }

            return retVal;
        }

        private static string addLine(int row, IList<string> lrsResults, string retVal)
        {
            if (lrsResults.Count() > (row - 1))
            {
                retVal += lrsResults[row - 2];
            }
            return retVal;
        }

        private void ShowSectorRow(StringBuilder sb, int row, string suffix, Sectors sectors, int totalHostiles, bool isNebula)
        {
            for (var column = 0; column < Constants.SECTOR_MAX; column++)
            {
                Sector sector = Sector.Get(sectors, row, column);

                switch (sector.Item)
                {
                    case SectorItem.Empty:

                        //todo: might be good to put some false positives here  (jsut throw in some random faction letters)
                        sb.Append(isNebula ? Utility.Utility.DamagedScannerUnit() : Constants.EMPTY);
                        break;

                    case SectorItem.PlayerShip:
                        sb.Append(Constants.PLAYERSHIP);
                        break;

                    case SectorItem.HostileShip:

                        //todo: later it might be nice to have something act on this.. say.. more power to the sensors can change this value
                        bool canActuallySeeEnemy = isNebula && (Utility.Utility.Random.Next(10) == 5); //todo: resource this out

                        if (!isNebula || canActuallySeeEnemy)
                        {
                            this.AppendShipDesignator(sb, totalHostiles, sector);
                        }
                        else
                        {
                            if (isNebula)
                            {
                                sb.Append(Utility.Utility.DamagedScannerUnit());
                            }
                        }

                        break;

                    case SectorItem.Star:

                        bool canActuallySeeStar = (!isNebula) || (isNebula && (Utility.Utility.Random.Next(10) == 6)); //todo: resource this out
                        if (canActuallySeeStar)
                        {
                            sb.Append(Constants.STAR);
                        }
                        else
                        {
                            if (isNebula)
                            {
                                sb.Append(Utility.Utility.DamagedScannerUnit());
                            }
                        }

                        break;

                    case SectorItem.Starbase:
                        //todo:  this.AppendFactionDesignator(sb, totalHostiles, sector);
                        //this code will be used when starbase is an object

                        sb.Append(Constants.STARBASE);
                        break;

                    case SectorItem.Debug:
                        sb.Append(Constants.DEBUG_MARKER);
                        break;

                    default:
                        sb.Append(Constants.NULL_MARKER);
                        break;
                }
            }
            if (suffix != null)
            {
                sb.Append(suffix);
            }

            this.ScanLine(sb.ToString());
            sb.Length = 0;
        }

        private void AppendShipDesignator(StringBuilder sb, int totalHostiles, ISector sector)
        {
            var ship = (IShip) sector.Object;

            //todo: get designator for faction

            string factionDesignator = this.Config.Get.FactionDetails(ship.Faction).designator;

            if (factionDesignator == "")
            {
                factionDesignator = "+?+";
            }

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
                this.Write.Console.WriteLine("bug. hostile not removed from display.");
            }

            //todo: hostile feds look like: ++-
            //if (sector.Object.Faction == Faction.Federation)
            //{
            //    sb.Append(Constants.FEDERATION);
            //}

            sb.Append(factionDesignator);
        }

        public void OutputScanWarnings(IRegion Region, IMap map, bool shieldsAutoRaised)
        {
            if (Region.GetHostiles().Count > 0)
            {                
                this.SRSScanHostile(Region);
            }

            this.Write.OutputConditionAndWarnings(map.Playership, this.Config.GetSetting<int>("ShieldsDownLevel"));

            if (shieldsAutoRaised)
            {
                map.Write.Line($"Shields automatically raised to {Shields.For(map.Playership).Energy}");
            }
        }

        //todo: this function needs to be part of SRS
        private void SRSScanHostile(IRegion Region)
        {
            this.Write.Line(string.Format(this.Config.GetText("HostileDetected"), (Region.GetHostiles().Count == 1 ? "" : "s")));

            bool inNebula = Region.Type == RegionType.Nebulae;

            foreach (var hostile in Region.GetHostiles())
            {
                var hostileName = hostile.Name;

                if (inNebula)
                {
                    hostileName = "Unknown";
                }

                if (hostile.Faction == FactionName.Federation)
                {
                    hostileName = hostile.Name + " " + Game.GetFederationShipRegistration(hostile);
                }

                this.Write.Line($"{this.Config.GetText("IDHostile")}{hostileName}");
            }

            this.Write.Line("");
        }
    }
}
