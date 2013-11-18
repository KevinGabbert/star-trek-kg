﻿using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Torpedoes : SubSystem_Base, IMap //, IDestructionCheck
    {
        #region Properties

            public int Count { get; set; }

        #endregion

        public Torpedoes(Map map)
        {
            this.Map = map;
            this.Type = SubsystemType.Torpedoes;
        }

        public override void OutputDamagedMessage()
        {
            Output.Write("Photon torpedo control is damaged. Repairs are underway.");
        }

        public override void OutputRepairedMessage()
        {
            Output.Write("Photon torpedo controls have been repaired.");
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public override void Controls(string command)
        {
            this.Controls(this.Map);
        }

        public void Controls(Map map)
        {
            if (Damaged() || Exhausted() || Quadrants.NoHostiles(map.Quadrants.Hostiles)) return;

            double direction;
            if (!Command.PromptUser("Enter firing direction (1.0--9.0): ", out direction)
                || direction < 1.0 || direction > 9.0)
            {
                Output.Write("Invalid direction.");
                return;
            }

            Output.Write("Photon torpedo fired...");
            this.Count--;

            var angle = ComputeAngle(map, direction);

            Location location = map.Playership.GetLocation();

            double x = location.Quadrant.X;
            double y = location.Quadrant.Y;

            var vx = Math.Cos(angle)/20;
            var vy = Math.Sin(angle)/20;
            int lastX = -1, lastY = -1;
            while (x >= 0 && 
                   y >= 0 && 
                   Math.Round(x) < Constants.SECTOR_MAX && 
                   Math.Round(y) < Constants.SECTOR_MAX)
            {
                var newX = (int) Math.Round(x);
                var newY = (int) Math.Round(y);
                if (lastX != newX || lastY != newY)
                {
                    Console.WriteLine("  [{0},{1}]", newX, newY);
                    lastX = newX;
                    lastY = newY;
                }

                var hostilesInSector = map.Quadrants.Hostiles.Where(ship => ship.Sector.X == newX && ship.Sector.Y == newY);

                if (Torpedoes.HitBaddies(map, hostilesInSector)) goto label;
                if (Torpedoes.HitSomethingElse(map, vx, vy, location, newY, newX, ref x, ref y)) goto label;
            }

            Output.Write("Photon torpedo failed to hit anything.");

            label:

            if (map.Quadrants.Hostiles.Count > 0)
            {
                map.Quadrants.ALLHostilesAttack(map);
            }
        }

        private static bool HitSomethingElse(Map map, double vx, double vy, 
                                              Location location, 
                                              int newY, 
                                              int newX,
                                              ref double x,
                                              ref double y)
        {

            //todo: move this code out of the function and pass location as Sector instead of a Navigation object
            Quadrant quadrant = Quadrants.Get(map, location.Quadrant.X, location.Quadrant.Y);
            Sector qLocation = null; // = quadrant.Map.Sectors.Where(s => s.X == newX && s.Y == newY).Single();

            switch (qLocation.Item)
            {
                case SectorItem.Starbase:
                    map.starbases--;

                    //quadrant.Starbase = false;
                    //quadrant.Map.Sectors.Where(s => s.X == newX && s.Y == newY).Single().Item = SectorItem.Empty;

                    Console.WriteLine(map.Playership.Name + " destroyed a Federation starbase at sector [{0},{1}]!",
                                      newX, newY);
                    return true;

                case SectorItem.Star:
                    Console.WriteLine(
                        "The torpedo was captured by a star's gravitational field at sector [{0},{1}].",
                        newX, newY);

                    return true;
            }

            x += vx;
            y += vy;
            return false;
        }

        private static bool HitBaddies(Map map, IEnumerable<Ship> query)
        {
            foreach (var ship in query)
            {
                Console.WriteLine("Hostile ship destroyed at sector [{0},{1}].", (ship.Sector.X), (ship.Sector.Y));

                map.Quadrants.Remove(ship, map);

                return true;
            }

            return false;
        }

        private bool Exhausted()
        {
            if (this.Count == 0)
            {
                Output.Write("Photon torpedoes exhausted.");
                return true;
            }
            return false;
        }

        public void Calculator(Map map)
        {
            Console.WriteLine();
            if (map.Quadrants.GetActive().Hostiles.Count == 0)
            {
                Console.WriteLine("There are no Hostile ships in this quadrant.");
                Console.WriteLine();
                return;
            }

            Location location = map.Playership.GetLocation(); 

            foreach (var ship in map.Quadrants.Hostiles)
            {
                Console.WriteLine("Direction {2:#.##}: Hostile ship in sector [{0},{1}].",
                                  (ship.Sector.X), (ship.Sector.Y),
                                  Map.ComputeDirection(location.Sector.X, location.Sector.Y, ship.Sector.X, ship.Sector.Y));
            }
        }

        private static double ComputeAngle(Map map, double direction) // todo: can this be refactored with nav computeangle?
        {
            var angle = -(Math.PI*(direction - 1.0)/4.0);
            if ((Utility.Random).Next(3) == 0)
            {
                angle += ((1.0 - 2.0*(Utility.Random).NextDouble())*Math.PI*2.0)*0.03;
            }
            return angle;
        }

        public new static Torpedoes For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Torpedoes). Add a Friendly to your GameConfig"); //todo: make this a custom exception
            }

            return (Torpedoes)ship.Subsystems.Single(s => s.Type == SubsystemType.Torpedoes);
        }
    }
}
