using StarTrek_KG.Actors;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Disruptors : SubSystem_Base, IWeapon
    {
        //this.Initialize();
        public Disruptors(Ship shipConnectedTo, IGame game): base(shipConnectedTo)
        {
            this.Type = SubsystemType.Disruptors;
        }

        //todo: Under Construction
        //This will be the first subsystem that will 
        public int Execute(Location targetLocation) 
        {
            IShip thisShip = this.ShipConnectedTo;

            var distance = Utility.Utility.Distance(targetLocation.Sector.X,
                                                    targetLocation.Sector.Y,
                                                    thisShip.Sector.X,
                                                    thisShip.Sector.Y);

            //Ship target; //= Object in sector with targetLocation.sectorX, targetLocation.sectorY

            //subtract attacking energy from this ship. 
            //--If this ship is a baddie and energy is at zero, then it can no longer fire.
            //if this is a baddie, and running out of energy effectively kills this ship, then this will become a nonfiring hulk.
            //change the ship icon to something to render it inert.  Now its a navigation hazard, like a star at present
            //obstacle detected:  Hulk of the Neg'var
            //later versions can have this ship eventually get boarded, fixed, and working
            //skip whats below


            //var seedEnergyToPowerWeapon = (this.Game.Config).GetSetting<int>("DisruptorShotSeed") *
            //                              (Utility.Utility.Random).NextDouble();

            ////Todo: this should be Disruptors.For(this.ShipConnectedTo).Shoot()
            ////todo: the -1 should be the ship energy you want to allocate
            //var attackingEnergy = (int)Utility.Utility.ShootBeamWeapon(seedEnergyToPowerWeapon, distance, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment");

            //var shieldsValueBeforeHit = Shields.For(this.ShipConnectedTo).Energy;

            //this.ShipConnectedTo.AbsorbHitFrom(badGuy, attackingEnergy);

            //Game.ReportShieldsStatus(map, shieldsValueBeforeHit);

            return 0;
        }

        public void TargetObject()
        {
        //    if (this.Damaged())
        //    {
        //        return;
        //    }

        //    this.Game.Write.Line("");
        //    this.Game.Write.Line("Objects to Target:");

        //    List<KeyValuePair<int, Sector>> sectorsWithObjects = Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

        //    string userReply;
        //    this.Game.Write.Line("");
        //    this.Game.Write.PromptUser("Enter number to lock Phasers: ", out userReply);

        //    int number = Convert.ToInt32(userReply);
        //    var objectToFireOn = sectorsWithObjects.Single(i => i.Key == number).Value;

        //    int phaserEnergy;
        //    if (!this.PromptUserForPhaserEnergy(out phaserEnergy))
        //    {
        //        this.Game.Write.Line("Invalid phaser energy level.");
        //        return;
        //    }

        //    if (!this.EnergyCheckFail(phaserEnergy, this.ShipConnectedTo))
        //    {
        //        this.ShipConnectedTo.Energy -= phaserEnergy;

        //        var destroyedShips = new List<IShip>();
        //        switch (objectToFireOn.Item)
        //        {
        //            case SectorItem.HostileShip:
        //            case SectorItem.FriendlyShip:
        //                this.FireOnShip(0, (IShip)objectToFireOn.Object, this.InNebula(), destroyedShips);
        //                break;

        //            case SectorItem.Starbase:
        //                //this.FireOnStarbase(0, (IShip)objectToFireOn.Object, this.InNebula(), destroyedShips);
        //                this.Game.Write.Line("Firing on Starbases unsupported with phasers (for now).");
        //                break;

        //            case SectorItem.Star:
        //                this.FireOnStar((IStar)objectToFireOn.Object);
        //                break;
        //        }

        //        this.Game.ALLHostilesAttack(this.Game.Map); //todo: this can't stay here becouse if an enemy ship has phasers, this will have an indefinite loop.  to fix, we should probably pass back phaserenergy success, and do the output. later.
        //    }
        }

        private void FireOnStar(IStar iStar)
        {
            this.Prompt.Line("");
            this.Prompt.Line($"Direct hit on {iStar.Name}. No apparent damage to Stellar Body.");
        }
    }
}
