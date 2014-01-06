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

            Location torpedoStartingLocation = this.ShipConnectedTo.GetLocation();
            Quadrant quadrant = Quadrants.Get(this.Map, torpedoStartingLocation.Quadrant);

            var currentLocation = new WeaponCoordinate(torpedoStartingLocation.Sector);
            var torpedoVector = new WeaponCoordinate(Math.Cos(angle) / 20, Math.Sin(angle) / 20);

            //TODO: WRITE SOME TORPEDO TESTS!

            //todo: add in a constructor to turn off coordinate bounds checking for this object only
            //either that, or come up with a null location so that the first WHILE will work
            var lastPosition = new Coordinate(-1, -1, false); 

            var newLocation = new Location();
            newLocation.Quadrant = quadrant;
            newLocation.Sector = new Sector();

            //todo: condense WHILE to be a function of Coordinate
            //todo: eliminate the twice rounding of torpedo location, as the same value is evaluated twice
            //todo: the rounding can happen once in a variable, and then referred to twice (see note below)
            while (Torpedoes.IsInQuadrant(currentLocation))
            {
                //Increment to next Sector
                if (this.HitSomething(currentLocation, lastPosition, newLocation, torpedoVector))
                {
                    return;
                }
            }

            Output.Write.Line("Photon torpedo failed to hit anything.");
        }

        private bool HitSomething(WeaponCoordinate currentLocation, Coordinate lastPosition, Location newLocation, WeaponCoordinate torpedoVector)
        {
            newLocation.Sector.IncrementBy(currentLocation);

            //todo: Condense into function of Coordinate
            if (Torpedoes.LastPositionAintNewPosition(newLocation, lastPosition))
            {
                Output.Write.Line(string.Format("  [{0},{1}]", newLocation.Sector.X, newLocation.Sector.Y));
                lastPosition.Update(newLocation);
            }

            Torpedoes.DebugTrack(newLocation);

            if (this.HitSomething(newLocation))
            {
                return true;
            }

            //todo: How about storing a *rounded* XY that is referred to by the While, and the new SectorToCheck
            currentLocation.IncrementBy(torpedoVector);
            return false;
        }

        private static bool LastPositionAintNewPosition(Location newTorpedoLocation, Coordinate torpedoLastPosition)
        {
            return torpedoLastPosition.X != newTorpedoLocation.Sector.X || torpedoLastPosition.Y != newTorpedoLocation.Sector.Y;
        }

        private static bool IsInQuadrant(WeaponCoordinate torpedoLocation)
        {
            return torpedoLocation.X >= Constants.SECTOR_MIN &&
                   torpedoLocation.Y >= Constants.SECTOR_MIN &&
                   Math.Round(torpedoLocation.X) < Constants.SECTOR_MAX &&
                   Math.Round(torpedoLocation.Y) < Constants.SECTOR_MAX;
        }

        private bool HitSomething(Location location)
        {
            if (this.HitHostile(location.Sector.Y, location.Sector.X))
            {
                Game.ALLHostilesAttack(this.Map);
                return true;
            }

            if (Torpedoes.HitSomethingElse(this.Map, location.Quadrant, location.Sector.Y, location.Sector.X))
            {
                Game.ALLHostilesAttack(this.Map);
                return true;
            }
            return false;
        }

        private bool HitHostile(int newY, int newX)
        {
            var thisQuadrant = this.Map.Quadrants.GetActive();
            var hostilesInQuadrant = thisQuadrant.GetHostiles();
            IShip hostileInSector = hostilesInQuadrant.SingleOrDefault(hostileShip => hostileShip.Sector.X == newX &&
                                                                                      hostileShip.Sector.Y == newY);

            if (hostileInSector != null)
            {
                Map.RemoveTargetFromSector(this.Map, hostileInSector);
                return true;
            }

            return false;
        }

        private static void DebugTrack(Location newLocation)
        {
            if (Constants.DEBUG_MODE)
            {
                Sector qLocation = newLocation.Quadrant.Sectors.Single(s => s.X == newLocation.Sector.X && s.Y == newLocation.Sector.Y);

                if (qLocation.Item == SectorItem.Empty)
                {
                    qLocation.Item = SectorItem.Debug;
                }
            }
        }

        private static bool HitSomethingElse(Map map,
                                              Quadrant quadrant, 
                                              int newY, 
                                              int newX)
        {

            //todo: move this code out of the function and pass location as Sector instead of a Navigation object
            Sector qLocation = quadrant.Sectors.Single(s => s.X == newX && s.Y == newY);

            switch (qLocation.Item)
            {
                case SectorItem.Starbase:
                    map.starbases--;

                    qLocation.Object = null;
                    qLocation.Item = SectorItem.Empty;

                    //yeah. How come a starbase can protect your from baddies but one torpedo hit takes it out?
                    Output.Write.Line(string.Format("A Federation starbase at sector [{0},{1}] has been destroyed!",
                                                    newX, newY));

                    //todo: When the Starbase is a full object, then allow the torpedoes to either lower its shields, or take out subsystems.
                    //todo: a concerted effort of 4? torpedoes will destroy an unshielded starbase.
                    //todo: however, you'd better hit the comms subsystem to prevent an emergency message, then shoot the log bouy
                    //todo: it sends out or other starbases will know of your crime.

                    return true;

                case SectorItem.Star:

                    var star = ((Star) qLocation.Object);

                    var starName = "UNKNOWN";

                    if(star != null)
                    {
                        starName = star.Name;
                    }

                    Output.Write.Line(string.Format(
                        "The torpedo was captured by the gravitational field of star: " + starName +
                        " at sector [{0},{1}].",
                        newX, newY));

                    return true;
            }

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

            var thisQuadrant = this.ShipConnectedTo.GetQuadrant();
            var thisQuadrantHostiles = thisQuadrant.GetHostiles();

            if (thisQuadrantHostiles.Count == 0)
            {
                Output.Write.Line("There are no Hostile ships in this quadrant.");
                Output.Write.Line("");
                return;
            }

            Location location = this.ShipConnectedTo.GetLocation();

            foreach (var ship in thisQuadrantHostiles)
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
