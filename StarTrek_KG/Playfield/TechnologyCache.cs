using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class TechnologyCache : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }
        public int MaxEnergyBonus { get; set; }

        public TechnologyCache(int maxEnergyBonus)
        {
            this.Type = typeof(TechnologyCache);
            this.Name = "Technology Cache";
            this.MaxEnergyBonus = maxEnergyBonus;
        }
    }
}
