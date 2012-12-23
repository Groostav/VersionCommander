﻿using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Extensions;

namespace VersionCommander.Tests
{
    [TestFixture]
    public class MsCoreLibExtensionsFixture
    {
        [Test]
        public void verify_IsOrderedBy()
        {
            var list = new List<int>() { 1, 2, 3, 4 };

            list.IsOrderedBy(item => item).Should().BeTrue();

            list = new List<int>() { 4, 3, 2, 1 };

            list.IsOrderedBy(item => item).Should().BeFalse();
        }

        [Test]
        public void verify_withMax()
        {
            var list = new List<int>() {1, 4, 3, 4, 2};
            var maxGroup = list.WithMax(x => x);

            maxGroup.Should().HaveCount(2);
            maxGroup.Should().BeEquivalentTo(new[] {4, 4});
        }

        [Test]
        public void verify_withMin()
        {
            var list = new List<int>() {2, 5, 3, 4, 2};
            var maxGroup = list.WithMin(x => x);

            maxGroup.Should().HaveCount(2);
            maxGroup.Should().BeEquivalentTo(new[] {2, 2});
        }
    }
}