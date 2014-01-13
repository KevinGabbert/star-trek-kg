namespace StarTrek_KG.Actors
{
    public class System
    {
        public Ship ShipConnectedTo { get; set; }
        public int Energy { get; set; }
        public bool Destroyed { get; set; }
        public Game Game { get; set; }
    }
}
