using System;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Actors
{
    public class Warp
    {
        /// <summary>
        /// Checks for energy required.  Subracts energy required for warp.
        /// </summary>
        /// <param name="ship"> </param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool EnergySubtracted(Ship ship, ref double distance)
        {
            bool returnVal;

            //app.config (max warp setting - set in initialize ship)
            distance *= Constants.SECTOR_MAX; // this computation ensures that any movement inside of a single sector is *free*, meaning no energy consumed

            var energyRequired = (int)distance; //rounds down for values < 1, meaning a distance of .1 is free
            if (energyRequired >= ship.Energy) //todo: change this to ship.energy
            {
                Output.Write.Line("Insufficient energy to travel that speed.");
                returnVal = false;
            }
            else
            {
                ship.Energy -= energyRequired;
                returnVal = true;
            }

            Output.Write.Line("");

            return returnVal;
        }
        public bool InvalidWarpFactorCheck(double maxWarpFactor, out double distance)
        {
            if (!Command.PromptUser(String.Format("Enter warp factor (0.1--{0}): ", maxWarpFactor), out distance)
                || distance < 0.1 
                || distance > maxWarpFactor)
            {
                Output.Write.Line("Invalid warp factor. Maximum Warp is " + maxWarpFactor + " at this time.");
                return true;
            }
            return false;
        }
        public bool EngageWarp(double direction, double distance, out int lastQuadY, out int lastQuadX, Map map)
        {
            var distanceEntered = distance;
            var success = this.EnergySubtracted(map.Playership, ref distance);

            if (success)
            {
                Navigation.For(map.Playership).Movement.Execute(direction, distance, distanceEntered, out lastQuadX, out lastQuadY);
            }
            else
            {
                lastQuadX = 0;
                lastQuadY = 0;
            }

            return success;
        }
    }
}
