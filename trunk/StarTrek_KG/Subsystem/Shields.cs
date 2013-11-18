﻿using System;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public  class Shields : SubSystem_Base, IMap
    {

        public static readonly string[] CONTROL_PANEL = {
                                                    "--- > Shield Controls ----------------",
                                                    "add = Add energy to shields.",
                                                    "sub = Subtract energy from shields."
                                                };
        
        public Shields(Map map)
        {
            this.Map = map;
            this.Type = SubsystemType.Shields;
            this.Damage = 0;
            this.Energy = 0;
        }

        public override void OutputDamagedMessage()
        {
            Output.Write("Shield control is damaged. Repairs are underway.");
        }
        public override void OutputRepairedMessage()
        {
            Output.Write("Shield controls have been repaired.");
        }
        public override void OutputMalfunctioningMessage()
        {
            Output.Write("The Shields are malfunctioning.");
        }

        public override void Controls(string command)
        {
            if (Damaged()) return;

            bool adding;
            switch (command)
            {
                case "add": 
                    adding = true;
                    break;
                case "sub":
                    adding = false;
                    this.MaxTransfer = this.Energy;
                    break;
                default:
                    Output.Write("Invalid command.");
                    return;
            }

            var transfer = this.TransferredFromUser();

            if(transfer != 0)
            {
                this.AddEnergy(transfer, adding);

                Console.WriteLine("Shield strength is now {0}. Total Energy level is now {1}.",
                                  this.Energy,
                                  Map.Playership.Energy);
            }
        }

        public new int TransferredFromUser()
        {
            double transfer;
            var readSuccess = Command.PromptUser(String.Format("Enter amount of energy (1--{0}): ", this.MaxTransfer), out transfer);
            var tooLittle = transfer < 1;
            var tooMuch = transfer > this.MaxTransfer;

            if (!readSuccess || tooLittle || tooMuch)
            {
                //todo: tell the user if they are adding too much or too little energy
                Output.Write("Invalid amount of energy.");
                return 0;
            }
            return (int)transfer;
        }

        public new static Shields For(Ship ship)
        {
            if(ship== null)
            {
                throw new GameConfigException("Ship not set up (shields). Add a Friendly to your GameConfig");
            }

            return (Shields)ship.Subsystems.Single(s => s.Type == SubsystemType.Shields);
        }
    }
}
