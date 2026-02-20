using NUnit.Framework;
using StarTrek_KG.Utility;

namespace UnitTests.UtilityTests
{
    [TestFixture]
    public class UtilityTests
    {
        //todo: test star assignment method

        [Test]
        public void TestSRSNebulaGeneration()
        {

            var x = "";

            for (int i = 0; i < 10; i++)
            {
                x += Utility.DamagedScannerUnit();
            }
        }
    }
}
