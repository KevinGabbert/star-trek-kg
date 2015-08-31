using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Phasers : SubSystem_Base, IWeapon
    {
        //dependencies to inject
        //Regions
        //Utility

        public Phasers(Ship shipConnectedTo, Game game): base(shipConnectedTo, game)
        {
            this.Game.Write = this.Game.Write; //todo: remove this when all subsystems are converted
            this.Type = SubsystemType.Phasers;
            this.ShipConnectedTo = shipConnectedTo;
        }

        public void Fire(int energyToFire) //, IShip shipFiringPhasers
        {
            if (!this.EnergyCheckFail(energyToFire, this.ShipConnectedTo))
            {
                //todo: move to Game() object
                this.Game.ALLHostilesAttack(this.Game.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.

                this.ShipConnectedTo.Energy = this.ShipConnectedTo.Energy -= energyToFire;
                Phasers.For(this.ShipConnectedTo).Execute(energyToFire);
            }
            else
            {
                //Energy Check has failed
                this.Game.Write.Line("Not enough Energy to fire Phasers");
            }
        }

        public IEnumerable<string> Controls(IShip shipFiringPhasers)
        {
            this.Game.Write.Output.Queue.Clear();

            if (this.Damaged()) return this.Game.Write.Output.Queue.ToList();

            //todo:  this doesn't *work* too well as a feature of *Regions*, but rather, of Ship?

            var regions = new Regions(this.Game.Map, this.Game.Write);

            //todo: this may need a different refactor
            List<string> hostilesOutputLines;
            bool gotHostiles = regions.NoHostiles(this.ShipConnectedTo.Map.Regions.GetActive().GetHostiles(), out hostilesOutputLines);

            if (gotHostiles)
            {
                return hostilesOutputLines;
            }

            int phaserEnergy;

            if (!this.PromptUserForPhaserEnergy(out phaserEnergy))
            {
                this.Game.Write.Line("Invalid energy level.");
                return this.Game.Write.Output.Queue.ToList();
            }

            this.Game.Write.Line("");

            this.Fire(phaserEnergy); //, shipFiringPhasers
            this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));

            return this.Game.Write.Output.Queue.ToList();
        }

        private void Execute(double phaserEnergy)
        {
            var inNebula = this.InNebula();

            //TODO: BUG: fired phaser energy won't subtract from ship's energy

            var destroyedShips = new List<IShip>();

            List<IShip> hostiles = this.Game.Map.Regions.GetActive().GetHostiles();

            foreach (var badGuyShip in hostiles)
            {
                this.FireOnShip(phaserEnergy, badGuyShip, inNebula, destroyedShips);
            }

            if (this.ShipConnectedTo.GetRegion().GetStarbaseCount() > 0 && this.Game.PlayerNowEnemyToFederation)
            {
                //todo: this is because starbases are not an object yet and we don't know how tough their shields are.. stay tuned, then delete this IF statement when they become like everyone else
                
                //for what its worth, Starbases will have a lot more power!
                this.Game.Write.Line("Starbases cannot be hit with phasers.. Yet..");
            }

            this.Game.Map.RemoveDestroyedShipsAndScavenge(destroyedShips);
        }

        private bool InNebula()
        {
            var inNebula = this.ShipConnectedTo.GetRegion().Type == RegionType.Nebulae;

            if (inNebula)
            {
                this.Game.Write.Line("Due to the Nebula, phaser effectiveness will be reduced.");
            }

            return inNebula;
        }

        private void FireOnShip(double phaserEnergy, IShip badGuyShip, bool inNebula, ICollection<IShip> destroyedShips)
        {
            Location location = this.ShipConnectedTo.GetLocation();

            this.Game.Write.Line("Phasers locked on: " + badGuyShip.Name);

            double distance = Utility.Utility.Distance(location.Sector.X, location.Sector.Y, badGuyShip.Sector.X,
                badGuyShip.Sector.Y);
            double deliveredEnergy = Utility.Utility.ShootBeamWeapon(phaserEnergy, distance, "PhaserShotDeprecationRate",
                "PhaserEnergyAdjustment", inNebula);

            this.BadGuyTakesDamage(destroyedShips, badGuyShip, deliveredEnergy);
        }

        private bool PromptUserForPhaserEnergy(out int phaserEnergy)
        {
            return this.Game.Write.PromptUser(SubsystemType.Phasers, $"Enter phaser energy (1--{this.ShipConnectedTo.Energy}): ", out phaserEnergy);
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

            if ((badGuyShields.Energy < 0) || badGuyShip.Energy < 0)
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

            if (badGuyShip.GetRegion().Type == RegionType.Nebulae)
            {
                badGuyShipName = "Unknown Hostile Ship";
                badguyShieldEnergy = "Unknown level";
            }

            var badGuy = Utility.Utility.HideXorYIfNebula(badGuyShip.GetRegion(), badGuyShip.Sector.X.ToString(), badGuyShip.Sector.Y.ToString());

            this.Game.Write.Line(
                string.Format(
                    "Hit " + badGuyShipName + " at sector [{0},{1}], shield strength now at {2}.",
                    badGuy.X, badGuy.Y, badguyShieldEnergy));
        }

        private static void DestroyBadGuy(ICollection<IShip> destroyedShips, IShip badGuyShip)
        {
            badGuyShip.Destroyed = true;
            badGuyShip.Energy = 0;
            Shields.For(badGuyShip).Energy = 0;

            //Other subsystems may retain some energy, for salvage mechanic.

            //Phasers hit all ships on a single turn, so this builds up a list of destroyed ships
            destroyedShips.Add(badGuyShip);
        }

        public static Phasers For(IShip ship)
        {
            return (Phasers)SubSystem_Base.For(ship, SubsystemType.Phasers);
        }

        public void TargetObject()
        {
            if (this.Damaged())
            {
                return;
            }

            this.Game.Write.Line("");
            this.Game.Write.Line("Objects to Target:");

            List<KeyValuePair<int, Sector>> sectorsWithObjects = Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

            string userReply;
            this.Game.Write.Line("");
            this.Game.Write.PromptUserConsole("Enter number to lock Phasers: ", out userReply);

            int number = Convert.ToInt32(userReply);
            var objectToFireOn = sectorsWithObjects.Single(i => i.Key == number).Value;

            int phaserEnergy;
            if (!this.PromptUserForPhaserEnergy(out phaserEnergy))
            {
                this.Game.Write.Line("Invalid phaser energy level.");
                return;
            }

            var hostilesHaveAttacked = false;
            if (!this.EnergyCheckFail(phaserEnergy, this.ShipConnectedTo))
            {
                int randomBadGuyShoots = Utility.Utility.TestableRandom(this.Game, 2, 2);
                if (randomBadGuyShoots == 1)
                {
                    this.Game.ALLHostilesAttack(this.Game.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.  
                    hostilesHaveAttacked = true;
                }

                this.ShipConnectedTo.Energy -= phaserEnergy;

                var destroyedShips = new List<IShip>();
                switch (objectToFireOn.Item)
                {
                     case SectorItem.HostileShip:
                     case SectorItem.FriendlyShip:
                         this.FireOnShip(phaserEnergy, (IShip)objectToFireOn.Object, this.InNebula(), destroyedShips);
                        break;

                     case SectorItem.Starbase:
                        //todo: support Starbase Hit points
                        this.Game.DestroyStarbase(this.Game.Map, objectToFireOn.Y, objectToFireOn.X, objectToFireOn);
                        break;

                     case SectorItem.Star:
                        this.FireOnStar((IStar)objectToFireOn.Object);
                        break;
                }

                this.Game.Map.RemoveDestroyedShipsAndScavenge(destroyedShips);

                if (!hostilesHaveAttacked)
                {
                    this.Game.ALLHostilesAttack(this.Game.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.  
                }
            }
        }

        private void FireOnStar(IStar iStar)
        {
            this.Game.Write.Line("");
            this.Game.Write.Line($"Direct hit on {iStar.Name}. No apparent damage to stellar body.");
        }
    }
}
