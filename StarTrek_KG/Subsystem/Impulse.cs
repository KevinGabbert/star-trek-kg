using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Impulse : SubSystem_Base, IInteract
    {
        public Impulse(Ship shipConnectedTo, IGame game) : base(shipConnectedTo, game)
        {
        }

        public static Impulse For(IShip ship)
        {
            return (Impulse)SubSystem_Base.For(ship, SubsystemType.Impulse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override List<string> Controls(string command)
        {
            // paste in Navigation.ImpulseControls

            //base._myPanel = Shields.SHIELD_PANEL;

            //IInteraction promptWriter = this.Game.Interact;

            //this.Game.Interact.Output.Queue.Clear();

            ////now we know that the shield Panel command has been retrieved.
            //if (!this.Damaged())
            //{
            //    if (this.NotRecognized(command, promptWriter))
            //    {
            //        this.Game.Interact.Line("Shield command not recognized."); //todo: resource this
            //    }
            //    else
            //    {
            //        //todo: this needs to change to read this.Game.Write.SubscriberPromptSubCommand, and that var needs to be "add"
            //        if (this.AddingTo(command, promptWriter))
            //        {
            //            this.AddOrGetValue(command, promptWriter);
            //        }
            //        else if (this.SubtractingFrom(command, promptWriter))
            //        {
            //            this.SubtractOrGetValue(command, promptWriter);
            //        }
            //    }
            //}
            //else
            //{
            //    this.Game.Interact.Line("Shields are Damaged. DamageLevel: {this.Damage}"); //todo: resource this
            //}

            //return this.Game.Interact.Output.Queue.ToList();

            return null;
        }

        public void GetValueFromUser(string subCommand, IInteraction prompt)
        {
            //var promptWriter = this.ShipConnectedTo.Game.Interact;

            //if (promptWriter.Subscriber.PromptInfo.Level == 1)
            //{
            //    string transfer;
            //    promptWriter.PromptUser(SubsystemType.Shields,
            //                            "Shields-> Transfer Energy-> ",
            //                            $"Enter amount of energy (1--{this.MaxTransfer}) ", //todo: resource this
            //                            out transfer,
            //                            this.Game.Interact.Output.Queue,
            //                            2);

            //    //todo: this is a little difficult.  why do we need to glue these 2 guys together?
            //    //(grabs everything that .PromptUser output)
            //    //this.Game.Write.Output.Queue.Enqueue(promptWriter.Output.Queue.Dequeue());
            //}

            //promptWriter.Subscriber.PromptInfo.SubCommand = subCommand;
        }
    }
}
