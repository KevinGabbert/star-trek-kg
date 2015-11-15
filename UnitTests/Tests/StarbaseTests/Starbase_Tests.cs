using NUnit.Framework;
using StarTrek_KG.Settings;

namespace UnitTests.ShipTests.StarbaseTests
{
    public class Starbase_Tests : TestClass_Base
    {
        [SetUp]
        public void SetUp()
        {
            TestRunner.GetTestConstants();
        }

        [TearDown]
        public void TearDown()
        {
            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;

            DEFAULTS.REGION_MIN = 0;
            DEFAULTS.REGION_MAX = 0;
        }


        [Test]
        public void NewStarbase()
        {
            _setup.SetupMapWithStarbase();

            Assert.IsNotNull(_setup.TestMap.Regions[0].Sectors);
        }
    }
}
