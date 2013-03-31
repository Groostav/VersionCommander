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
            _lastVersionNumber = 0L;
        }

        [Test]
        public void when_forking_a_childless_object()
        {
            //setup
            var visitor = new FindAndCopyVersioningChildVisitor();

            var parent = _testHelper.MakeVersioning<FlatPropertyBag>().GetVersionControlNode();
            
            parent.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", TestHelper.FlatPropsString.SetMethod, 1L),
                                      new TimestampedPropertyVersionDelta("2", TestHelper.FlatPropsString.SetMethod, 2L),
                                      new TimestampedPropertyVersionDelta("3", TestHelper.FlatPropsString.SetMethod, 3L));

            //act
            parent.Accept(visitor);

            //assert
            parent.Children.Should().BeEmpty();
            parent.Mutations.Should().HaveCount(3);
        }

        [Test]
        public void when_forking_an_object_with_a_single_child()
        {
            //setup
            var visitor = new FindAndCopyVersioningChildVisitor();

            var parent = _testHelper.MakeVersioning<DeepPropertyBag>().GetVersionControlNode();
            var givenChild = _testHelper.MakeVersioning<DeepPropertyBag>(parent);
            var swappedChild = _testHelper.MakeVersioning<DeepPropertyBag>();

            A.CallTo(() => givenChild.GetVersionControlNode().CurrentDepthCopy()).Returns(swappedChild);

            var givenMutation = new TimestampedPropertyVersionDelta(givenChild, TestHelper.DeepNestedVersioner.SetMethod, 1L);
            parent.Mutations.Add(givenMutation);

            //act
            parent.Accept(visitor);

            //assert
            parent.Children.Should().NotIntersectWith(new[] {givenChild.GetVersionControlNode(), parent});
            parent.Children.Should().ContainSingle(actualChild => ReferenceEquals(actualChild, swappedChild.GetVersionControlNode()));

            parent.Mutations.Single().Should().NotBeSameAs(givenMutation);

            A.CallTo(() => givenChild.GetVersionControlNode().CurrentDepthCopy())
             .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => swappedChild.GetVersionControlNode().RecursiveAccept(null))
             .WhenArgumentsMatch(args => args.Single() is FindAndCopyVersioningChildVisitor)
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void when_forking_an_object_with_grandchildren()
        {
            //setup
            var visitor = new FindAndCopyVersioningChildVisitor();

            var parent = _testHelper.MakeVersioning<DeepPropertyBag>();
            var child = MakeChildAndAssociativeMutationTo(parent.GetNativeObject<DeepPropertyBag>());

            var orphanedGrandchild = MakeChildAndAssociativeMutationTo(child.GetNativeObject<DeepPropertyBag>());
            var grandChild = MakeChildAndAssociativeMutationTo(child.GetNativeObject<DeepPropertyBag>());

            child.GetVersionControlNode().Children.Remove(orphanedGrandchild.GetVersionControlNode());
            orphanedGrandchild.GetVersionControlNode().Parent = null;

            var swappedChild = _testHelper.ConfigureCurrentDepthCopy(child);
            var swappedGrandchild = _testHelper.ConfigureCurrentDepthCopy(grandChild);

            //act
            parent.GetVersionControlNode().Accept(visitor);

            //assert
            var actualChild = parent.GetVersionControlNode().Children.Single();
            actualChild.Should().NotBeSameAs(child).And.BeSameAs(swappedChild.GetVersionControlNode());
            var actualGrandchild = actualChild.Children.Single();
            actualGrandchild.Should().NotBeSameAs(grandChild).And.BeSameAs(swappedGrandchild.GetVersionControlNode());

            actualChild.Mutations.Should().HaveCount(2);

        }

        private long _lastVersionNumber;
        private IVersionControlledObject MakeChildAndAssociativeMutationTo(DeepPropertyBag parent) 
        {
            if (parent == null || ! parent.IsUnderVersionCommand()) 
                throw new ArgumentException("Must be not null and versioning object.", "parent");

            var existingNode = parent.GetVersionControlNode();
            var child = _testHelper.MakeVersioning<DeepPropertyBag>(existingNode);
            var addChildToParent = new TimestampedPropertyVersionDelta(child, TestHelper.DeepNestedVersioner.SetMethod, _lastVersionNumber++);
            parent.GetVersionControlNode().Mutations.Add(addChildToParent);
            return child;
        }
    }
}