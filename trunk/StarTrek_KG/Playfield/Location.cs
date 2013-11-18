﻿
namespace StarTrek_KG.Playfield
{
    public class Location
    {
        public Quadrant Quadrant { get; set; }
        public Sector Sector { get; set; }

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
