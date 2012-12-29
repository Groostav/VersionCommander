using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.UnitTests
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
                throw new NotImplementedException();
            }

            public void AGenericMethod<Titem>(Titem item)
            {
                throw new NotImplementedException();
            }

            public int AProperty { get; set; }

            public int this[int index] 
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public int this[string index]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
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
        public void GetPropertyInfo_should_return_property_info_for_simple_property()
        {
            var propInfo = MethodInfoExtensions.GetPropertyInfo<TestClass, int>(x => x.AProperty);
            propInfo.Name.Should().Be("AProperty");
        }

        
        [Test]
        public void GetPropertyInfo_should_return_method_info_for_indexer()
        {
            var propInfo = MethodInfoExtensions.GetPropertyInfo<TestClass, int>(x => x[0]);
            propInfo.Name.Should().Be("Item");
        }
    }
}