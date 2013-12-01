using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface ISystem
    {
        double Energy { get; set; }
        bool Destroyed { get; set; }
        Map Map { get; set; }
    }
}
