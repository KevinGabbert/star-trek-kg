using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class ShortRangeScan : SubSystem_Base
    {
        public ShortRangeScan(IShip shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.ShortRangeScan;
        }

        public List<string> Controls()
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) return null;

            this.ShipConnectedTo.Map.Game.Interact.RenderSectors(SectorScanType.ShortRange, this);
            return this.ShipConnectedTo.Map.Game.Interact.Output.Queue?.ToList();
        }

        public static ShortRangeScan For(IShip ship)
        {
            return (ShortRangeScan)For(ship, SubsystemType.ShortRangeScan);
        }

        public IEnumerable<Sector> ObjectFinder()
        {
            if (ShortRangeScan.For(this.ShipConnectedTo).Damaged())
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("Cannot locate Objects for calculations");
                return new List<Sector>(); //todo: is this correct?
            }

            var thisRegion = this.ShipConnectedTo.GetRegion();

            IEnumerable<Sector> sectorsWithObjects = thisRegion.Sectors.Where(s => s.Item != SectorItem.Empty);

            return sectorsWithObjects;
        }
    }
}
