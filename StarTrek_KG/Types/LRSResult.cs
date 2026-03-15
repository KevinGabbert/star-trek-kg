using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Types
{
    public class LRSResult : IScanResult
    {
        public string SectorName
        {
            get { return Name; }
            set { this.Name = value; }
        }

        public Point Point { get; set; }
        public bool MyLocation { get; set; }
        public bool GalacticBarrier { get; set; }
        public bool Unknown { get; set; }
        public string Name { get; set; }

        public int? Hostiles { get; set; }
        public int? Starbases { get; set; }
        public int? Stars { get; set; }
        public bool HasDeuterium { get; set; }
        public LrsFeatureMask FeatureMask { get; set; }
        public string QuadrantName { get; set; }
        public bool SuppressUnknownNoise { get; set; }

        public override string ToString()
        {
            return $"{this.Hostiles.GetValueOrDefault(0)}{this.Starbases.GetValueOrDefault(0)}{this.Stars.GetValueOrDefault(0)}";
        }

        public string ToScanString()
        {
            const string bulletSeparator = " \u2022 ";
            return $"{this.Hostiles.GetValueOrDefault(0)}{bulletSeparator}{this.Starbases.GetValueOrDefault(0)}{bulletSeparator}{this.Stars.GetValueOrDefault(0)}";
        }
    }
}
