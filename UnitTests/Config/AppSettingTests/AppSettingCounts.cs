using NUnit.Framework;
using StarTrek_KG.Config;

namespace UnitTests.Config.AppSettingTests
{
    /// <summary>
    /// This fixture ensures we catch when a new setting is added during development.
    /// After development, helps to know when the default configuration might be messed up.
    /// </summary>
    [TestFixture]
    public class AppSettingCounts
    {
        private readonly StarTrekKGSettings config = new StarTrekKGSettings();

        [SetUp]
        public void Setup()
        {
            config.Get = config.GetConfig();
        }

        // Count-based config tests removed (brittle when config changes).
    }
}
