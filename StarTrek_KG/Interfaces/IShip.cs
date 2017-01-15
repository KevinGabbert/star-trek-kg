using System.Collections.Generic;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface IShip: ISectorObject
    {
        IMap Map { get; set; }
        FactionName Faction { get; set; }
        Subsystems Subsystems { get; set; }
        Coordinate Coordinate { get; set; }
        Allegiance Allegiance { get; set; }

        int Energy { get; set; }
        bool Destroyed { get; set; }

        Region GetRegion();
        Location GetLocation();
        void UpdateDivinedSectors();
        void Scavenge(ScavengeType scavengeType);
        void RepairEverything();
        string GetConditionAndSetIcon();
        bool AtLowEnergyLevel();

        void OutputLine(string textToOutput);
        void ClearOutputQueue();
        List<string> OutputQueue();
    }
}
