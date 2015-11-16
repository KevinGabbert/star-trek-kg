using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
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

        public Shields(Ship shipConnectedTo): base(shipConnectedTo)
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

        #region Commands

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override List<string> Controls(string command)
        {
            base._myPanel = Shields.SHIELD_PANEL;

            this.Prompt.Output.Queue.Clear();

            //now we know that the shield Panel command has been retrieved.
            if (!this.Damaged())
            {
                if (this.NotRecognized(command, this.Prompt))
                {
                    this.Prompt.Line("Shield command not recognized."); //todo: resource this
                }
                else
                {
                    //todo: this needs to change to read this.Game.Write.SubscriberPromptSubCommand, and that var needs to be "add"
                    if (this.AddingTo(command, this.Prompt))
                    {
                        this.AddOrGetValue(command, this.Prompt);
                    }
                    else if (this.SubtractingFrom(command, this.Prompt))
                    {
                        this.SubtractOrGetValue(command, this.Prompt);
                    }
                }
            }
            else
            {
                this.Prompt.Line("Shields are Damaged. DamageLevel: {this.Damage}"); //todo: resource this
            }

            return this.Prompt.Output.Queue.ToList();
        }

        private bool SubtractingFrom(string command, IInteraction promptInteraction)
        {
            return (command == "sub") || (promptInteraction.Subscriber.PromptInfo.SubCommand == "sub");
        }
        private bool AddingTo(string command, IInteraction promptInteraction)
        {
            return (command == "add") || (promptInteraction.Subscriber.PromptInfo.SubCommand == "add");
        }

        private void AddOrGetValue(string command, IInteraction prompt)
        {
            string add = "add"; //todo: resource this

            if (command != add)
            {
                 this.TransferAndReset(command, prompt, true);
            }
            else
            {
                this.GetValueFromUser(add, prompt);
            }
        }
        private void SubtractOrGetValue(string command, IInteraction prompt)
        {
            string subtract = "sub"; //todo: resource this

            if (command != subtract)
            {
                if (this.Energy > 0)
                {
                    this.MaxTransfer = this.Energy;
                    this.TransferAndReset(command, prompt, false);
                }
                else
                {
                    prompt.Line("Shields are currently DOWN.  Cannot subtract energy"); //todo: resource this
                }
            }
            else
            {
                if (this.Energy < 1)
                {
                    prompt.Line("Shields are currently DOWN. Cannot subtract energy. \r\n Exiting Panel."); //todo: resource this
                    prompt.ResetPrompt();
                }
                else
                {
                    this.GetValueFromUser(subtract, prompt);
                }
            }
        }

        #endregion

        #region Transferring energy

        private void TransferAndReset(string command, IInteraction prompt, bool adding)
        {
            if ((!string.IsNullOrWhiteSpace(command)) && command.All(char.IsDigit))
            {
                int energyToTransfer = Convert.ToInt32(command);
                this.DoTheTransfer(energyToTransfer, adding);

                prompt.ResetPrompt();
            }
            else
            {
                prompt.Line("Invalid transfer amount."); //todo: resource this
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
                this.Prompt.Line("Energy cannot be added to shields while in a nebula."); //todo: resource this
            }
            else
            {
                if (transfer > 0)
                {
                    if (adding && (this.ShipConnectedTo.Energy - transfer) < 1)
                    {
                        this.Prompt.Line("Energy to transfer to shields cannot exceed Ship energy reserves. No Change");
                        return;
                    }

                    var maxEnergy = (Convert.ToInt32(this.ShipConnectedTo.Game.Config.GetSetting<string>("SHIELDS_MAX")));
                    var totalEnergy = (this.Energy + transfer);

                    if (adding && (totalEnergy > maxEnergy))
                    {
                        if (adding && (totalEnergy > maxEnergy))
                        {
                            //todo: write code to add the difference if they exceed. There is no reason to make people type twice

                            this.Prompt.Line("Energy to transfer exceeds Shield Max capability.. No Change");
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

                    this.Prompt.Line($"Shield strength is now {this.Energy}. Total Energy level is now {this.ShipConnectedTo.Energy}.");

                    this.Prompt.OutputConditionAndWarnings(this.ShipConnectedTo, this.ShipConnectedTo.Game.Config.GetSetting<int>("ShieldsDownLevel"));
                }
            }
        }

        private int EnergyValidation(double transfer)
        {
            var prompt = this.ShipConnectedTo.Game.Interact;
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
        public void GetValueFromUser(string subCommand, IInteraction prompt)
        {
            //var promptWriter = this.ShipConnectedTo.Game.Interact;

            if (prompt.Subscriber.PromptInfo.Level == 1)
            {
                string transfer;
                prompt.PromptUser(SubsystemType.Shields,
                                        "Shields-> Transfer Energy-> ",
                                        $"Enter amount of energy (1--{this.MaxTransfer}) ", //todo: resource this
                                        out transfer,
                                        prompt.Output.Queue,
                                        2);

                //todo: this is a little difficult.  why do we need to glue these 2 guys together?
                //(grabs everything that .PromptUser output)
                //this.Game.Write.Output.Queue.Enqueue(promptWriter.Output.Queue.Dequeue());
            }

            prompt.Subscriber.PromptInfo.SubCommand = subCommand;
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
                shieldsAutoRaised = this.ShipConnectedTo.Game.Auto_Raise_Shields(this.ShipConnectedTo.Map, region);
            }

            return shieldsAutoRaised;
        }

        #endregion
    }
}
