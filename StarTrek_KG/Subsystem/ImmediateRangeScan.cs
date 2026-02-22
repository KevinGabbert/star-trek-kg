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
            var renderedResults = this.RunFullIRSScan(myLocation);

            foreach (string line in renderedResults)
            {
                this.ShipConnectedTo.Map.Game.Interact.SingleLine(line);
            }

            this.ShipConnectedTo.OutputLine("");

            return this.ShipConnectedTo.OutputQueue();
        }

        private IEnumerable<string> RunFullIRSScan(Location shipLocation)
        {
            //todo: if inefficiency ever becomes a problem this this could be split out into just getting names
            IEnumerable<IScanResult> irsData = shipLocation.Sector.GetIRSFullData(shipLocation, this.ShipConnectedTo.Map.Game);
            IEnumerable<string> renderedData = this.ShipConnectedTo.Map.Game.Interact.RenderScanWithNames(ScanRenderType.DoubleSingleLine, "*** Immediate Range Scan ***", irsData.ToList(), this.ShipConnectedTo.Map.Game);

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
