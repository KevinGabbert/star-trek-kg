using System;
using System.Linq;
using System.Text;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    //todo: this functionality is currently broken
    //todo: fix hostiles and starbases and stars to test fully
    public class LongRangeScan : SubSystem_Base, IMap
    {
        public LongRangeScan(Map map, Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
            this.Map = map;
            this.Type = SubsystemType.LongRangeScan;
        }

        public override void OutputDamagedMessage()
        {
            Output.Write.Resource("LRSDamaged");
            Output.Write.Resource("RepairsUnderway");
        }
        public override void OutputRepairedMessage()
        {
            Output.Write.Line("Long range scanner has been repaired.");
        }
        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public override void Controls(string command)
        {
            throw new NotImplementedException();
        }

        public void Controls()
        {
            if (Damaged()) return;

            var myLocation = this.ShipConnectedTo.GetLocation();

            Output.Write.SingleLine("-------------------");

            for (var quadrantY = myLocation.Quadrant.Y - 1; quadrantY <= myLocation.Quadrant.Y + 1; quadrantY++)
            {
                for (var quadrantX = myLocation.Quadrant.X - 1; quadrantX <= myLocation.Quadrant.X + 1; quadrantX++)
                {
                    Output.Write.WithNoEndCR(Constants.SCAN_SECTOR_DIVIDER + " ");

                    var renderingMyLocation = myLocation.Quadrant.X == quadrantX && myLocation.Quadrant.Y == quadrantY;

                    //todo: turn these into props.
                    int starbaseCount = -1;
                    int starCount = -1;
                    int hostileCount = -1;

                    if (!OutOfBounds(quadrantY, quadrantX))
                    {
                        Coordinate quadrantToScan = SetQuadrantToScan(quadrantY, quadrantX, myLocation);
                        LongRangeScan.GetMapInfoForScanner(this.Map, quadrantToScan, out hostileCount, out starbaseCount, out starCount);
                    }

                    if (renderingMyLocation)
                    {
                        Output.Write.HighlightTextBW(true);
                    }

                    Output.Write.WithNoEndCR(hostileCount.FormatForLRS());
                    Output.Write.WithNoEndCR(starbaseCount.FormatForLRS());
                    Output.Write.WithNoEndCR(starCount.FormatForLRS());
                    
                    if (renderingMyLocation)
                    {
                        Output.Write.HighlightTextBW(false);
                    }

                    Output.Write.WithNoEndCR(" ");
                }

                Output.Write.SingleLine(Constants.SCAN_SECTOR_DIVIDER);
                Output.Write.SingleLine("-------------------");
            }
        }

        private static bool OutOfBounds(int quadrantY, int quadrantX)
        {
            return quadrantX < 0 || quadrantY < 0 || quadrantX == Constants.QUADRANT_MAX || quadrantY == Constants.QUADRANT_MAX;
        }

        //todo: fix this
        private static Coordinate SetQuadrantToScan(int quadrantY, int quadrantX, Location myLocation)
        {
            var max = StarTrekKGSettings.GetSetting<int>("QuadrantMax") - 1;
            var min = StarTrekKGSettings.GetSetting<int>("QUADRANT_MIN");

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

        public static int GetMapInfoForScanner(Map map, Coordinate quadrant,
                                                        out int hostileCount,
                                                        out int starbaseCount,
                                                        out int starCount)
        {
            hostileCount = 0;
            starbaseCount = 0;
            starCount = 0;

            if (quadrant.Y >= 0 &&
                quadrant.X >= 0 &&
                quadrant.Y < Constants.QUADRANT_MAX &&
                quadrant.X < Constants.QUADRANT_MAX)
            {
                Quadrant quadrantToScan = Quadrants.Get(map, quadrant);

                hostileCount = quadrantToScan.GetHostiles().Count;
                starbaseCount = quadrantToScan.GetStarbaseCount();
                starCount = quadrantToScan.GetStarCount();

                quadrantToScan.Scanned = true;
            }
            return hostileCount;
        }

        public void Debug_Scan_All_Quadrants(bool setScanned)
        {
            foreach (var quadrant in this.Map.Quadrants)
            {
                quadrant.Scanned = true; 
            }
        }

        public new static LongRangeScan For(Ship ship)
        {
            if (ship == null)
            {
                Output.Write.Line("Ship not set up (LongRangeScan)."); //todo: reflect the name and refactor this to ISubsystem
                return null;
            }

            return (LongRangeScan) ship.Subsystems.Single(s => s.Type == SubsystemType.LongRangeScan);
        }
    }
}
