
namespace StarTrek_KG.Interfaces
{
    public interface IInteract
    {
        void GetValueFromUser(string subCommand, IInteraction prompt);
        bool NotRecognized(string command, IInteraction prompt);
    }
}
