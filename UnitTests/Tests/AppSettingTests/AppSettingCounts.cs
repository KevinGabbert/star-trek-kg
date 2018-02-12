using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Tests.AppSettingTests
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

        [Test]
        public void CountOfSettings()
        {
            Assert.AreEqual(85, config.Get.GameSettings.Count);
        }

        [Test]
        public void CountOfSettingstarSystems()
        {

            Assert.AreEqual(91, config.Get.StarSystems.Count);
        }

        [Test]
        public void CountOfFactions()
        {

            Assert.AreEqual(5, config.Get.Factions.Count);
        }

        [Test]
        public void CountOfFactionShips()
        {
            Assert.AreEqual(961, config.Get.Factions[FactionName.Federation.ToString()].FactionShips.Count);
        }

        [Test]
        public void CountOfFactionThreats()
        {
            Assert.AreEqual(7, config.Get.Factions[FactionName.Federation.ToString()].FactionThreats.Count);
        }

        [Test]
        public void CountOfConsoleText()
        {
            Assert.AreEqual(217, config.Get.ConsoleText.Count);
        }
    }
}
