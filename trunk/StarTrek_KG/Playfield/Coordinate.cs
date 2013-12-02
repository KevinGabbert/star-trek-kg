using StarTrek_KG.Config;
using StarTrek_KG.Exceptions;

namespace StarTrek_KG.Playfield
{
    public class Coordinate
    {
        #region Properties

        private int _x;
        public int X
        {
            get { return _x; }
            set
            {
                Coordinate.CheckForOutOfBounds(value);
                _x = value;
            }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set
            {
                Coordinate.CheckForOutOfBounds(value);
                _y = value;
            }
        }

        #endregion

        public Coordinate()
        {
        }

        public Coordinate(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Coordinate New(int y, int x)
        {
            var coordinate = new Coordinate(x, y);
            return coordinate;
        }

        private static void CheckForOutOfBounds(int value)
        {
            var boundsHigh = StarTrekKGSettings.GetSetting<int>("BoundsHigh");
            var boundsLow = StarTrekKGSettings.GetSetting<int>("BoundsLow");

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
    }
}
