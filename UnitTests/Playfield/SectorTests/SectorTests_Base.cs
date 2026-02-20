using NUnit.Framework;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;
using UnitTests.TestObjects;

namespace UnitTests.Playfield.SectorTests
{
    public class SectorTests_Base: TestClass_Base
    {
        protected Sector _testRegion;

        protected void SectorNewAsserts()
        {
            Assert.AreEqual(string.Empty, _testRegion.Name);
            
            Assert.AreEqual(false, _testRegion.Scanned);
            Assert.AreEqual(0, _testRegion.X);
            Assert.AreEqual(0, _testRegion.Y);
            Assert.AreEqual(true, _testRegion.Empty);
            Assert.Throws(Is.TypeOf<GameException>().And.Message.EqualTo("No Coordinates set up in Sector: "), () => _testRegion.GetHostiles());
        }
    }
}
