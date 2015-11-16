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

        public DamageControl(IShip shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.DamageControl; //needed for lookup
        }

        public override List<string> Controls(string command)
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (command == Commands.DamageControl.FixSubsystem)
            {
                string subsystemToFix;
                this.ShipConnectedTo.Map.Game.Interact.PromptUserConsole(this.ShowSubsystemsToFix(), out subsystemToFix);

                this.EmergencyFix(SubsystemType.GetFromAbbreviation(subsystemToFix));
            }

            return this.ShipConnectedTo.Map.Game.Interact.Output.Queue.ToList();
        }

        private string ShowSubsystemsToFix()
        {
            this.ShipConnectedTo.Map.Game.Interact.CreateCommandPanel();
            this.ShipConnectedTo.Map.Game.Interact.Panel("─── Subsystem to Fix", this.ShipConnectedTo.Map.Game.Interact.SHIP_PANEL);
            this.ShipConnectedTo.Map.Game.Interact.WithNoEndCR("Enter subsystem: ");
            return "";
        }

        private void EmergencyFix(SubsystemType subsystem)
        {
            List<ISubsystem> subsystemsFound = this.ShipConnectedTo.Subsystems.Where(s => s.Type == subsystem).ToList();

            if (!subsystemsFound.Any())
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("invalid or no fix required.");
                return;
            }

            ISubsystem subsystemToFix = subsystemsFound.Single();  //yeah.  I'd rather say single() above, but oh well..

            IShip ship = this.ShipConnectedTo;

            if (ship.Energy > 1000) //todo: resource this out.
            {
                ship.Energy -= 1000;  //todo: resource this out.
                subsystemToFix.For(ship).FullRepair();
                this.ShipConnectedTo.Map.Game.Interact.Line($"Ship Energy now at: {ship.Energy}");
            }
            else
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("Not Enough Energy for Emergency Fix of " + subsystemToFix.Type);
            }
        }

        public static DamageControl For(IShip ship)
        {
            return (DamageControl)For(ship, SubsystemType.DamageControl);
        }
    }
}
