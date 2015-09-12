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
            _setup.Game.Interact = new Interaction(null);
            _setup.TestMap = new Map(null, _setup.Game.Interact, _setup.Config);
            _setup.TestMap.Write = new Interaction(_setup.TestMap.Config);
            _setup.Game.Interact = new Interaction(_setup.TestMap.Config);

            this.Game = _setup.Game;
        }
    }
}
