using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class ImmediateRangeScan : SubSystem_Base
    {
        public ImmediateRangeScan(IShip shipConnectedTo): base(shipConnectedTo)
        {
            this.Type = SubsystemType.ImmediateRangeScan;
        }

        public IEnumerable<string> Controls()
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            if (this.Damaged()) return this.ShipConnectedTo.OutputQueue();

            //todo: refactor this pattern with LRS

            Location myLocation = this.ShipConnectedTo.GetLocation();
            var renderedResults = this.RunFullIRSScan(myLocation, 3, "*** Immediate Range Scan ***");

            foreach (string line in renderedResults)
            {
                this.ShipConnectedTo.Map.Game.Interact.SingleLine(line);
            }

            this.ShipConnectedTo.OutputLine("");
            this.OutputNebulaDegradationMessageIfNeeded();

            return this.ShipConnectedTo.OutputQueue();
        }

        public IEnumerable<string> ControlsPlus(int gridSize, int energyCost, string title)
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
            IEnumerable<string> renderedResults;
            if (gridSize >= DEFAULTS.COORDINATE_MAX)
            {
                renderedResults = this.RunFullSectorIRSScan(myLocation, title);
            }
            else
            {
                renderedResults = this.RunFullIRSScan(myLocation, gridSize, title);
            }

            foreach (string line in renderedResults)
            {
                this.ShipConnectedTo.Map.Game.Interact.SingleLine(line);
            }

            this.ShipConnectedTo.OutputLine("");
            this.OutputNebulaDegradationMessageIfNeeded();
            if (gridSize >= DEFAULTS.COORDINATE_MAX)
            {
                this.ConsumeFullSectorScanTurn();
            }

            return this.ShipConnectedTo.OutputQueue();
        }

        private void ConsumeFullSectorScanTurn()
        {
            var map = this.ShipConnectedTo?.Map;
            if (map == null)
            {
                return;
            }

            map.timeRemaining = Math.Max(0, map.timeRemaining - 1);
            map.Stardate++;
        }

        private void OutputNebulaDegradationMessageIfNeeded()
        {
            if (this.ShipConnectedTo?.GetSector()?.Type == SectorType.Nebulae)
            {
                this.ShipConnectedTo.OutputLine("Due to nebula interference, immediate scans are degraded.");
            }
        }

        private IEnumerable<string> RunFullSectorIRSScan(Location shipLocation, string title)
        {
            var irsData = new List<IScanResult>();
            var sector = shipLocation.Sector;
            for (var y = 0; y < DEFAULTS.COORDINATE_MAX; y++)
            {
                for (var x = 0; x < DEFAULTS.COORDINATE_MAX; x++)
                {
                    var result = sector.GetSectorInfo(sector, new Point(x, y), false, this.ShipConnectedTo.Map.Game);
                    result.MyLocation = shipLocation.Coordinate.X == x && shipLocation.Coordinate.Y == y;
                    sector.ApplyHostileScanResolution(result, shipLocation, this.ShipConnectedTo.Map.Game);
                    irsData.Add(result);
                }
            }

            return this.ShipConnectedTo.Map.Game.Interact.RenderScanWithNames(
                ScanRenderType.DoubleSingleLine,
                title,
                irsData.ToList(),
                this.ShipConnectedTo.Map.Game);
        }

        private IEnumerable<string> RunFullIRSScan(Location shipLocation, int gridSize, string title)
        {
            //todo: if inefficiency ever becomes a problem this this could be split out into just getting names
            IEnumerable<IScanResult> irsData = shipLocation.Sector.GetIRSFullData(shipLocation, this.ShipConnectedTo.Map.Game, gridSize);
            IEnumerable<string> renderedData = this.ShipConnectedTo.Map.Game.Interact.RenderScanWithNames(ScanRenderType.DoubleSingleLine, title, irsData.ToList(), this.ShipConnectedTo.Map.Game);

            return renderedData;
        }

        ///// <summary>
        ///// Used by CRS
        ///// </summary>
        ///// <param name="shipLocation"></param>
        ///// <returns></returns>
        //public List<string> RunIRSScan(Location shipLocation)
        //{
        //    var tlrsResults = shipLocation.Sector.GetIRSFullData(shipLocation, this.Game);
        //    var renderedData = this.Game.Write.RenderIRSData(tlrsResults, this.Game);

        //    return renderedData.ToList();
        //}

        /// <summary>
        /// SectorToScan
        /// </summary>
        /// <param name="locationToScan"></param>
        /// <returns></returns>
        public IRSResult Scan(Location locationToScan)
        {
            if (this.Damaged()) return null;

            //todo: refactor this with region.GetIRSFullData() inner loop

            bool outOfBounds;

            if (locationToScan.Sector != null)
            {
                outOfBounds = this.ShipConnectedTo.Map.OutOfBounds(locationToScan.Sector);
            }
            else
            {
                throw new NotImplementedException("This should not happen");
            }

            ////todo: breaks here when regionX or regionY is 8

            //todo: perform scan on location passed

            var shipSector = this.ShipConnectedTo.GetSector();

            //locationToScan.Sector is divined to the new one when crossing barrier
            IRSResult divinedResult = shipSector.GetSectorInfo(locationToScan.Sector, locationToScan.Coordinate, outOfBounds, this.ShipConnectedTo.Map.Game);

            return divinedResult;
        }

        public IRSResult Execute(Location locationToScan)
        {
            if (locationToScan?.Sector == null || locationToScan.Coordinate == null)
            {
                return new IRSResult
                {
                    GalacticBarrier = locationToScan?.Sector?.Type == SectorType.GalacticBarrier,
                    SectorName = locationToScan?.Sector?.Name,
                    Point = locationToScan?.Sector == null ? null : new Point(locationToScan.Sector.X, locationToScan.Sector.Y),
                    Unknown = locationToScan?.Sector == null
                };
            }

            var sectorResult = new IRSResult
            {
                Point = locationToScan.Coordinate.GetPoint(),
                Item = locationToScan.Coordinate.Item,
                Object = locationToScan.Coordinate.Object,
                SectorName = locationToScan.Sector.Name
            };

            //todo: support sector level nebulae
            //if (sectorToScan.Type != SectorType.Nebulae)
            //{
            //todo: these 2 concepts need to be combined
            //}

            locationToScan.Coordinate.Scanned = true;

            return sectorResult;
        }

        public new static ImmediateRangeScan For(IShip ship)
        {
            return (ImmediateRangeScan)SubSystem_Base.For(ship, SubsystemType.ImmediateRangeScan);
        }
    }
}
