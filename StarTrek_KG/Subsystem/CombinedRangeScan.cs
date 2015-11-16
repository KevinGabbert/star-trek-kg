using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    class CombinedRangeScan : SubSystem_Base
    {
        public CombinedRangeScan(Ship shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.CombinedRangeScan;
        }

        public List<string> Controls()
        {
            this.Prompt.Output.Queue.Clear();

            if (this.Damaged()) return this.Prompt.Output.Queue.ToList();

            if (ShortRangeScan.For(this.ShipConnectedTo).Damaged())
            {
                this.Prompt.Line("Combined Scan needs SRS Subsystem in order to run.");
            }

            this.Prompt.RenderSectors(SectorScanType.CombinedRange, this);

            return this.Prompt.Output.Queue.ToList();
        }

        public static CombinedRangeScan For(IShip ship)
        {
            return (CombinedRangeScan)For(ship, SubsystemType.CombinedRangeScan);
        }
    }
}
