﻿using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Torpedoes : SubSystem_Base, IMap 
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
            Output.Output.WriteLine("Photon torpedo control is damaged. Repairs are underway. ");
        }

        public override void OutputRepairedMessage()
        {
            Output.Output.WriteLine("Photon torpedo controls have been repaired. ");
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
            if (this.Damaged() || 
                this.Exhausted() || 
                Quadrants.NoHostiles(map.Quadrants.GetHostiles())) return;

            var firingDirection = Environment.NewLine +
                                  " 4   5   6 " + Environment.NewLine +
                                 @"   \ ↑ /  " + Environment.NewLine +
                                  "3 ← <*> → 7" + Environment.NewLine +
                                 @"   / ↓ \  " + Environment.NewLine +
                                  " 2   1   8" + Environment.NewLine +
                                  Environment.NewLine +
                                  "Enter firing direction (1.0--9.0) ";

            double direction;
            if (!Command.PromptUser(firingDirection, out direction)
                || direction < 1.0 
                || direction > 9.0)
            {
                Output.Output.WriteLine("Invalid direction.");
                return;
            }

            Output.Output.WriteLine("Photon torpedo fired...");
            this.Count--;

            var angle = Utility.Utility.ComputeAngle(map, direction);

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
                    Output.Output.WriteLine(string.Format("  [{0},{1}]", newX, newY));
                    lastX = newX;
                    lastY = newY;
                }

                //todo: query map.quadrants for ship

                var hostilesInSector = map.Quadrants.GetActive().GetHostiles().Where(ship => 
                                                                             ship.Sector.X == newX && 
                                                                             ship.Sector.Y == newY);

                if (Map.DestroyedBaddies(map, hostilesInSector) || 
                   (Torpedoes.HitSomethingElse(map, vx, vy, location, newY, newX, ref x, ref y)))
                {
                    this.AllHostilesAttack(map);
                    return;
                }
            }

            Output.Output.WriteLine("Photon torpedo failed to hit anything.");
        }

        //todo: refactor into to Game() object. one exists there
        private void AllHostilesAttack(Map map)
        {
            if (map.Quadrants.GetHostiles().Count > 0)
            {
                Game.ALLHostilesAttack(map);
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
            Sector qLocation = quadrant.Sectors.Where(s => s.X == newX && s.Y == newY).Single();

            switch (qLocation.Item)
            {
                case SectorItem.Starbase:
                    map.starbases--;

                    //quadrant.Starbase = false;
                    //quadrant.Map.Sectors.Where(s => s.X == newX && s.Y == newY).Single().Item = SectorItem.Empty;

                    Output.Output.WriteLine(string.Format(map.Playership.Name + " destroyed a Federation starbase at sector [{0},{1}]!",
                                      newX, newY));
                    return true;

                case SectorItem.Star:
                    Output.Output.WriteLine(string.Format(
                        "The torpedo was captured by a star's gravitational field at sector [{0},{1}].",
                        newX, newY));

                    return true;
            }

            x += vx;
            y += vy;
            return false;
        }

        private bool Exhausted()
        {
            if (this.Count == 0)
            {
                Output.Output.WriteLine("Photon torpedoes exhausted.");
                return true;
            }
            return false;
        }

        //todo: move to Utility() object
        public void Calculator(Map map)
        {
            Output.Output.WriteLine("");
            if (map.Quadrants.GetActive().GetHostiles().Count == 0)
            {
                Output.Output.WriteLine("There are no Hostile ships in this quadrant.");
                Output.Output.WriteLine("");
                return;
            }

            Location location = map.Playership.GetLocation(); 

            foreach (var ship in map.Quadrants.GetHostiles())
            {
                Output.Output.WriteLine(string.Format("Direction {2:#.##}: Hostile ship in sector [{0},{1}].",
                                  (ship.Sector.X), (ship.Sector.Y),
                                  Utility.Utility.ComputeDirection(location.Sector.X, location.Sector.Y, ship.Sector.X, ship.Sector.Y)));
            }
        }

        public new static Torpedoes For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Torpedoes).");   //todo: reflect the name and refactor this to ISubsystem
            }

            return (Torpedoes)ship.Subsystems.Single(s => s.Type == SubsystemType.Torpedoes);
        }
    }
}
