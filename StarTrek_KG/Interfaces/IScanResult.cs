using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface IScanResult
    {
        Point Point { get; set; }
        bool MyLocation { get; set; }

        bool GalacticBarrier { get; set; }
        bool Unknown { get; set; }
        //string Name { get; set; }
        string RegionName { get; set; }

        string ToScanString();
    }
}
