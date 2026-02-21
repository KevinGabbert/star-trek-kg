using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Output;

namespace UnitTests.Output
{
    [TestFixture]
    public class OutputTests
    {
        [Test]
        public void SubscriberOutput_Enqueue_WrapsInPreTags()
        {
            var output = new SubscriberOutput(new StarTrekKGSettings());

            output.Enqueue("hello");

            Assert.AreEqual(1, output.Queue.Count);
            Assert.IsTrue(output.Queue.Peek().Contains("<pre>hello</pre>"));
        }

        [Test]
        public void SubscriberOutput_WriteLine_AddsNewline()
        {
            var output = new SubscriberOutput(new StarTrekKGSettings());

            output.WriteLine("line");

            Assert.AreEqual(1, output.Queue.Count);
            Assert.IsTrue(output.Queue.Peek().Contains("line"));
        }

        [Test]
        public void SubscriberOutput_Write_List_HandlesNull()
        {
            var output = new SubscriberOutput(new StarTrekKGSettings());

            var result = output.Write((System.Collections.Generic.List<string>)null);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, output.Queue.Count);
        }

        [Test]
        public void SubscriberOutput_Clear_EmptiesQueue()
        {
            var output = new SubscriberOutput(new StarTrekKGSettings());
            output.Write("a");

            output.Clear();

            Assert.AreEqual(0, output.Queue.Count);
        }

    }
}
