using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Phasers : SubSystem_Base, IMap //, IDestructionCheck
    {
        public Phasers(Map map)
        {
            this.Map = map;
            this.Type = SubsystemType.Phasers;
        }

        public override void OutputDamagedMessage()
        {
            Output.Write("Phasers are damaged. Repairs are underway.");
        }

        public override void OutputRepairedMessage()
        {
            Output.Write("Phasers have been repaired.");
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public void Fire(double energy)
        {
            if (!EnergyCheckFail(energy, this.Map))
            {
                Phasers.Execute(this.Map, energy);

                //any remaining bad guys now have the opportunity to fire back
                this.Map.Quadrants.ALLHostilesAttack(this.Map);
            }
        }

        public override void Controls(string command)
        {
            this.Controls(this.Map);
        }

        public void Controls(Map map)
        {
            if (Damaged()) return;
            if (Quadrants.NoHostiles(map.Quadrants.GetActive().Hostiles)) return;

            double phaserEnergy;
            Console.WriteLine("Phasers locked on target.");
            if (!GotPhaserEnergyFromUser(map, out phaserEnergy) || EnergyCheckFail(phaserEnergy, map))
            {
                Output.Write("Invalid energy level.");
                return;
            }
            Console.WriteLine();

            this.Fire(phaserEnergy);
        }

        private static void Execute(Map map, double phaserEnergy)
        {
            Output.Write("Firing phasers..."); //todo: pull from config

            var destroyedShips = new List<Ship>();
            foreach (var badGuyShip in map.Quadrants.GetActive().Hostiles)
            {
                double deliveredEnergy = ComputeDeliveredEnergy(map, phaserEnergy, badGuyShip);
                Phasers.BadGuyTakesDamage(destroyedShips, badGuyShip, deliveredEnergy);
            }

            map.RemoveAllDestroyedShips(map, destroyedShips);//remove from Hostiles collection
        }

        private static bool GotPhaserEnergyFromUser(Map map, out double phaserEnergy)
        {
            return Command.PromptUser(String.Format("Enter phaser energy (1--{0}): ", map.Playership.Energy), out phaserEnergy);
        }

        private static bool EnergyCheckFail(double phaserEnergy, Map map)
        {
            return phaserEnergy < 1 || phaserEnergy > map.Playership.Energy;
        }
        private static bool StarshipTakesHit(Map map, double phaserEnergy)
        {
            Shields.For(map.Playership).Energy -= (int) phaserEnergy;
            if (Shields.For(map.Playership).Energy < 0)
            {
                Shields.For(map.Playership).Energy = 0;
                return true;
            }
            return false;
        }

        private static double ComputeDeliveredEnergy(Map map, double phaserEnergy, IShip badGuyShip)
        {
            var location = map.Playership.GetLocation();
            var distance = Map.Distance(location.Sector.X, location.Sector.Y, badGuyShip.Sector.X, badGuyShip.Sector.Y);
            var deliveredEnergy = phaserEnergy*(1.0 - distance/11.3);

            return deliveredEnergy;
        }

        private static void BadGuyTakesDamage(ICollection<Ship> destroyedShips, Ship badGuyShip, double deliveredEnergy)
        {
            var badGuyShields = Shields.For(badGuyShip);
            badGuyShields.Energy -= (int) deliveredEnergy;
            if (badGuyShields.Energy <= 0)
            {
                Console.WriteLine(badGuyShip.Name + " destroyed at sector [{0},{1}].",
                                  (badGuyShip.Sector.X), (badGuyShip.Sector.Y));

                badGuyShip.Destroyed = true;

                //Phasers hit all ships on a single turn, so this builds up a list of destroyed ships
                destroyedShips.Add(badGuyShip);
            }
            else
            {
                Console.WriteLine("Hit " + badGuyShip.Name + " at sector [{0},{1}]. Hostile shield strength dropped to {2}.",
                                  (badGuyShip.Sector.X), (badGuyShip.Sector.Y), badGuyShields.Energy);
            }
        }

        public new static Phasers For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Phasers). Add a Friendly to your GameConfig"); //todo: make this a custom exception
            }

            return (Phasers)ship.Subsystems.Single(s => s.Type == SubsystemType.Phasers);
        }
    }
}
