using System;
using System.Collections.Generic;
using System.Linq;
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

        public Phasers(IShip shipConnectedTo): base(shipConnectedTo)
        {
            this.Type = SubsystemType.Phasers;
            this.ShipConnectedTo = shipConnectedTo;
        }

        public void Fire(int energyToFire) //, IShip shipFiringPhasers
        {
            IGame game = this.ShipConnectedTo.Map.Game;

            if (!this.EnergyCheckFail(energyToFire, this.ShipConnectedTo))
            {
                //todo: move to Game() object
                game.ALLHostilesAttack(game.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.

                this.ShipConnectedTo.Energy = this.ShipConnectedTo.Energy -= energyToFire;
                Phasers.For(this.ShipConnectedTo).Execute(energyToFire);
            }
            else
            {
                //Energy Check has failed
                game.Interact.Line("Not enough Energy to fire Phasers");
            }
        }

        public IEnumerable<string> Controls(IShip shipFiringPhasers)
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) return this.ShipConnectedTo.OutputQueue();

            //todo:  this doesn't *work* too well as a feature of *Regions*, but rather, of Ship?

            var regions = new Regions(this.ShipConnectedTo.Map, this.ShipConnectedTo.Map.Game.Interact);

            //todo: this may need a different refactor
            List<string> hostilesOutputLines;
            bool gotHostiles = regions.NoHostiles(this.ShipConnectedTo.Map.Regions.GetActive().GetHostiles(), out hostilesOutputLines);

            if (gotHostiles)
            {
                return hostilesOutputLines;
            }

            string phaserEnergy;

            if (!this.PromptUserForPhaserEnergy(out phaserEnergy))
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("Invalid energy level.");
                return this.ShipConnectedTo.OutputQueue();
            }

            this.ShipConnectedTo.Map.Game.Interact.Line("");

            this.Fire(int.Parse(phaserEnergy)); //, shipFiringPhasers
            this.ShipConnectedTo.Map.Game.Interact.OutputConditionAndWarnings(this.ShipConnectedTo, this.ShipConnectedTo.Map.Game.Config.GetSetting<int>("ShieldsDownLevel"));

            return this.ShipConnectedTo.OutputQueue();
        }

        private void Execute(double phaserEnergy)
        {
            IGame game = this.ShipConnectedTo.Map.Game;
            bool inNebula = this.InNebula();

            //TODO: BUG: fired phaser energy won't subtract from ship's energy

            var destroyedShips = new List<IShip>();

            List<IShip> hostiles = game.Map.Regions.GetActive().GetHostiles();

            foreach (var badGuyShip in hostiles)
            {
                this.FireOnShip(phaserEnergy, badGuyShip, inNebula, destroyedShips);
            }

            if (this.ShipConnectedTo.GetRegion().GetStarbaseCount() > 0 && game.PlayerNowEnemyToFederation)
            {
                //todo: this is because starbases are not an object yet and we don't know how tough their shields are.. stay tuned, then delete this IF statement when they become like everyone else
                
                //for what its worth, Starbases will have a lot more power!
                game.Interact.Line("Starbases cannot be hit with phasers.. Yet..");
            }

            game.Map.RemoveDestroyedShipsAndScavenge(destroyedShips);
        }

        private bool InNebula()
        {
            var inNebula = this.ShipConnectedTo.GetRegion().Type == RegionType.Nebulae;

            if (inNebula)
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("Due to the Nebula, phaser effectiveness will be reduced.");
            }

            return inNebula;
        }

        private void FireOnShip(double phaserEnergy, IShip badGuyShip, bool inNebula, ICollection<IShip> destroyedShips)
        {
            Location location = this.ShipConnectedTo.GetLocation();

            this.ShipConnectedTo.Map.Game.Interact.Line("Phasers locked on: " + badGuyShip.Name);

            double distance = Utility.Utility.Distance(location.Sector.X, location.Sector.Y, badGuyShip.Sector.X,
                badGuyShip.Sector.Y);
            double deliveredEnergy = Utility.Utility.ShootBeamWeapon(phaserEnergy, distance, "PhaserShotDeprecationRate",
                "PhaserEnergyAdjustment", inNebula);

            this.BadGuyTakesDamage(destroyedShips, badGuyShip, deliveredEnergy);
        }

        private bool PromptUserForPhaserEnergy(out string phaserEnergy)
        {
            return this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Phasers, "Phasers:>", $"Enter phaser energy (1--{this.ShipConnectedTo.Energy}): ", out phaserEnergy, this.ShipConnectedTo.Map.Game.Interact.Output.Queue);
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

            this.ShipConnectedTo.Map.Game.Interact.Line(
                string.Format("Hit " + badGuyShipName + " at sector [{0},{1}], shield strength now at {2}.", badGuy.X, badGuy.Y, badguyShieldEnergy));
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

            this.ShipConnectedTo.Map.Game.Interact.Line("");
            this.ShipConnectedTo.Map.Game.Interact.Line("Objects to Target:");

            List<KeyValuePair<int, Sector>> sectorsWithObjects = Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

            string userReply;
            this.ShipConnectedTo.Map.Game.Interact.Line("");
            this.ShipConnectedTo.Map.Game.Interact.PromptUserConsole("Enter number to lock Phasers: ", out userReply);

            int number = Convert.ToInt32(userReply);
            var objectToFireOn = sectorsWithObjects.Single(i => i.Key == number).Value;

            string phaserEnergy;
            if (!this.PromptUserForPhaserEnergy(out phaserEnergy))
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("Invalid phaser energy level.");
                return;
            }

            var hostilesHaveAttacked = false;
            if (!this.EnergyCheckFail(int.Parse(phaserEnergy), this.ShipConnectedTo))
            {
                IGame game = this.ShipConnectedTo.Map.Game;
                int randomBadGuyShoots = Utility.Utility.TestableRandom(game, 2, 2);
                if (randomBadGuyShoots == 1)
                {
                    game.ALLHostilesAttack(game.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.  
                    hostilesHaveAttacked = true;
                }

                this.ShipConnectedTo.Energy -= int.Parse(phaserEnergy);

                var destroyedShips = new List<IShip>();
                switch (objectToFireOn.Item)
                {
                     case SectorItem.HostileShip:
                     case SectorItem.FriendlyShip:
                         this.FireOnShip(int.Parse(phaserEnergy), (IShip)objectToFireOn.Object, this.InNebula(), destroyedShips);
                        break;

                     case SectorItem.Starbase:
                        //todo: support Starbase Hit points
                        game.DestroyStarbase(game.Map, objectToFireOn.Y, objectToFireOn.X, objectToFireOn);
                        break;

                     case SectorItem.Star:
                        this.FireOnStar((IStar)objectToFireOn.Object);
                        break;
                }

                game.Map.RemoveDestroyedShipsAndScavenge(destroyedShips);

                if (!hostilesHaveAttacked)
                {
                    game.ALLHostilesAttack(game.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.  
                }
            }
        }

        private void FireOnStar(IStar iStar)
        {
            this.ShipConnectedTo.Map.Game.Interact.Line("");
            this.ShipConnectedTo.Map.Game.Interact.Line($"Direct hit on {iStar.Name}. No apparent damage to stellar body.");
        }
    }
}
