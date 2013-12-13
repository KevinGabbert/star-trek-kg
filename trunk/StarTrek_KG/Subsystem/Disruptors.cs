using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Subsystem
{
    public class Disruptors : SubSystem_Base, IMap, IWeapon
    {
        public override void OutputDamagedMessage()
        {
            throw new NotImplementedException();
        }

        public override void OutputRepairedMessage()
        {
            throw new NotImplementedException();
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }
    }
}
