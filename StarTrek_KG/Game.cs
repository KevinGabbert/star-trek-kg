using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.Commands;
using StarTrek_KG.Actors;

namespace StarTrek_KG
{
    /// <summary>
    /// This class consists of methods that have yet to be refactored into separate objects
    /// </summary>
    public class Game : IDisposable, IInteractContainer, IConfig, IGame
    {
        #region Properties
        private CommandDispatcher Dispatcher { get; set; }
        private string _lastTitlePictureKey;
        public string GameEventLogPath { get; private set; }

        public delegate TResult _promptFunc<T, out TResult>(T input, out T output);

        public IStarTrekKGSettings Config { get; set; }
        public IInteraction Interact { get; set; }
        public IMap Map { get; set; }
        private Render PrintSector { get; set; }

        public Game._promptFunc<string, bool> Prompt { get; set; }

        public List<FactionThreat> LatestTaunts { get; set; } //todo: temporary until proper object is created
        public bool PlayerNowEnemyToFederation { get; set; } //todo: temporary until Starbase object is created

        public bool Started { get; set; }
        public bool GameOver { get; private set; }
        public bool IsWarGamesMode { get; private set; }
        public bool IsSystemsCascadeMode { get; private set; }
        public Point SystemsCascadeDestinationSector { get; private set; }

        private CascadeStatus _cascadeStatus;

        public int RandomFactorForTesting
        {
            get;
            set;
        }

        #endregion

        private sealed class CascadeStatus
        {
            public bool Triggered { get; set; }
            public int TurnsSinceTriggered { get; set; }
            public int EmergencyTurnsRemaining { get; set; }
            public int EmergencyPowerPerTurn { get; set; }
            public int SrsNoiseLines { get; set; }
            public int CrsNoiseLines { get; set; }
            public int LrsNoiseLevel { get; set; }
            public Dictionary<string, int> PowerLevels { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// todo: all game workflow functions go here (currently, workflow is ensconced within actors)
        /// and some unsorted crap at the moment..
        /// </summary>
        public Game(IStarTrekKGSettings config, bool startup = true)
            : this(config, null, startup)
        {
        }

        public Game(IStarTrekKGSettings config, SetupOptions startConfigOverride, bool startup = true)
        {
            this.RandomFactorForTesting = 0;
            this.PlayerNowEnemyToFederation = false;  //todo: resource this out.
            this.Config = config;
            if (this.Interact == null)
            {
                try
                {
                    this.Interact = new Interaction(config)
                    {
                        CurrentPrompt = "Enter Command:>" //todo: resource this (default prompt)
                    };

                    this.Prompt = (string s, out string output) => this.Interact.PromptUserSubscriber(s, out output);
                }
                catch (Exception)
                {
                    this.Interact = new Interaction()
                    {
                        Output = new SubscriberOutput(config),
                        OutputError = true,
                        CurrentPrompt = "Terminal: " //todo: resource this.
                    };

                    return;
                }
            }

            //The config file is loaded here, and persisted through the rest of the game. 
            //Any settings that are not in the config at this point, will not be updated unless some fault tolerance is built in that
            //might try to reload the file. #NotInThisVersion
            this.Config.Get = this.Config.GetConfig();

            if (startup)
            {
                this.InitializeStartup(startConfigOverride);
            }
        }

        private void InitializeStartup(SetupOptions startConfigOverride)
        {
            this.LatestTaunts = new List<FactionThreat>();
            this.InitializeGameEventLog();

            //These constants need to be localized to Game:
            this.GetConstants();

            this.PrintSector = new Render(this.Interact, this.Config);

            var startConfig = startConfigOverride ?? this.BuildDefaultSetupOptions();
            this.IsWarGamesMode = startConfig.IsWarGamesMode;
            this.IsSystemsCascadeMode = startConfig.IsSystemsCascadeMode;
            this.InitMap(startConfig, this);

            if (this.IsSystemsCascadeMode)
            {
                this.InitializeSystemsCascadeState(startConfig);
            }

            //We don't want to start game without hostiles (unless deterministic scenario mode explicitly allows it).
            if (!startConfig.StrictDeterministic && this.HostileCheck(this.Map))
            {
                return; //todo: unless we want to have a mode that allows it for some reason.
            }

            //Set initial color scheme
            this.Interact.HighlightTextBW(false);

            //todo: why are we creating this PrintSector() class a second time??
            this.Interact = new Interaction(this.Map.HostilesToSetUp, Map.starbases, Map.Stardate, Map.timeRemaining, this.Config);
            this.PrintSector = new Render(this.Interact, this.Config);
            this.RecordPlayerTurnSnapshot();
            this.AppendGameEventLog($"Game initialized. Mode={(this.IsSystemsCascadeMode ? "systems-cascade" : this.IsWarGamesMode ? "war-games" : "standard")} Stardate={this.Map?.Stardate} TimeRemaining={this.Map?.timeRemaining}");
        }

        private void InitializeGameEventLog()
        {
            this.GameEventLogPath = GameEventLog.GetDefaultPath();
            GameEventLog.Reset(this.GameEventLogPath);
        }

        public void AppendGameEventLog(string message)
        {
            GameEventLog.Append(this.GameEventLogPath, message);
        }

        public List<string> GetGameEventLogLines()
        {
            return GameEventLog.ReadAll(this.GameEventLogPath);
        }

        private SetupOptions BuildDefaultSetupOptions()
        {
            return new SetupOptions
            {
                Initialize = true,
                AddNebulae = true,
                AddDeuterium = this.GetOptionalFeatureFlag("enable-deuterium-sectors"),
                AddGraviticMines = this.GetOptionalFeatureFlag("enable-gravitic-mines"),
                CoordinateDefs = this.SectorSetup()
            };
        }

        private void InitMap(SetupOptions startConfig, IGame game)
        {
            this.Map = new Map(startConfig, new Interaction(this.Config), this.Config, game);
            this.Interact = new Interaction(this.Config);
        }

        private void InitializeSystemsCascadeState(SetupOptions setup)
        {
            this._cascadeStatus = new CascadeStatus
            {
                Triggered = false,
                TurnsSinceTriggered = 0,
                EmergencyTurnsRemaining = Math.Max(0, setup.SystemsCascadeEmergencyPowerTurns),
                EmergencyPowerPerTurn = Math.Max(0, setup.SystemsCascadeEmergencyPowerPerTurn),
                SrsNoiseLines = 0,
                CrsNoiseLines = 0,
                LrsNoiseLevel = 0
            };

            this.InitializeCascadePowerLevels();
            this.SetSystemsCascadeDestination(setup.SystemsCascadeDestinationDistance);
        }

        private void InitializeCascadePowerLevels()
        {
            if (this._cascadeStatus == null)
            {
                return;
            }

            this._cascadeStatus.PowerLevels.Clear();
            var systems = new[] { "nav", "srs", "crs", "lrs", "she", "com", "tor", "pha", "dmg" };
            foreach (var system in systems)
            {
                this._cascadeStatus.PowerLevels[system] = 100;
            }
        }

        private void SetSystemsCascadeDestination(int requestedDistance)
        {
            var distance = requestedDistance < 1 ? 1 : requestedDistance;
            var start = this.Map?.Playership?.Point;
            if (start == null)
            {
                this.SystemsCascadeDestinationSector = new Point(0, 0);
                return;
            }

            var candidates = new List<Point>();
            for (var x = DEFAULTS.SECTOR_MIN; x < DEFAULTS.SECTOR_MAX; x++)
            {
                for (var y = DEFAULTS.SECTOR_MIN; y < DEFAULTS.SECTOR_MAX; y++)
                {
                    var candidate = new Point(x, y);
                    var manhattan = Math.Abs(candidate.X - start.X) + Math.Abs(candidate.Y - start.Y);
                    if (manhattan == distance)
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            if (!candidates.Any())
            {
                var fallbackX = Math.Min(DEFAULTS.SECTOR_MAX - 1, start.X + distance);
                this.SystemsCascadeDestinationSector = new Point(fallbackX, start.Y);
                return;
            }

            this.SystemsCascadeDestinationSector = candidates[Utility.Utility.Random.Next(candidates.Count)];
        }

        public int GetSystemsCascadeSrsNoiseLines()
        {
            return this._cascadeStatus?.SrsNoiseLines ?? 0;
        }

        public int GetSystemsCascadeCrsNoiseLines()
        {
            return this._cascadeStatus?.CrsNoiseLines ?? 0;
        }

        public int GetSystemsCascadeLrsNoiseLevel()
        {
            return this._cascadeStatus?.LrsNoiseLevel ?? 0;
        }

        public bool TryHandleSystemsCascadeCommand(string command, out List<string> output)
        {
            output = null;

            if (!this.IsSystemsCascadeMode || string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            var normalized = command.Trim().ToLowerInvariant();

            if (normalized == "cascade status")
            {
                this.PrintSystemsCascadeStatus();
                output = this.Map.Playership.OutputQueue();
                return true;
            }

            if (normalized == "pwr help")
            {
                this.PrintSystemsCascadePowerHelp();
                output = this.Map.Playership.OutputQueue();
                return true;
            }

            if (normalized == "pwr status")
            {
                this.PrintSystemsCascadePowerStatus();
                output = this.Map.Playership.OutputQueue();
                return true;
            }

            var transferMatch = Regex.Match(normalized, @"^pwr\s+transfer\s+(?<amount>\d+)\s+(?<from>[a-z]+)\s+(?<to>[a-z]+)$", RegexOptions.IgnoreCase);
            if (transferMatch.Success)
            {
                var amount = int.Parse(transferMatch.Groups["amount"].Value);
                var from = transferMatch.Groups["from"].Value;
                var to = transferMatch.Groups["to"].Value;
                this.TransferCascadePower(from, to, amount);
                output = this.Map.Playership.OutputQueue();
                return true;
            }

            return false;
        }

        private void PrintSystemsCascadePowerHelp()
        {
            this.Interact.Line("Systems Cascade Power Routing");
            this.Interact.Line("Commands:");
            this.Interact.Line("  pwr status");
            this.Interact.Line("  pwr transfer [amount] [from] [to]");
            this.Interact.Line("Examples:");
            this.Interact.Line("  pwr transfer 25 she srs");
            this.Interact.Line("  pwr transfer 20 tor nav");
            this.Interact.Line(this.Map?.GameConfig?.SystemsCascadePowerHelpText ?? "Shift power to scanners or repairs while managing anomalies.");
        }

        private void PrintSystemsCascadePowerStatus()
        {
            if (this._cascadeStatus == null)
            {
                return;
            }

            this.Interact.Line("--- Power Allocation ---");
            foreach (var kv in this._cascadeStatus.PowerLevels.OrderBy(k => k.Key))
            {
                this.Interact.Line($"{kv.Key}: {kv.Value}");
            }

            this.Interact.Line($"SRS Noise Lines: {this._cascadeStatus.SrsNoiseLines}");
            this.Interact.Line($"CRS Noise Lines: {this._cascadeStatus.CrsNoiseLines}");
            this.Interact.Line($"LRS Brownout: {this._cascadeStatus.LrsNoiseLevel}%");
        }

        private void TransferCascadePower(string from, string to, int amount)
        {
            if (this._cascadeStatus == null)
            {
                return;
            }

            if (amount <= 0)
            {
                this.Interact.Line("Power transfer amount must be greater than zero.");
                return;
            }

            if (!this._cascadeStatus.PowerLevels.ContainsKey(from) || !this._cascadeStatus.PowerLevels.ContainsKey(to))
            {
                this.Interact.Line("Unknown system. Use pwr status to see valid system keys.");
                return;
            }

            if (from == to)
            {
                this.Interact.Line("Source and destination systems must be different.");
                return;
            }

            if (this._cascadeStatus.PowerLevels[from] < amount)
            {
                this.Interact.Line($"{from} does not have enough allocated power to transfer {amount}.");
                return;
            }

            this._cascadeStatus.PowerLevels[from] -= amount;
            this._cascadeStatus.PowerLevels[to] += amount;
            this.Interact.Line($"Transferred {amount} power from {from} to {to}.");

            this.RepairSubsystemFromPower(to, amount);
            this.PrintSystemsCascadePowerStatus();
        }

        private void RepairSubsystemFromPower(string targetSystem, int amount)
        {
            if (amount < 15 || this.Map?.Playership == null)
            {
                return;
            }

            switch (targetSystem)
            {
                case "srs":
                    this._cascadeStatus.SrsNoiseLines = Math.Max(0, this._cascadeStatus.SrsNoiseLines - 1);
                    if (this._cascadeStatus.SrsNoiseLines < this.GetScannerRows("SRSRows"))
                    {
                        ShortRangeScan.For(this.Map.Playership).Damage = 0;
                    }
                    break;
                case "crs":
                    this._cascadeStatus.CrsNoiseLines = Math.Max(0, this._cascadeStatus.CrsNoiseLines - 1);
                    if (this._cascadeStatus.CrsNoiseLines < this.GetScannerRows("CRSRows"))
                    {
                        CombinedRangeScan.For(this.Map.Playership).Damage = 0;
                    }
                    break;
                case "lrs":
                    this._cascadeStatus.LrsNoiseLevel = Math.Max(0, this._cascadeStatus.LrsNoiseLevel - 20);
                    if (this._cascadeStatus.LrsNoiseLevel < 100)
                    {
                        LongRangeScan.For(this.Map.Playership).Damage = 0;
                    }
                    break;
                case "she":
                    Shields.For(this.Map.Playership).Damage = Math.Max(0, Shields.For(this.Map.Playership).Damage - 1);
                    break;
                case "nav":
                    Navigation.For(this.Map.Playership).Damage = Math.Max(0, Navigation.For(this.Map.Playership).Damage - 1);
                    break;
                case "com":
                    Computer.For(this.Map.Playership).Damage = Math.Max(0, Computer.For(this.Map.Playership).Damage - 1);
                    break;
                case "tor":
                    Torpedoes.For(this.Map.Playership).Damage = Math.Max(0, Torpedoes.For(this.Map.Playership).Damage - 1);
                    break;
                case "pha":
                    Phasers.For(this.Map.Playership).Damage = Math.Max(0, Phasers.For(this.Map.Playership).Damage - 1);
                    break;
                case "dmg":
                    DamageControl.For(this.Map.Playership).Damage = Math.Max(0, DamageControl.For(this.Map.Playership).Damage - 1);
                    break;
            }
        }

        private void ApplySystemsCascadeTurnEffects(string command)
        {
            if (!this.IsSystemsCascadeMode || this._cascadeStatus == null || this.Map?.Playership == null)
            {
                return;
            }

            if (this._cascadeStatus.EmergencyTurnsRemaining > 0)
            {
                this.Map.Playership.Energy += this._cascadeStatus.EmergencyPowerPerTurn;
                this._cascadeStatus.EmergencyTurnsRemaining--;
                this.Interact.Line($"Engineering emergency generation added {this._cascadeStatus.EmergencyPowerPerTurn} power. Remaining boost turns: {this._cascadeStatus.EmergencyTurnsRemaining}");
            }

            var activeSector = this.Map.Sectors.GetActive();
            if (activeSector?.Type == SectorType.Nebulae)
            {
                this.ApplyNebulaRepairTick();
            }
            else if (this._cascadeStatus.Triggered)
            {
                var graceTurns = Math.Max(0, this.Map.GameConfig?.SystemsCascadeGraceTurns ?? 0);
                if (this._cascadeStatus.TurnsSinceTriggered >= graceTurns)
                {
                    var escalationChance = this.Map.GameConfig?.SystemsCascadeEscalationChancePercent ?? 25;
                    if (escalationChance < 0)
                    {
                        escalationChance = 0;
                    }
                    if (escalationChance > 100)
                    {
                        escalationChance = 100;
                    }

                    if (Utility.Utility.Random.Next(100) < escalationChance)
                    {
                        this.ApplyRandomCascadeEscalation();
                    }
                }
                else
                {
                    this.Interact.Line($"Cascade grace window active ({graceTurns - this._cascadeStatus.TurnsSinceTriggered} turn(s) remaining).");
                }

                this._cascadeStatus.TurnsSinceTriggered++;
            }

            this.ApplyCascadeScannerOfflineRules();
            this.PrintSystemsCascadeTurnSummary();
        }

        private void ApplyNebulaRepairTick()
        {
            if (this._cascadeStatus == null || this.Map?.Playership == null)
            {
                return;
            }

            this._cascadeStatus.SrsNoiseLines = Math.Max(0, this._cascadeStatus.SrsNoiseLines - 1);
            this._cascadeStatus.CrsNoiseLines = Math.Max(0, this._cascadeStatus.CrsNoiseLines - 1);
            this._cascadeStatus.LrsNoiseLevel = Math.Max(0, this._cascadeStatus.LrsNoiseLevel - 10);

            foreach (var subsystem in this.Map.Playership.Subsystems.Where(s => s.Damage > 0))
            {
                subsystem.Damage = Math.Max(0, subsystem.Damage - 1);
            }

            this.Interact.Line("Nebula cover is allowing engineering teams to stabilize damaged systems.");
        }

        private void ApplyRandomCascadeEscalation()
        {
            var roll = Utility.Utility.Random.Next(6);
            switch (roll)
            {
                case 0:
                    this._cascadeStatus.SrsNoiseLines++;
                    this.Interact.Line("Cascade ripple: SRS scan-lines increased.");
                    break;
                case 1:
                    this._cascadeStatus.CrsNoiseLines++;
                    this.Interact.Line("Cascade ripple: CRS scan-lines increased.");
                    break;
                case 2:
                    this._cascadeStatus.LrsNoiseLevel = Math.Min(100, this._cascadeStatus.LrsNoiseLevel + 15);
                    this.Interact.Line("Cascade ripple: LRS signal brownout intensified.");
                    break;
                case 3:
                    Shields.For(this.Map.Playership).Damage++;
                    this.Interact.Line("Cascade ripple: Shield control destabilized.");
                    break;
                case 4:
                    Navigation.For(this.Map.Playership).Damage++;
                    this.Interact.Line("Cascade ripple: Navigation matrix drifted out of tolerance.");
                    break;
                default:
                    Computer.For(this.Map.Playership).Damage++;
                    this.Interact.Line("Cascade ripple: Computer core timing fault detected.");
                    break;
            }
        }

        private void ApplyCascadeScannerOfflineRules()
        {
            if (this._cascadeStatus == null || this.Map?.Playership == null)
            {
                return;
            }

            var srsRows = this.GetScannerRows("SRSRows");
            var crsRows = this.GetScannerRows("CRSRows");

            if (this._cascadeStatus.SrsNoiseLines >= srsRows)
            {
                this._cascadeStatus.SrsNoiseLines = srsRows;
                ShortRangeScan.For(this.Map.Playership).Damage = 1;
                this.Interact.Line("SRS is now offline due to full-spectrum scan-line corruption.");
            }

            if (this._cascadeStatus.CrsNoiseLines >= crsRows)
            {
                this._cascadeStatus.CrsNoiseLines = crsRows;
                CombinedRangeScan.For(this.Map.Playership).Damage = 1;
                this.Interact.Line("CRS is now offline due to full-spectrum scan-line corruption.");
            }

            if (this._cascadeStatus.LrsNoiseLevel >= 100)
            {
                this._cascadeStatus.LrsNoiseLevel = 100;
                LongRangeScan.For(this.Map.Playership).Damage = 1;
                this.Interact.Line("LRS is now offline due to extreme signal brownout.");
            }
        }

        private int GetScannerRows(string configName)
        {
            try
            {
                return this.Config.GetSetting<int>(configName);
            }
            catch
            {
                return 8;
            }
        }

        public void TriggerSystemsCascadeFromAnomaly(string glyph)
        {
            if (!this.IsSystemsCascadeMode || this._cascadeStatus == null || this.Map?.Playership == null)
            {
                return;
            }

            this._cascadeStatus.Triggered = true;
            if (this._cascadeStatus.TurnsSinceTriggered == 0)
            {
                this.Interact.Line("Engineering note: temporary anti-cascade damping engaged while systems recalibrate.");
            }
            this.Interact.Line($"Energy anomaly '{glyph}' impacted the ship and initiated a systems cascade event.");

            var scannerNoisePerHit = Math.Max(1, this.Map.GameConfig?.SystemsCascadeAnomalyScannerNoisePerHit ?? 2);
            var lrsNoisePerHit = Math.Max(5, this.Map.GameConfig?.SystemsCascadeAnomalyLrsNoisePerHit ?? 20);
            var anomalyEnergyHit = Math.Max(0, this.Map.GameConfig?.SystemsCascadeAnomalyEnergyHit ?? 60);

            switch (glyph)
            {
                case "&":
                    this._cascadeStatus.SrsNoiseLines += scannerNoisePerHit;
                    this.Interact.Line("Effect: SRS scan-line distortion increased.");
                    break;
                case "%":
                    this._cascadeStatus.CrsNoiseLines += scannerNoisePerHit;
                    this.Interact.Line("Effect: CRS scan-line distortion increased.");
                    break;
                case "$":
                    Shields.For(this.Map.Playership).Damage += 1;
                    this.Map.Playership.Energy -= anomalyEnergyHit;
                    this.Interact.Line("Effect: Shield grid surge and reactor power bleed.");
                    break;
                case "@":
                    Navigation.For(this.Map.Playership).Damage += 1;
                    this.Interact.Line("Effect: Navigation subsystem destabilized.");
                    break;
                case "~":
                    this._cascadeStatus.LrsNoiseLevel = Math.Min(100, this._cascadeStatus.LrsNoiseLevel + lrsNoisePerHit);
                    this.Interact.Line("Effect: Long-range sensor brownout escalated.");
                    break;
                case "-":
                    var weaponSubsystem = Utility.Utility.Random.Next(2) == 0
                        ? (ISubsystem)Torpedoes.For(this.Map.Playership)
                        : Phasers.For(this.Map.Playership);
                    weaponSubsystem.Damage += 1;
                    this.Interact.Line($"Effect: {weaponSubsystem.Type} suffered a harmonic fault.");
                    break;
            }

            if (this._cascadeStatus.EmergencyTurnsRemaining > 0 && this.Map.Playership.Energy < 150)
            {
                this.Map.Playership.Energy = 150;
                this.Interact.Line("Emergency generation prevented a catastrophic power collapse.");
            }

            this.ApplyCascadeScannerOfflineRules();
        }

        private void PrintSystemsCascadeStatus()
        {
            if (!this.IsSystemsCascadeMode)
            {
                return;
            }

            var shipSector = this.Map.Playership?.Point;
            var destination = this.SystemsCascadeDestinationSector;
            this.Interact.Line("--- Systems Cascade Status ---");
            this.Interact.Line($"Current Sector: [{shipSector?.X},{shipSector?.Y}]");
            this.Interact.Line($"Destination Sector: [{destination?.X},{destination?.Y}]");
            this.Interact.Line($"Ship Energy: {this.Map.Playership?.Energy}");
            this.Interact.Line($"Cascade Triggered: {(this._cascadeStatus?.Triggered == true ? "Yes" : "No")}");
            this.PrintSystemsCascadePowerStatus();
        }

        private void PrintSystemsCascadeTurnSummary()
        {
            if (!this.IsSystemsCascadeMode || this.Map?.Playership == null || this._cascadeStatus == null || this.SystemsCascadeDestinationSector == null)
            {
                return;
            }

            var ship = this.Map.Playership;
            var destination = this.SystemsCascadeDestinationSector;
            var distance = Math.Abs(ship.Point.X - destination.X) + Math.Abs(ship.Point.Y - destination.Y);
            var cascadeState = this._cascadeStatus.Triggered ? "ACTIVE" : "IDLE";

            this.Interact.Line($"Cascade HUD: Dest[{destination.X},{destination.Y}] Dist={distance} Energy={ship.Energy} Cascade={cascadeState}");
            this.Interact.Line($"Cascade HUD: SRS lines={this._cascadeStatus.SrsNoiseLines} CRS lines={this._cascadeStatus.CrsNoiseLines} LRS brownout={this._cascadeStatus.LrsNoiseLevel}%");

            var tip = "Use 'pwr status' and 'pwr transfer 20 she srs'.";
            if (ship.GetSector().Type != SectorType.Nebulae)
            {
                tip += " Find a nebula to stabilize systems faster.";
            }

            this.Interact.Line("Cascade HUD Tip: " + tip);
        }

        private void ApplyEnvironmentalTurnEffects()
        {
            var ship = this.Map?.Playership as Ship;
            if (ship == null || ship.Destroyed)
            {
                return;
            }

            this.ApplySporeTurnEffects(ship);
            this.ApplyBlackHoleTurnEffects(ship);
        }

        private void ApplySporeTurnEffects(Ship ship)
        {
            if (!ship.SporeContaminated)
            {
                return;
            }

            var activeSector = ship.GetSector();
            var docked = Navigation.For(ship).Docked;
            var nearStarbase = this.Map.IsDockingLocation(ship.Coordinate.Y, ship.Coordinate.X, activeSector.Coordinates);
            if (docked || nearStarbase)
            {
                ship.SporeContaminated = false;
                this.Interact.Line("Starbase decontamination complete. Spore biofilm cleared.");
                return;
            }

            var drain = this.Config.GetSetting<int>("SporeDrainPerTurn");
            if (drain < 0)
            {
                drain = 0;
            }

            ship.Energy -= drain;
            this.Interact.Line($"Spore biofilm drains {drain} energy this turn.");

            if (ship.Energy <= 0)
            {
                ship.Energy = 0;
                ship.Destroyed = true;
                return;
            }

            var sides = this.Config.GetSetting<int>("SporeCureRollSides");
            if (sides < 1)
            {
                sides = 12;
            }

            if (Utility.Utility.Random.Next(1, sides + 1) == 1)
            {
                ship.SporeContaminated = false;
                this.Interact.Line("Engineering purge successful. Spore contamination has dissipated.");
            }
        }

        private void ApplyBlackHoleTurnEffects(Ship ship)
        {
            var concreteMap = this.Map as Map;
            if (concreteMap == null)
            {
                return;
            }

            concreteMap.TryApplyBlackHolePull(ship);
        }

        public void HandlePlayerUsedWormhole()
        {
            var delay = this.GetIntSettingOrDefault("BorgWormholeDelayTurns", 5);
            foreach (var borg in this.GetBorgShips().Where(b => b.BorgTrackingPlayer))
            {
                borg.BorgReacquireDelayTurnsRemaining = Math.Max(borg.BorgReacquireDelayTurnsRemaining, delay);
                borg.BorgArrivalTurnsRemaining = 0;
                borg.BorgLastKnownPlayerSector = this.CopyPoint(this.Map?.Playership?.Point);
            }
        }

        public void HandlePlayerEscapedTemporalRift()
        {
            this.LoseBorgTracking("Temporal rift interference severs Borg pursuit lock.");
        }

        public void HandlePlayerSectorVisibilityChange(Sector sector)
        {
            if (sector?.Type == SectorType.Nebulae)
            {
                this.LoseBorgTracking("Nebula cover breaks Borg sensor lock.");
            }
        }

        public bool IsPlayerImmobilizedByBorg(IShip ship)
        {
            if (ship == null || ship != this.Map?.Playership || ship.GetSector()?.Type == SectorType.Nebulae || this.IsBorgSuppressedByFigureEightZipBug(ship))
            {
                return false;
            }

            var range = this.GetIntSettingOrDefault("BorgAttackRange", 2);
            return this.GetBorgShips()
                .Any(borg => borg.Point.X == ship.Point.X &&
                             borg.Point.Y == ship.Point.Y &&
                             borg.BorgRepelledTurnsRemaining < 0 &&
                             this.GetCoordinateDistance(borg, ship) <= range);
        }

        public bool TryApplyBorgWeaponDamage(IShip target, int damage, string weaponName)
        {
            if (!(target is Ship borg) || borg.Faction != FactionName.Borg)
            {
                return false;
            }

            if (borg.BorgDamageableTurnsRemaining <= 0)
            {
                borg.Energy = borg.MaxEnergy;
                Shields.For(borg).Energy = this.GetIntSettingOrDefault("BorgShieldEnergy", borg.MaxEnergy);
                this.Interact.Line($"{borg.Name} adapts to your {weaponName}. No lasting damage registered.");
                return true;
            }

            var shields = Shields.For(borg);
            var remainingDamage = Math.Max(0, damage);
            if (remainingDamage > 0 && shields.Energy > 0)
            {
                var absorbed = Math.Min(shields.Energy, remainingDamage);
                shields.Energy -= absorbed;
                remainingDamage -= absorbed;
            }

            if (remainingDamage > 0)
            {
                borg.Energy = Math.Max(1, borg.Energy - remainingDamage);
            }

            if (string.Equals(weaponName, "torpedo", StringComparison.OrdinalIgnoreCase))
            {
                borg.BorgRepelledTurnsRemaining = Math.Max(borg.BorgRepelledTurnsRemaining, 2);
                this.Interact.Line($"{borg.Name} is repelled. You have a brief escape window.");
            }

            this.Interact.Line($"{borg.Name} absorbs the {weaponName}. Borg integrity now E:{borg.Energy}/S:{shields.Energy}.");
            return true;
        }

        private void ApplyBorgTurnEffects()
        {
            var player = this.Map?.Playership;
            if (player == null || player.Destroyed)
            {
                return;
            }

            var borgShips = this.GetBorgShips().ToList();
            if (!borgShips.Any())
            {
                return;
            }

            var currentSector = player.GetSector();
            if (currentSector?.Type == SectorType.Nebulae)
            {
                this.LoseBorgTracking("Nebula cover breaks Borg sensor lock.");
                return;
            }

            foreach (var borg in borgShips)
            {
                if (borg.Destroyed)
                {
                    continue;
                }

                if (borg.Point.X == player.Point.X && borg.Point.Y == player.Point.Y)
                {
                    this.EnsureBorgTracking(borg);
                    this.ResolveBorgSameSectorTurn(borg, player);
                    continue;
                }

                if (!borg.BorgTrackingPlayer)
                {
                    continue;
                }

                this.UpdateBorgPursuitTarget(borg, player);
                if (borg.BorgReacquireDelayTurnsRemaining > 0)
                {
                    borg.BorgReacquireDelayTurnsRemaining--;
                    continue;
                }

                if (borg.BorgArrivalTurnsRemaining > 0)
                {
                    borg.BorgArrivalTurnsRemaining--;
                    if (borg.BorgArrivalTurnsRemaining > 0)
                    {
                        continue;
                    }
                }

                this.MoveBorgToPlayerSector(borg, player);
            }
        }

        private IEnumerable<Ship> GetBorgShips()
        {
            return this.Map?.Sectors?.GetHostiles()
                ?.OfType<Ship>()
                .Where(ship => ship.Faction == FactionName.Borg && !ship.Destroyed) ?? Enumerable.Empty<Ship>();
        }

        private void LoseBorgTracking(string message)
        {
            var borgShips = this.GetBorgShips().Where(b => b.BorgTrackingPlayer).ToList();
            if (!borgShips.Any())
            {
                return;
            }

            foreach (var borg in borgShips)
            {
                borg.BorgTrackingPlayer = false;
                borg.BorgArrivalTurnsRemaining = 0;
                borg.BorgReacquireDelayTurnsRemaining = 0;
                borg.BorgLastKnownPlayerSector = null;
                borg.BorgDamageableTurnsRemaining = 0;
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                this.Interact.Line(message);
            }
        }

        private void EnsureBorgTracking(Ship borg)
        {
            if (borg.BorgTrackingPlayer)
            {
                return;
            }

            borg.BorgTrackingPlayer = true;
            borg.BorgArrivalTurnsRemaining = 0;
            borg.BorgReacquireDelayTurnsRemaining = 0;
            borg.BorgDamageableTurnsRemaining = this.GetIntSettingOrDefault("BorgInitialDamageableTurns", 2);
            borg.BorgLastKnownPlayerSector = this.CopyPoint(this.Map?.Playership?.Point);
            this.Interact.Line($"{borg.Name} has adapted to your presence. Evasion is recommended.");
        }

        private void UpdateBorgPursuitTarget(Ship borg, IShip player)
        {
            if (borg.BorgLastKnownPlayerSector != null &&
                borg.BorgLastKnownPlayerSector.X == player.Point.X &&
                borg.BorgLastKnownPlayerSector.Y == player.Point.Y)
            {
                return;
            }

            borg.BorgLastKnownPlayerSector = this.CopyPoint(player.Point);
            borg.BorgArrivalTurnsRemaining = this.GetIntSettingOrDefault("BorgPursuitDelayTurns", 2);
        }

        private void MoveBorgToPlayerSector(Ship borg, IShip player)
        {
            var playerSector = player.GetSector();
            var destination = playerSector?.Coordinates?
                .Where(c => c.Item == CoordinateItem.Empty)
                .OrderByDescending(c => Math.Abs(c.X - player.Coordinate.X) + Math.Abs(c.Y - player.Coordinate.Y))
                .FirstOrDefault();
            if (destination == null)
            {
                return;
            }

            var oldSector = borg.GetSector();
            var oldCoordinate = oldSector.Coordinates.GetNoError(new Point(borg.Coordinate.X, borg.Coordinate.Y));
            if (oldCoordinate != null && oldCoordinate.Object == borg)
            {
                oldCoordinate.Item = CoordinateItem.Empty;
                oldCoordinate.Object = null;
            }

            borg.Point = new Point(playerSector.X, playerSector.Y);
            borg.Coordinate = destination;
            destination.Item = CoordinateItem.HostileShip;
            destination.Object = borg;
            borg.BorgArrivalTurnsRemaining = 0;

            this.Interact.Line($"{borg.Name} emerges in this sector on an intercept vector.");
            this.AppendGameEventLog($"{borg.Name} arrived in sector [{playerSector.X},{playerSector.Y}]");
        }

        private void ResolveBorgSameSectorTurn(Ship borg, IShip player)
        {
            if (borg.BorgDamageableTurnsRemaining > 0)
            {
                borg.BorgDamageableTurnsRemaining--;
            }
            else
            {
                borg.Energy = borg.MaxEnergy;
                Shields.For(borg).Energy = this.GetIntSettingOrDefault("BorgShieldEnergy", borg.MaxEnergy);
            }

            if (borg.BorgRepelledTurnsRemaining >= 0)
            {
                borg.BorgRepelledTurnsRemaining--;
                if (borg.BorgRepelledTurnsRemaining >= 0)
                {
                    return;
                }
            }

            if (this.IsBorgSuppressedByFigureEightZipBug(player))
            {
                this.Interact.Line($"{borg.Name} hesitates under Zip Bug interference.");
                return;
            }

            if (this.TryResolveBorgBlackHoleLure(borg, player) || this.TryResolveBorgWormholeLure(borg, player))
            {
                return;
            }

            var range = this.GetIntSettingOrDefault("BorgAttackRange", 2);
            if (this.GetCoordinateDistance(borg, player) > range)
            {
                this.MoveBorgTowardPlayer(borg, player);
            }

            if (this.GetCoordinateDistance(borg, player) <= range)
            {
                var drainPercent = this.GetIntSettingOrDefault("BorgPowerDrainPercent", 35);
                var drain = Math.Max(1, (int)Math.Ceiling(player.Energy * (drainPercent / 100.0)));
                player.Energy = Math.Max(0, player.Energy - drain);
                this.Interact.Line($"{borg.Name} locks down your ship and drains {drain} energy.");
                if (player.Energy <= 0)
                {
                    player.Destroyed = true;
                }
            }
        }

        private void MoveBorgTowardPlayer(Ship borg, IShip player)
        {
            var dx = Math.Sign(player.Coordinate.X - borg.Coordinate.X);
            var dy = Math.Sign(player.Coordinate.Y - borg.Coordinate.Y);
            var target = borg.GetSector().Coordinates.GetNoError(new Point(borg.Coordinate.X + dx, borg.Coordinate.Y + dy));
            if (target == null || target.Item != CoordinateItem.Empty)
            {
                return;
            }

            var origin = borg.GetSector().Coordinates.GetNoError(new Point(borg.Coordinate.X, borg.Coordinate.Y));
            if (origin != null && origin.Object == borg)
            {
                origin.Item = CoordinateItem.Empty;
                origin.Object = null;
            }

            borg.Coordinate = target;
            target.Item = CoordinateItem.HostileShip;
            target.Object = borg;
        }

        private bool TryResolveBorgBlackHoleLure(Ship borg, IShip player)
        {
            var sector = borg.GetSector();
            var blackHole = this.FindIntermediateHazard(sector, borg, player, CoordinateItem.BlackHole);
            if (blackHole == null)
            {
                return false;
            }

            var percent = this.GetIntSettingOrDefault("BorgBlackHoleLurePercent", 25);
            if (Utility.Utility.Random.Next(100) >= percent)
            {
                return false;
            }

            var origin = sector.Coordinates.GetNoError(new Point(borg.Coordinate.X, borg.Coordinate.Y));
            if (origin != null && origin.Object == borg)
            {
                origin.Item = CoordinateItem.Empty;
                origin.Object = null;
            }

            borg.Destroyed = true;
            this.Interact.Line($"{borg.Name} is pulled into a black hole.");
            this.AppendGameEventLog($"{borg.Name} destroyed by black hole lure");
            return true;
        }

        private bool TryResolveBorgWormholeLure(Ship borg, IShip player)
        {
            var sector = borg.GetSector();
            var wormholeCoordinate = this.FindIntermediateHazard(sector, borg, player, CoordinateItem.Wormhole);
            if (wormholeCoordinate?.Object is not Wormhole wormhole)
            {
                return false;
            }

            var percent = this.GetIntSettingOrDefault("BorgWormholeLurePercent", 100);
            if (Utility.Utility.Random.Next(100) >= percent)
            {
                return false;
            }

            var destinationSector = this.Map.Sectors[wormhole.DestinationSector];
            var destination = destinationSector?.Coordinates?
                .Where(c => c.Item == CoordinateItem.Empty)
                .OrderBy(_ => Utility.Utility.Random.Next())
                .FirstOrDefault();
            if (destination == null)
            {
                return false;
            }

            var oldCoordinate = sector.Coordinates.GetNoError(new Point(borg.Coordinate.X, borg.Coordinate.Y));
            if (oldCoordinate != null && oldCoordinate.Object == borg)
            {
                oldCoordinate.Item = CoordinateItem.Empty;
                oldCoordinate.Object = null;
            }

            borg.Point = new Point(destinationSector.X, destinationSector.Y);
            borg.Coordinate = destination;
            destination.Item = CoordinateItem.HostileShip;
            destination.Object = borg;
            borg.BorgReacquireDelayTurnsRemaining = this.GetIntSettingOrDefault("BorgWormholeDelayTurns", 5);
            borg.BorgArrivalTurnsRemaining = 0;
            borg.BorgLastKnownPlayerSector = this.CopyPoint(player.Point);
            this.Interact.Line($"{borg.Name} is diverted through a wormhole. Temporary respite achieved.");
            return true;
        }

        private Coordinate FindIntermediateHazard(Sector sector, IShip borg, IShip player, CoordinateItem hazard)
        {
            if (sector?.Coordinates == null || borg?.Coordinate == null || player?.Coordinate == null)
            {
                return null;
            }

            if (borg.Coordinate.X == player.Coordinate.X)
            {
                var minY = Math.Min(borg.Coordinate.Y, player.Coordinate.Y) + 1;
                var maxY = Math.Max(borg.Coordinate.Y, player.Coordinate.Y);
                return sector.Coordinates
                    .Where(c => c.X == borg.Coordinate.X && c.Y >= minY && c.Y < maxY && c.Item == hazard)
                    .FirstOrDefault();
            }

            if (borg.Coordinate.Y == player.Coordinate.Y)
            {
                var minX = Math.Min(borg.Coordinate.X, player.Coordinate.X) + 1;
                var maxX = Math.Max(borg.Coordinate.X, player.Coordinate.X);
                return sector.Coordinates
                    .Where(c => c.Y == borg.Coordinate.Y && c.X >= minX && c.X < maxX && c.Item == hazard)
                    .FirstOrDefault();
            }

            return null;
        }

        private int GetCoordinateDistance(IShip left, IShip right)
        {
            return Math.Max(Math.Abs(left.Coordinate.X - right.Coordinate.X), Math.Abs(left.Coordinate.Y - right.Coordinate.Y));
        }

        private Point CopyPoint(Point point)
        {
            return point == null ? null : new Point(point.X, point.Y);
        }

        public bool HandleZipBugShot(ICoordinate coordinate, string weaponName)
        {
            if (coordinate?.Object is not ZipBug zipBug)
            {
                return false;
            }

            this.Interact.Line($"{zipBug.Name} flickers away from your {weaponName} fire.");
            this.RelocateZipBug(zipBug);
            return true;
        }

        public bool CanTransferFlagToBorg(IShip playerShip, IShip targetShip)
        {
            return playerShip != null &&
                   targetShip is Ship borgShip &&
                   borgShip.Faction == FactionName.Borg &&
                   this.IsBorgSuppressedByFigureEightZipBug(playerShip);
        }

        public void PlayerFleetSupportFire(ISector activeSector)
        {
            this.FriendlyShipsSupportPlayer(this.Map, activeSector);
        }

        private void ApplyZipBugTurnEffects()
        {
            var player = this.Map?.Playership;
            if (player == null || player.Destroyed)
            {
                return;
            }

            foreach (var zipBug in this.GetZipBugs())
            {
                zipBug.TurnsInCurrentSector++;

                if (zipBug.Coordinate?.SectorDef != null &&
                    zipBug.Coordinate.SectorDef.X == player.Point.X &&
                    zipBug.Coordinate.SectorDef.Y == player.Point.Y)
                {
                    if (zipBug.Form == ZipBug.ZipBugForm.FigureEight)
                    {
                        var sectorBonus = this.GetIntSettingOrDefault("ZipBugFigureEightEnergyPerTurn", 200);
                        player.Energy += sectorBonus;
                        this.Interact.Line($"{zipBug.Name} renews your energy by {sectorBonus}.");
                    }

                    this.MoveZipBugWithinSector(zipBug, player);

                    var adjacent = this.IsZipBugAdjacentToPlayer(zipBug, player);
                    if (adjacent && !zipBug.WasAdjacentToPlayerLastTurn)
                    {
                        var bonus = this.GetIntSettingOrDefault("ZipBugAdjacentEnergyBonus", 1000);
                        player.Energy += bonus;
                        this.Interact.Line($"{zipBug.Name} alights nearby. Energy gain: {bonus}.");
                    }

                    zipBug.WasAdjacentToPlayerLastTurn = adjacent;
                }
                else
                {
                    zipBug.WasAdjacentToPlayerLastTurn = false;
                }

                var maxTurns = this.GetIntSettingOrDefault("ZipBugMaxTurnsPerSector", 3);
                if (zipBug.TurnsInCurrentSector >= maxTurns)
                {
                    this.RelocateZipBug(zipBug);
                }
            }
        }

        private IEnumerable<ZipBug> GetZipBugs()
        {
            return this.Map?.Sectors?
                .SelectMany(s => s?.Coordinates ?? Enumerable.Empty<Coordinate>())
                .Where(c => c?.Object is ZipBug)
                .Select(c => (ZipBug)c.Object)
                .ToList() ?? Enumerable.Empty<ZipBug>();
        }

        private void MoveZipBugWithinSector(ZipBug zipBug, IShip player)
        {
            var sector = this.Map.Sectors[new Point(zipBug.Coordinate.SectorDef.X, zipBug.Coordinate.SectorDef.Y)];
            var candidates = sector.Coordinates
                .Where(c => c.Item == CoordinateItem.Empty)
                .Where(c => !(c.X == player.Coordinate.X && c.Y == player.Coordinate.Y))
                .OrderBy(c => Math.Abs(c.X - player.Coordinate.X) + Math.Abs(c.Y - player.Coordinate.Y))
                .ThenBy(_ => Utility.Utility.Random.Next())
                .ToList();
            if (!candidates.Any())
            {
                return;
            }

            var target = candidates.First();
            var origin = sector.Coordinates.GetNoError(new Point(zipBug.Coordinate.X, zipBug.Coordinate.Y));
            if (origin != null && origin.Object == zipBug)
            {
                origin.Item = CoordinateItem.Empty;
                origin.Object = null;
            }

            zipBug.Coordinate = target;
            target.Item = CoordinateItem.ZipBug;
            target.Object = zipBug;
        }

        private bool IsPlayerProtectedByZipBug(IShip playerShip, out ZipBug zipBug)
        {
            zipBug = this.GetZipBugs()
                .FirstOrDefault(bug => bug.Coordinate?.SectorDef != null &&
                                       bug.Coordinate.SectorDef.X == playerShip.Point.X &&
                                       bug.Coordinate.SectorDef.Y == playerShip.Point.Y &&
                                       this.IsZipBugAdjacentToPlayer(bug, playerShip));
            return zipBug != null;
        }

        private bool IsBorgSuppressedByFigureEightZipBug(IShip playerShip)
        {
            return this.GetZipBugs().Any(bug =>
                bug.Form == ZipBug.ZipBugForm.FigureEight &&
                bug.Coordinate?.SectorDef != null &&
                playerShip?.Point != null &&
                bug.Coordinate.SectorDef.X == playerShip.Point.X &&
                bug.Coordinate.SectorDef.Y == playerShip.Point.Y);
        }

        private bool IsZipBugAdjacentToPlayer(ZipBug zipBug, IShip player)
        {
            if (zipBug?.Coordinate == null || player?.Coordinate == null)
            {
                return false;
            }

            var dx = Math.Abs(zipBug.Coordinate.X - player.Coordinate.X);
            var dy = Math.Abs(zipBug.Coordinate.Y - player.Coordinate.Y);
            return dx <= 1 && dy <= 1 && !(dx == 0 && dy == 0);
        }

        private void RelocateZipBug(ZipBug zipBug)
        {
            if (zipBug?.Coordinate?.SectorDef == null || this.Map?.Sectors == null)
            {
                return;
            }

            var originSector = this.Map.Sectors[new Point(zipBug.Coordinate.SectorDef.X, zipBug.Coordinate.SectorDef.Y)];
            var origin = originSector?.Coordinates?.GetNoError(new Point(zipBug.Coordinate.X, zipBug.Coordinate.Y));
            if (origin != null && origin.Object == zipBug)
            {
                origin.Item = CoordinateItem.Empty;
                origin.Object = null;
            }

            var destinationSector = this.Map.Sectors
                .Where(s => s != null && s.Coordinates.Any(c => c.Item == CoordinateItem.Empty))
                .OrderBy(_ => Utility.Utility.Random.Next())
                .FirstOrDefault();
            var destination = destinationSector?.Coordinates
                .Where(c => c.Item == CoordinateItem.Empty)
                .OrderBy(_ => Utility.Utility.Random.Next())
                .FirstOrDefault();
            if (destination == null)
            {
                return;
            }

            zipBug.ResetForRelocation();
            zipBug.Coordinate = destination;
            destination.Item = CoordinateItem.ZipBug;
            destination.Object = zipBug;
            this.ApplyZipBugAppearanceMaxEnergyBonus();
        }

        private void ApplyZipBugAppearanceMaxEnergyBonus()
        {
            if (this.Map?.Playership is not Ship player)
            {
                return;
            }

            var bonus = this.GetIntSettingOrDefault("ZipBugMaxEnergyAppearanceBonus", 100);
            if (bonus <= 0)
            {
                return;
            }

            player.MaxEnergy += bonus;
        }

        private void DepleteShipWeapons(IShip ship)
        {
            if (ship == null)
            {
                return;
            }

            Torpedoes.For(ship).Count = 0;
            Torpedoes.For(ship).Damage = Math.Max(1, Torpedoes.For(ship).Damage);
            Phasers.For(ship).Damage = Math.Max(1, Phasers.For(ship).Damage);

            var disruptors = ship.Subsystems.SingleOrDefault(s => s.Type == SubsystemType.Disruptors);
            if (disruptors != null)
            {
                disruptors.Damage = Math.Max(1, disruptors.Damage);
            }
        }

        private void RecordPlayerTurnSnapshot()
        {
            var ship = this.Map?.Playership as Ship;
            if (ship?.Point == null || ship.Coordinate == null)
            {
                return;
            }

            ship.TurnHistory ??= new List<LocationDef>();

            ship.TurnHistory.Add(new LocationDef(
                new Point(ship.Point.X, ship.Point.Y),
                new Point(ship.Coordinate.X, ship.Coordinate.Y)));

            var maxSnapshots = this.Config.GetSetting<int>("TemporalHistoryDepth");
            if (maxSnapshots < 1)
            {
                maxSnapshots = 20;
            }

            if (ship.TurnHistory.Count > maxSnapshots)
            {
                ship.TurnHistory.RemoveAt(0);
            }
        }

        #region Turn System

        #region Web

        /// <summary>
        /// Starts the game in Web mode.  Shows start screen
        /// </summary>
        public void RunSubscriber()
        {
            this.Start();
            this.PrintOpeningScreen();
        }

        /// <summary>
        /// This method reads the passed command and executes the appropriate  game resources
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public List<string> SubscriberSendAndGetResponse(string command)
        {
            this.Interact.Output.Clear();
            try
            {
                var stardateBefore = this.Map?.Stardate ?? 0;
                if (command.StartsWith("wrp ", StringComparison.OrdinalIgnoreCase))
                {
                    this.Dispatcher.HandleInput(command, this.Map.Playership);
                    if ((this.Map?.Stardate ?? 0) > stardateBefore)
                    {
                        this.MoveQuadrantFriendlies();
                    }
                    return this.Map.Playership.OutputQueue();
                }

                var retVal = this.Interact.ReadAndOutput(this.Map.Playership, this.Map.Text, command);

                if (retVal == null)
                {
                    retVal = new List<string>();
                }
                else
                {
                    this.ApplySystemsCascadeTurnEffects(command);
                    this.ApplyEnvironmentalTurnEffects();
                    this.ApplyBorgTurnEffects();
                    this.ApplyZipBugTurnEffects();
                    this.ReportGameStatus();
                    this.RecordPlayerTurnSnapshot();
                    if ((this.Map?.Stardate ?? 0) > stardateBefore)
                    {
                        this.MoveQuadrantFriendlies();
                    }
                    retVal = this.DeduplicateRepeatedScanOutput(this.Map.Playership.OutputQueue());
                }

                return retVal;
            }
            catch (Exception ex)
            {
                //todo: make this happen only if in debugmode
                this.Interact.Output.WriteLine($"There is an issue with the game. {ex.Message}");
                return this.Interact.Output.Queue.ToList();
            }
        }




        #endregion

        #endregion

        #region Setup

        private void Start()
        {
            this.Started = true;
            this.Interact.ResetPrompt();
            this.CommandInit();
        }

        private void CommandInit()
        {
            var repository = new CommandRepository(this.Config);
            this.Dispatcher = new CommandDispatcher(repository);

            this.Dispatcher.RegisterHandler("wrp", "set course", (ship, val) => {
                ship.NavigationSubsystem.SetCourse(val);
            });

            this.Dispatcher.RegisterHandler("wrp", "set speed", (ship, val) => {
                ship.NavigationSubsystem.SetWarpSpeed(val);
            });

            this.Dispatcher.RegisterHandler("wrp", "engage", (ship, val) => {
                ship.NavigationSubsystem.EngageWarp();
            });
        }


        //private void Initialize()
        //{
        //    this.Started = true;
        //    this.Interact.ResetPrompt();

        //    //TODO:  we can possibly reorder the baddies in this.Map.GameConfig..
        //    this.Map.Initialize(this.Map.GameConfig.CoordinateDefs, this.Map.GameConfig.AddNebulae); //we gonna start over
        //}

        private void GetConstants()
        {
            DEFAULTS.DEBUG_MODE = this.Config.GetSetting<bool>("DebugMode");

            if (DEFAULTS.DEBUG_MODE)
            {
                this.Interact.Line("// ---------------- Debug Mode ----------------");
            }

            DEFAULTS.COORDINATE_MIN = this.Config.GetSetting<int>("COORDINATE_MIN");
            DEFAULTS.COORDINATE_MAX = this.Config.GetSetting<int>("COORDINATE_MAX");

            DEFAULTS.SECTOR_MIN = this.Config.GetSetting<int>("SECTOR_MIN");
            DEFAULTS.SECTOR_MAX = this.Config.GetSetting<int>("SECTOR_MAX");

            DEFAULTS.SHIELDS_DOWN_LEVEL = this.Config.GetSetting<int>("ShieldsDownLevel");
            DEFAULTS.LOW_ENERGY_LEVEL = this.Config.GetSetting<int>("LowEnergyLevel");

            DEFAULTS.PLAYERSHIP = this.Config.GetSetting<string>("PlayerShipGlyph");
            DEFAULTS.STAR = this.Config.GetSetting<string>("StarGlyph");
            DEFAULTS.STARBASE = this.Config.GetSetting<string>("StarbaseGlyph");
            DEFAULTS.ALLY = this.Config.GetSetting<string>("AllyGlyph");
        }

        private bool GetOptionalFeatureFlag(string settingName)
        {
            try
            {
                return this.Config.GetSetting<bool>(settingName);
            }
            catch
            {
                // Missing feature key defaults to disabled.
                return false;
            }
        }

        private CoordinateDefs SectorSetup()
        {

            //todo: these CoordinateDefs can be computed somewhere
            //todo: this make a GetSectorDefsFromAppConfig()
            //todo: Output a message if GetSectorDefsFromAppConfig() fails, then use hardcoded setup and start game anyway

            return DefaultHardcodedSetup();
        }

        /// <summary>
        /// This is the setup we get if app config can not be read for some reason (or it is buggy)
        /// </summary>
        /// <returns></returns>
        private CoordinateDefs DefaultHardcodedSetup()
        {
            //todo: get rid of this.  generate on the fly!
            //todo: this needs to be  in a config file

            return new CoordinateDefs
                       {
                           //This tells us what Types of items will be generated at start.  if Points are passed, that is an
                           //indicator that an individual object needs to be placed, istead of generated objects from config file.

                           //todo: get rid of that second, stupid parameter.
                           new CoordinateDef(CoordinateItem.PlayerShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.HostileShip),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Starbase),
                           new CoordinateDef(CoordinateItem.Star),
                       };
        }

        #endregion

        #region Title

        /// <summary>
        /// Prints title and sets up the playfield.
        /// This is where the Map is created, and references to it are passed around from here on.
        /// </summary>
        private void PrintOpeningScreen()
        {
            this.RandomAppTitle(); //Printing the title at this point is really a debug step. (it shows that the game is started.  Otherwise, it could go after initialization)

            this.Interact.ResourceLine(this.Config.GetText("AppVersion").TrimStart(' '), "UnderConstructionMessage");
            if (this.IsSystemsCascadeMode)
            {
                this.PrintSystemsCascadeBriefing();
            }
            else
            {
                this.Interact.PrintMission();
            }
        }

        private void PrintSystemsCascadeBriefing()
        {
            this.Interact.Line("SYSTEMS CASCADE MODE");
            this.Interact.Line($"Destination Sector: [{this.SystemsCascadeDestinationSector.X},{this.SystemsCascadeDestinationSector.Y}]");
            this.Interact.Line("Objective: Reach destination while surviving cascade failures.");
            this.Interact.Line("Use scans to locate deuterium and avoid/prepare for energy anomalies.");
            this.Interact.Line("Nebula sectors contain no anomalies and support field repairs.");
            this.Interact.Line("Shields are ineffective in nebulae.");
            this.Interact.Line("Power transfer: pwr transfer [amount] [from] [to]");
            this.Interact.Line("Power status: pwr status");

            var introLines = this.Map?.GameConfig?.SystemsCascadeIntroLines;
            if (introLines != null)
            {
                foreach (var introLine in introLines.Where(line => !string.IsNullOrWhiteSpace(line)))
                {
                    this.Interact.Line(introLine);
                }
            }
        }

        private void RandomAppTitle()
        {
            int randomVal = Utility.Utility.Random.Next(3);

            switch (randomVal)
            {
                case 0:
                    this.AppTitleItem("Classic", 7);
                    break;

                case 2:
                    this.AppTitleItem("TNG", 7);
                    break;

                default:
                    this.AppTitleItem("Movie", 7);
                    break;
            }

            this.Interact.Resource("AppTitleSpace");

            this.RandomPicture();

            this.Interact.Resource("AppTitleSpace");
        }

        public void ShowRandomTitle()
        {
            this.RandomAppTitle();
        }

        private void RandomPicture()
        {
            var forcedPictureKey = this.Map?.GameConfig?.OpeningPictureKey;
            if (!string.IsNullOrWhiteSpace(forcedPictureKey))
            {
                var forcedPicture = this.GetAppTitlePictureDefs()
                    .FirstOrDefault(p => string.Equals(p.Item1, forcedPictureKey, StringComparison.OrdinalIgnoreCase));

                if (forcedPicture != null)
                {
                    this._lastTitlePictureKey = forcedPicture.Item1;
                    this.AppTitleItem(forcedPicture.Item1, forcedPicture.Item2);
                    return;
                }
            }

            List<Tuple<string, int>> pictures = this.GetAppTitlePictureDefs();
            if (pictures.Count == 0)
            {
                this.AppTitleItem("2ShipsSmall", 7);
                return;
            }

            int randomVal = Utility.Utility.Random.Next(pictures.Count);
            Tuple<string, int> pick = pictures[randomVal];
            if (!string.IsNullOrWhiteSpace(this._lastTitlePictureKey) && pictures.Count > 1)
            {
                if (string.Equals(this._lastTitlePictureKey, pick.Item1, StringComparison.OrdinalIgnoreCase))
                {
                    randomVal = (randomVal + 1) % pictures.Count;
                    pick = pictures[randomVal];
                }
            }

            this._lastTitlePictureKey = pick.Item1;
            this.AppTitleItem(pick.Item1, pick.Item2);
        }

        private List<Tuple<string, int>> GetAppTitlePictureDefs()
        {
            Dictionary<string, int> results = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            string[] exclude = { "Classic", "Movie", "TNG", "Space" };

            foreach (System.Configuration.ConfigurationElement element in this.Config.Get.ConsoleText)
            {
                Config.Elements.NameValue nameValue = (Config.Elements.NameValue)element;
                string key = nameValue.name;

                if (!key.StartsWith("AppTitle", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Match match = Regex.Match(key, @"^AppTitle(?<name>.+?)(?<index>\d+)$");
                if (!match.Success)
                {
                    continue;
                }

                string baseName = match.Groups["name"].Value;
                bool excluded = exclude.Any(e => string.Equals(e, baseName, StringComparison.OrdinalIgnoreCase));
                if (excluded)
                {
                    continue;
                }

                int index;
                if (!int.TryParse(match.Groups["index"].Value, out index))
                {
                    continue;
                }

                int currentMax;
                if (!results.TryGetValue(baseName, out currentMax) || index > currentMax)
                {
                    results[baseName] = index;
                }
            }

            List<Tuple<string, int>> pictures = new List<Tuple<string, int>>();
            foreach (KeyValuePair<string, int> entry in results)
            {
                // AppTitleItem prints 1..(endingLine-1)
                pictures.Add(Tuple.Create(entry.Key, entry.Value + 1));
            }

            return pictures;
        }

        private void AppTitleItem(string itemName, int endingLine)
        {
            for (int i = 1; i < endingLine; i++)
            {
                this.Interact.Resource($"AppTitle{itemName}{i}");
            }
        }

        #endregion

        #region Attacks

        /// <summary>
        /// At the end of each turn, if a hostile is in the same sector with Playership, it will attack.  If there are 37, then all 37 will..
        /// TODO: this needs to be changed.  after destruction, it appears to take several method returns to realize that we are dead.
        /// </summary>
        /// <returns></returns>
        public void ALLHostilesAttack(IMap map)
        {
            //todo: centralize this.
            //this is called from torpedo control/phaser control, and navigation control

            var returnValue = false;
            var activeSector = map.Sectors.GetActive();
            var hostilesAttacking = activeSector.GetHostiles();

            this.HostileStarbasesAttack(map, activeSector);

            returnValue = this.HostileShipsAttack(map, hostilesAttacking, returnValue);
            if (returnValue)
            {
                this.FriendlyShipsSupportPlayer(map, activeSector);
            }
        }

        private void FriendlyShipsSupportPlayer(IMap map, ISector activeSector)
        {
            if (activeSector?.Coordinates == null)
            {
                return;
            }

            var hostiles = activeSector.GetHostiles().Where(h => h?.Destroyed != true).ToList();
            if (!hostiles.Any())
            {
                return;
            }

            var friendlies = activeSector.Coordinates
                .Where(c => c?.Object is IShip ship &&
                            ship != map.Playership &&
                            !ship.Destroyed &&
                            ship.Allegiance == Allegiance.GoodGuy)
                .Select(c => (IShip)c.Object)
                .ToList();

            var destroyed = new List<IShip>();
            foreach (var friendly in friendlies)
            {
                var target = hostiles.FirstOrDefault(h => h?.Destroyed != true);
                if (target == null)
                {
                    break;
                }

                var attackEnergy = 150 + Utility.Utility.TestableRandom(this, 100, 100);
                this.Interact.Line($"{friendly.Name} fires on {target.Name}.");
                if (target.Faction == FactionName.Borg)
                {
                    this.TryApplyBorgWeaponDamage(target, attackEnergy, "support fire");
                }
                else if (target is Ship concreteTarget)
                {
                    concreteTarget.AbsorbHitFrom(friendly, attackEnergy);
                }
                if (target.Destroyed)
                {
                    destroyed.Add(target);
                    hostiles.Remove(target);
                }
            }

            if (destroyed.Any())
            {
                map.RemoveAllDestroyedShips(map, destroyed);
            }
        }

        private void MoveQuadrantFriendlies()
        {
            var concreteMap = this.Map as Map;
            if (concreteMap?.Sectors == null)
            {
                return;
            }

            foreach (var sector in concreteMap.Sectors.Where(s => s?.Coordinates != null))
            {
                var friendlies = sector.Coordinates
                    .Where(c => c?.Object is IShip ship &&
                                ship != concreteMap.Playership &&
                                !ship.Destroyed &&
                                ship.Allegiance == Allegiance.GoodGuy)
                    .Select(c => (IShip)c.Object)
                    .ToList();

                foreach (var friendly in friendlies)
                {
                    this.TryMoveFriendlyShip(concreteMap, friendly);
                }
            }
        }

        private void TryMoveFriendlyShip(Map map, IShip friendly)
        {
            if (Utility.Utility.Random.Next(100) >= QuadrantRules.GetFriendlyMovePercent(map))
            {
                return;
            }

            var currentSector = friendly.GetSector();
            var allowCrossQuadrant = Utility.Utility.Random.Next(100) < QuadrantRules.GetFriendlyCrossQuadrantMovePercent(map);
            var homeQuadrants = map.Sectors
                .Where(s => QuadrantRules.GetFriendlyFactionsForSector(map, s.X, s.Y).Any(f => f == friendly.Faction))
                .Select(s => QuadrantRules.GetQuadrantName(map, s.X, s.Y))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var candidateSectors = map.Sectors.Where(s => s != null && s.Type != SectorType.GalacticBarrier);
            if (!allowCrossQuadrant && homeQuadrants.Any())
            {
                candidateSectors = candidateSectors.Where(s => homeQuadrants.Contains(QuadrantRules.GetQuadrantName(map, s.X, s.Y), StringComparer.OrdinalIgnoreCase));
            }

            var destinationSector = candidateSectors
                .Where(s => Math.Abs(s.X - currentSector.X) <= 1 && Math.Abs(s.Y - currentSector.Y) <= 1 && !(s.X == currentSector.X && s.Y == currentSector.Y))
                .OrderBy(_ => Utility.Utility.Random.Next())
                .FirstOrDefault();
            if (destinationSector == null)
            {
                return;
            }

            var destination = destinationSector.Coordinates.Where(c => c.Item == CoordinateItem.Empty)
                .OrderBy(_ => Utility.Utility.Random.Next())
                .FirstOrDefault();
            if (destination == null)
            {
                return;
            }

            var oldCoordinate = currentSector.Coordinates.GetNoError(new Point(friendly.Coordinate.X, friendly.Coordinate.Y));
            if (oldCoordinate != null && oldCoordinate.Object == friendly)
            {
                oldCoordinate.Item = CoordinateItem.Empty;
                oldCoordinate.Object = null;
            }

            friendly.Point = new Point(destinationSector.X, destinationSector.Y);
            friendly.Coordinate = destination;
            destination.Item = CoordinateItem.FriendlyShip;
            destination.Object = friendly;
        }

        private bool HostileShipsAttack(IMap map, ICollection<IShip> hostilesAttacking, bool returnValue)
        {
            if (hostilesAttacking?.Count > 0)
            {
                foreach (var badGuy in hostilesAttacking)
                {
                    if (badGuy?.Destroyed == true)
                    {
                        continue;
                    }

                    if (badGuy.Faction == FactionName.Borg)
                    {
                        continue;
                    }

                    if (this.HostileShouldRetreat(badGuy))
                    {
                        this.HostileRepairAndRetreat(map, badGuy);
                        continue;
                    }

                    int randomHostileAttacksFactor = Utility.Utility.TestableRandom(this); //this.RandomFactorForTesting == 0 ? Utility.Utility.Random.Next() : this.RandomFactorForTesting;

                    this.HostileAttacks(map, badGuy, randomHostileAttacksFactor);
                }

                this.EnemiesWillNowTaunt();

                returnValue = true;
            }
            return returnValue;
        }

        private bool HostileShouldRetreat(IShip hostile)
        {
            if (hostile?.Subsystems == null || hostile.Faction == FactionName.Borg)
            {
                return false;
            }

            if (hostile is Ship concreteHostile)
            {
                if (concreteHostile.RetreatSuppressedTurns > 0)
                {
                    concreteHostile.RetreatSuppressedTurns--;
                    return false;
                }

                if (concreteHostile.HasRetreatedAndRearmed)
                {
                    return false;
                }

                var maxRetreatAttempts = this.GetIntSettingOrDefault("HostileMaxRetreatAttempts", 2);
                if (concreteHostile.RetreatAttempts >= maxRetreatAttempts)
                {
                    return false;
                }
            }

            var torDown = this.IsSubsystemDamagedForRetreat(hostile, SubsystemType.Torpedoes);
            var phaDown = this.IsSubsystemDamagedForRetreat(hostile, SubsystemType.Phasers);
            var dsrDown = this.IsSubsystemDamagedForRetreat(hostile, SubsystemType.Disruptors);
            return torDown && phaDown && dsrDown;
        }

        private bool IsSubsystemDamaged(IShip ship, SubsystemType subsystemType)
        {
            var subsystem = ship?.Subsystems?.SingleOrDefault(s => s.Type == subsystemType);
            return subsystem == null || subsystem.Damage > 0;
        }

        private bool IsSubsystemDamagedForRetreat(IShip ship, SubsystemType subsystemType)
        {
            var subsystem = ship?.Subsystems?.SingleOrDefault(s => s.Type == subsystemType);
            return subsystem != null && subsystem.Damage > 0;
        }

        private int GetIntSettingOrDefault(string key, int defaultValue)
        {
            try
            {
                return this.Config?.GetSetting<int>(key) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private void HostileRepairAndRetreat(IMap map, IShip hostile)
        {
            var sameSectorAsPlayer = hostile.Point.X == map.Playership.Point.X && hostile.Point.Y == map.Playership.Point.Y;
            if (hostile is Ship hostileShip)
            {
                hostileShip.RetreatTurns++;
            }

            if (hostile is Ship pacingHostile && pacingHostile.RetreatTurns % 2 == 0)
            {
                var repairedSubsystem = this.RepairOneSubsystem(hostile);
                if (sameSectorAsPlayer && !string.IsNullOrWhiteSpace(repairedSubsystem))
                {
                    this.Interact.Line($"{hostile.Name} repaired subsystem: {repairedSubsystem}.");
                }
            }

            if (this.TryWarpRetreatToOutpost(map, hostile, sameSectorAsPlayer))
            {
                return;
            }

            this.TryImpulseRetreat(map, hostile, sameSectorAsPlayer);
        }

        private string RepairOneSubsystem(IShip hostile)
        {
            var priority = new[]
            {
                SubsystemType.Warp,
                SubsystemType.Shields,
                SubsystemType.Torpedoes,
                SubsystemType.Phasers,
                SubsystemType.Disruptors
            };

            foreach (var subsystemType in priority)
            {
                var subsystem = hostile.Subsystems.SingleOrDefault(s => s.Type == subsystemType);
                if (subsystem?.Damage > 0)
                {
                    subsystem.PartialRepair();
                    return subsystemType.Name;
                }
            }

            return string.Empty;
        }

        private bool TryWarpRetreatToOutpost(IMap map, IShip hostile, bool sameSectorAsPlayer)
        {
            if (!(hostile is Ship hostileShip) || hostileShip.RetreatTurns < 2)
            {
                return false;
            }

            var gameMap = map as Map;
            var outposts = gameMap?.GetHostileOutposts();
            if (outposts == null || outposts.Count == 0)
            {
                return false;
            }

            var nearestOutpost = outposts
                .OrderBy(o => Math.Abs(o.SectorDef.X - hostile.Point.X) + Math.Abs(o.SectorDef.Y - hostile.Point.Y))
                .FirstOrDefault();
            if (nearestOutpost == null)
            {
                return false;
            }

            var deltaX = Math.Sign(nearestOutpost.SectorDef.X - hostile.Point.X);
            var deltaY = Math.Sign(nearestOutpost.SectorDef.Y - hostile.Point.Y);

            var nextSectorX = hostile.Point.X + deltaX;
            var nextSectorY = hostile.Point.Y + deltaY;
            if (nextSectorX < DEFAULTS.SECTOR_MIN || nextSectorX >= DEFAULTS.SECTOR_MAX ||
                nextSectorY < DEFAULTS.SECTOR_MIN || nextSectorY >= DEFAULTS.SECTOR_MAX)
            {
                return false;
            }

            var nextSector = map.Sectors[new Point(nextSectorX, nextSectorY)];
            if (nextSector == null || nextSector.Type == SectorType.GalacticBarrier)
            {
                return false;
            }

            var targetCoordinate = nextSector.Coordinates.GetNoError(new Point(hostile.Coordinate.X, hostile.Coordinate.Y));
            if (targetCoordinate == null || targetCoordinate.Item != CoordinateItem.Empty)
            {
                targetCoordinate = nextSector.Coordinates.Where(c => c.Item == CoordinateItem.Empty)
                    .OrderBy(_ => Utility.Utility.Random.Next())
                    .FirstOrDefault();
            }

            if (targetCoordinate == null)
            {
                return false;
            }

            var oldSector = hostile.GetSector();
            var oldCoordinate = oldSector.Coordinates.GetNoError(new Point(hostile.Coordinate.X, hostile.Coordinate.Y));
            if (oldCoordinate != null && oldCoordinate.Object == hostile)
            {
                oldCoordinate.Item = CoordinateItem.Empty;
                oldCoordinate.Object = null;
            }

            hostile.Point = new Point(nextSectorX, nextSectorY);
            hostile.Coordinate = targetCoordinate;
            targetCoordinate.Item = CoordinateItem.HostileShip;
            targetCoordinate.Object = hostile;
            this.AppendGameEventLog($"{hostile.Name} retreated via warp to sector [{nextSectorX},{nextSectorY}] coord [{targetCoordinate.X},{targetCoordinate.Y}]");

            if (sameSectorAsPlayer)
            {
                this.Interact.Line($"{hostile.Name} warps out of sector.");
            }

            var outpostCoordinate = nextSector.Coordinates.FirstOrDefault(c => c.Item == CoordinateItem.HostileOutpost);
            if (outpostCoordinate != null)
            {
                var dx = Math.Abs(outpostCoordinate.X - targetCoordinate.X);
                var dy = Math.Abs(outpostCoordinate.Y - targetCoordinate.Y);
                if (dx <= 1 && dy <= 1)
                {
                    hostile.RepairEverything();
                    if (hostile is Ship concreteHostile)
                    {
                        concreteHostile.Energy = Math.Max(concreteHostile.Energy, concreteHostile.MaxEnergy);
                        concreteHostile.RetreatTurns = 0;
                        concreteHostile.RetreatAttempts++;
                        concreteHostile.HasRetreatedAndRearmed = true;
                    }
                }
            }

            return true;
        }

        private void TryImpulseRetreat(IMap map, IShip hostile, bool sameSectorAsPlayer)
        {
            var sector = hostile.GetSector();
            var player = map.Playership;
            var options = new List<Point>
            {
                new Point(hostile.Coordinate.X - 1, hostile.Coordinate.Y - 1),
                new Point(hostile.Coordinate.X, hostile.Coordinate.Y - 1),
                new Point(hostile.Coordinate.X + 1, hostile.Coordinate.Y - 1),
                new Point(hostile.Coordinate.X - 1, hostile.Coordinate.Y),
                new Point(hostile.Coordinate.X + 1, hostile.Coordinate.Y),
                new Point(hostile.Coordinate.X - 1, hostile.Coordinate.Y + 1),
                new Point(hostile.Coordinate.X, hostile.Coordinate.Y + 1),
                new Point(hostile.Coordinate.X + 1, hostile.Coordinate.Y + 1)
            };

            var candidate = options
                .Where(p => p.X >= DEFAULTS.COORDINATE_MIN && p.X < DEFAULTS.COORDINATE_MAX &&
                            p.Y >= DEFAULTS.COORDINATE_MIN && p.Y < DEFAULTS.COORDINATE_MAX)
                .Select(p => sector.Coordinates.GetNoError(p))
                .Where(c => c != null && c.Item == CoordinateItem.Empty)
                .OrderByDescending(c => Math.Abs(c.X - player.Coordinate.X) + Math.Abs(c.Y - player.Coordinate.Y))
                .FirstOrDefault();

            if (candidate == null)
            {
                return;
            }

            var oldCoordinate = sector.Coordinates.GetNoError(new Point(hostile.Coordinate.X, hostile.Coordinate.Y));
            if (oldCoordinate != null && oldCoordinate.Object == hostile)
            {
                oldCoordinate.Item = CoordinateItem.Empty;
                oldCoordinate.Object = null;
            }

            hostile.Coordinate = candidate;
            candidate.Item = CoordinateItem.HostileShip;
            candidate.Object = hostile;
            this.AppendGameEventLog($"{hostile.Name} retreated via impulse to coord [{candidate.X},{candidate.Y}] in sector [{hostile.Point.X},{hostile.Point.Y}]");

            if (sameSectorAsPlayer)
            {
                this.Interact.Line($"{hostile.Name} uses impulse to retreat.");
            }
        }

        private void HostileStarbasesAttack(IMap map, ISector activeSector)
        {
            if (this.PlayerNowEnemyToFederation)
            {
                if (activeSector.Type != SectorType.Nebulae) //starbases don't belong in Nebulae.  If some dummy put one here intentionally, then it will do no damage.  Why? because if you have no shields, a hostile starbase will disable you with the first shot and kill you with the second. 
                {
                    var starbasesAttacking = activeSector.GetStarbaseCount();

                    for (int i = 0; i < starbasesAttacking; i++)
                    {
                        int hostileStarbaseAttacksRandom = Utility.Utility.TestableRandom(this);  //this.RandomFactorForTesting == 0 ? Utility.Utility.Random.Next() : this.RandomFactorForTesting;

                        //todo: modify starbase to be its own ship object on the map
                        //HACK: this is a little bit of a cheat, saying that the playership is attacking itself, but until the starbase is its own object, this should be fine
                        this.HostileAttacks(map, map.Playership, hostileStarbaseAttacksRandom);

                        int hostileStarbaseAttacksRandom2 = Utility.Utility.TestableRandom(this); //this.RandomFactorForTesting == 0 ? Utility.Utility.Random.Next() : this.RandomFactorForTesting;

                        //cause starbases are bastards like that.  hey.. You started it!
                        this.HostileAttacks(map, map.Playership, hostileStarbaseAttacksRandom2);

                        //todo: when starbases are their own object, they will fire once.. it will just hurt more.
                    }
                }
                else
                {
                    this.Interact.Line("Hostile Starbase fires blindly, unable to get a lock on your position in Nebula.");
                }
            }
        }

        private void HostileAttacks(IMap map, IShip badGuy, int randomFactor)
        {
            if (this.IsSubsystemDamaged(badGuy, SubsystemType.Torpedoes) &&
                this.IsSubsystemDamaged(badGuy, SubsystemType.Phasers) &&
                this.IsSubsystemDamaged(badGuy, SubsystemType.Disruptors))
            {
                return;
            }

            if (Navigation.For(map.Playership).Docked && !this.PlayerNowEnemyToFederation)
            {
                this.AttackDockedPlayership(badGuy, 0);
            }
            else
            {
                this.AttackNonDockedPlayership(map, badGuy, randomFactor);
            }
        }

        private void AttackNonDockedPlayership(IMap map, IShip badGuy, int randomFactor)
        {
            if (this.IsPlayerProtectedByZipBug(map.Playership, out var zipBug))
            {
                this.DepleteShipWeapons(badGuy);
                this.Interact.Line($"{zipBug.Name} distorts incoming fire. {badGuy.Name}'s weapons are depleted.");
                return;
            }

            var playerShipLocation = map.Playership.GetLocation();
            var distance = Utility.Utility.Distance(playerShipLocation.Coordinate.X,
                                                    playerShipLocation.Coordinate.Y,
                                                    badGuy.Coordinate.X,
                                                    badGuy.Coordinate.Y);

            int disruptorShotSeed = this.Config.GetSetting<int>("DisruptorShotSeed");

            //todo: randomFactor is blowing out the top of the int
            int seedEnergyToPowerWeapon = disruptorShotSeed; // * (randomFactor/5);

            var inNebula = badGuy.GetSector().Type == SectorType.Nebulae;

            //Todo: this should be Disruptors.For(this.ShipConnectedTo).Shoot()
            //todo: the -1 should be the ship energy you want to allocate
            var attackingEnergy = (int)Utility.Utility.ShootBeamWeapon(seedEnergyToPowerWeapon, distance, "DisruptorShotDeprecationLevel", "DisruptorEnergyAdjustment", inNebula);

            this.Interact.Line($"{badGuy.Name} fires on you.");
            this.AppendGameEventLog($"{badGuy.Name} attacked playership with beam energy {attackingEnergy} at distance {Math.Round(Convert.ToDouble(distance), 2)}");

            var shieldsValueBeforeHit = Shields.For(map.Playership).Energy;

            map.Playership.AbsorbHitFrom(badGuy, attackingEnergy);

            this.ReportShieldsStatus(map, shieldsValueBeforeHit);
        }

        private void AttackDockedPlayership(IShip attacker, int attackingEnergy)
        {
            string hitMessage = this.Interact.ShipHitMessage(attacker, attackingEnergy);

            this.Interact.Line(hitMessage + " No damage due to starbase shields.");
        }

        #endregion

        #region Taunts

        /// <summary>
        /// All enemies in PlayerShip's Sector shall now commence to unclog their noses in the general direction of the player.
        /// </summary>
        public void EnemiesWillNowTaunt()
        {
            //todo: move this to communications subsystem eventually
            var currentSector = this.Map.Playership.GetSector();
            var HostilesInSector = currentSector.GetHostiles();

            this.LatestTaunts = new List<FactionThreat>();

            IEnumerable<IShip> shipsWithTaunts = from ship in HostilesInSector
                                                    let tauntLikely = Utility.Utility.Random.Next(5) == 1
                                                    where tauntLikely
                                                    select ship;

            string currentThreat = "";
            // ReSharper disable once LoopCanBeConvertedToQuery //Linq should only be for selecting, not executing.
            foreach (var taunt in shipsWithTaunts)
            {
                currentThreat = this.SingleEnemyTaunt(taunt, currentThreat);
            }
        }

        /// <summary>
        /// this is just a bit inefficient, but the way to fix it is to have a refactor.  It works for now
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="currentThreat"></param>
        /// <returns></returns>
        private string SingleEnemyTaunt(IShip ship, string currentThreat)
        {
            var currentFaction = ship.Faction;

            if (currentFaction == null)
            {
                throw new GameException("null faction for taunt");
            }

            string currentShipName = null;

            this.Interact.Line("");

            if (currentFaction == FactionName.Federation)
            {
                //"NCC-500 U.S.S. Saladin  Saladin-class"
                //"NCC-500 U.S.S. FirstName SecondName  Saladin-class"

                currentShipName = ship.Name;
            }
            else if (currentFaction == FactionName.Klingon)
            {
                this.Interact.WithNoEndCR($"Klingon ship at {"[" + ship.Coordinate.X + "," + ship.Coordinate.Y + "]"} sends the following message: ");
            }
            else
            {
                this.Interact.WithNoEndCR($"Hostile at {"[" + ship.Coordinate.X + "," + ship.Coordinate.Y + "]"} sends the following message: ");
                currentShipName = ship.Name;
            }

            FactionThreat randomThreat = this.Config.GetThreats(currentFaction).Shuffle().First();

            currentThreat += string.Format(randomThreat.Threat, currentShipName);

            this.LatestTaunts.Add(randomThreat);

            this.Interact.Line(currentThreat);
            return currentThreat;
        }

        #endregion

        #region Shields

        public bool Auto_Raise_Shields(IMap map, ISector Sector)
        {
            bool shieldsRaised = false;

            if (Sector.Type != SectorType.Nebulae)
            {
                var thisShip = map.Playership;
                var thisShipEnergy = thisShip.Energy;
                var thisShipShields = Shields.For(thisShip);

                if (thisShipShields.Energy == 0) //todo: resource this out
                {
                    if (thisShipEnergy > 500) //todo: resource this out
                    {
                        thisShipShields.Energy = 500; //todo: resource this out
                        thisShip.Energy -= 500;

                        shieldsRaised = true;
                    }
                    else if (thisShipEnergy > 1)
                    {
                        var energyLeft = thisShipEnergy / 2; //todo: resource this out

                        thisShipShields.Energy = Convert.ToInt32(energyLeft);
                        thisShip.Energy = energyLeft;
                        shieldsRaised = true;
                    }
                }
            }

            return shieldsRaised;
        }

        //todo: move this to a report object?
        public void ReportShieldsStatus(IMap map, int shieldsValueBeforeHit)
        {
            var shieldsValueAfterHit = Shields.For(map.Playership).Energy;

            if (shieldsValueAfterHit <= shieldsValueBeforeHit)
            {
                if (shieldsValueAfterHit == 0)
                {
                    this.Interact.SingleLine("** Shields are Down **");
                }
                else
                {
                    this.Interact.SingleLine($"Shields dropped to {Shields.For(map.Playership).Energy}.");
                }
            }
        }

        #endregion

        #region Starbase

        public void DestroyStarbase(IMap map, int newY, int newX, ICoordinate qLocation)
        {
            //todo: technically, the script below should leave the Torpedoes class and move to a script class..
            //todo: raise an event that a script can use.

            //At present, a starbase can be destroyed by a single hit
            bool emergencyMessageSuccess = this.StarbaseEmergencyMessageAttempt();

            this.ExecuteStarbaseDestruction(map, newY, newX, qLocation);

            if (emergencyMessageSuccess)
            {
                this.Interact.Line("Before destruction, the Starbase was able to send an emergency message to Starfleet");
                this.Interact.Line("Federation Ships and starbases will now shoot you on sight!");

                this.PlayerNowEnemyToFederation = true;

                //todo: later, the map will be populated with fed ships at startup.. but this should be applicable in both situations :)
                map.AddHostileFederationShipsToExistingMap();
            }
            else
            {
                this.Interact.Line("Starbase was destroyed before getting out a distress call.");

                if (!this.PlayerNowEnemyToFederation)
                {
                    this.Interact.Line("For now, no one will know of this..");
                }
            }
        }

        private void ExecuteStarbaseDestruction(IMap map, int newY, int newX, ICoordinate qLocation)
        {
            Navigation.For(map.Playership).Docked = false;  //in case you shot it point-blank range..

            map.starbases--;

            qLocation.Object = null;
            qLocation.Item = CoordinateItem.Empty;
            this.AppendGameEventLog($"Starbase destroyed at sector [{newX},{newY}]");

            //yeah. How come a starbase can protect your from baddies but one torpedo hit takes it out?
            this.Interact.Line($"You have destroyed A Federation starbase! (at sector [{newX},{newY}])");

            this.Map.Playership.Scavenge(ScavengeType.Starbase);

            //todo: When the Starbase is a full object, then allow the torpedoes to either lower its shields, or take out subsystems.
            //todo: a concerted effort of 4? torpedoes will destroy an unshielded starbase.
            //todo: however, you'd better hit the comms subsystem to prevent an emergency message, then shoot the log bouy
            //todo: it sends out or other starbases will know of your crime.
        }

        private bool StarbaseEmergencyMessageAttempt()
        {
            return Utility.Utility.Random.Next(2) == 1;
        }

        #endregion

        //public static string GetFederationShipName(IShip ship)
        //{
        //    string currentShipName = ship.Name;

        //    if (currentShipName == "Starbase")
        //    {
        //        return currentShipName;
        //    }

        //    if (currentShipName == "Enterprise")
        //    {
        //        return "Starbase";
        //    }

        //    try
        //    {
        //        int USS = ship.Name.IndexOf("U.S.S. ");
        //        int spaceAfterGivenName = 0;

        //        var nameLength = ship.Name.Length;

        //        for (int i = nameLength; i > 0; i--)
        //        {
        //            var currentChar = ship.Name.Substring(i - 1, 1);
        //            if (currentChar == " ")
        //            {
        //                spaceAfterGivenName = i;
        //                break;
        //            }
        //        }

        //        currentShipName = ship.Name.Substring(USS, spaceAfterGivenName - USS).Trim();
        //        return currentShipName;
        //    }
        //    catch (Exception)
        //    {
        //        //HACK: At present, starbase name won't parse because I have it so that you are shooting yourself.  :D
        //        //todo: make starbase its own object
        //        //if (currentShipName == this.Map.Playership.Name)
        //        //{
        //            ship.Name = "Unknown";
        //        //}
        //        //else
        //        //{
        //        //    //yeah, something else broke.  tell the world.
        //        //    throw;
        //        //}
        //    }

        //    return currentShipName;
        //}

        public static string GetFederationShipRegistration(IShip ship)
        {
            int USS = ship.Name.IndexOf("U.S.S."); //todo: resource this out.

            string currentShipName = ship.Name.Substring(0, USS).Trim();
            return currentShipName;
        }

        //private bool HostileCheck(GameConfig startConfig)
        //{
        //    if (startConfig.CoordinateDefs.GetHostiles().Count() < 1)
        //    {
        //        Output.WriteLine("ERROR: --- No Hostiles have been set up.");

        //        //todo: perhaps we'd have a reason to make a "freeform" option or mode where you could practice shooting things, moving, etc.
        //        //todo: in that case, this function would not be called

        //        this.gameOver = true;
        //        return true;
        //    }
        //    return false;
        //}

        private bool HostileCheck(IMap map)
        {
            if (!map.Sectors.GetHostiles().Any())
            {
                this.Interact.Line("ERROR: --- No Hostiles have been set up.");

                //todo: perhaps we'd have a reason to make a "freeform" option or mode where you could practice shooting things, moving, etc.
                //todo: in that case, this function would not be called

                this.GameOver = true;
                return true;
            }
            return false;
        }

        private void ReportGameStatus()
        {
            if (this.IsSystemsCascadeMode)
            {
                this.ReportSystemsCascadeGameStatus();
                return;
            }

            int starbasesLeft = this.Map.Sectors.GetStarbaseCount();

            if (this.PlayerNowEnemyToFederation)
            {
                this.GameOver = this.Map.timeRemaining < 1 ||
                                starbasesLeft < 1 ||
                                this.Map.Playership.Destroyed ||
                                this.Map.Playership.Energy < 1;
            }
            else
            {
                this.GameOver = !(this.Map.Playership.Energy > 0 &&
                                  !this.Map.Playership.Destroyed &&
                                  (this.Map.Sectors.GetHostileCount() > 0) &&
                                  this.Map.timeRemaining > 0);
            }

            this.Interact.PrintMissionResult(this.Map.Playership, this.PlayerNowEnemyToFederation, starbasesLeft);
        }

        private void ReportSystemsCascadeGameStatus()
        {
            var ship = this.Map?.Playership;
            if (ship == null)
            {
                this.GameOver = true;
                return;
            }

            var destinationReached = this.SystemsCascadeDestinationSector != null &&
                                     ship.Point != null &&
                                     ship.Point.X == this.SystemsCascadeDestinationSector.X &&
                                     ship.Point.Y == this.SystemsCascadeDestinationSector.Y;

            if (destinationReached)
            {
                this.GameOver = true;
                this.Interact.Line("SYSTEMS CASCADE COMPLETE: Destination reached. Command crew survived the cascade.");
                return;
            }

            this.GameOver = ship.Destroyed || ship.Energy <= 0 || this.Map.timeRemaining <= 0;
            if (!this.GameOver)
            {
                return;
            }

            if (ship.Destroyed)
            {
                this.Interact.Line("SYSTEMS CASCADE FAILURE: Ship destroyed.");
            }
            else if (ship.Energy <= 0)
            {
                this.Interact.Line("SYSTEMS CASCADE FAILURE: Reactor energy depleted.");
            }
            else
            {
                this.Interact.Line("SYSTEMS CASCADE FAILURE: Time has run out before reaching destination.");
            }
        }

        public void MoveTimeForward(IMap map, Point lastSector, Point Sector)
        {
            if (lastSector.X != Sector.X || lastSector.Y != Sector.Y)
            {
                map.timeRemaining--;
                map.Stardate++;
            }
        }

        private List<string> DeduplicateRepeatedScanOutput(List<string> output)
        {
            if (output == null || output.Count < 2)
            {
                return output;
            }

            var sectorLineIndexes = output
                .Select((line, index) => new { line, index })
                .Where(x => !string.IsNullOrWhiteSpace(x.line) && x.line.StartsWith("Sector:", StringComparison.Ordinal))
                .Select(x => x.index)
                .ToList();

            if (sectorLineIndexes.Count >= 2)
            {
                for (var i = 0; i < sectorLineIndexes.Count - 1; i++)
                {
                    var first = sectorLineIndexes[i];
                    var second = sectorLineIndexes[i + 1];
                    var firstBlockLength = second - first;
                    var third = i + 2 < sectorLineIndexes.Count ? sectorLineIndexes[i + 2] : output.Count;
                    var secondBlockLength = third - second;

                    if (firstBlockLength <= 0 || secondBlockLength <= 0 || firstBlockLength != secondBlockLength)
                    {
                        continue;
                    }

                    var firstBlock = output.Skip(first).Take(firstBlockLength).ToList();
                    var secondBlock = output.Skip(second).Take(secondBlockLength).ToList();
                    if (firstBlock.SequenceEqual(secondBlock))
                    {
                        var normalizedByBlock = output.Take(second).Concat(output.Skip(third)).ToList();
                        this.Interact.Output.Clear();
                        this.Interact.Output.Write(normalizedByBlock);
                        return normalizedByBlock;
                    }
                }
            }

            if (output.Count % 2 != 0)
            {
                return output;
            }

            var half = output.Count / 2;
            for (var i = 0; i < half; i++)
            {
                if (!string.Equals(output[i], output[i + half], StringComparison.Ordinal))
                {
                    return output;
                }
            }

            var normalized = output.Take(half).ToList();
            this.Interact.Output.Clear();
            this.Interact.Output.Write(normalized);
            return normalized;
        }

        public void Dispose()
        {
            DEFAULTS.COORDINATE_MIN = 0;
            DEFAULTS.COORDINATE_MAX = 0;

            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }
    }
}

