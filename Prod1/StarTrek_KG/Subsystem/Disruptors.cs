using System;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Disruptors : SubSystem_Base, IMap, IWeapon
    {
        public Disruptors(Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
        }

        public override void OutputDamagedMessage()
        {
            throw new NotImplementedException();
        }

        public override void OutputRepairedMessage()
        {
            throw new NotImplementedException();
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function represents the amount of energy fired by an opposing ship.
        /// The value is a seeded random number that decreases by distance.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static int Shoot(double distance)
        {
            //todo: give ship a disruptor weapon type, enable it only on hostileType.Klingon.  delete this.

            var seed = StarTrekKGSettings.GetSetting<int>("DisruptorShotSeed"); //todo: pull from config
            var distanceDeprecationLevel = StarTrekKGSettings.GetSetting<double>("DisruptorShotDeprecationLevel"); //todo: pull deprecationlevel from config

            var adjustedDisruptorEnergy = (StarTrekKGSettings.GetSetting<double>("DisruptorEnergyAdjustment") - distance / distanceDeprecationLevel);
            var deliveredEnergy = (int)(seed * (Utility.Utility.Random).NextDouble() * adjustedDisruptorEnergy);

            return deliveredEnergy;
        }



        //todo: Under Construction
        //This will be the first subsystem that will 
        public int Shoot(Location targetLocation) //Playership.GetLocation();
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

            //var attackingEnergy = Disruptors.For(thisShip).Shoot(distance);

            //var shieldsValueBeforeHit = Shields.For(thisShip).Energy;

            //target.AbsorbHitFrom(this, attackingEnergy);


            //whoever called this function might need to tell this info to the user
            //Game.ReportShieldsStatus(map, shieldsValueBeforeHit);

            return 0;
        }
    }
}
