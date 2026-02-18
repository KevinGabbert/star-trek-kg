namespace StarTrek_KG.Interfaces
{
    public interface INavigationSubsystem
    {
        void SetCourse(string value);
        void SetWarpSpeed(string value);
        void EngageWarp();
    }

}
