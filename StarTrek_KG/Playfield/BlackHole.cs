using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class BlackHole : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }

        public BlackHole()
        {
            this.Type = typeof(BlackHole);
            this.Name = "Black Hole";
        }
    }
}
