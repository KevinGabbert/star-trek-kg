using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class Location
    {
        public Sector Sector { get; set; }
        public ICoordinate Coordinate { get; set; }

        public Location()
        {

        }

        public Location(Sector Sector, ICoordinate coordinate)
        {
            this.Sector = Sector;
            this.Coordinate = coordinate;
        }
    }
}
