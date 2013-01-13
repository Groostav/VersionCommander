using System.Linq;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;
using VersionCommander.UnitTests.TestingAssists;

namespace VersionCommander.IntegrationTests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void when_assigning_field_value_should_appear_persisted()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            var expected = "new StringProperty!";

            propertyBag.StringProperty = expected;

            Assert.That(propertyBag.StringProperty, Is.EqualTo(expected));
        }

        [Test]
        public void when_asking_for_version_just_post_construction_should_get_default_object()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();

            var postConstruction = Stopwatch.GetTimestamp();

            var given = "new StringProperty!";

            propertyBag.StringProperty = given;

            Assert.That(propertyBag.WithoutModificationsPast(postConstruction).StringProperty, Is.Null);
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

            propertyBag.StringProperty = "changed";

            propertyBag.UndoLastAssignmentTo(prop => prop.StringProperty);
            propertyBag.StringProperty.Should().BeNull();
        }

        [Test]
        public void should_throw_when_attempting_to_rollback_grandchild_object()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            propertyBag.StringProperty = "changed";

            Assert.Throws<UntrackedObjectException>(() => propertyBag.UndoLastAssignmentTo(prop => prop.StringProperty.Length));
        }

        //attempting to rollback child with no setter?

        [Test]
        public void when_using_the_oject_initializer_symmantics_it_should_properly_set_objcets_and_have_constructor_changes_in_version_control()
        {
            const int expectedCounty = 4;
            const string expectedStringey = "First String!";

            var sample = New.Versioning<FlatPropertyBag>(bag =>
                             {
                                 bag.IntProperty = expectedCounty;
                                 bag.StringProperty = expectedStringey;
                             });

            sample.IntProperty.Should().Be(expectedCounty);
            sample.StringProperty.Should().Be(expectedStringey);

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

            sample.VersionCommand().Should().NotBeNull().And.BeAssignableTo<IVersionController<DeepPropertyBag>>();
            sample.VersionControlNode().Should().NotBeNull().And.BeAssignableTo<IVersionControlNode>();
            sample.SpecialChild.VersionCommand().Should().NotBeNull().And.BeAssignableTo<IVersionController<FlatPropertyBag>>();
            sample.SpecialChild.VersionControlNode().Should().NotBeNull().And.BeAssignableTo<IVersionControlNode>();
        } 

        [Test]
        public void when_constructing_a_versioning_object_hierarchy_all_hierarchy_members_should_be_aware_of_their_parent_and_children()
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
        public void rolling_back_properties_of_unversioned_child_should_throw()
        {
            var parent = New.Versioning<DeepPropertyBag>(bag =>
                         {
                             bag.SpecialChild = new FlatPropertyBag();
                         });

            parent.SpecialChild.StringProperty = "ChildStringey";
            
            Assert.Throws<UntrackedObjectException>(() => parent.UndoLastAssignmentTo(x => x.SpecialChild.StringProperty));
        }
        
        [Test]
        //THIS IS PRETTY FUCKIN COOL.
        public void when_asking_for_a_previous_version_of_the_parent_it_should_bundle_a_previous_version_of_all_children()
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
                         {
                             bag.SpecialChild = New.Versioning<FlatPropertyBag>();
                         });
            var timeOfConstruction = Stopwatch.GetTimestamp();

            sample.SpecialChild.StringProperty = "ChildStringey";
            sample.Stringey = "Parent Stringy";

            var copy = sample.WithoutModificationsPast(timeOfConstruction);
            copy.Stringey.Should().BeNull();
            copy.SpecialChild.StringProperty.Should().BeNull();
        }        

        //TODO assert IsActive fix actually moves stuff to new memory.

        //get grand-fathering working
            //controller unit tests asserts that RunVisitorOnTree with Clone was called
            //also edits a copied child and makes sure the original wasnt altered

        //Make sure garbage collection works        
            //untested, unbouded memory growth.

        //make sure references make sense.
            //uhhhnnnyeehh-untested
        //make sure equals works as expected
                

        //whatever soluition you come up with for grand fathering make sure it threads well
        //make sure that if objects are tagged with IVersionablePropertyBag but not checked in anywhere everything still acts as expected

        //use cases:
            //user hits cancel after a series of dialogs, must undo all the dialogs
                //could be done via cloning the object, catching the event and re-assigning the clone.
            //user hits undo
            //user hits undo multiple times
            //user hits redo...
    }
}