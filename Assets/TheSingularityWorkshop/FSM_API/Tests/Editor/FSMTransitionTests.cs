using NUnit.Framework;

using System;

using TheSingularityWorkshop.FSM.API;

namespace TheSingularityWorkshop.FSM.Tests
{
    public class FSMTransitionTests
    {
        private class TestContext : IStateContext
        {
            public string Name { get; set; } = "TestContext";
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            bool called = false;
            Func<IStateContext, bool> condition = ctx => { called = true; return true; };
            var transition = new FSMTransition("A", "B", condition);

            Assert.AreEqual("A", transition.From);
            Assert.AreEqual("B", transition.To);
            Assert.AreEqual(condition, transition.Condition);

            var ctx = new TestContext();
            Assert.IsTrue(transition.Condition(ctx));
            Assert.IsTrue(called);
        }

        [Test]
        public void Constructor_Throws_OnNullOrWhitespaceFrom()
        {
            Func<IStateContext, bool> condition = ctx => true;
            Assert.Throws<ArgumentException>(() => new FSMTransition(null, "B", condition));
            Assert.Throws<ArgumentException>(() => new FSMTransition("", "B", condition));
            Assert.Throws<ArgumentException>(() => new FSMTransition("   ", "B", condition));
        }

        [Test]
        public void Constructor_Throws_OnNullOrWhitespaceTo()
        {
            Func<IStateContext, bool> condition = ctx => true;
            Assert.Throws<ArgumentException>(() => new FSMTransition("A", null, condition));
            Assert.Throws<ArgumentException>(() => new FSMTransition("A", "", condition));
            Assert.Throws<ArgumentException>(() => new FSMTransition("A", "   ", condition));
        }

        [Test]
        public void Constructor_Throws_OnNullCondition()
        {
            Assert.Throws<ArgumentNullException>(() => new FSMTransition("A", "B", null));
        }

        [Test]
        public void ToString_ReturnsExpectedFormat()
        {
            Func<IStateContext, bool> condition = ctx => true;
            var transition = new FSMTransition("A", "B", condition);
            Assert.AreEqual("A --[Condition]--> B", transition.ToString());
        }
    }
}