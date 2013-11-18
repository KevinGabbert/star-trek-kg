using System;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Computer : SubSystem_Base, IMap
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
            Output.Write("The main computer has been repaired.");
        }
        public override void OutputRepairedMessage()
        {
            Output.ComputerDamageMessage();
        }
        public override void OutputMalfunctioningMessage()
        {
            Output.Write("The Main Computer is malfunctioning.");
        }

        public override void Controls(string command)
        {
            if (Damaged()) return;

            var starship = this.Map.Playership;

            switch (command.ToLower())
            {
                case "rec":
                    Output.PrintGalacticRecord(this.Map.Quadrants);
                    break;
                case "sta":

                    //todo: get a list of all baddie names in quadrant

                    Output.PrintCurrentStatus(this.Map, 
                                              this.Damage, 
                                              Shields.For(starship).Damage, 
                                              Navigation.For(starship).Damage,
                                              ShortRangeScan.For(starship).Damage,
                                              LongRangeScan.For(starship).Damage,
                                              Torpedoes.For(starship).Damage,
                                              Phasers.For(starship).Damage,
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
                    Output.Write("Invalid computer command.");
                    break;
            }

            this.TakeDamage();
        }

        public new static Computer For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Computer). Add a Friendly to your GameConfig"); //todo: make this a custom exception
            }

            return (Computer)ship.Subsystems.Single(s => s.Type == SubsystemType.Computer);
        }

        //public string GenerateControlPanel()
        //{
            //reflect over functions?
        //}
    }
}
