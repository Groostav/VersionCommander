using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Machine.Specifications;
using NUnit.Framework;
using VersionCommander;
using VersionCommander.Tests;

namespace VersionCommander
{
    //blegh, resharper doesnt see it, and these tests are clumsy. 
    //I might use this for the integrationee stuff, but for an object as annoyingly complex
    //as InterceptedPropertyBagVersionController I might just use NUnit + Fluent Assertions.
    [Subject("user creates a ")]
    public class when_creating_version_controllers
    {
        private static FlatPropertyBag baseObject;
        private static IntercetpedPropertyBagVersionController<FlatPropertyBag> controller;
        private static string result = "Unassigned";
        private static long postConstruction;

        Establish context = () => 
        {
            baseObject = new FlatPropertyBag();
            controller = new IntercetpedPropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                                      new DefaultCloneFactory<FlatPropertyBag>(),
                                                                                      TestHelper.EmptyChangeSet());
            postConstruction = Stopwatch.GetTimestamp();
        };

        Because of = () =>
        {
            controller.Set(baseObject.PropertyInfoFor(x => x.Stringey), "New value!!");
            result = controller.GetVersionAt(postConstruction).Stringey;
        };

        It should_not_have_the_new_value_for_the_result = () => result.Should().NotBe("New value!!");
        It should_have_the_original_value_for_the_result = () => result.Should().Be(null);
    }


    [TestFixture]
    public class IntercetpedPropertyBagVersionControllerFixture
    {
        //GetVersionOfPropertyAt
        [Test]
        public void when_asking_for_construction_time_version_of_a_property_should_get_original_value()
        {
            var baseObject = new FlatPropertyBag();
            var versioningFlatBag = new IntercetpedPropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                                                 new DefaultCloneFactory<FlatPropertyBag>(),
                                                                                                 TestHelper.EmptyChangeSet());
            var constructionTimeStamp = Stopwatch.GetTimestamp();
            var changedValue = "Change!";

            versioningFlatBag.Set(baseObject.PropertyInfoFor(x => x.Stringey), changedValue);

            var original = versioningFlatBag.GetVersionAt(constructionTimeStamp);
            original.Should().NotBeNull();
            original.Stringey.Should().Be(null);
        }

        [Test]
        public void when_asking_for_version_of_property_under_deltas_should_retrieve_value_from_correct_delta()
        {
            var baseObject = new FlatPropertyBag();
            var targetSite = baseObject.PropertyInfoFor(x => x.Stringey).GetSetMethod();
            const int targetVersion = 2;
            const string targetVersionValue = "Two!";

            var changeSet = new List<TimestampedPropertyVersionDelta>()
                                {
                                    new TimestampedPropertyVersionDelta("One",              targetSite,  targetVersion - 1),
                                    new TimestampedPropertyVersionDelta(targetVersionValue, targetSite,  targetVersion),
                                    new TimestampedPropertyVersionDelta("Three",            targetSite,  targetVersion + 1)
                                };

            var versioningFlatBag = new IntercetpedPropertyBagVersionController<FlatPropertyBag>(baseObject,
                                                                                                 new DefaultCloneFactory<FlatPropertyBag>(),
                                                                                                 changeSet);


            var retrievedVersion = versioningFlatBag.GetVersionAt(targetVersion).Stringey;


            retrievedVersion.Should().Be(targetVersionValue);
        }
    }
}