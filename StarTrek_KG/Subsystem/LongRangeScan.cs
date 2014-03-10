using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions.System;
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
            IEnumerable<LRSResult> lrsData = shipLocation.Region.GetLRSFullData(shipLocation, this.Game);

            var renderedData = this.Game.Write.RenderLRSWithNames(lrsData.ToList(), this.Game);

            return renderedData;
        }

        public LRSResult Execute(Region RegionToScan)
        {
            var RegionResult = new LRSResult();

            if (RegionToScan.Type != RegionType.Nebulae)
            {
                RegionResult.Hostiles = RegionToScan.GetHostiles().Count;
                RegionResult.Starbases = RegionToScan.GetStarbaseCount();
                RegionResult.Stars = RegionToScan.GetStarCount();
                RegionResult.Name = RegionToScan.Name;
            }

            RegionToScan.Scanned = true;

            return RegionResult;
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
