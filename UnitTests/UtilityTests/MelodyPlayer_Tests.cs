using NUnit.Framework;
using StarTrek_KG.Utility;

namespace UnitTests.UtilityTests
{
    public class MelodyPlayer_Tests
    {
        [Test]
        public void test()
        {
            var m = new MelodyPlayer();

            m.Main();
        }
    }
}
