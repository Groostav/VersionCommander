using System;
using System.Diagnostics;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Machine.Specifications;
using NUnit.Framework;
using VersionCommander.Exceptions;
using VersionCommander.Extensions;

// ReSharper disable InconsistentNaming -- test method names do not comply with naming convention
#pragma warning disable 169 // -- MSpec static test methods are unused
namespace VersionCommander.Tests
{
    //blegh, resharper doesnt see it, and these tests are clumsy. h
    //I might use this for the integrationee stuff, but for an object as annoyingly complex
    //as InterceptedPropertyBagVersionController I might just use NUnit + Fluent Assertions.
    [Subject("user creates a ")]
    public class when_creating_version_controllers
    {
        private static FlatPropertyBag _baseObject;
        private static PropertyVersionController<FlatPropertyBag> _controller;
        private static string _result = "Unassigned";
        private static long _postConstruction;

        Establish context = () => 
        {
            _baseObject = new FlatPropertyBag();
            _controller = new PropertyVersionController<FlatPropertyBag>(_baseObject,
                                                                       TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                       TestHelper.EmptyChangeSet());
            _postConstruction = Stopwatch.GetTimestamp();
        };

        Because of = () =>
        {
            _controller.Set(_baseObject.PropertyInfoFor(x => x.StringProperty), "New value!!");
            _result = _controller.GetVersionAt(_postConstruction).StringProperty;
        };

        It should_not_have_the_new_value_for_the_result = () => _result.Should().NotBe("New value!!");
        It should_have_the_original_value_for_the_result = () => _result.Should().Be(null);
    }


    [TestFixture]
    public class PropertyVersionControllerFixture
    {
        [Test]
        public void when_using_explicit_setter()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   TestHelper.EmptyChangeSet());
            const string changedValue = "Change!";

            //act
            versioningFlatBag.Set(baseObject.PropertyInfoFor(x => x.StringProperty), changedValue);

            //assert
            versioningFlatBag.Mutations.Should().ContainSingle(mutation => changedValue.Equals(mutation.Arguments.Single()) );
        }

        [Test]
        public void when_getting_version_from_construction()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   TestHelper.EmptyChangeSet());
            var constructionTimeStamp = Stopwatch.GetTimestamp();
            var changedValue = "Change!";

            //act
            versioningFlatBag.Set(baseObject.PropertyInfoFor(x => x.StringProperty), changedValue);
            var original = versioningFlatBag.GetVersionAt(constructionTimeStamp);

            //assert
            original.Should().NotBeNull();
            original.StringProperty.Should().Be(null);
        }

        [Test]
        public void when_getting_specific_version()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const int targetVersion = 2;
            const string targetVersionValue = "Two!";

            var changeSet = TestHelper.ChangeSet(new[] {"One", targetVersionValue, "Three"},
                                                 Enumerable.Repeat(targetSite, 3),
                                                 new[] {targetVersion - 1L, targetVersion, targetVersion + 1L});
            
            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   changeSet);

            //act
            var retrievedVersion = versioningFlatBag.GetVersionAt(targetVersion).StringProperty;

            //assert
            retrievedVersion.Should().Be(targetVersionValue);
        }

        [Test]
        public void when_rolling_back_to_construction()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const int targetVersion = 1;

            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   TestHelper.ChangeSet("One", targetSite, targetVersion));
            //act
            versioningFlatBag.RollbackTo(targetVersion - 1);

            //assert
            versioningFlatBag.Get(targetSite.GetParentProperty()).Should().BeNull();
        }

        [Test]
        public void when_rolling_back_to_specific_version()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const int targetVersion = 2;
            const string targetVersionValue = "Two!";

            var changeSet = TestHelper.ChangeSet(new[] { "One", targetVersionValue, "Three" },
                                                 Enumerable.Repeat(targetSite, 3),
                                                 new[] { targetVersion - 1L, targetVersion, targetVersion + 1L });

            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   changeSet);
            //act
            versioningFlatBag.RollbackTo(targetVersion);

            //assert
            versioningFlatBag.Get(targetSite.GetParentProperty()).Should().Be(targetVersionValue);
        }

        [Test]
        public void when_undoing_assignment_to_child()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const int targetVersion = 2;
            const string targetValue = "Two!";

            var changeSet = TestHelper.ChangeSet(new[] { "One", targetValue, "Three" },
                                                 Enumerable.Repeat(targetSite, 3),
                                                 new[] { targetVersion - 1L, targetVersion, targetVersion + 1L });

            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   changeSet);
            //act
            versioningFlatBag.UndoLastAssignmentTo(self => self.StringProperty);

            //assert
            versioningFlatBag.Get(targetSite.GetParentProperty()).Should().Be(targetValue);
        }

        [Test]
        public void when_undoing_assignment_to_grandchild()
        {
            //setup
            var baseObject = new DeepPropertyBag();
            var childObject = A.Fake<FlatPropertyBag>();

            var versioningFlatBag = new PropertyVersionController<DeepPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<DeepPropertyBag>(),
                                                                                   TestHelper.ChangeSet(childObject, baseObject.PropertyInfoFor(x => x.SpecialChild).GetSetMethod(), 1));
            //act & assert
            var caught = Assert.Throws<UntrackedObjectException>(() => versioningFlatBag.UndoLastAssignmentTo(self => self.SpecialChild.StringProperty));
                //intrestingly enough, I can actually get mildly better setup-act-assert segregation with a nasty try-catch block.

            //assert
            A.CallTo(() => childObject.StringProperty).WithAnyArguments().MustNotHaveHappened();
        }

        [Test]
        public void when_undoing_assignment_to_self()
        {
            //setup
            var baseObject = new DeepPropertyBag();

            var versioningFlatBag = new PropertyVersionController<DeepPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<DeepPropertyBag>(),
                                                                                   TestHelper.EmptyChangeSet());
            //act & assert
            var caught = Assert.Throws<UntrackedObjectException>(() => versioningFlatBag.UndoLastAssignmentTo(self => self));
        }

        [Test]
        public void when_undoing_assignment_to_property_with_no_setter()
        {
            //setup
            var baseObject = new FlatPropertyBag();

            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   TestHelper.EmptyChangeSet());
            //act & assert
            var caught = Assert.Throws<UntrackedObjectException>(() => versioningFlatBag.UndoLastAssignmentTo(self => self.PropWithoutSetter));
        }



        //TODO make this a new class, specifically oriented toward ensuring memory isolation.



        [Test]
        public void when_rolling_back_unaltered_object()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var stringPropInfo = baseObject.PropertyInfoFor(x => x.StringProperty);
            var countPropInfo = baseObject.PropertyInfoFor(x => x.IntProperty);
            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   TestHelper.EmptyChangeSet());
            //act
            versioningFlatBag.RollbackTo(0);

            //assert
            versioningFlatBag.Get(stringPropInfo).Should().BeNull();
            versioningFlatBag.Get(countPropInfo).Should().Be(default(int));
        }

        [Test]
        public void when_getting_version_branch()
        {
             //setup
             var baseObject = new DeepPropertyBag();
             var childNode = A.Fake<IVersionControlNode>();
             var targetSite = baseObject.PropertyInfoFor(x => x.SpecialChild).GetSetMethod();
            
             var versioningFlatBag = new PropertyVersionController<DeepPropertyBag>(baseObject,
                                                                                    TestHelper.DefaultCloneFactoryFor<DeepPropertyBag>(),
                                                                                    TestHelper.ChangeSet(childNode, targetSite, version:0));
             //act
            versioningFlatBag.GetCurrentVersion();
            
             //assert
            A.CallTo(() => childNode.Accept(null)).WhenArgumentsMatch(args => args.Single().IsAction<IVersionControlNode>(versioningFlatBag.ScanAndClone)).MustHaveHappened();
            A.CallTo(() => childNode.CurrentDepthCopy()).MustHaveHappened();
        }

        [Test]
        public void when_editing_a_requested_version()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var targetSite = baseObject.PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            const string originalValue = "One";
            const int targetVersion = 1;

            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   TestHelper.ChangeSet(originalValue, targetSite, targetVersion));
            //act
            var clone = versioningFlatBag.GetCurrentVersion();
            clone.StringProperty = "something New";

            //assert
            versioningFlatBag.Get(targetSite.GetParentProperty()).Should().Be(originalValue);
                //assert that the change did not propagate through to the original
        }
    }
}