using StarTrek_KG.Output;

namespace UnitTests.ShipTests.Test_Harness_Objects
{
    public class TestClass_Base
    {
        protected readonly Test_Setup _setup = new Test_Setup();

        public Write Write { get; set; }

        public TestClass_Base()
        {
            _setup.Write = new Write(_setup.TestMap);
            this.Write = _setup.Write;
        }
    }
}
