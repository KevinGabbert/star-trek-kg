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

            this.Game.Write.RenderSector(SectorScanType.CombinedRange, this);
        }

        public static CombinedRangeScan For(IShip ship)
        {
            return (CombinedRangeScan)SubSystem_Base.For(ship, SubsystemType.CombinedRangeScan);
        }
    }
}
