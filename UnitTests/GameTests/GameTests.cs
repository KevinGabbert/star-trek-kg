
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Playfield;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class GameTests: TestClass_Base
    {
        [Test]
        public void NewGame()
        {
            //interesting.. this test causes MapWith3HostilesTheConfigWayAddedInDescendingOrder2() to fail when running after this one does

            var game = new Game((new StarTrekKGSettings()));
            
            Assert.IsInstanceOf<Map>(game.Map);

            (new StarTrekKGSettings()).Get = null;
        }

        [TearDown]
        public void TearDown()
        {
            
        }
    }
}
