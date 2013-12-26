using System;
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

        public Torpedoes(Map map, Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
            this.Map = map;
            this.Type = SubsystemType.Torpedoes;
        }

        public override void OutputDamagedMessage()
        {
            Output.Write.Line("Photon torpedo control is damaged. Repairs are underway. ");
        }

        public override void OutputRepairedMessage()
        {
            Output.Write.Line("Photon torpedo controls have been repaired. ");
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public override void Controls(string command)
        {
            this.Controls();
        }

        public void Controls()
        {
            if (this.Damaged() || 
                this.Exhausted() || 
                Quadrants.NoHostiles(this.Map.Quadrants.GetHostiles())) return;

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
                Output.Write.Line("Invalid direction.");
                return;
            }

            Output.Write.Line("Photon torpedo fired...");
            this.Count--;

            var angle = Utility.Utility.ComputeAngle(this.Map, direction);

            Location location = this.ShipConnectedTo.GetLocation();

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
                    Output.Write.Line(string.Format("  [{0},{1}]", newX, newY));
                    lastX = newX;
                    lastY = newY;
                }

                //todo: query map.quadrants for ship
                
                var hostilesInQuadrant = this.Map.Quadrants.GetActive().GetHostiles();
                var hostilesInSector = this.Map.Quadrants.GetActive().GetHostiles().Where(hostileShip =>
                                                                                          hostileShip.Sector.X == newX &&
                                                                                          hostileShip.Sector.Y == newY);

                if (Map.DestroyedBaddies(this.Map, hostilesInSector) || 
                   (Torpedoes.HitSomethingElse(this.Map, vx, vy, location, newY, newX, ref x, ref y)))
                {
                    Game.ALLHostilesAttack(this.Map);
                    return;
                }
            }

            Output.Write.Line("Photon torpedo failed to hit anything.");
        }

        private static bool HitSomethingElse(Map map, double vx, double vy, 
                                              Location location, 
                                              int newY, 
                                              int newX,
                                              ref double x,
                                              ref double y)
        {

            //todo: move this code out of the function and pass location as Sector instead of a Navigation object
            Quadrant quadrant = Quadrants.Get(map, location.Quadrant);
            Sector qLocation = quadrant.Sectors.Single(s => s.X == newX && s.Y == newY);

            switch (qLocation.Item)
            {
                case SectorItem.Starbase:
                    map.starbases--;

                    //quadrant.Starbase = false;
                    //quadrant.Map.Sectors.Where(s => s.X == newX && s.Y == newY).Single().Item = SectorItem.Empty;

                    Output.Write.Line(string.Format("A Federation starbase at sector [{0},{1}] has been destroyed!",
                                      newX, newY));
                    return true;

                case SectorItem.Star:

                    var star = ((Star) qLocation.Object);

                    Output.Write.Line(string.Format(
                        "The torpedo was captured by the gravitational field of star: " + star.Name + " at sector [{0},{1}].",
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
                Output.Write.Line("Photon torpedoes exhausted.");
                return true;
            }
            return false;
        }

        //todo: move to Utility() object
        public void Calculator(Map map)
        {
            Output.Write.Line("");
            if (map.Quadrants.GetActive().GetHostiles().Count == 0)
            {
                Output.Write.Line("There are no Hostile ships in this quadrant.");
                Output.Write.Line("");
                return;
            }

            Location location = this.ShipConnectedTo.GetLocation(); 

            foreach (var ship in map.Quadrants.GetHostiles())
            {
                Output.Write.Line(string.Format("Direction {2:#.##}: Hostile ship in sector [{0},{1}].",
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
