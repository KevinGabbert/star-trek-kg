using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class Wormhole : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }
        public int PairId { get; set; }
        public Point DestinationSector { get; set; }

        public Wormhole()
        {
            this.Type = typeof(Wormhole);
            this.Name = "Wormhole";
        }
    }
}
