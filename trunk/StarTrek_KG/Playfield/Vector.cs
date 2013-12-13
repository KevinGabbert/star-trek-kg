namespace StarTrek_KG.Playfield
{
    public class Vector
    {
        #region Properties

            public double X { get; set; }
            public double Y { get; set; }

        #endregion


        //todo: refactor this and coordinate
        public Vector()
        {
        }

        public Vector(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vector New(double y, double x)
        {
            var vector = new Vector(x, y);
            return vector;
        }
    }
}
