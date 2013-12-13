using System;
using StarTrek_KG.Config;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Subsystem
{
    public class Disruptors : SubSystem_Base, IMap, IWeapon
    {
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
    }
}
