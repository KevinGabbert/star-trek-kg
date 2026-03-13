using System;

namespace StarTrek_KG.Types
{
    [Flags]
    public enum LrsFeatureMask
    {
        None = 0,
        Starbase = 1,
        Wormhole = 2,
        Deuterium = 4,
        TemporalRift = 8,
        TechnologyCache = 16,
        Hazard = 32
    }
}
