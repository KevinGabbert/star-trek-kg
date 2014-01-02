using StarTrek_KG.Structs;

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