using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class DeuteriumCloud : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }

        public DeuteriumCloud(int amount)
        {
            this.Type = typeof(DeuteriumCloud);
            this.Name = "Deuterium Cloud";
            this.Amount = amount;
        }
    }
}
