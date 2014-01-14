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

            Constants.QUADRANT_MIN = (new StarTrekKGSettings()).GetSetting<int>("QUADRANT_MIN");
            Constants.QUADRANT_MAX = (new StarTrekKGSettings()).GetSetting<int>("QuadrantMax");
        }
    }
}
