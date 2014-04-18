using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Types
{
    public class LRSResult: IScanResult
    {
        private int _hostiles = -1;
        private int _starbases = -1;
        private int _stars = -1;

        public Coordinate Coordinate { get; set; }
        public bool MyLocation { get; set; }
        public bool GalacticBarrier { get; set; }
        public bool Unknown { get; set; }
        public string Name { get; set; }

        public string RegionName
        {
            get { return Name; }
            set { this.Name = value; }
        }

        public int Hostiles
        {
            get { return _hostiles; }
            set { _hostiles = value; }
        }

        public int Starbases
        {
            get { return _starbases; }
            set { _starbases = value; }
        }

        public int Stars
        {
            get { return _stars; }
            set { _stars = value; }
        }

        public override string ToString()
        {
            string returnVal = null;

            returnVal = this.Hostiles.ToString() + this.Starbases.ToString() + this.Stars.ToString();

            return returnVal;
        }

        /// <summary>
        /// Used in LRS
        /// </summary>
        /// <returns></returns>
        public string ToScanString()
        {
            string returnVal = null;

            returnVal = this.Hostiles.ToString() + " ∙ " + this.Starbases.ToString() + " ∙ " + this.Stars.ToString();

            return returnVal;
        }
    }
}
