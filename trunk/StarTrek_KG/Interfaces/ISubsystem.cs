﻿using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    /// <summary>
    /// This is the contract that we need to have for each subsystem to operate
    /// </summary>
    public interface ISubsystem
    {
        Map Map { get; set; }
        int Damage { get; set; }
        int MaxTransfer { get; set; }
        int Energy { get; set; }
        
        ISubsystem For(Ship ship);

        SubsystemType Type { get; set; }

        void OutputDamagedMessage();
        void OutputRepairedMessage();
        void OutputMalfunctioningMessage();

        void Controls(string command); //A common feature of this method is this is where you recieve damage
        bool Damaged();  //This is where you find out if you are damaged
        bool Repair();   //This is where you solve that damage problem you've been having.  (an automatic process)

        void TakeDamage();
        int GetNext(int seed);
        int TransferredFromUser();
    }
}
