using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    //todo: this functionality is currently broken
    //todo: fix hostiles and starbases and stars to test fully
    public class LongRangeScan : SubSystem_Base
    {
        public LongRangeScan(IShip shipConnectedTo) : base(shipConnectedTo)
        {
            this.Type = SubsystemType.LongRangeScan;
        }

        public override List<string> Controls(string command)
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();
            throw new NotImplementedException();
        }

        public List<string> Controls()
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) return this.ShipConnectedTo.OutputQueue();

            //todo: refactor this pattern with LRS

            Location myLocation = this.ShipConnectedTo.GetLocation();
            var renderedResults = this.RunFullLRSScan(myLocation);

            foreach (string line in renderedResults)
            {
                this.ShipConnectedTo.Map.Game.Interact.SingleLine(line);
            }

            this.ShipConnectedTo.OutputLine("");

            return this.ShipConnectedTo.OutputQueue();
        }

        public List<string> ControlsPlus(int gridSize, int energyCost, string title, bool markDeuteriumSectors)
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) return this.ShipConnectedTo.OutputQueue();

            if (energyCost > 0)
            {
                if (this.ShipConnectedTo.Energy < energyCost)
                {
                    this.ShipConnectedTo.OutputLine($"Insufficient energy for scan. Required: {energyCost}. Current: {this.ShipConnectedTo.Energy}.");
                    return this.ShipConnectedTo.OutputQueue();
                }

                this.ShipConnectedTo.Energy -= energyCost;
            }

            Location myLocation = this.ShipConnectedTo.GetLocation();
            var renderedResults = this.RunFullLRSScan(myLocation, gridSize, title, markDeuteriumSectors);

            foreach (string line in renderedResults)
            {
                this.ShipConnectedTo.Map.Game.Interact.SingleLine(line);
            }

            this.ShipConnectedTo.OutputLine("");

            return this.ShipConnectedTo.OutputQueue();
        }

        //used by CRS
        public List<string> RunLRSScan(Location shipLocation)
        {
            IGame game = this.ShipConnectedTo.Map.Game;

            IEnumerable<LRSResult> tlrsResults = shipLocation.Sector.GetLRSFullData(shipLocation, game);
            var renderedData = game.Interact.RenderLRSData(tlrsResults, game);

            return renderedData;
        }

        private IEnumerable<string> RunFullLRSScan(Location shipLocation)
        {
            return this.RunFullLRSScan(shipLocation, 3, "*** Long Range Scan ***", false);
        }

        private IEnumerable<string> RunFullLRSScan(Location shipLocation, int gridSize, string title, bool markDeuteriumSectors)
        {
            //todo: if inefficiency ever becomes a problem this this could be split out into just getting names
            var lrsData = shipLocation.Sector.GetLRSFullData(shipLocation, this.ShipConnectedTo.Map.Game, gridSize).ToList();
            if (markDeuteriumSectors)
            {
                var marker = this.GetDeuteriumSectorMarker();
                foreach (var result in lrsData.Where(r => !r.GalacticBarrier && r.HasDeuterium))
                {
                    result.SectorName = $"{result.SectorName} ({marker})";
                }
            }

            IEnumerable<string> renderedData = this.ShipConnectedTo.Map.Game.Interact.RenderScanWithNames(ScanRenderType.SingleLine, title, lrsData.Cast<IScanResult>().ToList(), this.ShipConnectedTo.Map.Game);

            return renderedData;
        }

        private string GetDeuteriumSectorMarker()
        {
            try
            {
                var marker = this.ShipConnectedTo.Map.Game.Config.GetSetting<string>("DeuteriumSectorIndicator");
                if (!string.IsNullOrWhiteSpace(marker))
                {
                    return marker.Trim();
                }
            }
            catch
            {
                // Fallback below
            }

            return ".";
        }

        public LRSResult GetInfo(Map map, string sectorName)
        {
            var sectorResult = new LRSResult();

            Sector sectorToScan = map.Sectors.Get(sectorName);

            sectorResult.Point = sectorToScan.GetPoint();
            sectorResult.Hostiles = 0;
            sectorResult.Starbases = 0;
            sectorResult.Stars = 0;
            sectorResult.GalacticBarrier = sectorToScan.Type == SectorType.GalacticBarrier;
            sectorResult.QuadrantName = QuadrantRules.GetQuadrantName(map, sectorToScan.X, sectorToScan.Y);

            if (sectorToScan.Type != SectorType.Nebulae)
            {
                sectorResult.Hostiles = sectorToScan.GetHostiles().Count;
                sectorResult.Starbases = sectorToScan.GetStarbaseCount();
                sectorResult.Stars = sectorToScan.GetStarCount();
                sectorResult.FeatureMask = GetSectorFeatureMask(sectorToScan);
                sectorResult.HasDeuterium = sectorToScan.Coordinates != null &&
                                            sectorToScan.Coordinates.Any(c =>
                                                c.Item == CoordinateItem.Deuterium || c.Item == CoordinateItem.DeuteriumCloud);
            }

            var barrierID = this.ShipConnectedTo.Map.Game.Config.GetSetting<string>("GalacticBarrierText");
            sectorResult.Name = sectorResult.GalacticBarrier ? barrierID : sectorToScan.Name;

            sectorToScan.Scanned = true;

            return sectorResult;
        }

        public static LRSResult Execute(Sector sectorToScan)
        {
            var sectorResult = new LRSResult
            {
                Point = sectorToScan.GetPoint()
            };
            sectorResult.GalacticBarrier = sectorToScan.Type == SectorType.GalacticBarrier;
            sectorResult.QuadrantName = QuadrantRules.GetQuadrantName(sectorToScan.Map, sectorToScan.X, sectorToScan.Y);

            if (sectorToScan.Type != SectorType.Nebulae)
            {
                sectorResult.Hostiles = sectorToScan.GetHostiles().Count;
                sectorResult.Starbases = sectorToScan.GetStarbaseCount();
                sectorResult.Stars = sectorToScan.GetStarCount();
                sectorResult.FeatureMask = GetSectorFeatureMask(sectorToScan);
                sectorResult.HasDeuterium = sectorToScan.Coordinates != null &&
                                            sectorToScan.Coordinates.Any(c =>
                                                c.Item == CoordinateItem.Deuterium || c.Item == CoordinateItem.DeuteriumCloud);
                sectorResult.Name = sectorResult.GalacticBarrier
                    ? sectorToScan.Map.Game.Config.GetSetting<string>("GalacticBarrierText")
                    : sectorToScan.Name;
            }
            else
            {
                sectorResult.Hostiles = null; //because LRS can't penetrate nebulae
                sectorResult.Starbases = null; //because LRS can't penetrate nebulae
                sectorResult.Stars = null; //because LRS can't penetrate nebulae
                sectorResult.Name = sectorToScan.Name;
            }

            sectorToScan.Scanned = true;

            return sectorResult;
        }

        public void Debug_Scan_All_Sectors(bool setScanned)
        {
            foreach (var Sector in this.ShipConnectedTo.Map.Sectors)
            {
                Sector.Scanned = setScanned;
            }
        }

        public new static LongRangeScan For(IShip ship)
        {
            return (LongRangeScan)SubSystem_Base.For(ship, SubsystemType.LongRangeScan);
        }

        private static LrsFeatureMask GetSectorFeatureMask(Sector sectorToScan)
        {
            if (sectorToScan?.Coordinates == null)
            {
                return LrsFeatureMask.None;
            }

            var mask = LrsFeatureMask.None;

            if (sectorToScan.Coordinates.Any(c => c.Item == CoordinateItem.Starbase))
            {
                mask |= LrsFeatureMask.Starbase;
            }

            if (sectorToScan.Coordinates.Any(c => c.Item == CoordinateItem.Wormhole))
            {
                mask |= LrsFeatureMask.Wormhole;
            }

            if (sectorToScan.Coordinates.Any(c => c.Item == CoordinateItem.Deuterium || c.Item == CoordinateItem.DeuteriumCloud))
            {
                mask |= LrsFeatureMask.Deuterium;
            }

            if (sectorToScan.Coordinates.Any(c => c.Item == CoordinateItem.TemporalRift))
            {
                mask |= LrsFeatureMask.TemporalRift;
            }

            if (sectorToScan.Coordinates.Any(c => c.Item == CoordinateItem.TechnologyCache))
            {
                mask |= LrsFeatureMask.TechnologyCache;
            }

            if (sectorToScan.Coordinates.Any(c =>
                    c.Item == CoordinateItem.GraviticMine ||
                    c.Item == CoordinateItem.GaseousAnomaly ||
                    c.Item == CoordinateItem.SporeField ||
                    c.Item == CoordinateItem.BlackHole ||
                    c.Item == CoordinateItem.EnergyAnomaly))
            {
                mask |= LrsFeatureMask.Hazard;
            }

            return mask;
        }
    }
}
