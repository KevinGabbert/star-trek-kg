﻿using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Actors
{
    public class ImpulseActor : IInteractContainer
    {
        public IInteraction Interact { get; set; }

        public ImpulseActor(IInteraction interaction)
        {
            this.Interact = interaction;

            if (this.Interact == null)
            {
                throw new GameException("Property Write is not set for Impulse. ");
            }
        }

        /// <summary>
        /// Checks for energy required.  Subracts energy required for warp.
        /// </summary>
        /// <param name="ship"> </param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private bool EnergySubtracted(IShip ship, ref int distance)
        {
            bool returnVal;

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
        public bool InvalidSublightFactorCheck(int maxSublightDistance, out string distance)
        {
            if (!this.Interact.PromptUser(SubsystemType.None, "Impulse:>", $"Enter Sublight distance (1-{maxSublightDistance}): ", out distance, this.Interact.Output.Queue)
                || int.Parse(distance) < 0
                || int.Parse(distance) > maxSublightDistance)
            {
                this.Interact.Line("Invalid sublight distance. Maximum distance is " + maxSublightDistance + " at this time.");
                return true;
            }
            return false;
        }
        public bool Engage(NavDirection direction, int distance, out int lastRegionY, out int lastRegionX, IMap map)
        {
            var success = this.EnergySubtracted(map.Playership, ref distance);

            if (success)
            {
                Navigation.For(map.Playership).Movement.Execute(MovementType.Impulse, direction, distance, out lastRegionX, out lastRegionY);
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
