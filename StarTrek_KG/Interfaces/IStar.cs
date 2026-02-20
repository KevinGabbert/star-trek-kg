
namespace StarTrek_KG.Interfaces
{
    public interface IStar: ICoordinateObject
    {
        new string Name { get; }
        int Gravity { get; }
    }
}
