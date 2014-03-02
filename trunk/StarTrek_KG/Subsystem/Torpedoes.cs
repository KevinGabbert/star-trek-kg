﻿using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Torpedoes : SubSystem_Base, IWeapon
    {
        #region Properties

            public int Count { get; set; }

        #endregion

        public Torpedoes(Ship shipConnectedTo, Game game): base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.Torpedoes; //for lookup
        }

        public override void Controls(string command)
        {
            this.Controls();
        }

        public void Controls()
        {
            if (this.Damaged() || 
                this.Exhausted() || 
                (new Quadrants(this.Game.Map, this.Game.Write)).NoHostiles(this.Game.Map.Quadrants.GetHostiles())) return;

            var firingDirection = Environment.NewLine +
                                  " 4   5   6 " + Environment.NewLine +
                                 @"   \ ↑ /  " + Environment.NewLine +
                                  "3 ← <*> → 7" + Environment.NewLine +
                                 @"   / ↓ \  " + Environment.NewLine +
                                  " 2   1   8" + Environment.NewLine +
                                  Environment.NewLine +
                                  "Enter firing direction (1.0--9.0) ";

            int direction;
            if (!this.Game.Write.PromptUser(firingDirection, out direction)
                || direction < 1.0 
                || direction > 9.0)
            {
                this.Game.Write.Line("Invalid direction.");
                return;
            }

            this.Shoot(direction);
        }

        public void Shoot(double direction)
        {
            if(this.Count < 1)
            {
                this.Game.Write.Line("Cannot fire.  Torpedo Room reports no Torpedoes to fire.");
                return;
            }

            this.Game.ALLHostilesAttack(this.Game.Map);

            var angle = Utility.Utility.ComputeAngle(direction);

            Location torpedoStartingLocation = this.ShipConnectedTo.GetLocation();
            Quadrant quadrant = Quadrants.Get(this.Game.Map, torpedoStartingLocation.Quadrant);

            //var currentLocation = new VectorCoordinate(torpedoStartingLocation.Sector);
            //var torpedoVector = new VectorCoordinate(Math.Cos(angle)/20, Math.Sin(angle)/20);

            this.Game.Write.Line("Photon torpedo fired...");
            this.Count--;

            //TODO: WRITE SOME TORPEDO TESTS!

            //todo: add in a constructor to turn off coordinate bounds checking for this object only
            //either that, or come up with a null location so that the first WHILE will work
            var lastPosition = new Coordinate(-1, -1, false);

            var newLocation = new Location();
            newLocation.Quadrant = quadrant;
            newLocation.Sector = new Sector();

            ////todo: condense WHILE to be a function of Coordinate
            ////todo: eliminate the twice rounding of torpedo location, as the same value is evaluated twice
            ////todo: the rounding can happen once in a variable, and then referred to twice (see note below)
            //while (Torpedoes.IsInQuadrant(currentLocation))
            //{
            //    //Increment to next Sector
            //    if (this.HitSomething(currentLocation, lastPosition, newLocation))
            //    {
            //        this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));
            //        return;
            //    }

            //    //Keep going.. because we haven't hit anything yet

            //    //todo: How about storing a *rounded* XY that is referred to by the While, and the new SectorToCheck
            //    currentLocation.IncrementBy(torpedoVector);
            //}

            this.Game.Write.Line("Photon torpedo failed to hit anything.");
        }

        //private bool HitSomething(VectorCoordinate currentLocation, Coordinate lastPosition, Location newLocation)
        //{
        //    newLocation.Sector.IncrementBy(currentLocation);

        //    //todo: Condense into function of Coordinate
        //    if (Torpedoes.LastPositionAintNewPosition(newLocation, lastPosition))
        //    {
        //        this.Game.Write.DebugLine(string.Format("  ~{0},{1}~", lastPosition.X, lastPosition.Y));

        //        var outputCoordinate = Utility.Utility.HideXorYIfNebula(this.ShipConnectedTo.GetQuadrant(), newLocation.Sector.X.ToString(), newLocation.Sector.Y.ToString());

        //        this.Game.Write.Line(string.Format("  [{0},{1}]", outputCoordinate.X, outputCoordinate.Y));

        //        lastPosition.Update(newLocation);
        //    }

        //    Torpedoes.DebugTrack(newLocation);

        //    return this.HitSomething(newLocation);
        //}

        private static bool LastPositionAintNewPosition(Location newTorpedoLocation, Coordinate torpedoLastPosition)
        {
            return torpedoLastPosition.X != newTorpedoLocation.Sector.X || torpedoLastPosition.Y != newTorpedoLocation.Sector.Y;
        }

        //private static bool IsInQuadrant(VectorCoordinate torpedoLocation)
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
            if (this.HitHostile(location.Sector.Y, location.Sector.X))
            {
                //TODO: Remove this from Torpedo Subsystem.  This needs to be called after a torpedo has fired

                this.Game.ALLHostilesAttack(this.Game.Map);
                return true;
            }

            if (this.HitSomethingElse(this.Game.Map, location.Quadrant, location.Sector.Y, location.Sector.X))
            {
                //TODO: Remove this from Torpedo Subsystem.  This needs to be called after a torpedo has fired
                this.Game.ALLHostilesAttack(this.Game.Map);
                return true;
            }

            return false;
        }

        private bool HitHostile(int newY, int newX)
        {
            var thisQuadrant = this.Game.Map.Quadrants.GetActive();
            var hostilesInQuadrant = thisQuadrant.GetHostiles();
            IShip hostileInSector = hostilesInQuadrant.SingleOrDefault(hostileShip => hostileShip.Sector.X == newX &&
                                                                                      hostileShip.Sector.Y == newY);
            if (hostileInSector != null)
            {
                this.Game.Map.RemoveTargetFromSector(this.Game.Map, hostileInSector);

                if (hostileInSector.Faction == FactionName.Federation)
                {
                    this.Game.Map.AddACoupleHostileFederationShipsToExistingMap();
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
            if (Constants.DEBUG_MODE)
            {
                List<Sector> qLocations = newLocation.Quadrant.Sectors.Where(s => s.X == newLocation.Sector.X && s.Y == newLocation.Sector.Y).ToList();

                var qLocation = new Sector();

                if (qLocations.Count > 0)
                {
                    qLocation = qLocations.Single();
                }

                if (qLocation.Item == SectorItem.Empty)
                {
                    qLocation.Item = SectorItem.Debug;
                }
            }
        }

        private bool HitSomethingElse(IMap map,
                                      IQuadrant quadrant, 
                                      int newY, 
                                      int newX)
        {

            //todo: move this code out of the function and pass location as Sector instead of a Navigation object
            List<Sector> qLocations = quadrant.Sectors.Where(s => s.X == newX && s.Y == newY).ToList();

            var qLocation = new Sector();

            if (qLocations.Count > 0)
            {
                qLocation = qLocations.Single(); //this is really for debug purposes.. It could be 1 line.
            }

            switch (qLocation.Item)
            {
                case SectorItem.Starbase:

                    this.Game.DestroyStarbase(map, newY, newX, qLocation);

                    return true;

                case SectorItem.Star:

                    var star = ((Star) qLocation.Object);

                    var starName = "UNKNOWN";

                    if(star != null)
                    {
                        starName = star.Name;
                    }

                    this.Game.Write.Line(string.Format(
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
                this.Game.Write.Line("Photon torpedoes exhausted.");
                return true;
            }
            return false;
        }

        public void Calculator()
        {
            if (this.Damaged())
            {
                this.Game.Write.Line("Cannot calculate while Torpedo subsystem is damaged.");
                return;
            }

            this.Game.Write.Line("");

            var thisQuadrant = this.ShipConnectedTo.GetQuadrant();
            var thisQuadrantHostiles = thisQuadrant.GetHostiles();

            //todo: once starbases are an object, then they are merely another hostile.  Delete this IF and the rest of the code should work fine.
            if (thisQuadrant.GetStarbaseCount() > 0)
            {
                if (Game.PlayerNowEnemyToFederation)
                {
                    Navigation.For(this.ShipConnectedTo).StarbaseCalculator(this.ShipConnectedTo);
                }
            }

            if (thisQuadrantHostiles.Count == 0)
            {
                this.Game.Write.Line("There are no Hostile ships in this quadrant.");
                this.Game.Write.Line("");
                return;
            }

            Location location = this.ShipConnectedTo.GetLocation();

            foreach (var ship in thisQuadrantHostiles)
            {
                string shipSectorX = (ship.Sector.X).ToString();
                string shipSectorY = (ship.Sector.Y).ToString();
                string direction = string.Format("{0:#.##}", Utility.Utility.ComputeDirection(location.Sector.X, location.Sector.Y, ship.Sector.X, ship.Sector.Y));

                direction = Utility.Utility.AdjustIfNebula(thisQuadrant, direction, ref shipSectorX, ref shipSectorY);

                this.Game.Write.Line(string.Format("Hostile ship in sector [{0},{1}]. Direction {2}: ", shipSectorX, shipSectorY, direction));
            }
        }

        public static Torpedoes For(IShip ship)
        {
            return (Torpedoes)SubSystem_Base.For(ship, SubsystemType.Torpedoes);
        }

        public void TargetObject()
        {
            if (this.Damaged())
            {
                return;
            }

            this.Game.Write.Line("");
            this.Game.Write.Line("Objects to Target:");

            Computer.For(this.ShipConnectedTo).ListObjectsInQuadrant();

            string userReply = null;
            this.Game.Write.Line("");
            this.Game.Write.PromptUser("Enter number to target: ", out userReply);

            this.Game.Write.Line("");
            this.Game.Write.Line("Target Object is not yet supported.");


        }
    }
}
