using System.Linq;
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
            var targetSite = new FlatPropertyBag().PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: targetSite, 
                                                      searchWholeTree: true);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();
            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive:false),
                                    new TimestampedPropertyVersionDelta("2", targetSite, 2L, isActive:false));

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
            var targetActiveDelta = new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive: true);
            node.Mutations.AddRange(targetActiveDelta,
                                    new TimestampedPropertyVersionDelta("2", targetSite, 2L, isActive: false));

            //act
            node.Accept(visitor);

            //assert
            targetActiveDelta.IsActive.Should().BeFalse();
            node.Mutations.Except(new[] {targetActiveDelta}).Should().OnlyContain(mutation => ! mutation.IsActive);
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
            var targetSite = new FlatPropertyBag().PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Redo,
                                                      targetSite: targetSite, 
                                                      searchWholeTree: true);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();
            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive:true),
                                    new TimestampedPropertyVersionDelta("2", targetSite, 2L, isActive:true));

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
        }

        [Test]
        public void when_redoing_a_specific_available_assignment()
        {
            //setup
            var targetSite = new FlatPropertyBag().PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Redo,
                                                      targetSite: targetSite,
                                                      searchWholeTree: false);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();

            var targetActiveDelta = new TimestampedPropertyVersionDelta("2", targetSite, 2L, isActive: false);
            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive: true),
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
            var targetSite = new FlatPropertyBag().PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: targetSite, 
                                                      searchWholeTree: false);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();
            A.CallTo(() => node.Children.First().Mutations)
             .Returns(new[]{new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive: true)});

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
            var targetSite = new FlatPropertyBag().PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: targetSite, 
                                                      searchWholeTree: false);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeVersionControlNodeWithChildren();

            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive: true),
                                    new TimestampedPropertyVersionDelta("2", targetSite, 1L, isActive: true));

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionClockResolutionException>(act);
        }

        [Test]
        public void when_undoing_operation_on_child()
        {
            //setup
            var targetSite = new FlatPropertyBag().PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo,
                                                      targetSite: targetSite, 
                                                      searchWholeTree: true);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var parent = _testHelper.MakeVersionControlNodeWithChildren();
            var child = _testHelper.MakeVersionControlNode();
            parent.Children.Add(child);

            parent.Mutations.AddRange(new[]{new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive:true), 
                                            new TimestampedPropertyVersionDelta("2", targetSite, 2L, isActive:true)});
            var targetMutation = new TimestampedPropertyVersionDelta("4", targetSite, 4L, isActive: true);
            child.Mutations.AddRange(new[]{new TimestampedPropertyVersionDelta("3", targetSite, 3L, isActive:true), 
                                           targetMutation});

            //act
            parent.Accept(visitor);

            //assert
            targetMutation.IsActive.Should().BeFalse();
            parent.Mutations.Should().OnlyContain(mutation => mutation.IsActive);
            child.Mutations.Except(new[] { targetMutation }).Should().OnlyContain(mutation => mutation.IsActive);
        }
    }
}