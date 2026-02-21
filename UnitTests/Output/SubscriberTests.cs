using System;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Output;

namespace UnitTests.Output
{
    [TestFixture]
    public class SubscriberTests
    {
        [Test]
        public void Ctor_InitializesPromptInfo()
        {
            var config = new StarTrekKGSettings();

            var subscriber = new Subscriber(config);

            Assert.IsNotNull(subscriber.PromptInfo);
            Assert.AreEqual(config.GetText("defaultPrompt"), subscriber.PromptInfo.DefaultPrompt);
        }

        [Test]
        public void Ctor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Subscriber(null));
        }
    }
}
