using System.Collections.Generic;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface ISector: ICoordinate
    {
        SectorItem Item { get; set; }
        ISectorObject Object { get; set; }
        Coordinate RegionDef { get; set; }
        SectorType Type { get; set; }
        List<DivinedSectorItem> Neighbors { get; set; }

        bool Scanned { get; set; }

        Coordinate GetCoordinate();
    }
}
