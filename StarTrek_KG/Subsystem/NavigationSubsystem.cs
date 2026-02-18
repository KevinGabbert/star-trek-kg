using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Subsystem
{
    public class NavigationSubsystem : INavigationSubsystem
    {
        private readonly IShip _ship;

        public NavigationSubsystem(IShip ship)
        {
            _ship = ship;
        }

        public void SetCourse(string value)
        {
            _ship.OutputLine($"[Navigation] Setting course to {value}");
        }

        public void SetWarpSpeed(string value)
        {
            _ship.OutputLine($"[Navigation] Setting warp speed to {value}");
        }

        public void EngageWarp()
        {
            _ship.OutputLine("[Navigation] Engaging warp drive!");
        }
    }
}
