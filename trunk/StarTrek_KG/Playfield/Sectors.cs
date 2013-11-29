using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;

namespace StarTrek_KG.Playfield
{
    public class Sectors: List<Sector>
    {
        public static Sector Get(int x, int y, List<Sector> sectors)
        {
            var sectorFound = sectors.Where(si => si.X == x && si.Y == y).ToList();
            
            if (sectorFound.Count() == 0)
            {
                throw new GameException("Sector not found: X: " + x + " Y: " + y + " SectorCount: " + sectors.Count);
            }

            return sectorFound.Single();
        }

        public static Sector GetNoError(int x, int y, List<Sector> sectors)
        {
            var sectorFound = sectors.Where(si => si.X == x && si.Y == y).ToList();

            return sectorFound.Count() == 1 ? sectorFound.Single() : null;
        }

        public void Assign(int x, int y, SectorItem itemToAssign)
        {
            Sectors.Get(x, y, this).Item = itemToAssign;
        }

        public void ClearAllFriendlies()
        {
            var friendlySectors = this.Where(s => s.Item == SectorItem.Friendly);

            foreach (var sector in friendlySectors)
            {
                sector.Item = SectorItem.Empty;
            }
        }

        //todo: refactor this?
        public bool NotFound(int x, int y)
        {
            var notFound = this.Where(s => s.X == x && s.Y == y).Count() == 0;
            return notFound;
        }

        public static void SetupNewSector(SectorDef sectorDef, Sectors newSectors, Quadrants quadrants)
        {
            //come up with random Sector nuumber
            var randomSectorX = (Utility.Random).Next(Constants.SECTOR_MAX);
            var randomSectorY = (Utility.Random).Next(Constants.SECTOR_MAX);

            if (newSectors.NotFound(randomSectorX, randomSectorY))
            {
                var newSector = new Sector(new LocationDef(sectorDef.QuadrantDef, new Coordinate(sectorDef.Sector.X, sectorDef.Sector.Y)));
                newSector.Item = sectorDef.Item;

                if (sectorDef.QuadrantDef == null)
                {
                    var randomQuadrantX = (Utility.Random).Next(Constants.SECTOR_MAX);
                    var randomQuadrantY = (Utility.Random).Next(Constants.SECTOR_MAX);

                    if(quadrants.NotFound(randomQuadrantX, randomQuadrantY))
                    {
                        newSector.QuadrantDef = new Coordinate(randomQuadrantX, randomQuadrantY);
                    }
                }

                newSectors.Add(newSector);    
            }
            //else
            //{
            //    //throw new GameException("Can't set up sector at " + sectorDef.Sector.X + "," + sectorDef.Sector.Y + ". Sector already set up.");
            //}
        }
    }
}
