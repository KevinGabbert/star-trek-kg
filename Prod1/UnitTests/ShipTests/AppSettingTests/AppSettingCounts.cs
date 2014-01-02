using NUnit.Framework;
using StarTrek_KG.Config;

namespace UnitTests.ShipTests.AppSettingTests
{
    /// <summary>
    /// This fixture ensures we catch when a new setting is added during development.
    /// After development, heslps to know whon the default configuration might be messed up.
    /// </summary>
    [TestFixture]
    public class AppSettingCounts
    {
        [SetUp]
        public void Setup()
        {
            StarTrekKGSettings.Get = StarTrekKGSettings.GetConfig();
        }

        [Test]
        public void CountOfSettings()
        {
            Assert.AreEqual(61, StarTrekKGSettings.Get.GameSettings.Count);
        }

        [Test]
        public void CountOfSettingstarSystems()
        {

            Assert.AreEqual(80, StarTrekKGSettings.Get.StarSystems.Count);
        }

        [Test]
        public void CountOfFactions()
        {

            Assert.AreEqual(3, StarTrekKGSettings.Get.Factions.Count);
        }


        [Test]
        public void CountOfConsoleText()
        {

            Assert.AreEqual(64, StarTrekKGSettings.Get.ConsoleText.Count);
        }
    }
}
