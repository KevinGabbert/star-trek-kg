using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Actors;

namespace StarTrek_KG.Output
{
    public class Render: IInteractContainer, IConfig
    {
        #region Properties

        public IInteraction Interact { get; set; }
        public IStarTrekKGSettings Config { get; set; }

        #endregion

        public Render(IInteraction interact, IStarTrekKGSettings config)
        {
            this.Config = config;
            this.Interact = interact;
            this.Interact.Config = config;
        }

        public void CreateSRSViewScreen(ISector Sector, IMap map, Location shipLocation, int totalHostiles, string SectorDisplayName, bool isNebula, StringBuilder sectorScanStringBuilder)
        {
            this.Interact.Output.WriteLine(this.Config.GetText("SRSTopBorder", "SRSSector"), SectorDisplayName);

            int srsRows = Convert.ToInt32(this.GetConfigText("SRSRows"));
            var game = map.Game as Game;
            var noiseRows = game?.IsSystemsCascadeMode == true ? game.GetSystemsCascadeSrsNoiseLines() : 0;
            for (int i = 0; i < srsRows; i++) //todo: resource out
            {
                var forceNoise = noiseRows > 0 && i >= srsRows - noiseRows;
                this.ShowSectorRow(sectorScanStringBuilder, i, this.GetSRSRowIndicator(i, map, shipLocation), Sector.Coordinates, totalHostiles, isNebula, forceNoise);
            }

            this.Interact.Output.WriteLine(this.Config.GetText("SRSBottomBorder", "SRSDockedIndicator"), Navigation.For(map.Playership).Docked);
        }

        public void CreateCRSViewScreen(ISector Sector, IMap map, Location shipLocation, int totalHostiles, string SectorDisplayName, bool isNebula, StringBuilder sectorScanStringBuilder)
        {
            List<string> lrsResults = LongRangeScan.For(map.Playership).RunLRSScan(shipLocation);

            var topBorder = this.Config.GetText("CRSTopBorder");

            this.CRS_Sector_ScanLine(SectorDisplayName, topBorder, shipLocation);
            this.ScanLine(topBorder, $" Energy: {map.Playership.Energy}   Shields: {Shields.For(map.Playership).Energy}");

            int crsRows = Convert.ToInt32(this.Config.GetText("CRSRows"));
            var game = map.Game as Game;
            var noiseRows = game?.IsSystemsCascadeMode == true ? game.GetSystemsCascadeCrsNoiseLines() : 0;
            for (int i = 0; i < crsRows; i++) 
            {
                var rowIndicator = this.GetCRSRightTextLine(i, map, lrsResults, totalHostiles);
                if (game?.IsSystemsCascadeMode == true)
                {
                    rowIndicator = this.ApplyLrsBrownoutNoise(rowIndicator, game.GetSystemsCascadeLrsNoiseLevel());
                }
                var forceNoise = noiseRows > 0 && i >= crsRows - noiseRows;
                this.ShowSectorRow(sectorScanStringBuilder, i, rowIndicator, Sector.Coordinates, totalHostiles, isNebula, forceNoise);
            }

            string lrsBottom = null;
            if (lrsResults.Count == 7)
            {
                lrsBottom = " " + lrsResults[6];
            }

            this.ScanLine(this.Config.GetText("CRSBottomBorder"), lrsBottom + this.Config.GetText("AppVersion"));
        }

        private string GetSRSRowIndicator(int row, IMap map, Location location)
        {
            string indicator;
            switch (row)
            {
                case 0:
                    indicator = string.Format(this.Config.GetText("SRSCoordinateIndicator"),
                                              Convert.ToString(location.Coordinate.X),
                                              Convert.ToString(location.Coordinate.Y));
                    break;
                case 1:
                    indicator = string.Format(this.Config.GetText("SRSSectorIndicator"),
                                              Convert.ToString(location.Sector.X),
                                              Convert.ToString(location.Sector.Y));
                    break;
                case 2:
                    indicator = string.Format(this.Config.GetText("SRSStardateIndicator"), map.Stardate);
                    break;
                case 3:
                    indicator = string.Format(this.Config.GetText("SRSTimeRemainingIndicator"), map.timeRemaining);
                    break;
                case 4:
                    indicator = string.Format(this.Config.GetText("SRSConditionIndicator"), map.Playership.GetConditionAndSetIcon());
                    break;
                case 5:
                    indicator = string.Format(this.Config.GetText("SRSEnergyIndicator"), map.Playership.Energy);
                    break;
                case 6:
                    indicator = string.Format(this.Config.GetText("SRSShieldsIndicator"), Shields.For(map.Playership).Energy);
                    break;
                case 7:
                    indicator = string.Format(this.Config.GetText("SRSTorpedoesIndicator"), Torpedoes.For(map.Playership).Count);
                    break;
                default:
                    indicator = "";
                    break;
            }

            return " " + indicator;
        }

        private void ScanLine(string srsText, string rightSideText = "")
        {
            int textMeasurement = srsText.Length;

            var srsLine = new StringBuilder(srsText);

            srsLine.Remove(textMeasurement, srsLine.ToString().Length - textMeasurement);
            srsLine.Insert(textMeasurement, rightSideText);

            this.Interact.SingleLine(srsLine.ToString());
        }

        private void CRS_Sector_ScanLine(string SectorName, string topBorder, Location location)
        {
            int topBorderAreaMeasurement = topBorder.Length + 1;
            var regionLineBuilder = new StringBuilder($"Sector: {SectorName}".PadRight(topBorderAreaMeasurement));

            regionLineBuilder.Remove(topBorderAreaMeasurement, regionLineBuilder.ToString().Length - topBorderAreaMeasurement);

            var blackHoleMarker = this.GetBlackHoleSectorMarker(location?.Sector);
            string SectorIndicator =
                $" Coord: [{Convert.ToString(location.Coordinate.X)},{Convert.ToString(location.Coordinate.Y)}]  Sec: §{Convert.ToString(location.Sector.X)}.{Convert.ToString(location.Sector.Y)}{blackHoleMarker}";

            regionLineBuilder.Insert(topBorderAreaMeasurement, SectorIndicator);

            this.Interact.SingleLine(regionLineBuilder.ToString());
        }

        private string GetBlackHoleSectorMarker(Sector sector)
        {
            if (sector?.Scanned != true || sector.Coordinates == null)
            {
                return string.Empty;
            }

            return sector.Coordinates.Any(c => c.Item == CoordinateItem.BlackHole)
                ? this.SymbolCell("BlackHoleChar", "°").Trim()
                : string.Empty;
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
            if (lrsResults.Count > row - 1)
            {
                retVal += lrsResults[row - 2];
            }
            return retVal;
        }

        private void ShowSectorRow(StringBuilder sb, int row, string suffix, Coordinates sectors, int totalHostiles, bool isNebula, bool forceNoise = false)
        {
            if (forceNoise)
            {
                for (var column = 0; column < DEFAULTS.COORDINATE_MAX; column++)
                {
                    sb.Append(this.GetNoiseCell());
                }

                if (suffix != null)
                {
                    sb.Append(suffix);
                }

                this.ScanLine(sb.ToString());
                sb.Length = 0;
                return;
            }

            for (var column = 0; column < DEFAULTS.COORDINATE_MAX; column++)
            {
                Coordinate sector = sectors[column, row];

                switch (sector.Item)
                {
                    case CoordinateItem.Empty:

                        //todo: might be good to put some false positives here  (jsut throw in some random faction letters)
                        sb.Append(isNebula ? Utility.Utility.DamagedScannerUnit() : DEFAULTS.EMPTY);
                        break;

                    case CoordinateItem.PlayerShip:
                        sb.Append(DEFAULTS.PLAYERSHIP);
                        break;

                    case CoordinateItem.HostileShip:

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

                    case CoordinateItem.Star:

                        bool canActuallySeeStar = !isNebula || (isNebula && (Utility.Utility.Random.Next(10) == 6)); //todo: resource this out
                        if (canActuallySeeStar)
                        {
                            sb.Append(DEFAULTS.STAR);
                        }
                        else
                        {
                            if (isNebula)
                            {
                                sb.Append(Utility.Utility.DamagedScannerUnit());
                            }
                        }

                        break;

                    case CoordinateItem.Starbase:
                        //todo:  this.AppendFactionDesignator(sb, totalHostiles, sector);
                        //this code will be used when starbase is an object

                        sb.Append(DEFAULTS.STARBASE);
                        break;

                    case CoordinateItem.Deuterium:
                        sb.Append(" . ");
                        break;

                    case CoordinateItem.DeuteriumCloud:
                        sb.Append(" . ");
                        break;

                    case CoordinateItem.GraviticMine:
                        sb.Append(DEFAULTS.EMPTY);
                        break;

                    case CoordinateItem.GaseousAnomaly:
                        sb.Append(" ~ ");
                        break;

                    case CoordinateItem.TemporalRift:
                        sb.Append(this.SymbolCell("TemporalRiftChar", "/"));
                        break;

                    case CoordinateItem.SporeField:
                        sb.Append(DEFAULTS.EMPTY);
                        break;

                    case CoordinateItem.BlackHole:
                        sb.Append(this.SymbolCell("BlackHoleChar", "°"));
                        break;

                    case CoordinateItem.EnergyAnomaly:
                        var anomaly = sector.Object as EnergyAnomaly;
                        var glyph = anomaly?.Glyph;
                        if (string.IsNullOrWhiteSpace(glyph))
                        {
                            glyph = "~";
                        }

                        sb.Append($" {glyph} ");
                        break;

                    case CoordinateItem.Debug:
                        sb.Append(DEFAULTS.DEBUG_MARKER);
                        break;

                    default:
                        sb.Append(DEFAULTS.NULL_MARKER);
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

        private string GetNoiseCell()
        {
            var noise = "~=-";
            var c1 = noise[Utility.Utility.Random.Next(noise.Length)];
            var c2 = noise[Utility.Utility.Random.Next(noise.Length)];
            var c3 = noise[Utility.Utility.Random.Next(noise.Length)];
            return $"{c1}{c2}{c3}";
        }

        private string ApplyLrsBrownoutNoise(string input, int noisePercent)
        {
            if (string.IsNullOrWhiteSpace(input) || noisePercent <= 0)
            {
                return input;
            }

            var chars = input.ToCharArray();
            const string noisePool = "~=-";
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                {
                    continue;
                }

                if (Utility.Utility.Random.Next(100) < noisePercent)
                {
                    chars[i] = noisePool[Utility.Utility.Random.Next(noisePool.Length)];
                }
            }

            return new string(chars);
        }

        private string SymbolCell(string key, string defaultValue)
        {
            try
            {
                var configured = this.Config?.GetSetting<string>(key);
                if (!string.IsNullOrWhiteSpace(configured))
                {
                    defaultValue = configured.Trim();
                }
            }
            catch
            {
                // Use defaultValue
            }

            var symbol = string.IsNullOrEmpty(defaultValue) ? " " : defaultValue.Substring(0, 1);
            return $" {symbol} ";
        }

        private void AppendShipDesignator(StringBuilder sb, int totalHostiles, ICoordinate sector)
        {
            var ship = (IShip) sector.Object;

            //todo: get designator for faction

            string factionDesignator = this.Config.Get.FactionDetails(ship.Faction).designator;

            if (factionDesignator == "")
            {
                factionDesignator = "+?+"; //todo: resource this
            }

            //bug can be viewed (and even tested here)
            //if last hostile was destroyed, it wont be removed from array.

            //noticed in playthrough that last hostile couldn't be seen.  is it properly labeled?

            //todo: resolve if issue
            //if (this.game.Map.Hostiles.Count < 1)
            //{
            //    this.Write.Output.WriteLine("bug. hostile not removed from display.");
            //}

            if (totalHostiles < 1)
            {
                this.Interact.Output.WriteLine("bug. hostile not removed from display.");
            }

            //todo: hostile feds look like: ++-
            //if (sector.Object.Faction == Faction.Federation)
            //{
            //    sb.Append(Constants.FEDERATION);
            //}

            sb.Append(factionDesignator);
        }

        public void OutputScanWarnings(ISector Sector, IMap map, bool shieldsAutoRaised)
        {
            if (Sector.GetHostiles().Count > 0)
            {                
                this.SRSScanHostile(Sector);
            }

            this.Interact.OutputConditionAndWarnings(map.Playership, this.Config.GetSetting<int>("ShieldsDownLevel"));

            if (shieldsAutoRaised)
            {
                map.Write.Line($"Shields automatically raised to {Shields.For(map.Playership).Energy}");
            }
        }

        //todo: this function needs to be part of SRS
        private void SRSScanHostile(ISector Sector)
        {
            this.Interact.Line(string.Format(this.Config.GetText("HostileDetected"), Sector.GetHostiles().Count == 1 ? "" : "s"));

            bool inNebula = Sector.Type == SectorType.Nebulae;

            foreach (var hostile in Sector.GetHostiles())
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

                this.Interact.Line($"{this.Config.GetText("IDHostile")}{hostileName}");
            }

            this.Interact.Line("");
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }
    }
}


