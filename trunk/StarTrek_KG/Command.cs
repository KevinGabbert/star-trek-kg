
using System;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Playfield;

namespace StarTrek_KG
{
    public class Command
    {
        public static readonly string[] ACTIVITY_PANEL = {
                                                             "nav = Navigation",
                                                             "srs = Short Range Scan",
                                                             "lrs = Long Range Scan",
                                                             "pha = Phaser Control",
                                                             "tor = Photon Torpedo Control",
                                                             "she = Shield Control",
                                                             "com = Access Computer"
                                                         };
        #region Properties

            public Map Map { get; set; }

        #endregion

        public Command(Map map)
        {
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

                    if (Shields.For(this.Map.Playership).Damaged()) break;
                    Output.PrintStrings(Shields.CONTROL_PANEL);

                    Output.Prompt("Enter shield control command: ");
                    var shieldsCommand = Console.ReadLine().Trim().ToLower();

                    Shields.For(this.Map.Playership).MaxTransfer = this.Map.Playership.Energy; //todo: this does nothing!
                    Shields.For(this.Map.Playership).Controls(shieldsCommand);
                    break;
                case "com":

                    if (Computer.For(this.Map.Playership).Damaged()) break;

                    Output.PrintStrings(Computer.CONTROL_PANEL);
                    Output.Prompt("Enter computer command: ");

                    //todo: readline needs to be done using an event
                    var computerCommand = Console.ReadLine().Trim().ToLower();

                    Computer.For(this.Map.Playership).Controls(computerCommand);
                    break;

                default: //case "?":
                    Output.PrintPanel(this.GetPanelHead(shipName), ACTIVITY_PANEL);
                    break;
            }
        }

        public string GetPanelHead(string shipName)
        {
            return "--- > " +  shipName + " < ---";
        }

        public static bool PromptUser(string promptMessage, out double value)
        {
            try
            {
                Output.WriteSingleLine(promptMessage);

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
