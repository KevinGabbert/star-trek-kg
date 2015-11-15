using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Constants.Commands;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Navigation : SubSystem_Base
    {
        #region Properties

        public bool Docked { get; set; } //todo: move this to ship
        public int MaxWarpFactor { get; set; }

        private Warp Warp { get;}
        private Impulse Impulse { get; }
        public Movement Movement { get; }

        #endregion

        #region Commands


        #endregion

        public Navigation(Ship shipConnectedTo, IGame game) : base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.Navigation;
            this.Warp = new Warp(this.Game.Interact);
            this.Impulse = new Impulse(this.Game.Interact);
            this.Movement = new Movement(shipConnectedTo, game);

            //todo: refactor this to be a module variable
        }

        private void DivineMaxWarpFactor()
        {
            this.MaxWarpFactor = (int) (0.2 + (Utility.Utility.Random).Next(9));

                //todo: Come up with a better system than this.. perhaps each turn allow *repairs* to increase the MaxWarpFactor
            this.Game.Interact.Line(string.Format(this.Game.Config.GetSetting<string>("MaxWarpFactorMessage"),
                this.MaxWarpFactor));
        }

        public override List<string> Controls(string command)
        {
            this.Game.Interact.Output.Queue.Clear();

            switch (command)
            {
                case Commands.Navigation.Warp:
                    this.WarpControls();
                    break;

                case Commands.Navigation.Impulse:
                    this.SublightControls();
                    break;

                case Commands.Navigation.NavigateToObject:
                    this.NavigateToObject();
                    break;
            }

            //todo: upon arriving in Region, all damaged controls need to be enumerated
            this.Game.Interact.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));

            ShipConnectedTo.UpdateDivinedSectors();

            return this.Game.Interact.Output.Queue.ToList();
        }

        private List<string> NavigateToObject()
        {
            this.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) //todo: change this to Impulse.For(  when navigation object is removed
            {
                return this.Game.Interact.Output.Queue.ToList();
            }

            this.Game.Interact.Line("");
            this.Game.Interact.Line("Objects in Region:");

            Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

            string userReply = null;
            this.Game.Interact.PromptUserConsole("Enter number of Object to travel to: ", out userReply);

            this.Game.Interact.Line("");
            this.Game.Interact.Line("Navigate to Object is not yet supported.");

            //this.NavigateToObject();
            return this.Game.Interact.Output.Queue.ToList();
        }

        private List<string> SublightControls()
        {
            this.Game.Interact.Output.Queue.Clear();

            string distance;
            NavDirection direction;

            if (this.Movement.PromptAndCheckCourse(out direction))
            {
                return this.Game.Interact.Output.Queue.ToList();
            }

            if (this.Impulse.InvalidSublightFactorCheck(this.MaxWarpFactor, out distance)) return this.Game.Interact.Output.Queue.ToList();

            int lastRegionY;
            int lastRegionX;

            if (!Impulse.Engage(direction, int.Parse(distance), out lastRegionY, out lastRegionX, this.Game.Map))
            {
                return this.Game.Interact.Output.Queue.ToList();
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

            return this.Game.Interact.Output.Queue.ToList();
        }

        /// <summary>
        /// This is the Warp Workflow
        /// </summary>
        private void WarpControls()
        {
            this.Game.Interact.Output.Queue.Clear();

            if (this.Damaged())
            {
                this.DivineMaxWarpFactor();
            }

            string distance;
            NavDirection direction;

            if (this.Movement.PromptAndCheckCourse(out direction))
            {
                return;
            }

            if (this.Warp.PromptAndCheckForInvalidWarpFactor(this.MaxWarpFactor, out distance)) return;

            int lastRegionY;
            int lastRegionX;

            if (!Warp.Engage(direction, int.Parse(distance), out lastRegionY, out lastRegionX, this.Game.Map))
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

            return;
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
            this.Game.Interact.ResourceLine("DockingMessageLowerShields");
            Shields.For(this.ShipConnectedTo).Energy = 0;

            Shields.For(this.ShipConnectedTo).Damage = 0;

            this.ShipConnectedTo.RepairEverything();

            this.Game.Interact.ResourceLine(this.Game.Config.GetSetting<string>("PlayerShip"), "SuccessfullDock");
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

            string RegionX;
            string RegionY;

            this.Game.Interact.Line(string.Format("Your Ship" + this.Game.Config.GetSetting<string>("LocatedInRegion"),
                (thisShip.Region.X), (thisShip.Region.Y)));

            if (!this.Game.Interact.PromptUser(SubsystemType.Navigation, "Navigation:>", this.Game.Config.GetSetting<string>("DestinationRegionX"), out RegionX, this.Game.Interact.Output.Queue, 1)
                || int.Parse(RegionX) < (DEFAULTS.Region_MIN + 1)
                || int.Parse(RegionX) > DEFAULTS.Region_MAX)
            {
                this.Game.Interact.Line(this.Game.Config.GetSetting<string>("InvalidXCoordinate"));
                return;
            }

            if (!this.Game.Interact.PromptUser(SubsystemType.Navigation, "Navigation:>", this.Game.Config.GetSetting<string>("DestinationRegionY"), out RegionY, this.Game.Interact.Output.Queue, 2)
                || int.Parse(RegionY) < (DEFAULTS.Region_MIN + 1)
                || int.Parse(RegionY) > DEFAULTS.Region_MAX)
            {
                this.Game.Interact.Line(this.Game.Config.GetSetting<string>("InvalidYCoordinate"));
                return;
            }

            this.Game.Interact.Line("");
            var qx = int.Parse(RegionX) - 1;
            var qy = int.Parse(RegionY) - 1;
            if (qx == thisShip.Region.X && qy == thisShip.Region.Y)
            {
                this.Game.Interact.Line(this.Game.Config.GetSetting<string>("TheCurrentLocation") + "Your Ship.");
                return;
            }

            this.Game.Interact.Line($"Direction: {Utility.Utility.ComputeDirection(thisShip.Region.X, thisShip.Region.Y, qx, qy):#.##}");
            this.Game.Interact.Line($"Distance:  {Utility.Utility.Distance(thisShip.Region.X, thisShip.Region.Y, qx, qy):##.##}");
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
                    this.Game.Interact.Line("-----------------");
                    this.Game.Interact.Line($"Starbase in sector [{(starbase.X + 1)},{(starbase.Y + 1)}].");

                    this.Game.Interact.Line($"Direction: {Utility.Utility.ComputeDirection(mySector.X, mySector.Y, starbase.X, starbase.Y):#.##}");
                    this.Game.Interact.Line($"Distance:  {Utility.Utility.Distance(mySector.X, mySector.Y, starbase.X, starbase.Y)/ DEFAULTS.SECTOR_MAX:##.##}");
                }
            }
            else
            {
                this.Game.Interact.Line("There are no starbases in this Region.");
            }
        }

        public static Navigation For(IShip ship)
        {
            return (Navigation) SubSystem_Base.For(ship, SubsystemType.Navigation);
        }
    }
}
