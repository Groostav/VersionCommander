using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Tests.TestingAssists;
using VersionCommander.Implementation.Visitors;
using VersionCommander.UnitTests.TestingAssists;

// ReSharper disable InconsistentNaming -- test method names do not comply with naming convention
#pragma warning disable 169 // -- MSpec static test methods are unused
namespace VersionCommander.UnitTests
{
    [TestFixture]
    public class PropertyVersionControllerFixture
    {
        [Test]
        public void when_using_explicit_setter_on_new_object()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.EmptyChangeSet(),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
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
            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            changeSet,
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
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

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.ChangeSet(originalValue, targetSite, version:1),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());

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

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.EmptyChangeSet(),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());

            //act
            var retrievedValue = controller.Get(targetSite.GetParentProperty());

            //assert
            retrievedValue.Should().Be(TestHelper.ProvidedNonDefaultFor<FlatPropertyBag, string>(x => x.StringProperty));
        }

        [Test]
        public void when_using_a_visitor()
        {
            //setup
            var controller = new PropertyVersionController<FlatPropertyBag>(TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>(), 
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.EmptyChangeSet(),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            var fakeChildren = new[] {A.Fake<IVersionControlNode>(), A.Fake<IVersionControlNode>()};
            var fakeVisitor = A.Fake<IPropertyTreeVisitor>();
            controller.Children.AddRange(fakeChildren);

            //act
            controller.Accept(fakeVisitor);

            //assert
            A.CallTo(() => fakeChildren.First().Accept(fakeVisitor)).MustHaveHappened();
            A.CallTo(() => fakeChildren.Last().Accept(fakeVisitor)).MustHaveHappened();
            A.CallTo(() => fakeVisitor.RunOn(controller)).MustHaveHappened();
        }

        [Test]
        public void when_getting_version_from_construction()
        {
            //setup
            const string originalValue = "Original";
            var baseObject = new FlatPropertyBag() {StringProperty = originalValue};
            var visitorFactory = TestHelper.FakeVisitorFactory();
            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.EmptyChangeSet(),
                                                                            visitorFactory,
                                                                            TestHelper.MakeConfiguredProxyFactory());

            const long constructionTimeStamp = 2;

            //act
            controller.Set(baseObject.PropertyInfoFor(x => x.StringProperty), "Change!", constructionTimeStamp + 1);
            var retrievedValue = controller.WithoutModificationsPast(constructionTimeStamp);

            //assert
            A.CallTo(() => visitorFactory.MakeVisitor<FindAndCopyVersioningChildVisitor>()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => visitorFactory.MakeRollbackVisitor(constructionTimeStamp)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test, ThereBeDragons("This should be a simple test, why the setup code looks like a month's grocery list I dont know.")]
        public void when_getting_version_WithoutModifcationsPast_specific_version()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var visitorFactory = TestHelper.FakeVisitorFactory();
            var proxyFactory = TestHelper.MakeConfiguredProxyFactory();
            var fakeFindAndCopy = A.Fake<IPropertyTreeVisitor>();
            var fakeRollback = A.Fake<IPropertyTreeVisitor>();
            var change = TestHelper.MakeDontCareChange();

            A.CallTo(() => visitorFactory.MakeVisitor<FindAndCopyVersioningChildVisitor>()).Returns(fakeFindAndCopy);
            A.CallTo(() => visitorFactory.MakeRollbackVisitor(0)).WithAnyArguments().Returns(fakeRollback);

            const long targetVersion = 2L;
            
            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            new[]{change},
                                                                            visitorFactory,
                                                                            proxyFactory);

            //act
            controller.WithoutModificationsPast(targetVersion);

            //assert : It must have attempted to clone itself, then invoked find children on the clone, then invoked rollback on the clone.
            A.CallTo(() => proxyFactory.CreateVersioning<FlatPropertyBag>(null, null, null))
                                       .WhenArgumentsMatch(args => calledWithBaseObjectAndChangeSet(args, baseObject, change))
                                       .MustHaveHappened();
            A.CallTo(() => visitorFactory.MakeVisitor<FindAndCopyVersioningChildVisitor>()).MustHaveHappened();
            A.CallTo(() => visitorFactory.MakeRollbackVisitor(targetVersion)).MustHaveHappened();
            A.CallTo(() => TestHelper.FakeProxyFactoryProvidedControlNode.Accept(fakeFindAndCopy)).MustHaveHappened();
            A.CallTo(() => TestHelper.FakeProxyFactoryProvidedControlNode.Accept(fakeRollback)).MustHaveHappened();
        }

        private static bool calledWithBaseObjectAndChangeSet(ArgumentCollection args, FlatPropertyBag baseObject, TimestampedPropertyVersionDelta versionDelta)
        {
            return args[0].Equals(baseObject) && (args[2] as IEnumerable<TimestampedPropertyVersionDelta>).Single().Equals(versionDelta);
        }

        [Test]
        public void when_rolling_back_to_construction()
        {
            //setup
            const string originalValue = "Original!";
            var baseObject = new FlatPropertyBag() {StringProperty = originalValue};
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const int targetVersion = 1;

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.ChangeSet("Change!", targetSite, targetVersion + 1),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act
            controller.RollbackTo(targetVersion);

            //assert
            controller.Get(targetSite.GetParentProperty()).Should().Be(originalValue);
        }

        [Test]
        public void when_rolling_back_to_specific_version()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const int targetVersion = 2;
            const string targetVersionValue = "Two!";

            var changeSet = TestHelper.ChangeSet(new[] { "One", targetVersionValue, "Three" },
                                                 Enumerable.Repeat(targetSite, 3),
                                                 new[] { targetVersion - 1L, targetVersion, targetVersion + 1L });

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            changeSet,
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act
            controller.RollbackTo(targetVersion);

            //assert
            controller.Get(targetSite.GetParentProperty()).Should().Be(targetVersionValue);
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

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            changeSet,
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act
            controller.UndoLastAssignmentTo(self => self.StringProperty);

            //assert
            controller.Get(targetSite.GetParentProperty()).Should().Be(targetValue);
            controller.Mutations.Should().ContainSingle(mutation => mutation.TargetSite == targetSite && ! mutation.IsActive);
        }

        [Test]
        public void when_undoing_assignment_to_grandchild()
        {
            //setup
            var baseObject = new DeepPropertyBag();
            var childObject = A.Fake<FlatPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.SpecialChild).GetSetMethod();

            var controller = new PropertyVersionController<DeepPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<DeepPropertyBag>(),
                                                                            TestHelper.ChangeSet(childObject, targetSite, 1),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act & assert
            Assert.Throws<UntrackedObjectException>(() => controller.UndoLastAssignmentTo(self => self.SpecialChild.StringProperty));
                //intrestingly enough, I can actually get mildly better setup-act-assert segregation with a nasty try-catch block.

            //assert
            A.CallTo(() => childObject.StringProperty).WithAnyArguments().MustNotHaveHappened();
        }

        [Test]
        public void when_undoing_assignment_to_self()
        {
            //setup
            var baseObject = new DeepPropertyBag();

            var controller = new PropertyVersionController<DeepPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<DeepPropertyBag>(),
                                                                            TestHelper.EmptyChangeSet(),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act & assert
            Assert.Throws<UntrackedObjectException>(() => controller.UndoLastAssignmentTo(self => self));
        }

        [Test]
        public void when_undoing_assignment_to_property_with_no_setter()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.EmptyChangeSet(),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act & assert
            Assert.Throws<UntrackedObjectException>(() => controller.UndoLastAssignmentTo(self => self.PropWithoutSetter));
        }

        [Test]
        public void when_redoing_first_delta()
        {
            //setup
            const string targetValue = "New Value!";
            var baseObject = new FlatPropertyBag() {StringProperty = "Original!"};
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.ChangeSet(targetValue, targetSite, version: 1, isActive: false),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act
            controller.RedoLastAssignmentTo(x => x.StringProperty);

            //assert
            controller.Get(targetSite.GetParentProperty()).Should().Be(targetValue);
        }

        [Test]
        public void when_redoing_specific_delta()
        {
            //setup
            var baseObject = new FlatPropertyBag() {StringProperty = "Original!"};
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            var intSite = baseObject.PropertyInfoFor(x => x.IntProperty).GetSetMethod();
            const string targetValue = "New value!";

            //TODO make this into a fluent factory. this isnt readable in the sligthest.
                //blegh, overhead of fluent factories in tests... how many times a dev go through this struggle.
            var changeSet = TestHelper.ChangeSet(new object[] {"Active!",   1,          targetValue,    2,          "NotActive!",   3},
                                                 new[] {       targetSite,  intSite,    targetSite,     intSite,    targetSite,     intSite},
                                                 new[] {       1L,          2L,         3L,             4L,         5L,             6L},
                                                 new[] {       true,        true,       false,          true,       false,          false}); 

            //complex data to test the search. 
            //Maybe a better approach here is to have a brain-dead test and then bury search in integration tests?

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            changeSet,
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act
            controller.RedoLastAssignmentTo(self => self.StringProperty);

            //assert
            controller.Get(targetSite.GetParentProperty()).Should().Be(targetValue);
            controller.Mutations.Single(mutation => mutation.TargetSite == targetSite && mutation.Arguments.Single().As<string>() == targetValue)
                                .IsActive.Should().BeTrue();
        }
      
                //well maybe its not such a pain in the ass...
        [Test]
        public void when_redoing_delta_thats_been_trashed()
        {
            var baseObject = new FlatPropertyBag() { StringProperty = "Original!" };
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.ChangeSet("undone value", targetSite, 1L, isActive: false),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act
            controller.Set(targetSite.GetParentProperty(), "New Value", 2L);

            //further act & assert
            Assert.Throws<UntrackedObjectException>(() => controller.RedoLastAssignmentTo(x => x.StringProperty));
        }


        //TODO make this a new class, specifically oriented toward ensuring memory isolation.



        [Test]
        public void when_rolling_back_unaltered_object()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var stringPropInfo = baseObject.PropertyInfoFor(x => x.StringProperty);
            var countPropInfo = baseObject.PropertyInfoFor(x => x.IntProperty);
            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.EmptyChangeSet(),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act
            controller.RollbackTo(0);

            //assert
            controller.Get(stringPropInfo).Should().BeNull();
            controller.Get(countPropInfo).Should().Be(default(int));
        }

        [Test]
        public void when_getting_a_current_version_branch()
        {
             //setup
             var baseObject = new DeepPropertyBag();
             var childNode = A.Fake<IVersionControlNode>();
             var targetSite = baseObject.PropertyInfoFor(x => x.SpecialChild).GetSetMethod();
            
             var controller = new PropertyVersionController<DeepPropertyBag>(baseObject,
                                                                             TestHelper.DefaultCloneFactoryFor<DeepPropertyBag>(),
                                                                             TestHelper.ChangeSet(childNode, targetSite, version: 0),
                                                                             TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
             //act
            controller.GetCurrentVersion();
            
             //assert
//            A.CallTo(() => childNode.Accept(null)).WhenArgumentsMatch(args => args.Single().IsAction<IVersionControlNode>(controller.FindAndCloneVersioningChildren)).MustHaveHappened();
//            A.CallTo(() => childNode.CurrentDepthCopy()).MustHaveHappened();
        }

        [Test]
        [ThereBeDragons("this is pretty integration-ee")]
        public void when_editing_a_version_branch()
        {
            //setup
            var baseObject = TestHelper.CreateWithNonDefaultProperties<FlatPropertyBag>();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const string originalValue = "One";
            const int targetVersion = 1;

            var controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                            TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                            TestHelper.ChangeSet(originalValue, targetSite, targetVersion),
                                                                            TestHelper.FakeVisitorFactory(),
                                                                            TestHelper.MakeConfiguredProxyFactory());
            //act
            var clone = controller.GetCurrentVersion();
            clone.StringProperty = "Something New";

            //assert
            controller.Get(targetSite.GetParentProperty()).Should().Be(originalValue);
                //assert that the change did not propagate through to the original
        }
    }
}