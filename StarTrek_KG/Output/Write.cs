using System;
using System.Collections.Generic;
using System.Reflection;
using StarTrek_KG.Actors;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Subsystem;
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

        public List<string> ACTIVITY_PANEL = new List<string>();

        //TODO:  Have Game expose and raise an output event
        //Have UI subscribe to it.

        //Output object
        //goal is to output message that a UI can read
        //all *print* mnemonics will be changed to Output
        //UI needs to read this text and display it how it wants

        public Write(int totalHostiles, int starbases, int stardate, int timeRemaining, IStarTrekKGSettings config)
        {
            this.Config = config;
            this.TotalHostiles = totalHostiles;
            this.Starbases = starbases;
            this.Stardate = stardate;
            this.TimeRemaining = timeRemaining;
        }

        public Write(IStarTrekKGSettings config)
        {
            this.Config = config;
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

        public void RenderQuadrantCounts(bool renderingMyLocation, int starbaseCount, int starCount, int hostileCount)
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



        public void CreateCommandPanel()
        {
            ACTIVITY_PANEL = new List<string>();

            ACTIVITY_PANEL.Add("nav = Navigation");
            ACTIVITY_PANEL.Add("srs = Short Range Scan");
            ACTIVITY_PANEL.Add("lrs = Long Range Scan");
            ACTIVITY_PANEL.Add("pha = Phaser Control");
            ACTIVITY_PANEL.Add("tor = Photon Torpedo Control");
            ACTIVITY_PANEL.Add("she = Shield Control");
            ACTIVITY_PANEL.Add("com = Access Computer");

            if(Constants.DEBUG_MODE)
            {
                ACTIVITY_PANEL.Add("");
                ACTIVITY_PANEL.Add("----------------------");
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
                case "nav":
                    Navigation.For(playerShip).Controls("nav");
                    break;

                case "srs":
                    ShortRangeScan.For(playerShip).Controls();
                    break;

                case "lrs":
                    LongRangeScan.For(playerShip).Controls();
                    break;

                case "pha": Phasers.For(playerShip).Controls(playerShip);
                    break;

                case "tor":
                    Torpedoes.For(playerShip).Controls();
                    break;

                case "she":

                    if (this.ShieldMenu(playerShip)) break;
                    break;

                case "com":

                    if (this.ComputerMenu(playerShip)) break;
                    break;

                case "dbg":
                    if (this.DebugMenu(playerShip)) break;
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

        private bool DebugMenu(Ship playerShip)
        {
            this.Strings(Debug.CONTROL_PANEL);
            this.WithNoEndCR("Enter Debug command: ");

            //todo: readline needs to be done using an event
            var debugCommand = Console.ReadLine().Trim().ToLower();

            Debug.For(playerShip).Controls(debugCommand);
            return false;
        }

        private bool ComputerMenu(Ship playerShip)
        {
            if (Computer.For(playerShip).Damaged()) return true;

            this.Strings(Computer.CONTROL_PANEL);
            this.WithNoEndCR("Enter computer command: ");

            //todo: readline needs to be done using an event
            var computerCommand = Console.ReadLine().Trim().ToLower();

            Computer.For(playerShip).Controls(computerCommand);
            return false;
        }

        private bool ShieldMenu(Ship playerShip)
        {
            if (Shields.For(playerShip).Damaged()) return true;

            Shields.SHIELD_PANEL = new List<string>();
            Shields.SHIELD_PANEL.Add(Environment.NewLine);

            var currentShieldEnergy = Shields.For(playerShip).Energy;

            if (currentShieldEnergy > 0)
            {
                Shields.SHIELD_PANEL.Add("--- > Shield Control: -- <CURRENTLY AT: " + currentShieldEnergy + "> --");
                Shields.SHIELD_PANEL.Add("add = Add energy to shields.");
                Shields.SHIELD_PANEL.Add("sub = Subtract energy from shields.");
            }
            else
            {
                Shields.SHIELD_PANEL.Add("--- > Shield Control: -- <DOWN> --");
                Shields.SHIELD_PANEL.Add("add = Add energy to shields.");
            }

            this.Strings(Shields.SHIELD_PANEL);

            this.WithNoEndCR("Enter shield control command: ");
            var shieldsCommand = Console.ReadLine().Trim().ToLower();

            Shields.For(playerShip).MaxTransfer = playerShip.Energy; //todo: this does nothing!
            Shields.For(playerShip).Controls(shieldsCommand);
            return false;
        }

        public string GetPanelHead(string shipName)
        {
            return "--- > " +  shipName + " < ---";
        }

        public bool PromptUser(string promptMessage, out double value)
        {
            try
            {
                this.WithNoEndCR(promptMessage);

                value = Double.Parse(this.Console.ReadLine());

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

    }
}


