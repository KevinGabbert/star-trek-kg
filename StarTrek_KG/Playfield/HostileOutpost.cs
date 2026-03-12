using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class HostileOutpost : ICoordinateObject
    {
        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }
        public int HitPoints { get; set; }

        public HostileOutpost()
        {
            this.Type = typeof(HostileOutpost);
            this.Name = "Hostile Outpost";
            this.HitPoints = 3;
        }
    }
}
