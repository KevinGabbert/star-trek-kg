using System.Linq;
using NUnit.Framework;
using UnitTests.TestObjects;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class HostileFireMessageTests : TestClass_Base
    {
        [SetUp]
        public void SetUp()
        {
            _setup.SetupMapWith1Hostile();
        }

        [Test]
        public void AllHostilesAttack_Outputs_Fire_Message()
        {
            this.Game.ALLHostilesAttack(_setup.TestMap);

            var output = _setup.TestMap.Playership.OutputQueue();
            Assert.True(output.Any(line => line.Contains("fires on you")));
        }
    }
}
