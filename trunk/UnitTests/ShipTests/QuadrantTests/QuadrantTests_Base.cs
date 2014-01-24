using NUnit.Framework;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;

namespace UnitTests.ShipTests.QuadrantTests
{
    public class QuadrantTests_Base: TestClass_Base
    {
        protected Quadrant _testQuadrant;

        protected void QuadrantNewAsserts()
        {
            Assert.AreEqual(string.Empty, _testQuadrant.Name);
            
            Assert.AreEqual(false, _testQuadrant.Scanned);
            Assert.AreEqual(0, _testQuadrant.X);
            Assert.AreEqual(0, _testQuadrant.Y);
            Assert.AreEqual(true, _testQuadrant.Empty);
            Assert.Throws(Is.TypeOf<GameException>().And.Message.EqualTo("No Sectors Set up in Quadrant: "), () => _testQuadrant.GetHostiles());
        }
    }
}
