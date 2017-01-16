using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    /// <summary>
    /// 
    /// </summary>
    public class Shields : SubSystem_Base, IInteract
    {
        /// <summary>
        /// This is the shield "menu" that the user will be using
        /// </summary>
        public static List<string> SHIELD_PANEL = new List<string>();

        public int MaxTransfer { get; set; } 

        public Shields(IShip shipConnectedTo): base(shipConnectedTo)
        {
            this.Type = SubsystemType.Shields;

            int defaultDamageValue = 0; //todo: resource this (get from config)
            int defaultEnergyValue = 0;  //todo: resource this (get from config)

            this.Damage = defaultDamageValue;
            this.Energy = defaultEnergyValue;
        }

        public new static Shields For(IShip ship)
        {
            return (Shields)For(ship, SubsystemType.Shields);
        }

        #region Commands

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override List<string> Controls(string command)
        {
            base._myPanel = Shields.SHIELD_PANEL;

            //now we know that the shield Panel command has been retrieved.
            if (!base.Damaged())
            {
                if (base.NotRecognized(command))
                {
                    this.ShipConnectedTo.OutputLine("Shield command not recognized."); //todo: resource this
                }
                else
                {
                    //todo: this needs to change to read this.Game.Write.SubscriberPromptSubCommand, and that var needs to be "add"
                    if (this.AddingTo(command))
                    {
                        this.AddOrGetValue(command);
                    }
                    else if (this.SubtractingFrom(command))
                    {
                        this.SubtractOrGetValue(command);
                    }
                }
            }
            else
            {
                this.ShipConnectedTo.OutputLine("Shields are Damaged. DamageLevel: {this.Damage}"); //todo: resource this
            }

            return this.ShipConnectedTo.OutputQueue();
        }

        private bool SubtractingFrom(string command)
        {
            return (command == "sub") || (this.ShipConnectedTo.GetSubCommand() == "sub");
        }
        private bool AddingTo(string command)
        {
            return (command == "add") || (this.ShipConnectedTo.GetSubCommand() == "add");
        }

        private void AddOrGetValue(string command)
        {
            string add = "add"; //todo: resource this

            if (command != add)
            {
                 this.TransferAndReset(command, true);
            }
            else
            {
                this.GetValueFromUser(add);
            }
        }
        private void SubtractOrGetValue(string command)
        {
            string subtract = "sub"; //todo: resource this

            if (command != subtract)
            {
                if (this.Energy > 0)
                {
                    this.MaxTransfer = this.Energy;
                    this.TransferAndReset(command, false);
                }
                else
                {
                    this.ShipConnectedTo.OutputLine("Shields are currently DOWN.  Cannot subtract energy"); //todo: resource this
                }
            }
            else
            {
                if (this.Energy < 1)
                {
                    this.ShipConnectedTo.OutputLine("Shields are currently DOWN. Cannot subtract energy. \r\n Exiting Panel."); //todo: resource this
                    this.ShipConnectedTo.ResetPrompt();
                }
                else
                {
                    this.GetValueFromUser(subtract);
                }
            }
        }

        #endregion

        #region Transferring energy

        private void TransferAndReset(string command, bool adding)
        {
            if (!string.IsNullOrWhiteSpace(command) && command.All(char.IsDigit))
            {
                int energyToTransfer = Convert.ToInt32(command);
                this.DoTheTransfer(energyToTransfer, adding);

                this.ShipConnectedTo.Map.Game.Interact.ResetPrompt();
            }
            else
            {
                this.ShipConnectedTo.OutputLine("Invalid transfer amount."); //todo: resource this
            }
        }

        private void DoTheTransfer(int transferAmount, bool adding)
        {
            int validatedAmountToTransfer = this.EnergyValidation(transferAmount);
            this.TransferEnergy(validatedAmountToTransfer, adding);
        }

        private void TransferEnergy(int transfer, bool adding)
        {
            if (this.ShipConnectedTo.GetRegion().Type == RegionType.Nebulae)
            {
                this.ShipConnectedTo.OutputLine("Energy cannot be added to shields while in a nebula."); //todo: resource this
            }
            else
            {
                if (transfer > 0)
                {
                    if (adding && this.ShipConnectedTo.Energy - transfer < 1)
                    {
                        this.ShipConnectedTo.OutputLine("Energy to transfer to shields cannot exceed Ship energy reserves. No Change");
                        return;
                    }

                    var maxEnergy = Convert.ToInt32(this.ShipConnectedTo.Map.Game.Config.GetSetting<string>("SHIELDS_MAX"));
                    var totalEnergy = this.Energy + transfer;

                    if (adding && (totalEnergy > maxEnergy))
                    {
                        if (adding && (totalEnergy > maxEnergy))
                        {
                            //todo: write code to add the difference if they exceed. There is no reason to make people type twice

                            this.ShipConnectedTo.OutputLine("Energy to transfer exceeds Shield Max capability.. No Change");
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

                    this.ShipConnectedTo.OutputLine($"Shield strength is now {this.Energy}. Total Energy level is now {this.ShipConnectedTo.Energy}.");

                    this.ShipConnectedTo.Map.Game.Interact.OutputConditionAndWarnings(this.ShipConnectedTo, this.ShipConnectedTo.Map.Game.Config.GetSetting<int>("ShieldsDownLevel"));
                }
            }
        }

        private int EnergyValidation(double transfer)
        {
            var prompt = this.ShipConnectedTo.Map.Game.Interact;
            bool tooLittle = transfer < 1;
            bool tooMuch = transfer > this.MaxTransfer;

            if (tooLittle)
            {
                prompt.Line("Cannot Transfer < 1 unit of Energy");
                return 0;
            }

            if (tooMuch)
            {
                prompt.Line("Cannot Transfer. Too Much Energy");
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

        //Interface
        public void GetValueFromUser(string subCommand)
        {
            PromptInfo promptInfo = this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo;

            if (promptInfo.Level == 1)
            {
                string transfer;
                this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Shields,
                                       "Shields-> Transfer Energy-> ",
                                       $"Enter amount of energy (1--{this.MaxTransfer}) ", //todo: resource this
                                       out transfer,
                                       this.ShipConnectedTo.Map.Game.Interact.Output.Queue,
                                       subPromptLevel: 2);
            }

            promptInfo.SubCommand = subCommand;
        }

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
                shieldsAutoRaised = this.ShipConnectedTo.Map.Game.Auto_Raise_Shields(this.ShipConnectedTo.Map, region);
            }

            return shieldsAutoRaised;
        }

        #endregion
    }
}
