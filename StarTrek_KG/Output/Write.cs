using System;
using System.Collections.Generic;
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
using StarTrek_KG.Utility;
using Console = StarTrek_KG.Utility.Console;

namespace StarTrek_KG.Output
{
    /// <summary>
    /// todo: the goal here is to be able to save all output to a file for later printing..
    /// The purpose of this class is to encapsulate all text that is to be read or written in the application.
    /// </summary>
    public class Write: IConfig, IOutputWrite
    {
        public IStarTrekKGSettings Config { get; set; }
        public int TotalHostiles { get; set; }
        public int TimeRemaining { get; set; }
        public int Starbases { get; set; }
        public int Stardate { get; set; }
        public List<string> OutputQueue { get; set; }

        public bool IsSubscriberApp { get; set; }
        public bool IsTelnetApp { get; set; }
        public IOutput Output { get; set; }

        //todo: make this non-static so we can test this class..

        private Console _console;
        public Console Console
        {
            get { return _console ?? (_console = new Console()); }
            set { _console = value; }
        }

        public List<string> ACTIVITY_PANEL  { get; set; }

        public readonly string ENTER_DEBUG_COMMAND = "Enter Debug command: ";
        public readonly string ENTER_COMPUTER_COMMAND = "Enter computer command: ";

        //TODO:  Have Game expose and raise an output event
        //Have UI subscribe to it.

        //Output object
        //goal is to output message that a UI can read
        //all *print* mnemonics will be changed to Output
        //UI needs to read this text and display it how it wants

        public Write(int totalHostiles, int starbases, int stardate, int timeRemaining, IStarTrekKGSettings config) : this(config)
        {
            this.ACTIVITY_PANEL = new List<string>();
            this.TotalHostiles = totalHostiles;
            this.Starbases = starbases;
            this.Stardate = stardate;
            this.TimeRemaining = timeRemaining;
        }

        public Write(IStarTrekKGSettings config)
        {
            this.ACTIVITY_PANEL = new List<string>();
            this.Config = config;

            this.IsTelnetApp = this.Config.GetSetting<bool>("IsTelnetApp");
            this.IsSubscriberApp = this.Config.GetSetting<bool>("IsSubscriberApp");

            if (this.IsTelnetApp)
            {
                this.Output = new TelnetWrite(config); //todo: config might be droppped
            }
            else if (this.IsSubscriberApp)
            {
                this.Output = new SubscriberWrite(config);
            }
            else
            {
                this.Output = this.Console;
            }
        }

        //missionResult needs to be an enum
        public void PrintCommandResult(Ship ship, bool starbasesAreHostile, int starbasesLeft)
        {
            var commandResult = string.Empty;

            if (ship.Destroyed)
            {
                commandResult = "MISSION FAILED: " + ship.Name.ToUpper() + " DESTROYED";
            }
            else if (ship.Energy == 0)
            {
                commandResult = "MISSION FAILED: " + ship.Name.ToUpper() + " RAN OUT OF ENERGY.";
            }
            else if (starbasesAreHostile && starbasesLeft == 0)
            {
                commandResult = "ALL FEDERATION STARBASES DESTROYED. YOU HAVE DEALT A SEVERE BLOW TO THE FEDERATION!";
            }
            else if (this.TotalHostiles == 0)
            {
                commandResult = "MISSION ACCOMPLISHED: ALL HOSTILE SHIPS DESTROYED. WELL DONE!!!";
            }
            else if (this.TimeRemaining == 0)
            {
                commandResult = "MISSION FAILED: " + ship.Name.ToUpper() + " RAN OUT OF TIME.";
            }

            //else - No status to report.  Game continues

            this.Line(commandResult);
        }

        //output as KeyValueCollection, and UI will build the string
        public void PrintMission()
        {
            this.Output.WriteLine(this.Config.GetText("MissionStatement"), this.TotalHostiles, this.TimeRemaining, this.Starbases);
            this.Output.WriteLine(this.Config.GetText("HelpStatement"));
            this.Output.WriteLine();
        }

        public void Strings(IEnumerable<string> strings)
        {
            foreach (var str in strings)
            {
                this.Output.WriteLine(str);
            }
            this.Output.WriteLine();
        }

        public void HighlightTextBW(bool on)
        {
            this.Output.HighlightTextBW(on);
        }

        public void Line(string stringToOutput)
        {
            this.Output.WriteLine(stringToOutput);
            this.Output.WriteLine();
        }

        public string GetFormattedConfigText(string configTextToWrite, object param1)
        {
            return string.Format(this.Config.GetText(configTextToWrite), param1);
        }

        public string GetFormattedConfigText(string configTextToWrite, object param1, object param2)
        {
            return string.Format(this.Config.GetText(configTextToWrite), param1, param2);
        }

        public void FormattedConfigLine(string configTextToWrite, object param1)
        {
            this.Output.Write(string.Format(this.Config.GetText(configTextToWrite), param1));
            this.Output.WriteLine();
        }

        public void FormattedConfigLine(string configTextToWrite, object param1, object param2)
        {
            this.Output.Write(string.Format(this.Config.GetText(configTextToWrite), param1, param2));
            this.Output.WriteLine();
        }

        /// <summary>
        /// Synctactic Sugar for pulling text
        /// </summary>
        /// <param name="configTextName"></param>
        /// <returns></returns>
        public void ConfigText(string configTextName)
        {
            this.Output.WriteLine(this.Config.GetText(configTextName));
        }

        public void DebugLine(string stringToOutput)
        {
            if (Constants.DEBUG_MODE)
            {
                this.Output.WriteLine(stringToOutput);
            }
        }

        public void Resource(string text)
        {
            this.Output.WriteLine(this.Config.GetText(text) + " ");
        }

        public void ResourceLine(string text)
        {
            this.Output.WriteLine(this.Config.GetText(text));
            this.Output.WriteLine();
        }

        public void ResourceSingleLine(string text)
        {
            this.Output.WriteLine(this.Config.GetText(text));
        }

        public void ResourceLine(string prependText, string text)
        {
            this.Output.WriteLine(prependText + " " + this.Config.GetText(text));
            this.Output.WriteLine();
        }

        public void SingleLine(string stringToOutput)
        {
            this.Output.WriteLine(stringToOutput);
        }

        public void WithNoEndCR(string stringToOutput)
        {
            this.Output.Write(stringToOutput);
        }

        public void DisplayPropertiesOf(object @object)
        {
            if (@object != null)
            {
                var objectPropInfos = @object.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo prop in objectPropInfos)
                {
                    this.Output.WriteLine("{0} : {1}", prop.Name, prop.GetValue(@object, null));
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

        private string Display(Menu menuItem)
        {
            return $"{menuItem.Name} = {menuItem.Description}";
        }

        public void CreateCommandPanel()
        {
            ACTIVITY_PANEL = new List<string>
            {
                "imp = Impulse Navigation",
                "wrp = Warp Navigation",
                "nto = Navigate To Object",
                "─────────────────────────────",
                "irs = Immediate Range Scan",
                "srs = Short Range Scan",
                "lrs = Long Range Scan",
                "crs = Combined Range Scan",
                "─────────────────────────────",
                "pha = Phaser Control",
                "tor = Photon Torpedo Control",
                "toq = Target Object in this Region",
                "─────────────────────────────",
                "she = Shield Control",
                "com = Access Computer",
                "dmg = Damage Control"
            };


            if(Constants.DEBUG_MODE)
            {
                ACTIVITY_PANEL.Add("");
                ACTIVITY_PANEL.Add("─────────────────────────────");
                ACTIVITY_PANEL.Add(this.Display(Menu.dbg));
            }
        }

        //This needs to be a command method that the UI passes values into.
        //Readline is done in UI

        /// <summary>
        /// What happens in here is that each method called generates an output then renders it to the screen
        /// </summary>
        /// <param name="playerShip"></param>
        /// <param name="mapText"></param>
        /// <param name="game"></param>
        /// <param name="command"></param>
        public void ReadAndOutput(Ship playerShip, string mapText, Game game, string command = null)
        {
            this.Output.Write(mapText);

            string readCommand;

            if (command != null && this.IsSubscriberApp)
            {
                readCommand = command.Trim().ToLower();
            }
            else
            {
                string readLine = this.Output.ReadLine();
                if (readLine == null) return;

                readCommand = readLine.Trim().ToLower();
            }

            if (readCommand == Menu.wrp.ToString() || readCommand == Menu.imp.ToString() || readCommand == Menu.nto.ToString())
            {
                Navigation.For(playerShip).Controls(command);
            }
            else if (readCommand == Menu.irs.ToString())
            {
                ImmediateRangeScan.For(playerShip).Controls();
            }
            else if (readCommand == Menu.srs.ToString())
            {
                ShortRangeScan.For(playerShip).Controls();
            }
            else if (readCommand == Menu.lrs.ToString())
            {
                LongRangeScan.For(playerShip).Controls();
            }
            else if (readCommand == Menu.crs.ToString())
            {
                CombinedRangeScan.For(playerShip).Controls();
            }
            else if (readCommand == Menu.pha.ToString())
            {
                Phasers.For(playerShip).Controls(playerShip);
            }
            else if (readCommand == Menu.tor.ToString())
            {
                Torpedoes.For(playerShip).Controls();
            }
            else if (readCommand == Menu.she.ToString())
            {
                this.ShieldMenu(playerShip);
            }
            else if (readCommand == Menu.com.ToString())
            {
                this.ComputerMenu(playerShip);
            }
            else if (readCommand == Menu.toq.ToString())
            {
                Computer.For(playerShip).Controls(command);
            }
            else if (readCommand == Menu.dmg.ToString())
            {
                this.DamageControlMenu(playerShip);
            }
            else if (readCommand == Menu.dbg.ToString())
            {
                this.DebugMenu(playerShip);
            }
            else if (readCommand == Menu.ver.ToString())
            {
                this.Output.WriteLine(this.Config.GetText("AppVersion").TrimStart(' '));
            }
            else if (readCommand == Menu.cls.ToString())
            {
                this.Output.Clear();
            }
            else
            {
                this.CreateCommandPanel();
                this.Panel(this.GetPanelHead(playerShip.Name), ACTIVITY_PANEL);
            }
        }

        public void Panel(string panelHead, IEnumerable<string> strings)
        {
            this.Output.WriteLine();
            this.Output.WriteLine(panelHead);
            this.Output.WriteLine();

            foreach (var str in strings)
            {
                this.Output.WriteLine(str);
            }

            this.Output.WriteLine();
        }

        private void DebugMenu(IShip playerShip)
        {
            this.Strings(Debug.CONTROL_PANEL);
            this.WithNoEndCR(this.ENTER_DEBUG_COMMAND);

            //todo: readline needs to be done using an event
            var debugCommand = Output.ReadLine().Trim().ToLower();

            Debug.For(playerShip).Controls(debugCommand);
        }

        private void ComputerMenu(IShip playerShip)
        {
            if (Computer.For(playerShip).Damaged()) return;

            this.Strings(Computer.CONTROL_PANEL);
            this.WithNoEndCR(this.ENTER_COMPUTER_COMMAND);

            //todo: readline needs to be done using an event
            var computerCommand = Output.ReadLine().Trim().ToLower();

            Computer.For(playerShip).Controls(computerCommand);
        }

        private void DamageControlMenu(IShip playerShip)
        {
            this.Strings(DamageControl.CONTROL_PANEL);
            this.WithNoEndCR("Enter Damage Control Command: ");

            //todo: readline needs to be done using an event
            var damageControlCommand = Output.ReadLine().Trim().ToLower();

            DamageControl.For(playerShip).Controls(damageControlCommand);
        }

        private void ShieldMenu(IShip playerShip)
        {
            if (Shields.For(playerShip).Damaged()) return;

            Shields.SHIELD_PANEL = new List<string>
            {
                Environment.NewLine
            };

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
            var shieldsCommand = Output.ReadLine().Trim().ToLower();

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

                value = int.Parse(this.Output.ReadLine());

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
                this.Output.Write(promptMessage);

                var readLine = this.Output.ReadLine();
                if (readLine != null) value = readLine.ToLower();

                return true;
            }
            catch 
            {
                value = "";
            }

            return false;
        }

        public static string ShipHitMessage(IShip attacker, int attackingEnergy)
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

            return $"Your ship has been hit by {attackerName} at sector [{attackerSector.X},{attackerSector.Y}], {attackingEnergy} megawatts of energy.";
        }

        public static string MisfireMessage(IShip attacker)
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

            return string.Format("Misfire by " + attackerName + " at sector [{0},{1}].", attackerSector.X, attackerSector.Y);
        }

        public void OutputConditionAndWarnings(Ship ship, int shieldsDownLevel)
        {
            ship.GetConditionAndSetIcon();

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

        //public IEnumerable<string> RenderIRSData(IEnumerable<IRSResult> irsData, Game game)
        //{
        //    var renderedResults = new List<string>();
        //    int scanColumn = 0;

        //    renderedResults.Add("╒═════╤═════╤═════╕");

        //    string currentLRSScanLine = "│";

        //    foreach (IRSResult dataPoint in irsData)
        //    {
        //        string currentRegionResult = null;

        //        if (dataPoint.Unknown)
        //        {
        //            currentRegionResult = Utility.Utility.DamagedScannerUnit();
        //        }
        //        else if (dataPoint.GalacticBarrier)
        //        {
        //            currentRegionResult = game.Config.GetSetting<string>("GalacticBarrier");
        //        }
        //        else
        //        {
        //            currentRegionResult += dataPoint;
        //        }

        //        currentLRSScanLine += " " + currentRegionResult + " " + "│";

        //        if (scanColumn == 2 || scanColumn == 5)
        //        {
        //            renderedResults.Add(currentLRSScanLine);
        //            renderedResults.Add("╞═════╪═════╪═════╡");
        //            currentLRSScanLine = "│";
        //        }

        //        if (scanColumn == 8)
        //        {
        //            renderedResults.Add(currentLRSScanLine);
        //        }

        //        scanColumn++;
        //    }

        //    renderedResults.Add("╘═════╧═════╧═════╛");

        //    return renderedResults;
        //}

        //todo: refactor with RenderLRSWithNames
        public IEnumerable<string> RenderScanWithNames(ScanRenderType scanRenderType, string title, List<IScanResult> data, Game game)
        {
            var renderedResults = new List<string>();
            int scanColumn = 0;  //todo resource this
            string longestName = Write.GetLongestName(data);

            var galacticBarrierText = this.Config.GetSetting<string>("GalacticBarrierText");
            var barrierID = galacticBarrierText; 
            var cellPadding = 1; //todo resource this

            int cellLength = longestName.Length > barrierID.Length ? longestName.Length : barrierID.Length;
            cellLength += cellPadding;

            var topLeft = this.Config.Setting(scanRenderType + "TopLeft");
            var topMiddle = this.Config.Setting(scanRenderType + "TopMiddle");
            var topRight = this.Config.Setting(scanRenderType + "TopRight");

            var bottomLeft = this.Config.Setting(scanRenderType + "BottomLeft");
            var bottomMiddle = this.Config.Setting(scanRenderType + "BottomMiddle");
            var bottomRight = this.Config.Setting(scanRenderType + "BottomRight");

            var cellLine = new string(Convert.ToChar(this.Config.Setting(scanRenderType + "CellLine")), cellLength + cellPadding);

            renderedResults.Add("");
            renderedResults.Add(title.PadCenter(((cellLength + cellPadding) * 3) + 5)); //*3 because of borders, +5 to line it up better.   //todo resource this
            renderedResults.Add(topLeft + cellLine + topMiddle + cellLine + topMiddle + cellLine + topRight);

            this.RenderMiddle(scanRenderType, data, barrierID, galacticBarrierText, cellLength, scanColumn, renderedResults, cellLine);

            renderedResults.Add(bottomLeft + cellLine + bottomMiddle + cellLine + bottomMiddle + cellLine + bottomRight);

            return renderedResults;
        }

        private void RenderMiddle(ScanRenderType scanRenderType, 
                                  IEnumerable<IScanResult> data, 
                                  string barrierID, 
                                  string galacticBarrierText,
                                  int cellLength, 
                                  int scanColumn, 
                                  ICollection<string> renderedResults, 
                                  string cellLine)
        {
            var verticalBoxLine = this.Config.Setting("VerticalBoxLine");

            var middleLeft = this.Config.Setting(scanRenderType + "MiddleLeft");
            var middle = this.Config.Setting(scanRenderType + "Middle");
            var middleRight = this.Config.Setting(scanRenderType + "MiddleRight");

            string currentLRSScanLine0 = verticalBoxLine;
            string currentLRSScanLine1 = verticalBoxLine;
            string currentLRSScanLine2 = verticalBoxLine;

            foreach (IScanResult scanDataPoint in data)
            {
                string currentRegionName = "";
                string currentRegionResult = null;
                string regionCoordinate = "";

                if (scanDataPoint.Coordinate != null)
                {
                    regionCoordinate = Constants.SECTOR_INDICATOR + scanDataPoint.Coordinate.X + "." +
                                       scanDataPoint.Coordinate.Y + "";

                    currentRegionName += scanDataPoint.RegionName;
                }

                if (scanDataPoint.Unknown)
                {
                    currentRegionResult = Utility.Utility.DamagedScannerUnit();
                }
                else if (scanDataPoint.GalacticBarrier)
                {
                    currentRegionName += barrierID;
                    currentRegionResult = galacticBarrierText;
                }
                else
                {
                    currentRegionResult += scanDataPoint.ToScanString();
                }

                //breaks because coordinate is not populated when nebula

                currentLRSScanLine0 += " " + regionCoordinate.PadCenter(cellLength) + verticalBoxLine;
                currentLRSScanLine1 += " " + currentRegionName.PadCenter(cellLength) + verticalBoxLine;
                currentLRSScanLine2 += currentRegionResult.PadCenter(cellLength + 1) + verticalBoxLine; //todo resource this

                if (scanColumn == 2 || scanColumn == 5) //todo resource this
                {
                    renderedResults.Add(currentLRSScanLine0);
                    renderedResults.Add(currentLRSScanLine1);
                    renderedResults.Add(currentLRSScanLine2);

                    renderedResults.Add(middleLeft + cellLine + middle + cellLine + middle + cellLine + middleRight);

                    currentLRSScanLine0 = verticalBoxLine;
                    currentLRSScanLine1 = verticalBoxLine;
                    currentLRSScanLine2 = verticalBoxLine;
                }

                if (scanColumn == 8) //todo resource this
                {
                    renderedResults.Add(currentLRSScanLine0);
                    renderedResults.Add(currentLRSScanLine1);
                    renderedResults.Add(currentLRSScanLine2);
                }

                scanColumn++;
            }
        }

        private static string GetLongestName(IEnumerable<IScanResult> data)
        {
            string longestName = "";

            foreach (IScanResult result in data)
            {
                if (result?.RegionName != null)
                {
                    longestName = longestName.Length > result.RegionName.Length ? longestName : result.RegionName;
                }
            }

            return longestName;
        }

        public string Course()
        {
            //todo: pull this from app.config

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


