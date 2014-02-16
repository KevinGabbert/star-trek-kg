using System;
using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    public interface IQuadrant
    {
        QuadrantType Type { get; set; }
        string Name { get; set; }
        IMap Map { get; set; }
        Sectors Sectors { get; set; }
        bool Scanned { get; set; }
        bool Empty { get; set; }
        bool Active { get; set; } 

        int X { get; set; }
        int Y { get; set; }

        void Create(Stack<string> baddieNames, FactionName stockBaddieFaction, bool addStars = true, bool makeNebulae = false);
        void Create(IMap map, Stack<string> quadrantNames, Stack<string> baddieNames, FactionName stockBaddieFaction, out int nameIndex, bool addStars = true, bool makeNebulae = false);
        void Create(Stack<string> quadrantNames, Stack<String> baddieNames, FactionName stockBaddieFaction, Coordinate quadrantXY, out int nameIndex, IEnumerable<Sector> itemsToPopulate, bool addStars = true, bool isNebulae = false);

        void InitializeSectors(Quadrant quadrant,
            List<Sector> itemsToPopulate,
            Stack<string> baddieNames,
            FactionName stockBaddieFaction,
            bool addStars,
            bool makeNebulae = false);

        IEnumerable<Sector> AddStars(Quadrant quadrant, int totalStarsInQuadrant);
        Sector AddStar(Quadrant quadrant);
        string CreateStars(Quadrant quadrant, int totalStarsInQuadrant, SectorType starSectorType = SectorType.StarSystem);

        void PopulateMatchingItem(Quadrant quadrant, ICollection<Sector> itemsToPopulate, int x, int y,
            Stack<string> baddieNames, FactionName stockBaddieFaction);

        void AddSector(Quadrant quadrant, int x, int y, SectorItem itemToPopulate, Stack<string> baddieNames, FactionName stockBaddieFaction);
        void AddShip(IShip ship, ISector toSector);
        void RemoveShip(IShip ship);
        Ship CreateHostileShip(ISector position, Stack<string> listOfBaddies, FactionName stockBaddieFaction);
        void AddEmptySector(Quadrant quadrant, int x, int y);
        bool NoHostiles(List<Ship> hostiles);

        /// <summary>
        /// goes through each sector in this quadrant and counts hostiles
        /// </summary>
        /// <returns></returns>
        List<IShip> GetHostiles();

        /// <summary>
        /// goes through each sector in this quadrant and clears hostiles
        /// </summary>
        /// <returns></returns>
        void ClearHostiles();

        /// <summary>
        /// goes through each sector in this quadrant and clears item requested
        /// </summary>
        /// <returns></returns>
        void ClearSectorsWithItem(SectorItem item);

        int GetStarbaseCount();
        int GetStarCount();
        ISector GetSector(ICoordinate coordinate);
        string ToString();
        void Update(Location newLocation);
        Quadrant ToQuadrant();
    }
}
