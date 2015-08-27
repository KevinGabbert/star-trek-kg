using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface IShip: ISectorObject
    {
        Game Game { get; set; }
        FactionName Faction { get; set; }
        Subsystems Subsystems { get; set; }
        Coordinate Coordinate { get; set; }
        Allegiance Allegiance { get; set; }

        int Energy { get; set; }
        bool Destroyed { get; set; }

        Region GetRegion();
    }
}
