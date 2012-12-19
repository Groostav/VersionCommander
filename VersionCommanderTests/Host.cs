using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using System.Collections;
using VersionCommander.Exceptions;

namespace VersionCommander
{
    [TestFixture]
    public  class Host
    {
        [DebuggerDisplay("DeepPropertyBag : Stringey = {Stringey}")]
        public class DeepPropertyBag : ICloneable, IEquatable<DeepPropertyBag>, IVersionablePropertyBag
        {
            public virtual FlatPropertyBag SpecialChild { get; set; }
            public virtual IList<FlatPropertyBag> ChildBags { get; set; }
            public virtual string Stringey { get; set; }

            public object Clone()
            {
                var returnable = Mapper.Map<DeepPropertyBag>(this);
                return returnable;
            }

            public bool Equals(DeepPropertyBag other)
            {
                throw new NotImplementedException();
            }
        }

        [DebuggerDisplay("FlatPropertyBag : Stringey = {Stringey}")]
        public class FlatPropertyBag : ICloneable, IEquatable<FlatPropertyBag>, IVersionablePropertyBag
        {
            public virtual string Stringey { get; set; }
            public virtual int County { get; set; }

            public object Clone()
            {
                return Mapper.Map<FlatPropertyBag>(this);
            }

            #region Equality Nonsense

            public bool Equals(FlatPropertyBag other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Stringey, other.Stringey) && County == other.County;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((FlatPropertyBag) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (Stringey != null ? Stringey.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ County;
                    return hashCode;
                }
            }

            public static bool operator ==(FlatPropertyBag left, FlatPropertyBag right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(FlatPropertyBag left, FlatPropertyBag right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        [Test]
        public void Should_persist_on_simple_field_assignment()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            var expected = "new Stringey!";

            propertyBag.Stringey = expected;

            Assert.That(propertyBag.Stringey, Is.EqualTo(expected));
        }

        [Test]
        public void should_get_original_copy_when_asking_for_origins_ticks()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();

            var postConstruction = Stopwatch.GetTimestamp();

            var given = "new Stringey!";

            propertyBag.Stringey = given;

            Assert.That(propertyBag.WithoutModificationsPast(postConstruction).Stringey, Is.Null);
        }

        [Test]
        //this is not a unit test...
        public void custom_equality_comparison_should_return_ok()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            var other = propertyBag;

            Assert.That(other, Is.EqualTo(propertyBag));
        }

        [Test]
        public void should_rollback_a_specified_property()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            propertyBag.Stringey = "changed";
            propertyBag.UndoLastAssignmentTo(prop => prop.Stringey);
            propertyBag.Stringey.Should().BeNull();
        }

        [Test]
        public void should_throw_when_rolling_back_a_wtf_property()
        {
            var propertyBag = New.Versioning<FlatPropertyBag>();
            propertyBag.Stringey = "changed";

            Assert.Throws<NotImplementedException>(() => propertyBag.UndoLastAssignmentTo(prop => prop.Stringey.Length));
        }

        [Test]
        public void dicking_around_with_autoamppers_cloneing_functionality()
        {
            var sample = new DeepPropertyBag()
                             {
                                 ChildBags = new List<FlatPropertyBag>()
                                                {
                                                    new FlatPropertyBag() { County = 4, Stringey = "I remember this C#..." },
                                                    new FlatPropertyBag() { County = 5, Stringey = "deeply nested object initializer nonsense" }
                                                },
                                 SpecialChild = new FlatPropertyBag() { County = 6, Stringey = "Sigh, I wish there were a better way" },
                                 Stringey = "Newed"
                             };

            var clonedViaAutomapper = (DeepPropertyBag)sample.Clone();

            clonedViaAutomapper.Should().NotBeNull();
        }

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
        public void Should_properly_track_a_version_controlled_child()
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
                             {
                                 bag.SpecialChild = New.Versioning<FlatPropertyBag>();                              
                             });

            sample.Should().NotBeNull();
            sample.SpecialChild.Should().NotBeNull();

            sample.SpecialChild.Should().BeAssignableTo<FlatPropertyBag>();
            sample.SpecialChild.Should().BeAssignableTo<IVersionController<FlatPropertyBag>>();
            sample.SpecialChild.Should().BeAssignableTo<IVersionControlNode>();

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