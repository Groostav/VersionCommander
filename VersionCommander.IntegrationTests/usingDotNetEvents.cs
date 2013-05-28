using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.IntegrationTests;

namespace VersionCommander.IntegrationTests
{

    [TestFixture]
    public class Testing
    {
        [Test]
        public void tryingToRaiseAnEvent()
        {
            var bus = new EventBus();

            var @event = new MyEvent();
            bus.publish(@event);

            @event.SubscriberWasCalled.Should().BeTrue();
        }

        public void Subscriber(MyEvent @event)
        {
            @event.SubscriberWasCalled = true;
        }
    }

    // Special EventArgs class to hold info about Shapes. 
    public class MyEvent
    {
        public bool SubscriberWasCalled { get; set; }
    }

    // Base class event publisher 
    public class EventBus
    {
        public event Action<MyEvent> ShapeChanged;

        public void publish(MyEvent e)
        {
            ShapeChanged(e);
        }
    }

}