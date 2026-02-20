using StarTrek_KG.Structs;

namespace StarTrek_KG.Playfield
{
    public class LocationDef
    {
        public Point Sector { get; set; }
        public Point Coordinate { get; set; }

        public LocationDef()
        {

        }

        public LocationDef(NonNullable<Point> sector, NonNullable<Point> coordinate)
        {
            this.Sector = sector;
            this.Coordinate = coordinate;
        }

        public LocationDef(int sectorX, int sectorY, int coordinateX, int coordinateY)
        {
            var sector = new Point(sectorX, sectorY);
            var coordinate = new Point(coordinateX, coordinateY);

            this.Sector = sector;
            this.Coordinate = coordinate;
        }

        internal bool IsNeighbor(Location currentShipLocation)
        {
            throw new System.NotImplementedException();
        }
    }
}
