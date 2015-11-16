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

        private WarpActor Warp { get;}
        private ImpulseActor Impulse { get; }

        public Movement Movement { get; }

        #endregion

        public Navigation(Ship shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.Navigation;

            this.Warp = new WarpActor(this.ShipConnectedTo.Map.Game.Interact);
            this.Impulse = new ImpulseActor(this.ShipConnectedTo.Map.Game.Interact);

            this.Movement = new Movement(shipConnectedTo);

            //todo: refactor this to be a module variable
        }

        private void DivineMaxWarpFactor()
        {
            this.MaxWarpFactor = (int) (0.2 + (Utility.Utility.Random).Next(9));

                //todo: Come up with a better system than this.. perhaps each turn allow *repairs* to increase the MaxWarpFactor
            this.ShipConnectedTo.Map.Game.Interact.Line(string.Format(this.ShipConnectedTo.Map.Game.Config.GetSetting<string>("MaxWarpFactorMessage"),
                this.MaxWarpFactor));
        }

        public override List<string> Controls(string command)
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

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
            this.ShipConnectedTo.Map.Game.Interact.OutputConditionAndWarnings(this.ShipConnectedTo, this.ShipConnectedTo.Map.Game.Config.GetSetting<int>("ShieldsDownLevel"));

            ShipConnectedTo.UpdateDivinedSectors();

            return this.ShipConnectedTo.Map.Game.Interact.Output.Queue.ToList();
        }

        private List<string> NavigateToObject()
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) //todo: change this to Impulse.For(  when navigation object is removed
            {
                return this.ShipConnectedTo.Map.Game.Interact.Output.Queue.ToList();
            }

            this.ShipConnectedTo.Map.Game.Interact.Line("");
            this.ShipConnectedTo.Map.Game.Interact.Line("Objects in Region:");

            Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

            string userReply = null;
            this.ShipConnectedTo.Map.Game.Interact.PromptUserConsole("Enter number of Object to travel to: ", out userReply);

            this.ShipConnectedTo.Map.Game.Interact.Line("");
            this.ShipConnectedTo.Map.Game.Interact.Line("Navigate to Object is not yet supported.");

            //this.NavigateToObject();
            return this.ShipConnectedTo.Map.Game.Interact.Output.Queue.ToList();
        }

        private List<string> SublightControls()
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            string distance;
            NavDirection direction;

            if (this.Movement.PromptAndCheckCourse(out direction))
            {
                return this.ShipConnectedTo.Map.Game.Interact.Output.Queue.ToList();
            }

            if (this.Impulse.InvalidSublightFactorCheck(this.MaxWarpFactor, out distance)) return this.ShipConnectedTo.Map.Game.Interact.Output.Queue.ToList();

            int lastRegionY;
            int lastRegionX;

            if (!Impulse.Engage(direction, int.Parse(distance), out lastRegionY, out lastRegionX, this.ShipConnectedTo.Map))
            {
                return this.ShipConnectedTo.Map.Game.Interact.Output.Queue.ToList();
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

            return this.ShipConnectedTo.Map.Game.Interact.Output.Queue.ToList();
        }

        /// <summary>
        /// This is the Warp Workflow
        /// </summary>
        private void WarpControls()
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

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

            if (!Warp.Engage(direction, int.Parse(distance), out lastRegionY, out lastRegionX, this.ShipConnectedTo.Map))
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
            IGame game = this.ShipConnectedTo.Map.Game;

            if (!game.PlayerNowEnemyToFederation) //No Docking allowed if they hate you.
            {
                this.Docked = game.Map.IsDockingLocation(thisShip.Sector.Y, thisShip.Sector.X,
                    game.Map.Regions.GetActive().Sectors);
            }

            if (Docked)
            {
                this.SuccessfulDockWithStarbase();
            }
            else
            {
                this.TakeAttackDamageOrRepair(game.Map, lastRegionY, lastRegionX);
            }
        }

        private void SuccessfulDockWithStarbase()
        {
            this.ShipConnectedTo.Map.Game.Interact.ResourceLine("DockingMessageLowerShields");
            Shields.For(this.ShipConnectedTo).Energy = 0;

            Shields.For(this.ShipConnectedTo).Damage = 0;

            this.ShipConnectedTo.RepairEverything();

            this.ShipConnectedTo.Map.Game.Interact.ResourceLine(this.ShipConnectedTo.Map.Game.Config.GetSetting<string>("PlayerShip"), "SuccessfullDock");
        }

        //todo: move to Game() object
        private void TakeAttackDamageOrRepair(IMap map, int lastRegionY, int lastRegionX)
        {
            Location thisShip = this.ShipConnectedTo.GetLocation();
            IGame game = this.ShipConnectedTo.Map.Game;

            var currentRegion = Regions.Get(map, thisShip.Region);

            var hostiles = currentRegion.GetHostiles();
            var baddiesHangingAround = hostiles.Count > 0;

            var hostileFedsInRegion = hostiles.Any(h => h.Faction == FactionName.Federation);
                //todo: Cheap.  Use a property for this.

            var stillInSameRegion = lastRegionX == thisShip.Region.X && lastRegionY == thisShip.Region.Y;

            if ((baddiesHangingAround && stillInSameRegion) ||
                hostileFedsInRegion ||
                (game.PlayerNowEnemyToFederation && currentRegion.GetStarbaseCount() > 0))
            {
                game.ALLHostilesAttack(game.Map);
            }
            else
            {
                this.ShipConnectedTo.Subsystems.PartialRepair();
            }
        }

        public void Calculator()
        {
            if (this.Damaged()) return;

            IStarTrekKGSettings config = this.ShipConnectedTo.Map.Game.Config;

            //todo: ask additional question.  sublight or warp

            var thisShip = this.ShipConnectedTo.GetLocation();

            string RegionX;
            string RegionY;

            this.ShipConnectedTo.Map.Game.Interact.Line(string.Format("Your Ship" + config.GetSetting<string>("LocatedInRegion"),
                (thisShip.Region.X), (thisShip.Region.Y)));

            if (!this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Navigation, "Navigation:>", config.GetSetting<string>("DestinationRegionX"), out RegionX, this.ShipConnectedTo.Map.Game.Interact.Output.Queue, 1)
                || int.Parse(RegionX) < (DEFAULTS.REGION_MIN + 1)
                || int.Parse(RegionX) > DEFAULTS.REGION_MAX)
            {
                this.ShipConnectedTo.Map.Game.Interact.Line(config.GetSetting<string>("InvalidXCoordinate"));
                return;
            }

            if (!this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Navigation, "Navigation:>", config.GetSetting<string>("DestinationRegionY"), out RegionY, this.ShipConnectedTo.Map.Game.Interact.Output.Queue, 2)
                || int.Parse(RegionY) < (DEFAULTS.REGION_MIN + 1)
                || int.Parse(RegionY) > DEFAULTS.REGION_MAX)
            {
                this.ShipConnectedTo.Map.Game.Interact.Line(config.GetSetting<string>("InvalidYCoordinate"));
                return;
            }

            this.ShipConnectedTo.Map.Game.Interact.Line("");
            var qx = int.Parse(RegionX) - 1;
            var qy = int.Parse(RegionY) - 1;
            if (qx == thisShip.Region.X && qy == thisShip.Region.Y)
            {
                this.ShipConnectedTo.Map.Game.Interact.Line(config.GetSetting<string>("TheCurrentLocation") + "Your Ship.");
                return;
            }

            this.ShipConnectedTo.Map.Game.Interact.Line($"Direction: {Utility.Utility.ComputeDirection(thisShip.Region.X, thisShip.Region.Y, qx, qy):#.##}");
            this.ShipConnectedTo.Map.Game.Interact.Line($"Distance:  {Utility.Utility.Distance(thisShip.Region.X, thisShip.Region.Y, qx, qy):##.##}");
        }

        public void StarbaseCalculator(IShip shipConnectedTo)
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
                    this.ShipConnectedTo.Map.Game.Interact.Line("-----------------");
                    this.ShipConnectedTo.Map.Game.Interact.Line($"Starbase in sector [{(starbase.X + 1)},{(starbase.Y + 1)}].");
                    
                    this.ShipConnectedTo.Map.Game.Interact.Line($"Direction: {Utility.Utility.ComputeDirection(mySector.X, mySector.Y, starbase.X, starbase.Y):#.##}");
                    this.ShipConnectedTo.Map.Game.Interact.Line($"Distance:  {Utility.Utility.Distance(mySector.X, mySector.Y, starbase.X, starbase.Y)/ DEFAULTS.SECTOR_MAX:##.##}");
                }
            }
            else
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("There are no starbases in this Region.");
            }
        }

        public static Navigation For(IShip ship)
        {
            return (Navigation) SubSystem_Base.For(ship, SubsystemType.Navigation);
        }
    }
}
