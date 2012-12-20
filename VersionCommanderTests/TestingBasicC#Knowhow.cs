using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Castle.DynamicProxy;
using VersionCommander.Tests;

namespace VersionCommander
{
    public class TestingBasicCSharpKnowhow
    {
        //-> MemberInfo
        // |-> PropertyInfo (what _members are)
        // |-> MethodBase
        //   |-> MethodInfo (what Invocation.Method is)

        //actually kind've bummed that for all C#s type inferance it couldn't figure that out. 
        //also bummed that TargetType is null, srsly what good is something like that? I blame Hamilton.

         public static class ChoosingOnMultipleGenericMethodsAvailable
         {
             //compile time exception: method with the same signature already delcared, duh.
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
                cast.Should().NotBe(default(IVersionControlNode));
                cast.Should().NotBe(default(CloneableStruct));
                cast.Should().BeNull();
            }

            //so this begs the question: wtf is the default IVersionControlNode? whats the default of any interface?
            //Can I do equals on the default of an interface that extends IEquatable?
        }


        [Test]
        public void messing_around_with_autoamppers_cloneing_functionality()
        {
            var firstChild = new FlatPropertyBag() {County = 4, Stringey = "I remember this C#..."};
            var secondChild = new FlatPropertyBag() {County = 5, Stringey = "deeply nested object initializer nonsense"};
            var childList = new List<FlatPropertyBag>() {firstChild, secondChild};

            var sample = new DeepPropertyBag()
            {
                ChildBags = childList,
                SpecialChild = new FlatPropertyBag() { County = 6, Stringey = "Sigh, I wish there were a better way" },
                Stringey = "Newed"
            };

            var clonedViaAutomapper = (DeepPropertyBag)sample.Clone();

            clonedViaAutomapper.Should().NotBeNull();

            //ahh, so automapper doesnt actually perform a deep copy for me. I should've known this.
            clonedViaAutomapper.SpecialChild.Should().BeSameAs(sample.SpecialChild);
            clonedViaAutomapper.ChildBags.First().Should().BeSameAs(firstChild);
        }

    }
}