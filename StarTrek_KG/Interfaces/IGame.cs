using System.Collections.Generic;
using StarTrek_KG.Playfield;
using StarTrek_KG.Types;

namespace StarTrek_KG.Interfaces
{
    public interface IGame
    {
        IStarTrekKGSettings Config { get; set; }
        IInteraction Interact { get; set; }
        IMap Map { get; set; }
        int RandomFactorForTesting { get; set; }
        bool PlayerNowEnemyToFederation { get; set; }
        bool IsWarGamesMode { get; }
        List<FactionThreat> LatestTaunts { get; set; }
        Game._promptFunc<string, bool> Prompt { get; set; }

        void DestroyStarbase(IMap map, int newY, int newX, ICoordinate qLocation);
        void ALLHostilesAttack(IMap map);
        bool Auto_Raise_Shields(IMap map, ISector Sector);
        void MoveTimeForward(IMap map, Point lastSector, Point Sector);
        void ShowRandomTitle();
    }
}
