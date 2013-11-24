﻿using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Config.Elements;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Subsystem
{
    public class Subsystems: List<ISubsystem>
    {
        private Playfield.Map map;

        public Subsystems(Playfield.Map map)
        {
            // TODO: Complete member initialization
            this.map = map;

            this.AddRange(new List<ISubsystem>(){
                                     new Shields(map) { Energy = 0 },
                                     new Computer(map),
                                     new Navigation(map),
                                     new ShortRangeScan(map),
                                     new LongRangeScan(map),
                                     new Torpedoes(map),
                                     new Phasers(map)
                                  });
        }
        public bool TakeDamageIfAppropriate(int boltStrength)
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

        public bool DamageRandomSubsystem()
        {
            bool wasDamaged = false;
            List<ISubsystem> remainingSubsystems = this.FindAll(s => !s.Damaged()); //at present.. Damaged() is true if subsystem has any damage value

            if (remainingSubsystems.Count > 0)
            {
                var rand = new Random();
                ISubsystem subSystemToDamage = remainingSubsystems[rand.Next(remainingSubsystems.Count)];
                subSystemToDamage.TakeDamage(); //todo: subSystemToDamage.TakeDamage(boltStrength); 

                wasDamaged = true;

                //Console.WriteLine(subSystemToDamage.GetType().ToString() + " Damaged");
            }

            return wasDamaged;
        }
    }
}
