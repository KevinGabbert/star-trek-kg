using StarTrek_KG.Config;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;

namespace StarTrek_KG.Playfield
{
    public class Point : IPoint
    {
        #region Properties

        private bool OutOfBounds { get; set; }

        private int _x;
        public int X
        {
            get { return _x; }
            set
            {
               this.CheckForOutOfBounds(value);

                _x = value;
            }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set
            {
                _y = value;
            }
        }

        #endregion

        public Point()
        {
        }

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Point GetRandom()
        {
            return new Point(Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MAX),
                                  Utility.Utility.Random.Next(DEFAULTS.COORDINATE_MAX));
        }

        private void CheckForOutOfBounds(int value)
        {
            //todo: we should not be hitting this in the game. User needs to be told that they hit the galactic barrier
            //todo: this might need to be moved somewhere else.  It can't be mocked like this.
            var boundsHigh = new StarTrekKGSettings().GetSetting<int>("BoundsHigh");
            var boundsLow = new StarTrekKGSettings().GetSetting<int>("BoundsLow");

            if ((value > boundsHigh) || 
                 value < boundsLow) 
            {
                this.OutOfBounds = true;
            }
        }

        public override string ToString()
        {
            return "Point: " + this.X + ", " + this.Y;
        }

        public void Update(Location newLocation)
        {
            this.X = newLocation.Coordinate.X;
            this.Y = newLocation.Coordinate.Y;
        }

        public Sector ToSector()
        {
            var newSector = new Sector(this);
            return newSector;
        }
    }
}
