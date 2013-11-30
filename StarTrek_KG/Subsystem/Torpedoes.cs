using System;
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
            Output.WriteLine("Photon torpedo control is damaged. Repairs are underway.");
        }

        public override void OutputRepairedMessage()
        {
            Output.WriteLine("Photon torpedo controls have been repaired.");
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
            if (Damaged() || Exhausted() || Quadrants.NoHostiles(map.Quadrants.GetHostiles())) return;

            double direction;
            if (!Command.PromptUser("Enter firing direction (1.0--9.0): ", out direction)
                || direction < 1.0 || direction > 9.0)
            {
                Output.WriteLine("Invalid direction.");
                return;
            }

            Output.WriteLine("Photon torpedo fired...");
            this.Count--;

            var angle = Utility.ComputeAngle(map, direction);

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

                var hostilesInSector = map.Quadrants.GetHostiles().Where(ship => ship.Sector.X == newX && ship.Sector.Y == newY);

                if (Map.DestroyedBaddies(map, hostilesInSector)) goto label;
                if (Torpedoes.HitSomethingElse(map, vx, vy, location, newY, newX, ref x, ref y)) goto label;
            }

            Output.WriteLine("Photon torpedo failed to hit anything.");

            label:

            if (map.Quadrants.GetHostiles().Count > 0)
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

        private bool Exhausted()
        {
            if (this.Count == 0)
            {
                Output.WriteLine("Photon torpedoes exhausted.");
                return true;
            }
            return false;
        }

        public void Calculator(Map map)
        {
            Console.WriteLine();
            if (map.Quadrants.GetActive().GetHostiles().Count == 0)
            {
                Output.WriteLine("There are no Hostile ships in this quadrant.");
                Output.WriteLine("");
                return;
            }

            Location location = map.Playership.GetLocation(); 

            foreach (var ship in map.Quadrants.GetHostiles())
            {
                Console.WriteLine("Direction {2:#.##}: Hostile ship in sector [{0},{1}].",
                                  (ship.Sector.X), (ship.Sector.Y),
                                  Utility.ComputeDirection(location.Sector.X, location.Sector.Y, ship.Sector.X, ship.Sector.Y));
            }
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
