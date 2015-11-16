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

        public DamageControl(Ship shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.DamageControl; //needed for lookup
        }

        public override List<string> Controls(string command)
        {
            this.Prompt.Output.Queue.Clear();

            if (command == Commands.DamageControl.FixSubsystem)
            {
                string subsystemToFix;
                this.Prompt.PromptUserConsole(this.ShowSubsystemsToFix(), out subsystemToFix);

                this.EmergencyFix(SubsystemType.GetFromAbbreviation(subsystemToFix));
            }

            return this.Prompt.Output.Queue.ToList();
        }

        private string ShowSubsystemsToFix()
        {
            this.Prompt.CreateCommandPanel();
            this.Prompt.Panel("─── Subsystem to Fix", this.Prompt.SHIP_PANEL);
            this.Prompt.WithNoEndCR("Enter subsystem: ");
            return "";
        }

        private void EmergencyFix(SubsystemType subsystem)
        {
            List<ISubsystem> subsystemsFound = this.ShipConnectedTo.Subsystems.Where(s => s.Type == subsystem).ToList();

            if (!subsystemsFound.Any())
            {
                this.Prompt.Line("invalid or no fix required.");
                return;
            }

            var subsystemToFix = subsystemsFound.Single();  //yeah.  I'd rather say single() above, but oh well..

            var thisShip = this.ShipConnectedTo;

            if (thisShip.Energy > 1000) //todo: resource this out.
            {
                thisShip.Energy -= 1000;  //todo: resource this out.
                subsystemToFix.For(thisShip, this.ShipConnectedTo.Game).FullRepair();
                this.Prompt.Line($"Ship Energy now at: {thisShip.Energy}");
            }
            else
            {
                this.Prompt.Line("Not Enough Energy for Emergency Fix of " + subsystemToFix.Type);
            }
        }

        public static DamageControl For(IShip ship)
        {
            return (DamageControl)For(ship, SubsystemType.DamageControl);
        }
    }
}
