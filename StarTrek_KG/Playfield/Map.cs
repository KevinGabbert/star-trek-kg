using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;

namespace StarTrek_KG.Playfield
{
    public class Map: IMap
    {
        #region Properties

            public IGame Game { get; set; }
            private Sectors _sectors;
            public Sectors Sectors
            {
                get => this._sectors;
                set
                {
                    this._sectors = value;

                    // Compatibility shortcut: existing code still uses Map.Sectors.
                    // Under the galaxy layer, that always points to the current galaxy sectors.
                    if (value != null)
                    {
                        this.Galaxies ??= new Galaxies();
                        if (!this.Galaxies.Any())
                        {
                            this.CurrentGalaxyId = 0;
                            this.Galaxies.Add(new Galaxy(0, "Galaxy 0", value));
                            return;
                        }
                    }

                    var current = this.CurrentGalaxy;
                    if (current != null)
                    {
                        current.Sectors = value;
                    }
                }
            }
            public Galaxies Galaxies { get; private set; }
            public int CurrentGalaxyId { get; private set; }
            public Galaxy CurrentGalaxy => this.Galaxies?.GetById(this.CurrentGalaxyId) ?? this.Galaxies?.FirstOrDefault();
            public const int MaxGalaxyCount = ushort.MaxValue;
            public Ship Playership { get; set; } // todo: v2.0 will have a List<StarShip>().
            public SetupOptions GameConfig { get; set; }
            public IInteraction Write { get; set; }
            public IStarTrekKGSettings Config { get; set; }
            public FactionName DefaultHostile { get; set; }

            public int Stardate { get; set; }
            public int timeRemaining { get; set; }
            public int starbases { get; set; }
            public string Text { get; set; }
            public int HostilesToSetUp { get; set; }

        #endregion

        public Map()
        {
            this.Galaxies = new Galaxies();
            this.CurrentGalaxyId = 0;
        }

        public Map(SetupOptions setupOptions, IInteraction write, IStarTrekKGSettings config, IGame game, FactionName defaultHostile = null) : this()
        {
            this.Game = game;
            this.Config = config;
            this.Write = write;

            this.DefaultHostile = defaultHostile ?? FactionName.Klingon;
            this.Initialize(setupOptions);
        }

        public void Initialize(SetupOptions setupOptions)
        {
            this.GameConfig = setupOptions;

            if (setupOptions != null)
            {
                if (setupOptions.Initialize)
                {
                    if (setupOptions.CoordinateDefs == null)
                    {
                        //todo: then use appConfigSectorDefs  
                        throw new GameConfigException(this.Config.GetSetting<string>("NoCoordinateDefsSetUp"));
                    }

                    this.Initialize(setupOptions.CoordinateDefs, setupOptions.AddNebulae); //Playership is set up here.
                }

                //if (setupOptions.GenerateMap)
                //{
                //    //this.Sectors.PopulateSectors(setupOptions.CoordinateDefs, this);
                //}
            }
        }

        public void Initialize(CoordinateDefs sectorDefs, bool generateWithNebulae)
        {
            this.GetGlobalInfo();

            //This list should match baddie type that is created
            List<string> SectorNames = this.Config.GetStarSystems();

            this.Write.DebugLine("Got Starsystems");

            //TODO: if there are less than 64 Sector names then there will be problems..

            var names = new Stack<string>(SectorNames.Shuffle());

            var baddieShipNames = this.Config.ShipNames(this.DefaultHostile);

            this.Write.DebugLine("Got Baddies");

            //todo: modify this to populate with multiple faction types..
            var baddieNames = new Stack<string>(baddieShipNames.Shuffle());

            //todo: this just set up a "friendly"
            this.InitializeSectorsWithBaddies(names, baddieNames, this.DefaultHostile, sectorDefs, generateWithNebulae);

            this.Write.DebugLine("Intialized Sectors with Baddies");

            if (sectorDefs != null)
            {
                this.SetupPlayerShipInSectors(sectorDefs);
            }

            this.SpawnQuadrantFriendlies();

            //Modify this to output everything
            if (DEFAULTS.DEBUG_MODE)
            {
                //TODO: write a hidden command that displays everything. (for debug purposes)

                this.Write.DisplayPropertiesOf(this.Playership); //This line may go away as it should be rolled out with a new Sector
                this.Write.Line(this.Config.GetSetting<string>("DebugModeEnd"));
                this.Write.Line("");
            }

            this.Playership?.UpdateDivinedSectors();
            this.HostilesToSetUp = this.Sectors.GetHostileCount();
        }

        private void SpawnQuadrantFriendlies()
        {
            if (this.Sectors == null)
            {
                return;
            }

            var countPerFaction = QuadrantRules.GetFriendlyShipsPerFaction(this);
            if (countPerFaction < 1)
            {
                return;
            }

            var quadrantNames = new[]
            {
                QuadrantRules.GetQuadrantName(this, 0, 0),
                QuadrantRules.GetQuadrantName(this, Math.Max(0, DEFAULTS.SECTOR_MAX - 1), 0),
                QuadrantRules.GetQuadrantName(this, 0, Math.Max(0, DEFAULTS.SECTOR_MAX - 1)),
                QuadrantRules.GetQuadrantName(this, Math.Max(0, DEFAULTS.SECTOR_MAX - 1), Math.Max(0, DEFAULTS.SECTOR_MAX - 1))
            }.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var quadrantName in quadrantNames)
            {
                var sectorsInQuadrant = this.Sectors
                    .Where(s => s != null &&
                                s.Type != SectorType.GalacticBarrier &&
                                string.Equals(QuadrantRules.GetQuadrantName(this, s.X, s.Y), quadrantName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (!sectorsInQuadrant.Any())
                {
                    continue;
                }

                var friendlyFactions = QuadrantRules.GetFriendlyFactionsForSector(this, sectorsInQuadrant[0].X, sectorsInQuadrant[0].Y).ToList();
                foreach (var faction in friendlyFactions)
                {
                    for (var i = 0; i < countPerFaction; i++)
                    {
                        var sector = sectorsInQuadrant.OrderBy(_ => Utility.Utility.Random.Next()).FirstOrDefault();
                        if (sector == null)
                        {
                            break;
                        }

                        var empty = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty)
                            .OrderBy(_ => Utility.Utility.Random.Next())
                            .FirstOrDefault();
                        if (empty == null)
                        {
                            break;
                        }

                        var ship = new Ship(faction, QuadrantRules.GetRandomShipNameForFaction(this, faction), empty, this);
                        sector.AddShip(ship, empty);
                    }
                }
            }
        }

        public void SetupPlayerShipInSectors(CoordinateDefs sectorDefs)
        {
            //if we have > 0 friendlies with XYs, then we will place them.
            //if we have at least 1 friendly with no XY, then config will be used to generate that type of ship.

            List<CoordinateDef> playerShips = sectorDefs.PlayerShips().ToList();

            if (playerShips.Any())
            {
                try
                {
                    this.SetUpPlayerShip(playerShips.Single()); //todo: this will eventually change

                    Coordinate sectorToPlaceShip = this.Sectors.GetActive().Coordinates[this.Playership.Coordinate.X, this.Playership.Coordinate.Y];

                    //This places our newly created ship into our newly created List of Sectors.
                    sectorToPlaceShip.Item = CoordinateItem.PlayerShip;
                    this.ApplyInitialZipBugAppearanceBonus();
                }
                catch (InvalidOperationException ex)
                {
                    throw new GameConfigException(this.Config.GetSetting<string>("InvalidPlayershipSetup") + ex.Message);
                }
                catch (Exception ex){ throw new GameConfigException(this.Config.GetSetting<string>("ErrorPlayershipSetup") + ex.Message); }
            }
            else
            {
                //this.Sectors[0].Active = true;
                this.Sectors[0].SetActive();
            }
        }

        private void ApplyInitialZipBugAppearanceBonus()
        {
            if (this.Playership is not Ship playership || this.Sectors == null)
            {
                return;
            }

            var zipBugCount = this.Sectors.SelectMany(s => s.Coordinates).Count(c => c.Item == CoordinateItem.ZipBug);
            if (zipBugCount < 1)
            {
                return;
            }

            var bonusPerAppearance = this.GetSettingOrDefault("ZipBugMaxEnergyAppearanceBonus", 100);
            if (bonusPerAppearance <= 0)
            {
                return;
            }

            playership.MaxEnergy += zipBugCount * bonusPerAppearance;
        }

        //Creates a 2D array of Sectors.  This is how all of our game pieces will be moving around.
        public void InitializeSectorsWithBaddies(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, CoordinateDefs sectorDefs, bool generateWithNebulae)
        {
            var sectors = new Sectors(this, this.Write);
            this.InitializePrimaryGalaxy(sectors);

            //Friendlies are added separately
            List<Coordinate> itemsToPopulateThatAreNotPlayerShip = sectorDefs.ToCoordinates(this.Sectors).Where(q => q.Item != CoordinateItem.PlayerShip).ToList();

            this.Write.DebugLine("ItemsToPopulate: " + itemsToPopulateThatAreNotPlayerShip.Count + " Sectors: " + this.Sectors.Count);
            
            //todo: this can be done with a single loop populating a list of XYs
            this.GenerateSquareGalaxy(names, baddieNames, stockBaddieFaction, itemsToPopulateThatAreNotPlayerShip, generateWithNebulae);
        }

        private void InitializePrimaryGalaxy(Sectors sectors)
        {
            if (sectors == null)
            {
                throw new ArgumentNullException(nameof(sectors));
            }

            this.Galaxies = new Galaxies();
            this.CurrentGalaxyId = 0;

            this.Galaxies.Add(new Galaxy(0, this.GetDefaultGalaxyName(0), sectors));
            this.Sectors = sectors;
        }

        public Galaxy AddGalaxy(string name, Sectors sectors)
        {
            if (this.Galaxies == null)
            {
                this.Galaxies = new Galaxies();
            }

            if (this.Galaxies.Count >= MaxGalaxyCount)
            {
                throw new GameException($"Maximum galaxy count reached: {MaxGalaxyCount}.");
            }

            var nextId = this.Galaxies.Any() ? this.Galaxies.Max(g => g.Id) + 1 : 0;
            var galaxy = new Galaxy(nextId, string.IsNullOrWhiteSpace(name) ? this.GetDefaultGalaxyName(nextId) : name, sectors ?? new Sectors(this, this.Write));
            this.Galaxies.Add(galaxy);

            if (this.Galaxies.Count == 1)
            {
                this.CurrentGalaxyId = galaxy.Id;
                this.Sectors = galaxy.Sectors;
            }

            return galaxy;
        }

        public string GetCurrentGalaxyName()
        {
            return this.CurrentGalaxy?.Name ?? this.GetDefaultGalaxyName(this.CurrentGalaxyId);
        }

        public void RenameCurrentGalaxy(string name)
        {
            var current = this.CurrentGalaxy;
            if (current == null)
            {
                return;
            }

            current.Name = string.IsNullOrWhiteSpace(name)
                ? this.GetDefaultGalaxyName(current.Id)
                : name.Trim();
        }

        private string GetDefaultGalaxyName(int id)
        {
            return $"NGC-{(100 + Math.Max(0, id)):000}";
        }

        public void GenerateSquareGalaxy(Stack<string> names, Stack<string> baddieNames, FactionName stockBaddieFaction, List<Coordinate> itemsToPopulate, bool generateWithNebula)
        {
            if (DEFAULTS.SECTOR_MAX == 0)
            {
                throw new GameException("No Sectors to set up.  Sector_MAX set to Zero");
            }

            for (var SectorX = 0; SectorX < DEFAULTS.SECTOR_MAX; SectorX++) //todo: app.config
            {
                for (var SectorY = 0; SectorY < DEFAULTS.SECTOR_MAX; SectorY++)
                {
                    int index;
                    var newSector = new Sector(this);
                    var SectorXY = new Point(SectorX, SectorY);

                    bool isNebulae = false;
                    if (generateWithNebula)
                    {
                        isNebulae = Utility.Utility.Random.Next(11) == 10; //todo pull this setting from config
                    }

                    newSector.Create(names, baddieNames, stockBaddieFaction, SectorXY, out index, itemsToPopulate,
                                       this.GameConfig.AddStars, isNebulae);

                    this.Sectors.Add(newSector);

                    if (DEFAULTS.DEBUG_MODE)
                    {
                        this.Write.SingleLine(this.Config.GetSetting<string>("DebugAddingNewSector"));

                        this.Write.DisplayPropertiesOf(newSector);

                        //TODO: each object within Sector needs a .ToString()

                        this.Write.Line("");
                    }
                }
            }

            if (this.GameConfig?.AddDeuterium ?? true)
            {
                this.PopulateDeuteriumPockets();
                this.PopulateDeuteriumClouds();
            }

            this.PopulateGaseousAnomalyFields();
            this.PopulateTemporalRifts();
            this.PopulateSporeFields();
            this.PopulateBlackHoles();
            this.PopulateBorgCubes();
            this.PopulateZipBugs();
            this.PopulateHostileOutposts();
            this.PopulateTechnologyCaches();
            this.PopulateWormholes();

            if (this.GameConfig?.AddGraviticMines ?? true)
            {
                this.PopulateGraviticMines();
            }

            if (this.GameConfig?.AddEnergyAnomalies ?? false)
            {
                this.PopulateEnergyAnomalies();
            }

            if (this.GameConfig?.IsSystemsCascadeMode ?? false)
            {
                this.PopulateNebulaDeuteriumBonus();
            }

            if (this.GameConfig?.AddDeuterium ?? true)
            {
                this.NormalizeDeuteriumTotalsPerSector();
            }
        }

        private void PopulateDeuteriumPockets()
        {
            var percent = this.GetSettingOrDefault(
                "DeuteriumSectorSpawnPercent",
                this.GetSettingOrDefault("DeuteriumSectorPercent", 35));
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var candidates = this.Sectors?
                .Where(sector => sector?.Coordinates != null && sector.Coordinates.Any(c => c.Item == CoordinateItem.Empty))
                .ToList();

            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            var totalSectors = candidates.Count;
            var targetCount = (int)Math.Round(totalSectors * (percent / 100.0));
            if (targetCount <= 0)
            {
                return;
            }

            if (targetCount > totalSectors)
            {
                targetCount = totalSectors;
            }

            var selected = new List<Sector>(targetCount);
            const int hostileWeight = 3;

            while (selected.Count < targetCount && candidates.Count > 0)
            {
                var index = this.PickWeightedSectorIndex(candidates, hostileWeight);
                var chosen = candidates[index];
                candidates.RemoveAt(index);
                selected.Add(chosen);
            }

            foreach (var sector in selected)
            {
                var emptyCoordinates = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
                if (emptyCoordinates.Count == 0)
                {
                    continue;
                }

                var pickedCoordinate = emptyCoordinates[Utility.Utility.Random.Next(emptyCoordinates.Count)];
                this.PlaceDeuteriumAt(pickedCoordinate);
            }
        }

        private void PopulateHostileOutposts()
        {
            var percent = this.GetSettingOrDefault("HostileOutpostSectorPercent", 8);
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var candidates = this.Sectors?
                .Where(sector => sector?.Coordinates != null)
                .Where(sector => sector.Coordinates.Any(c => c.Item == CoordinateItem.Empty))
                .ToList();

            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            var targetCount = (int)Math.Round(candidates.Count * (percent / 100.0));
            targetCount = Math.Max(1, Math.Min(targetCount, candidates.Count));

            foreach (var sector in candidates.Shuffle().Take(targetCount))
            {
                var coordinate = sector.Coordinates
                    .Where(c => c.Item == CoordinateItem.Empty)
                    .OrderBy(_ => Utility.Utility.Random.Next())
                    .FirstOrDefault();

                if (coordinate == null)
                {
                    continue;
                }

                coordinate.Item = CoordinateItem.HostileOutpost;
                coordinate.Object = new HostileOutpost
                {
                    Coordinate = coordinate
                };
            }
        }

        private void PopulateNebulaDeuteriumBonus()
        {
            var multiplier = this.GameConfig?.SystemsCascadeNebulaDeuteriumMultiplier ?? 1;
            if (multiplier <= 1)
            {
                return;
            }

            foreach (var sector in this.Sectors.Where(s => s.Type == SectorType.Nebulae))
            {
                var emptyCoordinates = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
                if (!emptyCoordinates.Any())
                {
                    continue;
                }

                var additions = Math.Min(multiplier, emptyCoordinates.Count);
                for (var i = 0; i < additions; i++)
                {
                    if (!emptyCoordinates.Any())
                    {
                        break;
                    }

                    var pickedIndex = Utility.Utility.Random.Next(emptyCoordinates.Count);
                    var pickedCoordinate = emptyCoordinates[pickedIndex];
                    emptyCoordinates.RemoveAt(pickedIndex);
                    this.PlaceDeuteriumAt(pickedCoordinate);
                }
            }
        }

        private void PopulateDeuteriumClouds()
        {
            var percent = this.Config?.GetSetting<int>("DeuteriumCloudSectorPercent") ?? 0;
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var minSize = this.Config?.GetSetting<int>("DeuteriumCloudMinSize") ?? 1;
            var maxSize = this.Config?.GetSetting<int>("DeuteriumCloudMaxSize") ?? 4;
            if (minSize < 1) minSize = 1;
            if (maxSize < minSize) maxSize = minSize;
            if (maxSize > 4) maxSize = 4;

            var candidates = this.Sectors?
                .Where(sector => sector?.Coordinates != null && sector.Type != SectorType.GalacticBarrier)
                .ToList();

            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            var targetCount = (int)Math.Round(candidates.Count * (percent / 100.0));
            if (targetCount <= 0)
            {
                targetCount = 1;
            }

            targetCount = Math.Min(targetCount, candidates.Count);

            while (targetCount > 0 && candidates.Count > 0)
            {
                var pickIndex = Utility.Utility.Random.Next(candidates.Count);
                var sector = candidates[pickIndex];
                candidates.RemoveAt(pickIndex);

                if (this.TryPlaceDeuteriumCloudInSector(sector, minSize, maxSize))
                {
                    targetCount--;
                }
            }
        }

        private bool TryPlaceDeuteriumCloudInSector(Sector sector, int minSize, int maxSize)
        {
            var size = Utility.Utility.Random.Next(minSize, maxSize + 1);
            var maxStartX = DEFAULTS.COORDINATE_MAX - size;
            var maxStartY = DEFAULTS.COORDINATE_MAX - size;
            if (maxStartX < 0 || maxStartY < 0)
            {
                return false;
            }

            const int maxPlacementAttempts = 40;
            for (var attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                var startX = Utility.Utility.Random.Next(0, maxStartX + 1);
                var startY = Utility.Utility.Random.Next(0, maxStartY + 1);

                var cloudCells = new List<Coordinate>();
                var fits = true;

                for (var y = startY; y < startY + size && fits; y++)
                {
                    for (var x = startX; x < startX + size; x++)
                    {
                        var coordinate = sector.Coordinates.GetNoError(new Point(x, y));
                        if (coordinate == null || coordinate.Item != CoordinateItem.Empty)
                        {
                            fits = false;
                            break;
                        }

                        cloudCells.Add(coordinate);
                    }
                }

                if (!fits || cloudCells.Count == 0)
                {
                    continue;
                }

                foreach (var cell in cloudCells)
                {
                    this.PlaceDeuteriumCloudAt(cell);
                }

                this.PlaceCloudGuardsAndMines(sector, startX, startY, size);
                return true;
            }

            return false;
        }

        private void PlaceCloudGuardsAndMines(Sector sector, int startX, int startY, int size)
        {
            var perimeter = this.GetCloudPerimeterEmptyCoordinates(sector, startX, startY, size);
            if (perimeter.Count == 0)
            {
                return;
            }

            var guardCount = this.Config?.GetSetting<int>("DeuteriumCloudGuardCount") ?? 2;
            if (guardCount < 0) guardCount = 0;
            if (guardCount > 2) guardCount = 2; // Guards naturally pair up.

            for (var i = 0; i < guardCount && perimeter.Count > 0; i++)
            {
                var idx = Utility.Utility.Random.Next(perimeter.Count);
                var guardCoordinate = perimeter[idx];
                perimeter.RemoveAt(idx);
                this.PlaceCloudGuardAt(sector, guardCoordinate);
            }

            var mineChance = this.Config?.GetSetting<int>("DeuteriumCloudMineChancePercent") ?? 50;
            if (mineChance < 0) mineChance = 0;
            if (mineChance > 100) mineChance = 100;
            var allowMinesInStarbaseSectors = this.GetBoolSettingOrDefault("AllowMinesInStarbaseSectors", false);
            var sectorHasStarbase = this.SectorHasStarbase(sector);
            if (sectorHasStarbase && !allowMinesInStarbaseSectors)
            {
                return;
            }

            foreach (var coordinate in perimeter.Where(c => c.Item == CoordinateItem.Empty))
            {
                if (Utility.Utility.Random.Next(100) < mineChance)
                {
                    coordinate.Item = CoordinateItem.GraviticMine;
                    coordinate.Object = new GraviticMine();
                }
            }
        }

        private List<Coordinate> GetCloudPerimeterEmptyCoordinates(Sector sector, int startX, int startY, int size)
        {
            var candidates = new List<Coordinate>();
            var endX = startX + size - 1;
            var endY = startY + size - 1;

            for (var y = startY - 1; y <= endY + 1; y++)
            {
                for (var x = startX - 1; x <= endX + 1; x++)
                {
                    if (x < 0 || y < 0 || x >= DEFAULTS.COORDINATE_MAX || y >= DEFAULTS.COORDINATE_MAX)
                    {
                        continue;
                    }

                    var insideCloud = x >= startX && x <= endX && y >= startY && y <= endY;
                    if (insideCloud)
                    {
                        continue;
                    }

                    var coordinate = sector.Coordinates.GetNoError(new Point(x, y));
                    if (coordinate != null && coordinate.Item == CoordinateItem.Empty)
                    {
                        candidates.Add(coordinate);
                    }
                }
            }

            return candidates;
        }

        private void PlaceCloudGuardAt(Sector sector, Coordinate coordinate)
        {
            if (sector == null || coordinate == null || coordinate.Item != CoordinateItem.Empty)
            {
                return;
            }

            var names = this.Config.ShipNames(FactionName.Klingon);
            if (names == null || names.Count == 0)
            {
                return;
            }

            var randomName = names[Utility.Utility.Random.Next(names.Count)];
            var hostile = new Ship(FactionName.Klingon, randomName, coordinate, this);
            var hostileShields = Shields.For(hostile);
            hostileShields.Energy = 300 + Utility.Utility.TestableRandom(this.Game, 200, 200);
            hostile.Energy = 300;

            sector.AddShip(hostile, coordinate);
        }

        private void PopulateEnergyAnomalies()
        {
            var densityPercent = this.GameConfig?.SystemsCascadeAnomalyDensityPercent ?? 10;
            if (densityPercent <= 0)
            {
                return;
            }

            if (densityPercent > 100)
            {
                densityPercent = 100;
            }

            var anomalyGlyphs = new[] { "&", "%", "$", "@", "~", "-" };

            foreach (var sector in this.Sectors)
            {
                if (sector.Type == SectorType.Nebulae)
                {
                    continue;
                }

                var empties = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
                if (!empties.Any())
                {
                    continue;
                }

                var targetCount = (int)Math.Round(empties.Count * (densityPercent / 100.0));
                if (targetCount <= 0)
                {
                    continue;
                }

                targetCount = Math.Min(targetCount, empties.Count);

                for (var i = 0; i < targetCount; i++)
                {
                    if (!empties.Any())
                    {
                        break;
                    }

                    var emptyIndex = Utility.Utility.Random.Next(empties.Count);
                    var coordinate = empties[emptyIndex];
                    empties.RemoveAt(emptyIndex);

                    var glyph = anomalyGlyphs[Utility.Utility.Random.Next(anomalyGlyphs.Length)];
                    coordinate.Item = CoordinateItem.EnergyAnomaly;
                    coordinate.Object = new EnergyAnomaly
                    {
                        Coordinate = coordinate,
                        Glyph = glyph,
                        EffectKey = glyph,
                        Name = $"Energy Anomaly {glyph}"
                    };
                }
            }
        }

        private void PopulateGraviticMines()
        {
            var allowMinesInStarbaseSectors = this.GetBoolSettingOrDefault("AllowMinesInStarbaseSectors", false);
            var hostileSectors = this.Sectors?
                .Where(sector => sector?.Coordinates != null && sector.Coordinates.Any(c => c.Item == CoordinateItem.HostileShip))
                .Where(sector => allowMinesInStarbaseSectors || !this.SectorHasStarbase(sector))
                .ToList();

            if (hostileSectors == null || hostileSectors.Count == 0)
            {
                return;
            }

            foreach (var sector in hostileSectors)
            {
                var emptyCoordinates = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
                if (emptyCoordinates.Count == 0)
                {
                    continue;
                }

                var pickedCoordinate = emptyCoordinates[Utility.Utility.Random.Next(emptyCoordinates.Count)];
                pickedCoordinate.Item = CoordinateItem.GraviticMine;
                pickedCoordinate.Object = new GraviticMine();
            }
        }

        private void PopulateGaseousAnomalyFields()
        {
            var percent = this.GetSettingOrDefault("GaseousAnomalySectorPercent", 15);
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var minGroupSize = this.GetSettingOrDefault("GaseousAnomalyGroupMin", 6);
            var maxGroupSize = this.GetSettingOrDefault("GaseousAnomalyGroupMax", 16);
            if (minGroupSize < 1) minGroupSize = 1;
            if (maxGroupSize < minGroupSize) maxGroupSize = minGroupSize;
            if (maxGroupSize > (DEFAULTS.COORDINATE_MAX * DEFAULTS.COORDINATE_MAX))
            {
                maxGroupSize = DEFAULTS.COORDINATE_MAX * DEFAULTS.COORDINATE_MAX;
            }

            var candidates = this.Sectors
                .Where(s => s != null &&
                            s.Type != SectorType.GalacticBarrier &&
                            s.Type != SectorType.Nebulae &&
                            s.Coordinates != null &&
                            s.Coordinates.Any(c => c.Item == CoordinateItem.Empty))
                .ToList();

            if (!candidates.Any())
            {
                return;
            }

            foreach (var sector in candidates)
            {
                if (Utility.Utility.Random.Next(100) >= percent)
                {
                    continue;
                }

                this.TryPlaceGaseousAnomalyGroup(sector, minGroupSize, maxGroupSize);
            }
        }

        private void TryPlaceGaseousAnomalyGroup(Sector sector, int minGroupSize, int maxGroupSize)
        {
            var empty = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
            if (!empty.Any())
            {
                return;
            }

            var targetSize = Utility.Utility.Random.Next(minGroupSize, maxGroupSize + 1);
            targetSize = Math.Min(targetSize, empty.Count);

            var start = empty[Utility.Utility.Random.Next(empty.Count)];
            var group = new List<Coordinate> { start };
            var visited = new HashSet<string> { $"{start.X},{start.Y}" };

            while (group.Count < targetSize)
            {
                var anchor = group[Utility.Utility.Random.Next(group.Count)];
                var neighbors = this.GetOrthogonalEmptyNeighbors(sector, anchor)
                    .Where(n => !visited.Contains($"{n.X},{n.Y}"))
                    .ToList();

                if (!neighbors.Any())
                {
                    var fallback = empty.Where(c => !visited.Contains($"{c.X},{c.Y}")).ToList();
                    if (!fallback.Any())
                    {
                        break;
                    }

                    var picked = fallback[Utility.Utility.Random.Next(fallback.Count)];
                    group.Add(picked);
                    visited.Add($"{picked.X},{picked.Y}");
                    continue;
                }

                var next = neighbors[Utility.Utility.Random.Next(neighbors.Count)];
                group.Add(next);
                visited.Add($"{next.X},{next.Y}");
            }

            foreach (var coordinate in group)
            {
                coordinate.Item = CoordinateItem.GaseousAnomaly;
                coordinate.Object = new GaseousAnomaly
                {
                    Coordinate = coordinate
                };
            }
        }

        private void PopulateTemporalRifts()
        {
            var percent = this.GetSettingOrDefault("TemporalRiftSectorPercent", 5);
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var candidates = this.GetFeatureCandidateSectors();
            foreach (var sector in candidates)
            {
                if (Utility.Utility.Random.Next(100) >= percent)
                {
                    continue;
                }

                this.TryPlaceTemporalRift(sector);
            }
        }

        private void TryPlaceTemporalRift(Sector sector)
        {
            // "/" diagonal has x + y = constant.
            var sum = Utility.Utility.Random.Next(2, (DEFAULTS.COORDINATE_MAX - 1) * 2 - 1);
            var points = new List<Coordinate>();

            for (var x = 0; x < DEFAULTS.COORDINATE_MAX; x++)
            {
                var y = sum - x;
                if (y < 0 || y >= DEFAULTS.COORDINATE_MAX)
                {
                    continue;
                }

                var coordinate = sector.Coordinates.GetNoError(new Point(x, y));
                if (coordinate != null && coordinate.Item == CoordinateItem.Empty)
                {
                    points.Add(coordinate);
                }
            }

            if (points.Count < 3)
            {
                return;
            }

            foreach (var coordinate in points)
            {
                coordinate.Item = CoordinateItem.TemporalRift;
                coordinate.Object = new TemporalRift
                {
                    Coordinate = coordinate
                };
            }
        }

        private void PopulateSporeFields()
        {
            var percent = this.GetSettingOrDefault("SporeSectorPercent", 5);
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var candidates = this.GetFeatureCandidateSectors();
            foreach (var sector in candidates)
            {
                if (Utility.Utility.Random.Next(100) >= percent)
                {
                    continue;
                }

                var empty = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
                if (!empty.Any())
                {
                    continue;
                }

                var coordinate = empty[Utility.Utility.Random.Next(empty.Count)];
                coordinate.Item = CoordinateItem.SporeField;
                coordinate.Object = new SporeField
                {
                    Coordinate = coordinate
                };
            }
        }

        private void PopulateBlackHoles()
        {
            var percent = this.GetSettingOrDefault("BlackHoleSectorPercent", 5);
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var candidates = this.GetFeatureCandidateSectors();
            foreach (var sector in candidates)
            {
                if (Utility.Utility.Random.Next(100) >= percent)
                {
                    continue;
                }

                this.TryPlaceSingleBlackHole(sector);
            }
        }

        private void TryPlaceSingleBlackHole(Sector sector)
        {
            if (sector == null || sector.Coordinates == null)
            {
                return;
            }

            if (sector.Coordinates.Any(c => c.Item == CoordinateItem.BlackHole))
            {
                return;
            }

            var empty = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
            if (!empty.Any())
            {
                return;
            }

            var target = empty[Utility.Utility.Random.Next(empty.Count)];
            target.Item = CoordinateItem.BlackHole;
            target.Object = new BlackHole
            {
                Coordinate = target
            };
        }

        private void PopulateTechnologyCaches()
        {
            var percent = this.GetSettingOrDefault("TechnologyCacheSectorPercent", 3);
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var candidates = this.GetFeatureCandidateSectors();
            foreach (var sector in candidates)
            {
                if (Utility.Utility.Random.Next(100) >= percent)
                {
                    continue;
                }

                var empty = sector.Coordinates.Where(c => c.Item == CoordinateItem.Empty).ToList();
                if (!empty.Any())
                {
                    continue;
                }

                var coordinate = empty[Utility.Utility.Random.Next(empty.Count)];
                this.PlaceTechnologyCacheAt(coordinate);
            }
        }

        private void PopulateWormholes()
        {
            var percent = this.GetSettingOrDefault("WormholeSectorPercent", 3);
            if (percent <= 0)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }

            var candidates = this.GetFeatureCandidateSectors();
            if (candidates.Count < 2)
            {
                return;
            }

            var targetCount = (int)Math.Round(candidates.Count * (percent / 100.0));
            if (targetCount < 2)
            {
                targetCount = 2;
            }

            if (targetCount > candidates.Count)
            {
                targetCount = candidates.Count;
            }

            if (targetCount % 2 != 0)
            {
                targetCount--;
            }

            if (targetCount < 2)
            {
                return;
            }

            var selected = candidates.Shuffle().Take(targetCount).ToList();
            var pairId = 1;
            for (var i = 0; i < selected.Count; i += 2)
            {
                var firstSector = selected[i];
                var secondSector = selected[i + 1];
                var firstCoordinate = firstSector.Coordinates
                    .Where(c => c.Item == CoordinateItem.Empty && !this.IsReservedPlayerStartCoordinate(c))
                    .OrderBy(_ => Utility.Utility.Random.Next())
                    .FirstOrDefault();
                var secondCoordinate = secondSector.Coordinates
                    .Where(c => c.Item == CoordinateItem.Empty && !this.IsReservedPlayerStartCoordinate(c))
                    .OrderBy(_ => Utility.Utility.Random.Next())
                    .FirstOrDefault();
                if (firstCoordinate == null || secondCoordinate == null)
                {
                    continue;
                }

                this.PlaceWormholeAt(firstCoordinate, secondSector.GetPoint(), pairId);
                this.PlaceWormholeAt(secondCoordinate, firstSector.GetPoint(), pairId);
                pairId++;
            }
        }

        private bool IsReservedPlayerStartCoordinate(Coordinate coordinate)
        {
            return coordinate?.SectorDef != null &&
                   this.GameConfig?.CoordinateDefs?.PlayerShips().Any(def =>
                       def.SectorDef != null &&
                       def.Coordinate != null &&
                       def.SectorDef.X == coordinate.SectorDef.X &&
                       def.SectorDef.Y == coordinate.SectorDef.Y &&
                       def.Coordinate.X == coordinate.X &&
                       def.Coordinate.Y == coordinate.Y) == true;
        }

        private List<Sector> GetFeatureCandidateSectors()
        {
            return this.Sectors
                .Where(s => s != null &&
                            s.Type != SectorType.GalacticBarrier &&
                            s.Coordinates != null &&
                            s.Coordinates.Any(c => c.Item == CoordinateItem.Empty))
                .ToList();
        }

        private void PopulateBorgCubes()
        {
            var requestedCount = this.GetSettingOrDefault("BorgCubeCount", 2);
            if (requestedCount <= 0 || this.Sectors == null)
            {
                return;
            }

            var existingBorg = this.Sectors
                .SelectMany(s => s?.Coordinates ?? Enumerable.Empty<Coordinate>())
                .Count(c => c?.Object is Ship ship && ship.Faction == FactionName.Borg);
            var remaining = requestedCount - existingBorg;
            if (remaining <= 0)
            {
                return;
            }

            var deltaSectors = this.Sectors
                .Where(s => s != null &&
                            s.Type != SectorType.GalacticBarrier &&
                            string.Equals(QuadrantRules.GetQuadrantName(this, s.X, s.Y), "Delta", StringComparison.OrdinalIgnoreCase))
                .OrderBy(_ => Utility.Utility.Random.Next())
                .ToList();

            foreach (var sector in deltaSectors)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var coordinate = sector.Coordinates
                    .Where(c => c.Item == CoordinateItem.Empty)
                    .OrderBy(_ => Utility.Utility.Random.Next())
                    .FirstOrDefault();
                if (coordinate == null)
                {
                    continue;
                }

                var borg = new Ship(FactionName.Borg, "Borg Cube", coordinate, this);
                this.ConfigureBorgShip(borg);
                sector.AddShip(borg, coordinate);
                remaining--;
            }
        }

        private void PopulateZipBugs()
        {
            var requestedCount = this.GetSettingOrDefault("ZipBugCount", 2);
            if (requestedCount <= 0 || this.Sectors == null)
            {
                return;
            }

            var candidates = this.GetFeatureCandidateSectors()
                .OrderBy(_ => Utility.Utility.Random.Next())
                .ToList();

            foreach (var sector in candidates)
            {
                if (requestedCount <= 0)
                {
                    break;
                }

                var coordinate = sector.Coordinates
                    .Where(c => c.Item == CoordinateItem.Empty)
                    .OrderBy(_ => Utility.Utility.Random.Next())
                    .FirstOrDefault();
                if (coordinate == null)
                {
                    continue;
                }

                coordinate.Item = CoordinateItem.ZipBug;
                coordinate.Object = new ZipBug
                {
                    Coordinate = coordinate
                };
                requestedCount--;
            }
        }

        private void ConfigureBorgShip(Ship borg)
        {
            if (borg == null)
            {
                return;
            }

            var borgEnergy = this.GetSettingOrDefault("BorgEnergy", 10000);
            var borgShields = this.GetSettingOrDefault("BorgShieldEnergy", 10000);
            borg.Energy = borgEnergy;
            borg.MaxEnergy = borgEnergy;
            Shields.For(borg).Energy = borgShields;

            var torpedoes = Torpedoes.For(borg);
            torpedoes.Count = 0;
            torpedoes.Damage = Math.Max(1, torpedoes.Damage);

            Phasers.For(borg).Damage = Math.Max(1, Phasers.For(borg).Damage);

            var disruptors = borg.Subsystems.SingleOrDefault(s => s.Type == SubsystemType.Disruptors);
            if (disruptors != null)
            {
                disruptors.Damage = Math.Max(1, disruptors.Damage);
            }

            borg.BorgDamageableTurnsRemaining = this.GetSettingOrDefault("BorgInitialDamageableTurns", 2);
        }

        public bool IsPlayershipNearBlackHole(IShip ship = null, int radius = 2)
        {
            var subject = ship ?? this.Playership;
            if (subject?.Coordinate == null)
            {
                return false;
            }

            var sector = subject.GetSector();
            var blackHoles = sector?.Coordinates?.Where(c => c.Item == CoordinateItem.BlackHole).ToList();
            if (blackHoles == null || blackHoles.Count == 0)
            {
                return false;
            }

            return blackHoles.Any(h =>
                Math.Max(Math.Abs(h.X - subject.Coordinate.X), Math.Abs(h.Y - subject.Coordinate.Y)) <= radius);
        }

        public bool TryApplyBlackHolePull(IShip ship)
        {
            if (ship?.Coordinate == null)
            {
                return false;
            }

            var pullRadius = this.GetSettingOrDefault("BlackHolePullRadius", 2);
            if (!this.IsPlayershipNearBlackHole(ship, pullRadius))
            {
                return false;
            }

            var currentSector = ship.GetSector();
            var nearest = currentSector.Coordinates
                .Where(c => c.Item == CoordinateItem.BlackHole)
                .OrderBy(c => Math.Abs(c.X - ship.Coordinate.X) + Math.Abs(c.Y - ship.Coordinate.Y))
                .FirstOrDefault();

            if (nearest == null)
            {
                return false;
            }

            var dx = Math.Sign(nearest.X - ship.Coordinate.X);
            var dy = Math.Sign(nearest.Y - ship.Coordinate.Y);
            var direction = this.DirectionFromDelta(dx, dy);

            var nextSector = Sectors.GetNext(this, currentSector, direction);
            if (this.Sectors.IsGalacticBarrier(nextSector))
            {
                this.Write?.Line("Black hole gravity well strains the ship against the galactic barrier.");
            }
            else
            {
                ship.Point = new Point(nextSector.X, nextSector.Y);
                nextSector.SetActive();
                this.SetPlayershipInActiveSector(this);
                this.Write?.Line("Black hole gravity pulls the ship one sector.");
            }

            var drain = this.GetSettingOrDefault("BlackHolePullEnergyDrain", 75);
            ship.Energy -= drain;
            if (ship.Energy <= 0)
            {
                ship.Energy = 0;
                ship.Destroyed = true;
            }

            this.timeRemaining--;
            this.Stardate++;
            this.Write?.Line($"Black hole shear drains {drain} energy.");
            return true;
        }

        public bool TrySendShipBackInTime(IShip ship, int turnsBack)
        {
            var concrete = ship as Ship;
            if (concrete?.TurnHistory == null || concrete.TurnHistory.Count == 0)
            {
                return false;
            }

            if (turnsBack < 1)
            {
                turnsBack = 1;
            }

            var index = concrete.TurnHistory.Count - 1 - turnsBack;
            if (index < 0 || index >= concrete.TurnHistory.Count)
            {
                return false;
            }

            var snapshot = concrete.TurnHistory[index];
            var sector = this.Sectors[new Point(snapshot.Sector.X, snapshot.Sector.Y)];
            var coordinate = sector?.Coordinates?.GetNoError(new Point(snapshot.Coordinate.X, snapshot.Coordinate.Y));
            if (sector == null || coordinate == null)
            {
                return false;
            }

            this.SetPlayershipInLocation(ship, this, new Location(sector, coordinate));
            return true;
        }

        private NavDirection DirectionFromDelta(int dx, int dy)
        {
            if (dx == 0 && dy > 0) return NavDirection.Down;
            if (dx == 0 && dy < 0) return NavDirection.Up;
            if (dx > 0 && dy == 0) return NavDirection.Right;
            if (dx < 0 && dy == 0) return NavDirection.Left;
            if (dx > 0 && dy > 0) return NavDirection.RightDown;
            if (dx > 0 && dy < 0) return NavDirection.RightUp;
            if (dx < 0 && dy > 0) return NavDirection.LeftDown;
            return NavDirection.LeftUp;
        }

        private IEnumerable<Coordinate> GetOrthogonalEmptyNeighbors(Sector sector, Coordinate coordinate)
        {
            var deltas = new[]
            {
                new Point(0, -1),
                new Point(1, 0),
                new Point(0, 1),
                new Point(-1, 0)
            };

            foreach (var delta in deltas)
            {
                var x = coordinate.X + delta.X;
                var y = coordinate.Y + delta.Y;
                if (x < 0 || y < 0 || x >= DEFAULTS.COORDINATE_MAX || y >= DEFAULTS.COORDINATE_MAX)
                {
                    continue;
                }

                var neighbor = sector.Coordinates.GetNoError(new Point(x, y));
                if (neighbor != null && neighbor.Item == CoordinateItem.Empty)
                {
                    yield return neighbor;
                }
            }
        }

        private int PickWeightedSectorIndex(IReadOnlyList<Sector> sectors, int hostileWeight)
        {
            var totalWeight = 0;
            var weights = new int[sectors.Count];

            for (var i = 0; i < sectors.Count; i++)
            {
                var hasHostiles = sectors[i].Coordinates.Any(c => c.Item == CoordinateItem.HostileShip);
                var weight = hasHostiles ? hostileWeight : 1;
                weights[i] = weight;
                totalWeight += weight;
            }

            var roll = Utility.Utility.Random.Next(totalWeight);
            var cumulative = 0;

            for (var i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                {
                    return i;
                }
            }

            return weights.Length - 1;
        }

        private bool SectorHasStarbase(Sector sector)
        {
            return sector?.Coordinates != null && sector.Coordinates.Any(c => c.Item == CoordinateItem.Starbase);
        }

        private void PlaceDeuteriumAt(Coordinate coordinate)
        {
            if (coordinate == null)
            {
                return;
            }

            var min = this.Config?.GetSetting<int>("DeuteriumMin") ?? 1;
            var max = this.Config?.GetSetting<int>("DeuteriumMax") ?? 75;

            if (max < min)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            var amount = Utility.Utility.Random.Next(min, max + 1);
            coordinate.Item = CoordinateItem.Deuterium;
            coordinate.Object = new Deuterium(amount);
        }

        private void PlaceDeuteriumCloudAt(Coordinate coordinate)
        {
            if (coordinate == null)
            {
                return;
            }

            var min = this.Config?.GetSetting<int>("DeuteriumMin") ?? 1;
            var max = this.Config?.GetSetting<int>("DeuteriumMax") ?? 75;

            if (max < min)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            var amount = Utility.Utility.Random.Next(min, max + 1);
            coordinate.Item = CoordinateItem.DeuteriumCloud;
            coordinate.Object = new DeuteriumCloud(amount);
        }

        private void PlaceTechnologyCacheAt(Coordinate coordinate)
        {
            if (coordinate == null)
            {
                return;
            }

            var min = this.GetSettingOrDefault("TechnologyCacheMinBonus", 500);
            var max = this.GetSettingOrDefault("TechnologyCacheMaxBonus", 2000);

            if (max < min)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            var amount = Utility.Utility.Random.Next(min, max + 1);
            coordinate.Item = CoordinateItem.TechnologyCache;
            coordinate.Object = new TechnologyCache(amount)
            {
                Coordinate = coordinate
            };
        }

        private void PlaceWormholeAt(Coordinate coordinate, Point destinationSector, int pairId)
        {
            if (coordinate == null || destinationSector == null)
            {
                return;
            }

            coordinate.Item = CoordinateItem.Wormhole;
            coordinate.Object = new Wormhole
            {
                Coordinate = coordinate,
                DestinationSector = new Point(destinationSector.X, destinationSector.Y),
                PairId = pairId
            };
        }

        private void NormalizeDeuteriumTotalsPerSector()
        {
            var minSectorTotal = this.GetSettingOrDefault("DeuteriumSectorTotalMin", 200);
            var maxSectorTotal = this.GetSettingOrDefault("DeuteriumSectorTotalMax", 800);

            if (maxSectorTotal < minSectorTotal)
            {
                var temp = minSectorTotal;
                minSectorTotal = maxSectorTotal;
                maxSectorTotal = temp;
            }

            foreach (var sector in this.Sectors.Where(s => s?.Coordinates != null))
            {
                var deuteriumCoordinates = sector.Coordinates
                    .Where(c => c.Item == CoordinateItem.Deuterium || c.Item == CoordinateItem.DeuteriumCloud)
                    .ToList();

                if (deuteriumCoordinates.Count == 0)
                {
                    continue;
                }

                var targetTotal = Utility.Utility.Random.Next(minSectorTotal, maxSectorTotal + 1);
                this.DistributeDeuteriumAcrossCoordinates(deuteriumCoordinates, targetTotal);
            }
        }

        private void DistributeDeuteriumAcrossCoordinates(IList<Coordinate> deuteriumCoordinates, int totalAmount)
        {
            if (deuteriumCoordinates == null || deuteriumCoordinates.Count == 0)
            {
                return;
            }

            var minCloudAmount = this.GetSettingOrDefault("DeuteriumCloudPointMin", 50);
            if (minCloudAmount < 1)
            {
                minCloudAmount = 1;
            }

            var minimumTotal = deuteriumCoordinates.Sum(c =>
                c.Item == CoordinateItem.DeuteriumCloud ? minCloudAmount : 1);

            if (totalAmount < minimumTotal)
            {
                totalAmount = minimumTotal;
            }

            var remaining = totalAmount;
            for (var i = 0; i < deuteriumCoordinates.Count; i++)
            {
                var coordinate = deuteriumCoordinates[i];
                var minimumForThisSlot = coordinate.Item == CoordinateItem.DeuteriumCloud ? minCloudAmount : 1;
                var minimumForRemaining = deuteriumCoordinates
                    .Skip(i + 1)
                    .Sum(c => c.Item == CoordinateItem.DeuteriumCloud ? minCloudAmount : 1);
                var maximumForThisSlot = remaining - minimumForRemaining;

                var assigned = i == deuteriumCoordinates.Count - 1
                    ? remaining
                    : Utility.Utility.Random.Next(minimumForThisSlot, maximumForThisSlot + 1);

                remaining -= assigned;

                if (coordinate.Item == CoordinateItem.Deuterium)
                {
                    coordinate.Object = new Deuterium(assigned);
                }
                else if (coordinate.Item == CoordinateItem.DeuteriumCloud)
                {
                    coordinate.Object = new DeuteriumCloud(assigned);
                }
            }
        }

        private int GetSettingOrDefault(string key, int defaultValue)
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

        private bool GetBoolSettingOrDefault(string key, bool defaultValue)
        {
            try
            {
                return this.Config?.GetSetting<bool>(key) ?? defaultValue;
            }
            catch
            {
                try
                {
                    return (this.Config?.GetSetting<int>(key) ?? (defaultValue ? 1 : 0)) != 0;
                }
                catch
                {
                    return defaultValue;
                }
            }
        }

        public IEnumerable<Coordinate> AddStarbases()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This will eventually be moved into each individual object
        /// </summary>
        public void GetGlobalInfo()
        {
            //this.Hostiles = new Hostiles(); //todo: create an initial size the same as HostilesToSetUp

            this.HostilesToSetUp = this.Config.GetSetting<int>("totalHostiles") + Utility.Utility.Random.Next(6);
            this.Stardate = this.Config.GetSetting<int>("stardate") + Utility.Utility.Random.Next(50);
            this.timeRemaining = this.Config.GetSetting<int>("timeRemaining") + Utility.Utility.Random.Next(10);
            this.starbases = this.Config.GetSetting<int>("starbases") + Utility.Utility.Random.Next(3);

            this.Text = this.Config.GetSetting<string>("CommandPrompt");

            this.Write.DebugLine("HostilesToSetUp: " + this.HostilesToSetUp);
            this.Write.DebugLine("Stardate: " + Stardate);
            this.Write.DebugLine("timeRemaining: " + this.HostilesToSetUp);
            this.Write.DebugLine("starbases: " + this.HostilesToSetUp);
        }

        //todo: refactor these to a setup object
        public void SetUpPlayerShip(CoordinateDef playerShipDef)
        {
            this.Write.DebugLine(this.Config.GetSetting<string>("DebugSettingUpPlayership"));

            //todo: remove this requirement
            if (this.Sectors.IsNullOrEmpty())
            {
                throw new GameException(this.Config.GetSetting<string>("SectorsNeedToBeSetup1"));
            }

            //todo: if playershipDef.GetFromConfig then grab info from config.  else set up with default random numbers.

            string playerShipName = this.Config.GetSetting<string>("PlayerShip");

            var startingSector = new Coordinate(new LocationDef(playerShipDef.SectorDef, new Point(playerShipDef.Coordinate.X, playerShipDef.Coordinate.Y)));

            this.Playership = new Ship(FactionName.Federation, playerShipName, startingSector, this)
            {
                Allegiance = Allegiance.GoodGuy,
                Energy = this.Config.GetSetting<int>("energy"),
                UsePlayerGlyph = true
            };
            this.Playership.MaxEnergy = this.Playership.Energy;
            this.Playership.InPlayerFleet = true;

            this.SetupPlayershipSector(playerShipDef);

            this.SetupSubsystems();

            this.Playership.Destroyed = false;
        }

        public void SetupSubsystems()
        {
            this.GetSubsystemSetupFromConfig();

            //todo:  do we pull strings from config and then put a switch statement below to set up individual systems??

            foreach (ISubsystem subSystem in this.Playership.Subsystems)
            {
                subSystem.Damage = 0;
                subSystem.ShipConnectedTo = this.Playership;
            }

            this.SetupPlayershipNav();
            this.SetupPlayershipShields();
            this.SetupPlayershipTorpedoes();

            //ShortRangeScan.For(this.Playership).Damage = 0;
            //ShortRangeScan.For(this.Playership).ShipConnectedTo = this.Playership;

            //LongRangeScan.For(this.Playership).Damage = 0;
            //LongRangeScan.For(this.Playership).ShipConnectedTo = this.Playership;

            //CombinedRangeScan.For(this.Playership).Damage = 0;
            //CombinedRangeScan.For(this.Playership).ShipConnectedTo = this.Playership;

            //Computer.For(this.Playership).Damage = 0;
            //Computer.For(this.Playership).ShipConnectedTo = this.Playership;

            //Phasers.For(this.Playership).Damage = 0;
            //Phasers.For(this.Playership).ShipConnectedTo = this.Playership;

            //DamageControl.For(this.Playership).Damage = 0;
            //DamageControl.For(this.Playership).ShipConnectedTo = this.Playership;
        }

        public void GetSubsystemSetupFromConfig()
        {
            //TODO: Finish this

            //var subsystemsToSetUp = new List<ISubsystem>();

            ////pull desired subsystem Setup from App.Config

            //foreach (var subsystem in appConfigSubSystem)
            //{
            //    //Set up SubSystem
            //    subsystemsToSetUp.Add(new Shields(this));

            //    //Might have to do a switch if we can't use reflection to create the objcet
            //}
        }

        public void SetupPlayershipSector(CoordinateDef playerShipDef)
        {
            if (playerShipDef.SectorDef == null)
            {
                var alphaSectors = this.Sectors
                    .Where(q => string.Equals(QuadrantRules.GetQuadrantName(this, q.X, q.Y), "Alpha", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var selectedSector = alphaSectors.Any()
                    ? alphaSectors[Utility.Utility.Random.Next(alphaSectors.Count)]
                    : null;

                playerShipDef.SectorDef = selectedSector != null
                    ? new Point(selectedSector.X, selectedSector.Y)
                    : Point.GetRandom();
            }

            if(!this.Sectors.Any())
            {
                throw new ArgumentException(this.Config.GetSetting<string>("SectorsNotSetUp"));
            }

            Sector regionWithPlayershipDef = this.Sectors.Single(q => q.X == playerShipDef.SectorDef.X && q.Y == playerShipDef.SectorDef.Y);
            this.Playership.Point = new Point(regionWithPlayershipDef.X, regionWithPlayershipDef.Y);

            this.Playership.GetSector().SetActive();
        }

        public void SetupPlayershipTorpedoes()
        {
            var torpedoes = Torpedoes.For(this.Playership);

            torpedoes.ShipConnectedTo = this.Playership;
            torpedoes.Count = this.Config.GetSetting<int>("photonTorpedoes");
            //torpedoes.Damage = 0;
        }

        public void SetupPlayershipShields()
        {
            var starshipShields = Shields.For(this.Playership);

            //starshipShields.ShipConnectedTo = this.Playership;
            starshipShields.Energy = 0;
            //starshipShields.Damage = 0;
        }

        public void SetupPlayershipNav()
        {
            var starshipNAV = Navigation.For(this.Playership);

            //starshipNAV.ShipConnectedTo = this.Playership;
            starshipNAV.Movement.ShipConnectedTo = this.Playership;
            //starshipNAV.Damage = 0;
            starshipNAV.MaxWarpFactor = this.Config.GetSetting<int>("MaxWarpFactor");
            starshipNAV.Docked = false;
        }

        /// <summary>
        /// Legacy code. todo: needs to be rewritten.  Checks all sectors around starbase to see if its a good place to dock.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="sectors"></param>
        /// <returns></returns>
        public bool IsDockingLocation(int i, int j, Coordinates sectors)
        {
            //http://stackoverflow.com/questions/3150678/using-linq-with-2d-array-select-not-found
            for (int y = i - 1; y <= i + 1; y++)
            {
                for (int x = j - 1; x <= j + 1; x++)
                {
                    Coordinate gotSector = Coordinates.GetNoError(x, y, sectors);

                    if (gotSector?.Item == CoordinateItem.Starbase)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        ///// <summary>
        ///// TODO: this needs to be changed.  after destruction, it appears to take several method returns to realize that we are dead.
        ///// </summary>
        ///// <returns></returns>
        //public bool ALLHostilesAttack()
        //{
        //    //this is called from torpedo control/phaser control, and navigation control

        //    if (this.Hostiles.Count > 0)
        //    {
        //        var starship = Navigation.For(this.Playership);
        //        foreach (var badGuy in this.Hostiles)
        //        {
        //            if (starship.docked)
        //            {
        //                Console.WriteLine(
        //                    this.Playership.Name + " hit by ship at sector [{0},{1}]. No damage due to starbase shields.",
        //                    (badGuy.Coordinate.X), (badGuy.Coordinate.Y));
        //            }
        //            else
        //            {
        //                if (!Ship.AbsorbHitFrom(badGuy, this)) return true;
        //            }
        //        }
        //        return true;
        //    }

        //    return false;
        //}

        public CoordinateItem GetItem(int SectorX, int SectorY, int sectorX, int sectorY)
        {
            var item = this.Get(SectorX, SectorY, sectorX, sectorY).Item;
            return item;
        }

        public Coordinate Get(int SectorX, int SectorY, int sectorX, int sectorY)
        {
            var item = this.Sectors.Single(q => q.X == SectorX &&
                                                  q.Y == SectorY).Coordinates.Single(s => s.X == sectorX &&
                                                                                        s.Y == sectorY);
            return item;
        }

        public void RemoveAllDestroyedShips(IMap map, IEnumerable<IShip> destroyedShips)
        {
            map.Sectors.Remove(destroyedShips);
            map.Sectors.GetActive().GetHostiles().RemoveAll(s => s.Destroyed);
        }
        public void RemoveDestroyedShipsAndScavenge(List<IShip> destroyedShips)
        {
            this.ApplyHostileDestructionSplashDamage(destroyedShips);
            this.RemoveAllDestroyedShips(this, destroyedShips); //remove from Hostiles collection

            foreach (var destroyedShip in destroyedShips)
            {
                this.Game?.AppendGameEventLog($"Ship destroyed: {destroyedShip?.Name} faction={destroyedShip?.Faction} sector [{destroyedShip?.Point?.X},{destroyedShip?.Point?.Y}] coord [{destroyedShip?.Coordinate?.X},{destroyedShip?.Coordinate?.Y}]");
                this.Playership.Scavenge(destroyedShip.Faction == FactionName.Federation
                    ? ScavengeType.FederationShip
                    : ScavengeType.OtherShip);

                //todo: add else if for starbase when the time comes
            }

            this.Playership.UpdateDivinedSectors();
        }

        private void ApplyHostileDestructionSplashDamage(IEnumerable<IShip> destroyedShips)
        {
            var destroyedList = destroyedShips?.Where(s => s != null && s.Allegiance == Allegiance.BadGuy).ToList();
            if (destroyedList == null || destroyedList.Count == 0)
            {
                return;
            }

            var damage = this.GetSettingOrDefault("HostileDestructionSplashDamage", 100);
            if (damage <= 0)
            {
                return;
            }

            foreach (var destroyed in destroyedList)
            {
                var sector = destroyed.GetSector();
                if (sector?.Coordinates == null)
                {
                    continue;
                }

                var nearbyTargets = sector.GetHostiles()
                    .Where(h => !h.Destroyed && !string.Equals(h.Name, destroyed.Name, StringComparison.OrdinalIgnoreCase))
                    .Cast<IShip>()
                    .ToList();
                nearbyTargets.Add(this.Playership);

                foreach (var target in nearbyTargets.Distinct())
                {
                    if (target?.Coordinate == null || target.Destroyed)
                    {
                        continue;
                    }

                    var dx = Math.Abs(target.Coordinate.X - destroyed.Coordinate.X);
                    var dy = Math.Abs(target.Coordinate.Y - destroyed.Coordinate.Y);
                    if (dx > 1 || dy > 1)
                    {
                        continue;
                    }

                    if (target == this.Playership)
                    {
                        this.Write?.Line($"Shockwave from destroyed hostile {destroyed.Name} damaged your ship for {damage}.");
                        this.Game?.AppendGameEventLog($"Splash damage: destroyed hostile {destroyed.Name} dealt {damage} to playership");
                    }

                    if (target is Ship concreteTarget)
                    {
                        concreteTarget.AbsorbHitFrom(destroyed, damage);
                    }
                }
            }
        }

        public void RemoveTargetFromSector(IMap map, IShip ship)
        {
            map.Sectors.Remove(ship);
        }

        /// <summary>
        ///  Removes all friendlies fromevery sector in the entire map.
        ///  zips through all sectors in all Sectors.  remove any friendlies
        ///  This is a bit of a brute force approach, and not preferred, as it disguises any bugs that might have to do with forgetting
        ///  to remove the ship at the right time.  This function will need to go away or stop being used when or if this game is modified
        ///  to have multiple friendlies, as is the eventual plan.
        /// </summary>
        /// <param name="map"></param>
        public void RemovePlayership(IMap map)
        {
            var coordinate = map.Playership.Coordinate;
            if (coordinate?.Object is GaseousAnomaly)
            {
                coordinate.Item = CoordinateItem.GaseousAnomaly;
            }
            else if (coordinate?.Object is TemporalRift)
            {
                coordinate.Item = CoordinateItem.TemporalRift;
            }
            else if (coordinate?.Object is SporeField)
            {
                coordinate.Item = CoordinateItem.SporeField;
            }
            else if (coordinate?.Object is BlackHole)
            {
                coordinate.Item = CoordinateItem.BlackHole;
            }
            else if (coordinate?.Object is TechnologyCache)
            {
                coordinate.Item = CoordinateItem.TechnologyCache;
            }
            else if (coordinate?.Object is Wormhole)
            {
                coordinate.Item = CoordinateItem.Wormhole;
            }
            else if (coordinate != null)
            {
                coordinate.Item = CoordinateItem.Empty;
                coordinate.Object = null;
            }
        }

        public List<Coordinate> GetHostileOutposts()
        {
            return this.Sectors
                .Where(s => s?.Coordinates != null)
                .SelectMany(s => s.Coordinates)
                .Where(c => c.Item == CoordinateItem.HostileOutpost)
                .ToList();
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.  Sets down a friendly 
        /// </summary>
        /// <param name="map"></param>
        public void SetPlayershipInActiveSector(IMap map)
        {
            this.RemovePlayership(map);

            var activeSector = map.Sectors.GetActive();

            var targetCoordinate = activeSector.Coordinates[map.Playership.Coordinate.X, map.Playership.Coordinate.Y];
            this.ConsumeDeuteriumIfPresent(map.Playership, targetCoordinate);
            targetCoordinate.Item = CoordinateItem.PlayerShip;
            targetCoordinate.Object = map.Playership;
            this.BringPlayerFleetToSector(activeSector);
            (this.Game as Game)?.HandlePlayerSectorVisibilityChange(activeSector);
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.  Sets down a friendly 
        /// </summary>
        /// <param name="shipToSet"></param>
        /// <param name="map"></param>
        /// <param name="newLocation"></param>
        public void SetPlayershipInLocation(IShip shipToSet, IMap map, Location newLocation)
        {
            var previousSector = shipToSet?.GetSector();
            this.RemovePlayership(map);

            newLocation.Sector.SetActive();

            Coordinate foundSector = this.LookupSector(shipToSet.GetSector(), newLocation);
            var encounteredWormhole = foundSector?.Item == CoordinateItem.Wormhole
                ? foundSector.Object as Wormhole
                : null;
            this.ConsumeDeuteriumIfPresent(shipToSet, foundSector);
            foundSector.Item = CoordinateItem.PlayerShip;
            foundSector.Object = shipToSet;

            shipToSet.Point = new Point(newLocation.Sector.X, newLocation.Sector.Y);
            shipToSet.Coordinate = foundSector;
            var changedSector = previousSector == null ||
                                previousSector.X != newLocation.Sector.X ||
                                previousSector.Y != newLocation.Sector.Y;
            if (changedSector)
            {
                this.BringPlayerFleetToSector(newLocation.Sector);
            }
            this.ResolveWormholeTransitIfPresent(shipToSet, foundSector, encounteredWormhole);
            (this.Game as Game)?.HandlePlayerSectorVisibilityChange(newLocation.Sector);
        }

        public IEnumerable<Ship> GetPlayerFleetShips()
        {
            return this.Sectors?
                       .SelectMany(s => s?.Coordinates ?? Enumerable.Empty<Coordinate>())
                       .Where(c => c?.Object is Ship)
                       .Select(c => (Ship)c.Object)
                       .Where(ship => ship.InPlayerFleet && !ship.Destroyed)
                       .Distinct()
                   ?? Enumerable.Empty<Ship>();
        }

        public void PromoteToPlayership(Ship ship)
        {
            if (ship == null || this.Playership == null || ship == this.Playership)
            {
                return;
            }

            var previousFlagship = this.Playership;
            previousFlagship.InPlayerFleet = true;
            previousFlagship.Allegiance = Allegiance.GoodGuy;
            ship.InPlayerFleet = true;
            ship.Allegiance = Allegiance.GoodGuy;

            var previousSectorCoordinate = previousFlagship.GetSector().Coordinates
                .GetNoError(new Point(previousFlagship.Coordinate.X, previousFlagship.Coordinate.Y));
            if (previousSectorCoordinate != null)
            {
                previousSectorCoordinate.Item = CoordinateItem.FriendlyShip;
                previousSectorCoordinate.Object = previousFlagship;
                previousFlagship.Coordinate = previousSectorCoordinate;
            }

            this.Playership = ship;
            DEFAULTS.PLAYERSHIP = this.ResolvePlayershipGlyph(ship);
            var playershipSectorCoordinate = ship.GetSector().Coordinates
                .GetNoError(new Point(ship.Coordinate.X, ship.Coordinate.Y));
            if (playershipSectorCoordinate != null)
            {
                playershipSectorCoordinate.Item = CoordinateItem.PlayerShip;
                playershipSectorCoordinate.Object = ship;
                ship.Coordinate = playershipSectorCoordinate;
            }

            Navigation.For(previousFlagship).Docked = false;
            Navigation.For(ship).Docked = false;
        }

        private string ResolvePlayershipGlyph(Ship ship)
        {
            if (ship?.UsePlayerGlyph == true)
            {
                try
                {
                    return this.Config.GetSetting<string>("PlayerShipGlyph");
                }
                catch
                {
                    return DEFAULTS.PLAYERSHIP;
                }
            }

            try
            {
                var glyph = this.Config.Get?.FactionDetails(ship?.Faction)?.designator;
                if (!string.IsNullOrWhiteSpace(glyph))
                {
                    return glyph;
                }
            }
            catch
            {
                // Fall back below.
            }

            return DEFAULTS.PLAYERSHIP;
        }

        public void BringPlayerFleetToSector(Sector sector)
        {
            if (sector?.Coordinates == null || this.Playership == null)
            {
                return;
            }

            var fleetShips = this.GetPlayerFleetShips()
                .Where(ship => ship != this.Playership)
                .ToList();

            foreach (var ship in fleetShips)
            {
                var currentSector = ship.GetSector();
                var origin = currentSector?.Coordinates?.GetNoError(new Point(ship.Coordinate.X, ship.Coordinate.Y));
                if (origin != null && origin.Object == ship)
                {
                    origin.Item = CoordinateItem.Empty;
                    origin.Object = null;
                }

                var destination = sector.Coordinates
                    .Where(c => c.Item == CoordinateItem.Empty)
                    .OrderByDescending(c => Math.Abs(c.X - this.Playership.Coordinate.X) + Math.Abs(c.Y - this.Playership.Coordinate.Y))
                    .ThenBy(_ => Utility.Utility.Random.Next())
                    .FirstOrDefault();

                if (destination == null)
                {
                    continue;
                }

                ship.Point = new Point(sector.X, sector.Y);
                ship.Coordinate = destination;
                destination.Item = CoordinateItem.FriendlyShip;
                destination.Object = ship;
                Navigation.For(ship).Docked = false;
            }
        }

        private void ConsumeDeuteriumIfPresent(IShip shipToSet, Coordinate coordinate)
        {
            if (coordinate == null || shipToSet == null)
            {
                return;
            }

            if (coordinate.Item == CoordinateItem.Deuterium)
            {
                var deuterium = coordinate.Object as Deuterium;
                var amount = deuterium?.Amount ?? 0;
                if (amount > 0)
                {
                    shipToSet.Energy += amount;
                }

                coordinate.Item = CoordinateItem.Empty;
                coordinate.Object = null;

                this.Write?.Line("Bussard collectors have been charged with deuterium.");
            }

            if (coordinate.Item == CoordinateItem.DeuteriumCloud)
            {
                var cloud = coordinate.Object as DeuteriumCloud;
                var amount = cloud?.Amount ?? 0;
                if (amount > 0)
                {
                    shipToSet.Energy += amount;
                }

                coordinate.Item = CoordinateItem.Empty;
                coordinate.Object = null;

                this.Write?.Line("Bussard collectors have been charged with deuterium from a cloud.");
            }

            if (coordinate.Item == CoordinateItem.GraviticMine)
            {
                var damage = this.Config?.GetSetting<int>("GraviticMineDamage") ?? 200;
                shipToSet.Energy -= damage;

                coordinate.Item = CoordinateItem.Empty;
                coordinate.Object = null;

                this.Write?.Line("Gravitic mine detonated and damaged the playership.");
            }

            if (coordinate.Item == CoordinateItem.EnergyAnomaly)
            {
                var anomaly = coordinate.Object as EnergyAnomaly;
                var glyph = anomaly?.Glyph ?? "~";

                this.Game?.TriggerSystemsCascadeFromAnomaly(glyph);

                coordinate.Item = CoordinateItem.Empty;
                coordinate.Object = null;
                this.Game?.AppendGameEventLog($"Item consumed: energy anomaly glyph={glyph} at coord [{coordinate.X},{coordinate.Y}]");
            }

            if (coordinate.Item == CoordinateItem.TechnologyCache)
            {
                var cache = coordinate.Object as TechnologyCache;
                var bonus = cache?.MaxEnergyBonus ?? 0;
                if (bonus > 0 && shipToSet is Ship ship)
                {
                    ship.MaxEnergy += bonus;
                    ship.Energy = ship.MaxEnergy;
                }
                else if (shipToSet is Ship shipNoBonus)
                {
                    // Keep energy in sync when cache exists but bonus is non-positive.
                    shipNoBonus.Energy = Math.Min(shipNoBonus.Energy, shipNoBonus.MaxEnergy);
                }

                this.Write?.Line(bonus > 0
                    ? $"Technology cache recovered. Maximum energy capacity increased by {bonus}."
                    : "Technology cache recovered. No usable energy increase was found.");
                this.Game?.AppendGameEventLog($"Item consumed: technology cache bonus={bonus} at coord [{coordinate.X},{coordinate.Y}]");

                coordinate.Item = CoordinateItem.Empty;
                coordinate.Object = null;
            }

            if (coordinate.Item == CoordinateItem.SporeField)
            {
                if (shipToSet is Ship ship)
                {
                    ship.SporeContaminated = true;
                }

                this.Write?.Line("Spore contact detected. Biofilm is draining reactor efficiency.");
            }
        }

        private void ResolveWormholeTransitIfPresent(IShip shipToSet, Coordinate coordinate, Wormhole encounteredWormhole = null)
        {
            var wormhole = encounteredWormhole ?? coordinate?.Object as Wormhole;
            if (shipToSet == null || wormhole?.DestinationSector == null)
            {
                return;
            }

            var destinationSector = this.Sectors[wormhole.DestinationSector];
            var emptyCoordinates = destinationSector?.Coordinates?
                .Where(c => c.Item == CoordinateItem.Empty)
                .ToList();
            if (destinationSector == null || emptyCoordinates == null || emptyCoordinates.Count == 0)
            {
                this.Write?.Line("Wormhole destabilized. No clear emergence point was available.");
                this.Game?.AppendGameEventLog($"Wormhole transit failed: pair={wormhole.PairId} destination sector [{wormhole.DestinationSector.X},{wormhole.DestinationSector.Y}] had no empty coordinate.");
                return;
            }

            var destinationCoordinate = emptyCoordinates[Utility.Utility.Random.Next(emptyCoordinates.Count)];
            this.Write?.Line("Wormhole engaged...");
            this.Write?.Line($"Ship emerged at sector [{destinationSector.X},{destinationSector.Y}], coord [{destinationCoordinate.X},{destinationCoordinate.Y}].");
            this.Game?.AppendGameEventLog($"Wormhole transit: pair={wormhole.PairId} from sector [{shipToSet.Point?.X},{shipToSet.Point?.Y}] to sector [{destinationSector.X},{destinationSector.Y}] coord [{destinationCoordinate.X},{destinationCoordinate.Y}]");

            if (coordinate != null)
            {
                coordinate.Object = wormhole;
            }
            this.RemovePlayership(this);
            destinationSector.SetActive();
            destinationCoordinate.Item = CoordinateItem.PlayerShip;
            destinationCoordinate.Object = shipToSet;
            shipToSet.Point = new Point(destinationSector.X, destinationSector.Y);
            shipToSet.Coordinate = destinationCoordinate;
            this.timeRemaining = Math.Max(0, this.timeRemaining - 1);
            this.Stardate++;
            (this.Game as Game)?.HandlePlayerUsedWormhole();
        }

        private Coordinate LookupSector(Sector oldSector, Location newLocation)
        {
            //todo: divine where ship should be with old region and newlocation with negative numbers

            IEnumerable<Coordinate> matchingSectors = newLocation.Sector.Coordinates
                                                  .Where(s => s.X == newLocation.Coordinate.X && 
                                                              s.Y == newLocation.Coordinate.Y);
            Coordinate foundSector;

            try
            {
                foundSector = matchingSectors.Single();
            }
            catch (Exception)
            {
                //todo: if sector not found then this is a bug.
                throw new ArgumentException($"Coordinate {newLocation.Coordinate.X}, {newLocation.Coordinate.Y}");
            }

            return foundSector;
        }

        public void AddACoupleHostileFederationShipsToExistingMap()
        {
            List<string> federationShipNames = this.Config.ShipNames(FactionName.Federation);
            var federaleNames = new Stack<string>(federationShipNames.Shuffle());

            foreach (var region in this.Sectors)
            {
                bool added = this.AddHostileFedToEmptySector(region, federaleNames);
                if (added) break; //we found an empty Sector to dump new fed ships into..
            }
        }

        private bool AddHostileFedToEmptySector(ISector Sector, Stack<string> federaleNames)
        {
            var HostilesInSector = Sector.GetHostiles();
            if (HostilesInSector.Any()) //we don't want to mix with Klingons just yet..
            {
                var klingons = HostilesInSector.Where(h => h.Faction == FactionName.Klingon);

                if (!klingons.Any())
                {
                    this.AddHostilesToSector(Sector, federaleNames);

                    //we are only doing this once..
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a random number of federation ships to the map
        /// </summary>
        public void AddHostileFederationShipsToExistingMap()
        {
            var federationShipNames = this.Config.ShipNames(FactionName.Federation);
            var federaleNames = new Stack<string>(federationShipNames.Shuffle());

            foreach (var Sector in this.Sectors)
            {
                if (Utility.Utility.Random.Next(5) == 1) //todo: resource this out.
                {
                    if (!Sector.GetHostiles().Any()) //we don't want to mix with Klingons just yet..
                    {
                        this.AddHostilesToSector(Sector, federaleNames);
                    }
                }
            }
        }

        private void AddHostilesToSector(ISector Sector, Stack<string> federaleNames)
        {
            var numberOfHostileFeds = Utility.Utility.Random.Next(2);

            for (int i = 0; i < numberOfHostileFeds; i++)
            {
                //todo: refactor to Sector object
                ICoordinate coordinateToAddTo = this.GetUnoccupiedRandomCoordinate(Sector);
                this.AddHostileFederale(Sector, coordinateToAddTo, federaleNames);
            }
        }

        private ICoordinate GetUnoccupiedRandomCoordinate(ISector Sector)
        {
            Point randomCoordinate = GetRandomCoordinate();

            ICoordinate coordinate = Sector.GetCoordinate(randomCoordinate);
            bool sectorIsEmpty = coordinate.Item == CoordinateItem.Empty;

            if (!sectorIsEmpty)
            {
                coordinate = this.GetUnoccupiedRandomCoordinate(Sector);
            }

            return coordinate;
        }

        private static Point GetRandomCoordinate()
        {
            int x = Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MIN);
            int y = Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MAX);

            var randomCoordinate = new Point(x, y);
            return randomCoordinate;
        }

        private void AddHostileFederale(ISector Sector, ICoordinate coordinate, Stack<string> federaleNames)
        {
            var newPissedOffFederale = new Ship(FactionName.Federation, federaleNames.Pop(), coordinate, this);
            Shields.For(newPissedOffFederale).Energy = Utility.Utility.Random.Next(100, 500); //todo: resource those numbers out

            Sector.AddShip(newPissedOffFederale, coordinate);

            this.Write.Line("Comm Reports a Federation starship has warped into Sector: " + Sector.Name);
        }

        public IEnumerable<IShip> GetAllFederationShips()
        {
            //todo: finish this.
            return new List<Ship>();
        }

        public bool OutOfBounds(Sector region)
        {
            bool result;

            if (region != null)
            {
                bool inTheNegative = region.X < 0 || region.Y < 0;
                bool maxxed = region.X == DEFAULTS.SECTOR_MAX || region.Y == DEFAULTS.SECTOR_MAX;

                bool yOnMap = region.Y >= 0 && region.Y < DEFAULTS.SECTOR_MAX;
                bool xOnMap = region.X >= 0 && region.X < DEFAULTS.SECTOR_MAX;

                result = (inTheNegative || maxxed) && !(yOnMap && xOnMap);
            }
            else
            {
                result = true;
            }

            return result;
        }

        public string GetConfigText(string textToGet)
        {
            return this.Config.GetText(textToGet);
        }
    }
}


//private static List<Coordinate> GetSectorObjects(int starbases, int HostilesToSetUp)
//{
//    var SectorObjects = new List<Coordinate>();

//    //get stars for Sector and subtract from parameter (will be subtracted when this is hit next?)
//    //newSector.Stars = 1 + (Utility.Random).Next(Constants.SECTOR_MAX);
//    //get hostiles for Sector and subtract from big list
//    //get starbase T/F and subtract from big list
//    return SectorObjects;
//}


//public override string ToString()
//{
//    string returnVal = null;

//    //if debugMode
//    //returns the location of every single object in the map

//    //roll out every object in:
//    //this.Sectors;
//    //this.GameConfig;
            

//    return returnVal;
//}

//todo: finish this
//public CoordinateItem GetShip(int SectorX, int SectorY, int sectorX, int sectorY)
//{
//    var t = this.Sectors.Where(q => q.X == SectorX &&
//                                      q.Y == SectorY).Single().Coordinates.Where(s => s.X == sectorX &&
//                                                                                    s.Y == sectorY).Single().Item;


//}

///// <summary>
///// Removes all friendlies fromevery sector in the entire map.
///// </summary>
///// <param name="map"></param>
//public void RemoveAllFriendlies(IMap map)
//{
//    var sectorsWithFriendlies = map.Sectors.SelectMany(Sector => Sector.Coordinates.Where(sector => sector.Item == CoordinateItem.Friendly));

//    foreach (Coordinate sector in sectorsWithFriendlies)
//    {
//        sector.Item = CoordinateItem.Empty;
//    }
//}


