using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class GraviticMine : ICoordinateObject
    {
        public GraviticMine()
        {
            this.Type = typeof(GraviticMine);
            this.Name = "Gravitic Mine";
        }

        public ICoordinate Coordinate { get; set; }
        public System.Type Type { get; set; }
        public string Name { get; set; }
    }
}
