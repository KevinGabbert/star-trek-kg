using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class Sectors: List<Sector>
    {
        public Sector Get(ICoordinate coordinate)
        {
            var gotSectors = this.Where(s => s.X == coordinate.X && s.Y == coordinate.Y).ToList();

            if (!gotSectors.Any())
            {
                throw new GameConfigException("Sector not found:  X: " + coordinate.X + " Y: " + coordinate.Y + " Total Sectors: " + " Total Sectors: " + gotSectors.Count());
            }

            if (gotSectors.Count() > 1)
            {
                throw new GameConfigException("Multiple sectors found. X: " + coordinate.X + " Y: " + coordinate.Y + " Total Sectors: " + gotSectors.Count());
            }

            //There can only be one active sector
            return gotSectors.Single();
        }

        public Sector GetNoError(ICoordinate coordinate)
        {
            var gotSectors = this.Where(s => s.X == coordinate.X && s.Y == coordinate.Y).ToList();

            //There can only be one active sector
            return gotSectors.SingleOrDefault();
        }

        public static Sector GetNoError(int x, int y, IEnumerable<Sector> sectors)
        {
            var sectorFound = sectors.Where(si => si.X == x && si.Y == y).ToList();

            return sectorFound.Count() == 1 ? sectorFound.Single() : null;
        }

        //todo: refactor this against Region.NotFound()
        private bool NotFound(ICoordinate coordinate)
        {
            var notFound = this.Count(s => s.X == coordinate.X && s.Y == coordinate.Y) == 0;
            return notFound;
        }

        public static void SetupNewSector(SectorDef sectorDef, Sectors newSectors, Regions Regions)
        {
            //todo: rewrite this function to get rid of GOTO

            StartOver:
            var randomSector = Coordinate.GetRandom();

            if (newSectors.NotFound(randomSector))
            {
                Sectors.SetupRandomRegionDef(sectorDef, Regions);

                var locationDef = new LocationDef(sectorDef.RegionDef, new Coordinate(sectorDef.Sector.X, sectorDef.Sector.Y));
                var newSector = new Sector(locationDef)
                {
                    Item = sectorDef.Item
                };

                newSectors.Add(newSector);    
            }
            else
            {
                //Console.WriteLine("Sector already Set up: " + sectorDef.Sector.X + "," + sectorDef.Sector.Y);
                goto StartOver;
            }
        }

        private static void SetupRandomRegionDef(SectorDef sectorDef, Regions Regions)
        {
            StartOverQ:
            if (sectorDef.RegionDef == null)
            {
                var randomRegion = Coordinate.GetRandom();

                if (Regions.NotFound(randomRegion))
                {
                    sectorDef.RegionDef = randomRegion;
                }
                else
                {
                    //we got a duplicate random number.  This Region is already set up.
                    goto StartOverQ; //todo: rewrite function to remove goto
                }
            }
        }
    }
}

    //public void Assign(int x, int y, SectorItem itemToAssign)
    //{
    //    Sectors.Get(x, y, this).Item = itemToAssign;
    //}

    //public void ClearAllFriendlies()
    //{
    //    var friendlySectors = this.Where(s => s.Item == SectorItem.PlayerShip);

    //    foreach (var sector in friendlySectors)
    //    {
    //        sector.Item = SectorItem.Empty;
    //    }
    //}

    //public static Sector Get(int x, int y, List<Sector> sectors)
    //{
    //    var sectorFound = sectors.Where(si => si.X == x && si.Y == y).ToList();
            
    //    if (!sectorFound.Any())
    //    {
    //        throw new GameException("Sector not found: X: " + x + " Y: " + y + " SectorCount: " + sectors.Count);
    //    }

    //    return sectorFound.Single();
    //}