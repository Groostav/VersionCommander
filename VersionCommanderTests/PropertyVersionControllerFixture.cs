
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.NullObjects;
using VersionCommander.Implementation.Visitors;
using VersionCommander.UnitTests.TestingAssists;

#pragma warning disable 169 // -- MSpec static test methods are unused
namespace VersionCommander.UnitTests
{
    [TestFixture]
    public class PropertyVersionControllerFixture
    {
        private TestHelper _testHelper;

        private const string ExistingControllerArgumentName = "existingControlNode";
        private const string ExistingObjectArgumentName = "existingObject";

        [SetUp]
        public void SetupTestingAssists()
        {
            _testHelper = new TestHelper();

            var parameterNames = MethodInfoExtensions.GetMethodInfo<IProxyFactory>(x => x.CreateVersioning<object>(null, null, null))
                                                     .GetParameters()
                                                     .Select(param => param.Name);
            parameterNames.Should().Contain(new[] { ExistingControllerArgumentName, ExistingObjectArgumentName });
        }

        [Test]
        public void when_using_explicit_setter_on_new_object()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                            _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                            _testHelper.EmptyChangeSet(),
                                                                            _testHelper.MakeConfiguredVisitorFactory(),
                                                                            _testHelper.MakeConfiguredProxyFactory());
            const string changedValue = "Change!";

            //act
            controller.Set(baseObject.PropertyInfoFor(x => x.StringProperty), changedValue, 1);

            //assert
            controller.Mutations.Should().ContainSingle(mutation => mutation.Arguments.Single().Equals(changedValue));
        }

        [Test]
        public void when_using_explicit_setter_on_object_with_history()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            var changeSet = TestHelper.ChangeSet("Original", targetSite, 1L, isActive:false);
            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                            _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                            changeSet,
                                                                            _testHelper.MakeConfiguredVisitorFactory(),
                                                                            _testHelper.MakeConfiguredProxyFactory());
            const string changedValue = "Change!";

            //act
            controller.Set(targetSite.GetParentProperty(), changedValue, 2L);

            //assert
            controller.Mutations.Should().ContainSingle(mutation => mutation.Arguments.Single().Equals(changedValue));
            controller.Mutations.Should().HaveCount(1);
        }

        [Test]
        public void when_using_explicit_getter_on_object_with_history()
        {
            //setup
            const string originalValue = "Original!";
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               TestHelper.ChangeSet(originalValue, targetSite, version:1),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());

            //act
            var retrievedValue = controller.Get(targetSite.GetParentProperty());

            //assert
            retrievedValue.Should().Be(originalValue);
        }
        
        [Test]
        public void when_using_explicit_getter_on_new_object()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               _testHelper.EmptyChangeSet(),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());

            //act
            var retrievedValue = controller.Get(targetSite.GetParentProperty());

            //assert
            retrievedValue.Should().Be(TestHelper.ProvidedNonDefaultFor<FlatPropertyBag, string>(x => x.StringProperty));
        }

        [Test]
        public void when_using_a_visitor()
        {
            //setup
            var controller = new PropertyBagVersionController<FlatPropertyBag>(TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>(), 
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               _testHelper.EmptyChangeSet(),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());

            var fakeChildren = new[] {A.Fake<IVersionControlNode>(), A.Fake<IVersionControlNode>()};
            var fakeVisitor = A.Fake<IVersionControlTreeVisitor>();
            controller.Children.AddRange(fakeChildren);

            //act
            controller.Accept(fakeVisitor);

            //assert
            A.CallTo(() => fakeChildren.First().RecursiveAccept(fakeVisitor)).MustHaveHappened();
            A.CallTo(() => fakeChildren.Last().RecursiveAccept(fakeVisitor)).MustHaveHappened();
            A.CallTo(() => fakeVisitor.OnEntry(controller)).MustHaveHappened();
        }

        [Test]
        public void when_getting_version_without_modifications_past_specific_version()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var change = TestHelper.MakeDontCareChange<FlatPropertyBag>();

            const long targetVersion = 2L;
            
            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               new[]{change},
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());

            //act
            controller.WithoutModificationsPast(targetVersion);

            //assert : It must have created a versioning copy of itself, then invoked find children on the clone, then invoked rollback on the clone.
            A.CallTo(() => _testHelper.ProvidedProxyFactory.CreateVersioning<FlatPropertyBag>(null, null, null))
             .WhenArgumentsMatch(args => calledWithBaseObjectAndChangeSet(args, baseObject, change))
             .MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedVisitorFactory.MakeVisitor<FindAndCopyVersioningChildVisitor>()).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedVisitorFactory.MakeRollbackVisitor(targetVersion)).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedControlNode.Accept(_testHelper.ProvidedFindAndCopyVersioningChildVisitor)).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedControlNode.Accept(_testHelper.ProvidedRollbackVisitor)).MustHaveHappened();
        }

        private static bool calledWithBaseObjectAndChangeSet(ArgumentCollection args, FlatPropertyBag baseObject, TimestampedPropertyVersionDelta versionDelta)
        {
            var correct = args.Get<IVersionControlNode>(ExistingControllerArgumentName).Mutations.Single().Equals(versionDelta);
            correct &= args.Get<FlatPropertyBag>(ExistingObjectArgumentName).Equals(baseObject);
            return correct;
        }

        [Test]
        public void when_rolling_back()
        {
            //setup
            const string originalValue = "Original!";
            var baseObject = new FlatPropertyBag() {StringProperty = originalValue};
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const int targetVersion = 1;

            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               TestHelper.ChangeSet("Change!", targetSite, targetVersion + 1),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());
            //act
            controller.RollbackTo(targetVersion);

            //assert
            A.CallTo(() => _testHelper.ProvidedVisitorFactory.MakeRollbackVisitor(targetVersion)).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedRollbackVisitor.OnEntry(controller)).MustHaveHappened();
        }

        [Test]
        public void when_undoing_assignment_to_child()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const int targetVersion = 2;
            const string targetValue = "Two!";

            var changeSet = TestHelper.ChangeSet(new[] { "One", targetValue, "Three" },
                                                 Enumerable.Repeat(targetSite, 3),
                                                 new[] { targetVersion - 1L, targetVersion, targetVersion + 1L });

            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               changeSet,
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());
            //act
            controller.UndoLastAssignmentTo(self => self.StringProperty);

            //assert
            // RunVisitorOnTree(_visitorFactory.MakeDeltaApplicationVisitor(includeDescendents: false, makeActive:false, targetSite:targetSetter));
            A.CallTo(() => _testHelper.ProvidedVisitorFactory.MakeDeltaApplicationVisitor(ChangeType.Undo, false, targetSite)).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedDeltaApplicationVisitor.OnEntry(controller)).MustHaveHappened();

        }

        [Test]
        public void when_undoing_assignment_to_grandchild()
        {
            //setup
            var baseObject = new DeepPropertyBag();
            var childObject = A.Fake<DeepPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.DeepChild).GetSetMethod();

            var controller = new PropertyBagVersionController<DeepPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<DeepPropertyBag>(),
                                                                               TestHelper.ChangeSet(childObject, targetSite, 1),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());
            //act
            TestDelegate act = () => controller.UndoLastAssignmentTo(self => self.DeepChild.DeepStringProperty);

            //assert
            Assert.Throws<UntrackedObjectException>(act);
            A.CallTo(() => childObject.DeepStringProperty).MustNotHaveHappened();
        }

        [Test]
        public void when_undoing_assignment_to_self()
        {
            //setup
            var controller = new PropertyBagVersionController<FlatPropertyBag>(TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>(),
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               _testHelper.EmptyChangeSet(),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());
            //act
            TestDelegate act = () => controller.UndoLastAssignmentTo(self => self);

            //assert
            Assert.Throws<UntrackedObjectException>(act);
        }

        [Test]
        public void when_undoing_assignment_where_no_candidate_changes_exist()
        {
            //setup
            var controller = new PropertyBagVersionController<FlatPropertyBag>(TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>(),
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               _testHelper.EmptyChangeSet(),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());
            //act
            TestDelegate act = () => controller.UndoLastAssignmentTo(self => self.StringProperty);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
        }

        [Test]
        public void when_undoing_assignment_to_property_with_no_setter()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();

            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               _testHelper.EmptyChangeSet(),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());
            //act
            TestDelegate act = () => controller.UndoLastAssignmentTo(self => self.PropWithoutSetter);

            //assert
            Assert.Throws<UntrackedObjectException>(act);
            A.CallTo(_testHelper.ProvidedVisitorFactory.MakeDeltaApplicationVisitor(ChangeType.Unknown, false))
             .WithAnyArguments()
             .MustNotHaveHappened();
        }

        [Test]
        public void when_redoing_delta()
        {
            //setup
            const string targetValue = "New Value!";
            var baseObject = new FlatPropertyBag() {StringProperty = "Original!"};
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               TestHelper.ChangeSet(targetValue, targetSite, version: 1, isActive: false),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());
            //act
            controller.RedoLastAssignmentTo(x => x.StringProperty);

            //assert
            A.CallTo(() => _testHelper.ProvidedDeltaApplicationVisitor.OnEntry(controller)).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedVisitorFactory.MakeDeltaApplicationVisitor(ChangeType.Redo, false, targetSite)).MustHaveHappened();
        }
      
        [Test]
        public void when_redoing_delta_thats_been_trashed()
        {
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               TestHelper.ChangeSet("undone value", targetSite, 1L, isActive: false),
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());
            //act
            controller.Set(targetSite.GetParentProperty(), "New Value", 2L);
            TestDelegate act = () => controller.RedoLastAssignmentTo(x => x.StringProperty);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
            A.CallTo(() => _testHelper.ProvidedVisitorFactory.MakeDeltaApplicationVisitor(ChangeType.Unknown, false, null))
             .WithAnyArguments()
             .MustNotHaveHappened();
        }

        [Test]
        public void when_getting_a_current_version()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var change = TestHelper.MakeDontCareChange<FlatPropertyBag>();

            var controller = new PropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                               _testHelper.MakeConfiguredCloneFactoryFor<FlatPropertyBag>(),
                                                                               new[] { change },
                                                                               _testHelper.MakeConfiguredVisitorFactory(),
                                                                               _testHelper.MakeConfiguredProxyFactory());

            //act
            controller.GetCurrentVersion();

            //assert : It must have created a versioning copy of itself, then invoked find children on the clone, then invoked rollback on the clone.
            A.CallTo(() => _testHelper.ProvidedProxyFactory.CreateVersioning<FlatPropertyBag>(null, null, null))
             .WhenArgumentsMatch(args => calledWithBaseObjectAndChangeSet(args, baseObject, change))
             .MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedVisitorFactory.MakeVisitor<FindAndCopyVersioningChildVisitor>()).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedVisitorFactory.MakeRollbackVisitor(long.MaxValue)).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedControlNode.Accept(_testHelper.ProvidedFindAndCopyVersioningChildVisitor)).MustHaveHappened();
            A.CallTo(() => _testHelper.ProvidedControlNode.Accept(_testHelper.ProvidedRollbackVisitor)).MustHaveHappened();
        }
    }
}