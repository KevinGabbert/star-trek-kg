using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;

namespace UnitTests.TestObjects
{
    public class TestClass_Base
    {
        protected readonly Test_Setup _setup = new Test_Setup();

        public Game Game { get; set; }

        public TestClass_Base()
        {
            //todo: pass in config
            _setup.Game.Interact = new Interaction(new StarTrekKGSettings());

            _setup.TestMap = new Map(null, _setup.Game.Interact, _setup.Config, this.Game)
            {
                Write = _setup.Game.Interact
            };

            _setup.Game.Interact = _setup.Game.Interact;

            this.Game = _setup.Game;
        }
    }
}
