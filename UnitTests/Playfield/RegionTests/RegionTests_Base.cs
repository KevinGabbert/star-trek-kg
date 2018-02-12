using NUnit.Framework;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;
using UnitTests.TestObjects;

namespace UnitTests.Playfield.RegionTests
{
    public class RegionTests_Base: TestClass_Base
    {
        protected Region _testRegion;

        protected void RegionNewAsserts()
        {
            Assert.AreEqual(string.Empty, _testRegion.Name);
            
            Assert.AreEqual(false, _testRegion.Scanned);
            Assert.AreEqual(0, _testRegion.X);
            Assert.AreEqual(0, _testRegion.Y);
            Assert.AreEqual(true, _testRegion.Empty);
            Assert.Throws(Is.TypeOf<GameException>().And.Message.EqualTo("No Sectors Set up in Region: "), () => _testRegion.GetHostiles());
        }
    }
}
