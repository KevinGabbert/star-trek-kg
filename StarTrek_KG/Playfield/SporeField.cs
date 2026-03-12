using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class SporeField : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }

        public SporeField()
        {
            this.Type = typeof(SporeField);
            this.Name = "Spore Field";
        }
    }
}
