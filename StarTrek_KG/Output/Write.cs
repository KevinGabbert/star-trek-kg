using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using StarTrek_KG.Actors;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
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
    /// The purpose of this class is to encapsulate all text that is to be read or written in the application.
    /// </summary>
    public class Write: IConfig, IWriter
    {
        #region Properties and Constants
        public IStarTrekKGSettings Config { get; set; }
        public IOutputMethod Output { get; set; }

        #region Subscriber

        //todo: this needs to be broken out into a Subscriber object (and make it into a prop)

        public bool IsSubscriberApp { get; set; }
        public SubsystemType SubscriberPromptSubSystem { get; set; }
        public int SubscriberPromptLevel { get; set; }

        #endregion

        private Console _console;
        private Console Console
        {
            get { return _console ?? (_console = new Console()); }
            set { _console = value; }
        }

        private int TotalHostiles { get; set; }
        private int TimeRemaining { get; set; }
        private int Starbases { get; set; }
        private int Stardate { get; set; }

        private bool IsTelnetApp { get; set; }

        public List<string> ACTIVITY_PANEL { get; set; }

        //todo: resource these out.
        private readonly string ENTER_DEBUG_COMMAND = "Enter Debug command: ";
        private readonly string ENTER_COMPUTER_COMMAND = "Enter computer command: ";

        #endregion

        public Write(IStarTrekKGSettings config)
        {
            this.ACTIVITY_PANEL = new List<string>();
            this.Config = config;

            this.IsTelnetApp = this.Config.GetSetting<bool>("IsTelnetApp");
            this.IsSubscriberApp = this.Config.GetSetting<bool>("IsSubscriberApp");

            if (this.IsTelnetApp)
            {
                this.Output = new TelnetOutput(config); //todo: config might be droppped
            }
            else if (this.IsSubscriberApp)
            {
                this.Output = new SubscriberOutput(config);
            }
            else
            {
                this.Output = this.Console;
            }
        }

        public Write(int totalHostiles, int starbases, int stardate, int timeRemaining, IStarTrekKGSettings config) : this(config)
        {
            this.ACTIVITY_PANEL = new List<string>();
            this.TotalHostiles = totalHostiles;
            this.Starbases = starbases;
            this.Stardate = stardate;
            this.TimeRemaining = timeRemaining;
        }

        #region Config

        /// <summary>
        /// Synctactic Sugar for pulling text
        /// </summary>
        /// <param name="configTextName"></param>
        /// <returns></returns>
        public void ConfigText(string configTextName)
        {
            this.Output.WriteLine(this.Config.GetText(configTextName));
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

        #endregion

        #region Reflection

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

        #endregion

        #region Rendering

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

        //todo: resource out
        public string RenderCourse()
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

        #endregion

        #region MenusAndPrompts

        private string DisplayMenuItem(Menu menuItem)
        {
            return $"{menuItem.Name} = {menuItem.Description}";
        }

        //todo: resource out
        public List<string> Panel(string panelHead, IEnumerable<string> strings)
        {
            this.Output.WriteLine();
            this.Output.WriteLine(panelHead);
            this.Output.WriteLine();

            foreach (var str in strings)
            {
                this.Output.WriteLine(str);
            }

            this.Output.WriteLine();

            return this.Output.Queue.ToList();
        }

        //todo: resource out
        private string GetPanelHead(string shipName)
        {
            return "─── " + shipName + " ───";
        }

        /// <summary>
        /// What happens in here is that each method called generates an output then renders it to the screen
        /// </summary>
        /// <param name="playerShip"></param>
        /// <param name="mapText"></param>
        /// <param name="game"></param>
        /// <param name="userInput"></param>
        public List<string> ReadAndOutput(Ship playerShip, string mapText, Game game, string userInput = null)
        {
            if (!this.IsSubscriberApp)
            {
                this.Output.Write(mapText);
            }

            string userCommand;

            if (userInput != null && this.IsSubscriberApp)
            {
                userCommand = userInput.Trim().ToLower();
                this.Output.Clear();
            }
            else
            {
                string readLine = this.Output.ReadLine();
                if (readLine == null) return null;

                userCommand = readLine.Trim().ToLower();
            }

            return this.OutputMenu(playerShip, userCommand);
        }
    
        private List<string> OutputMenu(IShip playerShip, string userCommand)
        {
            List<string> retVal;

            if (this.SubscriberPromptLevel == 0)
            {
                retVal = this.EvalTopLevelMenuCommand(playerShip, userCommand);
            }
            else
            {
                retVal = this.EvalSubLevelCommand(playerShip, userCommand, this.SubscriberPromptLevel);
            }

            return retVal;
        }

        private List<string> EvalTopLevelMenuCommand(IShip playerShip, string menuCommand)
        {
            IEnumerable<string> retVal = new List<string>();

            if (menuCommand == Menu.wrp.ToString() || menuCommand == Menu.imp.ToString() || menuCommand == Menu.nto.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.Navigation;

                //todo: we may need to break out warp and imp, or change the process here because we can't tell which of the 3 we need for prompt.

                retVal = Navigation.For(playerShip).Controls(menuCommand);
            }
            else if (menuCommand == Menu.irs.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.ImmediateRangeScan;
                retVal = ImmediateRangeScan.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.srs.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.ShortRangeScan;
                retVal = ShortRangeScan.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.lrs.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.LongRangeScan;
                retVal = LongRangeScan.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.crs.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.CombinedRangeScan;
                retVal = CombinedRangeScan.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.pha.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.Phasers;
                retVal = Phasers.For(playerShip).Controls(playerShip);
            }
            else if (menuCommand == Menu.tor.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.Torpedoes;
                retVal = Torpedoes.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.she.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.Shields;

                retVal = this.ShieldMenu(playerShip);
            }
            else if (menuCommand == Menu.com.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.Computer;
                retVal = this.ComputerMenu(playerShip);
            }
            else if (menuCommand == Menu.toq.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.Computer;
                retVal = Computer.For(playerShip).Controls(menuCommand); //todo: promptlevel should be able to tell us anything else we need.
            }
            else if (menuCommand == Menu.dmg.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.DamageControl;
                retVal = this.DamageControlMenu(playerShip);
            }
            else if (menuCommand == Menu.dbg.ToString())
            {
                this.SubscriberPromptSubSystem = SubsystemType.Debug;
                retVal = this.DebugMenu(playerShip);
            }
            else if (menuCommand == Menu.ver.ToString())
            {
                this.Output.WriteLine(this.Config.GetText("AppVersion").TrimStart(' '));
            }
            else if (menuCommand == Menu.cls.ToString())
            {
                this.Output.Clear();
            }
            else
            {
                this.CreateCommandPanel();
                retVal = this.Panel(this.GetPanelHead(playerShip.Name), ACTIVITY_PANEL);
            }

            return retVal.ToList();
        }

        /// <summary>
        /// Checks against menu commands for matching subsystem
        /// </summary>
        /// <param name="stringToCheck"></param>
        /// <param name="subsystem"></param>
        /// <param name="promptLevel"></param>
        /// <returns></returns>
        private bool IsAcceptable(string stringToCheck, SubsystemType subsystem, int promptLevel)
        {
            //todo: verify that this config entry exists
            MenuItems menuItems = this.Config.GetMenuItems($"{subsystem}Panel");
            bool acceptable = menuItems.Cast<MenuItemDef>().Any(m => m.name == stringToCheck && m.promptLevel == promptLevel) != null;
            return acceptable;
        }


        public void CreateCommandPanel()
        {
            //todo: resource out menu
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

            if (Constants.DEBUG_MODE)
            {
                ACTIVITY_PANEL.Add("");
                ACTIVITY_PANEL.Add("─────────────────────────────");
                ACTIVITY_PANEL.Add(this.DisplayMenuItem(Menu.dbg));
            }
        }

        #region Sub-Level Menus

        public List<string> EvalSubLevelCommand(IShip playerShip, string playerEnteredText, int promptLevel)
        {
            IEnumerable<string> retVal = new List<string>();

            string menuName = this.SubscriberPromptSubSystem.Name;

            //todo: Finish other menus.  refactor their common menu creation steps
            if (this.IsAcceptable(playerEnteredText, this.SubscriberPromptSubSystem, this.SubscriberPromptLevel))
            {
                ISubsystem subsystem = SubSystem_Base.GetSubsystemFor(playerShip, this.SubscriberPromptSubSystem);
                this.Output.Write(subsystem.Controls(playerEnteredText));
            }
            else
            {
                this.Output.Write($"Unrecognized Command. Exiting {menuName} Menu");
                this.SubscriberPromptLevel = 0; //resets our menu level

                return null;
            }

            return retVal.ToList();
        }

        private IEnumerable<string> DebugMenu(IShip playerShip)
        {
           IEnumerable<MenuItemDef> menuItems = this.Config.GetMenuItems($"{this.SubscriberPromptSubSystem}Panel").Cast<MenuItemDef>();

            this.OutputStrings(Debug.DEBUG_PANEL);
            this.WithNoEndCR(this.ENTER_DEBUG_COMMAND);

            //todo: readline needs to be done using an event
            var debugCommand = Output.ReadLine().Trim().ToLower();

            Debug.For(playerShip).Controls(debugCommand);

            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> ComputerMenu(IShip playerShip)
        {
            if (Computer.For(playerShip).Damaged()) return this.Output.Queue.ToList();

            IEnumerable<MenuItemDef> menuItems = this.Config.GetMenuItems($"{this.SubscriberPromptSubSystem}Panel").Cast<MenuItemDef>();

            this.OutputStrings(Computer.CONTROL_PANEL);
            this.WithNoEndCR(this.ENTER_COMPUTER_COMMAND);

            //todo: readline needs to be done using an event
            var computerCommand = Output.ReadLine().Trim().ToLower();

            Computer.For(playerShip).Controls(computerCommand);

            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> DamageControlMenu(IShip playerShip)
        {
            IEnumerable<MenuItemDef> menuItems = this.Config.GetMenuItems($"{this.SubscriberPromptSubSystem}Panel").Cast<MenuItemDef>();

            this.OutputStrings(DamageControl.DAMAGE_PANEL);

            this.WithNoEndCR("Enter Damage Control Command: ");

            //todo: readline needs to be done using an event
            var damageControlCommand = Output.ReadLine().Trim().ToLower();

            DamageControl.For(playerShip).Controls(damageControlCommand);

            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> ShieldMenu(IShip playerShip, string shieldsCommand = "")
        {
            if (Shields.For(playerShip).Damaged()) return this.Output.Queue.ToList();

            Shields.SHIELD_PANEL = new List<string>
            {
                Environment.NewLine
            };

            var currentShieldEnergy = Shields.For(playerShip).Energy;

            IEnumerable<MenuItemDef> menuItems = this.Config.GetMenuItems($"{this.SubscriberPromptSubSystem}Panel").Cast<MenuItemDef>();

            //todo: replace the below with menuItems grabbed here.

            //todo: resource out header
            //todo: *DOWN* feature should be a upgrade functionality
            if (currentShieldEnergy > 0)
            {
                //todo:add header from config file.
                Shields.SHIELD_PANEL.Add(string.Format("─── Shield Control: ── {0} ──", $"< CURRENTLY AT: {currentShieldEnergy}>"));

                foreach (MenuItemDef menuItem in menuItems)
                {
                    Shields.SHIELD_PANEL.Add($"{menuItem.name} {menuItem.divider} {menuItem.description}");
                }
            }
            else
            {
                //todo: resource out header
                Shields.SHIELD_PANEL.Add(string.Format("─── Shield Control: ── {0} ──", "DOWN"));

                var itemToAdd = menuItems.First(m => m.name == "add");
                Shields.SHIELD_PANEL.Add($"{itemToAdd.name} {itemToAdd.divider} {itemToAdd.description}");
            }

            this.OutputStrings(Shields.SHIELD_PANEL);

            //todo: resource out header

            Shields.For(playerShip).MaxTransfer = playerShip.Energy; //todo: this does nothing!

            string shieldPromptReply;

            this.PromptUser(SubsystemType.Shields, "Shield Panel Command:> ", out shieldPromptReply, 1);

            Shields.For(playerShip).Controls(shieldsCommand);         
            
            return this.Output.Queue;
        }

        #endregion

        #region Prompt

        /// <summary>
        /// The point of this method is to get information from the user.  
        /// In the case of the console, readline will display cursor, and wait for the user to reply.
        /// In the case of Sebscriber (ex. Web), then we must end the call that got us here so we can get the information back from the user.
        /// </summary>
        /// <param name="promptSubsystem"></param>
        /// <param name="promptMessage"></param>
        /// <param name="value"></param>
        /// <param name="subPromptLevel"></param>
        /// <returns></returns>
        public bool PromptUser(SubsystemType promptSubsystem, string promptMessage, out string value, int subPromptLevel = 0)
        {
            try
            {
                this.WithNoEndCR(promptMessage);

                if (this.IsSubscriberApp)
                {
                    //todo: subsystemtype.None is currently the holding for subsystems like Impulse, and Warp. they need to be broken out to a separate class.
                    this.SubscriberPromptSubSystem = promptSubsystem;
                    this.SubscriberPromptLevel = subPromptLevel;

                    //todo: an endCR might need to be added here

                    value = "-1";
                }
                else
                {
                     value = this.Output.ReadLine().Trim().ToLower();
                }

                return true;
            }
            catch
            {
                value = "0";
            }

            return false;
        }

        //tod: combine this with PromptUser
        public bool PromptUserSubscriber(string promptMessage, out string value)
        {
            value = null;

            this.Output.Write(promptMessage);

            //todo: Game.Mode to submenu?
            //that mode will persist across ajax calls, so user will have to either type in a submenu entry, or 
            //exit it.

            //var readLine = this.Output.ReadLine();
            //if (readLine != null) value = readLine.ToLower();

            return false;
        }

        public bool PromptUserConsole(string promptMessage, out string value)
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

        #endregion

        #endregion  

        #region Misc

        public void HighlightTextBW(bool on)
        {
            this.Output.HighlightTextBW(on);
        }

        public List<string> Line(string stringToOutput)
        {
            var linesToOutput = new List<string>();

            linesToOutput.AddRange(this.Output.WriteLine(stringToOutput));
            linesToOutput.Add(this.Output.WriteLine());

            return linesToOutput;
        }

        public void OutputStrings(IEnumerable<string> strings)
        {
            foreach (var str in strings)
            {
                this.Output.WriteLine(str);
            }
            this.Output.WriteLine();
        }

        public void WithNoEndCR(string stringToOutput)
        {
            this.Output.Write(stringToOutput);
        }

        public void DebugLine(string stringToOutput)
        {
            if (Constants.DEBUG_MODE)
            {
                this.Output.WriteLine(stringToOutput);
            }
        }

        public void SingleLine(string stringToOutput)
        {
            this.Output.WriteLine(stringToOutput);
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

        #endregion
    }
}


