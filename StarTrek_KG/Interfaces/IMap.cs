using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    /// <summary>
    /// Implementers of this will be a map of some type
    /// </summary>
    public interface IMap : IConfig
    {
        IGame Game { get; set; }
        Ship Playership { get; set; } // todo: eventually make this an IEnumerable<StarShip>().
        Sectors Sectors { get; set; }
        SetupOptions GameConfig { get; set; }
        IInteraction Write { get; set; }

        int Stardate { get; set; }
        int timeRemaining { get; set; }
        int starbases { get; set; }
        string Text { get; set; }

        int HostilesToSetUp { get; set; }

        void Initialize(SetupOptions setupOptions);
        void Initialize(CoordinateDefs sectorDefs, bool generateWithNebulae);
        void SetupPlayerShipInSectors(CoordinateDefs sectorDefs);
        void InitializeSectorsWithBaddies(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, CoordinateDefs sectorDefs, bool generateWithNebulae);
        void GenerateSquareGalaxy(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, List<Coordinate> itemsToPopulate, bool generateWithNebulae);
        IEnumerable<Coordinate> AddStarbases();
        IEnumerable<IShip> GetAllFederationShips();

        /// <summary>
        /// This will eventually be moved into each individual object
        /// </summary>
        void GetGlobalInfo();

        void SetUpPlayerShip(CoordinateDef playerShipDef);
        void SetupSubsystems();
        void GetSubsystemSetupFromConfig();
        void SetupPlayershipSector(CoordinateDef playerShipDef);
        void SetupPlayershipTorpedoes();
        void SetupPlayershipShields();
        void SetupPlayershipNav();

        /// <summary>
        /// Legacy code. todo: needs to be rewritten.  Checks all sectors around starbase to see if its a good place to dock.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="sectors"></param>
        /// <returns></returns>
        bool IsDockingLocation(int i, int j, Coordinates sectors);

        CoordinateItem GetItem(int SectorX, int SectorY, int sectorX, int sectorY);
        Coordinate Get(int SectorX, int SectorY, int sectorX, int sectorY);
        void RemoveAllDestroyedShips(IMap map, IEnumerable<IShip> destroyedShips);
        void RemoveDestroyedShipsAndScavenge(List<IShip> destroyedShips);
        void RemoveTargetFromSector(IMap map, IShip ship);

        ///// <summary>
        ///// Removes all friendlies fromevery sector in the entire map.
        ///// </summary>
        ///// <param name="map"></param>
        //void RemoveAllFriendlies(IMap map);

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.  Sets down a friendly 
        /// </summary>
        /// <param name="map"></param>
        void SetPlayershipInActiveSector(IMap map);

        string ToString();

        void AddHostileFederationShipsToExistingMap();
        void AddACoupleHostileFederationShipsToExistingMap();

        bool OutOfBounds(Sector region);
        void SetPlayershipInLocation(IShip shipToSet, IMap map, Location newLocation);
    }
}
