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

        public static List<string> WARP_PANEL = new List<string>();


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

        private int ComputeMaxImpulseDistance()
        {
            var energy = this.ShipConnectedTo?.Energy ?? 1;
            var cap = DEFAULTS.COORDINATE_MAX; // keep impulse travel reasonable; tweak if needed
            if (energy < 1)
            {
                return 1;
            }
            return energy > cap ? cap : energy;
        }

        private void EnsureMaxWarpFactor()
        {
            if (this.MaxWarpFactor <= 0)
            {
                this.MaxWarpFactor = this.ShipConnectedTo.Map.Game.Config.GetSetting<int>("MaxWarpFactor");
            }

            if (this.Damaged())
            {
                this.DivineMaxWarpFactor();
            }
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
            //this.ShipConnectedTo.ClearOutputQueue();

            var promptInfo = this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo;
            SubsystemType currentSelectedSubsystem = promptInfo.SubSystem;

            //todo: fix subsystem? use constant
            //todo: we may want to use warp subsystem
            if (currentSelectedSubsystem == SubsystemType.Navigation)
            {
                if (promptInfo.Level > 0)
                {
                    this.WarpPromptControls(command);
                }
                else if (command == SubsystemType.Warp.Abbreviation)
                {
                    this.WarpControls();
                }
            }
            else if (currentSelectedSubsystem == SubsystemType.Impulse)
            {
                this.SublightControls(command);
            }

            //todo:
            //else if (x == Commands.Navigation.NavigateToObject)
            //{
            //    this.NavigateToObject();
            //}

            //todo: upon arriving in Sector, all damaged controls need to be enumerated
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
            this.ShipConnectedTo.OutputLine("Objects in Sector:");

            Computer.For(this.ShipConnectedTo).ListObjectsInSector();

            string userReply = null;
            this.ShipConnectedTo.Map.Game.Interact.PromptUserConsole("Enter number of Object to travel to: ", out userReply);

            this.ShipConnectedTo.OutputLine("");
            this.ShipConnectedTo.OutputLine("Navigate to Object is not yet supported.");

            //this.NavigateToObject();
            return this.ShipConnectedTo.OutputQueue();
        }

        private List<string> SublightControls(string command)
        {
            //this.ShipConnectedTo.ClearOutputQueue();

            if (!base.Damaged())
            {
                var promptLevel = this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo.Level;

                // Expecting numeric input at this point.
                if (string.IsNullOrWhiteSpace(command) || !command.IsNumeric() || command.Contains("."))
                {
                    this.ShipConnectedTo.OutputLine("Invalid entry.");
                    this.RepromptImpulse(promptLevel);
                    return this.ShipConnectedTo.OutputQueue();
                }

                switch (promptLevel)
                {
                    case 1:
                        // direction input
                        int directionValue;
                        if (!int.TryParse(command, out directionValue))
                        {
                            this.ShipConnectedTo.OutputLine("Invalid course.");
                            this.RepromptImpulse(promptLevel);
                            return this.ShipConnectedTo.OutputQueue();
                        }

                        var directions = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();
                        if (directionValue < directions.Min() || directionValue > directions.Max())
                        {
                            this.ShipConnectedTo.OutputLine("Invalid course.");
                            this.RepromptImpulse(promptLevel);
                            return this.ShipConnectedTo.OutputQueue();
                        }

                        this._movementDirection = (NavDirection)directionValue;
                        this.GetValueFromUser("distance");
                        break;

                    case 2:
                        // distance input
                        int distance;
                        if (!int.TryParse(command, out distance))
                        {
                            this.ShipConnectedTo.OutputLine("Invalid distance.");
                            this.RepromptImpulse(promptLevel);
                            return this.ShipConnectedTo.OutputQueue();
                        }

                        this.MaxDistance = this.ComputeMaxImpulseDistance();
                        if (distance < 1 || distance > this.MaxDistance)
                        {
                            this.ShipConnectedTo.OutputLine($"Invalid distance. Enter 1 to {this.MaxDistance}.");
                            this.RepromptImpulse(promptLevel);
                            return this.ShipConnectedTo.OutputQueue();
                        }

                        int lastSectorY;
                        int lastSectorX;

                        if (!Impulse.Engage(this._movementDirection, distance, out lastSectorY,
                            out lastSectorX, this.ShipConnectedTo.Map))
                        {
                            return this.ShipConnectedTo.OutputQueue();
                        }

                        this.RepairOrTakeDamage(lastSectorX, lastSectorY);

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
                        break;
                }

                //prompt needs to reset, then do a CRS, SRS, or just return a message to say that you have traveled.
            }

            return this.ShipConnectedTo.OutputQueue();
        }

        private List<string> WarpPromptControls(string command)
        {
            var promptLevel = this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo.Level;

            if (promptLevel == 1)
            {
                this.EnsureMaxWarpFactor();
            }

            if (string.IsNullOrWhiteSpace(command) || !command.IsNumeric() || command.Contains("."))
            {
                this.ShipConnectedTo.OutputLine("Invalid entry.");
                this.RepromptWarp(promptLevel);
                return this.ShipConnectedTo.OutputQueue();
            }

            switch (promptLevel)
            {
                case 1:
                    int directionValue;
                    if (!int.TryParse(command, out directionValue))
                    {
                        this.ShipConnectedTo.OutputLine("Invalid course.");
                        this.RepromptWarp(promptLevel);
                        return this.ShipConnectedTo.OutputQueue();
                    }

                    var directions = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();
                    if (directionValue < directions.Min() || directionValue > directions.Max())
                    {
                        this.ShipConnectedTo.OutputLine("Invalid course.");
                        this.RepromptWarp(promptLevel);
                        return this.ShipConnectedTo.OutputQueue();
                    }

                    this._movementDirection = (NavDirection)directionValue;
                    this.PromptForWarpFactor();
                    break;

                case 2:
                    int warpFactor;
                    if (!int.TryParse(command, out warpFactor))
                    {
                        this.ShipConnectedTo.OutputLine("Invalid warp factor.");
                        this.RepromptWarp(promptLevel);
                        return this.ShipConnectedTo.OutputQueue();
                    }

                    if (warpFactor < 1 || warpFactor > this.MaxWarpFactor)
                    {
                        this.ShipConnectedTo.OutputLine($"Invalid warp factor. Enter 1 to {this.MaxWarpFactor}.");
                        this.RepromptWarp(promptLevel);
                        return this.ShipConnectedTo.OutputQueue();
                    }

                    int lastSectorY;
                    int lastSectorX;

                    if (!Warp.Engage(this._movementDirection, warpFactor, out lastSectorY, out lastSectorX, this.ShipConnectedTo.Map))
                    {
                        return this.ShipConnectedTo.OutputQueue();
                    }

                    this.RepairOrTakeDamage(lastSectorX, lastSectorY);

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
                    break;
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

            int lastSectorY;
            int lastSectorX;

            if (!Warp.Engage(direction, int.Parse(distance), out lastSectorY, out lastSectorX, this.ShipConnectedTo.Map))
            {
                return;
            }

            this.RepairOrTakeDamage(lastSectorX, lastSectorY);

            var crs = CombinedRangeScan.For(this.ShipConnectedTo);
            if (crs.Damaged())
            {
                ShortRangeScan.For(this.ShipConnectedTo).Controls();
            }
            else
            {
                crs.Controls();
            }

            //todo: upon arriving in Sector, all damaged controls need to be enumerated
            //this.Game.Write.OutputConditionAndWarnings(this.ShipConnectedTo, this.Game.Config.GetSetting<int>("ShieldsDownLevel"));

            return;
        }

        private void RepairOrTakeDamage(int lastSectorX, int lastSectorY)
        {
            this.Docked = false;

            Location thisShip = this.ShipConnectedTo.GetLocation();
            IGame game = this.ShipConnectedTo.Map.Game;

            if (!game.PlayerNowEnemyToFederation) //No Docking allowed if they hate you.
            {
                this.Docked = game.Map.IsDockingLocation(thisShip.Coordinate.Y, thisShip.Coordinate.X,
                    game.Map.Sectors.GetActive().Coordinates);
            }

            if (Docked)
            {
                this.SuccessfulDockWithStarbase();
            }
            else
            {
                this.TakeAttackDamageOrRepair(game.Map, lastSectorY, lastSectorX);
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
        private void TakeAttackDamageOrRepair(IMap map, int lastSectorY, int lastSectorX)
        {
            Location thisShip = this.ShipConnectedTo.GetLocation();
            IGame game = this.ShipConnectedTo.Map.Game;

            Sector currentSector = map.Sectors[thisShip.Sector];

            List<IShip> hostiles = currentSector.GetHostiles();
            bool baddiesHangingAround = hostiles.Count > 0;

            bool hostileFedsInSector = hostiles.Any(h => h.Faction == FactionName.Federation);
                //todo: Cheap.  Use a property for this.

            bool stillInSameSector = lastSectorX == thisShip.Sector.X && lastSectorY == thisShip.Sector.Y;

            if ((baddiesHangingAround && stillInSameSector) ||
                hostileFedsInSector ||
                (game.PlayerNowEnemyToFederation && currentSector.GetStarbaseCount() > 0))
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

            string SectorX;
            string SectorY;

            this.ShipConnectedTo.OutputLine(string.Format("Your Ship" + config.GetSetting<string>("LocatedInSector"),
                thisShip.Sector.X, thisShip.Sector.Y));

            if (!this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Navigation, "Navigation:>", config.GetSetting<string>("DestinationSectorX"), out SectorX, this.ShipConnectedTo.Map.Game.Interact.Output.Queue, 1)
                || int.Parse(SectorX) < DEFAULTS.SECTOR_MIN + 1
                || int.Parse(SectorX) > DEFAULTS.SECTOR_MAX)
            {
                this.ShipConnectedTo.OutputLine(config.GetSetting<string>("InvalidXCoordinate"));
                return;
            }

            if (!this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Navigation, "Navigation:>", config.GetSetting<string>("DestinationSectorY"), out SectorY, this.ShipConnectedTo.Map.Game.Interact.Output.Queue, 2)
                || int.Parse(SectorY) < DEFAULTS.SECTOR_MIN + 1
                || int.Parse(SectorY) > DEFAULTS.SECTOR_MAX)
            {
                this.ShipConnectedTo.OutputLine(config.GetSetting<string>("InvalidYCoordinate"));
                return;
            }

            this.ShipConnectedTo.OutputLine("");
            var qx = int.Parse(SectorX) - 1;
            var qy = int.Parse(SectorY) - 1;
            if (qx == thisShip.Sector.X && qy == thisShip.Sector.Y)
            {
                this.ShipConnectedTo.OutputLine(config.GetSetting<string>("TheCurrentLocation") + "Your Ship.");
                return;
            }

            this.ShipConnectedTo.OutputLine($"Direction: {Utility.Utility.ComputeDirection(thisShip.Sector.X, thisShip.Sector.Y, qx, qy):#.##}");
            this.ShipConnectedTo.OutputLine($"Distance:  {Utility.Utility.Distance(thisShip.Sector.X, thisShip.Sector.Y, qx, qy):##.##}");
        }

        public void StarbaseCalculator(IShip shipConnectedTo)
        {
            if (this.Damaged())
            {
                return;
            }

            var mySector = shipConnectedTo.Coordinate;

            var thisSector = shipConnectedTo.GetSector();

            var starbasesInSector = thisSector.Coordinates.Where(s => s.Item == CoordinateItem.Starbase).ToList();

            if (starbasesInSector.Any())
            {
                foreach (var starbase in starbasesInSector)
                {
                    this.ShipConnectedTo.OutputLine("-----------------");
                    this.ShipConnectedTo.OutputLine($"Starbase in sector [{starbase.X + 1},{starbase.Y + 1}].");
                    
                    this.ShipConnectedTo.OutputLine($"Direction: {Utility.Utility.ComputeDirection(mySector.X, mySector.Y, starbase.X, starbase.Y):#.##}");
                    this.ShipConnectedTo.OutputLine($"Distance:  {Utility.Utility.Distance(mySector.X, mySector.Y, starbase.X, starbase.Y)/ DEFAULTS.COORDINATE_MAX:##.##}");
                }
            }
            else
            {
                this.ShipConnectedTo.OutputLine("There are no starbases in this Sector.");
            }
        }

        public new static Navigation For(IShip ship)
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
                this.MaxDistance = this.ComputeMaxImpulseDistance();
                this.ShipConnectedTo.Map.Game.Interact.PromptUser(SubsystemType.Impulse,
                                                                   "Impulse Control-> Distance-> ",
                                                                   $"Enter distance to travel (1--{this.MaxDistance}) ", //todo: resource this
                                                                   out transfer,
                                                                   this.ShipConnectedTo.Map.Game.Interact.Output.Queue,
                                                                   subPromptLevel: 2);
            }

            promptInfo.SubCommand = subCommand;
        }

        private void PromptForWarpFactor()
        {
            string transfer;
            this.ShipConnectedTo.Map.Game.Interact.PromptUser(
                SubsystemType.Navigation,
                "Warp Control-> Speed-> ",
                $"Enter warp factor (1--{this.MaxWarpFactor}) ",
                out transfer,
                this.ShipConnectedTo.Map.Game.Interact.Output.Queue,
                subPromptLevel: 2);
        }

        private void RepromptImpulse(int promptLevel)
        {
            string throwaway;

            if (promptLevel <= 1)
            {
                this.ShipConnectedTo.Map.Game.Interact.PromptUser(
                    SubsystemType.Impulse,
                    $"{this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo.DefaultPrompt}Impulse Control -> Enter Direction ->",
                    null,
                    out throwaway,
                    this.ShipConnectedTo.Map.Game.Interact.Output.Queue,
                    1);
            }
            else
            {
                this.GetValueFromUser("distance");
            }
        }

        private void RepromptWarp(int promptLevel)
        {
            string throwaway;

            if (promptLevel <= 1)
            {
                this.ShipConnectedTo.Map.Game.Interact.PromptUser(
                    SubsystemType.Navigation,
                    $"{this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo.DefaultPrompt}Warp Control -> ",
                    this.ShipConnectedTo.Map.Game.Interact.RenderCourse() + "Enter Course: ",
                    out throwaway,
                    this.ShipConnectedTo.Map.Game.Interact.Output.Queue,
                    1);
            }
            else
            {
                this.PromptForWarpFactor();
            }
        }
    }
}
