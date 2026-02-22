using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class Coordinates: List<Coordinate>
    {
        public Coordinate this[int x, int y] => this.Get(new Point(x, y));
        public Coordinate this[Point sector] => this.Get(sector);

        public Coordinate Get(IPoint coordinate)
        {
            List<Coordinate> gotSectors = this.Where(s => s.X == coordinate.X && s.Y == coordinate.Y).ToList();

            if (!gotSectors.Any())
            {
                throw new GameConfigException("Coordinate not found:  X: " + coordinate.X + " Y: " + coordinate.Y + " Total Coordinates: " + " Total Coordinates: " + gotSectors.Count);
            }

            if (gotSectors.Count > 1)
            {
                throw new GameConfigException("Multiple sectors found. X: " + coordinate.X + " Y: " + coordinate.Y + " Total Coordinates: " + gotSectors.Count);
            }

            //There can only be one active sector
            return gotSectors.Single();
        }

        public Coordinate GetNoError(IPoint coordinate)
        {
            return this.SingleOrDefault(s => s.X == coordinate.X && s.Y == coordinate.Y); //There can only be one active sector
        }

        public static Coordinate GetNoError(int x, int y, IEnumerable<Coordinate> sectors)
        {
            List<Coordinate> sectorFound = sectors.Where(si => si.X == x && si.Y == y).ToList();

            return sectorFound.Count == 1 ? sectorFound.SingleOrDefault() : null;
        }

        //todo: refactor this against Sector.NotFound()
        private bool NotFound(IPoint coordinate)
        {
            return !this.Any(s => s.X == coordinate.X && s.Y == coordinate.Y);
        }

        public static void SetupNewSector(CoordinateDef sectorDef, Coordinates newSectors, Sectors Sectors)
        {
            //todo: rewrite this function to get rid of GOTO

            StartOver:
            var randomSector = Point.GetRandom();

            if (newSectors.NotFound(randomSector))
            {
                Coordinates.SetupRandomSectorDef(sectorDef, Sectors);

                LocationDef locationDef = new LocationDef(sectorDef.SectorDef, new Point(sectorDef.Coordinate.X, sectorDef.Coordinate.Y));
                var newSector = new Coordinate(locationDef)
                {
                    Item = sectorDef.Item
                };

                newSectors.Add(newSector);    
            }
            else
            {
                //Console.WriteLine("Coordinate already Set up: " + sectorDef.Coordinate.X + "," + sectorDef.Coordinate.Y);
                goto StartOver;
            }
        }

        private static void SetupRandomSectorDef(CoordinateDef sectorDef, Sectors Sectors)
        {
            StartOverQ:
            if (sectorDef.SectorDef == null)
            {
                var randomSector = Point.GetRandom();

                if (Sectors.NotFound(randomSector))
                {
                    sectorDef.SectorDef = randomSector;
                }
                else
                {
                    //we got a duplicate random number.  This Sector is already set up.
                    goto StartOverQ; //todo: rewrite function to remove goto
                }
            }
        }
    }
}

    //public void Assign(int x, int y, CoordinateItem itemToAssign)
    //{
    //    Coordinates.Get(x, y, this).Item = itemToAssign;
    //}

    //public void ClearAllFriendlies()
    //{
    //    var friendlySectors = this.Where(s => s.Item == CoordinateItem.PlayerShip);

    //    foreach (var sector in friendlySectors)
    //    {
    //        sector.Item = CoordinateItem.Empty;
    //    }
    //}

    //public static Coordinate Get(int x, int y, List<Coordinate> sectors)
    //{
    //    var sectorFound = sectors.Where(si => si.X == x && si.Y == y).ToList();
            
    //    if (!sectorFound.Any())
    //    {
    //        throw new GameException("Coordinate not found: X: " + x + " Y: " + y + " SectorCount: " + sectors.Count);
    //    }

    //    return sectorFound.Single();
    //}
