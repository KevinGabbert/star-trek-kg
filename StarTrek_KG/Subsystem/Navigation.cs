using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class Navigation : SubSystem_Base, IInteract
    {
        private NavDirection _movementDirection;

        #region Properties

        public bool Docked { get; set; } //todo: move this to ship
        public int MaxWarpFactor { get; set; }
        public int MaxDistance { get; set; } = 1;

        private WarpActor Warp { get;}
        private ImpulseActor Impulse { get; }

        public Movement Movement { get; }

        #endregion

        public Navigation(IShip shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.Navigation;

            this.Warp = new WarpActor(this.ShipConnectedTo.Map.Game.Interact);
            this.Impulse = new ImpulseActor(this.ShipConnectedTo.Map.Game.Interact);

            this.Movement = new Movement(shipConnectedTo);

            //todo: refactor this to be a module variable
        }

        private void DivineMaxWarpFactor()
        {
            this.MaxWarpFactor = (int) (0.2 + Utility.Utility.Random.Next(9));

                //todo: Come up with a better system than this.. perhaps each turn allow *repairs* to increase the MaxWarpFactor
            this.ShipConnectedTo.OutputLine(string.Format(this.ShipConnectedTo.Map.Game.Config.GetSetting<string>("MaxWarpFactorMessage"),
                this.MaxWarpFactor));
        }

        public override List<string> Controls(string command)
        {
            this.ShipConnectedTo.ClearOutputQueue();

            SubsystemType x = this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo.SubSystem;

            //todo: what to do if is numeric.  How do we know what subsystem we are in?

            if (x == SubsystemType.Warp)
            {
                this.WarpControls();
            }
            else if (x == SubsystemType.Impulse)
            {
                this.SublightControls(command);
            }
            //else if (x == Commands.Navigation.NavigateToObject)
            //{
            //    this.NavigateToObject();
            //}

            //todo: upon arriving in Region, all damaged controls need to be enumerated
            this.ShipConnectedTo.Map.Game.Interact.OutputConditionAndWarnings(this.ShipConnectedTo, this.ShipConnectedTo.Map.Game.Config.GetSetting<int>("ShieldsDownLevel"));

            ShipConnectedTo.UpdateDivinedSectors();

            return this.ShipConnectedTo.OutputQueue();
        }

        private List<string> NavigateToObject()
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) //todo: change this to Impulse.For(  when navigation object is removed
            {
                return this.ShipConnectedTo.OutputQueue();
            }

            this.ShipConnectedTo.OutputLine("");
            this.ShipConnectedTo.OutputLine("Objects in Region:");

            Computer.For(this.ShipConnectedTo).ListObjectsInRegion();

            string userReply = null;
            this.ShipConnectedTo.Map.Game.Interact.PromptUserConsole("Enter number of Object to travel to: ", out userReply);

            this.ShipConnectedTo.OutputLine("");
            this.ShipConnectedTo.OutputLine("Navigate to Object is not yet supported.");

            //this.NavigateToObject();
            return this.ShipConnectedTo.OutputQueue();
        }

        private List<string> SublightControls(string command)
        {
            this.ShipConnectedTo.ClearOutputQueue();

            if (!base.Damaged())
            {
                //todo: do like SHE
                if (!command.IsNumeric() || base.NotRecognized(command))
                {
                    this.ShipConnectedTo.OutputLine("Navigation command not recognized."); //todo: resource this
                }
                else
                {
                    //expecting a numeric value at this point.
                    if (command.IsNumeric())
                    {
                        if (this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo.Level == 1)
                        {
                            //todo: verify course is valid
                            this._movementDirection = (NavDirection) Convert.ToInt32(command);

                            //(this.Movement.PromptAndCheckCourse(out direction))

                            this.GetValueFromUser("distance");
                        }
                        else if (this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo.Level == 2)
                        {
                            //todo:

                            //if (this.Impulse.InvalidSublightFactorCheck(this.MaxWarpFactor, out distance))
                            //    return this.ShipConnectedTo.OutputQueue();

                            int lastRegionY;
                            int lastRegionX;

                            //command should be distance here

                            if (!Impulse.Engage(this._movementDirection, int.Parse(command), out lastRegionY,
                                out lastRegionX, this.ShipConnectedTo.Map))
                            {
                                return this.ShipConnectedTo.OutputQueue();
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

                            this.ShipConnectedTo.ResetPrompt();
                        }
                    }
                }

                //prompt needs to reset, then do a CRS, SRS, or just return a message to say that you have traveled.
            }

            return this.ShipConnectedTo.OutputQueue();
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

            Region currentRegion = map.Regions[thisShip.Region];

            List<IShip> hostiles = currentRegion.GetHostiles();
            bool baddiesHangingAround = hostiles.Count > 0;

            bool hostileFedsInRegion = hostiles.Any(h => h.Faction == FactionName.Federation);
                //todo: Cheap.  Use a property for this.

            bool stillInSameRegion = lastRegionX == thisShip.Region.X && lastRegionY == thisShip.Region.Y;

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

            this.ShipConnectedTo.OutputLine(string.Format("Your Ship" + config.GetSetting<string>("LocatedInRegion"),
                thisShip.Region.X, thisShip.Region.Y));

            if (!this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Navigation, "Navigation:>", config.GetSetting<string>("DestinationRegionX"), out RegionX, this.ShipConnectedTo.Map.Game.Interact.Output.Queue, 1)
                || int.Parse(RegionX) < DEFAULTS.REGION_MIN + 1
                || int.Parse(RegionX) > DEFAULTS.REGION_MAX)
            {
                this.ShipConnectedTo.OutputLine(config.GetSetting<string>("InvalidXCoordinate"));
                return;
            }

            if (!this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Navigation, "Navigation:>", config.GetSetting<string>("DestinationRegionY"), out RegionY, this.ShipConnectedTo.Map.Game.Interact.Output.Queue, 2)
                || int.Parse(RegionY) < DEFAULTS.REGION_MIN + 1
                || int.Parse(RegionY) > DEFAULTS.REGION_MAX)
            {
                this.ShipConnectedTo.OutputLine(config.GetSetting<string>("InvalidYCoordinate"));
                return;
            }

            this.ShipConnectedTo.OutputLine("");
            var qx = int.Parse(RegionX) - 1;
            var qy = int.Parse(RegionY) - 1;
            if (qx == thisShip.Region.X && qy == thisShip.Region.Y)
            {
                this.ShipConnectedTo.OutputLine(config.GetSetting<string>("TheCurrentLocation") + "Your Ship.");
                return;
            }

            this.ShipConnectedTo.OutputLine($"Direction: {Utility.Utility.ComputeDirection(thisShip.Region.X, thisShip.Region.Y, qx, qy):#.##}");
            this.ShipConnectedTo.OutputLine($"Distance:  {Utility.Utility.Distance(thisShip.Region.X, thisShip.Region.Y, qx, qy):##.##}");
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
                    this.ShipConnectedTo.OutputLine("-----------------");
                    this.ShipConnectedTo.OutputLine($"Starbase in sector [{starbase.X + 1},{starbase.Y + 1}].");
                    
                    this.ShipConnectedTo.OutputLine($"Direction: {Utility.Utility.ComputeDirection(mySector.X, mySector.Y, starbase.X, starbase.Y):#.##}");
                    this.ShipConnectedTo.OutputLine($"Distance:  {Utility.Utility.Distance(mySector.X, mySector.Y, starbase.X, starbase.Y)/ DEFAULTS.SECTOR_MAX:##.##}");
                }
            }
            else
            {
                this.ShipConnectedTo.OutputLine("There are no starbases in this Region.");
            }
        }

        public static Navigation For(IShip ship)
        {
            return (Navigation) SubSystem_Base.For(ship, SubsystemType.Navigation);
        }

        //Interface
        public void GetValueFromUser(string subCommand)
        {
            PromptInfo promptInfo = this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo;

            if (promptInfo.Level == 1)
            {
                string transfer;
                this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Impulse,
                                                                   "Impulse Control-> Distance-> ",
                                                                   $"Enter distance to travel (1--{this.MaxDistance}) ", //todo: resource this
                                                                   out transfer,
                                                                   this.ShipConnectedTo.Map.Game.Interact.Output.Queue,
                                                                   subPromptLevel: 2);
            }

            promptInfo.SubCommand = subCommand;
        }
    }
}
