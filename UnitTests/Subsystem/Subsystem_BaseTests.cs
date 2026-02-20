using Moq;
using NUnit.Framework;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;
using UnitTests.TestObjects;

namespace UnitTests.Subsystem
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

        [Test]
        public void TakeDamage()
        {
            DEFAULTS.DEBUG_MODE = false;
            var mockedShields = new Mock<ISubsystem>();

            int i = 0;
            int i1 = i;
            mockedShields.Setup(c => c.GetNext(It.IsAny<int>())).Returns(() => i1).Callback(() => i = -1);

            mockedShields.Object.TakeDamage();
            Assert.Greater(1, mockedShields.Object.Damage);
        }
    }
}
