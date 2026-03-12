using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class TemporalRift : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }

        public TemporalRift()
        {
            this.Type = typeof(TemporalRift);
            this.Name = "Temporal Rift";
        }
    }
}
