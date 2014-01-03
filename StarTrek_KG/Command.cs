
using System;
using System.Collections.Generic;
using StarTrek_KG.Output;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Playfield;
using KGConsole = StarTrek_KG.Subsystem.Console;

namespace StarTrek_KG
{
    public class Command
    {
        public static List<string> ACTIVITY_PANEL = new List<string>();
                                                         
        #region Properties

            public Map Map { get; set; }

            //todo: make this non-static so we can test this class..

            private static KGConsole _console;

            public static KGConsole Console
            {
                get { return _console ?? (_console = new KGConsole()); }
                set { _console = value; }
            }


        #endregion

        public Command(Map map)
        {
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

            this.Map = map;
        }

        #region Event Handlers

        //public delegate void PromptUserHandler(object sender, EventArgs data);
        //public event PromptUserHandler PromptUser;

        //public void OnPromptUser(object sender, EventArgs data)
        //{
        //    // Check if there are any Subscribers
        //    if (PromptUser != null)
        //    {
        //        // Call the Event
        //        PromptUser(this, data);
        //    }
        //}

        //public delegate void OutputHandler(object sender, EventArgs data);
        //public event OutputHandler Output;

        //public void OnOutput(object sender, EventArgs data)
        //{
        //    // Check if there are any Subscribers
        //    if (Output != null)
        //    {
        //        Output(this, data);
        //    }
        //}

        #endregion

        //This needs to be a command method that the UI passes values into.
        //Readline is done in UI

        public void Prompt(string shipName)
        {
            Command.Console.Write(this.Map.Text);

            var readLine = Command.Console.ReadLine();
            if (readLine == null) return;

            var command = readLine.Trim().ToLower();
            switch (command)
            {
                case "nav": 
                    Navigation.For(this.Map.Playership).Controls("nav");
                    break;

                case "srs": 
                    ShortRangeScan.For(this.Map.Playership).Controls();
                    break;

                case "lrs": 
                    LongRangeScan.For(this.Map.Playership).Controls();
                    break;

                case "pha": Phasers.For(this.Map.Playership).Controls(this.Map.Playership);
                    break;

                case "tor": 
                    Torpedoes.For(this.Map.Playership).Controls();
                    break;

                case "she":

                    if (this.ShieldMenu()) break;
                    break;

                case "com":

                    if (this.ComputerMenu()) break;
                    break;

                case "dbg":
                    if (this.DebugMenu()) break;
                    break;

                default: //case "?":
                    Draw.Panel(this.GetPanelHead(shipName), ACTIVITY_PANEL);
                    break;
            }
        }

        private bool DebugMenu()
        {
            Output.Write.Strings(Debug.CONTROL_PANEL);
            Output.Write.WithNoEndCR("Enter Debug command: ");

            //todo: readline needs to be done using an event
            var debugCommand = Console.ReadLine().Trim().ToLower();

            Debug.For(this.Map.Playership).Controls(debugCommand);
            return false;
        }

        private bool ComputerMenu()
        {
            if (Computer.For(this.Map.Playership).Damaged()) return true;

            Output.Write.Strings(Computer.CONTROL_PANEL);
            Output.Write.WithNoEndCR("Enter computer command: ");

            //todo: readline needs to be done using an event
            var computerCommand = Console.ReadLine().Trim().ToLower();

            Computer.For(this.Map.Playership).Controls(computerCommand);
            return false;
        }

        private bool ShieldMenu()
        {
            if (Shields.For(this.Map.Playership).Damaged()) return true;

            Shields.SHIELD_PANEL = new List<string>();
            Shields.SHIELD_PANEL.Add(Environment.NewLine);

            var currentShieldEnergy = Shields.For(this.Map.Playership).Energy;

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

            Output.Write.Strings(Shields.SHIELD_PANEL);

            Output.Write.WithNoEndCR("Enter shield control command: ");
            var shieldsCommand = Console.ReadLine().Trim().ToLower();

            Shields.For(this.Map.Playership).MaxTransfer = this.Map.Playership.Energy; //todo: this does nothing!
            Shields.For(this.Map.Playership).Controls(shieldsCommand);
            return false;
        }

        public string GetPanelHead(string shipName)
        {
            return "--- > " +  shipName + " < ---";
        }

        public static bool PromptUser(string promptMessage, out double value)
        {
            try
            {
                Output.Write.WithNoEndCR(promptMessage);

                value = Double.Parse(Command.Console.ReadLine());

                return true;
            }
            catch
            {
                value = 0;
            }

            return false;
        }

        public static bool PromptUser(string promptMessage, out string value)
        {
            value = null;

            try
            {
                Command.Console.Write(promptMessage);

                var readLine = Command.Console.ReadLine();
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
