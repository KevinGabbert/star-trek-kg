using System;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Playfield
{
    public class Deuterium : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }

        public Deuterium(int amount)
        {
            this.Type = typeof(Deuterium);
            this.Name = "Deuterium";
            this.Amount = amount;
        }
    }
}
