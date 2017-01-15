using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    class CombinedRangeScan : SubSystem_Base
    {
        public CombinedRangeScan(IShip shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.CombinedRangeScan;
        }

        public List<string> Controls()
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) return this.ShipConnectedTo.OutputQueue();

            if (ShortRangeScan.For(this.ShipConnectedTo).Damaged())
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("Combined Scan needs SRS Subsystem in order to run.");
            }

            this.ShipConnectedTo.Map.Game.Interact.RenderSectors(SectorScanType.CombinedRange, this);

            return this.ShipConnectedTo.OutputQueue();
        }

        public static CombinedRangeScan For(IShip ship)
        {
            return (CombinedRangeScan)For(ship, SubsystemType.CombinedRangeScan);
        }
    }
}
