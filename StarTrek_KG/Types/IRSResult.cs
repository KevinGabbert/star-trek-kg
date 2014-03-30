using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Types
{
    public class IRSResult: IScanResult
    {
        public Coordinate Coordinate { get; set; }
        public bool MyLocation { get; set; }
        public bool GalacticBarrier { get; set; }
        public bool Unknown { get; set; }
        public string Name { get; set; }
    }
}
