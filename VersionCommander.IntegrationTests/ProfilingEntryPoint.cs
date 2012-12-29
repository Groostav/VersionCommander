using System.Diagnostics;
using FluentAssertions;
using VersionCommander.Implementation.Tests.TestingAssists;
using VersionCommander.UnitTests.TestingAssists;

namespace VersionCommander.IntegrationTests
{
    public class ProfilingEntryPoint
    {
        public static void Main(string[] args)
        {
            var sample = New.Versioning<DeepPropertyBag>(bag =>
            {
                bag.SpecialChild = New.Versioning<FlatPropertyBag>();
            });
            var timeOfConstruction = Stopwatch.GetTimestamp();

            sample.SpecialChild.StringProperty = "ChildStringey";
            sample.Stringey = "Parent Stringy";

            //ok, so I was hoping that Castle would cache the proxy type, but the profiler says otherwise.
            //can I force it to cache that type somehow? I 
            var copy = sample.WithoutModificationsPast(timeOfConstruction);
            copy.Stringey.Should().BeNull();
            copy.SpecialChild.StringProperty.Should().BeNull();
        } 
    }
}