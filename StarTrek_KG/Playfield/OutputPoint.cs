namespace StarTrek_KG.Playfield
{
    public class OutputPoint
    {
        #region Properties

        public string X { get; set; }

        public string Y { get; set; }

        #endregion

        public OutputPoint(string x, string y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
