using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
    //todo: this functionality is currently broken
    //todo: fix hostiles and starbases and stars to test fully
    public class LongRangeScan : SubSystem_Base
    {
        public LongRangeScan(Ship shipConnectedTo, Game game)
        {
            this.Game = game;

            this.Initialize();

            this.ShipConnectedTo = shipConnectedTo;
            this.Type = SubsystemType.LongRangeScan;
        }

        public override void OutputDamagedMessage()
        {
            this.Game.Write.Resource("LRSDamaged");
            this.Game.Write.Resource("RepairsUnderway");
        }
        public override void OutputRepairedMessage()
        {
            this.Game.Write.Line("Long range scanner has been repaired.");
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

            this.Game.Write.SingleLine("-------------------");

            for (var quadrantY = myLocation.Quadrant.Y - 1; quadrantY <= myLocation.Quadrant.Y + 1; quadrantY++)
            {
                for (var quadrantX = myLocation.Quadrant.X - 1; quadrantX <= myLocation.Quadrant.X + 1; quadrantX++)
                {
                    this.Game.Write.WithNoEndCR(Constants.SCAN_SECTOR_DIVIDER + " ");

                    var renderingMyLocation = myLocation.Quadrant.X == quadrantX && myLocation.Quadrant.Y == quadrantY;

                    //todo: turn these into props.
                    int starbaseCount = -1;
                    int starCount = -1;
                    int hostileCount = -1;

                    if (!OutOfBounds(quadrantY, quadrantX))
                    {
                        Coordinate quadrantToScan = SetQuadrantToScan(quadrantY, quadrantX, myLocation);
                        this.GetMapInfoForScanner(this.Game.Map, quadrantToScan, out hostileCount, out starbaseCount, out starCount);
                    }

                    this.Game.Write.RenderQuadrantCounts(renderingMyLocation, starbaseCount, starCount, hostileCount);
                    this.Game.Write.WithNoEndCR(" ");
                }

                this.Game.Write.SingleLine(Constants.SCAN_SECTOR_DIVIDER);
                this.Game.Write.SingleLine("-------------------");
            }
        }

        private bool OutOfBounds(int quadrantY, int quadrantX)
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

        public int GetMapInfoForScanner(Map map, Coordinate quadrant,
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
            foreach (var quadrant in this.Game.Map.Quadrants)
            {
                quadrant.Scanned = true; 
            }
        }

        public new static LongRangeScan For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (LongRangeScan)."); //todo: reflect the name and refactor this to ISubsystem
                return null;
            }

            return (LongRangeScan) ship.Subsystems.Single(s => s.Type == SubsystemType.LongRangeScan);
        }
    }
}
