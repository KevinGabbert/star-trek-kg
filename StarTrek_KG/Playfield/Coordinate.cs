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
                CheckForOutOfBounds(value);
                _x = value;
            }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set
            {
                CheckForOutOfBounds(value);
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
            if ((value > 7) || value < 0) //todo: pull this item from app.config
            {
                throw new GameConfigException("Out of bounds");
            }
        }
    }
}
