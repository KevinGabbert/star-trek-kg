using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface ISector
    {
        #region Properties

        SectorType Type { get; set; }
        IMap Map { get; set; }
        Coordinates Coordinates { get; set; }

        string Name { get; set; }
        bool Scanned { get; set; }
        bool Empty { get; set; }
        bool Active { get; set; } 

        int X { get; set; }
        int Y { get; set; }

        #endregion

        Point GetPoint();

        void Create(Stack<string> baddieNames, FactionName stockBaddieFaction, bool addStars = true, bool makeNebulae = false);
        void Create(IMap map, Stack<string> sectorNames, Stack<string> baddieNames, FactionName stockBaddieFaction, out int nameIndex, bool addStars = true, bool makeNebulae = false);
        void Create(Stack<string> sectorNames, Stack<string> baddieNames, FactionName stockBaddieFaction, Point sectorPoint, out int nameIndex, IEnumerable<Coordinate> itemsToPopulate, bool addStars = true, bool isNebulae = false);

        void InitializeCoordinates(Sector Sector,
            List<Coordinate> itemsToPopulate,
            Stack<string> baddieNames,
            FactionName stockBaddieFaction,
            bool addStars,
            bool makeNebulae = false);

        Coordinate AddStar(Sector Sector);
        IEnumerable<Coordinate> AddStars(Sector Sector, int totalStarsInRegion);

        string CreateStars(Sector Sector, int totalStarsInRegion, CoordinateType starSectorType = CoordinateType.StarSystem);

        void PopulateMatchingItem(Sector Sector, ICollection<Coordinate> itemsToPopulate, int x, int y,
            Stack<string> baddieNames, FactionName stockBaddieFaction, bool makeNebulae);

        void AddCoordinate(Sector Sector, int x, int y, CoordinateItem itemToPopulate, Stack<string> baddieNames, FactionName stockBaddieFaction);
        void AddShip(IShip ship, ICoordinate toCoordinate);
        void RemoveShip(IShip ship);

        Ship CreateHostileShip(ICoordinate position, Stack<string> listOfBaddies, FactionName stockBaddieFaction, IGame game);

        void AddEmptyCoordinate(Sector Sector, int x, int y);
        bool NoHostiles(List<Ship> hostiles);

        /// <summary>
        /// goes through each sector in this Sector and counts hostiles
        /// </summary>
        /// <returns></returns>
        List<IShip> GetHostiles();

        /// <summary>
        /// goes through each sector in this Sector and clears hostiles
        /// </summary>
        /// <returns></returns>
        void ClearHostiles();

        /// <summary>
        /// goes through each sector in this Sector and clears item requested
        /// </summary>
        /// <returns></returns>
        void ClearSectorsWithItem(CoordinateItem item);

        int GetStarbaseCount();
        int GetStarCount();
        ICoordinate GetCoordinate(IPoint coordinate);

        void Update(Location newLocation);

        string ToString();
        Sector ToSector();
    }
}
