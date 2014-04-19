
using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface IMap : IConfig
    {
        Ship Playership { get; set; } // todo: v2.0 will have a List<StarShip>().
        Regions Regions { get; set; }
        SetupOptions GameConfig { get; set; }
        int Stardate { get; set; }
        int timeRemaining { get; set; }
        int starbases { get; set; }
        string Text { get; set; }
        IOutputWrite Write { get; set; }

        int HostilesToSetUp { get; set; }

        void Initialize(SetupOptions setupOptions);
        void Initialize(SectorDefs sectorDefs, bool generateWithNebulae);
        void SetupPlayerShipInSectors(SectorDefs sectorDefs);
        void InitializeRegionsWithBaddies(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, SectorDefs sectorDefs, bool generateWithNebulae);
        void GenerateSquareGalaxy(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, List<Sector> itemsToPopulate, bool generateWithNebulae);
        IEnumerable<Sector> AddStarbases();
        IEnumerable<IShip> GetAllFederationShips();

        /// <summary>
        /// This will eventually be moved into each individual object
        /// </summary>
        void GetGlobalInfo();

        void SetUpPlayerShip(SectorDef playerShipDef);
        void SetupSubsystems();
        void GetSubsystemSetupFromConfig();
        void SetupPlayershipRegion(SectorDef playerShipDef);
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
        bool IsDockingLocation(int i, int j, Sectors sectors);

        SectorItem GetItem(int RegionX, int RegionY, int sectorX, int sectorY);
        Sector Get(int RegionX, int RegionY, int sectorX, int sectorY);
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

        bool OutOfBounds(Region region);
        void SetPlayershipInLocation(IMap map, Location newLocation);
    }
}
