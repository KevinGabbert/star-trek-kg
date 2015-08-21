using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
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

        public Warp Warp { get;}
        public Impulse Impulse { get; }
        public Movement Movement { get; }

        private Game.PromptFunc<string, bool> _prompt;

        #endregion

        public Navigation(Ship shipConnectedTo, Game game, Game.PromptFunc<string, bool> prompt) : base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.Navigation;

            this.Warp = new Warp(this.Game.Write);
            this.Impulse = new Impulse(this.Game.Write);

            this.Movement = new Movement(shipConnectedTo, game);
            this._prompt = prompt;
            //todo: refactor this to be a module variable
        }

        private void DivineMaxWarpFactor()
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

                    this.WarpControls(this._prompt);
                    break;

                case "imp":
                    this.SublightControls();
                    break;

                case "nto":
                    this.NavigateToObject();
                    break;
            }

            //todo: upon arriving in Region, all damaged controls need to be enumerated
            this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));

            ShipConnectedTo.UpdateDivinedSectors();
        }

        private void NavigateToObject()
        {
            if (this.Damaged()) //todo: change this to Impulse.For(  when navigation object is removed
            {
                return;
            }

            this.Game.Write.Line("");
            this.Game.Write.Line("Objects in Region:");

            Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

            string userReply = null;
            this.Game.Write.PromptUser("Enter number of Object to travel to: ", out userReply);

            this.Game.Write.Line("");
            this.Game.Write.Line("Navigate to Object is not yet supported.");

            //this.NavigateToObject();
        }

        public void SublightControls()
        {
            int distance;
            int direction;

            if (this.Movement.PromptAndCheckCourse(this._prompt, out direction))
            {
                return;
            }

            if (this.Impulse.InvalidSublightFactorCheck(this.MaxWarpFactor, out distance)) return;

            int lastRegionY;
            int lastRegionX;

            if (!Impulse.Engage(direction, distance, out lastRegionY, out lastRegionX, this.Game.Map))
            {
                return;
            }

            this.RepairOrTakeDamage(lastRegionX, lastRegionY);

            var crs = CombinedRangeScan.For(this.ShipConnectedTo);
            if (crs.Damaged())
            {
                ShortRangeScan.For(this.ShipConnectedTo).Controls();
            }
            else
            {
                crs.Controls();
            }
        }

        /// <summary>
        /// This is the Warp Workflow
        /// </summary>
        public void WarpControls(Game.PromptFunc<string, bool> promptUser)
        {
            if (this.Damaged())
            {
                this.DivineMaxWarpFactor();
            }

            int distance;
            int direction;

            if (this.Movement.PromptAndCheckCourse(promptUser, out direction))
            {
                return;
            }

            if (this.Warp.PromptAndCheckForInvalidWarpFactor(this.MaxWarpFactor, out distance)) return;

            int lastRegionY;
            int lastRegionX;

            if (!Warp.Engage(direction, distance, out lastRegionY, out lastRegionX, this.Game.Map))
            {
                return;
            }

            this.RepairOrTakeDamage(lastRegionX, lastRegionY);

            var crs = CombinedRangeScan.For(this.ShipConnectedTo);
            if (crs.Damaged())
            {
                ShortRangeScan.For(this.ShipConnectedTo).Controls();
            }
            else
            {
                crs.Controls();
            }

            //todo: upon arriving in Region, all damaged controls need to be enumerated
            //this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));
        }

        private void RepairOrTakeDamage(int lastRegionX, int lastRegionY)
        {
            this.Docked = false;

            Location thisShip = this.ShipConnectedTo.GetLocation();

            if (!this.Game.PlayerNowEnemyToFederation) //No Docking allowed if they hate you.
            {
                this.Docked = this.Game.Map.IsDockingLocation(thisShip.Sector.Y, thisShip.Sector.X,
                    this.Game.Map.Regions.GetActive().Sectors);
            }

            if (Docked)
            {
                this.SuccessfulDockWithStarbase();
            }
            else
            {
                this.TakeAttackDamageOrRepair(this.Game.Map, lastRegionY, lastRegionX);
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
        private void TakeAttackDamageOrRepair(IMap map, int lastRegionY, int lastRegionX)
        {
            var thisShip = this.ShipConnectedTo.GetLocation();

            var currentRegion = Regions.Get(map, thisShip.Region);

            var hostiles = currentRegion.GetHostiles();
            var baddiesHangingAround = hostiles.Count > 0;

            var hostileFedsInRegion = hostiles.Any(h => h.Faction == FactionName.Federation);
                //todo: Cheap.  Use a property for this.

            var stillInSameRegion = lastRegionX == thisShip.Region.X && lastRegionY == thisShip.Region.Y;

            if ((baddiesHangingAround && stillInSameRegion) ||
                hostileFedsInRegion ||
                (this.Game.PlayerNowEnemyToFederation && currentRegion.GetStarbaseCount() > 0))
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
            if (this.Damaged()) return;

            //todo: ask additional question.  sublight or warp

            var thisShip = this.ShipConnectedTo.GetLocation();

            int RegionX;
            int RegionY;

            this.Game.Write.Line(string.Format("Your Ship" + this.Game.Config.GetSetting<string>("LocatedInRegion"),
                (thisShip.Region.X), (thisShip.Region.Y)));

            if (!this.Game.Write.PromptUser(this.Game.Config.GetSetting<string>("DestinationRegionX"), out RegionX)
                || RegionX < (Constants.Region_MIN + 1)
                || RegionX > Constants.Region_MAX)
            {
                this.Game.Write.Line(this.Game.Config.GetSetting<string>("InvalidXCoordinate"));
                return;
            }

            if (!this.Game.Write.PromptUser(this.Game.Config.GetSetting<string>("DestinationRegionY"), out RegionY)
                || RegionY < (Constants.Region_MIN + 1)
                || RegionY > Constants.Region_MAX)
            {
                this.Game.Write.Line(this.Game.Config.GetSetting<string>("InvalidYCoordinate"));
                return;
            }

            this.Game.Write.Line("");
            var qx = ((int) (RegionX)) - 1;
            var qy = ((int) (RegionY)) - 1;
            if (qx == thisShip.Region.X && qy == thisShip.Region.Y)
            {
                this.Game.Write.Line(this.Game.Config.GetSetting<string>("TheCurrentLocation") + "Your Ship.");
                return;
            }

            this.Game.Write.Line($"Direction: {Utility.Utility.ComputeDirection(thisShip.Region.X, thisShip.Region.Y, qx, qy):#.##}");
            this.Game.Write.Line($"Distance:  {Utility.Utility.Distance(thisShip.Region.X, thisShip.Region.Y, qx, qy):##.##}");
        }

        public void StarbaseCalculator(Ship shipConnectedTo)
        {
            if (this.Damaged())
            {
                return;
            }

            var mySector = shipConnectedTo.Sector;

            var thisRegion = shipConnectedTo.GetRegion();

            var starbasesInSector = thisRegion.Sectors.Where(s => s.Item == SectorItem.Starbase).ToList();

            if (starbasesInSector.Any())
            {
                foreach (var starbase in starbasesInSector)
                {
                    this.Game.Write.Line("-----------------");
                    this.Game.Write.Line($"Starbase in sector [{(starbase.X + 1)},{(starbase.Y + 1)}].");

                    this.Game.Write.Line(
                        $"Direction: {Utility.Utility.ComputeDirection(mySector.X, mySector.Y, starbase.X, starbase.Y):#.##}");
                    this.Game.Write.Line(
                        $"Distance:  {Utility.Utility.Distance(mySector.X, mySector.Y, starbase.X, starbase.Y)/Constants.SECTOR_MAX:##.##}");
                }
            }
            else
            {
                this.Game.Write.Line("There are no starbases in this Region.");
            }
        }

        public static Navigation For(IShip ship)
        {
            return (Navigation) SubSystem_Base.For(ship, SubsystemType.Navigation);
        }
    }
}
