﻿using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Interfaces
{
    /// <summary>
    /// This is the contract that we need to have for each subsystem to operate
    /// </summary>
    public interface ISubsystem
    {
        SubsystemType Type { get; set; }
        Ship ShipConnectedTo { get; set; }

        int Damage { get; set; }
        int MaxTransfer { get; set; }
        int Energy { get; set; }
        IInteraction Prompt { get; set; }

        ISubsystem For(Ship ship);

        /// <summary>
        /// A common feature of this method is this is where you recieve damage
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        List<string> Controls(string command);

        /// <summary>
        /// This is where you find out if you are damaged
        /// </summary>
        /// <returns></returns>
        bool Damaged();
        void TakeDamage();

        /// <summary>
        /// This is where you solve that damage problem you've been having.  (an automatic process)
        /// </summary>
        /// <returns></returns>
        bool PartialRepair();

        void FullRepair();
        int GetNext(int seed);
        int TransferredFromUser();
    }
}
