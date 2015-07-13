using StarTrek_KG.Structs;

namespace StarTrek_KG.Playfield
{
    public class LocationDef
    {
        public Coordinate Region { get; set; }
        public Coordinate Sector { get; set; }

        public LocationDef()
        {

        }

        public LocationDef(NonNullable<Coordinate> Region, NonNullable<Coordinate> sector)
        {
            this.Region = Region;
            this.Sector = sector;
        }

        public LocationDef(int RegionX, int RegionY, int sectorX, int sectorY)
        {
            var region = new Coordinate(RegionX, RegionY);
            var sector = new Coordinate(sectorX, sectorY);

            this.Region = region;
            this.Sector = sector;
        }

        internal bool IsNeighbor(Location currentShipLocation)
        {
            throw new System.NotImplementedException();
        }
    }
}