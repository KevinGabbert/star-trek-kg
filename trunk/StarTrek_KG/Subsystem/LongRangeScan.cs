using System;
using System.Collections.Generic;
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

            Location myLocation = this.ShipConnectedTo.GetLocation();

            //var lrsResults = this.RunLRSScan(myLocation);

            var testLRSResults = myLocation.Quadrant.GetLRSData(myLocation, this.Game);
            this.Game.Write.RenderLRSData(testLRSResults, this.Game);
        }

        //TODO: When done, this will replace existing LRS Scan
        public List<string> RunLRSScan(Location myLocation)
        {
            var lrsScanLines = new List<string>();
            bool currentlyInNebula = myLocation.Quadrant.Type == QuadrantType.Nebulae;

            lrsScanLines.Add("┌─────┬─────┬─────┐");

            var scanRow = 0;
            for (var quadrantY = myLocation.Quadrant.Y - 1; quadrantY <= myLocation.Quadrant.Y + 1; quadrantY++)
            {
                this.ScanRow(myLocation, quadrantY, lrsScanLines, scanRow, currentlyInNebula);
                scanRow++;
            }

            if (currentlyInNebula)
            {
                this.Game.Write.SingleLine("Long Range Scan inoperative while in Nebula.");
            }

            return lrsScanLines;
        }

        private void ScanRow(Location myLocation, int quadrantY, ICollection<string> lrsScanLines, int scanRow, bool currentlyInNebula)
        {
            string currentLRSScanLine = "";
            for (var quadrantX = myLocation.Quadrant.X - 1; quadrantX <= myLocation.Quadrant.X + 1; quadrantX++)
            {
                currentLRSScanLine += Constants.SCAN_SECTOR_DIVIDER + " ";

                //var renderingMyLocation = myLocation.Quadrant.X == quadrantX &&
                //                          myLocation.Quadrant.Y == quadrantY;

                //todo: turn these into props.
                var outOfBounds = this.Game.Map.OutOfBounds(quadrantY, quadrantX);

                if (!currentlyInNebula)
                {
                    currentLRSScanLine = this.GetQuadrantInfo(quadrantY, outOfBounds, currentLRSScanLine, quadrantX);
                }
                else
                {
                    currentLRSScanLine += Utility.Utility.NebulaUnit();
                }

                currentLRSScanLine += " ";
            }

            lrsScanLines.Add(currentLRSScanLine + Constants.SCAN_SECTOR_DIVIDER);

            if (scanRow == 0 || scanRow == 1)
            {
                lrsScanLines.Add("├─────┼─────┼─────┤");
            }
            else
            {
                lrsScanLines.Add("└─────┴─────┴─────┘");
            }
        }

        private string GetQuadrantInfo(int quadrantY, bool outOfBounds, string currentLRSScanLine, int quadrantX)
        {
            if (!outOfBounds)
            {
                currentLRSScanLine = GetQuadrantData(quadrantY, quadrantX, currentLRSScanLine);
            }
            else
            {
                currentLRSScanLine += this.Game.Config.GetSetting<string>("GalacticBarrier");
            }

            return currentLRSScanLine;
        }

        private string GetQuadrantData(int quadrantY, int quadrantX, string currentLRSScanLine)
        {
            Quadrant quadrantToScan = Quadrants.Get(this.Game.Map, this.CoordinateToScan(quadrantY, quadrantX));

            if (quadrantToScan.Type != QuadrantType.Nebulae)
            {
                int starbaseCount;
                int starCount;
                int hostileCount;

                this.Execute(quadrantToScan, out hostileCount, out starbaseCount, out starCount);

                //if (renderingMyLocation)
                //{
                //    currentLRSScanLine += "<HighlightStart />";
                //}

                currentLRSScanLine += this.Game.Write.RenderQuadrantCounts(starbaseCount, starCount, hostileCount);

                //if (renderingMyLocation)
                //{
                //    currentLRSScanLine += "<HighlightEnd />";
                //}
            }
            else
            {
                //todo: if renderingMyLocation, then highlight text
                //todo: pull out markup while rendering 
                //can change the backcolor

                //if (renderingMyLocation)
                //{
                //    currentLRSScanLine += "<HighlightStart />";
                //}

                currentLRSScanLine += Utility.Utility.NebulaUnit();

                //if (renderingMyLocation)
                //{
                //    currentLRSScanLine += "<HighlightEnd />";
                //}
            }
            return currentLRSScanLine;
        }

        //todo: fix this
        private Coordinate CoordinateToScan(int quadrantY, int quadrantX)
        {
            var max = this.Game.Config.GetSetting<int>("QuadrantMax") - 1;
            var min = this.Game.Config.GetSetting<int>("QUADRANT_MIN");

            int divinedQuadX = quadrantX;
            int divinedQuadY = quadrantY;

            if (quadrantX - 1 < min)
            {
                divinedQuadX = min;
            }

            if ((quadrantX > max))
            {
                divinedQuadX = max;
            }

            if (quadrantX + 1 > max)
            {
                divinedQuadX = max;
            }

            if (quadrantY - 1 < min)
            {
                divinedQuadY = min;
            }

            if ((quadrantY > max))
            {
                divinedQuadY = max;
            }

            var quadrantToScan = new Coordinate(divinedQuadX, divinedQuadY);

            return quadrantToScan;
        }

        public void Execute(Quadrant quadrantToScan,
                                    out int hostileCount,
                                    out int starbaseCount,
                                    out int starCount)
        {
            hostileCount = 0;
            starbaseCount = 0;
            starCount = 0;

            if (quadrantToScan.Type != QuadrantType.Nebulae)
            {
                hostileCount = quadrantToScan.GetHostiles().Count;
                starbaseCount = quadrantToScan.GetStarbaseCount();
                starCount = quadrantToScan.GetStarCount();
            }

            quadrantToScan.Scanned = true;  
        }

        public LRSResult Execute(Quadrant quadrantToScan)
        {
            var quadrantResult = new LRSResult();

            if (quadrantToScan.Type != QuadrantType.Nebulae)
            {
                quadrantResult.Hostiles = quadrantToScan.GetHostiles().Count;
                quadrantResult.Starbases = quadrantToScan.GetStarbaseCount();
                quadrantResult.Stars = quadrantToScan.GetStarCount();
            }

            quadrantToScan.Scanned = true;

            return quadrantResult;
        }

        public void Debug_Scan_All_Quadrants(bool setScanned)
        {
            foreach (var quadrant in this.Game.Map.Quadrants)
            {
                quadrant.Scanned = setScanned;
            }
        }

        public static LongRangeScan For(IShip ship)
        {
            return (LongRangeScan)SubSystem_Base.For(ship, SubsystemType.LongRangeScan);
        }
    }
}
