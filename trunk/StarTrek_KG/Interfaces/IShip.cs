using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface IShip: ISectorObject
    {
        FactionName Faction { get; set; }
        int Energy { get; set; }
        Allegiance Allegiance { get; set; }
        bool Destroyed { get; set; }
        Subsystems Subsystems { get; set; }
        Coordinate Coordinate { get; set; }

        Region GetRegion();
    }
}
