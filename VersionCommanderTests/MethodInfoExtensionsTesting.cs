using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Tests
{
    [TestFixture]
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
    }
}