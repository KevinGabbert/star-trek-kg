using NUnit.Framework;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;
using UnitTests.TestObjects;

namespace UnitTests.Playfield.SectorTests
{
    public class SectorTests_Base: TestClass_Base
    {
        protected Sector _testSector;

        protected void SectorNewAsserts()
        {
            Assert.AreEqual(string.Empty, _testSector.Name);
            
            Assert.AreEqual(false, _testSector.Scanned);
            Assert.AreEqual(0, _testSector.X);
            Assert.AreEqual(0, _testSector.Y);
            Assert.AreEqual(true, _testSector.Empty);
            Assert.Throws(Is.TypeOf<GameException>().And.Message.EqualTo("No Coordinates set up in Sector: "), () => _testSector.GetHostiles());
        }
    }
}
