using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Constants.Commands;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class DamageControl : SubSystem_Base
    {
        //todo: resource out this menu
        public static readonly string[] DAMAGE_PANEL = {
                                                    "--- > Damage Control --------------",
                                                    "fix = Emergency Fix subsystem"
                                                };

        public DamageControl(Ship shipConnectedTo, Game game) : base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.DamageControl; //needed for lookup
        }

        public override List<string> Controls(string command)
        {
            this.Game.Interact.Output.Queue.Clear();

            if (command == Commands.DamageControl.FixSubsystem)
            {
                string subsystemToFix;
                this.Game.Interact.PromptUserConsole(this.ShowSubsystemsToFix(), out subsystemToFix);

                this.EmergencyFix(SubsystemType.GetFromAbbreviation(subsystemToFix));
            }

            return this.Game.Interact.Output.Queue.ToList();
        }

        private string ShowSubsystemsToFix()
        {
            this.Game.Interact.CreateCommandPanel();
            this.Game.Interact.Panel("─── Subsystem to Fix", this.Game.Interact.SHIP_PANEL);
            this.Game.Interact.WithNoEndCR("Enter subsystem: ");
            return "";
        }

        private void EmergencyFix(SubsystemType subsystem)
        {
            List<ISubsystem> subsystemsFound = this.ShipConnectedTo.Subsystems.Where(s => s.Type == subsystem).ToList();

            if (!subsystemsFound.Any())
            {
                this.Game.Interact.Line("invalid or no fix required.");
                return;
            }

            var subsystemToFix = subsystemsFound.Single();  //yeah.  I'd rather say single() above, but oh well..

            var thisShip = this.ShipConnectedTo;

            if (thisShip.Energy > 1000) //todo: resource this out.
            {
                thisShip.Energy -= 1000;  //todo: resource this out.
                subsystemToFix.For(thisShip, this.Game).FullRepair();
                this.Game.Interact.Line("Ship Energy now at: " + thisShip.Energy);
            }
            else
            {
                this.Game.Interact.Line("Not Enough Energy for Emergency Fix of " + subsystemToFix.Type);
            }
        }

        public static DamageControl For(IShip ship)
        {
            return (DamageControl)SubSystem_Base.For(ship, SubsystemType.DamageControl);
        }
    }
}
