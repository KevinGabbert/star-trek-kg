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
        public CombinedRangeScan(Ship shipConnectedTo, Game game) : base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.CombinedRangeScan;
        }

        public List<string> Controls()
        {
            this.Game.Write.Output.OutputQueue.Clear();

            if (this.Damaged()) return this.Game.Write.Output.OutputQueue.ToList();

            if (ShortRangeScan.For(this.ShipConnectedTo).Damaged())
            {
                this.Game.Write.Line("Combined Scan needs SRS Subsystem in order to run.");
            }

            this.Game.Write.RenderSectors(SectorScanType.CombinedRange, this);

            return this.Game.Write.Output.OutputQueue.ToList();
        }

        public static CombinedRangeScan For(IShip ship)
        {
            return (CombinedRangeScan)SubSystem_Base.For(ship, SubsystemType.CombinedRangeScan);
        }
    }
}
