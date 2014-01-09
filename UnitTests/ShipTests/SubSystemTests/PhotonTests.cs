using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Subsystem;
using UnitTests.ShipTests.Test_Harness_Objects;

namespace UnitTests.ShipTests.SubSystemTests
{
    [TestFixture]
    public class PhotonTests
    {
        private readonly Test_Setup setup = new Test_Setup();

        [Test]
        public void ALLHostilesAttack_ShipUndocked_WithShields()
        {
            //todo:  verify that quadrants are set up correctly.
            //todo: This test does not run alone.  what do the other tests set up that this needs?  why don't thea other tests tear down their stuff?

            //todo: will we need to mock out the Console.write process just so that we can test the output?  I'm thinking so..
            setup.SetupMapWith2Hostiles();

            Assert.AreEqual(setup.TestMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"), "Ship energy not at expected amount");

            //raise shields
            Shields.For(setup.TestMap.Playership).SetEnergy(2500); //hopefully a single hit wont be harder than this!
            Assert.AreEqual(2500, Shields.For(setup.TestMap.Playership).Energy, "Unexpected shield energy level"); //shields charged correctly // todo: do more tests on this in ShieldTests    

  
            Assert.AreEqual(setup.TestMap.Playership.Energy, StarTrekKGSettings.GetSetting<int>("energy"), "Ship energy not at expected amount");

            Game.ALLHostilesAttack(setup.TestMap);

            Assert.IsFalse(setup.TestMap.Playership.Destroyed);
            Assert.Less(Shields.For(setup.TestMap.Playership).Energy, 2500);
            Assert.AreEqual(StarTrekKGSettings.GetSetting<int>("energy"), setup.TestMap.Playership.Energy, "expected no change to ship energy");

            //Assert that ship has taken 2 hits.
            //todo: use a mock to determine that Ship.AbsorbHitFrom() was called twice.
        }
    }
}
