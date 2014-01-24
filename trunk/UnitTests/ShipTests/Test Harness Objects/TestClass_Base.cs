using StarTrek_KG;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;

namespace UnitTests.ShipTests
{
    public class TestClass_Base
    {
        protected readonly Test_Setup _setup = new Test_Setup();

        public Game Game { get; set; }

        public TestClass_Base()
        {
            _setup.Game.Write = new Write(null);
            _setup.TestMap = new Map(null, _setup.Game.Write, _setup.Config);
            _setup.TestMap.Write = new Write(_setup.TestMap.Config);
            _setup.Game.Write = new Write(_setup.TestMap.Config);

            this.Game = _setup.Game;
        }
    }
}
