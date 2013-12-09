using NUnit.Framework;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Subsystem;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class Subsystem_BaseTests
    {
        private Computer _testComputer;
        
        [SetUp]
        public void Setup()
        {
            _testComputer = new Computer(new Map(new GameConfig
                                   {
                                        Initialize = true,
                                        //GenerateMap = true
                                   })); 
        }
    }
}
