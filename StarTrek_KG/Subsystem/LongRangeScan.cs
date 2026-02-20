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
            //todo: if inefficiency ever becomes a problem this this could be split out into just getting names
            IEnumerable<IScanResult> lrsData = shipLocation.Sector.GetLRSFullData(shipLocation, this.ShipConnectedTo.Map.Game);
            IEnumerable<string> renderedData = this.ShipConnectedTo.Map.Game.Interact.RenderScanWithNames(ScanRenderType.SingleLine, "*** Long Range Scan ***", lrsData.ToList(), this.ShipConnectedTo.Map.Game);

            return renderedData;
        }

        public LRSResult GetInfo(Map map, string regionName)
        {
            var regionResult = new LRSResult();

            Sector regionToScan = map.Sectors.Get(regionName);

            regionResult.Point = regionToScan.GetPoint();

            if (regionToScan.Type != SectorType.Nebulae)
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

        public static LRSResult Execute(Sector regionToScan)
        {
            var regionResult = new LRSResult
            {
                Point = regionToScan.GetPoint()
            };


            if (regionToScan.Type != SectorType.Nebulae)
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
            foreach (var Sector in this.ShipConnectedTo.Map.Sectors)
            {
                Sector.Scanned = setScanned;
            }
        }

        public new static LongRangeScan For(IShip ship)
        {
            return (LongRangeScan)SubSystem_Base.For(ship, SubsystemType.LongRangeScan);
        }
    }
}
