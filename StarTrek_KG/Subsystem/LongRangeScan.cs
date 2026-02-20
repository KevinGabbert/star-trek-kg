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

        public LRSResult GetInfo(Map map, string sectorName)
        {
            var sectorResult = new LRSResult();

            Sector sectorToScan = map.Sectors.Get(sectorName);

            sectorResult.Point = sectorToScan.GetPoint();
            sectorResult.Hostiles = 0;
            sectorResult.Starbases = 0;
            sectorResult.Stars = 0;

            if (sectorToScan.Type != SectorType.Nebulae)
            {
                sectorResult.Hostiles = sectorToScan.GetHostiles().Count;
                sectorResult.Starbases = sectorToScan.GetStarbaseCount();
                sectorResult.Stars = sectorToScan.GetStarCount();
            }

            var barrierID = "Galactic Barrier"; //todo: resource this
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

            sectorResult.Hostiles = 0;
            sectorResult.Starbases = 0;
            sectorResult.Stars = 0;

            if (sectorToScan.Type != SectorType.Nebulae)
            {
                sectorResult.Hostiles = sectorToScan.GetHostiles().Count;
                sectorResult.Starbases = sectorToScan.GetStarbaseCount();
                sectorResult.Stars = sectorToScan.GetStarCount();
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
    }
}
