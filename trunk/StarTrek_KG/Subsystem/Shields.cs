using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Shields : SubSystem_Base
    {
        public static List<string> SHIELD_PANEL = new List<string>();

        public Shields(Map map, Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
            this.Map = map;
            this.Type = SubsystemType.Shields;
            this.Damage = 0;
            this.Energy = 0;
        }

        public override void OutputDamagedMessage()
        {
            Output.Write.Line("Shield control is damaged. Repairs are underway.");
        }

        public override void OutputRepairedMessage()
        {
            Output.Write.Line("Shield control has been repaired.");
        }

        public override void OutputMalfunctioningMessage()
        {
            Output.Write.Line("The Shields are malfunctioning.");
        }

        public override void Controls(string command)
        {
            if (this.Damaged()) return;

            bool adding = false;
            switch (command)
            {
                case "add":
                    adding = true;
                    break;

                case "sub":

                    if (this.Energy > 0)
                    {
                        this.MaxTransfer = this.Energy;
                    }
                    else
                    {
                        Output.Write.Line("Shields are currently DOWN.  Cannot subtract energy");
                        goto EndControls;
                    }

                    break;

                default:
                    Output.Write.Line("Invalid command.");
                    return;
            }

            var transfer = this.TransferredFromUser();

            if (transfer > 0)
            {
                this.AddEnergy(transfer, adding);

                Output.Write.Line(string.Format("Shield strength is now {0}. Total Energy level is now {1}.",
                                                this.Energy,
                                                this.ShipConnectedTo));
            }

            EndControls:
            ;
        }

        public new int TransferredFromUser()
        {
            double transfer;
            bool readSuccess = Command.PromptUser(String.Format("Enter amount of energy (1--{0}): ", this.MaxTransfer),
                                                 out transfer);

            return EnergyValidation(transfer, readSuccess);
        }

        public int EnergyValidation(double transfer, bool readSuccess)
        {
            var tooLittle = transfer < 1;
            var tooMuch = transfer > this.MaxTransfer;

            if (tooLittle)
            {
                Output.Write.Line("Cannot Transfer < 1 unit of Energy");
                return 0;
            }

            if (tooMuch)
            {
                Output.Write.Line("Cannot Transfer. Too Much Energy");
                return 0;
            }

            if (!readSuccess)
            {
                //todo: tell the user if they are adding too much or too little energy
                Output.Write.Line("Invalid amount of energy.");
                return 0;
            }

            return (int)transfer;
        }

        public static Shields For(IShip ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (shields). Add a Friendly to your GameConfig");
            }

            return (Shields) ship.Subsystems.Single(s => s.Type == SubsystemType.Shields);
        }
    }
}
