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
    //todo: this functionality is currently broken
    //todo: fix hostiles and starbases and stars to test fully
    public class LongRangeScan : SubSystem_Base
    {
        public LongRangeScan(Ship shipConnectedTo, Game game) : base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.LongRangeScan;
        }

        public override void Controls(string command)
        {
            throw new NotImplementedException();
        }

        public void Controls()
        {
            if (Damaged()) return;

            //todo: refactor this pattern with LRS

            Location myLocation = this.ShipConnectedTo.GetLocation();
            var renderedResults = this.RunFullLRSScan(myLocation);

            foreach (var line in renderedResults)
            {
                this.Game.Write.SingleLine(line);
            }
        }

        //used by CRS
        public List<string> RunLRSScan(Location shipLocation)
        {
            var tlrsResults = shipLocation.Region.GetLRSFullData(shipLocation, this.Game);
            var renderedData = this.Game.Write.RenderLRSData(tlrsResults, this.Game);

            return renderedData;
        }

        public IEnumerable<string> RunFullLRSScan(Location shipLocation)
        {
            //todo: if inefficiency ever becomes a problem this this could be split out into just getting names
            IEnumerable<IScanResult> lrsData = shipLocation.Region.GetLRSFullData(shipLocation, this.Game);
            IEnumerable<string> renderedData = this.Game.Write.RenderScanWithNames(ScanRenderType.SingleLine, "*** Long Range Scan ***", lrsData.ToList(), this.Game);

            return renderedData;
        }

        public LRSResult GetInfo(Map map, string regionName)
        {
            var regionResult = new LRSResult();

            Region regionToScan = map.Regions.Get(regionName);

            regionResult.Coordinate = regionToScan.GetCoordinate();

            if (regionToScan.Type != RegionType.Nebulae)
            {
                regionResult.Hostiles = regionToScan.GetHostiles().Count;
                regionResult.Starbases = regionToScan.GetStarbaseCount();
                regionResult.Stars = regionToScan.GetStarCount();
            }

            var barrierID = "Galactic Barrier"; //todo: resource this
            regionResult.Name = regionResult.GalacticBarrier ? barrierID : regionToScan.Name;

            regionToScan.Scanned = true;

            return regionResult;
        }

        public LRSResult Execute(Region regionToScan)
        {
            var regionResult = new LRSResult();

            regionResult.Coordinate = regionToScan.GetCoordinate();

            if (regionToScan.Type != RegionType.Nebulae)
            {
                regionResult.Hostiles = regionToScan.GetHostiles().Count;
                regionResult.Starbases = regionToScan.GetStarbaseCount();
                regionResult.Stars = regionToScan.GetStarCount();
                regionResult.Name = regionToScan.Name;
            }

            regionToScan.Scanned = true;

            return regionResult;
        }

        public void Debug_Scan_All_Regions(bool setScanned)
        {
            foreach (var Region in this.Game.Map.Regions)
            {
                Region.Scanned = setScanned;
            }
        }

        public static LongRangeScan For(IShip ship)
        {
            return (LongRangeScan)SubSystem_Base.For(ship, SubsystemType.LongRangeScan);
        }
    }
}
