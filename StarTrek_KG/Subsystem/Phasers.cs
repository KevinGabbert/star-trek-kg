using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Phasers : SubSystem_Base, IMap, ICommand, IWrite, IDraw
    {
        public Phasers(Map map, Ship shipConnectedTo, Draw draw, Write write, Command command)
        {
            this.Draw = draw;
            this.Write = write;
            this.Command = command;

            this.Initialize();

            if (this.Draw == null)
            {
                throw new GameException("Property Draw is not set for: " + this.Type);
            }

            this.ShipConnectedTo = shipConnectedTo;
            this.Map = map;
            this.Type = SubsystemType.Phasers;
        }

        public override void OutputDamagedMessage()
        {
            this.Write.Line("Phasers are damaged. Repairs are underway.");
        }

        public override void OutputRepairedMessage()
        {
            this.Write.Line("Phasers have been repaired.");
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public void Fire(double energyToFire, IShip shipFiringPhasers)
        {
            if (!EnergyCheckFail(energyToFire, shipFiringPhasers))
            {
                shipFiringPhasers.Energy = this.ShipConnectedTo.Energy -= energyToFire;
                Phasers.For(this.ShipConnectedTo).Execute(this.Map, energyToFire);

                //todo: move to Game() object
                //todo: move to Game() object
                //any remaining bad guys now have the opportunity to fire back
                (new Game(this.Draw, false)).ALLHostilesAttack(this.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.
            }
            else
            {
                //Energy Check has failed
                this.Write.Line("Not enough Energy to fire Phasers");
            }
        }

        public void Controls(IShip shipFiringPhasers)
        {
            if (this.Damaged()) return;
            if (Quadrants.NoHostiles(this.Map.Quadrants.GetActive().GetHostiles()))
            {
                return;
            }

            double phaserEnergy;

            this.Write.Line("Phasers locked on target."); //todo: there should be an element of variation on this if computer is damaged.

            if (!this.PromptUserForPhaserEnergy(this.Map, out phaserEnergy))
            {
                this.Write.Line("Invalid energy level.");
                return;
            }
            this.Write.Line("");

            this.Fire(phaserEnergy, shipFiringPhasers);
        }

        private void Execute(Map map, double phaserEnergy)
        {
            this.Write.Line("Firing phasers..."); //todo: pull from config

            //TODO: BUG: fired phaser energy won't subtract from ship's energy

            var destroyedShips = new List<IShip>();
            foreach (var badGuyShip in map.Quadrants.GetActive().GetHostiles())
            {
                Location location = map.Playership.GetLocation();

                double distance = Utility.Utility.Distance(location.Sector.X, location.Sector.Y, badGuyShip.Sector.X, badGuyShip.Sector.Y);

                double deliveredEnergy = Utility.Utility.ShootBeamWeapon(phaserEnergy, distance, "PhaserShotDeprecationRate", "PhaserEnergyAdjustment");

                this.BadGuyTakesDamage(destroyedShips, badGuyShip, deliveredEnergy);
            }

            map.RemoveAllDestroyedShips(map, destroyedShips);//remove from Hostiles collection
        }

        private bool PromptUserForPhaserEnergy(Map map, out double phaserEnergy)
        {
            return this.Command.PromptUser(String.Format("Enter phaser energy (1--{0}): ", map.Playership.Energy), out phaserEnergy);
        }

        //todo: move to Utility() object
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

        //todo: move to badguy.DamageControl() object
        private void BadGuyTakesDamage(ICollection<IShip> destroyedShips, IShip badGuyShip, double deliveredEnergy)
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
                this.Write.Line(string.Format("Hit " + badGuyShip.Name + " at sector [{0},{1}]. " + badGuyShip.Name + " shield strength down to {2}.",
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
