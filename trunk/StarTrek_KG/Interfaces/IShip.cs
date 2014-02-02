using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG.Interfaces
{
    public interface IShip: ISectorObject
    {
        string Name { get; set; }
        double Energy { get; set; }

        Allegiance Allegiance { get; set; }
        bool Destroyed { get; set; }
        Subsystems Subsystems { get; set; }
        Coordinate Coordinate { get; set; }

        Quadrant GetQuadrant();
    }
}
