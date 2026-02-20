using System.Collections.Generic;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface ICoordinate : IPoint
    {
        CoordinateItem Item { get; set; }
        ICoordinateObject Object { get; set; }
        Point SectorDef { get; set; }
        CoordinateType Type { get; set; }
        List<DivinedCoordinateItem> Neighbors { get; set; }

        bool Scanned { get; set; }

        Point GetPoint();
    }
}
