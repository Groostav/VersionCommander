using System;
using System.Diagnostics;
using FluentAssertions;
using VersionCommander.UnitTests.TestingAssists;

namespace VersionCommander.IntegrationTests
{
    public class ProfilingEntryPoint
    {
        public static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var sample = New.Versioning<DeepPropertyBag>(bag =>
            {
                bag.SpecialChild = New.Versioning<FlatPropertyBag>();
            });
            stopwatch.Stop();
            Console.WriteLine("First construction took {0}ms", stopwatch.ElapsedMilliseconds);
            var timeOfConstruction = Stopwatch.GetTimestamp();

            const string constructionChildString = "ChildStringey";
            sample.SpecialChild.StringProperty = constructionChildString;
            sample.Stringey = "Parent Stringy";

            stopwatch.Restart();
            //ok, so I was hoping that Castle would cache the proxy type, but the profiler says otherwise.
            //can I force it to cache that type somehow? I 
            var copy = sample.WithoutModificationsPast(timeOfConstruction);
            stopwatch.Stop();
            Console.WriteLine("first call to 'WithoutModificationsPast' took {0}ms", stopwatch.ElapsedMilliseconds);
            copy.Stringey.Should().BeNull();
            copy.SpecialChild.StringProperty.Should().BeNull();

            const string desiredString = "This change gets in before the next timestamp!";
            sample.Stringey = desiredString;

            var newTimestamp = Stopwatch.GetTimestamp();

            sample.Stringey = "Another change!";
            sample.SpecialChild.StringProperty = "Yet more changes!";

            stopwatch.Restart();
            copy = sample.WithoutModificationsPast(newTimestamp);
            stopwatch.Stop();
            Console.WriteLine("Second call to 'WithoutModificationsPast' took {0}ms", stopwatch.ElapsedMilliseconds);

            copy.Stringey.Should().Be(desiredString);
            copy.SpecialChild.StringProperty.Should().Be(constructionChildString);

            Console.WriteLine("any key to exit");
            Console.ReadKey();
        } 
    }
}