
using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Playfield;

namespace StarTrek_KG
{
    public class Command
    {
        public static List<string> ACTIVITY_PANEL = new List<string>();
                                                         
        #region Properties

            public Map Map { get; set; }

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
            Console.Write(this.Map.Text);

            var readLine = Console.ReadLine();
            if (readLine == null) return;

            var command = readLine.Trim().ToLower();
            switch (command)
            {
                case "nav": 
                    Navigation.For(this.Map.Playership).Controls("nav");
                    break;

                case "srs": 
                    ShortRangeScan.For(this.Map.Playership).Controls(this.Map);
                    break;

                case "lrs": 
                    LongRangeScan.For(this.Map.Playership).Controls(this.Map);
                    break;

                case "pha": Phasers.For(this.Map.Playership).Controls(this.Map, this.Map.Playership);
                    break;

                case "tor": 
                    Torpedoes.For(this.Map.Playership).Controls(this.Map);
                    break;

                case "she":

                    if (ShieldMenu()) break;
                    break;

                case "com":

                    if (ComputerMenu()) break;
                    break;

                case "dbg":
                    if (DebugMenu()) break;
                    break;

                default: //case "?":
                    Output.Write.Panel(this.GetPanelHead(shipName), ACTIVITY_PANEL);
                    break;
            }
        }

        private bool DebugMenu()
        {
            if (Computer.For(this.Map.Playership).Damaged()) return true;

            Output.Write.Strings(Debug.CONTROL_PANEL);
            Output.Write.Prompt("Enter Debug command: ");

            //todo: readline needs to be done using an event
            var debugCommand = Console.ReadLine().Trim().ToLower();

            Debug.For(this.Map.Playership).Controls(debugCommand);
            return false;
        }

        private bool ComputerMenu()
        {
            if (Computer.For(this.Map.Playership).Damaged()) return true;

            Output.Write.Strings(Computer.CONTROL_PANEL);
            Output.Write.Prompt("Enter computer command: ");

            //todo: readline needs to be done using an event
            var computerCommand = Console.ReadLine().Trim().ToLower();

            Computer.For(this.Map.Playership).Controls(computerCommand);
            return false;
        }

        private bool ShieldMenu()
        {
            if (Shields.For(this.Map.Playership).Damaged()) return true;
            Output.Write.Strings(Shields.CONTROL_PANEL);

            Output.Write.Prompt("Enter shield control command: ");
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
                Output.Write.SingleLine(promptMessage);

                value = Double.Parse(Console.ReadLine());

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
                Console.Write(promptMessage);

                var readLine = Console.ReadLine();
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
