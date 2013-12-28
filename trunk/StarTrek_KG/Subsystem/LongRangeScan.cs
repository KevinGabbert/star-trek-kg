using System;
using System.Linq;
using System.Text;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
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
            Output.Write.LongRangeScanDamageMessage();
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

        public void Controls(Map map)
        {
            if (Damaged()) return;

            var sb = new StringBuilder();
            var myLocation = this.ShipConnectedTo.GetLocation();

            Output.Write.SingleLine("-------------------");

            for (var quadrantY = myLocation.Quadrant.Y - 1; quadrantY <= myLocation.Quadrant.Y + 1; quadrantY++)
            {
                for (var quadrantX = myLocation.Quadrant.X - 1; quadrantX <= myLocation.Quadrant.X + 1; quadrantX++)
                {
                    sb.Append("| ");

                    //todo: turn these into props.
                    int starbaseCount = -1;
                    int starCount = -1;
                    int hostileCount = -1;

                    var quadrantToScan = SetQuadrantToScan(quadrantY, quadrantX, myLocation);

                    LongRangeScan.GetMapInfoForScanner(map, quadrantToScan, out hostileCount, out starbaseCount, out starCount);

                    sb.Append(String.Format("{0}{1}{2} ", hostileCount.FormatForLRS(), starbaseCount.FormatForLRS(), starCount.FormatForLRS()));
                }

                sb.Append("|");

                Output.Write.Line(sb.ToString());
                sb.Length = 0;
                Output.Write.SingleLine("-------------------");
            }
        }


        //todo: fix this
        private static Coordinate SetQuadrantToScan(int quadrantY, int quadrantX, Location myLocation)
        {
            Coordinate quadrantToScan;

            var boundsHigh = StarTrekKGSettings.GetSetting<int>("BoundsHigh");
            var boundsLow = StarTrekKGSettings.GetSetting<int>("BoundsLow");

            if ((myLocation.Quadrant.X > boundsHigh) || myLocation.Quadrant.X - 1 < boundsLow)
            {
                quadrantToScan = new Coordinate(quadrantX, quadrantY);
            }
            else
            {
                if (myLocation.Quadrant.X + 1 > boundsHigh)
                {
                    quadrantToScan = new Coordinate(quadrantX - 1, quadrantY);
                }
                else
                {
                    quadrantToScan = new Coordinate(quadrantX, quadrantY);
                }
            }
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

        public new static LongRangeScan For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (LongRangeScan)."); //todo: reflect the name and refactor this to ISubsystem
            }

            return (LongRangeScan) ship.Subsystems.Single(s => s.Type == SubsystemType.LongRangeScan);
        }
    }
}
