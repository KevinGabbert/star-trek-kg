using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    public class Navigation : SubSystem_Base, IMap
    {
        #region Properties

            public bool docked { get; set; } //todo: move this to ship
            public int MaxWarpFactor { get; set; }

            public Warp Warp { get; set; }
            public Movement Movement { get; set; }

        #endregion

        public Navigation(Map map, Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
            this.Map = map;
            this.Type = SubsystemType.Navigation;

            this.Warp = new Warp();
            this.Movement = new Movement(map, shipConnectedTo);
        }

        public override void OutputDamagedMessage()
        {
            Output.Write.Line(StarTrekKGSettings.GetSetting<string>("WarpEnginesDamaged"));
        }
        public override void OutputRepairedMessage()
        {
            Output.Write.Line(StarTrekKGSettings.GetSetting<string>("WarpEnginesRepaired"));
        }
        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        private void SetMaxWarpFactor()
        {
            this.MaxWarpFactor = (int)(0.2 + (Utility.Utility.Random).Next(9)); //todo: Come up with a better system than this.. perhaps each turn allow *repairs* to increase the MaxWarpFactor
            Output.Write.Line(string.Format(StarTrekKGSettings.GetSetting<string>("MaxWarpFactorMessage"), this.MaxWarpFactor));
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
            string direction;

            if (this.Movement.InvalidCourseCheck(out direction)) return;

            //todo: I'd like to check this sooner than *after* we start moving.  I have always disliked this behavior in the game
            if (this.Warp.InvalidWarpFactorCheck(this.MaxWarpFactor, out distance)) return;

            int lastQuadY;
            int lastQuadX;

            if (!Warp.EngageWarp(direction, distance, out lastQuadY, out lastQuadX, this.Map)) return;

            this.RepairOrTakeDamage(lastQuadX, lastQuadY);

            ShortRangeScan.For(this.ShipConnectedTo).Controls();

            //todo: upon arriving in quadrant, all damaged controls need to be enumerated
        }

        private void RepairOrTakeDamage(int lastQuadX, int lastQuadY) 
        {
            Location thisShip = this.ShipConnectedTo.GetLocation();

            docked = this.Map.IsDockingLocation(thisShip.Sector.Y, thisShip.Sector.X, this.Map.Quadrants.GetActive().Sectors);
            if (docked)
            {
                this.SuccessfulDockWithStarbase();
            }
            else
            {
                this.TakeAttackDamageOrRepair(this.Map, lastQuadY, lastQuadX);
            }
        }

        //todo: move to Game() object
        private void SuccessfulDockWithStarbase()
        {
            Output.Write.ResourceLine("DockingMessageLowerShields");
            Shields.For(this.ShipConnectedTo).Damage = 0;

            this.ShipConnectedTo.RepairEverything();

            Output.Write.ResourceLine(StarTrekKGSettings.GetSetting<string>("PlayerShip"), "SuccessfullDock");
        }

        //todo: move to Game() object
        private void TakeAttackDamageOrRepair(Map map, int lastQuadY, int lastQuadX)
        {
            var thisShip = this.ShipConnectedTo.GetLocation();
            var baddiesHangingAround = Quadrants.Get(map, thisShip.Quadrant).GetHostiles().Count > 0;
            var stillInThisQuadrant = lastQuadX == thisShip.Quadrant.X && lastQuadY == thisShip.Quadrant.Y;

            if (baddiesHangingAround && stillInThisQuadrant)
            {
                Game.ALLHostilesAttack(this.Map);
            }
            else
            {
                this.ShipConnectedTo.Subsystems.PartialRepair();
            }
        }

        public void Calculator(Map map)
        {
            var thisShip = this.ShipConnectedTo.GetLocation();

            double quadX;
            double quadY;

            Output.Write.Line(string.Format("Your Ship" + StarTrekKGSettings.GetSetting<string>("LocatedInQuadrant"), (thisShip.Quadrant.X), (thisShip.Quadrant.Y)));

            if (!Command.PromptUser(StarTrekKGSettings.GetSetting<string>("DestinationQuadrantX"), out quadX)
                || quadX < (Constants.QUADRANT_MIN + 1) 
                || quadX > Constants.QUADRANT_MAX)
                {
                    Output.Write.Line(StarTrekKGSettings.GetSetting<string>("InvalidXCoordinate"));
                    return;
                }

            if (!Command.PromptUser(StarTrekKGSettings.GetSetting<string>("DestinationQuadrantY"), out quadY)
                || quadY < (Constants.QUADRANT_MIN + 1) 
                || quadY > Constants.QUADRANT_MAX)
                {
                    Output.Write.Line(StarTrekKGSettings.GetSetting<string>("InvalidYCoordinate"));
                    return;
                }

            Output.Write.Line("");
            var qx = ((int)(quadX)) - 1;
            var qy = ((int)(quadY)) - 1;
            if (qx == thisShip.Quadrant.X && qy == thisShip.Quadrant.Y)
            {
                Output.Write.Line(StarTrekKGSettings.GetSetting<string>("TheCurrentLocation") + "Your Ship.");
                return;
            }

            Output.Write.Line(string.Format("Direction: {0:#.##}", Utility.Utility.ComputeDirection(thisShip.Quadrant.X, thisShip.Quadrant.Y, qx, qy)));
            Output.Write.Line(string.Format("Distance:  {0:##.##}", Utility.Utility.Distance(thisShip.Quadrant.X, thisShip.Quadrant.Y, qx, qy)));
        }

        public new static Navigation For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException(StarTrekKGSettings.GetSetting<string>("NavigationNotSetUp")); //todo: reflect the name and refactor this to ISubsystem
            }

            return (Navigation)ship.Subsystems.Single(s => s.Type == SubsystemType.Navigation);
        }

        ////temporary
        //public static Location GetLocation(Ship ship)
        //{
        //    Navigation returnVal = Navigation.For(ship);
        //    var location = new Location(returnVal.Map, returnVal.sectorX, returnVal.sectorY);

        //    location.Quadrant.X = returnVal.quadrantX;
        //    location.Quadrant.Y = returnVal.quadrantY;

        //    return location; //returnVal.Location;
        //}
    }
}
