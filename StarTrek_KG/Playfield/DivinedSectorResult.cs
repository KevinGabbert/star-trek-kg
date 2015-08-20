namespace StarTrek_KG.Playfield
{
    public class DivinedSectorResult
    {
        public string Direction { get; set; }
        public int CurrentLocationX { get; set; }
        public int CurrentLocationY { get; set; }
        public Coordinate RegionCoordinateToGet { get; set; }
        public Coordinate SectorCoordinateToGet { get; set; }

        public DivinedSectorResult Get(string direction,
                                        int regionX,
                                        int regionY,
                                        int sectorX,
                                        int sectorY,
                                        int currentX, 
                                        int currentY)
        {

            this.Direction = direction;
            this.CurrentLocationX = currentX;
            this.CurrentLocationY = currentY; 

            this.RegionCoordinateToGet = new Coordinate(regionX, regionY);
            this.SectorCoordinateToGet = new Coordinate(sectorX, sectorY);

            return this;
        }
    }
}
