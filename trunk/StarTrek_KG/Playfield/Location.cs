using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class Location
    {
        public Region Region { get; set; }
        public ISector Sector { get; set; }

        public Location()
        {

        }

        public Location(Region Region, ISector sector)
        {
            this.Region = Region;
            this.Sector = sector;
        }
    }
}
