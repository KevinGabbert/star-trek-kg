using StarTrek_KG;
using StarTrek_KG.Config;

namespace UnitTests
{
    public static class TestRunner
    {
        public static void GetTestConstants()
        {
            Constants.SECTOR_MIN = (new StarTrekKGSettings()).GetSetting<int>("SECTOR_MIN");
            Constants.SECTOR_MAX = (new StarTrekKGSettings()).GetSetting<int>("SECTOR_MAX");

            Constants.Region_MIN = (new StarTrekKGSettings()).GetSetting<int>("Region_MIN");
            Constants.Region_MAX = (new StarTrekKGSettings()).GetSetting<int>("RegionMax");
        }
    }
}
