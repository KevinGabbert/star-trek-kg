using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Actors
{
    public class WarpActor: IInteractContainer
    {
        public IInteraction Interact { get; set; }

        public WarpActor(IInteraction interaction)
        {
            this.Interact = interaction;

            if (this.Interact == null)
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
        public bool EnergySubtracted(IShip ship, ref int distance)
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
                this.Interact.Line("Insufficient energy to travel that speed.");
                returnVal = false;
            }
            else
            {
                ship.Energy -= energyRequired;
                returnVal = true;
            }

            this.Interact.Line("");

            return returnVal;
        }
        public bool PromptAndCheckForInvalidWarpFactor(int maxWarpFactor, out string distance)
        {
            if (!this.Interact.PromptUser(SubsystemType.None, "Warp:>", $"Enter warp factor (1-{maxWarpFactor}): ", out distance, this.Interact.Output.Queue))
            {
                return true;
            }

            // If prompt is being set (web flow), distance will not be a user value yet.
            if (string.IsNullOrWhiteSpace(distance) || distance == "-1")
            {
                return true;
            }

            if (distance.Contains("."))
            {
                this.Interact.Line("Invalid warp factor. Maximum Warp is " + maxWarpFactor + " at this time.");
                this.Interact.PromptUser(SubsystemType.None, "Warp:>", $"Enter warp factor (1-{maxWarpFactor}): ", out distance, this.Interact.Output.Queue);
                return true;
            }

            int warpFactor;
            if (!int.TryParse(distance, out warpFactor) || warpFactor < 1 || warpFactor > maxWarpFactor)
            {
                this.Interact.Line("Invalid warp factor. Maximum Warp is " + maxWarpFactor + " at this time.");
                this.Interact.PromptUser(SubsystemType.None, "Warp:>", $"Enter warp factor (1-{maxWarpFactor}): ", out distance, this.Interact.Output.Queue);
                return true;
            }

            return false;
        }
        public bool Engage(NavDirection direction, int distance, out int lastRegionY, out int lastRegionX, IMap map)
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



        //Interface:

        //private void GetValueFromUser(string subCommand)
    }
}
