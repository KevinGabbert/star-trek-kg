using StarTrek_KG.Playfield;

namespace StarTrek_KG.Actors
{
    public class System
    {
        public Ship ShipConnectedTo { get; set; }
        public int Energy { get; set; }


        public bool Destroyed { get; set; }
        public Map Map { get; set; }
    }
}
