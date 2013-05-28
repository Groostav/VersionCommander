using System;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using VersionCommander.Implementation.Extensions;
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

        [Test, Ignore("not imeplemented")]
        public void when_asking_for_descendents_of_node_with_children()
        {
//            _testHelper.MakeConfiguredVisitorFactory();
//
//            var visitor = new DescendentAggregatorVisitor();
//
//            var parent = _testHelper.MakeVersionControlNodeWithChildren();
//
//            parent.Accept(visitor);
//
//            foreach (var node in parent.Children.Union(parent))
//            {
//                A.CallTo(visit)
//            }
//
//            throw new NotImplementedException();

        }
    }
}