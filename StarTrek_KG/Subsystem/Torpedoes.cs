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

            var torpedoLocation = new FiringCoordinate(torpedoStartingLocation.Sector);
            var weaponAngle = new FiringCoordinate(Math.Cos(angle) / 20, Math.Sin(angle) / 20);


            //TODO: WRITE SOME TORPEDO TESTS!

            //todo: add in a constructor to turn off coordinate bounds checking for this object only
            //either that, or come up with a null location so that the first WHILE will work
            //var torpedoLastSector = new Coordinate(-1, -1); 
            int lastSector_X = -1;
            int lastSector_Y = -1;

            //var sectorToCheck = new Coordinate();

            //todo: condense WHILE to be a function of Coordinate
            //todo: eliminate the twice rounding of torpedo location, as the same value is evaluated twice
            //todo: the rounding can happen once in a variable, and then referred to twice (see note below)
            while (torpedoLocation.X >= 0 &&
                   torpedoLocation.Y >= 0 &&
                   Math.Round(torpedoLocation.X) < Constants.SECTOR_MAX &&
                   Math.Round(torpedoLocation.Y) < Constants.SECTOR_MAX)
            {

                //todo: eliminate the rounding here
                //todo: we don't need to create a NEW object here, do we? how about just replacing the prop values?
                var sectorToCheck = new Coordinate((int)Math.Round(torpedoLocation.X), (int)Math.Round(torpedoLocation.Y));

                //todo: Condense into function of Coordinate
                if (lastSector_X != sectorToCheck.X || lastSector_Y != sectorToCheck.Y)
                {
                    Output.Write.Line(string.Format("  [{0},{1}]", sectorToCheck.X, sectorToCheck.Y));
                    lastSector_X = sectorToCheck.X;
                    lastSector_Y = sectorToCheck.Y;
                }

                //todo: pass Location object
                this.DebugTorpedoTrack(sectorToCheck.X, sectorToCheck.Y, quadrant);

                //todo: pass location object
                if (this.HitSomethingInSector(quadrant, sectorToCheck.Y, sectorToCheck.X))
                {
                    return;
                }

                //todo: condense into function of coordinate  (.Increment())
                //todo: How about storing a *rounded* XY that is referred to by the While, and the new SectorToCheck
                torpedoLocation.X += weaponAngle.X;
                torpedoLocation.Y += weaponAngle.Y;
            }

            Output.Write.Line("Photon torpedo failed to hit anything.");
        }

        private bool HitSomethingInSector(Quadrant quadrant, int newY, int newX)
        {
            if (this.HitHostile(newY, newX))
            {
                Game.ALLHostilesAttack(this.Map);
                return true;
            }

            if (Torpedoes.HitSomethingElse(this.Map, quadrant, newY, newX))
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

        private void DebugTorpedoTrack(int newX, int newY, Quadrant quadrant)
        {
            if (Constants.DEBUG_MODE)
            {
                Sector qLocation = quadrant.Sectors.Single(s => s.X == newX && s.Y == newY);

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
