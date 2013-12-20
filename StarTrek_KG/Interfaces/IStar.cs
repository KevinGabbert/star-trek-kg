
namespace StarTrek_KG.Interfaces
{
    public interface IStar: ISectorObject
    {
        string Name { get; }
        int Gravity { get; }
    }
}
