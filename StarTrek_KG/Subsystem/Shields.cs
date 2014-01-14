using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
namespace StarTrek_KG.Subsystem
{
    public class Shields : SubSystem_Base
    {
        public static List<string> SHIELD_PANEL = new List<string>();

        public Shields(Ship shipConnectedTo, Game game)
        {
            this.Game = game;

            this.Initialize();

            this.ShipConnectedTo = shipConnectedTo;
            this.Type = SubsystemType.Shields;
            this.Damage = 0;
            this.Energy = 0;
        }

        public override void OutputDamagedMessage()
        {
            this.Game.Write.Line("Shield control is damaged. Repairs are underway.");
        }

        public override void OutputRepairedMessage()
        {
            this.Game.Write.Line("Shield control has been repaired.");
        }

        public override void OutputMalfunctioningMessage()
        {
            this.Game.Write.Line("The Shields are malfunctioning.");
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
                        this.Game.Write.Line("Shields are currently DOWN.  Cannot subtract energy");
                        goto EndControls;
                    }

                    break;

                default:
                    this.Game.Write.Line("Invalid this.Write");
                    return;
            }

            var transfer = this.TransferredFromUser();

            if (transfer > 0)
            {

                if (adding && (this.ShipConnectedTo.Energy - transfer) < 1)
                {
                    this.Game.Write.Line("Energy to transfer to shields cannot exceed Ship energy reserves. No Change");
                    goto EndControls;
                }

                var maxEnergy = (Convert.ToInt32((new StarTrekKGSettings()).GetSetting<string>("SHIELDS_MAX")));
                var totalEnergy = (this.Energy + transfer);

                if (adding && (totalEnergy > maxEnergy))
                {
                    if (adding && (totalEnergy > maxEnergy))
                    {
                        //todo: write code to add the difference if they exceed. There is no reason to make people type twice

                        this.Game.Write.Line("Energy to transfer exceeds Shield Max capability.. No Change");
                        goto EndControls;
                    }

                    if (totalEnergy <= maxEnergy || !adding)
                    {
                        this.AddEnergy(totalEnergy, adding); //todo: add limit on ship energy level 
                    } 
                }
                else
                {
                    this.AddEnergy(transfer, adding);
                }

                this.Game.Write.Line(string.Format("Shield strength is now {0}. Total Energy level is now {1}.",
                                                this.Energy,
                                                this.ShipConnectedTo.Energy));
            }

            EndControls:;
        }

        public new int TransferredFromUser()
        {
            double transfer;
            bool readSuccess = this.Game.Write.PromptUser(String.Format("Enter amount of energy (1--{0}): ", this.MaxTransfer),
                                                 out transfer);

            return EnergyValidation(transfer, readSuccess);
        }

        public int EnergyValidation(double transfer, bool readSuccess)
        {
            var tooLittle = transfer < 1;
            var tooMuch = transfer > this.MaxTransfer;

            if (tooLittle)
            {
                this.Game.Write.Line("Cannot Transfer < 1 unit of Energy");
                return 0;
            }

            if (tooMuch)
            {
                this.Game.Write.Line("Cannot Transfer. Too Much Energy");
                return 0;
            }

            if (!readSuccess)
            {
                //todo: tell the user if they are adding too much or too little energy
                this.Game.Write.Line("Invalid amount of energy.");
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
