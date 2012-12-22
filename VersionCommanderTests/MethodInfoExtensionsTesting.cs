using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Extensions;

namespace VersionCommander.Tests
{
    public class MethodInfoExtensionsTesting
    {
        private class TestClass
        {
            public static void StaticTestMethod()
            {
                throw new NotImplementedException();
            }

            public void AMethod()
            {
                throw new System.NotImplementedException();
            }

            public void AGenericMethod<Titem>(Titem item)
            {
                throw new NotImplementedException();
            }
        }


        [Test]
        public void GetMethodInfo_should_return_method_info()
        {
            var methodInfo = MethodInfoExtensions.GetMethodInfo<TestClass>(c => c.AMethod());
            methodInfo.Name.Should().Be("AMethod");
        }

        [Test]
        public void GetMethodInfo_should_return_method_info_for_generic_method()
        {
            var methodInfo = MethodInfoExtensions.GetMethodInfo<TestClass>(c => c.AGenericMethod(default(int)));

            methodInfo.Name.Should().Be("AGenericMethod");
            methodInfo.GetParameters().First().ParameterType.Should().Be(typeof(int));
        }

        [Test]
        public void GetMethodInfo_should_return_method_info_for_static_method_on_static_class()
        {
            var methodInfo = MethodInfoExtensions.GetMethodInfo(() => TestClass.StaticTestMethod());

            methodInfo.Name.Should().Be("StaticTestMethod");
            methodInfo.IsStatic.Should().BeTrue();
        }

        [Test]
        public void verify_IsOrderedBy()
        {
            var list = new List<int>() {1, 2, 3, 4};

            list.IsOrderedBy(item => item).Should().BeTrue();

            list = new List<int>() {4, 3, 2, 1};

            list.IsOrderedBy(item => item).Should().BeFalse();
        }
    }
}