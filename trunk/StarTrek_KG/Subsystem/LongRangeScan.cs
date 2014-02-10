using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;
using StarTrek_KG.TypeSafeEnums;

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

        public override void Controls(string command)
        {
            throw new NotImplementedException();
        }

        public void Controls()
        {
            if (Damaged()) return;

            Location myLocation = this.ShipConnectedTo.GetLocation();
            bool currentlyInNebula = myLocation.Quadrant.Type == QuadrantType.Nebulae;

            if (!currentlyInNebula)
            {
                this.RunLRSScan(myLocation);
            }
            else
            {
                this.Game.Write.SingleLine("Long Range Scan inoperative while in Nebula.");
            }
        }

        private void RunLRSScan(Location myLocation)
        {
            this.Game.Write.SingleLine("-------------------");

            for (var quadrantY = myLocation.Quadrant.Y - 1; quadrantY <= myLocation.Quadrant.Y + 1; quadrantY++)
            {
                for (var quadrantX = myLocation.Quadrant.X - 1; quadrantX <= myLocation.Quadrant.X + 1; quadrantX++)
                {
                    this.Game.Write.WithNoEndCR(Constants.SCAN_SECTOR_DIVIDER + " ");

                    var renderingMyLocation = myLocation.Quadrant.X == quadrantX &&
                                              myLocation.Quadrant.Y == quadrantY;

                    //todo: turn these into props.

                    var outOfBounds = OutOfBounds(quadrantY, quadrantX);

                    if (!outOfBounds)
                    {
                        Quadrant quadrantToScan = Quadrants.Get(this.Game.Map,
                            this.CoordinateToScan(quadrantY, quadrantX));

                        if (quadrantToScan.Type != QuadrantType.Nebulae)
                        {
                            int starbaseCount = -1;
                            int starCount = -1;
                            int hostileCount = -1;

                            this.Execute(quadrantToScan, out hostileCount, out starbaseCount, out starCount);

                            this.Game.Write.RenderQuadrantCounts(renderingMyLocation, starbaseCount, starCount,
                                hostileCount);
                        }
                        else
                        {
                            this.Game.Write.RenderNebula(renderingMyLocation);
                        }
                    }
                    else
                    {
                        this.Game.Write.WithNoEndCR(this.Game.Config.GetSetting<string>("GalacticBorder"));
                    }

                    this.Game.Write.WithNoEndCR(" ");
                }

                this.Game.Write.SingleLine(Constants.SCAN_SECTOR_DIVIDER);
                this.Game.Write.SingleLine("-------------------");
            }
        }

        private bool OutOfBounds(int quadrantY, int quadrantX)
        {
            var inTheNegative = quadrantX < 0 || quadrantY < 0;
            var maxxed = quadrantX == Constants.QUADRANT_MAX || quadrantY == Constants.QUADRANT_MAX;

            var yOnMap = quadrantY >= 0 && quadrantY < Constants.QUADRANT_MAX;
            var xOnMap = quadrantX >= 0 && quadrantX < Constants.QUADRANT_MAX;

            return (inTheNegative || maxxed) && !(yOnMap && xOnMap);
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

        public void Debug_Scan_All_Quadrants(bool setScanned)
        {
            foreach (var quadrant in this.Game.Map.Quadrants)
            {
                quadrant.Scanned = setScanned;
            }
        }

        public static LongRangeScan For(Ship ship)
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
