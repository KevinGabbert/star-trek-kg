using StarTrek_KG;
using StarTrek_KG.Output;

namespace UnitTests.ShipTests.Test_Harness_Objects
{
    public class TestClass_Base
    {
        protected readonly Test_Setup _setup = new Test_Setup();

        public Command Command { get; set; }
        public Write Write { get; set; }
        public Draw Draw { get; set; }

        public TestClass_Base()
        {
            _setup.Draw = new Draw(_setup.Write);
            _setup.Command = new Command(_setup.TestMap, _setup.Write, _setup.Draw);
            _setup.Write = new Write(_setup.Command);

            this.Command = _setup.Command;
            this.Draw = _setup.Draw;
            this.Write = _setup.Write;
        }
    }
}
