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

        public Computer(Map map, Ship shipConnectedTo)
        {
            this.Map = map;
            this.ShipConnectedTo = shipConnectedTo;
            this.Type = SubsystemType.Computer;
            this.Damage = 0;
        }

        public override void OutputDamagedMessage()
        {
            Output.Write.Line("The main computer has been Damaged.");
        }
        public override void OutputRepairedMessage()
        {
            Output.Write.Line("The main computer has been repaired.");
        }
        public override void OutputMalfunctioningMessage()
        {
            Output.Write.Line("The Main Computer is malfunctioning.");
        }

        public override void Controls(string command)
        {
            if (Damaged()) return;

            var starship = this.ShipConnectedTo;

            switch (command.ToLower())
            {
                case "rec":
                    Output.Write.PrintGalacticRecord(this.Map.Quadrants);
                    break;
                case "sta":

                    //todo: get a list of all baddie names in quadrant

                    Output.Write.PrintCurrentStatus(this.Map, 
                                              this.Damage, 
                                              starship,
                                              this.ShipConnectedTo.GetQuadrant());
                    break;
                case "tor":
                    Torpedoes.For(this.ShipConnectedTo).Calculator(this.Map);
                    break;
                case "bas":
                    this.Map.StarbaseCalculator(this.ShipConnectedTo); 
                    break;
                case "nav":
                    Navigation.For(this.ShipConnectedTo).Calculator(this.Map);
                    break;
                default:
                    Output.Write.Line("Invalid computer command.");
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
