using System.Collections.Generic;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface IShip: ICoordinateObject, IShipUI
    {
        IMap Map { get; set; }
        FactionName Faction { get; set; }
        Subsystems Subsystems { get; set; }
        Point Point { get; set; }
        Allegiance Allegiance { get; set; }
        INavigationSubsystem NavigationSubsystem { get; }


        int Energy { get; set; }
        bool Destroyed { get; set; }

        Sector GetSector();
        Location GetLocation();
        void UpdateDivinedSectors();
        void Scavenge(ScavengeType scavengeType);
        void RepairEverything();
        string GetConditionAndSetIcon();
        bool AtLowEnergyLevel();
    }
}
