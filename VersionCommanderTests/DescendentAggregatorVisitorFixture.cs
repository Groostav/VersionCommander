using NUnit.Framework;
using VersionCommander.Implementation.Visitors;
using VersionCommander.UnitTests.TestingAssists;

namespace VersionCommander.UnitTests
{
    [TestFixture]
    public class DescendentAggregatorVisitorFixture
    {
        private TestHelper _testHelper;

        [SetUp]
        public void setupTestHelper()
        {
            _testHelper = new TestHelper();
        }

        [Test]
        public void when_asking_for_descendents_of_node_with_children()
        {
            _testHelper.MakeConfiguredVisitorFactory();

            var visitor = new DescendentAggregatorVisitor();
            //visitor.Visit();
        }
    }
}