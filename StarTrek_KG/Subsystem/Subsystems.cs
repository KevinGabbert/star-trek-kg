﻿using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Subsystems: List<ISubsystem>
    {
        public Subsystems(IMap map, Ship shipConnectedTo, IStarTrekKGSettings config)
        {
            // TODO: Complete member initialization
            var game = new Game(config, false)
            {
                Map = map
            };

            var write = new Interaction(config);
            game.Interact = write;

            shipConnectedTo.Game = game;

            this.AddRange(new List<ISubsystem>(){
                                     new Shields(shipConnectedTo),
                                     new ImmediateRangeScan(shipConnectedTo),
                                     new ShortRangeScan(shipConnectedTo),
                                     new LongRangeScan(shipConnectedTo),
                                     new CombinedRangeScan(shipConnectedTo),
                                     new Warp(shipConnectedTo),
                                     new Impulse(shipConnectedTo),
                                     new Debug(shipConnectedTo, game),
                                     new Computer(shipConnectedTo),
                                     new Navigation(shipConnectedTo),
                                     new Torpedoes(shipConnectedTo),
                                     new Phasers(shipConnectedTo),
                                     new DamageControl(shipConnectedTo)//TODO: get game ref from shipCOnnectedTo
                                  });
        }

        //todo: create an indexer so we can look up a subsystem

        public void FullRepair()
        {
            foreach (ISubsystem subsystem in this)
            {
                subsystem.FullRepair(); //TODO: make the priority level configurable

                if(subsystem.Type == SubsystemType.Torpedoes)
                {
                    ((Torpedoes)subsystem).Count = 10;        //TODO: (this.game.config).GetSetting<int>("repairTorpedoes");
                }

                if (subsystem.Type == SubsystemType.Navigation)
                {
                    ((Navigation)subsystem).Docked = true;        //TODO: (this.game.config).GetSetting<int>("repairDocked");
                }
            }

            //todo: starbases can upgrade energy maximums during game
        }

        /// <summary>
        /// repairs one item every time called
        /// </summary>
        /// <returns></returns>
        public void PartialRepair()
        {
            foreach (var subsystem in this)
            {
                subsystem.PartialRepair(); //TODO: make the priority level configurable
            }
        }

        public bool TakeDamageIfWeCan(int boltStrength)
        {
            bool retVal = false; 

            //choose a random subsystem from this, and tell it to subtract damage from itself.
            //TODO: eventually I'd like to take the exact amount of energy delivered to a ship, bute it some, and then transfer it 
            //to a random subsystem.

            ISubsystem shields = this.Single(s => s.Type == SubsystemType.Shields);
            bool shipHasShieldControlEquipped = shields != null;

            //If ship has shields then have shields take damage
            if (shipHasShieldControlEquipped) 
            {
                if(shields.Energy < 1)
                {
                    retVal = this.DamageRandomSubsystem(); //todo:  "destroy" a subsystem.. (need to get to a starbase to get it back)
                }
                else
                {
                    shields.TakeDamage(); //todo: shields.TakeDamage(boltStrength); 
                }
            }

            return retVal;
        }

        //todo: move to Game() object????
        private bool DamageRandomSubsystem()
        {
            bool wasDamaged = false;
            List<ISubsystem> remainingSubsystems = this.FindAll(s => !s.Damaged() && s.Type != SubsystemType.Debug); //at present.. Damaged() is true if subsystem has any damage value

            if (remainingSubsystems.Count > 0) //debug is ignored by all code, so we will never have less than 1
            {
                var rand = Utility.Utility.Random;
                ISubsystem subSystemToDamage = null;

                while (subSystemToDamage == null)
                {
                    subSystemToDamage = remainingSubsystems[rand.Next(remainingSubsystems.Count - 1)];
                }

                subSystemToDamage.TakeDamage(); //todo: subSystemToDamage.TakeDamage(boltStrength); 

                wasDamaged = true;
            }
            else
            {
                //todo: destroy ship
            }

            return wasDamaged;
        }

        //Needs to be improved
        public override string ToString()
        {
            string returnVal = "";
            foreach (ISubsystem subSystem in this)
            {
                returnVal += $" {subSystem.GetType().Name}";
            }

            return returnVal;
        }
    }
}
