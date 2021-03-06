﻿using System;
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
            var rolledBackBag = propertyBag.WithoutModificationsPast(postConstruction);

            Assert.That(rolledBackBag.StringProperty, Is.Null);
            Assert.That(propertyBag.StringProperty, Is.EqualTo(given));
        }

        [Test]
        public void when_performing_equals_on_checked_in_objects()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            Debug.Assert(propertyBag is IEquatable<FlatPropertyBag>);
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

            sample.GetVersionControlNode().Mutations.Should().HaveCount(2);
        }

        [Test]
        public void when_constructing_a_versioning_object_hierarchy_all_hierarchy_members_should_have_controllers()
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
                             {
                                 bag.FlatChild = New.Versioning<FlatPropertyBag>();                              
                             });

            sample.Should().NotBeNull();
            sample.FlatChild.Should().NotBeNull();

            sample.VersionCommand().Should().NotBeNull().And.BeAssignableTo<IVersionController<DeepPropertyBag>>();
            sample.GetVersionControlNode().Should().NotBeNull().And.BeAssignableTo<IVersionControlNode>();
            sample.FlatChild.VersionCommand().Should().NotBeNull().And.BeAssignableTo<IVersionController<FlatPropertyBag>>();
            sample.FlatChild.GetVersionControlNode().Should().NotBeNull().And.BeAssignableTo<IVersionControlNode>();
        } 

        [Test]
        public void when_constructing_a_versioning_object_hierarchy_all_hierarchy_members_should_be_aware_of_their_parent_and_children()
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
            {
                bag.FlatChild = New.Versioning<FlatPropertyBag>();
            });

            sample.Should().NotBeNull();
            sample.FlatChild.Should().NotBeNull();

            sample.GetVersionControlNode().Children.Single().Should().BeSameAs(sample.FlatChild.GetVersionControlNode());
            sample.FlatChild.GetVersionControlNode().Parent.Should().BeSameAs(sample.GetVersionControlNode());
        }
        
        [Test]
        public void rolling_back_properties_of_unversioned_child_should_throw()
        {
            var parent = New.Versioning<DeepPropertyBag>(bag =>
                         {
                             bag.FlatChild = new FlatPropertyBag();
                         });

            parent.FlatChild.StringProperty = "ChildStringey";

            Assert.Throws<UntrackedObjectException>(() => parent.UndoLastAssignmentTo(x => x.FlatChild.StringProperty));
        }
        
        [Test]
        public void when_asking_for_a_previous_version_of_the_parent_it_should_bundle_a_previous_version_of_all_children()
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
                         {
                             bag.FlatChild = New.Versioning<FlatPropertyBag>();
                         });
            var timeOfConstruction = Stopwatch.GetTimestamp();

            sample.FlatChild.StringProperty = "ChildStringey";
            sample.DeepStringProperty = "Parent Stringy";

            var copy = sample.WithoutModificationsPast(timeOfConstruction);
            copy.DeepStringProperty.Should().BeNull();
            copy.FlatChild.StringProperty.Should().BeNull();
        }        
    }
}