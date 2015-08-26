using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Actors
{
    public class Warp: IWrite
    {
        public IOutputWrite Write { get; set; }

        public Warp(IOutputWrite output)
        {
            this.Write = output;

            if (this.Write == null)
            {
                throw new GameException("Property Write is not set for Warp. ");
            }
        }

        /// <summary>
        /// Checks for energy required.  Subracts energy required for warp.
        /// </summary>
        /// <param name="ship"> </param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool EnergySubtracted(Ship ship, ref int distance)
        {
            bool returnVal;

            //app.config (max warp setting - set in initialize ship)
            if (distance < 1)
            {
                distance *= 10; //Constants.SECTOR_MAX; // this computation ensures that any movement inside of a single sector is *free*, meaning no energy consumed
            }

            var energyRequired = distance; //rounds down for values < 1, meaning a distance of .1 is free
            if (energyRequired >= ship.Energy) //todo: change this to ship.energy
            {
                this.Write.Line("Insufficient energy to travel that speed.");
                returnVal = false;
            }
            else
            {
                ship.Energy -= energyRequired;
                returnVal = true;
            }

            this.Write.Line("");

            return returnVal;
        }
        public bool PromptAndCheckForInvalidWarpFactor(int maxWarpFactor, out int distance)
        {
            if (!this.Write.PromptUser(SubsystemType.None, $"Enter warp factor (1-{maxWarpFactor}): ", out distance)
                || distance < 0 
                || distance > maxWarpFactor)
            {
                this.Write.Line("Invalid warp factor. Maximum Warp is " + maxWarpFactor + " at this time.");
                return true;
            }
            return false;
        }
        public bool Engage(int direction, int distance, out int lastRegionY, out int lastRegionX, IMap map)
        {
            var success = this.EnergySubtracted(map.Playership, ref distance);

            if (success)
            {
                Navigation.For(map.Playership).Movement.Execute(MovementType.Warp, direction, distance, out lastRegionX, out lastRegionY);
            }
            else
            {
                lastRegionX = 0;
                lastRegionY = 0;
            }

            return success;
        }
    }
}
