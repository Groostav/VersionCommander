using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;

namespace VersionCommander.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void when_assigning_field_value_should_appear_persisted()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            var expected = "new Stringey!";

            propertyBag.Stringey = expected;

            Assert.That(propertyBag.Stringey, Is.EqualTo(expected));
        }

        [Test]
        public void when_asking_for_version_just_post_construction_should_get_default_object()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();

            var postConstruction = Stopwatch.GetTimestamp();

            var given = "new Stringey!";

            propertyBag.Stringey = given;

            Assert.That(propertyBag.WithoutModificationsPast(postConstruction).Stringey, Is.Null);
        }

        [Test]
        public void when_performing_equals_on_checked_in_objects()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            var other = propertyBag;

            Assert.That(other, Is.EqualTo(propertyBag));
        }

        [Test]
        public void when_undoing_a_specific_property_the_result_should_have_the_previous_value()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            propertyBag.Stringey = "changed";
            propertyBag.UndoLastAssignmentTo(prop => prop.Stringey);
            propertyBag.Stringey.Should().BeNull();
        }

        [Test]
        public void should_throw_when_attempting_to_rollback_grandchild_object()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            propertyBag.Stringey = "changed";

            Assert.Throws<NotImplementedException>(() => propertyBag.UndoLastAssignmentTo(prop => prop.Stringey.Length));
        }

        //attempting to rollback child with no setter?

        [Test]
        public void when_using_the_oject_initializer_symmantics_it_should_properly_set_objcets_and_have_constructor_changes_in_version_control()
        {
            const int expectedCounty = 4;
            const string expectedStringey = "First String!";

            var sample = New.Versioning<FlatPropertyBag>(bag =>
                             {
                                 bag.County = expectedCounty;
                                 bag.Stringey = expectedStringey;
                             });

            sample.County.Should().Be(expectedCounty);
            sample.Stringey.Should().Be(expectedStringey);

            sample.VersionControlNode().Mutations.Should().HaveCount(2);
        }

        [Test]
        public void when_constructing_a_versioning_object_hierarchy_all_hierarchy_members_should_have_controllers()
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
                             {
                                 bag.SpecialChild = New.Versioning<FlatPropertyBag>();                              
                             });

            sample.Should().NotBeNull();
            sample.SpecialChild.Should().NotBeNull();

            sample.VersionControl().Should().NotBeNull().And.BeAssignableTo<IVersionController<DeepPropertyBag>>();
            sample.VersionControlNode().Should().NotBeNull().And.BeAssignableTo<IVersionControlNode>();
            sample.SpecialChild.VersionControl().Should().NotBeNull().And.BeAssignableTo<IVersionController<FlatPropertyBag>>();
            sample.SpecialChild.VersionControlNode().Should().NotBeNull().And.BeAssignableTo<IVersionControlNode>();
        } 

        [Test]
        public void when_constructing_a_versioning_object_hierarchy_all_hierarchy_members_should_be_aware_of_their_parent_and_children ()
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
            {
                bag.SpecialChild = New.Versioning<FlatPropertyBag>();
            });

            sample.Should().NotBeNull();
            sample.SpecialChild.Should().NotBeNull();

            sample.VersionControlNode().Children.Single().Should().BeSameAs(sample.SpecialChild.VersionControlNode());
            sample.SpecialChild.VersionControlNode().Parent.Should().BeSameAs(sample.VersionControlNode());
        }
        
        [Test]
        public void when_rolling_back_properties_of_unversioned_child()
        {
            var parent = New.Versioning<DeepPropertyBag>(bag =>
                         {
                             bag.SpecialChild = new FlatPropertyBag();
                         });

            parent.SpecialChild.Stringey = "ChildStringey";

            Assert.Throws<NotImplementedException>(() => parent.UndoLastAssignmentTo(x => x.SpecialChild.Stringey));
        }
        
        [Test]
        public void when_asking_for_a_previous_version_of_the_parent_it_should_bundle_a_previous_version_of_all_children()
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
                         {
                             bag.SpecialChild = New.Versioning<FlatPropertyBag>();
                         });
            var timeOfConstruction = Stopwatch.GetTimestamp();

            sample.SpecialChild.Stringey = "ChildStringey";
            sample.Stringey = "Parent Stringy";

            var copy = sample.WithoutModificationsPast(timeOfConstruction);
            copy.Stringey.Should().BeNull();
            copy.SpecialChild.Stringey.Should().BeNull();
        }        

        //get grand-fathering working
        //Make sure garbage collection works        
        //make sure references make sense.
        //make sure equals works as expected
        //whatever soluition you come up with for grand fathering make sure it threads well
        //make sure that if objects are tagged with IVersionablePropertyBag but not checked in anywhere everything still acts as expected

        //use cases:
            //user hits cancel after a series of dialogs, must undo all the dialogs
                //could be done via cloning the object, catching the event and re-assigning the clone.
            //user hits undo
            //user hits redo...
    }
}