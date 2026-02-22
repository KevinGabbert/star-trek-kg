namespace StarTrek_KG.Playfield
{
    public class DivinedCoordinateResult
    {
        public string Direction { get; set; }
        public int CurrentLocationX { get; set; }
        public int CurrentLocationY { get; set; }
        public Point SectorCoordinateToGet { get; set; }
        public Point CoordinatePointToGet { get; set; }

        public DivinedCoordinateResult Get(string direction,
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

            this.SectorCoordinateToGet = new Point(regionX, regionY);
            this.SectorCoordinateToGet = new Point(sectorX, sectorY);

            return this;
        }
    }
}
