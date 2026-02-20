using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Actors
{
    public class System
    {
        public IShip ShipConnectedTo { get; set; }
        public int Energy { get; set; }
        public bool Destroyed { get; set; }
    }
}
