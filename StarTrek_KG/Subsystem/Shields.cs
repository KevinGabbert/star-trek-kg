using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Security.Policy;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    /// <summary>
    /// 
    /// </summary>
    public class Shields : SubSystem_Base
    {
        /// <summary>
        /// This is the shield "menu" that the user will be using
        /// </summary>
        public static List<string> SHIELD_PANEL = new List<string>();

        public Shields(Ship shipConnectedTo, Game game): base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.Shields;

            int defaultDamageValue = 0; //todo: resource this (get from config)
            int defaultEnergyValue = 0;  //todo: resource this (get from config)

            this.Damage = defaultDamageValue;
            this.Energy = defaultEnergyValue;
        }

        public static Shields For(IShip ship)
        {
            return (Shields)SubSystem_Base.For(ship, SubsystemType.Shields);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override List<string> Controls(string command)
        {
            var promptWriter = this.ShipConnectedTo.Game.Write;

            this.Game.Write.Output.Queue.Clear();

            //now we know that the shield Panel command has been retrieved.
            if (!this.Damaged())
            {
                //todo: this needs to change to read this.Game.Write.SubscriberPromptSubCommand, and that var needs to be "add"
                if ((command == "add") || (promptWriter.SubscriberPromptSubCommand == "add"))
                {
                    if (command != "add")
                    {
                        this.DoTheTransfer(command);

                        promptWriter.ResetPrompt();
                    }
                    else
                    {
                        this.GetValueFromUser();
                        promptWriter.SubscriberPromptSubCommand = "add";
                    }   
                }
                else if ((command == "sub") || (promptWriter.SubscriberPromptSubCommand == "sub"))
                {
                    if (command != "sub")
                    {
                        if (this.Energy > 0)
                        {
                            this.MaxTransfer = this.Energy;
                            this.DoTheTransfer(command);

                            promptWriter.ResetPrompt();
                        }
                        else
                        {
                            this.Game.Write.Line("Shields are currently DOWN.  Cannot subtract energy");
                            //todo: resource this
                        }
                    }
                }
            }
            else
            {
                //todo: do we output something if damaged?
            }

            return this.Game.Write.Output.Queue.ToList();
        }

        private void DoTheTransfer(string command)
        {
            int transferAmount = Convert.ToInt32(command);

            this.EnergyValidation(Convert.ToInt32(transferAmount));

            this.TransferEnergy(transferAmount, true);
        }

        #region Transferring energy

        private void TransferEnergy(int transfer, bool adding)
        {
            if (this.ShipConnectedTo.GetRegion().Type == RegionType.Nebulae)
            {
                this.Game.Write.Line("Energy cannot be added to shields while in a nebula."); //todo: resource this
            }
            else
            {
                if (transfer > 0)
                {
                    if (adding && (this.ShipConnectedTo.Energy - transfer) < 1)
                    {
                        this.Game.Write.Line("Energy to transfer to shields cannot exceed Ship energy reserves. No Change");
                        return;
                    }

                    var maxEnergy = (Convert.ToInt32(this.Game.Config.GetSetting<string>("SHIELDS_MAX")));
                    var totalEnergy = (this.Energy + transfer);

                    if (adding && (totalEnergy > maxEnergy))
                    {
                        if (adding && (totalEnergy > maxEnergy))
                        {
                            //todo: write code to add the difference if they exceed. There is no reason to make people type twice

                            this.Game.Write.Line("Energy to transfer exceeds Shield Max capability.. No Change");
                            return;
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

                    this.Game.Write.Line(
                        $"Shield strength is now {this.Energy}. Total Energy level is now {this.ShipConnectedTo.Energy}.");

                    this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));
                }
            }
        }

        public void GetValueFromUser()
        {
            var promptWriter = this.ShipConnectedTo.Game.Write;

            if (promptWriter.SubscriberPromptLevel == 1)
            {
                string transfer;
                promptWriter.PromptUser(SubsystemType.Shields,
                    "Shields-> Transfer Energy-> ",
                    $"Enter amount of energy (1--{this.MaxTransfer}):> ", //todo: resource this
                    out transfer, 2);
            }
        }

        private int EnergyValidation(double transfer)
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

            //if (!readSuccess)
            //{
            //    //todo: tell the user if they are adding too much or too little energy
            //    this.Game.Write.Line("Invalid amount of energy.");
            //    return 0;
            //}

            return (int)transfer;
        }

        #endregion

        #region Automation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public bool AutoRaiseShieldsIfNeeded(IRegion region)
        {
            //todo: resource this out as a feature that is turned on by default

            //todo: later this feature will be an earnable upgrade. (there will need to be some upgrade configs for that)

            bool shieldsAutoRaised = false;
            if (region.GetHostiles().Count > 0)
            {
                shieldsAutoRaised = Game.Auto_Raise_Shields(this.ShipConnectedTo.Map, region);
            }

            return shieldsAutoRaised;
        }

        #endregion
    }
}
