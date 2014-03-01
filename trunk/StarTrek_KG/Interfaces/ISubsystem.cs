using StarTrek_KG.Actors;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    /// <summary>
    /// This is the contract that we need to have for each subsystem to operate
    /// </summary>
    public interface ISubsystem
    {
        int Damage { get; set; }
        int MaxTransfer { get; set; }
        int Energy { get; set; }
        Game Game { get; set; }
        SubsystemType Type { get; set; }
        Ship ShipConnectedTo { get; set; }

        ISubsystem For(Ship ship, Game game);

        void Controls(string command); //A common feature of this method is this is where you recieve damage
        
        bool Damaged();  //This is where you find out if you are damaged
        bool PartialRepair();   //This is where you solve that damage problem you've been having.  (an automatic process)
        void FullRepair();

        void TakeDamage();
        int GetNext(int seed);
        int TransferredFromUser();
    }
}
