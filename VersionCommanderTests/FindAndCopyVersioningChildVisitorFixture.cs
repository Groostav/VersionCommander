using System;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Implementation.Visitors;
using VersionCommander.UnitTests.TestingAssists;

namespace VersionCommander.UnitTests
{
    [TestFixture]
    public class FindAndCopyVersioningChildVisitorFixture
    {
        private TestHelper _testHelper;

        [SetUp]
        public void Setup()
        {
            _testHelper = new TestHelper();
        }

        [Test, Ignore("not yet completed")]
        public void when_forking_an_object_with_a_single_child()
        {
            //setup
            var visitor = new FindAndCopyVersioningChildVisitor();
            _testHelper.ProvidedFindAndCopyVersioningChildVisitor = visitor;

            var parent = _testHelper.CreateVersioningObjectWithChildren();
            var child = _testHelper.CreateVersioningObject();
            parent.Children.Add(child);

            //act
            parent.Accept(_testHelper.ProvidedFindAndCopyVersioningChildVisitor);

            //assert
            var result = parent.Children.Should().NotIntersectWith(new[] {child, parent});
        }
    }
}