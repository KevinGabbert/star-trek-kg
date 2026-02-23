
namespace StarTrek_KG.Enums
{
    public enum CoordinateItem //Used by CoordinateDefs only
    {
        Empty,
        Star,
        HostileShip, //make this go away. The goal here is that a ship can be hostile or not, and even change its mind upon negotiations or being fired upon.
        FriendlyShip,
        PlayerShip, 
        Starbase,
        Deuterium,
        GraviticMine,
        Debug
    };
}
