using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface IShip
    {
        Sector Sector { get; }
        bool Destroyed { get; set; }
    }
}
