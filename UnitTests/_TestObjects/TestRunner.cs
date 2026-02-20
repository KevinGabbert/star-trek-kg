using StarTrek_KG.Config;
using StarTrek_KG.Settings;

namespace UnitTests.TestObjects
{
    public static class TestRunner
    {
        public static void GetTestConstants()
        {
            DEFAULTS.COORDINATE_MIN = new StarTrekKGSettings().GetSetting<int>("SECTOR_MIN");
            DEFAULTS.COORDINATE_MAX = new StarTrekKGSettings().GetSetting<int>("SECTOR_MAX");

            DEFAULTS.SECTOR_MIN = new StarTrekKGSettings().GetSetting<int>("SECTOR_MIN");
            DEFAULTS.SECTOR_MAX = new StarTrekKGSettings().GetSetting<int>("SECTOR_MAX");
        }
    }
}
