
namespace StarTrek_KG.Enums
{
    //TODO: get rid of enum. A SectorItem needs to be the actual object held.  (a star, or ship (I want a starbase to be a type of ship so it can have hitpoints, shield, be hostile, and fight back.))
    public enum SectorItem
    {
        Empty,
        Star,
        Hostile,
        Friendly, //playership
        Starbase
    };
}
