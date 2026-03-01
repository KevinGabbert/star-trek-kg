using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using StarTrek_KG.Commands;
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
        public static readonly string COURSE_GRID =
            Environment.NewLine +
             " 4   5   6 " + Environment.NewLine +
            @"   \ | /  " + Environment.NewLine +
             "3 - <*> - 7" + Environment.NewLine +
            @"   / | \  " + Environment.NewLine +
             " 2   1   8" + Environment.NewLine +
            Environment.NewLine;

        #endregion

        public Interaction()
        {
            this.Config = new StarTrek_KG.Config.StarTrekKGSettings();
            this.Subscriber = new Subscriber(this.Config);
            this.Output = new SubscriberOutput(this.Config);
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

        public void RenderSectorCounts(bool renderingMyLocation, int? starbaseCount, int? starCount, int? hostileCount)
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

        public string RenderSectorCounts(int? starbaseCount, int? starCount, int? hostileCount)
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

        public void RenderUnscannedSector(bool renderingMyLocation)
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

        public void RenderSectors(CoordinateScanType scanType, ISubsystem subsystem)
        {
            IShip shipConnectedTo = subsystem.ShipConnectedTo;
            IGame game = shipConnectedTo.Map.Game;

            var location = shipConnectedTo.GetLocation();
            Sector region = game.Map.Sectors[location.Sector];
            var shieldsAutoRaised = Shields.For(shipConnectedTo).AutoRaiseShieldsIfNeeded(region);
            var printSector = new Render(this, game.Config);

            int totalHostiles = game.Map.Sectors.GetHostileCount();
            var isNebula = region.Type == SectorType.Nebulae;
            string regionDisplayName = region.Name;
            var sectorScanStringBuilder = new StringBuilder();

            if (isNebula)
            {
                regionDisplayName += " Nebula"; //todo: resource out.
            }

            this.Line("");

            switch (scanType)
            {
                case CoordinateScanType.CombinedRange:
                    printSector.CreateCRSViewScreen(region, game.Map, location, totalHostiles, regionDisplayName, isNebula, sectorScanStringBuilder);
                    break;

                case CoordinateScanType.ShortRange:
                    printSector.CreateSRSViewScreen(region, game.Map, location, totalHostiles, regionDisplayName, isNebula, sectorScanStringBuilder);
                    break;

                default:
                    throw new NotImplementedException();
            }

            printSector.OutputScanWarnings(region, game.Map, shieldsAutoRaised);

            region.ClearSectorsWithItem(CoordinateItem.Debug); //Clears any debug Markers that might have been set
            region.Scanned = true;

            this.Line("");
        }

        public List<string> RenderLRSData(IEnumerable<LRSResult> lrsData, IGame game)
        {
            var renderedResults = new List<string>();
            int scanColumn = 0;

            var verticalBoxLine = this.Config.Setting("VerticalBoxLine");
            var topLeft = this.Config.Setting("SingleLineTopLeft");
            var topMiddle = this.Config.Setting("SingleLineTopMiddle");
            var topRight = this.Config.Setting("SingleLineTopRight");
            var middleLeft = this.Config.Setting("SingleLineMiddleLeft");
            var middle = this.Config.Setting("SingleLineMiddle");
            var middleRight = this.Config.Setting("SingleLineMiddleRight");
            var bottomLeft = this.Config.Setting("SingleLineBottomLeft");
            var bottomMiddle = this.Config.Setting("SingleLineBottomMiddle");
            var bottomRight = this.Config.Setting("SingleLineBottomRight");
            var cellLine = new string(Convert.ToChar(this.Config.Setting("SingleLineCellLine")), 5);

            renderedResults.Add(topLeft + cellLine + topMiddle + cellLine + topMiddle + cellLine + topRight);

            string currentLRSScanLine = verticalBoxLine;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (LRSResult dataPoint in lrsData)
            {
                currentLRSScanLine = BuildLRSInterior(game, dataPoint, currentLRSScanLine, renderedResults, ref scanColumn);
            }

            renderedResults.Add(bottomLeft + cellLine + bottomMiddle + cellLine + bottomMiddle + cellLine + bottomRight);

            return renderedResults;
        }

        private static string BuildLRSInterior(IGame game, IScanResult dataPoint, string currentLRSScanLine, ICollection<string> renderedResults, ref int scanColumn)
        {
            string currentSectorResult = null;

            if (dataPoint.Unknown)
            {
                currentSectorResult = Utility.Utility.DamagedScannerUnit(dataPoint.Point);
            }
            else if (dataPoint.GalacticBarrier)
            {
                string barrierCrs;
                try
                {
                    barrierCrs = game.Config.GetSetting<string>("GalacticBarrierCRS");
                }
                catch
                {
                    barrierCrs = "XXX";
                }
                currentSectorResult = barrierCrs;
            }
            else
            {
                currentSectorResult += dataPoint;
            }

            var verticalBoxLine = game.Config.Setting("VerticalBoxLine");
            var middleLeft = game.Config.Setting("SingleLineMiddleLeft");
            var middle = game.Config.Setting("SingleLineMiddle");
            var middleRight = game.Config.Setting("SingleLineMiddleRight");
            var cellLine = new string(Convert.ToChar(game.Config.Setting("SingleLineCellLine")), 5);

            currentLRSScanLine += $" {currentSectorResult} " + verticalBoxLine;

            if (scanColumn == 2 || scanColumn == 5)
            {
                renderedResults.Add(currentLRSScanLine);
                renderedResults.Add(middleLeft + cellLine + middle + cellLine + middle + cellLine + middleRight);
                currentLRSScanLine = verticalBoxLine;
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

        //    renderedResults.Add("+-----------------+");

        //    string currentLRSScanLine = "¦";

        //    foreach (IRSResult dataPoint in irsData)
        //    {
        //        string currentSectorResult = null;

        //        if (dataPoint.Unknown)
        //        {
        //            currentSectorResult = Utility.Utility.DamagedScannerUnit();
        //        }
        //        else if (dataPoint.GalacticBarrier)
        //        {
        //            currentSectorResult = game.Config.GetSetting<string>("GalacticBarrierCRS");
        //        }
        //        else
        //        {
        //            currentSectorResult += dataPoint;
        //        }

        //        currentLRSScanLine += " " + currentSectorResult + " " + "¦";

        //        if (scanColumn == 2 || scanColumn == 5)
        //        {
        //            renderedResults.Add(currentLRSScanLine);
        //            renderedResults.Add("¦-----+-----+-----¦");
        //            currentLRSScanLine = "¦";
        //        }

        //        if (scanColumn == 8)
        //        {
        //            renderedResults.Add(currentLRSScanLine);
        //        }

        //        scanColumn++;
        //    }

        //    renderedResults.Add("+-----------------+");

        //    return renderedResults;
        //}

        //todo: refactor with RenderLRSWithNames
        public IEnumerable<string> RenderScanWithNames(ScanRenderType scanRenderType, string title, List<IScanResult> data, IGame game)
        {
            int scanColumn = 0;  //todo resource this
            int longestText = Interaction.GetLongestScanText(data);

            var galacticBarrierText = this.Config.GetSetting<string>("GalacticBarrierText");
            var barrierID = galacticBarrierText;
            var cellPadding = 1; //todo resource this

            int cellLength = longestText > barrierID.Length ? longestText : barrierID.Length;
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

            var coordinateIndicator = scanRenderType == ScanRenderType.DoubleSingleLine ? "°" : DEFAULTS.SECTOR_INDICATOR;

            foreach (IScanResult scanDataPoint in data)
            {
                string currentSectorName = "";
                string currentSectorResult = null;
                string regionCoordinate = "";

                if (scanDataPoint.Point != null)
                {
                    regionCoordinate = coordinateIndicator + scanDataPoint.Point.X + "." +
                                         scanDataPoint.Point.Y + "";

                    currentSectorName += scanDataPoint.SectorName;
                }

                if (scanDataPoint.Unknown)
                {
                    currentSectorResult = Utility.Utility.DamagedScannerUnit(scanDataPoint.Point);
                }
                else if (scanDataPoint.GalacticBarrier)
                {
                    currentSectorName = barrierID;
                    currentSectorResult = galacticBarrierText;
                }
                else
                {
                    currentSectorResult += scanDataPoint.ToScanString();
                }

                if (scanRenderType == ScanRenderType.DoubleSingleLine && scanDataPoint.MyLocation)
                {
                    currentSectorName = "YOU";
                    currentSectorResult = "YOU";
                }

                //breaks because coordinate is not populated when nebula

                currentLRSScanLine0 += " " + regionCoordinate.PadCenter(cellLength) + verticalBoxLine;
                currentLRSScanLine1 += " " + currentSectorName.PadCenter(cellLength) + verticalBoxLine;
                currentLRSScanLine2 += " " + currentSectorResult.PadCenter(cellLength) + verticalBoxLine; //todo resource this

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
            return COURSE_GRID;
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
            return "--- " + shipName + " ---";
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

            if (this.Subscriber.PromptInfo.Level == 0 &&
                playerShip?.Map?.Game?.IsWarGamesMode == true &&
                this.TryParseWarGamesNaturalLanguage(playerShip, userCommand, out var warGamesAction))
            {
                return this.ExecuteWarGamesNaturalLanguageAction(playerShip, warGamesAction).ToList();
            }

            if (this.Subscriber.PromptInfo.Level == 0)
            {
                string nlCommand = NaturalLanguageRouter.TryParse(userCommand);
                if (!string.IsNullOrWhiteSpace(nlCommand))
                {
                    userCommand = nlCommand;
                }
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

                case "title":
                    playerShip.Map.Game.ShowRandomTitle();
                    return this.Output.Queue.ToList();

                case "?":
                case "help":
                    this.ResetPrompt();
                    retVal.AddRange(this.Output.WriteLine($"--- {playerShip.Name} Command Help ---"));
                    retVal.AddRange(this.Output.WriteLine(""));
                    retVal.AddRange(this.Output.WriteLine("NAVIGATION"));
                    retVal.AddRange(this.Output.WriteLine("  imp   Impulse navigation"));
                    retVal.AddRange(this.Output.WriteLine("  wrp   Warp navigation"));
                    retVal.AddRange(this.Output.WriteLine("  nto   Navigate to object"));
                    retVal.AddRange(this.Output.WriteLine(""));
                    retVal.AddRange(this.Output.WriteLine("SCANNERS"));
                    retVal.AddRange(this.Output.WriteLine("  irs   Immediate range scan"));
                    retVal.AddRange(this.Output.WriteLine("  srs   Short range scan"));
                    retVal.AddRange(this.Output.WriteLine("  lrs   Long range scan"));
                    retVal.AddRange(this.Output.WriteLine("  crs   Combined range scan"));
                    retVal.AddRange(this.Output.WriteLine(""));
                    retVal.AddRange(this.Output.WriteLine("WEAPONS"));
                    retVal.AddRange(this.Output.WriteLine("  pha   Phasers"));
                    retVal.AddRange(this.Output.WriteLine("  tor   Photon torpedoes"));
                    retVal.AddRange(this.Output.WriteLine("  toq   Target object in region"));
                    retVal.AddRange(this.Output.WriteLine(""));
                    retVal.AddRange(this.Output.WriteLine("SYSTEMS"));
                    retVal.AddRange(this.Output.WriteLine("  she   Shields"));
                    retVal.AddRange(this.Output.WriteLine("  com   Computer"));
                    retVal.AddRange(this.Output.WriteLine("  dmg   Damage control"));
                    if (playerShip.Map?.Game?.IsWarGamesMode == true)
                    {
                        retVal.AddRange(this.Output.WriteLine("  wgm   War Games actor control"));
                    }
                    retVal.AddRange(this.Output.WriteLine(""));
                    retVal.AddRange(this.Output.WriteLine("NATURAL LANGUAGE EXAMPLES"));
                    retVal.AddRange(this.Output.WriteLine("  add 500 to shields"));
                    retVal.AddRange(this.Output.WriteLine("  raise shields"));
                    retVal.AddRange(this.Output.WriteLine("  warp 3 course 7"));
                    retVal.AddRange(this.Output.WriteLine("  move right 3"));
                    retVal.AddRange(this.Output.WriteLine("  long range scan"));
                    retVal.AddRange(this.Output.WriteLine("  fire phasers 200"));
                    retVal.AddRange(this.Output.WriteLine("  fire torpedo at 3 4"));
                    retVal.AddRange(this.Output.WriteLine(""));
                    return retVal;

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
            bool acceptable = menuItems.Cast<MenuItemDef>().Any(m => m.name == stringToCheck && m.promptLevel == promptLevel);
            return acceptable;
        }

        public void CreateCommandPanel()
        {
            this.CreateCommandPanelFor(null);
        }

        private void CreateCommandPanelFor(IShip playerShip)
        {
            //todo: resource out this menu
            SHIP_PANEL = new List<string>
            {
                "imp = Impulse Navigation",
                "wrp = Warp Navigation",
                "nto = Navigate To Object",
                "-----------------------------",
                "irs = Immediate Range Scan",
                "srs = Short Range Scan",
                "lrs = Long Range Scan",
                "crs = Combined Range Scan",
                "-----------------------------",
                "pha = Phaser Control",
                "tor = Photon Torpedo Control",
                "toq = Target Object in this Sector",
                "-----------------------------",
                "she = Shield Control",
                "com = Access Computer",
                "dmg = Damage Control"
            };

            if (playerShip?.Map?.Game?.IsWarGamesMode == true)
            {
                SHIP_PANEL.Add("wgm = War Games Actor Control");
            }

            if (DEFAULTS.DEBUG_MODE)
            {
                SHIP_PANEL.Add("");
                SHIP_PANEL.Add("-----------------------------");
                SHIP_PANEL.Add(this.DisplayMenuItem(Menu.dbg));
            }
        }

        private List<string> EvalTopLevelMenuCommand(IShip playerShip, string menuCommand)
        {
            //todo: resource this out.
            List<string> retVal = new List<string>();

            //if (menuCommand == Menu.nto.ToString() || menuCommand == Menu.wrp.ToString()) //|| menuCommand == Menu.imp.ToString()
            //{
            //    this.Subscriber.PromptInfo.SubSystem = SubsystemType.Navigation;

            //    //todo: we may need to break out warp and imp, or change the process here because we can't tell which of the 3 we need for prompt.

            //    retVal = Navigation.For(playerShip).Controls(menuCommand);
            //}

            if (menuCommand == Menu.nto.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Navigation;
                retVal = Navigation.For(playerShip).Controls(menuCommand);
            }
            else if (menuCommand == Menu.wrp.ToString())
            {
                this.Subscriber.PromptInfo.SubSystem = SubsystemType.Navigation;
                retVal = this.WarpMenu(playerShip).ToList();
            }


            //else if (menuCommand == Menu.wrp.ToString())
            //{
            //    //todo: deprecated. remove  - KLW
            //    //this.Subscriber.PromptInfo.SubSystem = SubsystemType.Warp;
            //    //retVal = Warp.For(playerShip).Controls(menuCommand).ToList();
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
            else if (menuCommand == Menu.wgm.ToString())
            {
                if (playerShip.Map?.Game?.IsWarGamesMode == true)
                {
                    this.Subscriber.PromptInfo.SubSystem = SubsystemType.WarGames;
                    retVal = this.WarGamesMenu(playerShip).ToList();
                }
                else
                {
                    retVal = this.Output.WriteLine("War Games Actor Control is only available in war games mode.");
                }
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
                this.CreateCommandPanelFor(playerShip);

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

            if ((this.Subscriber.PromptInfo.SubSystem == SubsystemType.Navigation ||
                 this.Subscriber.PromptInfo.SubSystem == SubsystemType.Impulse) &&
                this.Subscriber.PromptInfo.Level > 0)
            {
                ISubsystem navSubsystem = SubSystem_Base.GetSubsystemFor(playerShip, this.Subscriber.PromptInfo.SubSystem);
                retVal = navSubsystem.Controls(playerEnteredText);
                return retVal?.ToList();
            }

            if (this.Subscriber.PromptInfo.SubSystem == SubsystemType.WarGames &&
                this.Subscriber.PromptInfo.Level > 0)
            {
                retVal = this.EvalWarGamesCommand(playerShip, playerEnteredText);
                return retVal?.ToList();
            }

            if ((this.Subscriber.PromptInfo.SubSystem == SubsystemType.Torpedoes ||
                 this.Subscriber.PromptInfo.SubSystem == SubsystemType.Phasers) &&
                this.Subscriber.PromptInfo.Level > 0)
            {
                ISubsystem weaponSubsystem = SubSystem_Base.GetSubsystemFor(playerShip, this.Subscriber.PromptInfo.SubSystem);
                retVal = weaponSubsystem.Controls(playerEnteredText);
                return retVal?.ToList();
            }

            if (this.IsAcceptable(playerEnteredText, this.Subscriber.PromptInfo.SubSystem, this.Subscriber.PromptInfo.Level))
            {
                ISubsystem subsystem = SubSystem_Base.GetSubsystemFor(playerShip, this.Subscriber.PromptInfo.SubSystem);
                retVal = subsystem.Controls(playerEnteredText);
            }
            else
            {
                retVal = new List<string>
                {
                    $"Unrecognized Command. Exiting {menuName} Menu" //todo: resource this
                };

                this.Subscriber.PromptInfo.Level = 0; //resets our menu level

                return null;
            }

            return retVal?.ToList();
        }

        private IEnumerable<string> DebugMenu(IShip playerShip)
        {
            var rawMenuItems = this.Config.GetMenuItems($"{this.Subscriber.PromptInfo.SubSystem}Panel");
            if (rawMenuItems == null)
            {
                return this.Output.Queue.ToList();
            }

            var menuItems = rawMenuItems.Cast<MenuItemDef>().ToList();
            if (!menuItems.Any())
            {
                return this.Output.Queue.ToList();
            }

            var debugPanel = Debug.DEBUG_PANEL.ToList();
            Interaction.AddShipPanelOption(menuItems, debugPanel);

            this.OutputStrings(debugPanel);

            string promptReply;
            this.PromptUser(SubsystemType.Debug,
                $"{this.Subscriber.PromptInfo.DefaultPrompt}Debug -> ",
                null,
                out promptReply,
                this.Output.Queue,
                1);

            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> WarGamesMenu(IShip playerShip)
        {
            this.Output.WriteLine("");
            this.Output.WriteLine("--- War Games Actor Control ---");
            this.Output.WriteLine("hst = Add klingon ship");
            this.Output.WriteLine("fed = Add federation ship");
            this.Output.WriteLine("str = Add star");
            this.Output.WriteLine("stb = Add starbase");
            this.Output.WriteLine("deu = Add deuterium");
            this.Output.WriteLine("gmn = Add gravitic mine");
            this.Output.WriteLine("nl examples:");
            this.Output.WriteLine("  add 1 hostile to sector");
            this.Output.WriteLine("  add 3 klingons");
            this.Output.WriteLine("  add ikc nastaj");
            this.Output.WriteLine("  add 1 federation ship");
            this.Output.WriteLine("  add 3 starbases");
            this.Output.WriteLine("  destroy ikc nivdup");
            this.Output.WriteLine("  destroy 1 klingon");
            this.Output.WriteLine("  destroy 2 stars");
            this.Output.WriteLine("ship = Exit back to Ship Panel");
            this.Output.WriteLine("");

            string promptReply;
            this.PromptUser(
                SubsystemType.WarGames,
                $"{this.Subscriber.PromptInfo.DefaultPrompt}War Games -> ",
                null,
                out promptReply,
                this.Output.Queue,
                1);

            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> EvalWarGamesCommand(IShip playerShip, string command)
        {
            var normalizedCommand = (command ?? string.Empty).Trim();
            switch (normalizedCommand.ToLowerInvariant())
            {
                case "hst":
                    return this.AddRandomHostileShipsToCurrentSector(playerShip, 1);
                case "fed":
                    return this.AddRandomActorsToCurrentSector(playerShip, WarGamesActorKind.FactionShip, 1, FactionName.Federation);
                case "str":
                    return this.AddRandomActorsToCurrentSector(playerShip, WarGamesActorKind.Star, 1);
                case "stb":
                    return this.AddRandomActorsToCurrentSector(playerShip, WarGamesActorKind.Starbase, 1);
                case "deu":
                    return this.AddRandomActorsToCurrentSector(playerShip, WarGamesActorKind.Deuterium, 1);
                case "gmn":
                    return this.AddRandomActorsToCurrentSector(playerShip, WarGamesActorKind.GraviticMine, 1);
                case "back":
                    return this.WarGamesMenu(playerShip);
                default:
                    if (this.TryParseWarGamesNaturalLanguage(playerShip, normalizedCommand, out var action))
                    {
                        return this.ExecuteWarGamesNaturalLanguageAction(playerShip, action);
                    }

                    this.Output.WriteLine("Unrecognized war games actor command.");
                    return this.Output.Queue.ToList();
            }
        }

        private IEnumerable<string> ExecuteWarGamesNaturalLanguageAction(IShip playerShip, WarGamesAction action)
        {
            if (action.Mode == WarGamesActionMode.Add)
            {
                if (!string.IsNullOrWhiteSpace(action.TargetName))
                {
                    return this.AddNamedShipToCurrentSector(playerShip, action.TargetName, action.Faction);
                }

                if (action.ActorKind == WarGamesActorKind.HostileShip)
                {
                    return this.AddRandomHostileShipsToCurrentSector(playerShip, action.Count);
                }

                if (action.ActorKind == WarGamesActorKind.FactionShip && action.Faction != null)
                {
                    return this.AddRandomActorsToCurrentSector(playerShip, action.ActorKind, action.Count, action.Faction);
                }

                return this.AddRandomActorsToCurrentSector(playerShip, action.ActorKind, action.Count);
            }

            if (action.Mode == WarGamesActionMode.DestroyByName)
            {
                return this.DestroyActorByNameInCurrentSector(playerShip, action.TargetName);
            }

            if (action.ActorKind == WarGamesActorKind.HostileShip)
            {
                return this.DestroyActorsInCurrentSector(playerShip, action.ActorKind, action.Count);
            }

            if (action.ActorKind == WarGamesActorKind.FactionShip && action.Faction != null)
            {
                return this.DestroyActorsInCurrentSector(playerShip, action.ActorKind, action.Count, action.Faction);
            }

            return this.DestroyActorsInCurrentSector(playerShip, action.ActorKind, action.Count);
        }

        private IEnumerable<string> AddNamedShipToCurrentSector(IShip playerShip, string requestedName, FactionName requestedFaction = null)
        {
            var sector = playerShip.GetSector();
            if (sector == null)
            {
                this.Output.WriteLine("No active sector available.");
                return this.Output.Queue.ToList();
            }

            var empties = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
            if (!empties.Any())
            {
                this.Output.WriteLine($"Nothing can be added to sector [{sector.X},{sector.Y}] because it is full.");
                return this.Output.Queue.ToList();
            }
            var emptyCoordinate = empties[Utility.Utility.Random.Next(empties.Count)];

            var exactName = requestedName?.Trim();
            if (string.IsNullOrWhiteSpace(exactName))
            {
                this.Output.WriteLine("A ship name is required.");
                return this.Output.Queue.ToList();
            }

            if (!this.TryResolveShipNameAndFaction(playerShip, exactName, requestedFaction, out var resolvedName, out var resolvedFaction))
            {
                this.Output.WriteLine($"Ship '{exactName}' was not found in config ship lists.");
                return this.Output.Queue.ToList();
            }

            this.AddShipToCoordinate(playerShip, sector, emptyCoordinate, resolvedFaction, resolvedName);
            this.Output.WriteLine($"{resolvedFaction} ship {resolvedName} added at coordinate [{emptyCoordinate.X},{emptyCoordinate.Y}] in sector [{sector.X},{sector.Y}].");
            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> AddRandomHostileShipsToCurrentSector(IShip playerShip, int count)
        {
            var hostileFactions = this.GetHostileShipFactions(playerShip).ToList();
            if (!hostileFactions.Any())
            {
                this.Output.WriteLine("No hostile factions are configured.");
                return this.Output.Queue.ToList();
            }

            var sector = playerShip.GetSector();
            if (sector == null)
            {
                this.Output.WriteLine("No active sector available.");
                return this.Output.Queue.ToList();
            }

            if (count <= 0)
            {
                this.Output.WriteLine("Count must be greater than zero.");
                return this.Output.Queue.ToList();
            }

            var empties = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
            if (!empties.Any())
            {
                this.Output.WriteLine($"Nothing can be added to sector [{sector.X},{sector.Y}] because it is full.");
                return this.Output.Queue.ToList();
            }

            var itemsToAdd = Math.Min(count, empties.Count);
            var omitted = count - itemsToAdd;

            for (var i = 0; i < itemsToAdd; i++)
            {
                var coordinate = empties[Utility.Utility.Random.Next(empties.Count)];
                empties.Remove(coordinate);
                var faction = hostileFactions[Utility.Utility.Random.Next(hostileFactions.Count)];
                this.AddActorToCoordinate(playerShip, sector, coordinate, WarGamesActorKind.FactionShip, faction);
                var shipName = (coordinate.Object as IShip)?.Name ?? "unknown";
                this.Output.WriteLine($"{faction} hostile {shipName} added at coordinate [{coordinate.X},{coordinate.Y}] in sector [{sector.X},{sector.Y}].");
            }

            if (omitted > 0)
            {
                this.Output.WriteLine($"{omitted} actor(s) were not added because sector [{sector.X},{sector.Y}] is full.");
            }

            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> AddRandomActorsToCurrentSector(IShip playerShip, WarGamesActorKind actorKind, int count, FactionName faction = null)
        {
            var sector = playerShip.GetSector();
            if (sector == null)
            {
                this.Output.WriteLine("No active sector available.");
                return this.Output.Queue.ToList();
            }

            if (count <= 0)
            {
                this.Output.WriteLine("Count must be greater than zero.");
                return this.Output.Queue.ToList();
            }

            var empties = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
            if (!empties.Any())
            {
                this.Output.WriteLine($"Nothing can be added to sector [{sector.X},{sector.Y}] because it is full.");
                return this.Output.Queue.ToList();
            }

            var itemsToAdd = Math.Min(count, empties.Count);
            var omitted = count - itemsToAdd;

            for (var i = 0; i < itemsToAdd; i++)
            {
                var coordinate = empties[Utility.Utility.Random.Next(empties.Count)];
                empties.Remove(coordinate);
                this.AddActorToCoordinate(playerShip, sector, coordinate, actorKind, faction);

                if (actorKind == WarGamesActorKind.FactionShip)
                {
                    var shipName = (coordinate.Object as IShip)?.Name ?? "unknown";
                    this.Output.WriteLine($"{faction} ship {shipName} added at coordinate [{coordinate.X},{coordinate.Y}] in sector [{sector.X},{sector.Y}].");
                }
                else
                {
                    this.Output.WriteLine($"{this.GetWarGamesActorDisplayName(actorKind)} added at coordinate [{coordinate.X},{coordinate.Y}] in sector [{sector.X},{sector.Y}].");
                }
            }

            if (omitted > 0)
            {
                this.Output.WriteLine($"{omitted} actor(s) were not added because sector [{sector.X},{sector.Y}] is full.");
            }

            return this.Output.Queue.ToList();
        }

        private void AddActorToCoordinate(IShip playerShip, ISector sector, Coordinate coordinate, WarGamesActorKind actorKind, FactionName faction = null)
        {
            switch (actorKind)
            {
                case WarGamesActorKind.HostileShip:
                    var hostileFactions = this.GetHostileShipFactions(playerShip).ToList();
                    var hostileFaction = hostileFactions.Any()
                        ? hostileFactions[Utility.Utility.Random.Next(hostileFactions.Count)]
                        : FactionName.Klingon;
                    this.AddShipToCoordinate(playerShip, sector, coordinate, hostileFaction, null);
                    return;

                case WarGamesActorKind.FactionShip:
                    var useFaction = faction ?? FactionName.Klingon;
                    this.AddShipToCoordinate(playerShip, sector, coordinate, useFaction, null);
                    return;

                case WarGamesActorKind.Star:
                    coordinate.Item = CoordinateItem.Star;
                    coordinate.Object = new Actors.Star
                    {
                        Name = $"{sector.Name} Star",
                        Designation = "*"
                    };
                    return;

                case WarGamesActorKind.Starbase:
                    coordinate.Item = CoordinateItem.Starbase;
                    coordinate.Object = null;
                    return;

                case WarGamesActorKind.Deuterium:
                    var min = this.Config.GetSetting<int>("DeuteriumMin");
                    var max = this.Config.GetSetting<int>("DeuteriumMax");
                    if (max < min)
                    {
                        max = min;
                    }

                    coordinate.Item = CoordinateItem.Deuterium;
                    coordinate.Object = new Deuterium(Utility.Utility.Random.Next(min, max + 1));
                    return;

                case WarGamesActorKind.GraviticMine:
                    coordinate.Item = CoordinateItem.GraviticMine;
                    coordinate.Object = new GraviticMine
                    {
                        Coordinate = coordinate
                    };
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(actorKind), actorKind, null);
            }
        }

        private void AddShipToCoordinate(IShip playerShip, ISector sector, Coordinate coordinate, FactionName faction, string fallbackName = null)
        {
            var names = playerShip.Map.Config.ShipNames(faction);
            var name = names != null && names.Any()
                ? names[Utility.Utility.Random.Next(names.Count)]
                : fallbackName;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"{faction} Patrol";
            }

            var ship = new Actors.Ship(faction, name, coordinate, playerShip.Map);
            sector.AddShip(ship, coordinate);
        }

        private IEnumerable<string> DestroyActorByNameInCurrentSector(IShip playerShip, string targetName)
        {
            var sector = playerShip.GetSector();
            if (sector == null)
            {
                this.Output.WriteLine("No active sector available.");
                return this.Output.Queue.ToList();
            }

            if (string.IsNullOrWhiteSpace(targetName))
            {
                this.Output.WriteLine("A target name is required.");
                return this.Output.Queue.ToList();
            }

            var coordinate = sector.Coordinates.FirstOrDefault(c =>
                c.Object != null &&
                !string.IsNullOrWhiteSpace(c.Object.Name) &&
                string.Equals(c.Object.Name, targetName, StringComparison.OrdinalIgnoreCase));

            if (coordinate == null)
            {
                this.Output.WriteLine($"No actor named '{targetName}' found in sector [{sector.X},{sector.Y}].");
                return this.Output.Queue.ToList();
            }

            var destroyedName = coordinate.Object?.Name ?? targetName;
            coordinate.Item = CoordinateItem.Empty;
            coordinate.Object = null;
            this.Output.WriteLine($"Destroyed {destroyedName} at coordinate [{coordinate.X},{coordinate.Y}] in sector [{sector.X},{sector.Y}].");
            return this.Output.Queue.ToList();
        }

        private IEnumerable<string> DestroyActorsInCurrentSector(IShip playerShip, WarGamesActorKind actorKind, int count, FactionName faction = null)
        {
            var sector = playerShip.GetSector();
            if (sector == null)
            {
                this.Output.WriteLine("No active sector available.");
                return this.Output.Queue.ToList();
            }

            if (count <= 0)
            {
                this.Output.WriteLine("Count must be greater than zero.");
                return this.Output.Queue.ToList();
            }

            var matches = this.GetMatchingCoordinatesForActor(sector, actorKind, faction).ToList();
            if (!matches.Any())
            {
                var actorDisplay = actorKind == WarGamesActorKind.FactionShip && faction != null
                    ? $"{faction} ship"
                    : this.GetWarGamesActorDisplayName(actorKind);
                this.Output.WriteLine($"No {actorDisplay} found in sector [{sector.X},{sector.Y}].");
                return this.Output.Queue.ToList();
            }

            var toRemove = Math.Min(count, matches.Count);
            var omitted = count - toRemove;

            for (var i = 0; i < toRemove; i++)
            {
                var coordinate = matches[Utility.Utility.Random.Next(matches.Count)];
                matches.Remove(coordinate);
                var destroyedName = coordinate.Object?.Name;
                coordinate.Item = CoordinateItem.Empty;
                coordinate.Object = null;

                if (!string.IsNullOrWhiteSpace(destroyedName))
                {
                    this.Output.WriteLine($"Destroyed {destroyedName} at coordinate [{coordinate.X},{coordinate.Y}] in sector [{sector.X},{sector.Y}].");
                }
                else
                {
                    this.Output.WriteLine($"Destroyed {this.GetWarGamesActorDisplayName(actorKind)} at coordinate [{coordinate.X},{coordinate.Y}] in sector [{sector.X},{sector.Y}].");
                }
            }

            if (omitted > 0)
            {
                this.Output.WriteLine($"{omitted} actor(s) could not be destroyed because only {toRemove} matched in sector [{sector.X},{sector.Y}].");
            }

            return this.Output.Queue.ToList();
        }

        private IEnumerable<Coordinate> GetMatchingCoordinatesForActor(ISector sector, WarGamesActorKind actorKind, FactionName faction = null)
        {
            switch (actorKind)
            {
                case WarGamesActorKind.HostileShip:
                    return sector.Coordinates.Where(c => c.Item == CoordinateItem.HostileShip &&
                                                         c.Object is IShip);
                case WarGamesActorKind.FactionShip:
                    if (faction == null)
                    {
                        return Enumerable.Empty<Coordinate>();
                    }

                    return sector.Coordinates.Where(c => c.Item == CoordinateItem.HostileShip &&
                                                         c.Object is IShip ship &&
                                                         ship.Faction == faction);
                case WarGamesActorKind.Star:
                    return sector.Coordinates.Where(c => c.Item == CoordinateItem.Star);
                case WarGamesActorKind.Starbase:
                    return sector.Coordinates.Where(c => c.Item == CoordinateItem.Starbase);
                case WarGamesActorKind.Deuterium:
                    return sector.Coordinates.Where(c => c.Item == CoordinateItem.Deuterium);
                case WarGamesActorKind.GraviticMine:
                    return sector.Coordinates.Where(c => c.Item == CoordinateItem.GraviticMine);
                default:
                    return Enumerable.Empty<Coordinate>();
            }
        }

        private string GetWarGamesActorDisplayName(WarGamesActorKind actorKind)
        {
            switch (actorKind)
            {
                case WarGamesActorKind.HostileShip:
                    return "hostile ship";
                case WarGamesActorKind.FactionShip:
                    return "faction ship";
                case WarGamesActorKind.Star:
                    return "star";
                case WarGamesActorKind.Starbase:
                    return "starbase";
                case WarGamesActorKind.Deuterium:
                    return "deuterium";
                case WarGamesActorKind.GraviticMine:
                    return "gravitic mine";
                default:
                    return "actor";
            }
        }

        private bool TryParseWarGamesNaturalLanguage(IShip playerShip, string command, out WarGamesAction action)
        {
            action = null;

            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            var normalized = Regex.Replace(command.Trim(), @"\s+", " ");

            var addMatch = Regex.Match(normalized, @"^add\s+(?<count>\d+)\s+(?<actor>.+?)(?:\s+(?:to|in)\s+sector)?$", RegexOptions.IgnoreCase);
            if (addMatch.Success)
            {
                if (!int.TryParse(addMatch.Groups["count"].Value, out var addCount))
                {
                    return false;
                }

                var actorText = this.TrimWarGamesSectorQualifier(addMatch.Groups["actor"].Value);

                if (this.TryMapWarGamesActorType(actorText, out var actorKind))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.Add,
                        ActorKind = actorKind,
                        Count = addCount
                    };
                    return true;
                }

                if (this.TryMapFaction(actorText, out var faction))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.Add,
                        ActorKind = WarGamesActorKind.FactionShip,
                        Faction = faction,
                        Count = addCount
                    };
                    return true;
                }

                if (addCount == 1 &&
                    this.TryResolveShipNameAndFaction(playerShip, actorText, null, out var resolvedName, out var resolvedFaction))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.Add,
                        ActorKind = WarGamesActorKind.FactionShip,
                        Faction = resolvedFaction,
                        TargetName = resolvedName,
                        Count = 1
                    };
                    return true;
                }

                return false;
            }

            var addSingleMatch = Regex.Match(normalized, @"^add\s+(?<actor>.+?)(?:\s+(?:to|in)\s+sector)?$", RegexOptions.IgnoreCase);
            if (addSingleMatch.Success)
            {
                var actorText = this.TrimWarGamesSectorQualifier(addSingleMatch.Groups["actor"].Value);

                if (this.TryMapWarGamesActorType(actorText, out var actorKind))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.Add,
                        ActorKind = actorKind,
                        Count = 1
                    };
                    return true;
                }

                if (this.TryMapFaction(actorText, out var faction))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.Add,
                        ActorKind = WarGamesActorKind.FactionShip,
                        Faction = faction,
                        Count = 1
                    };
                    return true;
                }

                if (this.TryResolveShipNameAndFaction(playerShip, actorText, null, out var resolvedName, out var resolvedFaction))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.Add,
                        ActorKind = WarGamesActorKind.FactionShip,
                        Faction = resolvedFaction,
                        TargetName = resolvedName,
                        Count = 1
                    };
                    return true;
                }

                return false;
            }

            var destroyCountMatch = Regex.Match(normalized, @"^destroy\s+(?<count>\d+)\s+(?<actor>.+?)(?:\s+(?:from|in)\s+sector)?$", RegexOptions.IgnoreCase);
            if (destroyCountMatch.Success)
            {
                if (!int.TryParse(destroyCountMatch.Groups["count"].Value, out var destroyCount))
                {
                    return false;
                }

                var actorText = this.TrimWarGamesSectorQualifier(destroyCountMatch.Groups["actor"].Value);

                if (this.TryMapWarGamesActorType(actorText, out var actorKind))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.DestroyByActor,
                        ActorKind = actorKind,
                        Count = destroyCount
                    };
                    return true;
                }

                if (this.TryMapFaction(actorText, out var faction))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.DestroyByActor,
                        ActorKind = WarGamesActorKind.FactionShip,
                        Faction = faction,
                        Count = destroyCount
                    };
                    return true;
                }

                if (destroyCount == 1 &&
                    this.TryResolveShipNameAndFaction(playerShip, actorText, null, out var resolvedName, out _))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.DestroyByName,
                        TargetName = resolvedName
                    };
                    return true;
                }

                return false;
            }

            var destroySingleTypeMatch = Regex.Match(normalized, @"^destroy\s+(?<actor>.+?)(?:\s+(?:from|in)\s+sector)?$", RegexOptions.IgnoreCase);
            if (destroySingleTypeMatch.Success)
            {
                var actorText = this.TrimWarGamesSectorQualifier(destroySingleTypeMatch.Groups["actor"].Value);
                if (this.TryMapWarGamesActorType(actorText, out var actorKind))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.DestroyByActor,
                        ActorKind = actorKind,
                        Count = 1
                    };
                    return true;
                }

                if (this.TryMapFaction(actorText, out var faction))
                {
                    action = new WarGamesAction
                    {
                        Mode = WarGamesActionMode.DestroyByActor,
                        ActorKind = WarGamesActorKind.FactionShip,
                        Faction = faction,
                        Count = 1
                    };
                    return true;
                }

                action = new WarGamesAction
                {
                    Mode = WarGamesActionMode.DestroyByName,
                    TargetName = actorText
                };
                return true;
            }

            return false;
        }

        private string TrimWarGamesSectorQualifier(string text)
        {
            var normalized = Regex.Replace((text ?? string.Empty).Trim(), @"\s+", " ");
            normalized = Regex.Replace(normalized, @"\s+(to|in|from)\s+sector$", string.Empty, RegexOptions.IgnoreCase);
            return normalized.Trim();
        }

        private bool TryMapWarGamesActorType(string actorText, out WarGamesActorKind actorKind)
        {
            actorKind = WarGamesActorKind.Star;
            if (string.IsNullOrWhiteSpace(actorText))
            {
                return false;
            }

            var normalized = actorText.Trim().ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"\s+", " ");

            switch (normalized)
            {
                case "hostile":
                case "hostiles":
                case "hostile ship":
                case "hostile ships":
                case "ship":
                case "ships":
                    actorKind = WarGamesActorKind.HostileShip;
                    return true;

                case "star":
                case "stars":
                    actorKind = WarGamesActorKind.Star;
                    return true;

                case "starbase":
                case "starbases":
                    actorKind = WarGamesActorKind.Starbase;
                    return true;

                case "deuterium":
                case "deuterium sector":
                case "deuterium sectors":
                    actorKind = WarGamesActorKind.Deuterium;
                    return true;

                case "gravitic mine":
                case "gravitic mines":
                case "mine":
                case "mines":
                    actorKind = WarGamesActorKind.GraviticMine;
                    return true;

                default:
                    return false;
            }
        }

        private bool TryMapFaction(string factionText, out FactionName faction)
        {
            faction = null;
            if (string.IsNullOrWhiteSpace(factionText))
            {
                return false;
            }

            var normalized = factionText.Trim().ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"\s+", " ");

            switch (normalized)
            {
                case "fed":
                case "feds":
                case "federation":
                case "federations":
                case "federation ship":
                case "federation ships":
                    faction = FactionName.Federation;
                    return true;
                case "klingon":
                case "klingons":
                case "klingon ship":
                case "klingon ships":
                    faction = FactionName.Klingon;
                    return true;
                case "romulan":
                case "romulans":
                case "romulan ship":
                case "romulan ships":
                    faction = FactionName.Romulan;
                    return true;
                case "vulcan":
                case "vulcans":
                case "vulcan ship":
                case "vulcan ships":
                    faction = FactionName.Vulcan;
                    return true;
                case "gorn":
                case "gorns":
                case "gorn ship":
                case "gorn ships":
                    faction = FactionName.Gorn;
                    return true;
                case "other":
                case "other ship":
                case "other ships":
                    faction = FactionName.Other;
                    return true;
                case "testfaction":
                case "test faction":
                case "testfactions":
                case "test faction ship":
                case "test faction ships":
                    faction = FactionName.TestFaction;
                    return true;
                default:
                    return false;
            }
        }

        private bool TryResolveShipNameAndFaction(
            IShip playerShip,
            string requestedName,
            FactionName preferredFaction,
            out string resolvedName,
            out FactionName resolvedFaction)
        {
            resolvedName = null;
            resolvedFaction = null;

            if (playerShip?.Map?.Config == null || string.IsNullOrWhiteSpace(requestedName))
            {
                return false;
            }

            var candidateFactions = new List<FactionName>();
            if (preferredFaction != null)
            {
                candidateFactions.Add(preferredFaction);
            }

            candidateFactions.AddRange(this.GetKnownShipFactions().Where(f => preferredFaction == null || f != preferredFaction));

            foreach (var faction in candidateFactions)
            {
                List<string> names;
                try
                {
                    names = playerShip.Map.Config.ShipNames(faction);
                }
                catch
                {
                    continue;
                }

                var match = names?.FirstOrDefault(n => string.Equals(n?.Trim(), requestedName.Trim(), StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(match))
                {
                    resolvedName = match.Trim();
                    resolvedFaction = faction;
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<FactionName> GetHostileShipFactions(IShip playerShip)
        {
            var factions = this.GetKnownShipFactions();
            var hostileFactions = new List<FactionName>();

            foreach (var faction in factions)
            {
                if (!this.IsHostileFaction(playerShip, faction))
                {
                    continue;
                }

                try
                {
                    var names = playerShip.Map.Config.ShipNames(faction);
                    if (names != null && names.Any())
                    {
                        hostileFactions.Add(faction);
                    }
                }
                catch
                {
                }
            }

            return hostileFactions;
        }

        private bool IsHostileFaction(IShip playerShip, FactionName faction)
        {
            try
            {
                var factionElement = playerShip?.Map?.Config?.Factions?[faction.ToString()];
                if (factionElement != null)
                {
                    return string.Equals(factionElement.allegiance, "Hostile", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
            }

            return faction != FactionName.Vulcan;
        }

        private List<FactionName> GetKnownShipFactions()
        {
            return new List<FactionName>
            {
                FactionName.Klingon,
                FactionName.Federation,
                FactionName.Romulan,
                FactionName.Gorn,
                FactionName.Vulcan,
                FactionName.Other,
                FactionName.TestFaction
            };
        }

        private enum WarGamesActionMode
        {
            Add,
            DestroyByActor,
            DestroyByName
        }

        private enum WarGamesActorKind
        {
            HostileShip,
            FactionShip,
            Star,
            Starbase,
            Deuterium,
            GraviticMine
        }

        private sealed class WarGamesAction
        {
            public WarGamesActionMode Mode { get; set; }
            public WarGamesActorKind ActorKind { get; set; }
            public FactionName Faction { get; set; }
            public int Count { get; set; }
            public string TargetName { get; set; }
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
            //todo: pass in dependencies and refactor to impulse object
            //pass in config, output, subscriber, promptUser, outputstrings

            //if (Impulse.For(playerShip).Damaged()) return this.Output.Queue.ToList();

            Impulse.IMPULSE_PANEL = new List<string>{ Environment.NewLine };

            try
            {
                var menuItems = this.Config.GetMenuItems($"{this.Subscriber.PromptInfo.SubSystem}Panel").Cast<MenuItemDef>();

                this.OutputImpulseMenu(menuItems);
            }
            catch
            {
                //todo: fix error
            }

            string impulsePromptReply;

            //todo: this needs to be divined?
            this.PromptUser(SubsystemType.Impulse, 
                            $"{this.Subscriber.PromptInfo.DefaultPrompt}Impulse Control -> Enter Direction ->",
                            null, 
                            out impulsePromptReply, 
                            this.Output.Queue, 
                            1);

            if (!string.IsNullOrWhiteSpace(impulsePanelCommand))
            {
                Navigation.For(playerShip).Controls(impulsePanelCommand);
            }

            return this.Output.Queue;
        }

        private void OutputImpulseMenu(IEnumerable<MenuItemDef> menuItems)
        {
            //todo: resource out header
            var menuItemDefs = menuItems as IList<MenuItemDef> ?? menuItems.ToList();

            //todo:add header from config file.

            // ReSharper disable once UseStringInterpolation //todo: resource this
            Impulse.IMPULSE_PANEL.Add($"--- Impulse Status: -- <Not Implemented Yet> --");

            Impulse.IMPULSE_PANEL.Add(Environment.NewLine);
            foreach (var line in COURSE_GRID.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Impulse.IMPULSE_PANEL.Add("    " + line);
                }
            }
            Impulse.IMPULSE_PANEL.Add(Environment.NewLine);

            foreach (MenuItemDef menuItem in menuItemDefs)
            {
                Impulse.IMPULSE_PANEL.Add($"{menuItem.name} {menuItem.divider} {menuItem.description}");
            }

            Interaction.AddShipPanelOption(menuItemDefs, Impulse.IMPULSE_PANEL);

            this.OutputStrings(Impulse.IMPULSE_PANEL);
        }

        private IEnumerable<string> WarpMenu(IShip playerShip, string warpPanelCommand = "")
        {
            // Setup Warp panel
            Navigation.WARP_PANEL = new List<string> { Environment.NewLine };

            try
            {
                var menuItems = this.Config.GetMenuItems($"{this.Subscriber.PromptInfo.SubSystem}Panel").Cast<MenuItemDef>();
                this.OutputWarpMenu(playerShip, menuItems);
            }
            catch
            {
                // todo: handle config error
            }

            string warpPromptReply;

            this.PromptUser(
                SubsystemType.Navigation,
                $"{this.Subscriber.PromptInfo.DefaultPrompt}Warp Control -> ",
                this.RenderCourse() + "Enter Course: ",
                out warpPromptReply,
                this.Output.Queue,
                1
            );

            this.Subscriber.PromptInfo.SubCommand = SubsystemType.Warp.Abbreviation;
            if (!string.IsNullOrWhiteSpace(warpPanelCommand))
            {
                Navigation.For(playerShip).Controls(warpPanelCommand);
            }

            return this.Output.Queue;
        }


        private void OutputWarpMenu(IShip playerShip, IEnumerable<MenuItemDef> menuItems)
        {
            int damage = Navigation.For(playerShip).Damage;
            Navigation.WARP_PANEL.Add($"--- Warp: Dmg {damage}% --");
            Navigation.WARP_PANEL.Add(Environment.NewLine);
            Navigation.WARP_PANEL.Add(Environment.NewLine);

            this.OutputStrings(Navigation.WARP_PANEL);
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
                Shields.SHIELD_PANEL.Add(string.Format("--- Shield Status: -- {0} --", $"< CURRENTLY AT: {currentShieldEnergy}>"));

                foreach (MenuItemDef menuItem in menuItemDefs)
                {
                    Shields.SHIELD_PANEL.Add($"{menuItem.name} {menuItem.divider} {menuItem.description}");
                }
            }
            else
            {
                //todo: resource out header
                // ReSharper disable once UseStringInterpolation
                Shields.SHIELD_PANEL.Add(string.Format("--- Shield Status: -- {0} --", "DOWN"));

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
                    foreach (var line in promptMessage.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None))
                    {
                        this.Output.WriteLine(line);
                    }
                }

                this.CurrentPrompt = promptDisplay;

                this.Subscriber.PromptInfo.SubSystem = promptSubsystem;
                this.Subscriber.PromptInfo.Level = subPromptLevel;

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
            //todo: Game.Mode to submenu?
            //that mode will persist across ajax calls, so user will have to either type in a submenu entry, or 
            //exit it.



            foreach (var line in promptMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                this.Output.WriteLine(line);

            }
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
            Sector attackerSector = attacker.GetSector();
            OutputPoint attackerPoint = Utility.Utility.HideXorYIfNebula(attackerSector, attacker.Coordinate.X.ToString(), attacker.Coordinate.Y.ToString());

            string attackerName = attackerSector.Type == SectorType.Nebulae ? "Unknown Ship" : attacker.Name;

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
            string message = string.Format(shipHitBy, attackerName, attackerPoint.X, attackerPoint.Y, attackingEnergy);

            return message;
        }

        public string MisfireMessage(IShip attacker)
        {
            var attackerSector = attacker.GetSector();
            var attackerPoint = Utility.Utility.HideXorYIfNebula(attackerSector, attacker.Coordinate.X.ToString(), attacker.Coordinate.Y.ToString());

            string attackerName = attackerSector.Type == SectorType.Nebulae ? "Unknown Ship" : attacker.Name;

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
            string misfireAtCoordinate = this.GetConfigText("misfireAtCoordinate");

            return string.Format(misfireBy + attackerName + misfireAtCoordinate, attackerPoint.X, attackerPoint.Y);
        }

        public void OutputConditionAndWarnings(IShip ship, int shieldsDownLevel)
        {
            // Suppress condition warnings if the player is in the Warp control men

            //todo: making this work for now, but wrp should still become its own subsystem in the future.
            //this supression message needs to be more standardized
            var promptInfo = ship.Map.Game.Interact.Subscriber.PromptInfo;
            if (promptInfo.SubSystem == SubsystemType.Navigation &&
    (string.Equals(promptInfo.SubCommand, "wrp", StringComparison.OrdinalIgnoreCase)
     || promptInfo.Level >= 1))
            {
                return;
            }


            ship.GetConditionAndSetIcon();

            if (ship.AtLowEnergyLevel())
            {
                this.ResourceLine("LowEnergyLevel");
            }

            if (ship.GetSector().Type == SectorType.Nebulae)
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
                if (result?.SectorName != null)
                {
                    longestName = longestName.Length > result.SectorName.Length ? longestName : result.SectorName;
                }
            }

            return longestName;
        }

        private static int GetLongestScanText(IEnumerable<IScanResult> data)
        {
            int longest = 0;

            foreach (IScanResult result in data)
            {
                if (result?.SectorName != null && result.SectorName.Length > longest)
                {
                    longest = result.SectorName.Length;
                }

                if (result != null)
                {
                    string scanText = result.ToScanString();
                    if (scanText != null && scanText.Length > longest)
                    {
                        longest = scanText.Length;
                    }
                }
            }

            return longest;
        }

        public void PrintMissionResult(IShip ship, bool starbasesAreHostile, int starbasesLeft)
        {
            if (ship?.Map == null)
            {
                return;
            }

            var commandResult = string.Empty;
            var hostilesLeft = ship.Map.Sectors.GetHostileCount();
            var timeRemaining = ship.Map.timeRemaining;

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
                commandResult = $"{missionFailed}: {ship.Name.ToUpper()} {energyExhausted}. Ship {ship.Name.ToUpper()} has run out of power.";
            }
            else if (starbasesAreHostile && starbasesLeft == 0)
            {
                commandResult = $"{allFedShipsDestroyed}";
            }
            else if (hostilesLeft == 0)
            {
                commandResult = $"{missionAccomplished}";
            }
            else if (timeRemaining == 0)
            {
                commandResult = $"{missionFailed} {ship.Name.ToUpper()} {timeOver}";
            }

            //else - No status to report.  Game continues

            if (!string.IsNullOrWhiteSpace(commandResult))
            {
                this.Line(commandResult);
            }
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



