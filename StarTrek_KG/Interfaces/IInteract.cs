
namespace StarTrek_KG.Interfaces
{
    public interface IInteract
    {
        void GetValueFromUser(string subCommand);

        bool NotRecognized(string command, IInteraction promptInteraction);
    }
}
