using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StarTrek_KG.Output;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;
using UnitTests.TestObjects;

namespace UnitTests.Output
{
    [TestFixture]
    public class InteractTests : TestClass_Base
    {
        private Interaction _interact;

        [SetUp]
        public void SetUp()
        {
            _setup.SetupMapWith1Friendly();
            _interact = (Interaction)Game.Interact;
            _interact.Output.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            DEFAULTS.DEBUG_MODE = false;
        }

        [Test]
        public void ResetPrompt_ResetsPromptState()
        {
            _interact.CurrentPrompt = "custom";
            _interact.Subscriber.PromptInfo.SubCommand = "x";
            _interact.Subscriber.PromptInfo.SubSystem = SubsystemType.Shields;
            _interact.Subscriber.PromptInfo.Level = 2;

            _interact.ResetPrompt();

            Assert.AreEqual(_interact.Subscriber.PromptInfo.DefaultPrompt, _interact.CurrentPrompt);
            Assert.AreEqual(string.Empty, _interact.Subscriber.PromptInfo.SubCommand);
            Assert.AreEqual(SubsystemType.None, _interact.Subscriber.PromptInfo.SubSystem);
            Assert.AreEqual(0, _interact.Subscriber.PromptInfo.Level);
        }

        [Test]
        public void PromptUser_SetsPromptInfoAndQueuesMessage()
        {
            var queue = new Queue<string>();

            var result = _interact.PromptUser(
                SubsystemType.Shields,
                "PROMPT> ",
                "Line1\nLine2",
                out var value,
                queue,
                2);

            Assert.IsTrue(result);
            Assert.AreEqual("-1", value);
            Assert.AreEqual("PROMPT> ", _interact.CurrentPrompt);
            Assert.AreEqual(SubsystemType.Shields, _interact.Subscriber.PromptInfo.SubSystem);
            Assert.AreEqual(2, _interact.Subscriber.PromptInfo.Level);
            Assert.IsTrue(queue.Any(q => q.Contains("Line1")));
            Assert.IsTrue(queue.Any(q => q.Contains("Line2")));
        }

        [Test]
        public void CreateCommandPanel_WithoutDebugMode_DoesNotIncludeDebugCommand()
        {
            DEFAULTS.DEBUG_MODE = false;
            _interact.CreateCommandPanel();

            Assert.IsFalse(_interact.SHIP_PANEL.Any(line => line.StartsWith("dbg")));
            Assert.IsTrue(_interact.SHIP_PANEL.Any(line => line.StartsWith("imp")));
        }

        [Test]
        public void CreateCommandPanel_WithDebugMode_IncludesDebugCommand()
        {
            DEFAULTS.DEBUG_MODE = true;
            _interact.CreateCommandPanel();

            Assert.IsTrue(_interact.SHIP_PANEL.Any(line => line.StartsWith("dbg")));
        }

        [Test]
        public void Panel_WritesHeaderAndLines()
        {
            var output = _interact.Panel("HEADER", new[] { "one", "two" });

            Assert.IsTrue(output.Any(line => line.Contains("HEADER")));
            Assert.IsTrue(output.Any(line => line.Contains("one")));
            Assert.IsTrue(output.Any(line => line.Contains("two")));
        }

        [Test]
        public void ReadAndOutput_Help_PrintsHelpText()
        {
            var output = _interact.ReadAndOutput(Game.Map.Playership, "map", "help");

            Assert.IsTrue(output.Any(line => line.Contains("NAVIGATION")));
            Assert.IsTrue(output.Any(line => line.Contains("SCANNERS")));
            Assert.IsTrue(output.Any(line => line.Contains("SYSTEMS")));
        }

        [Test]
        public void DebugLine_OnlyOutputsWhenDebugModeEnabled()
        {
            DEFAULTS.DEBUG_MODE = false;
            _interact.DebugLine("debug");
            Assert.IsFalse(_interact.Output.Queue.Any(q => q.Contains("debug")));

            DEFAULTS.DEBUG_MODE = true;
            _interact.DebugLine("debug");
            Assert.IsTrue(_interact.Output.Queue.Any(q => q.Contains("debug")));
        }

        [Test]
        public void OutputStrings_WritesAllStrings()
        {
            _interact.OutputStrings(new[] { "a", "b" });

            Assert.IsTrue(_interact.Output.Queue.Any(q => q.Contains("a")));
            Assert.IsTrue(_interact.Output.Queue.Any(q => q.Contains("b")));
        }

        [Test]
        public void RenderCourse_ReturnsCourseGrid()
        {
            var course = _interact.RenderCourse();

            Assert.IsTrue(course.Contains("<*>"));
            Assert.IsTrue(course.Contains("1"));
            Assert.IsTrue(course.Contains("8"));
        }
    }
}
