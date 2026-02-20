using NUnit.Framework;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Actors.StarbaseTests
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
            DEFAULTS.COORDINATE_MIN = 0;
            DEFAULTS.COORDINATE_MAX = 0;

            DEFAULTS.SECTOR_MIN = 0;
            DEFAULTS.SECTOR_MAX = 0;
        }


        [Test]
        public void NewStarbase()
        {
            _setup.SetupMapWithStarbase();

            Assert.IsNotNull(_setup.TestMap.Sectors[0].Coordinates);
        }
    }
}
