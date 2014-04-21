using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    public class ImmediateRangeScan : SubSystem_Base
    {
        public ImmediateRangeScan(Ship shipConnectedTo, Game game): base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.ImmediateRangeScan;
        }

        public void Controls()
        {
            if (this.Damaged()) return;

            //todo: refactor this pattern with LRS

            Location myLocation = this.ShipConnectedTo.GetLocation();
            var renderedResults = this.RunFullIRSScan(myLocation);

            foreach (var line in renderedResults)
            {
                this.Game.Write.SingleLine(line);
            }
        }

        public IEnumerable<string> RunFullIRSScan(Location shipLocation)
        {
            //todo: if inefficiency ever becomes a problem this this could be split out into just getting names
            IEnumerable<IScanResult> irsData = shipLocation.Region.GetIRSFullData(shipLocation, this.Game);
            IEnumerable<string> renderedData = this.Game.Write.RenderScanWithNames(ScanRenderType.DoubleSingleLine, "*** Immediate Range Scan ***", irsData.ToList(), this.Game);

            return renderedData;
        }

        ///// <summary>
        ///// Used by CRS
        ///// </summary>
        ///// <param name="shipLocation"></param>
        ///// <returns></returns>
        //public List<string> RunIRSScan(Location shipLocation)
        //{
        //    var tlrsResults = shipLocation.Region.GetIRSFullData(shipLocation, this.Game);
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

            var outOfBounds = this.Game.Map.OutOfBounds(locationToScan.Region);

            ////todo: breaks here when regionX or regionY is 8

            //todo: perform scan on location passed

            var shipRegion = this.ShipConnectedTo.GetRegion();

            //locationToScan.Region is divined to the new one when crossing barrier
            IRSResult divinedResult = shipRegion.GetSectorInfo(locationToScan.Region, locationToScan.Sector, outOfBounds, this.Game);

            return divinedResult;
        }

        public IRSResult Execute(Location locationToScan)
        {
            var sectorResult = new IRSResult();

            sectorResult.Coordinate = locationToScan.Sector.GetCoordinate();

            //todo: support sector level nebulae
            //if (sectorToScan.Type != RegionType.Nebulae)
            //{
                //todo: these 2 concepts need to be combined
                sectorResult.Item = locationToScan.Sector.Item;
                sectorResult.Object = locationToScan.Sector.Object;
                sectorResult.RegionName = locationToScan.Region.Name;
            //}

            locationToScan.Sector.Scanned = true;

            return sectorResult;
        }

        public static ImmediateRangeScan For(IShip ship)
        {
            return (ImmediateRangeScan)SubSystem_Base.For(ship, SubsystemType.ImmediateRangeScan);
        }
    }
}
