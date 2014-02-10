﻿using System;
using StarTrek_KG.Actors;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Disruptors : SubSystem_Base
    {
        //this.Initialize();
        public Disruptors(Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
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

            Ship target; //= Object in sector with targetLocation.sectorX, targetLocation.sectorY

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
    }
}
