using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Constants;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Extensions.System;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Output
{
    /// <summary>
    /// todo: the goal here is to be able to save all output to a file for later printing..
    /// The purpose of this class is to encapsulate all text that is to be read or written in the application.
    /// </summary>
    public class Interaction: IConfig, IInteraction
    {
        #region Properties and Constants

        public IStarTrekKGSettings Config { get; set; }
        public IOutputMethod Output { get; set; }

        public Subscriber Subscriber { get; set; }

        public string CurrentPrompt { get; set; }
        public bool OutputError { get; set; }

        private int TotalHostiles { get; }
        private int TimeRemaining { get; }
        private int Starbases { get; }

        private int Stardate { get; set; }

        public List<string> SHIP_PANEL { get; set; }

        //todo: resource these out.
        private readonly string ENTER_DEBUG_COMMAND = "Enter Debug command: ";
        private readonly string ENTER_COMPUTER_COMMAND = "Enter computer command: ";

        #endregion

        public Interaction()
        {
            this.Subscriber = new Subscriber(this.Config);
        }

        public Interaction(IStarTrekKGSettings config)
        {
            this.SHIP_PANEL = new List<string>();
            this.Config = config;

            this.Subscriber = new Subscriber(this.Config);
            this.Output = new SubscriberOutput(config);
        }

        public Interaction(int totalHostiles, int starbases, int stardate, int timeRemaining, IStarTrekKGSettings config) : this(config)
        {
            this.Subscriber = new Subscriber(this.Config);

            this.SHIP_PANEL = new List<string>();
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
            this.Output.WriteLine(this.GetConfigText(configTextName));
        }

        public string GetFormattedConfigText(string configTextToWrite, object param1)
        {
            return string.Format(this.GetConfigText(configTextToWrite), param1);
        }

        public string GetFormattedConfigText(string configTextToWrite, object param1, object param2)
        {
            return string.Format(this.GetConfigText(configTextToWrite), param1, param2);
        }

        public void FormattedConfigLine(string configTextToWrite, object param1)
        {
            this.Output.Write(string.Format(this.GetConfigText(configTextToWrite), param1));
            this.Output.WriteLine();
        }

        public void FormattedConfigLine(string configTextToWrite, object param1, object param2)
        {
            this.Output.Write(string.Format(this.GetConfigText(configTextToWrite), param1, param2));
            this.Output.WriteLine();
        }

        public void Resource(string text)
        {
            this.Output.WriteLine(this.GetConfigText(text) + " ");
        }

        public void ResourceLine(string text)
        {
            this.Output.WriteLine(this.GetConfigText(text));
            this.Output.WriteLine();
        }

        public void ResourceSingleLine(string text)
        {
            this.Output.WriteLine(this.GetConfigText(text));
        }

        public void ResourceLine(string prependText, string text)
        {
            this.Output.WriteLine(prependText + " " + this.GetConfigText(text));
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

        public void RenderRegionCounts(bool renderingMyLocation, int? starbaseCount, int? starCount, int? hostileCount)
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

        public string RenderRegionCounts(int? starbaseCount, int? starCount, int? hostileCount)
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

        //public void OutputConditionAndWarnings(IShip ship, int shieldsDownLevel)
        //{
        //    this.Output.Write("OutputConditionAndWarnings not implemented."); //todo: implement this.
        //}

        public void RenderSectors(SectorScanType scanType, ISubsystem subsystem)
        {
            IShip shipConnectedTo = subsystem.ShipConnectedTo;
            IGame game = shipConnectedTo.Map.Game;

            var location = shipConnectedTo.GetLocation();
            Region region = game.Map.Regions[location.Region];
            var shieldsAutoRaised = Shields.For(shipConnectedTo).AutoRaiseShieldsIfNeeded(region);
            var printSector = new Render(this, game.Config);

            int totalHostiles = game.Map.Regions.GetHostileCount();
            var isNebula = region.Type == RegionType.Nebulae;
            string regionDisplayName = region.Name;
            var sectorScanStringBuilder = new StringBuilder();

            if (isNebula)
            {
                regionDisplayName += " Nebula"; //todo: resource out.
            }

            this.Line("");

            switch (scanType)
            {
                case SectorScanType.CombinedRange:
                    printSector.CreateCRSViewScreen(region, game.Map, location, totalHostiles, regionDisplayName, isNebula, sectorScanStringBuilder);
                    break;

                case SectorScanType.ShortRange:
                    printSector.CreateSRSViewScreen(region, game.Map, location, totalHostiles, regionDisplayName, isNebula, sectorScanStringBuilder);
                    break;

                default:
                    throw new NotImplementedException();
            }

            printSector.OutputScanWarnings(region, game.Map, shieldsAutoRaised);

            region.ClearSectorsWithItem(SectorItem.Debug); //Clears any debug Markers that might have been set
            region.Scanned = true;
        }

        public List<string> RenderLRSData(IEnumerable<LRSResult> lrsData, IGame game)
        {
            var renderedResults = new List<string>();
            int scanColumn = 0;

            renderedResults.Add("┌─────┬─────┬─────┐");

            string currentLRSScanLine = "│";

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (LRSResult dataPoint in lrsData)
            {
                currentLRSScanLine = BuildLRSInterior(game, dataPoint, currentLRSScanLine, renderedResults, ref scanColumn);
            }

            renderedResults.Add("└─────┴─────┴─────┘");

            return renderedResults;
        }

        private static string BuildLRSInterior(IGame game, IScanResult dataPoint, string currentLRSScanLine, ICollection<string> renderedResults, ref int scanColumn)
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

            currentLRSScanLine += $" {currentRegionResult} " + "│";

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
            return currentLRSScanLine;
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
        public IEnumerable<string> RenderScanWithNames(ScanRenderType scanRenderType, string title, List<IScanResult> data, IGame game)
        {
            int scanColumn = 0;  //todo resource this
            string longestName = Interaction.GetLongestName(data);

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

            var renderedResults = new List<string>
            {
                "",
                title.PadCenter((cellLength + cellPadding)*3 + 5), //*3 because of borders, +5 to line it up better.   //todo resource this
                topLeft + cellLine + topMiddle + cellLine + topMiddle + cellLine + topRight
            };
  
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
                    regionCoordinate = DEFAULTS.SECTOR_INDICATOR + scanDataPoint.Coordinate.X + "." +
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
        public List<string> ReadAndOutput(IShip playerShip, string mapText, string userInput = null)
        {
            this.Output.Write(mapText);

            return this.ProcessInputString(playerShip, userInput);
        }

        private List<string> ProcessInputString(IShip playerShip, string userInput)
        {
            string userCommand;
            if (this.GetUserCommand(userInput, out userCommand))
            {
                return null;
            }

            if (userCommand.Contains(" "))
            {
                return ProcessMultiStepCommand(playerShip, userCommand);
            }
            else
            {
                return this.OutputMenu(playerShip, userCommand);
            }
        }

        private bool GetUserCommand(string userInput, out string userCommand)
        {
            if (userInput != null)
            {
                userCommand = userInput.Trim().ToLower();
                this.Output.Clear();
            }
            else
            {
                string readLine = this.Output.ReadLine();
                if (readLine == null)
                {
                    userCommand = null;
                    return true;
                }

                userCommand = readLine.Trim().ToLower();
            }
            return false;
        }

        /// <summary>
        /// Basically, since we know what the user wants at this point, for each level, then we simply turn right around and call the next parsed command.
        /// </summary>
        /// <param name="playerShip"></param>
        /// <param name="userCommand"></param>
        /// <returns></returns>
        private List<string> ProcessMultiStepCommand(IShip playerShip, string userCommand)
        {
            this.Subscriber.PromptInfo.RawCommandText = userCommand;

            Queue<string> commands = new Queue<string>(userCommand.Split(' '));

            this.Subscriber.PromptInfo.SetCommands(commands.ToList());

            bool previousCommandWasIssuedAndValid = this.Subscriber.PromptInfo.Level > 0;

            List<string> returnVal = null;

            if (!previousCommandWasIssuedAndValid)
            {
                returnVal = this.EvalTopLevelMenuCommand(playerShip, commands.Dequeue());
            }

            previousCommandWasIssuedAndValid = this.Subscriber.PromptInfo.Level > 0;

            while (commands.Count > 0 && previousCommandWasIssuedAndValid)
            {
                //we only care about the last returnVal
                returnVal = this.OutputMenu(playerShip, commands.Dequeue());
            }

            return returnVal;
        }

        private List<string> OutputMenu(IShip playerShip, string userCommand)
        {
            var promptLevel = this.Subscriber.PromptInfo.Level;
            List<string> retVal = new List<string>();

            switch (userCommand)
            {
                case "level":
                    return this.Output.WriteLine($"At Prompt Level: {promptLevel}"); //todo: resource this

                case "ship":
                case "out":
                    this.ResetPrompt();

                    if (promptLevel > 0)
                    {
                        retVal.AddRange(this.Output.WriteLine("Exiting Panel.")); //todo: resource this
                    }

                    retVal.AddRange(this.Output.WriteLine("Ship Panel now Active.")); //todo: resource this

                    return retVal;
                case "back":

                    if (promptLevel > 0)
                    {
                        this.SetPrompt(promptLevel - 1);
                    }

                    //todo: if level 1 then show this.
                    retVal.AddRange(this.ShieldMenu(playerShip));

                    //todo: if level 0 then show Command Menu
                    //todo: errors out in this case, tries to look up menu in shield subsystem and it shouldn't.
                    //Why is it even in there in the first place??

                    return retVal;
            }

            return this.EvalCommand(playerShip, userCommand);
        }

        private List<string> EvalCommand(IShip playerShip, string userCommand)
        {
            List<string> retVal;

            switch (this.Subscriber.PromptInfo.Level)
            {
                case 0:
                    retVal = this.EvalTopLevelMenuCommand(playerShip, userCommand);
                    break;

                default:
                    retVal = this.EvalSubLevelCommand(playerShip, userCommand, this.Subscriber.PromptInfo.Level);
                    break;
            }

            return retVal;
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
            MenuItems menuItems = this.Config.GetMenuItems($"{subsystem}Panel"); //todo: resource this
            bool acceptable = menuItems.Cast<MenuItemDef>().Any(m => m.name == stringToCheck && m.promptLevel == promptLevel) != null;
            return acceptable;
        }

        public void CreateCommandPanel()
        {
            //todo: resource out this menu
            SHIP_PANEL = new List<string>
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

            if (DEFAULTS.DEBUG_MODE)
            {
                SHIP_PANEL.Add("");
                SHIP_PANEL.Add("─────────────────────────────");
                SHIP_PANEL.Add(this.DisplayMenuItem(Menu.dbg));
            }
        }

        private List<string> EvalTopLevelMenuCommand(IShip playerShip, string menuCommand)
        {
            List<string> retVal = new List<string>();

            if (menuCommand == Menu.nto.ToString() || menuCommand == Menu.wrp.ToString()) //|| menuCommand == Menu.imp.ToString()
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Navigation;

                //todo: we may need to break out warp and imp, or change the process here because we can't tell which of the 3 we need for prompt.

                retVal = Navigation.For(playerShip).Controls(menuCommand);
            }
            //else if (menuCommand == Menu.wrp.ToString())
            //{
            //    this.Subscriber.PromptInfo.SubSystem = SubsystemType.Warp;
            //    retVal = Warp.For(playerShip).Controls(menuCommand).ToList();
            //}
            else if (menuCommand == Menu.imp.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Impulse;
                retVal = this.ImpulseMenu(playerShip).ToList();
            }
            else if (menuCommand == Menu.irs.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.ImmediateRangeScan;
                retVal = ImmediateRangeScan.For(playerShip).Controls().ToList();
            }
            else if (menuCommand == Menu.srs.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.ShortRangeScan;
                retVal = ShortRangeScan.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.lrs.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.LongRangeScan;
                retVal = LongRangeScan.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.crs.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.CombinedRangeScan;
                retVal = CombinedRangeScan.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.pha.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Phasers;
                retVal = Phasers.For(playerShip).Controls(playerShip).ToList();
            }
            else if (menuCommand == Menu.tor.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Torpedoes;
                retVal = Torpedoes.For(playerShip).Controls();
            }
            else if (menuCommand == Menu.she.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Shields;

                retVal = this.ShieldMenu(playerShip).ToList();
            }
            else if (menuCommand == Menu.com.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Computer;
                retVal = this.ComputerMenu(playerShip).ToList();
            }
            else if (menuCommand == Menu.toq.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Computer;
                retVal = Computer.For(playerShip).Controls(menuCommand); //todo: promptlevel should be able to tell us anything else we need.
            }
            else if (menuCommand == Menu.dmg.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.DamageControl;
                retVal = this.DamageControlMenu(playerShip).ToList();
            }
            else if (menuCommand == Menu.dbg.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Debug;
                retVal = this.DebugMenu(playerShip).ToList();
            }
            else if (menuCommand == Menu.ver.ToString())
            {
                retVal = this.Output.WriteLine("Application Version: " + this.GetConfigText("AppVersion").Remove(1,1)); //todo: resource this
            }
            else
            {
                if ((menuCommand != "?") && (menuCommand != "help")) //todo: resource this
                {
                        this.Output.WriteLine("Invalid Command"); //todo: make this to be red text
                }

                this.ResetPrompt();
                this.CreateCommandPanel();

                var panel = this.Panel(this.GetPanelHead(playerShip.Name), SHIP_PANEL).ToList();
                retVal.AddRange(panel);
            }

            return retVal.ToList();
        }

        #region Sub-Level Menus

        public List<string> EvalSubLevelCommand(IShip playerShip, string playerEnteredText, int promptLevel)
        {
            IEnumerable<string> retVal = new List<string>();

            string menuName = this.Subscriber.PromptInfo.SubSystem.Name;

            //todo: Finish other menus.  refactor their common menu creation steps
            if (this.IsAcceptable(playerEnteredText, this.Subscriber.PromptInfo.SubSystem, this.Subscriber.PromptInfo.Level))
            {
                ISubsystem subsystem = SubSystem_Base.GetSubsystemFor(playerShip, this.Subscriber.PromptInfo.SubSystem);
                retVal = subsystem.Controls(playerEnteredText);
            }
            else
            {
                retVal = new List<string>()
                {
                    $"Unrecognized Command. Exiting {menuName} Menu"      //todo: resource this
                }; 

                this.Subscriber.PromptInfo.Level = 0; //resets our menu level

                return null;
            }

            return retVal?.ToList();
        }

        private IEnumerable<string> DebugMenu(IShip playerShip)
        {
           IEnumerable<MenuItemDef> menuItems = this.Config.GetMenuItems($"{this.Subscriber.PromptInfo.SubSystem}Panel").Cast<MenuItemDef>();

            Interaction.AddShipPanelOption(menuItems, Debug.DEBUG_PANEL);

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

            IEnumerable<MenuItemDef> menuItems = this.Config.GetMenuItems($"{this.Subscriber.PromptInfo.SubSystem}Panel").Cast<MenuItemDef>();

            Interaction.AddShipPanelOption(menuItems, Computer.CONTROL_PANEL);

            this.OutputStrings(Computer.CONTROL_PANEL);
            this.WithNoEndCR(this.ENTER_COMPUTER_COMMAND);

            //todo: readline needs to be done using an event
            var computerCommand = Output.ReadLine().Trim().ToLower();

            Computer.For(playerShip).Controls(computerCommand);

            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> DamageControlMenu(IShip playerShip)
        {
            IEnumerable<MenuItemDef> menuItems = this.Config.GetMenuItems($"{this.Subscriber.PromptInfo.SubSystem}Panel").Cast<MenuItemDef>();

            Interaction.AddShipPanelOption(menuItems, DamageControl.DAMAGE_PANEL);

            this.OutputStrings(DamageControl.DAMAGE_PANEL);

            this.WithNoEndCR("Enter Damage Control Command: ");

            //todo: readline needs to be done using an event
            var damageControlCommand = Output.ReadLine().Trim().ToLower();

            DamageControl.For(playerShip).Controls(damageControlCommand);

            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> ShieldMenu(IShip playerShip, string shieldPanelCommand = "") //config, output
        {
            //todo: pass in dependencies and refactor to shield object
            //pass in config, output, subscriber, promptUser, outputstrings

            if (Shields.For(playerShip).Damaged()) return this.Output.Queue.ToList();

            Shields.SHIELD_PANEL = new List<string>
            {
                Environment.NewLine
            };

            var currentShieldEnergy = Shields.For(playerShip).Energy;

            try
            {
                var menuItems = this.Config.GetMenuItems($"{this.Subscriber.PromptInfo.SubSystem}Panel").Cast<MenuItemDef>();

                this.OutputShieldMenu(menuItems, currentShieldEnergy);
            }
            catch
            {
                //todo: fix error
            }

            Shields.For(playerShip).MaxTransfer = playerShip.Energy; //todo: this does nothing!

            string shieldPromptReply;

            //todo: this needs to be divined?
            this.PromptUser(SubsystemType.Shields, $"{this.Subscriber.PromptInfo.DefaultPrompt}Shield Control -> ",
                null, out shieldPromptReply, this.Output.Queue, 1);

            Shields.For(playerShip).Controls(shieldPanelCommand);

            return this.Output.Queue;
        }

        private IEnumerable<string> ImpulseMenu(IShip playerShip, string impulsePanelCommand = "") //config, output
        {
            //todo: pass in dependencies and refactor to shield object
            //pass in config, output, subscriber, promptUser, outputstrings

            //if (Impulse.For(playerShip).Damaged()) return this.Output.Queue.ToList();

            //todo: this should be a list of directions?
            Impulse.PANEL = new List<string>
            {
                Environment.NewLine,
                "panel1",
                "panel2"
            };

            try
            {
                //var menuItems = this.Config.GetMenuItems($"{this.Subscriber.PromptInfo.SubSystem}Panel").Cast<MenuItemDef>();

                //this.OutputMenu(menuItems);
            }
            catch
            {
                //todo: fix error
            }

            string impulsePromptReply;

            //todo: this needs to be divined?
            this.PromptUser(SubsystemType.Impulse, $"{this.Subscriber.PromptInfo.DefaultPrompt}Impulse Control -> ",
                null, out impulsePromptReply, this.Output.Queue, 1);

            Navigation.For(playerShip).Controls(impulsePanelCommand);

            return this.Output.Queue;
        }

        private void OutputShieldMenu(IEnumerable<MenuItemDef> menuItems, int currentShieldEnergy)
        {
            //todo: resource out header
            //todo: *DOWN* feature should be a upgrade functionality
            var menuItemDefs = menuItems as IList<MenuItemDef> ?? menuItems.ToList();

            if (currentShieldEnergy > 0)
            {
                //todo:add header from config file.

                // ReSharper disable once UseStringInterpolation //todo: resource this
                Shields.SHIELD_PANEL.Add(string.Format("─── Shield Status: ── {0} ──", $"< CURRENTLY AT: {currentShieldEnergy}>"));

                foreach (MenuItemDef menuItem in menuItemDefs)
                {
                    Shields.SHIELD_PANEL.Add($"{menuItem.name} {menuItem.divider} {menuItem.description}");
                }
            }
            else
            {
                //todo: resource out header
                // ReSharper disable once UseStringInterpolation
                Shields.SHIELD_PANEL.Add(string.Format("─── Shield Status: ── {0} ──", "DOWN"));

                MenuItemDef itemToAdd = menuItemDefs.First(m => m.name == "add"); //todo: resource this
                Shields.SHIELD_PANEL.Add($"{itemToAdd.name} {itemToAdd.divider} {itemToAdd.description}");
            }

            Interaction.AddShipPanelOption(menuItemDefs, Shields.SHIELD_PANEL);

            this.OutputStrings(Shields.SHIELD_PANEL);
        }

        private static void AddShipPanelOption(IEnumerable<MenuItemDef> menuItemDefs, ICollection<string> panel)
        {
            MenuItemDef shipOption = menuItemDefs.First(m => m.name == OBJECT_TYPE.SHIP.ToLower()); //todo: resource this
            panel.Add($"{shipOption.name} {shipOption.divider} {shipOption.description}");
        }

        #endregion

        #region Prompt

        public void ResetPrompt()
        {
            this.CurrentPrompt = this.Subscriber.PromptInfo.DefaultPrompt; //todo: set this to config file default prompt 

            this.Subscriber.PromptInfo.SubCommand = "";
            this.Subscriber.PromptInfo.SubSystem = SubsystemType.None;
            this.Subscriber.PromptInfo.Level = 0;
        }

        public void SetPrompt(int promptLevel)
        {
            if (promptLevel == 0)
            {
                this.ResetPrompt();
            }
            else
            {
                if (this.Subscriber.PromptInfo.MultiStepCommandChain != null)
                {
                    this.Subscriber.PromptInfo.SetSubCommandTo(promptLevel);
                }

                this.Subscriber.PromptInfo.Level = promptLevel;
            }
        }

        /// <summary>
        /// The point of this method is to get information from the user.  
        /// We must end the call that got us here so we can get the information back from the user.
        /// </summary>
        /// <param name="promptSubsystem"></param>
        /// <param name="promptDisplay"></param>
        /// <param name="promptMessage"></param>
        /// <param name="value"></param>
        /// <param name="queueToWriteTo"></param>
        /// <param name="subPromptLevel"></param>
        /// <returns></returns>
        public bool PromptUser(SubsystemType promptSubsystem, 
                                string promptDisplay, 
                                string promptMessage, 
                                out string value, 
                                Queue<string> queueToWriteTo, 
                                
                                int subPromptLevel = 0)
        {

            //this.Subscriber.PromptInfo.ShipReference = shipReference;
        
            bool retVal = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(promptMessage))
                {
                    this.WithNoEndCR(promptMessage);
                }

                this.CurrentPrompt = promptDisplay;

                //todo: subsystemtype.None is currently the holding for subsystems like Impulse, and Warp. they need to be broken out to a separate class.
                this.Subscriber.PromptInfo.SubSystem = promptSubsystem;
                this.Subscriber.PromptInfo.Level = subPromptLevel;

                //todo: an endCR might need to be added here

                value = "-1";
                retVal = true;
            }
            catch
            {
                value = "0";
            }

            // ReSharper disable once UnusedVariable
            foreach (string queueItem in this.Output.Queue.ToList())
            {
                queueToWriteTo.Enqueue(this.Output.Queue.Dequeue());
            }

            return retVal;
        }

        public bool PromptUser(SubsystemType promptSubsystem, string promptMessage, out int value, int subPromptLevel = 0)
        {
            throw new NotImplementedException();
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
            if (DEFAULTS.DEBUG_MODE)
            {
                this.Output.WriteLine(stringToOutput);
            }
        }

        public void SingleLine(string stringToOutput)
        {
            this.Output.WriteLine(stringToOutput);
        }

        public string ShipHitMessage(IShip attacker, int attackingEnergy)
        {
            Region attackerRegion = attacker.GetRegion();
            OutputCoordinate attackerSector = Utility.Utility.HideXorYIfNebula(attackerRegion, attacker.Sector.X.ToString(), attacker.Sector.Y.ToString());

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

            string shipHitBy = this.GetConfigText("shipHitBy");
            string message = string.Format(shipHitBy, attackerName, attackerSector.X, attackerSector.Y, attackingEnergy);

            return message;
        }

        public string MisfireMessage(IShip attacker)
        {
            var attackerRegion = attacker.GetRegion();
            var attackerSector = Utility.Utility.HideXorYIfNebula(attackerRegion, attacker.Sector.X.ToString(), attacker.Sector.Y.ToString());

            string attackerName = attackerRegion.Type == RegionType.Nebulae ? "Unknown Ship" : attacker.Name;

            if (attacker.Faction == FactionName.Federation)
            {
                attackerName = attacker.Name;
            }

            //todo: fix this
            //HACK: until starbases become real objects.. getting tired of this.
            if (attackerName == "Enterprise")
            {
                attackerName = "Hostile Starbase";
            }

            string misfireBy = this.GetConfigText("misfireBy");
            string misfireAtSector = this.GetConfigText("misfireAtSector");

            return string.Format(misfireBy + attackerName + misfireAtSector, attackerSector.X, attackerSector.Y);
        }

        public void OutputConditionAndWarnings(IShip ship, int shieldsDownLevel)
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

        public void PrintMissionResult(IShip ship, bool starbasesAreHostile, int starbasesLeft)
        {
            var commandResult = string.Empty;

            string missionFailed = this.GetConfigText("MissionFailed");
            string shipDestroyed = this.GetConfigText("ShipDestroyed");
            string energyExhausted = this.GetConfigText("EnergyExhausted");
            string allFedShipsDestroyed = this.GetConfigText("AllFedShipsDestroyed");
            string missionAccomplished = this.GetConfigText("MissionAccomplished");
            string timeOver = this.GetConfigText("TimeOver");

            if (ship.Destroyed)
            {
                commandResult = $"{missionFailed}: {ship.Name.ToUpper()} {shipDestroyed}";
            }
            else if (ship.Energy == 0)
            {
                commandResult = $"{missionFailed}: {ship.Name.ToUpper()} {energyExhausted}.";
            }
            else if (starbasesAreHostile && starbasesLeft == 0)
            {
                commandResult = $"{allFedShipsDestroyed}";
            }
            else if (this.TotalHostiles == 0)
            {
                commandResult = $"{missionAccomplished}";
            }
            else if (this.TimeRemaining == 0)
            {
                commandResult = $"{missionFailed} {ship.Name.ToUpper()} {timeOver}";
            }

            //else - No status to report.  Game continues

            this.Line(commandResult);
        }

        public void PrintMission()
        {
            this.Output.WriteLine(this.GetConfigText("MissionStatement"), this.TotalHostiles, this.TimeRemaining, this.Starbases);
            this.Output.WriteLine(this.GetConfigText("HelpStatement"));
            this.Output.WriteLine();
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }

        #endregion
    }
}


