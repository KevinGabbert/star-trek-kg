using System;
using StarTrek_KG.Exceptions;

namespace StarTrek_KG.Playfield
{
    public class LocationDef
    {
        public Coordinate Quadrant { get; set; }
        public Coordinate Sector { get; set; }

        public LocationDef()
        {

        }

        public LocationDef(NonNullable<Coordinate> quadrant, NonNullable<Coordinate> sector)
        {
            //if(quadrant == null)
            //{
            //    throw new GameException("Error during Initialization. Quadrant cannot be null");
            //}

            //if (sector == null)
            //{
            //    throw new GameException("Error during Initialization. Sector cannot be null");
            //}

            this.Quadrant = quadrant;
            this.Sector = sector;
        }

        public LocationDef(int quadrantX, int quadrantY, int sectorX, int sectorY)
        {
            var quadrant = new Coordinate(quadrantX, quadrantY);
            var sector = new Coordinate(sectorX, sectorY);

            this.Quadrant = quadrant;
            this.Sector = sector;
        }
    }
}