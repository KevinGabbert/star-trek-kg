using StarTrek_KG;
using StarTrek_KG.Output;

namespace UnitTests.ShipTests.Test_Harness_Objects
{
    public class TestClass_Base
    {
        protected readonly Test_Setup _setup = new Test_Setup();

        public Game Game { get; set; }

        public TestClass_Base()
        {
            _setup.Game.Write = new Write(_setup.TestMap);
            this.Game = _setup.Game;
        }
    }
}
