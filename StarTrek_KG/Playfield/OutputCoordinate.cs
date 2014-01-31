namespace StarTrek_KG.Playfield
{
    public class OutputCoordinate
    {
        #region Properties

        public string X { get; set; }

        public string Y { get; set; }

        #endregion

        public OutputCoordinate(string x, string y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
