using NUnit.Framework;
using StarTrek_KG;

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
            Constants.SECTOR_MIN = 0;
            Constants.SECTOR_MAX = 0;

            Constants.Region_MIN = 0;
            Constants.Region_MAX = 0;
        }


        [Test]
        public void NewStarbase()
        {
            _setup.SetupMapWithStarbase();

            Assert.IsNotNull(_setup.TestMap.Regions[0].Sectors);
        }
    }
}
