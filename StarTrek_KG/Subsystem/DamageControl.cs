using System;
using StarTrek_KG.Actors;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Subsystem
{
    public class DamageControl : SubSystem_Base
    {
        public DamageControl(Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
        }

        public void EmergencyFix(ISubsystem subsystem)
        {
            var thisShip = this.ShipConnectedTo;

            if (thisShip.Energy > 1000)
            {
                thisShip.Energy -= 1000;
                subsystem.For(thisShip, this.Game).FullRepair();
            }
            else
            {
                this.Game.Write.Line("Not Enough Energy for Emergency Fix of " + subsystem.Type);
            }
        }
    }
}
