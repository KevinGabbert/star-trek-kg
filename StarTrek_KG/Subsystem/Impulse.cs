using System.Collections.Generic;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    /// <summary>
    /// These are what facilitate movement.  if this breaks, movement wont work
    /// </summary>
    public class Impulse : SubSystem_Base, IInteract
    {
        public static List<string> IMPULSE_PANEL = new List<string>();
        public Impulse(IShip shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.Impulse;
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
            //TODO: Navigation doesn't know what system it is supposed to be using!
            Navigation.For(this.ShipConnectedTo).Controls(command);

            //if there are any lower menu levels, then do like shields

            return null;
        }

        public void GetValueFromUser(string subCommand)
        {
            //PromptInfo promptInfo = this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo;

            //if (promptInfo.Level == 1)
            //{
            //    string transfer;
            //    this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Impulse,
            //                                                      "Impulse-> Enter Course-> ",
            //                                                      $"{this.SystemPrompt.RenderCourse()} Enter Course: ", //todo: resource this
            //                                                      out transfer,
            //                                                      this.ShipConnectedTo.Map.Game.Interact.Output.Queue,
            //                                                      subPromptLevel: 2);
            //}

            //promptInfo.SubCommand = subCommand;
        }
    }
}
