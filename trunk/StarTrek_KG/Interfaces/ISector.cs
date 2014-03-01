using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface ISector: ICoordinate
    {
        SectorItem Item { get; set; }
        ISectorObject Object { get; set; }
        Coordinate QuadrantDef { get; set; }
        SectorType Type { get; set; }

        //void IncrementBy(VectorCoordinate coordinate);
    }
}
