﻿using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Visitors;
using VersionCommander.UnitTests.TestingAssists;

namespace VersionCommander.UnitTests
{
    [TestFixture]
    public class DeltaApplicationVisitorFixture
    {
        private TestHelper _testHelper;

        [SetUp]
        public void setupTestHelper()
        {
            _testHelper = new TestHelper();
        }
        
        [Test]
        public void when_undoing_on_an_empty_set_of_version_deltas()
        {
            //Setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: null, 
                                                      searchWholeTree: true);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
        }

        [Test]
        public void when_undoing_a_version_delta_when_all_version_deltas_are_already_undone()
        {
            //setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: TestHelper.FlatPropsString.SetMethod, 
                                                      searchWholeTree: true);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();
            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", TestHelper.FlatPropsString.SetMethod, 1L, isActive: false),
                                    new TimestampedPropertyVersionDelta("2", TestHelper.FlatPropsString.SetMethod, 2L, isActive: false));

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
        }

        [Test]
        public void when_undoing_a_specific_available_assignment()
        {
            //setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: null, 
                                                      searchWholeTree: true);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();

            var targetSite = new FlatPropertyBag().PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive: true),
                                    new TimestampedPropertyVersionDelta("2", targetSite, 2L, isActive: false));

            //act
            node.Accept(visitor);

            //assert
            node.Mutations.Should().OnlyContain(mutation => ! mutation.IsActive);
        }        

        //not sure how I feel about copy-pasting tests based on nearly identical behavior.
        [Test]
        public void when_redoing_on_an_empty_set_of_version_deltas()
        {
            //Setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Redo,
                                                      targetSite: null, 
                                                      searchWholeTree: true);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
        }

        [Test]
        public void when_redoing_a_version_delta_that_has_only_active_deltas()
        {
            //setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Redo,
                                                      targetSite: TestHelper.FlatPropsString.SetMethod, 
                                                      searchWholeTree: true);

            var node = _testHelper.MakeVersionControlNodeWithChildren();
            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", TestHelper.FlatPropsString.SetMethod, 1L, isActive: true),
                                    new TimestampedPropertyVersionDelta("2", TestHelper.FlatPropsString.SetMethod, 2L, isActive: true));

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
        }

        [Test]
        public void when_redoing_a_specific_available_assignment()
        {
            //setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Redo,
                                                      targetSite: TestHelper.FlatPropsString.SetMethod,
                                                      searchWholeTree: false);

            var node = _testHelper.MakeVersionControlNodeWithChildren();

            var targetActiveDelta = new TimestampedPropertyVersionDelta("2", TestHelper.FlatPropsString.SetMethod, 2L, isActive: false);
            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", TestHelper.FlatPropsString.SetMethod, 1L, isActive: true),
                                    targetActiveDelta);
                                   
            //act
            node.Accept(visitor);

            //assert
            targetActiveDelta.IsActive.Should().BeTrue();
            node.Mutations.Except(new[]{targetActiveDelta}).Should().OnlyContain(mutation => mutation.IsActive);
        }

        [Test]
        public void when_undoing_assignment_to_parent_when_assignment_only_available_on_children()
        {
            //setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: TestHelper.FlatPropsString.SetMethod, 
                                                      searchWholeTree: false);

            var node = _testHelper.MakeVersionControlNodeWithChildren();
            A.CallTo(() => node.Children.First().Mutations)
             .Returns(new[] { new TimestampedPropertyVersionDelta("1", TestHelper.FlatPropsString.SetMethod, 1L, isActive: true) });

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
            visitor.Descendents.Should().HaveCount(1).And.Contain(node);
        }        

        [Test]
        public void when_attempting_to_undo_something_with_ambigious_versioning()
        {
            //setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: TestHelper.FlatPropsString.SetMethod, 
                                                      searchWholeTree: false);

            var node = _testHelper.MakeVersionControlNodeWithChildren();

            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", TestHelper.FlatPropsString.SetMethod, 1L, isActive: true),
                                    new TimestampedPropertyVersionDelta("2", TestHelper.FlatPropsString.SetMethod, 1L, isActive: true));

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionClockResolutionException>(act);
        }

        [Test]
        public void when_undoing_operation_on_child()
        {
            //setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: TestHelper.DeepPropsString.SetMethod, 
                                                      searchWholeTree: true);

            var parent = _testHelper.MakeVersioning<DeepPropertyBag>().GetVersionControlNode();
            var child = _testHelper.MakeVersioningObject(parent).GetVersionControlNode();

            TimestampedPropertyVersionDelta targetMutation;

            parent.Mutations.AddRange(new[]{                  new TimestampedPropertyVersionDelta("1", TestHelper.DeepPropsString.SetMethod, 1L, isActive:true), 
                                                              new TimestampedPropertyVersionDelta("2", TestHelper.DeepPropsString.SetMethod, 2L, isActive:true)});
            child.Mutations.AddRange(new[]{                   new TimestampedPropertyVersionDelta("3", TestHelper.DeepPropsString.SetMethod, 3L, isActive:true), 
                                             targetMutation = new TimestampedPropertyVersionDelta("4", TestHelper.DeepPropsString.SetMethod, 4L, isActive:true)});

            //act
            parent.Accept(visitor);

            //assert
            targetMutation.IsActive.Should().BeFalse();
            parent.Mutations.Should().OnlyContain(mutation => mutation.IsActive);
            child.Mutations.Except(targetMutation).Should().OnlyContain(mutation => mutation.IsActive);
        }
    }
}
