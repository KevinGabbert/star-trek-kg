﻿using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Navigation : SubSystem_Base
    {
        #region Properties

            public bool docked { get; set; } //todo: move this to ship
            public int MaxWarpFactor { get; set; }

            public Warp Warp { get; set; }
            public Movement Movement { get; set; }

        #endregion

        public Navigation(Ship shipConnectedTo, Game game)
        {
            this.Game = game;

            this.Initialize();

            this.ShipConnectedTo = shipConnectedTo;
            this.Type = SubsystemType.Navigation;

            this.Warp = new Warp(this.Game.Write);
            this.Movement = new Movement(shipConnectedTo, game);
        }

        public override void OutputDamagedMessage()
        {
            this.Game.Write.Line(this.Game.Config.GetSetting<string>("WarpEnginesDamaged"));
        }
        public override void OutputRepairedMessage()
        {
            this.Game.Write.Line(this.Game.Config.GetSetting<string>("WarpEnginesRepaired"));
        }
        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        private void SetMaxWarpFactor()
        {
            this.MaxWarpFactor = (int)(0.2 + (Utility.Utility.Random).Next(9)); //todo: Come up with a better system than this.. perhaps each turn allow *repairs* to increase the MaxWarpFactor
            this.Game.Write.Line(string.Format(this.Game.Config.GetSetting<string>("MaxWarpFactorMessage"), this.MaxWarpFactor));
        }

        public override void Controls(string command)
        {
            this.Controls();
        }

        public void Controls()
        {
            if(this.Damaged())
            {
                this.SetMaxWarpFactor();
            }

            double distance;
            double direction;

            if (this.Movement.InvalidCourseCheck(out direction))
            {
                return;
            }

            //todo: I'd like to check this sooner than *after* we start moving.  I have always disliked this behavior in the game
            if (this.Warp.InvalidWarpFactorCheck(this.MaxWarpFactor, out distance)) return;

            int lastQuadY;
            int lastQuadX;

            if (!Warp.EngageWarp(direction, distance, out lastQuadY, out lastQuadX, this.Game.Map))
            {
                return;
            }

            this.RepairOrTakeDamage(lastQuadX, lastQuadY);

            ShortRangeScan.For(this.ShipConnectedTo).Controls();

            //todo: upon arriving in quadrant, all damaged controls need to be enumerated
        }

        private void RepairOrTakeDamage(int lastQuadX, int lastQuadY) 
        {
            Location thisShip = this.ShipConnectedTo.GetLocation();

            docked = this.Game.Map.IsDockingLocation(thisShip.Sector.Y, thisShip.Sector.X, this.Game.Map.Quadrants.GetActive().Sectors);
            if (docked)
            {
                this.SuccessfulDockWithStarbase();
            }
            else
            {
                this.TakeAttackDamageOrRepair(this.Game.Map, lastQuadY, lastQuadX);
            }
        }

        //todo: move to Game() object
        private void SuccessfulDockWithStarbase()
        {
            this.Game.Write.ResourceLine("DockingMessageLowerShields");
            Shields.For(this.ShipConnectedTo).Damage = 0;

            this.ShipConnectedTo.RepairEverything();

            this.Game.Write.ResourceLine(this.Game.Config.GetSetting<string>("PlayerShip"), "SuccessfullDock");
        }

        //todo: move to Game() object
        private void TakeAttackDamageOrRepair(IMap map, int lastQuadY, int lastQuadX)
        {
            var thisShip = this.ShipConnectedTo.GetLocation();

            var hostiles = Quadrants.Get(map, thisShip.Quadrant).GetHostiles();
            var baddiesHangingAround = hostiles.Count > 0;

            var hostileFedsInQuadrant = hostiles.Any(h => h.Faction == Faction.Federation); //todo: Cheap.  Use a property for this.

            var stillInSameQuadrant = lastQuadX == thisShip.Quadrant.X && lastQuadY == thisShip.Quadrant.Y;

            if ((baddiesHangingAround && stillInSameQuadrant) || hostileFedsInQuadrant)
            {
                this.Game.ALLHostilesAttack(this.Game.Map);
            }
            else
            {
                this.ShipConnectedTo.Subsystems.PartialRepair();
            }
        }

        public void Calculator()
        {
            var thisShip = this.ShipConnectedTo.GetLocation();

            double quadX;
            double quadY;

            this.Game.Write.Line(string.Format("Your Ship" + this.Game.Config.GetSetting<string>("LocatedInQuadrant"), (thisShip.Quadrant.X), (thisShip.Quadrant.Y)));

            if (!this.Game.Write.PromptUser(this.Game.Config.GetSetting<string>("DestinationQuadrantX"), out quadX)
                || quadX < (Constants.QUADRANT_MIN + 1) 
                || quadX > Constants.QUADRANT_MAX)
                {
                    this.Game.Write.Line(this.Game.Config.GetSetting<string>("InvalidXCoordinate"));
                    return;
                }

            if (!this.Game.Write.PromptUser(this.Game.Config.GetSetting<string>("DestinationQuadrantY"), out quadY)
                || quadY < (Constants.QUADRANT_MIN + 1) 
                || quadY > Constants.QUADRANT_MAX)
                {
                    this.Game.Write.Line(this.Game.Config.GetSetting<string>("InvalidYCoordinate"));
                    return;
                }

            this.Game.Write.Line("");
            var qx = ((int)(quadX)) - 1;
            var qy = ((int)(quadY)) - 1;
            if (qx == thisShip.Quadrant.X && qy == thisShip.Quadrant.Y)
            {
                this.Game.Write.Line(this.Game.Config.GetSetting<string>("TheCurrentLocation") + "Your Ship.");
                return;
            }

            this.Game.Write.Line(string.Format("Direction: {0:#.##}", Utility.Utility.ComputeDirection(thisShip.Quadrant.X, thisShip.Quadrant.Y, qx, qy)));
            this.Game.Write.Line(string.Format("Distance:  {0:##.##}", Utility.Utility.Distance(thisShip.Quadrant.X, thisShip.Quadrant.Y, qx, qy)));
        }

        public static Navigation For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Navigation Not Set Up"); //todo: reflect the name and refactor this to ISubsystem
            }

            return (Navigation)ship.Subsystems.Single(s => s.Type == SubsystemType.Navigation);
        }
    }
}
