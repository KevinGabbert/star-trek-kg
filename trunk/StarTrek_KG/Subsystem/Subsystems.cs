using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Subsystems: List<ISubsystem>
    {
        public Subsystems(Map map, Ship shipConnectedTo, IStarTrekKGSettings config)
        {
            // TODO: Complete member initialization
            var game = new Game(config, false);
            game.Map = map;

            var write = new Write(config);
            game.Write = write;

            this.AddRange(new List<ISubsystem>(){
                                     new Debug(shipConnectedTo, game),
                                     new Shields(shipConnectedTo, game) { Energy = 0 },
                                     new Computer(shipConnectedTo, game),
                                     new Navigation(shipConnectedTo, game),
                                     new ShortRangeScan(shipConnectedTo, game),
                                     new LongRangeScan(shipConnectedTo, game),
                                     new Torpedoes(shipConnectedTo, game),
                                     new Phasers(shipConnectedTo, game) //TODO: get game ref from shipCOnnectedTo
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
                    ((Navigation)subsystem).docked = true;        //TODO: (this.game.config).GetSetting<int>("repairDocked");
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

        //todo: move to Game() object????
        public bool DamageRandomSubsystem()
        {
            bool wasDamaged = false;
            List<ISubsystem> remainingSubsystems = this.FindAll(s => !s.Damaged()); //at present.. Damaged() is true if subsystem has any damage value

            if (remainingSubsystems.Count > 0)
            {
                var rand = Utility.Utility.Random;
                ISubsystem subSystemToDamage = null;

               while (subSystemToDamage == null || subSystemToDamage.Type == SubsystemType.Debug)
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
                returnVal += " " + subSystem.GetType().Name;
            }

            return returnVal;
        }
    }
}
