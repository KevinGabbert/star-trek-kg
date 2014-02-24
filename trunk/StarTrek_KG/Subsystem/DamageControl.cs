using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class DamageControl : SubSystem_Base
    {
        public static readonly string[] CONTROL_PANEL = {
                                                    "--- > Damage Control --------------",
                                                    "fix = Emergency Fix subsystem"
                                                };

        public DamageControl(Ship shipConnectedTo, Game game) : base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.DamageControl; //needed for lookup
        }

        public override void Controls(string command)
        {
            if (command == "fix")
            {
                string subsystemToFix;
                this.Game.Write.PromptUser(this.ShowSubsystemsToFix(), out subsystemToFix);

                this.EmergencyFix(SubsystemType.GetFromAbbreviation(subsystemToFix));
            }
        }

        public string ShowSubsystemsToFix()
        {
            this.Game.Write.CreateCommandPanel();
            this.Game.Write.Panel("--- > Subsystem to Fix", this.Game.Write.ACTIVITY_PANEL);
            this.Game.Write.WithNoEndCR("Enter subsystem: ");
            return "";
        }

        public void EmergencyFix(SubsystemType subsystem)
        {
            List<ISubsystem> subsystemsFound = this.ShipConnectedTo.Subsystems.Where(s => s.Type == subsystem).ToList();

            if (!subsystemsFound.Any())
            {
                this.Game.Write.Line("invalid or no fix required.");
                return;
            }

            var subsystemToFix = subsystemsFound.Single();  //yeah.  I'd rather say single() above, but oh well..

            var thisShip = this.ShipConnectedTo;

            if (thisShip.Energy > 1000) //todo: resource this out.
            {
                thisShip.Energy -= 1000;  //todo: resource this out.
                subsystemToFix.For(thisShip, this.Game).FullRepair();
                this.Game.Write.Line("Ship Energy now at: " + thisShip.Energy);
            }
            else
            {
                this.Game.Write.Line("Not Enough Energy for Emergency Fix of " + subsystemToFix.Type);
            }
        }

        public static DamageControl For(IShip ship)
        {
            return (DamageControl)SubSystem_Base.For(ship, SubsystemType.DamageControl);
        }
    }
}
