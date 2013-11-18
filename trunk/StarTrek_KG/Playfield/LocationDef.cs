namespace StarTrek_KG.Playfield
{
    public class LocationDef
    {
        public Coordinate Quadrant { get; set; }
        public Coordinate Sector { get; set; }

        public LocationDef()
        {

        }

        public LocationDef(Coordinate quadrant, Coordinate sector)
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
