using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Castle.DynamicProxy;
using VersionCommander.Tests.TestingAssists;

namespace VersionCommander.Tests.ExternalTests
{
    public class TestingBasicCSharpKnowhow
    {

         public static class ChoosingOnMultipleGenericMethodsAvailable
         {
             //compile time exception: method with the same signature already delcared, duh.
                //more specifically, Eric Lippart has a blog post on this stuff: Generic type constraints are not considered in the compilers search for signatures.
                //http://blogs.msdn.com/b/ericlippert/archive/2009/12/10/constraints-are-not-part-of-the-signature.aspx

//             public static TSubject PickMe<TSubject>()
//                 where TSubject : new()
//             {
//                 throw new Exception("Closest match was generic with new() condition");
//             }  
             
             public static TSubject PickMe<TSubject>()
                 where TSubject : ICloneable
             {
                 throw new Exception("closest match was generic with Cloneable() condition!");
             }      
       
//             public static TSubject PickMe<TSubject>()
//                 where TSubject : IEquatable<TSubject>
//             {
//                 throw new Exception("closest match was generic with Cloneable() condition!");
//             }
         }

        [TestFixture]
        public class CSharpLanguageFeatureTesting
        {
            private struct Base
            {
            }
            //ok, so structs cannot inherit, but they can implement interfaces...
//            private struct Derrived : Base
//            {
//            }
            private struct CloneableStruct : ICloneable, IEquatable<CloneableStruct>
            {
                private readonly string _value;

                public CloneableStruct(string value)
                {
                    _value = value;
                }

                public object Clone()
                {
                    throw new NotImplementedException();
                }

                #region Equals nonsense

                public bool Equals(CloneableStruct other)
                {
                    return string.Equals(_value, other._value);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    return obj is CloneableStruct && Equals((CloneableStruct) obj);
                }

                public override int GetHashCode()
                {
                    return (_value != null ? _value.GetHashCode() : 0);
                }

                public static bool operator ==(CloneableStruct left, CloneableStruct right)
                {
                    return left.Equals(right);
                }

                public static bool operator !=(CloneableStruct left, CloneableStruct right)
                {
                    return !left.Equals(right);
                }

                #endregion
            }

            [Test]
            public void when_asing_a_struct_to_an_interface()
            {
                var cloneable = new CloneableStruct("not default");
                var properCast = cloneable as ICloneable;
                properCast.Should().NotBeNull();
                properCast.Should().NotBe(default(CloneableStruct));
            }

            //Wow, good on you C#, a compile time warning.
            //woulda been nice if you didnt allow for explicit and implicit cast overrides tho.
//            [Test]
//            public void when_asing_a_struct_to_an_interface_it_doesnt_implement()
//            {
//                var cloneable = new CloneableStruct("also not default");
//                var improperCast = cloneable as IVersionControlNode; //compile-time error. Intresting                
//            }

            [Test, Ignore("cant dynamic proxy structs since they cant be derrived... that makes my life a fair bit easier.")]
            public void when_asing_a_generated_type()
            {
                var fromCastle = new ProxyGenerator().CreateClassProxy(typeof (CloneableStruct), new object[]{"Value!"}, new IInterceptor[0]);
                var cast = fromCastle as IVersionControlNode;
                cast.Should().NotBe(default(CloneableStruct));
                cast.Should().BeNull();
            }

            //the default of any interface is null, because interfaces are ref types. They're so ref types, the act of casting something as its interface
            //converts it to a ref type. Intresting.
            //Can I do equals on the default of an interface that extends IEquatable?
        }


        [Test]
        public void messing_around_with_autoamppers_cloneing_functionality()
        {
            var firstChild = new FlatPropertyBag() {IntProperty = 4, StringProperty = "I remember this C#..."};
            var secondChild = new FlatPropertyBag() {IntProperty = 5, StringProperty = "deeply nested object initializer nonsense"};
            var childList = new List<FlatPropertyBag>() {firstChild, secondChild};

            var sample = new DeepPropertyBag()
            {
                ChildBags = childList,
                SpecialChild = new FlatPropertyBag() { IntProperty = 6, StringProperty = "Sigh, I wish there were a better way" },
                Stringey = "Newed"
            };

            var clonedViaAutomapper = (DeepPropertyBag)sample.Clone();

            clonedViaAutomapper.Should().NotBeNull();

            //ahh, so automapper doesnt actually perform a deep copy for me. I should've known this.
            clonedViaAutomapper.SpecialChild.Should().BeSameAs(sample.SpecialChild);
            clonedViaAutomapper.ChildBags.First().Should().BeSameAs(firstChild);
        }
       
        [Test]
        public void how_the_bugger_does_array_implement_add()
        {
            var array = new string[3];

            var cast = array as ICollection<string>;

            Assert.Throws<NotSupportedException>(() => cast.Add("Wat"));
            //so that makes sense, this is typical "collection is a fixed size" behavior.
        }

        [Test]
        public void when_calling_equals_on_delegates()
        {
            var func = new Func<int>(() => 1);
//            func.Should().Be(new Func<int>(() => 1));  //fails
            func.Should().NotBe(new Func<int>(() => 1)); //so this is intresting,
            //microsoft MVPs immediatly start talking about performance optimizations, and how the compiler will optimize those lambdas to avoid code explosion.
            //but even still, there is documentation for MulticastDelegate.Equals(), and this doesnt seem to adhere to that.
                //http://social.msdn.microsoft.com/Forums/nl/netfxbcl/thread/b6f9d0e8-78b8-4950-bcc1-f6717bdb4388
                //http://msdn.microsoft.com/en-us/library/1ts3c5tx

            (func as MulticastDelegate).Equals(new Func<int>(() => 1) as MulticastDelegate).Should().BeFalse();  
            //so I guess it boils down to lambdas, and how LambdaExpression.Compile() works. 

            var action = new Action(when_calling_equals_on_delegates);
            action.Should().Be(new Action(when_calling_equals_on_delegates));
            //so when you create a direct delegate, everything goes smoothly: the invocation lists are compared as as you'd expect,

            //but when you use a lambda...
            action.Should().NotBe(new Action(() => when_calling_equals_on_delegates()));
            //you get unequality. This is where the iffy behavior mentioned by the MS MVP kick in: if the compiler realizes that this lambda is identical
            //with another lambda somewhere else, it will use the same generated function for each lambda, and thus equality between the two will return true,
            //as they'll both be multicast delegates with the same invocation list.                
        }        

        private class IntBox
        {
            public int IntProperty { get; set; }
        }

        [Test, Ignore("This is issue #32 with Fake it easy @https://github.com/FakeItEasy/FakeItEasy/issues/32")]
        public void when_using_A_callTo_on_a_property_set_call()
        {
            var fake = A.Fake<IntBox>();
            fake.IntProperty = 4;

            A.CallTo(() => fake.IntProperty).WithAnyArguments().MustNotHaveHappened();
            //ahhh so set calls are excluded...

//            A.CallTo(() => fake.IntProperty = -1).WithAnyArguments().MustHaveHappened();
            //and of course an expression cannot contain an assignment operator...
        }

        [Test]
        public void when_using_A_callTo_on_a_property_get_call()
        {
            var fake = A.Fake<FlatPropertyBag>();
            var retrieved = fake.IntProperty;

            A.CallTo(() => fake.IntProperty).WithAnyArguments().MustHaveHappened();
        }
    }
}