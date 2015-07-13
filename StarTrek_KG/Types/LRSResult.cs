using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Types
{
    public class LRSResult: IScanResult
    {
        public string RegionName
        {
            get { return Name; }
            set { this.Name = value; }
        }

        public Coordinate Coordinate { get; set; }
        public bool MyLocation { get; set; }
        public bool GalacticBarrier { get; set; }
        public bool Unknown { get; set; }
        public string Name { get; set; }

        public int Hostiles { get; set; } = -1;
        public int Starbases { get; set; } = -1;
        public int Stars { get; set; } = -1;

        public override string ToString()
        {
            string returnVal = null;

            returnVal = $"{this.Hostiles}{this.Starbases}{this.Stars}";

            return returnVal;
        }

        /// <summary>
        /// Used in LRS
        /// </summary>
        /// <returns></returns>
        public string ToScanString()
        {
            string returnVal = null;

            returnVal = $"{this.Hostiles} ∙ {this.Starbases} ∙ {this.Stars}";

            return returnVal;
        }
    }
}
