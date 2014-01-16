using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class Location
    {
        public Quadrant Quadrant { get; set; }
        public ISector Sector { get; set; }

        public Location()
        {

        }

        public Location(Quadrant quadrant, Sector sector)
        {
            this.Quadrant = quadrant;
            this.Sector = sector;
        }
    }
}
