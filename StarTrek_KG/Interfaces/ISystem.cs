using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface ISystem
    {
        int Energy { get; set; }
        bool Destroyed { get; set; }
        Map Map { get; set; }
    }
}
