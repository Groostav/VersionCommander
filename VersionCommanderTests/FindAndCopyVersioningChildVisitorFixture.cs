using System;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Extensions;
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

        [Test]
        public void when_forking_an_object_with_a_single_child()
        {
            //setup
            var visitor = new FindAndCopyVersioningChildVisitor();
            var targetSite = MethodInfoExtensions.GetPropertyInfo<DeepPropertyBag, FlatPropertyBag>(x => x.SpecialChild);
            _testHelper.ProvidedFindAndCopyVersioningChildVisitor = visitor;

            var parent = _testHelper.MakeVersionControlNodeWithChildren();
            var child = _testHelper.MakeVersionControlNode();
            parent.Mutations.Add(_testHelper.CreateDeltaAndAssociateWithNode<FlatPropertyBag>(child, targetSite.SetMethod, 1L, isActive:true));
            parent.Children.Add(child);

            //act
            parent.Accept(_testHelper.ProvidedFindAndCopyVersioningChildVisitor);

            //assert
            var result = parent.Children.Should().IntersectWith(new[] {child, parent});
        }
    }
}