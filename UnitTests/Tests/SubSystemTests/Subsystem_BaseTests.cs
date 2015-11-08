using NUnit.Framework;
using StarTrek_KG.Settings;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class Subsystem_BaseTests
    {
        private Test_Setup _setup = new Test_Setup();
        
        [SetUp]
        public void Setup()
        {

            DEFAULTS.DEBUG_MODE = false;
            
        }
    }
}
