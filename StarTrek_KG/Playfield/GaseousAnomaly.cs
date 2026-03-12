using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class GaseousAnomaly : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }

        public GaseousAnomaly()
        {
            this.Type = typeof(GaseousAnomaly);
            this.Name = "Gaseous Anomaly";
        }
    }
}
