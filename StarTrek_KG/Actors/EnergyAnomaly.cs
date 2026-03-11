using System;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Actors
{
    public class EnergyAnomaly : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }
        public string Glyph { get; set; }
        public string EffectKey { get; set; }

        public EnergyAnomaly()
        {
            this.Type = this.GetType();
            this.Name = "Energy Anomaly";
            this.Glyph = "~";
            this.EffectKey = "~";
        }
    }
}
