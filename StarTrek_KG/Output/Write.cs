using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Extensions.System;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using Console = StarTrek_KG.Utility.Console;

namespace StarTrek_KG.Output
{
    /// <summary>
    /// todo: the goal here is to be able to save all output to a file for later printing..
    /// </summary>
    public class Write: IConfig, IOutputWrite
    {
        public IStarTrekKGSettings Config { get; set; }
        public int TotalHostiles { get; set; }
        public int TimeRemaining { get; set; }
        public int Starbases { get; set; }
        public int Stardate { get; set; }

        //todo: make this non-static so we can test this class..

        private Console _console;
        public Console Console
        {
            get { return _console ?? (_console = new Console()); }
            set { _console = value; }
        }

        public List<string> ACTIVITY_PANEL  { get; set; }

        //TODO:  Have Game expose and raise an output event
        //Have UI subscribe to it.

        //Output object
        //goal is to output message that a UI can read
        //all *print* mnemonics will be changed to Output
        //UI needs to read this text and display it how it wants

        public Write(int totalHostiles, int starbases, int stardate, int timeRemaining, IStarTrekKGSettings config)
        {
            this.ACTIVITY_PANEL = new List<string>();
            this.Config = config;
            this.TotalHostiles = totalHostiles;
            this.Starbases = starbases;
            this.Stardate = stardate;
            this.TimeRemaining = timeRemaining;
        }

        public Write(IStarTrekKGSettings config)
        {
            this.ACTIVITY_PANEL = new List<string>();
            this.Config = config;
        }

        //missionResult needs to be an enum
        public void PrintCommandResult(Ship ship, bool starbasesAreHostile, int starbasesLeft)
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
            else if (starbasesAreHostile && starbasesLeft == 0)
            {
                missionEndResult = "ALL FEDERATION STARBASES DESTROYED. YOU HAVE DEALT A SEVERE BLOW TO THE FEDERATION!";
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
            this.Console.WriteLine(this.Config.GetText("MissionStatement"), this.TotalHostiles, this.TimeRemaining, this.Starbases);
            this.Console.WriteLine();
        }

        public void Strings(IEnumerable<string> strings)
        {
            foreach (var str in strings)
            {
                this.Console.WriteLine(str);
            }
            this.Console.WriteLine();
        }

        public void HighlightTextBW(bool on)
        {
            this.Console.HighlightTextBW(on);
        }

        public void Line(string stringToOutput)
        {
            this.Console.WriteLine(stringToOutput);
            this.Console.WriteLine();
        }

        public void DebugLine(string stringToOutput)
        {
            if (Constants.DEBUG_MODE)
            {
                this.Console.WriteLine(stringToOutput);
            }
        }

        public void Resource(string text)
        {
            this.Console.WriteLine(this.Config.GetText(text) + " ");
        }

        public void ResourceLine(string text)
        {
            this.Console.WriteLine(this.Config.GetText(text));
            this.Console.WriteLine();
        }

        public void ResourceSingleLine(string text)
        {
            this.Console.WriteLine(this.Config.GetText(text));
        }

        public void ResourceLine(string prependText, string text)
        {
            this.Console.WriteLine(prependText + " " + this.Config.GetText(text));
            this.Console.WriteLine();
        }

        public void SingleLine(string stringToOutput)
        {
            this.Console.WriteLine(stringToOutput);
        }

        public void WithNoEndCR(string stringToOutput)
        {
            this.Console.Write(stringToOutput);
        }

        public void DisplayPropertiesOf(object @object)
        {
            if (@object != null)
            {
                var objectPropInfos = @object.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo prop in objectPropInfos)
                {
                    this.Console.WriteLine("{0} : {1}", prop.Name, prop.GetValue(@object, null));
                }
            }

        }

        public void RenderRegionCounts(bool renderingMyLocation, int starbaseCount, int starCount, int hostileCount)
        {
            if (renderingMyLocation)
            {
                this.HighlightTextBW(true);
            }

            this.WithNoEndCR(hostileCount.FormatForLRS());
            this.WithNoEndCR(starbaseCount.FormatForLRS());
            this.WithNoEndCR(starCount.FormatForLRS());

            if (renderingMyLocation)
            {
                this.HighlightTextBW(false);
            }
        }

        public string RenderRegionCounts(int starbaseCount, int starCount, int hostileCount)
        {
            string counts = "";

            //todo: Can we get unicode reversed text?
            //if (renderingMyLocation)
            //{
            //    this.HighlightTextBW(true);
            //}

            counts += hostileCount.FormatForLRS();
            counts += starbaseCount.FormatForLRS();
            counts += starCount.FormatForLRS();

            //if (renderingMyLocation)
            //{
            //    this.HighlightTextBW(false);
            //}

            return counts;
        }

        public void RenderUnscannedRegion(bool renderingMyLocation)
        {
            if (renderingMyLocation)
            {
                this.HighlightTextBW(true);
            }

            this.WithNoEndCR(" ");
            this.WithNoEndCR("?");
            this.WithNoEndCR(" ");

            if (renderingMyLocation)
            {
                this.HighlightTextBW(false);
            }
        }

        public void RenderNebula(bool renderingMyLocation)
        {
            if (renderingMyLocation)
            {
                this.HighlightTextBW(true);
            }

            this.WithNoEndCR("N");
            this.WithNoEndCR("N");
            this.WithNoEndCR("N");

            if (renderingMyLocation)
            {
                this.HighlightTextBW(false);
            }
        }

        public void CreateCommandPanel()
        {
            ACTIVITY_PANEL = new List<string>();

            ACTIVITY_PANEL.Add("imp = Impulse Navigation");
            ACTIVITY_PANEL.Add("wrp = Warp Navigation");
            ACTIVITY_PANEL.Add("nto = Navigate To Object");
            ACTIVITY_PANEL.Add("─────────────────────────────");
            ACTIVITY_PANEL.Add("irs = Immediate Range Scan");
            ACTIVITY_PANEL.Add("srs = Short Range Scan");
            ACTIVITY_PANEL.Add("lrs = Long Range Scan");
            ACTIVITY_PANEL.Add("crs = Combined Range Scan");
            ACTIVITY_PANEL.Add("─────────────────────────────");
            ACTIVITY_PANEL.Add("pha = Phaser Control");
            ACTIVITY_PANEL.Add("tor = Photon Torpedo Control");
            ACTIVITY_PANEL.Add("toq = Target Object in this Region");
            ACTIVITY_PANEL.Add("─────────────────────────────");
            ACTIVITY_PANEL.Add("she = Shield Control");
            ACTIVITY_PANEL.Add("com = Access Computer");
            ACTIVITY_PANEL.Add("dmg = Damage Control");

            if(Constants.DEBUG_MODE)
            {
                ACTIVITY_PANEL.Add("");
                ACTIVITY_PANEL.Add("─────────────────────────────");
                ACTIVITY_PANEL.Add("dbg = Debug Test Mode");
            }
        }

        //This needs to be a command method that the UI passes values into.
        //Readline is done in UI

        public void Prompt(Ship playerShip, string mapText, Game game)
        {
            this.Console.Write(mapText);

            var readLine = this.Console.ReadLine();
            if (readLine == null) return;

            var command = readLine.Trim().ToLower();
            switch (command)
            {
                case "wrp":
                case "imp":
                case "nto":
                    Navigation.For(playerShip).Controls(command);
                    break;

                case "irs":
                    ImmediateRangeScan.For(playerShip).Controls();
                    break;

                case "srs":
                    ShortRangeScan.For(playerShip).Controls();
                    break;

                case "lrs":
                    LongRangeScan.For(playerShip).Controls();
                    break;

                case "crs":
                    CombinedRangeScan.For(playerShip).Controls();
                    break;

                case "pha": 
                    Phasers.For(playerShip).Controls(playerShip);
                    break;

                case "tor":
                    Torpedoes.For(playerShip).Controls();
                    break;

                case "she":
                    this.ShieldMenu(playerShip);
                    break;

                case "com":
                    this.ComputerMenu(playerShip);
                    break;

                case "toq":
                    Computer.For(playerShip).Controls(command);
                    break;

                case "dmg":
                    this.DamageControlMenu(playerShip);
                    break;

                case "dbg":
                    this.DebugMenu(playerShip);
                    break;

                default: //case "?":
                    this.CreateCommandPanel();
                    this.Panel(this.GetPanelHead(playerShip.Name), ACTIVITY_PANEL);
                    break;
            }
        }

        public void Panel(string panelHead, IEnumerable<string> strings)
        {
            this.Console.WriteLine();
            this.Console.WriteLine(panelHead);
            this.Console.WriteLine();

            foreach (var str in strings)
            {
                this.Console.WriteLine(str);
            }

            this.Console.WriteLine();
        }

        private void DebugMenu(IShip playerShip)
        {
            this.Strings(Debug.CONTROL_PANEL);
            this.WithNoEndCR("Enter Debug command: ");

            //todo: readline needs to be done using an event
            var debugCommand = Console.ReadLine().Trim().ToLower();

            Debug.For(playerShip).Controls(debugCommand);
        }

        private void ComputerMenu(IShip playerShip)
        {
            if (Computer.For(playerShip).Damaged()) return;

            this.Strings(Computer.CONTROL_PANEL);
            this.WithNoEndCR("Enter computer command: ");

            //todo: readline needs to be done using an event
            var computerCommand = Console.ReadLine().Trim().ToLower();

            Computer.For(playerShip).Controls(computerCommand);
        }

        private void DamageControlMenu(IShip playerShip)
        {
            this.Strings(DamageControl.CONTROL_PANEL);
            this.WithNoEndCR("Enter Damage Control Command: ");

            //todo: readline needs to be done using an event
            var damageControlCommand = Console.ReadLine().Trim().ToLower();

            DamageControl.For(playerShip).Controls(damageControlCommand);
        }

        private void ShieldMenu(IShip playerShip)
        {
            if (Shields.For(playerShip).Damaged()) return;

            Shields.SHIELD_PANEL = new List<string>();
            Shields.SHIELD_PANEL.Add(Environment.NewLine);

            var currentShieldEnergy = Shields.For(playerShip).Energy;

            if (currentShieldEnergy > 0)
            {
                Shields.SHIELD_PANEL.Add("─── Shield Control: ── <CURRENTLY AT: " + currentShieldEnergy + "> ──");
                Shields.SHIELD_PANEL.Add("add = Add energy to shields.");
                Shields.SHIELD_PANEL.Add("sub = Subtract energy from shields.");
            }
            else
            {
                Shields.SHIELD_PANEL.Add("─── Shield Control: ── <DOWN> ──");
                Shields.SHIELD_PANEL.Add("add = Add energy to shields.");
            }

            this.Strings(Shields.SHIELD_PANEL);

            this.WithNoEndCR("Enter shield control command: ");
            var shieldsCommand = Console.ReadLine().Trim().ToLower();

            Shields.For(playerShip).MaxTransfer = playerShip.Energy; //todo: this does nothing!
            Shields.For(playerShip).Controls(shieldsCommand);
        }

        public string GetPanelHead(string shipName)
        {
            return "─── " + shipName + " ───";
        }

        public bool PromptUser(string promptMessage, out int value)
        {
            try
            {
                this.WithNoEndCR(promptMessage);

                value = int.Parse(this.Console.ReadLine());

                return true;
            }
            catch
            {
                value = 0;
            }

            return false;
        }

        public bool PromptUser(string promptMessage, out string value)
        {
            value = null;

            try
            {
                this.Console.Write(promptMessage);

                var readLine = this.Console.ReadLine();
                if (readLine != null) value = readLine.ToLower();

                return true;
            }
            catch 
            {
                value = "";
            }

            return false;
        }

        public static string ShipHitMessage(IShip attacker)
        {
            var attackerRegion = attacker.GetRegion();
            var attackerSector = Utility.Utility.HideXorYIfNebula(attackerRegion, attacker.Sector.X.ToString(), attacker.Sector.Y.ToString());

            string attackerName = attackerRegion.Type == RegionType.Nebulae ? "Unknown Ship" : attacker.Name;

            if (attacker.Faction == FactionName.Federation)
            {
                attackerName = attacker.Name;
            }

            //HACK: until starbases become real objects.. getting tired of this.
            if (attackerName == "Enterprise")
            {
                attackerName = "Hostile Starbase";
            }

            return String.Format("Your Ship has been hit by " + attackerName + " at sector [{0},{1}].", attackerSector.X, attackerSector.Y);
        }

        public void OutputConditionAndWarnings(Ship ship, int shieldsDownLevel)
        {
            var condition = ship.GetConditionAndSetIcon();

            if (ship.AtLowEnergyLevel())
            {
                this.ResourceLine("LowEnergyLevel");
            }

            if (ship.GetRegion().Type == RegionType.Nebulae)
            {
                this.SingleLine("");
                this.ResourceLine("NebulaWarning");
            }

            if (Shields.For(ship).Energy == shieldsDownLevel && !Navigation.For(ship).Docked)
            {
                this.ResourceLine("ShieldsDown");
            }
        }

        public void RenderSectors(SectorScanType scanType, ISubsystem subsystem)
        {
            var location = subsystem.ShipConnectedTo.GetLocation();
            Region Region = Regions.Get(subsystem.Game.Map, location.Region);
            var shieldsAutoRaised = Shields.For(subsystem.ShipConnectedTo).AutoRaiseShieldsIfNeeded(Region);
            var printSector = (new Render(this, subsystem.Game.Config));

            int totalHostiles = subsystem.Game.Map.Regions.GetHostileCount();
            var isNebula = (Region.Type == RegionType.Nebulae);
            string RegionDisplayName = Region.Name;
            var sectorScanStringBuilder = new StringBuilder();

            if (isNebula)
            {
                RegionDisplayName += " Nebula"; //todo: resource out.
            }

            this.Line("");

            switch (scanType)
            {
                case SectorScanType.CombinedRange:
                    printSector.CreateCRSViewScreen(Region, subsystem.Game.Map, location, totalHostiles, RegionDisplayName, isNebula, sectorScanStringBuilder);
                    break;

                case SectorScanType.ShortRange:
                    printSector.CreateSRSViewScreen(Region, subsystem.Game.Map, location, totalHostiles, RegionDisplayName, isNebula, sectorScanStringBuilder);         
                    break;

                default:
                    throw new NotImplementedException();
            }

            printSector.OutputScanWarnings(Region, subsystem.Game.Map, shieldsAutoRaised);

            Region.ClearSectorsWithItem(SectorItem.Debug); //Clears any debug Markers that might have been set
            Region.Scanned = true;
        }

        public List<string> RenderLRSData(IEnumerable<LRSResult> lrsData, Game game)
        {
            var renderedResults = new List<string>();
            int scanColumn = 0;

            renderedResults.Add("┌─────┬─────┬─────┐");

            string currentLRSScanLine = "│";

            foreach (LRSResult dataPoint in lrsData)
            {
                string currentRegionResult = null;

                if (dataPoint.Unknown)
                {
                    currentRegionResult = Utility.Utility.DamagedScannerUnit();
                }
                else if(dataPoint.GalacticBarrier)
                {
                    currentRegionResult = game.Config.GetSetting<string>("GalacticBarrier");
                }
                else
                {
                    currentRegionResult += dataPoint;
                }

                currentLRSScanLine += " " +  currentRegionResult + " " + "│";

                if (scanColumn == 2 || scanColumn == 5)
                {
                    renderedResults.Add(currentLRSScanLine);
                    renderedResults.Add("├─────┼─────┼─────┤");
                    currentLRSScanLine = "│";
                }

                if (scanColumn == 8)
                {
                    renderedResults.Add(currentLRSScanLine);
                }

                scanColumn++;
            }

            renderedResults.Add("└─────┴─────┴─────┘");

            return renderedResults;
        }

        public IEnumerable<string> RenderIRSData(IEnumerable<IRSResult> irsData, Game game)
        {
            var renderedResults = new List<string>();
            int scanColumn = 0;

            renderedResults.Add("╒═════╤═════╤═════╕");

            string currentLRSScanLine = "│";

            foreach (IRSResult dataPoint in irsData)
            {
                string currentRegionResult = null;

                if (dataPoint.Unknown)
                {
                    currentRegionResult = Utility.Utility.DamagedScannerUnit();
                }
                else if (dataPoint.GalacticBarrier)
                {
                    currentRegionResult = game.Config.GetSetting<string>("GalacticBarrier");
                }
                else
                {
                    currentRegionResult += dataPoint;
                }

                currentLRSScanLine += " " + currentRegionResult + " " + "│";

                if (scanColumn == 2 || scanColumn == 5)
                {
                    renderedResults.Add(currentLRSScanLine);
                    renderedResults.Add("╞═════╪═════╪═════╡");
                    currentLRSScanLine = "│";
                }

                if (scanColumn == 8)
                {
                    renderedResults.Add(currentLRSScanLine);
                }

                scanColumn++;
            }

            renderedResults.Add("╘═════╧═════╧═════╛");

            return renderedResults;
        }

        public IEnumerable<string> RenderLRSWithNames(List<LRSResult> lrsData, Game game)
        {
            var renderedResults = new List<string>();
            int scanColumn = 0;
            string longestName = "";
            foreach (LRSResult result in lrsData)
            {
                if (result != null)
                {
                    if (result.Name != null)
                    {
                        longestName = longestName.Length > result.Name.Length ? longestName : result.Name;
                    }
                }
            }

            var barrierID = "Galactic Barrier"; //todo resource this
            var cellPadding = 1; //todo resource this

            int cellLength = longestName.Length > barrierID.Length ? longestName.Length : barrierID.Length;
            cellLength += cellPadding;

            var cellLine = new string('─', cellLength + cellPadding);

            renderedResults.Add("");
            renderedResults.Add("*** Long Range Scan ***".PadCenter(((cellLength + cellPadding) * 3) + 5)); //*3 because of borders, +5 to line it up better.  todo: resource this out.
            renderedResults.Add("┌" + cellLine + "┬" + cellLine + "┬" + cellLine + "┐");

            string currentLRSScanLine0 = "│";
            string currentLRSScanLine1 = "│";
            string currentLRSScanLine2 = "│";

            foreach (LRSResult dataPoint in lrsData)
            {
                string currentRegionName = "";
                string currentRegionResult = null;
                string regionCoordinate = "";

                if (dataPoint.Coordinate != null)
                {
                    regionCoordinate = "[" + dataPoint.Coordinate.X + "," + dataPoint.Coordinate.Y + "]";
                    currentRegionName += dataPoint.Name;
                }

                if (dataPoint.Unknown)
                {
                    currentRegionResult = Utility.Utility.DamagedScannerUnit();
                }
                else if (dataPoint.GalacticBarrier)
                {
                    currentRegionName += barrierID; 
                    currentRegionResult = game.Config.GetSetting<string>("GalacticBarrier");
                }
                else
                {
                    currentRegionResult += dataPoint.ToLongString();
                }

                //breaks because coordinate is not populated when nebula

                currentLRSScanLine0 += " " + regionCoordinate.PadCenter(cellLength) + "│";
                currentLRSScanLine1 += " " + currentRegionName.PadCenter(cellLength) + "│";
                currentLRSScanLine2 += currentRegionResult.PadCenter(cellLength + 1) + "│";

                if (scanColumn == 2 || scanColumn == 5)
                {
                    renderedResults.Add(currentLRSScanLine0);
                    renderedResults.Add(currentLRSScanLine1);
                    renderedResults.Add(currentLRSScanLine2);

                    renderedResults.Add("├" + cellLine + "┼" + cellLine + "┼" + cellLine + "┤");

                    currentLRSScanLine0 = "│";
                    currentLRSScanLine1 = "│";
                    currentLRSScanLine2 = "│";
                }

                if (scanColumn == 8)
                {
                    renderedResults.Add(currentLRSScanLine0);
                    renderedResults.Add(currentLRSScanLine1);
                    renderedResults.Add(currentLRSScanLine2);
                }

                scanColumn++;
            }

            renderedResults.Add("└" + cellLine + "┴" + cellLine + "┴" + cellLine + "┘");

            return renderedResults;
        }

        //todo: refactor with RenderLRSWithNames
        public IEnumerable<string> RenderIRSWithNames(List<IRSResult> irsData, Game game)
        {
            var renderedResults = new List<string>();
            int scanColumn = 0;
            string longestName = "";
            foreach (IRSResult result in irsData)
            {
                if (result != null)
                {
                    if (result.Name != null)
                    {
                        longestName = longestName.Length > result.Name.Length ? longestName : result.Name;
                    }
                }
            }

            var barrierID = "Galactic Barrier"; //todo resource this
            var cellPadding = 1; //todo resource this

            int cellLength = longestName.Length > barrierID.Length ? longestName.Length : barrierID.Length;
            cellLength += cellPadding;

            var cellLine = new string('═', cellLength + cellPadding);

            renderedResults.Add("");
            renderedResults.Add("*** Immediate Range Scan *** <skewed>".PadCenter(((cellLength + cellPadding) * 3) + 5)); //*3 because of borders, +5 to line it up better.  todo: resource this out.
            renderedResults.Add("╒" + cellLine + "╤" + cellLine + "╤" + cellLine + "╕");

            string currentLRSScanLine0 = "│";
            string currentLRSScanLine1 = "│";
            string currentLRSScanLine2 = "│";

            foreach (IRSResult irsDataPoint in irsData)
            {
                string currentRegionName = "";
                string currentRegionResult = null;
                string regionCoordinate = "";

                if (irsDataPoint.Coordinate != null)
                {
                    regionCoordinate = Constants.SECTOR_INDICATOR + irsDataPoint.Coordinate.X + "." + irsDataPoint.Coordinate.Y + "";
                    currentRegionName += irsDataPoint.RegionName;
                }

                if (irsDataPoint.Unknown)
                {
                    currentRegionResult = Utility.Utility.DamagedScannerUnit();
                }
                else if (irsDataPoint.GalacticBarrier)
                {
                    currentRegionName += barrierID;
                    currentRegionResult = game.Config.GetSetting<string>("GalacticBarrier");
                }
                else
                {
                    currentRegionResult += irsDataPoint;
                }

                //breaks because coordinate is not populated when nebula

                currentLRSScanLine0 += " " + regionCoordinate.PadCenter(cellLength) + "│";
                currentLRSScanLine1 += " " + currentRegionName.PadCenter(cellLength) + "│";
                currentLRSScanLine2 += currentRegionResult.PadCenter(cellLength + 1) + "│";

                if (scanColumn == 2 || scanColumn == 5)
                {
                    renderedResults.Add(currentLRSScanLine0);
                    renderedResults.Add(currentLRSScanLine1);
                    renderedResults.Add(currentLRSScanLine2);

                    renderedResults.Add("╞" + cellLine + "╪" + cellLine + "╪" + cellLine + "╡");

                    currentLRSScanLine0 = "│";
                    currentLRSScanLine1 = "│";
                    currentLRSScanLine2 = "│";
                }

                if (scanColumn == 8)
                {
                    renderedResults.Add(currentLRSScanLine0);
                    renderedResults.Add(currentLRSScanLine1);
                    renderedResults.Add(currentLRSScanLine2);
                }

                scanColumn++;
            }

            renderedResults.Add("╘" + cellLine + "╧" + cellLine + "╧" + cellLine + "╛");

            return renderedResults;
        }

        public string Course()
        {
            return Environment.NewLine +
                   " 4   5   6 " + Environment.NewLine +
                   @"   \ ↑ /  " + Environment.NewLine +
                   "3 ← <*> → 7" + Environment.NewLine +
                   @"   / ↓ \  " + Environment.NewLine +
                   " 2   1   8" + Environment.NewLine +
                   Environment.NewLine;
        }

    }
}


