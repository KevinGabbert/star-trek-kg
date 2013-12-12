using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Phasers : SubSystem_Base, IMap, IWeapon
    {
        public Phasers(Map map)
        {
            this.Map = map;
            this.Type = SubsystemType.Phasers;
        }

        public override void OutputDamagedMessage()
        {
            Output.WriteLine("Phasers are damaged. Repairs are underway.");
        }

        public override void OutputRepairedMessage()
        {
            Output.WriteLine("Phasers have been repaired.");
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public void Fire(double energyToFire, IShip shipFiringPhasers)
        {
            if (!EnergyCheckFail(energyToFire, shipFiringPhasers))
            {
                shipFiringPhasers.Energy = this.Map.Playership.Energy -= energyToFire;
                Phasers.Execute(this.Map, energyToFire);

                //any remaining bad guys now have the opportunity to fire back
                this.Map.Quadrants.ALLHostilesAttack(this.Map);
            }
            else
            {
                //Energy Check has failed
                Output.WriteLine("Not enough Energy to fire Phasers");
            }
        }

        public void Controls(Map map, IShip shipFiringPhasers)
        {
            if (this.Damaged()) return;
            if (Quadrants.NoHostiles(map.Quadrants.GetActive().GetHostiles()))
            {
                return;
            }

            double phaserEnergy;

            Output.WriteLine("Phasers locked on target."); //todo: there should be an element of variation on this if computer is damaged.

            if (!Phasers.PromptUserForPhaserEnergy(map, out phaserEnergy))
            {
                Output.WriteLine("Invalid energy level.");
                return;
            }
            Output.WriteLine("");

            this.Fire(phaserEnergy, shipFiringPhasers);
        }

        private static void Execute(Map map, double phaserEnergy)
        {
            Output.WriteLine("Firing phasers..."); //todo: pull from config

            //TODO: BUG: fired phaser energy won't subtract from ship's energy

            var destroyedShips = new List<IShip>();
            foreach (var badGuyShip in map.Quadrants.GetActive().GetHostiles())
            {
                double deliveredEnergy = ComputeDeliveredEnergy(map, phaserEnergy, badGuyShip);
                Phasers.BadGuyTakesDamage(destroyedShips, badGuyShip, deliveredEnergy);
            }

            map.RemoveAllDestroyedShips(map, destroyedShips);//remove from Hostiles collection
        }

        private static bool PromptUserForPhaserEnergy(Map map, out double phaserEnergy)
        {
            return Command.PromptUser(String.Format("Enter phaser energy (1--{0}): ", map.Playership.Energy), out phaserEnergy);
        }

        private static bool EnergyCheckFail(double phaserEnergy, IShip firingShip)
        {
            return phaserEnergy < 1 || phaserEnergy > firingShip.Energy;
        }

        //private static bool StarshipTakesHit(Map map, double phaserEnergy)
        //{
        //    Shields.For(map.Playership).Energy -= (int) phaserEnergy;
        //    if (Shields.For(map.Playership).Energy < 0)
        //    {
        //        Shields.For(map.Playership).Energy = 0;
        //        return true;
        //    }
        //    return false;
        //}

        private static double ComputeDeliveredEnergy(Map map, double phaserEnergy, IShip badGuyShip)
        {
            var location = map.Playership.GetLocation();
            var distance = Utility.Distance(location.Sector.X, location.Sector.Y, badGuyShip.Sector.X, badGuyShip.Sector.Y);
            var deliveredEnergy = phaserEnergy*(1.0 - distance/11.3);

            return deliveredEnergy;
        }

        private static void BadGuyTakesDamage(ICollection<IShip> destroyedShips, IShip badGuyShip, double deliveredEnergy)
        {
            //todo: add more descriptive output messages depending on how much phaser energy absorbed by baddie

            var badGuyShields = Shields.For(badGuyShip);
            badGuyShields.Energy -= (int) deliveredEnergy;
            if (badGuyShields.Energy <= 0)
            {
                badGuyShip.Destroyed = true;

                //Phasers hit all ships on a single turn, so this builds up a list of destroyed ships
                destroyedShips.Add(badGuyShip);
            }
            else
            {
                Output.WriteLine(string.Format("Hit " + badGuyShip.Name + " at sector [{0},{1}]. Hostile shield strength dropped to {2}.",
                                  (badGuyShip.Sector.X), (badGuyShip.Sector.Y), badGuyShields.Energy));
            }
        }

        public new static Phasers For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Phasers). Add a Friendly to your GameConfig"); 
            }

            return (Phasers)ship.Subsystems.Single(s => s.Type == SubsystemType.Phasers);
        }
    }
}
