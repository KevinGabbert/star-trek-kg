﻿using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Phasers : SubSystem_Base
    {
        //dependencies to inject
        //Quadrants
        //Utility

        public Phasers(Ship shipConnectedTo, Game game)
        {
            this.Game = game;

            //For subsystem_Base, temporarily
            this.Write = this.Game.Write; //todo: remove this when all subsystems are converted

            this.Initialize();

            this.ShipConnectedTo = shipConnectedTo;
            this.Type = SubsystemType.Phasers;
        }

        public override void OutputDamagedMessage()
        {
            this.Game.Write.Line("Phasers are damaged. Repairs are underway.");
        }

        public override void OutputRepairedMessage()
        {
            this.Game.Write.Line("Phasers have been repaired.");
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public void Fire(double energyToFire, IShip shipFiringPhasers)
        {
            if (!this.EnergyCheckFail(energyToFire, shipFiringPhasers))
            {
                shipFiringPhasers.Energy = this.ShipConnectedTo.Energy -= energyToFire;
                Phasers.For(this.ShipConnectedTo).Execute(energyToFire);

                //todo: move to Game() object
                this.Game.ALLHostilesAttack(this.ShipConnectedTo.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.
            }
            else
            {
                //Energy Check has failed
                this.Game.Write.Line("Not enough Energy to fire Phasers");
            }
        }

        public void Controls(IShip shipFiringPhasers)
        {
            if (this.Damaged()) return;

            //todo:  this doesn't *work* too well as a feature of *quadrants*, but rather, of Ship?
            if ((new Quadrants(this.ShipConnectedTo.Map, this.Game.Write)).NoHostiles(this.ShipConnectedTo.Map.Quadrants.GetActive().GetHostiles()))
            {
                return;
            }

            double phaserEnergy;

            this.Game.Write.Line("Phasers locked on target."); //todo: there should be an element of variation on this if computer is damaged.

            if (!this.PromptUserForPhaserEnergy(out phaserEnergy))
            {
                this.Game.Write.Line("Invalid energy level.");
                return;
            }
            this.Game.Write.Line("");

            this.Fire(phaserEnergy, shipFiringPhasers);
        }

        private void Execute(double phaserEnergy)
        {
            this.Game.Write.Line("Firing phasers..."); //todo: pull from config

            //TODO: BUG: fired phaser energy won't subtract from ship's energy

            var destroyedShips = new List<IShip>();
            foreach (var badGuyShip in this.ShipConnectedTo.Map.Quadrants.GetActive().GetHostiles())
            {
                Location location = this.ShipConnectedTo.Map.Playership.GetLocation();

                double distance = Utility.Utility.Distance(location.Sector.X, location.Sector.Y, badGuyShip.Sector.X, badGuyShip.Sector.Y);

                double deliveredEnergy = Utility.Utility.ShootBeamWeapon(phaserEnergy, distance, "PhaserShotDeprecationRate", "PhaserEnergyAdjustment");

                this.BadGuyTakesDamage(destroyedShips, badGuyShip, deliveredEnergy);
            }

            this.ShipConnectedTo.Map.RemoveAllDestroyedShips(this.ShipConnectedTo.Map, destroyedShips);//remove from Hostiles collection
        }

        private bool PromptUserForPhaserEnergy(out double phaserEnergy)
        {
            return this.Game.Write.PromptUser(String.Format("Enter phaser energy (1--{0}): ", this.ShipConnectedTo.Map.Playership.Energy), out phaserEnergy);
        }

        //todo: move to Utility() object
        private bool EnergyCheckFail(double phaserEnergy, IShip firingShip)
        {
            return phaserEnergy < 1 || phaserEnergy > firingShip.Energy;
        }

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
                this.Game.Write.Line(string.Format("Hit " + badGuyShip.Name + " at sector [{0},{1}]. " + badGuyShip.Name + " shield strength down to {2}.",
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
