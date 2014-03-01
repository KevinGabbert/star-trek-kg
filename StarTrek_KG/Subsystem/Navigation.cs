using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Navigation : SubSystem_Base
    {
        #region Properties

        public bool Docked { get; set; } //todo: move this to ship
        public int MaxWarpFactor { get; set; }

        public Warp Warp { get; set; }
        public Impulse Impulse { get; set; }
        public Movement Movement { get; set; }

        #endregion

        public Navigation(Ship shipConnectedTo, Game game) : base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.Navigation;

            this.Warp = new Warp(this.Game.Write);
            this.Impulse = new Impulse(this.Game.Write);

            this.Movement = new Movement(shipConnectedTo, game);
        }

        private void SetMaxWarpFactor()
        {
            this.MaxWarpFactor = (int) (0.2 + (Utility.Utility.Random).Next(9));
                //todo: Come up with a better system than this.. perhaps each turn allow *repairs* to increase the MaxWarpFactor
            this.Game.Write.Line(string.Format(this.Game.Config.GetSetting<string>("MaxWarpFactorMessage"),
                this.MaxWarpFactor));
        }

        public override void Controls(string command)
        {

            switch (command)
            {
                case "wrp":
                    this.WarpControls();
                    break;

                case "imp":
                    this.SublightControls();
                    break;

                case "nto":
                    this.NavigateToObject();
                    break;
            }
        }

        private void NavigateToObject()
        {
            if (this.Damaged()) //todo: change this to Impulse.For(  when navigation object is removed
            {
                return;
            }

            if (ShortRangeScan.For(this.ShipConnectedTo).Damaged())
            {
                this.Game.Write.Line("Unable to scan sector to navigate.");
                return;
            }
            else
            {
                this.Game.Write.Line("Objects in sector:");

                //todo: write a list of all Objects in sector

                this.Game.Write.Line("Navigate to Object is not yet supported.");

                //this.Game.Write.WithNoEndCR("Enter number of Object to travel to: ");

                ////todo: readline needs to be done using an event
                //var navCommand = Console.ReadLine().Trim().ToLower();
            }
        }

        public void SublightControls()
        {
            int distance;
            int direction;

            if (this.Movement.InvalidCourseCheck(out direction))
            {
                return;
            }

            if (this.Impulse.InvalidSublightFactorCheck(this.MaxWarpFactor, out distance)) return;

            int lastQuadY;
            int lastQuadX;

            if (!Impulse.Engage(direction, distance, out lastQuadY, out lastQuadX, this.Game.Map))
            {
                return;
            }

            this.RepairOrTakeDamage(lastQuadX, lastQuadY);

            var crs = CombinedRangeScan.For(this.ShipConnectedTo);
            if (crs.Damaged())
            {
                ShortRangeScan.For(this.ShipConnectedTo).Controls();
            }
            else
            {
                crs.Controls();
            }

            //todo: upon arriving in quadrant, all damaged controls need to be enumerated
            //this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));
        }

        public void WarpControls()
        {
            if (this.Damaged())
            {
                this.SetMaxWarpFactor();
            }

            int distance;
            int direction;

            if (this.Movement.InvalidCourseCheck(out direction))
            {
                return;
            }


            if (this.Warp.InvalidWarpFactorCheck(this.MaxWarpFactor, out distance)) return;

            int lastQuadY;
            int lastQuadX;

            if (!Warp.Engage(direction, distance, out lastQuadY, out lastQuadX, this.Game.Map))
            {
                return;
            }

            this.RepairOrTakeDamage(lastQuadX, lastQuadY);

            var crs = CombinedRangeScan.For(this.ShipConnectedTo);
            if (crs.Damaged())
            {
                ShortRangeScan.For(this.ShipConnectedTo).Controls();
            }
            else
            {
                crs.Controls();
            }

            //todo: upon arriving in quadrant, all damaged controls need to be enumerated
            //this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));
        }

        private void RepairOrTakeDamage(int lastQuadX, int lastQuadY)
        {
            this.Docked = false;

            Location thisShip = this.ShipConnectedTo.GetLocation();

            if (!this.Game.PlayerNowEnemyToFederation) //No Docking allowed if they hate you.
            {
                this.Docked = this.Game.Map.IsDockingLocation(thisShip.Sector.Y, thisShip.Sector.X,
                    this.Game.Map.Quadrants.GetActive().Sectors);
            }

            if (Docked)
            {
                this.SuccessfulDockWithStarbase();
            }
            else
            {
                this.TakeAttackDamageOrRepair(this.Game.Map, lastQuadY, lastQuadX);
            }
        }

        private void SuccessfulDockWithStarbase()
        {
            this.Game.Write.ResourceLine("DockingMessageLowerShields");
            Shields.For(this.ShipConnectedTo).Energy = 0;

            Shields.For(this.ShipConnectedTo).Damage = 0;

            this.ShipConnectedTo.RepairEverything();

            this.Game.Write.ResourceLine(this.Game.Config.GetSetting<string>("PlayerShip"), "SuccessfullDock");
        }

        //todo: move to Game() object
        private void TakeAttackDamageOrRepair(IMap map, int lastQuadY, int lastQuadX)
        {
            var thisShip = this.ShipConnectedTo.GetLocation();

            var currentQuadrant = Quadrants.Get(map, thisShip.Quadrant);

            var hostiles = currentQuadrant.GetHostiles();
            var baddiesHangingAround = hostiles.Count > 0;

            var hostileFedsInQuadrant = hostiles.Any(h => h.Faction == FactionName.Federation);
                //todo: Cheap.  Use a property for this.

            var stillInSameQuadrant = lastQuadX == thisShip.Quadrant.X && lastQuadY == thisShip.Quadrant.Y;

            if ((baddiesHangingAround && stillInSameQuadrant) ||
                hostileFedsInQuadrant ||
                (this.Game.PlayerNowEnemyToFederation && currentQuadrant.GetStarbaseCount() > 0))
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

            //todo: ask additional question.  sublight or warp

            var thisShip = this.ShipConnectedTo.GetLocation();

            int quadX;
            int quadY;

            this.Game.Write.Line(string.Format("Your Ship" + this.Game.Config.GetSetting<string>("LocatedInQuadrant"),
                (thisShip.Quadrant.X), (thisShip.Quadrant.Y)));

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
            var qx = ((int) (quadX)) - 1;
            var qy = ((int) (quadY)) - 1;
            if (qx == thisShip.Quadrant.X && qy == thisShip.Quadrant.Y)
            {
                this.Game.Write.Line(this.Game.Config.GetSetting<string>("TheCurrentLocation") + "Your Ship.");
                return;
            }

            this.Game.Write.Line(string.Format("Direction: {0:#.##}",
                Utility.Utility.ComputeDirection(thisShip.Quadrant.X, thisShip.Quadrant.Y, qx, qy)));
            this.Game.Write.Line(string.Format("Distance:  {0:##.##}",
                Utility.Utility.Distance(thisShip.Quadrant.X, thisShip.Quadrant.Y, qx, qy)));
        }

        public static Navigation For(IShip ship)
        {
            return (Navigation) SubSystem_Base.For(ship, SubsystemType.Navigation);
        }
    }
}
