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

            Constants.QUADRANT_MIN = 0;
            Constants.QUADRANT_MAX = 0;
        }


        [Test]
        public void NewStarbase()
        {
            _setup.SetupMapWithStarbase();

            Assert.IsNotNull(_setup.TestMap.Quadrants[0].Sectors);
        }
    }
}
