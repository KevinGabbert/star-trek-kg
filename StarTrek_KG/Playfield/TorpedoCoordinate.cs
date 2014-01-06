using System;

namespace StarTrek_KG.Playfield
{
    public class WeaponCoordinate
    {
       #region Properties

        private double _x;
        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
            }
        }

        private double _y;

        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
            }
        }

        #endregion

        public WeaponCoordinate()
        {
        }

        public WeaponCoordinate(Coordinate xy)
        {
            this.X = xy.X;
            this.Y = xy.Y;
        }

        public WeaponCoordinate(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return "Coordinate: " + this.X + ", " + this.Y;
        }

        internal void IncrementBy(WeaponCoordinate vector)
        {
            this.X += vector.X;
            this.Y += vector.Y;
        }
    }
}
