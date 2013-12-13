using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    //todo: make feature where opposing ships can hack into your computer (and you, theirs) if shields are down
    public class Computer : SubSystem_Base
    {
        public static readonly string[] CONTROL_PANEL = {
                                                    "--- > Main Computer --------------",
                                                    "rec = Cumulative Galactic Record",
                                                    "sta = Status Report",
                                                    "tor = Photon Torpedo Calculator",
                                                    "bas = Starbase Calculator",
                                                    "nav = Navigation Calculator"
                                                };

        public Computer(Map map)
        {
            this.Map = map;
            this.Type = SubsystemType.Computer;
            this.Damage = 0;
        }

        public override void OutputDamagedMessage()
        {
            Output.Output.WriteLine("The main computer has been Damaged.");
        }
        public override void OutputRepairedMessage()
        {
            Output.Output.WriteLine("The main computer has been repaired.");
        }
        public override void OutputMalfunctioningMessage()
        {
            Output.Output.WriteLine("The Main Computer is malfunctioning.");
        }

        public override void Controls(string command)
        {
            if (Damaged()) return;

            var starship = this.Map.Playership;

            switch (command.ToLower())
            {
                case "rec":
                    Output.Output.PrintGalacticRecord(this.Map.Quadrants);
                    break;
                case "sta":

                    //todo: get a list of all baddie names in quadrant

                    Output.Output.PrintCurrentStatus(this.Map, 
                                              this.Damage, 
                                              starship,
                                              this.Map.Playership.GetQuadrant());
                    break;
                case "tor":
                    Torpedoes.For(this.Map.Playership).Calculator(this.Map);
                    break;
                case "bas":
                    this.Map.StarbaseCalculator(); 
                    break;
                case "nav":
                    Navigation.For(this.Map.Playership).Calculator(this.Map);
                    break;
                default:
                    Output.Output.WriteLine("Invalid computer command.");
                    break;
            }
        }

        public new static Computer For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Computer).");
            }

            return (Computer)ship.Subsystems.Single(s => s.Type == SubsystemType.Computer); //todo: reflect the name and refactor this to ISubsystem
        }

        //public string GenerateControlPanel()
        //{
            //reflect over functions?
        //}
    }
}
