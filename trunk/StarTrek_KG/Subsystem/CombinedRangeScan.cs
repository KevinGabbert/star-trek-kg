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

        public void Controls()
        {
            if (this.Damaged()) return;

            if (ShortRangeScan.For(this.ShipConnectedTo).Damaged())
            {
                this.Game.Write.Line("Combined Scan needs SRS Subsystem in order to run.");
            }

            this.Game.Write.RenderSectors(SectorScanType.CombinedRange, this);
        }

        public static CombinedRangeScan For(IShip ship)
        {
            return (CombinedRangeScan)SubSystem_Base.For(ship, SubsystemType.CombinedRangeScan);
        }
    }
}
