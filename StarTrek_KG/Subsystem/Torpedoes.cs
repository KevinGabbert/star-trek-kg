﻿using System;
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

        public Torpedoes(Ship shipConnectedTo): base(shipConnectedTo)
        {
            this.Type = SubsystemType.Torpedoes; //for lookup
        }

        public override List<string> Controls(string command)
        {
            return this.Controls();
        }

        public List<string> Controls()
        {
            IGame game = this.ShipConnectedTo.Map.Game;
            IInteraction prompt = game.Interact;

            prompt.Output.Queue.Clear();

            List<string> hostileCheckOutput;

            if (this.Damaged() || 
                this.Exhausted() || 
                (new Regions(game.Map, prompt)).NoHostiles(game.Map.Regions.GetHostiles(), out hostileCheckOutput)) return prompt.Output.Queue.ToList();

            prompt.Output.Write(hostileCheckOutput);

            //todo: resource this out.
            var firingDirection = Environment.NewLine +
                                  " 4   5   6 " + Environment.NewLine +
                                 @"   \ ↑ /  " + Environment.NewLine +
                                  "3 ← <*> → 7" + Environment.NewLine +
                                 @"   / ↓ \  " + Environment.NewLine +
                                  " 2   1   8" + Environment.NewLine +
                                  Environment.NewLine +
                                  "Enter firing direction (1.0--9.0) ";

            string direction;
            if (!prompt.PromptUser(SubsystemType.Phasers, "Phasers->", firingDirection, out direction, prompt.Output.Queue)
                || int.Parse(direction) < 1.0 
                || int.Parse(direction) > 9.0)
            {
                prompt.Line("Invalid direction.");
                return prompt.Output.Queue.ToList();
            }

            this.Shoot(int.Parse(direction));

            return prompt.Output.Queue.ToList();
        }

        public void Shoot(double direction)
        {
            IGame game = this.ShipConnectedTo.Map.Game;
            if (this.Count < 1)
            {
                this.Prompt.Line("Cannot fire.  Torpedo Room reports no Torpedoes to fire.");
                return;
            }

            game.ALLHostilesAttack(game.Map);

            double angle = Utility.Utility.ComputeAngle(direction);

            Location torpedoStartingLocation = this.ShipConnectedTo.GetLocation();
            Region Region = Regions.Get(game.Map, torpedoStartingLocation.Region);

            //var currentLocation = new VectorCoordinate(torpedoStartingLocation.Sector);
            //var torpedoVector = new VectorCoordinate(Math.Cos(angle)/20, Math.Sin(angle)/20);

            this.Prompt.Line("Photon torpedo fired...");
            this.Count--;

            //TODO: WRITE SOME TORPEDO TESTS!

            //todo: add in a constructor to turn off coordinate bounds checking for this object only
            //either that, or come up with a null location so that the first WHILE will work
            var lastPosition = new Coordinate(-1, -1);

            var newLocation = new Location
            {
                Region = Region,
                Sector = new Sector()
            };

            ////todo: condense WHILE to be a function of Coordinate
            ////todo: eliminate the twice rounding of torpedo location, as the same value is evaluated twice
            ////todo: the rounding can happen once in a variable, and then referred to twice (see note below)
            //while (Torpedoes.IsInRegion(currentLocation))
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

            this.Prompt.Line("Photon torpedo failed to hit anything.");
        }

        //private bool HitSomething(VectorCoordinate currentLocation, Coordinate lastPosition, Location newLocation)
        //{
        //    newLocation.Sector.IncrementBy(currentLocation);

        //    //todo: Condense into function of Coordinate
        //    if (Torpedoes.LastPositionAintNewPosition(newLocation, lastPosition))
        //    {
        //        this.Game.Write.DebugLine(string.Format("  ~{0},{1}~", lastPosition.X, lastPosition.Y));

        //        var outputCoordinate = Utility.Utility.HideXorYIfNebula(this.ShipConnectedTo.GetRegion(), newLocation.Sector.X.ToString(), newLocation.Sector.Y.ToString());

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

            if (this.HitHostile(location.Sector.Y, location.Sector.X))
            {
                //TODO: Remove this from Torpedo Subsystem.  This needs to be called after a torpedo has fired

                game.ALLHostilesAttack(game.Map);
                return true;
            }

            if (this.HitSomethingElse(game.Map, location.Region, location.Sector.Y, location.Sector.X))
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

            var thisRegion = game.Map.Regions.GetActive();
            var hostilesInRegion = thisRegion.GetHostiles();
            IShip hostileInSector = hostilesInRegion.SingleOrDefault(hostileShip => hostileShip.Sector.X == newX &&
                                                                                      hostileShip.Sector.Y == newY);
            if (hostileInSector != null)
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
                List<Sector> qLocations = newLocation.Region.Sectors.Where(s => s.X == newLocation.Sector.X && s.Y == newLocation.Sector.Y).ToList();

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
                                      IRegion Region, 
                                      int newY, 
                                      int newX)
        {
            IGame game = this.ShipConnectedTo.Map.Game;


            //todo: move this code out of the function and pass location as Sector instead of a Navigation object
            List<Sector> qLocations = Region.Sectors.Where(s => s.X == newX && s.Y == newY).ToList();

            var qLocation = new Sector();

            if (qLocations.Count > 0)
            {
                qLocation = qLocations.Single(); //this is really for debug purposes.. It could be 1 line.
            }

            switch (qLocation.Item)
            {
                case SectorItem.Starbase:

                    game.DestroyStarbase(map, newY, newX, qLocation);

                    return true;

                case SectorItem.Star:

                    var star = ((Star) qLocation.Object);

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
                this.ShipConnectedTo.Map.Game.Interact.Line("Photon torpedoes exhausted.");
                return true;
            }
            return false;
        }

        public void Calculator()
        {
            if (this.Damaged())
            {
                this.Prompt.Line("Cannot calculate while Torpedo subsystem is damaged.");
                return;
            }

            this.Prompt.Line("");

            var thisRegion = this.ShipConnectedTo.GetRegion();
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
                this.Prompt.Line("There are no Hostile ships in this Region.");
                this.Prompt.Line("");
                return;
            }

            Location location = this.ShipConnectedTo.GetLocation();

            foreach (var ship in thisRegionHostiles)
            {
                string shipSectorX = (ship.Sector.X).ToString();
                string shipSectorY = (ship.Sector.Y).ToString();
                string direction = $"{Utility.Utility.ComputeDirection(location.Sector.X, location.Sector.Y, ship.Sector.X, ship.Sector.Y):#.##}";

                direction = Utility.Utility.AdjustIfNebula(thisRegion, direction, ref shipSectorX, ref shipSectorY);

                this.Prompt.Line($"Hostile ship in sector [{shipSectorX},{shipSectorY}]. Direction {direction}: ");
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

            this.Prompt.Line("");
            this.Prompt.Line("Objects to Target:");

            Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

            string userReply = null;
            this.Prompt.Line("");
            this.Prompt.PromptUserConsole("Enter number to target: ", out userReply);

            this.Prompt.Line("");
            this.Prompt.Line("Target Object is not yet supported.");
        }
    }
}
