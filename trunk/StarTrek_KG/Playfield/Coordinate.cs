﻿using StarTrek_KG.Config;
using StarTrek_KG.Exceptions;

namespace StarTrek_KG.Playfield
{
    public class Coordinate
    {
        protected bool _enforceBoundsChecking = true;

        #region Properties

        private int _x;
        public int X
        {
            get { return _x; }
            set
            {
                if (this._enforceBoundsChecking)
                {
                    Coordinate.CheckForOutOfBounds(value);
                }
                _x = value;
            }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set
            {
                if (this._enforceBoundsChecking)
                {
                    Coordinate.CheckForOutOfBounds(value);
                }
                _y = value;
            }
        }

        #endregion

        public Coordinate()
        {
        }

        public Coordinate(int x, int y, bool enforceBoundsChecking = true)
        {
            _enforceBoundsChecking = enforceBoundsChecking;
            this.X = x;
            this.Y = y;
        }

        public static Coordinate New(int y, int x)
        {
            var coordinate = new Coordinate(x, y);
            return coordinate;
        }

        public static Coordinate GetRandom()
        {
            return new Coordinate((Utility.Utility.Random).Next(Constants.SECTOR_MAX),
                                  (Utility.Utility.Random).Next(Constants.SECTOR_MAX));
        }

        private static void CheckForOutOfBounds(int value)
        {
            //todo: we should not be hitting this in the game. User needs to be told that they hit the galactic barrier

            var boundsHigh = (new StarTrekKGSettings()).GetSetting<int>("BoundsHigh");
            var boundsLow = (new StarTrekKGSettings()).GetSetting<int>("BoundsLow");

            if ((value > boundsHigh) || 
                 value < boundsLow) 
            {
                throw new GameConfigException("Out of bounds");
            }
        }

        public override string ToString()
        {
            return "Coordinate: " + this.X + ", " + this.Y;
        }

        internal void Update(Location newLocation)
        {
            this.X = newLocation.Sector.X;
            this.Y = newLocation.Sector.Y;
        }
    }
}
