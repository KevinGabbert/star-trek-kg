using StarTrek_KG.Config;
using StarTrek_KG.Settings;

namespace UnitTests
{
    public static class TestRunner
    {
        public static void GetTestConstants()
        {
            DEFAULTS.SECTOR_MIN = new StarTrekKGSettings().GetSetting<int>("SECTOR_MIN");
            DEFAULTS.SECTOR_MAX = new StarTrekKGSettings().GetSetting<int>("SECTOR_MAX");

            DEFAULTS.REGION_MIN = new StarTrekKGSettings().GetSetting<int>("Region_MIN");
            DEFAULTS.REGION_MAX = new StarTrekKGSettings().GetSetting<int>("RegionMax");
        }
    }
}
