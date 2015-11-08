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
        List<FactionThreat> LatestTaunts { get; set; }
        Game._promptFunc<string, bool> Prompt { get; set; }

        void DestroyStarbase(IMap map, int newY, int newX, Sector qLocation);
        void ALLHostilesAttack(IMap map);
        bool Auto_Raise_Shields(IMap map, IRegion Region);
        void MoveTimeForward(IMap map, Coordinate lastRegion, Coordinate Region);

    }
}
