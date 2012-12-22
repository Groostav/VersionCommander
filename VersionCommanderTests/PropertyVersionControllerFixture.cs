using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FluentAssertions;
using Machine.Specifications;
using NUnit.Framework;
using VersionCommander.Extensions;

namespace VersionCommander.Tests
{
    //blegh, resharper doesnt see it, and these tests are clumsy. 
    //I might use this for the integrationee stuff, but for an object as annoyingly complex
    //as InterceptedPropertyBagVersionController I might just use NUnit + Fluent Assertions.
    [Subject("user creates a ")]
    public class when_creating_version_controllers
    {
        private static FlatPropertyBag baseObject;
        private static PropertyVersionController<FlatPropertyBag> controller;
        private static string _result = "Unassigned";
        private static long postConstruction;

        Establish context = () => 
        {
            baseObject = new FlatPropertyBag();
            controller = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                       TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                       TestHelper.EmptyChangeSet());
            postConstruction = Stopwatch.GetTimestamp();
        };

        Because of = () =>
        {
            controller.Set(baseObject.PropertyInfoFor(x => x.Stringey), "New value!!");
            _result = controller.GetVersionAt(postConstruction).Stringey;
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
            versioningFlatBag.Set(baseObject.PropertyInfoFor(x => x.Stringey), changedValue);

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
            versioningFlatBag.Set(baseObject.PropertyInfoFor(x => x.Stringey), changedValue);
            var original = versioningFlatBag.GetVersionAt(constructionTimeStamp);

            //assert
            original.Should().NotBeNull();
            original.Stringey.Should().Be(null);
        }

        [Test]
        public void when_getting_specific_version()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var targetSite = baseObject.PropertyInfoFor(x => x.Stringey).GetSetMethod();
            const int targetVersion = 2;
            const string targetVersionValue = "Two!";

            var changeSet = new[]
                                {
                                    new TimestampedPropertyVersionDelta("One",              targetSite,  targetVersion - 1),
                                    new TimestampedPropertyVersionDelta(targetVersionValue, targetSite,  targetVersion),
                                    new TimestampedPropertyVersionDelta("Three",            targetSite,  targetVersion + 1)
                                };
            

            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   TestHelper.ChangeSet(new[] { "One", targetVersionValue, "Three" }, 
                                                                                                        Enumerable.Repeat(targetSite, 3);, 
                                                                                                        new[] { targetVersion - 1L, targetVersion, targetVersion + 1L }));

            //act
            var retrievedVersion = versioningFlatBag.GetVersionAt(targetVersion).Stringey;

            //assert
            retrievedVersion.Should().Be(targetVersionValue);
        }

        [Test]
        public void when_rolling_back_to_construction()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var targetSite = baseObject.PropertyInfoFor(x => x.Stringey).GetSetMethod();
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
            var targetSite = baseObject.PropertyInfoFor(x => x.Stringey).GetSetMethod();
            const int targetVersion = 2;
            const string targetVersionValue = "Two!";

            var changeSet = new[]
                                {
                                    new TimestampedPropertyVersionDelta("One",              targetSite,  targetVersion - 1),
                                    new TimestampedPropertyVersionDelta(targetVersionValue, targetSite,  targetVersion),
                                    new TimestampedPropertyVersionDelta("Three",            targetSite,  targetVersion + 1)
                                };

            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   changeSet);
            //act
            versioningFlatBag.RollbackTo(targetVersion);

            //assert
            versioningFlatBag.Get(targetSite.GetParentProperty()).Should().Be(targetVersionValue);
        }



        //TODO make this a new class, specifically oriented toward ensuring memory isolation.



        [Test]
        public void when_rolling_back_unaltered_object()
        {
            //setup
            var baseObject = new FlatPropertyBag();
            var stringPropInfo = baseObject.PropertyInfoFor(x => x.Stringey);
            var countPropInfo = baseObject.PropertyInfoFor(x => x.County);
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
            var targetSite = baseObject.PropertyInfoFor(x => x.Stringey).GetSetMethod();
            const string originalValue = "One";
            const int targetVersion = 1;

            var versioningFlatBag = new PropertyVersionController<FlatPropertyBag>(baseObject,
                                                                                   TestHelper.DefaultCloneFactoryFor<FlatPropertyBag>(),
                                                                                   TestHelper.ChangeSet(originalValue, targetSite, targetVersion));
            //act
            var clone = versioningFlatBag.GetCurrentVersion();
            clone.Stringey = "something New";

            //assert
            versioningFlatBag.Get(targetSite.GetParentProperty()).Should().Be(originalValue);
                //assert that the change did not propagate through to the original
        }
    }
}