
namespace StarTrek_KG.Interfaces
{
    public interface IStar: ISectorObject
    {
        new string Name { get; }
        int Gravity { get; }
    }
}
