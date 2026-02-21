using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Output;
using StarTrek_KG.TypeSafeEnums;

namespace UnitTests.Output
{
    [TestFixture]
    public class PromptTests
    {
        [Test]
        public void PromptInfo_SetsDefaultPrompt()
        {
            var prompt = new PromptInfo(">");

            Assert.AreEqual(">", prompt.DefaultPrompt);
        }

        [Test]
        public void PromptInfo_MultiStepChain_AssignsAndCanRead()
        {
            var prompt = new PromptInfo(">");
            var chain = new List<string> { "a", "b", "c" };

            prompt.MultiStepCommandChain = chain;

            Assert.AreEqual(chain, prompt.MultiStepCommandChain);
        }

        [Test]
        public void PromptInfo_SubCommand_CanBeSet()
        {
            var prompt = new PromptInfo(">");

            prompt.SubCommand = "wrp";

            Assert.AreEqual("wrp", prompt.SubCommand);
        }

        [Test]
        public void PromptInfo_TracksSubsystemAndLevel()
        {
            var prompt = new PromptInfo(">");

            prompt.SubSystem = SubsystemType.Navigation;
            prompt.Level = 2;

            Assert.AreEqual(SubsystemType.Navigation, prompt.SubSystem);
            Assert.AreEqual(2, prompt.Level);
        }
    }
}
