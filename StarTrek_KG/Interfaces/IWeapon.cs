namespace StarTrek_KG.Interfaces
{
    //todo: make it so that a weapon can be fired manually if the computer is down (slower and more inaccurate)
    public interface IBeamWeapon
    {
        double Shoot(double energyToPowerWeapon, double distance);
    }
}
