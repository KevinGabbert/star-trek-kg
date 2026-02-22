using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Torpedoes : SubSystem_Base, IWeapon
    {
        #region Properties

            public int Count { get; set; }

        #endregion

        public Torpedoes(IShip shipConnectedTo): base(shipConnectedTo)
        {
            this.Type = SubsystemType.Torpedoes; //for lookup
        }

        public override List<string> Controls(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return this.Controls();
            }

            int directionValue;
            if (!int.TryParse(command, out directionValue) || directionValue < 1 || directionValue > 9)
            {
                this.ShipConnectedTo.OutputLine("Invalid direction.");
                this.PromptForDirection();
                return this.ShipConnectedTo.OutputQueue();
            }

            this.Shoot(directionValue);
            this.ShipConnectedTo.ResetPrompt();
            return this.ShipConnectedTo.OutputQueue();
        }

        public List<string> Controls()
        {
            IGame game = this.ShipConnectedTo.Map.Game;
            IInteraction prompt = game.Interact;

            prompt.Output.Queue.Clear();

            List<string> hostileCheckOutput;

            if (this.Damaged() || 
                this.Exhausted() || 
                new Sectors(game.Map, prompt).NoHostiles(game.Map.Sectors.GetHostiles(), out hostileCheckOutput)) return prompt.Output.Queue.ToList();

            prompt.Output.Write(hostileCheckOutput);

            //todo: resource this out.
            var firingDirection = Output.Interaction.COURSE_GRID +
                                  "Enter firing direction (1.0--9.0) ";

            string direction;
            if (!prompt.PromptUser(SubsystemType.Torpedoes, "Torpedoes->", firingDirection, out direction, prompt.Output.Queue, 1))
            {
                return prompt.Output.Queue.ToList();
            }

            if (direction == "-1")
            {
                return prompt.Output.Queue.ToList();
            }

            int directionValue;
            if (!int.TryParse(direction, out directionValue) || directionValue < 1 || directionValue > 9)
            {
                prompt.Line("Invalid direction.");
                return prompt.Output.Queue.ToList();
            }

            this.Shoot(directionValue);

            return prompt.Output.Queue.ToList();
        }

        private void PromptForDirection()
        {
            string throwaway;
            IInteraction prompt = this.ShipConnectedTo.Map.Game.Interact;
            prompt.PromptUser(
                SubsystemType.Torpedoes,
                "Torpedoes->",
                this.BuildFiringDirectionPrompt(),
                out throwaway,
                prompt.Output.Queue,
                1);
        }

        private string BuildFiringDirectionPrompt()
        {
            return Output.Interaction.COURSE_GRID +
                   "Enter firing direction (1.0--9.0) ";
        }

        public void Shoot(double direction)
        {
            IGame game = this.ShipConnectedTo.Map.Game;
            if (this.Count < 1)
            {
                this.ShipConnectedTo.OutputLine("Cannot fire.  Torpedo Room reports no Torpedoes to fire.");
                return;
            }

            game.ALLHostilesAttack(game.Map);

            Location torpedoStartingLocation = this.ShipConnectedTo.GetLocation();
            Sector Sector = game.Map.Sectors[torpedoStartingLocation.Sector];

            //var currentLocation = new VectorCoordinate(torpedoStartingLocation.Coordinate);
            //var torpedoVector = new VectorCoordinate(Math.Cos(angle)/20, Math.Sin(angle)/20);

            this.ShipConnectedTo.OutputLine("Photon torpedo fired...");
            this.Count--;

            //TODO: WRITE SOME TORPEDO TESTS!

            //todo: add in a constructor to turn off coordinate bounds checking for this object only
            //either that, or come up with a null location so that the first WHILE will work
            var lastPosition = new Point(-1, -1);

            var newLocation = new Location
            {
                Sector = Sector,
                Coordinate = new Coordinate()
            };

            var delta = GetDirectionDelta(direction);
            var min = DEFAULTS.COORDINATE_MIN;
            var maxExclusive = DEFAULTS.COORDINATE_MAX;

            var currentX = torpedoStartingLocation.Coordinate.X;
            var currentY = torpedoStartingLocation.Coordinate.Y;

            while (true)
            {
                currentX += delta.X;
                currentY += delta.Y;

                if (currentX < min || currentY < min || currentX >= maxExclusive || currentY >= maxExclusive)
                {
                    break;
                }

                newLocation.Coordinate.X = currentX;
                newLocation.Coordinate.Y = currentY;

                if (Torpedoes.LastPositionAintNewPosition(newLocation, lastPosition))
                {
                    lastPosition.Update(newLocation);
                }

                Torpedoes.DebugTrack(newLocation);

                if (this.HitSomething(newLocation))
                {
                    return;
                }
            }

            this.ShipConnectedTo.OutputLine("Photon torpedo failed to hit anything.");
        }

        private static Point GetDirectionDelta(double direction)
        {
            if (Math.Abs(direction - 9.0) < 0.001)
            {
                direction = 1.0;
            }

            switch ((int)Math.Round(direction))
            {
                case 1: // down
                    return new Point(0, 1);
                case 2: // down-left
                    return new Point(-1, 1);
                case 3: // left
                    return new Point(-1, 0);
                case 4: // up-left
                    return new Point(-1, -1);
                case 5: // up
                    return new Point(0, -1);
                case 6: // up-right
                    return new Point(1, -1);
                case 7: // right
                    return new Point(1, 0);
                case 8: // down-right
                    return new Point(1, 1);
                default:
                    return new Point(0, 0);
            }
        }

        //private bool HitSomething(VectorCoordinate currentLocation, Point lastPosition, Location newLocation)
        //{
        //    newLocation.Coordinate.IncrementBy(currentLocation);

        //    //todo: Condense into function of Point
        //    if (Torpedoes.LastPositionAintNewPosition(newLocation, lastPosition))
        //    {
        //        this.Game.Write.DebugLine(string.Format("  ~{0},{1}~", lastPosition.X, lastPosition.Y));

        //        var outputCoordinate = Utility.Utility.HideXorYIfNebula(this.ShipConnectedTo.GetSector(), newLocation.Coordinate.X.ToString(), newLocation.Coordinate.Y.ToString());

        //        this.Game.Write.Line(string.Format("  [{0},{1}]", outputCoordinate.X, outputCoordinate.Y));

        //        lastPosition.Update(newLocation);
        //    }

        //    Torpedoes.DebugTrack(newLocation);

        //    return this.HitSomething(newLocation);
        //}

        private static bool LastPositionAintNewPosition(Location newTorpedoLocation, Point torpedoLastPosition)
        {
            return torpedoLastPosition.X != newTorpedoLocation.Coordinate.X || torpedoLastPosition.Y != newTorpedoLocation.Coordinate.Y;
        }

        //private static bool IsInRegion(VectorCoordinate torpedoLocation)
        //{
        //    var torpedoXIsNotMin = torpedoLocation.X >= Constants.SECTOR_MIN;
        //    var torpedoYIsNotMIN = torpedoLocation.Y >= Constants.SECTOR_MIN;

        //    var torpedoXisNotMax = Math.Round(torpedoLocation.X) < Constants.SECTOR_MAX;
        //    var torpedoYisNotMax = Math.Round(torpedoLocation.Y) < Constants.SECTOR_MAX;

        //    //todo: look into issue:
        //    var xIsNotNegative = torpedoLocation.X > -1; //adjusting for an issue around sector 0,0
        //    var yIsNotNegative = torpedoLocation.Y > -1; //adjusting for an issue around sector 0,0

        //    var condition = (torpedoXIsNotMin || torpedoYIsNotMIN) &&
        //                    (torpedoXisNotMax && torpedoYisNotMax) &&
        //                    (xIsNotNegative && yIsNotNegative);

        //    return (condition);
        //}

        private bool HitSomething(Location location)
        {
            IGame game = this.ShipConnectedTo.Map.Game;

            if (this.HitHostile(location.Coordinate.Y, location.Coordinate.X))
            {
                //TODO: Remove this from Torpedo Subsystem.  This needs to be called after a torpedo has fired

                game.ALLHostilesAttack(game.Map);
                return true;
            }

            if (this.HitSomethingElse(game.Map, location.Sector, location.Coordinate.Y, location.Coordinate.X))
            {
                //TODO: Remove this from Torpedo Subsystem.  This needs to be called after a torpedo has fired
                game.ALLHostilesAttack(game.Map);
                return true;
            }

            return false;
        }

        private bool HitHostile(int newY, int newX)
        {
            IGame game = this.ShipConnectedTo.Map.Game;

            var thisRegion = game.Map.Sectors.GetActive();
            var targetCoordinate = thisRegion.Coordinates.GetNoError(new Point(newX, newY));
            var hostileInSector = targetCoordinate?.Object as IShip;

            if (targetCoordinate?.Item == CoordinateItem.HostileShip && hostileInSector != null)
            {
                game.Map.RemoveTargetFromSector(game.Map, hostileInSector);

                if (hostileInSector.Faction == FactionName.Federation)
                {
                    game.Map.AddACoupleHostileFederationShipsToExistingMap();
                    this.ShipConnectedTo.Scavenge(ScavengeType.FederationShip);
                }

                if (hostileInSector.Faction == FactionName.Klingon)
                {
                    this.ShipConnectedTo.Scavenge(ScavengeType.OtherShip);
                }

                return true;
            }

            return false;
        }

        private static void DebugTrack(Location newLocation)
        {
            if (DEFAULTS.DEBUG_MODE)
            {
                List<Coordinate> qLocations = newLocation.Sector.Coordinates.Where(s => s.X == newLocation.Coordinate.X && s.Y == newLocation.Coordinate.Y).ToList();

                var qLocation = new Coordinate();

                if (qLocations.Count > 0)
                {
                    qLocation = qLocations.Single();
                }

                if (qLocation.Item == CoordinateItem.Empty)
                {
                    qLocation.Item = CoordinateItem.Debug;
                }
            }
        }

        private bool HitSomethingElse(IMap map,
                                      ISector Sector, 
                                      int newY, 
                                      int newX)
        {
            IGame game = this.ShipConnectedTo.Map.Game;


            //todo: move this code out of the function and pass location as Coordinate instead of a Navigation object
            List<Coordinate> qLocations = Sector.Coordinates.Where(s => s.X == newX && s.Y == newY).ToList();

            var qLocation = new Coordinate();

            if (qLocations.Count > 0)
            {
                qLocation = qLocations.Single(); //this is really for debug purposes.. It could be 1 line.
            }

            switch (qLocation.Item)
            {
                case CoordinateItem.Starbase:

                    game.DestroyStarbase(map, newY, newX, qLocation);

                    return true;

                case CoordinateItem.Star:

                    var star = (Star) qLocation.Object;

                    var starName = "UNKNOWN";

                    if(star != null)
                    {
                        starName = star.Name;
                    }

                    game.Interact.Line(string.Format(
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
                this.ShipConnectedTo.OutputLine("Photon torpedoes exhausted.");
                return true;
            }
            return false;
        }

        public void Calculator()
        {
            if (this.Damaged())
            {
                this.ShipConnectedTo.OutputLine("Cannot calculate while Torpedo subsystem is damaged.");
                return;
            }

            this.ShipConnectedTo.OutputLine("");

            var thisRegion = this.ShipConnectedTo.GetSector();
            var thisRegionHostiles = thisRegion.GetHostiles();

            //todo: once starbases are an object, then they are merely another hostile.  Delete this IF and the rest of the code should work fine.
            if (thisRegion.GetStarbaseCount() > 0)
            {
                if (this.ShipConnectedTo.Map.Game.PlayerNowEnemyToFederation)
                {
                    Navigation.For(this.ShipConnectedTo).StarbaseCalculator(this.ShipConnectedTo);
                }
            }

            if (thisRegionHostiles.Count == 0)
            {
                this.ShipConnectedTo.OutputLine("There are no Hostile ships in this Sector.");
                this.ShipConnectedTo.OutputLine("");
                return;
            }

            Location location = this.ShipConnectedTo.GetLocation();

            foreach (var ship in thisRegionHostiles)
            {
                string shipSectorX = ship.Coordinate.X.ToString();
                string shipSectorY = ship.Coordinate.Y.ToString();
                string direction = $"{Utility.Utility.ComputeDirection(location.Coordinate.X, location.Coordinate.Y, ship.Coordinate.X, ship.Coordinate.Y):#.##}";

                direction = Utility.Utility.AdjustIfNebula(thisRegion, direction, ref shipSectorX, ref shipSectorY);

                this.ShipConnectedTo.OutputLine($"Hostile ship in sector [{shipSectorX},{shipSectorY}]. Direction {direction}: ");
            }
        }

        public new static Torpedoes For(IShip ship)
        {
            return (Torpedoes)SubSystem_Base.For(ship, SubsystemType.Torpedoes);
        }

        public void TargetObject()
        {
            if (this.Damaged())
            {
                return;
            }

            this.ShipConnectedTo.OutputLine("");
            this.ShipConnectedTo.OutputLine("Objects to Target:");

            Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

            string userReply = null;
            this.ShipConnectedTo.OutputLine("");
            this.ShipConnectedTo.Map.Game.Interact.PromptUserConsole("Enter number to target: ", out userReply);

            this.ShipConnectedTo.OutputLine("");
            this.ShipConnectedTo.OutputLine("Target Object is not yet supported.");
        }
    }
}
