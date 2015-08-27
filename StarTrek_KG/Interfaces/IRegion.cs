using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface IRegion
    {
        #region Properties

        RegionType Type { get; set; }
        IMap Map { get; set; }
        Sectors Sectors { get; set; }

        string Name { get; set; }
        bool Scanned { get; set; }
        bool Empty { get; set; }
        bool Active { get; set; } 

        int X { get; set; }
        int Y { get; set; }

        #endregion

        Coordinate GetCoordinate();

        void Create(Stack<string> baddieNames, FactionName stockBaddieFaction, bool addStars = true, bool makeNebulae = false);
        void Create(IMap map, Stack<string> RegionNames, Stack<string> baddieNames, FactionName stockBaddieFaction, out int nameIndex, bool addStars = true, bool makeNebulae = false);
        void Create(Stack<string> RegionNames, Stack<string> baddieNames, FactionName stockBaddieFaction, Coordinate RegionXY, out int nameIndex, IEnumerable<Sector> itemsToPopulate, bool addStars = true, bool isNebulae = false);

        void InitializeSectors(Region Region,
            List<Sector> itemsToPopulate,
            Stack<string> baddieNames,
            FactionName stockBaddieFaction,
            bool addStars,
            bool makeNebulae = false);

        Sector AddStar(Region Region);
        IEnumerable<Sector> AddStars(Region Region, int totalStarsInRegion);

        string CreateStars(Region Region, int totalStarsInRegion, SectorType starSectorType = SectorType.StarSystem);

        void PopulateMatchingItem(Region Region, ICollection<Sector> itemsToPopulate, int x, int y,
            Stack<string> baddieNames, FactionName stockBaddieFaction, bool makeNebulae);

        void AddSector(Region Region, int x, int y, SectorItem itemToPopulate, Stack<string> baddieNames, FactionName stockBaddieFaction);
        void AddShip(IShip ship, ISector toSector);
        void RemoveShip(IShip ship);

        Ship CreateHostileShip(ISector position, Stack<string> listOfBaddies, FactionName stockBaddieFaction, Game game);

        void AddEmptySector(Region Region, int x, int y);
        bool NoHostiles(List<Ship> hostiles);

        /// <summary>
        /// goes through each sector in this Region and counts hostiles
        /// </summary>
        /// <returns></returns>
        List<IShip> GetHostiles();

        /// <summary>
        /// goes through each sector in this Region and clears hostiles
        /// </summary>
        /// <returns></returns>
        void ClearHostiles();

        /// <summary>
        /// goes through each sector in this Region and clears item requested
        /// </summary>
        /// <returns></returns>
        void ClearSectorsWithItem(SectorItem item);

        int GetStarbaseCount();
        int GetStarCount();
        ISector GetSector(ICoordinate coordinate);

        void Update(Location newLocation);

        string ToString();
        Region ToRegion();
    }
}
