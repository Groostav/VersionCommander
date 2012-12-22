using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Memento;

namespace VersionCommander.Tests.ExternalTests
{
    [TestFixture]
    //hmm, poking around in this package, moral of the story: Yo dawg Events are pretty nice and 
    public class TestingBuusMementoPackage
    {
        [Test]
        public void when_attempting_to_restore_an_externally_tracked_list()
        {
            var mementor = new Mementor(isEnabled: true);

            var collection = new List<string>();
            var item = "hey!";
            mementor.ElementAdd(collection, item);//this is an extension method? Oh I see, he didnt want to put custom-list logic on Mementor. Nice.
            //it also doesnt actually add the element to the list:
            collection.Add(item);
            collection.Should().HaveCount(1);

            mementor.Undo();
            //hmm, so it expects the caller to do the Add but it will handle the remove?

            collection.Should().BeEmpty();
        }


        public class DomainPropertyBag{}

        public class SimpleMementoingClass
        {
            public IList<DomainPropertyBag> Domainees { get; set; }
            public DomainPropertyBag ChiefDomainee { get; set; }

            public string Address { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        //yeah so each of these getters/setters would have to invoke the mementor?

        [Test]
        public void when_attempting_to_version_a_property_bag()
        {
            var originalAddress = "12345 Fake Street";
            var subscriber = new SimpleMementoingClass()
                                 {
                                     Domainees = new List<DomainPropertyBag>()
                                                     {
                                                         new DomainPropertyBag(),
                                                         new DomainPropertyBag()
                                                     },
                                     ChiefDomainee = new DomainPropertyBag(),
                                     Address = originalAddress,
                                     FirstName = "If only Customer Objects",
                                     LastName = "were ever this simple"
                                 };

            var mementor = new Mementor();

            var newAddress = "Changed Address!";
            mementor.PropertyChange(subscriber, () => subscriber.Address);
            //ohhh typing is a little weak here.
            subscriber.Address = newAddress; //ahh, note the order matters of course.

            subscriber.Address.Should().Be(newAddress);

            mementor.Undo();
            //and I cant undo specific changes, just the last one.

            subscriber.Address.Should().Be(originalAddress);
            //still, very cool. 
                //In terms of a deployed program, this is tough to implement.  
                //I like the pub-sub design of it, code is superbly clean.
        }

        [Test]
        public void when_forcing_bad_things_via_expression()
        {
            var subscriber = new SimpleMementoingClass();
            var mementor = new Mementor();

            subscriber.Address = "Changed!";
            Assert.Throws<InvalidCastException>(() => mementor.PropertyChange(subscriber, () => mementor.ToString()));
            //aww duder and the exceptions arn't handled nicely here either, Linq at the wrong thing and get a generic Expression.OhgodohgodohgodException
                //why not public void PropertyChange<TSubject, TTargetSite>(this TSubject subject, Func<TSubject, TTargetSite> transform...)
                    //would be mementor.PropertyChange(subscriber, sub => sub.SomeProp), would provide some additional typiing on the expression.
        }
    }
}