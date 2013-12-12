using System;
using System.Linq;
using System.Text;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    //todo: this functionality is currently broken
    //todo: fix hostiles and starbases and stars to test fully
    public class LongRangeScan : SubSystem_Base, IMap
    {
        public LongRangeScan(Map map)
        {
            this.Map = map;
            this.Type = SubsystemType.LongRangeScan;
        }

        public override void OutputDamagedMessage()
        {
            Output.LongRangeScanDamageMessage();
        }
        public override void OutputRepairedMessage()
        {
            Output.WriteLine("Long range scanner has been repaired.");
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
            var myLocation = this.Map.Playership.GetLocation();

            Output.WriteSingleLine("-------------------");

            for (var quadrantY = myLocation.Quadrant.Y - 1; quadrantY <= myLocation.Quadrant.Y + 1; quadrantY++)
            {
                for (var quadrantX = myLocation.Quadrant.X - 1; quadrantX <= myLocation.Quadrant.X + 1; quadrantX++)
                {
                    sb.Append("| ");

                    //todo: turn these into props.
                    int starbaseCount;
                    int starCount;
                    int hostileCount;

                    LongRangeScan.GetMapInfoForScanner(map, quadrantX, quadrantY, out hostileCount, out starbaseCount, out starCount);

                    sb.Append(String.Format("{0}{1}{2} ", hostileCount, starbaseCount, starCount));
                }

                sb.Append("|");

                Output.WriteLine(sb.ToString());
                sb.Length = 0;
                Output.WriteSingleLine("-------------------");
            }
        }

        public static int GetMapInfoForScanner(Map map, int quadrantX, int quadrantY,
                                                        out int hostileCount,
                                                        out int starbaseCount,
                                                        out int starCount)
        {
            hostileCount = 0;
            starbaseCount = 0;
            starCount = 0;

            if (quadrantY >= 0 &&
                quadrantX >= 0 &&
                quadrantY < Constants.QUADRANT_MAX &&
                quadrantX < Constants.QUADRANT_MAX)
            {
                Quadrant quadrant = Quadrants.Get(map, quadrantX, quadrantY);
                quadrant.Scanned = true;

                hostileCount = quadrant.GetHostiles().Count;
                starbaseCount = quadrant.GetStarbaseCount();
                starCount = quadrant.GetStarCount();
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
