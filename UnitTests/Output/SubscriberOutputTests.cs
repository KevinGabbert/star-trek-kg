using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Config;
using StarTrek_KG.Output;

namespace UnitTests.Output
{
    [TestFixture]
    public class SubscriberOutputTests
    {
        [Test]
        public void Write_EnqueuesPreformattedLines()
        {
            var output = new SubscriberOutput(new StarTrekKGSettings());

            output.Write("line1");
            output.Write("line2");

            Assert.AreEqual(2, output.Queue.Count);
            Assert.IsTrue(output.Queue.First().Contains("line1"));
        }

        [Test]
        public void Write_List_EnqueuesAllLines()
        {
            var output = new SubscriberOutput(new StarTrekKGSettings());

            output.Write(new System.Collections.Generic.List<string> { "a", "b" });

            Assert.AreEqual(2, output.Queue.Count);
            Assert.IsTrue(output.Queue.Any(q => q.Contains("a")));
            Assert.IsTrue(output.Queue.Any(q => q.Contains("b")));
        }

        [Test]
        public void WriteLine_Empty_AddsBlankLine()
        {
            var output = new SubscriberOutput(new StarTrekKGSettings());

            output.WriteLine();

            Assert.AreEqual(1, output.Queue.Count);
        }
    }
}
