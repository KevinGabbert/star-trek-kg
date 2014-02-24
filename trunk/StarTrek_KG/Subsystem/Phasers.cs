using System;
using System.Collections.Generic;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Phasers : SubSystem_Base
    {
        //dependencies to inject
        //Quadrants
        //Utility

        public Phasers(Ship shipConnectedTo, Game game): base(shipConnectedTo, game)
        {
            this.Game.Write = this.Game.Write; //todo: remove this when all subsystems are converted
            this.Type = SubsystemType.Phasers;
        }

        public void Fire(double energyToFire, IShip shipFiringPhasers)
        {
            if (!this.EnergyCheckFail(energyToFire, shipFiringPhasers))
            {
                //todo: move to Game() object
                this.Game.ALLHostilesAttack(this.Game.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.

                shipFiringPhasers.Energy = this.ShipConnectedTo.Energy -= energyToFire;
                Phasers.For(this.ShipConnectedTo).Execute(energyToFire);
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
            if ((new Quadrants(this.Game.Map, this.Game.Write)).NoHostiles(this.ShipConnectedTo.Map.Quadrants.GetActive().GetHostiles()))
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
            this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));
        }

        private void Execute(double phaserEnergy)
        {
            var inNebula = this.ShipConnectedTo.GetQuadrant().Type == QuadrantType.Nebulae;

            if (inNebula)
            {
                this.Game.Write.Line("Due to the Nebula, phaser effectiveness will be reduced.");
            }

            this.Game.Write.Line("Firing phasers..."); //todo: pull from config

            //TODO: BUG: fired phaser energy won't subtract from ship's energy

            var destroyedShips = new List<IShip>();
            foreach (var badGuyShip in this.Game.Map.Quadrants.GetActive().GetHostiles())
            {
                Location location = this.ShipConnectedTo.GetLocation();

                double distance = Utility.Utility.Distance(location.Sector.X, location.Sector.Y, badGuyShip.Sector.X, badGuyShip.Sector.Y);
                double deliveredEnergy = Utility.Utility.ShootBeamWeapon(phaserEnergy, distance, "PhaserShotDeprecationRate", "PhaserEnergyAdjustment", inNebula);

                this.BadGuyTakesDamage(destroyedShips, badGuyShip, deliveredEnergy);
            }

            if (this.ShipConnectedTo.GetQuadrant().GetStarbaseCount() > 0 && this.Game.PlayerNowEnemyToFederation)
            {
                //todo: this is because starbases are not an object yet and we don't know how tough their shields are.. stay tuned, then delete this IF statement when they become like everyone else
                //for what its worth, Starbases will have a lot more power!
                this.Game.Write.Line("Starbases cannot be hit with phasers.. Yet..");
            }

            this.Game.Map.RemoveAllDestroyedShips(this.Game.Map, destroyedShips);//remove from Hostiles collection

            foreach (var destroyedShip in destroyedShips)
            {
                if(destroyedShip.Faction == FactionName.Federation)
                {
                    this.ShipConnectedTo.Scavenge(ScavengeType.FederationShip);
                }
                else
                {
                    this.ShipConnectedTo.Scavenge(ScavengeType.OtherShip);
                }

                //todo: add else if for starbase when the time comes
            }
        }

        private bool PromptUserForPhaserEnergy(out double phaserEnergy)
        {
            return this.Game.Write.PromptUser(String.Format("Enter phaser energy (1--{0}): ", this.ShipConnectedTo.Energy), out phaserEnergy);
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
                Phasers.DestroyBadGuy(destroyedShips, badGuyShip);
            }
            else
            {
                this.DamageBadGuy(badGuyShip, badGuyShields);
            }
        }

        private void DamageBadGuy(IShip badGuyShip, Actors.System badGuyShields)
        {
            string badGuyShipName = badGuyShip.Name;
            string badguyShieldEnergy = badGuyShields.Energy.ToString();

            if (badGuyShip.GetQuadrant().Type == QuadrantType.Nebulae)
            {
                badGuyShipName = "Unknown Hostile Ship";
                badguyShieldEnergy = "Unknown level";
            }

            var badGuy = Utility.Utility.HideXorYIfNebula(badGuyShip.GetQuadrant(), badGuyShip.Sector.X.ToString(), badGuyShip.Sector.Y.ToString());

            this.Game.Write.Line(
                string.Format(
                    "Hit " + badGuyShipName + " at sector [{0},{1}], shield strength now at {2}.",
                    badGuy.X, badGuy.Y, badguyShieldEnergy));
        }

        private static void DestroyBadGuy(ICollection<IShip> destroyedShips, IShip badGuyShip)
        {
            badGuyShip.Destroyed = true;

            //Phasers hit all ships on a single turn, so this builds up a list of destroyed ships
            destroyedShips.Add(badGuyShip);
        }

        public static Phasers For(IShip ship)
        {
            return (Phasers)SubSystem_Base.For(ship, SubsystemType.Phasers);
        }
    }
}
