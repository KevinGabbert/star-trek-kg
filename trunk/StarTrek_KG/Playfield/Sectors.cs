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
            
            if (!sectorFound.Any())
            {
                throw new GameException("Sector not found: X: " + x + " Y: " + y + " SectorCount: " + sectors.Count);
            }

            return sectorFound.Single();
        }

        public Sector Get(Coordinate coordinate)
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

        //todo: refactor this against Quadrant.NotFound()

        public bool NotFound(Coordinate coordinate)
        {
            var notFound = this.Count(s => s.X == coordinate.X && s.Y == coordinate.Y) == 0;
            return notFound;
        }

        public static void SetupNewSector(SectorDef sectorDef, Sectors newSectors, Quadrants quadrants)
        {
            //todo: rewrite this function to get rid of GOTO

            StartOver:
            var randomSector = Coordinate.GetRandom();

            if (newSectors.NotFound(randomSector))
            {
                Sectors.SetupRandomQuadrantDef(sectorDef, quadrants);

                var locationDef = new LocationDef(sectorDef.QuadrantDef, new Coordinate(sectorDef.Sector.X, sectorDef.Sector.Y));
                var newSector = new Sector(locationDef);
                newSector.Item = sectorDef.Item;

                newSectors.Add(newSector);    
            }
            else
            {
                //Console.WriteLine("Sector already Set up: " + sectorDef.Sector.X + "," + sectorDef.Sector.Y);
                goto StartOver;
            }
        }

        private static void SetupRandomQuadrantDef(SectorDef sectorDef, Quadrants quadrants)
        {
            StartOverQ:
            if (sectorDef.QuadrantDef == null)
            {
                var randomQuadrant = Coordinate.GetRandom();

                if (quadrants.NotFound(randomQuadrant))
                {
                    sectorDef.QuadrantDef = randomQuadrant;
                }
                else
                {
                    //we got a duplicate random number.  This quadrant is already set up.
                    goto StartOverQ; //todo: rewrite function to remove goto
                }
            }
        }
    }
}
