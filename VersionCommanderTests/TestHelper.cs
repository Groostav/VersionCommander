using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VersionCommander.Tests
{
    public static class TestHelper
    {
        public static IEnumerable<TimestampedPropertyVersionDelta> EmptyChangeSet()
        {
            return Enumerable.Empty<TimestampedPropertyVersionDelta>();
        }
    }

    public static class PropertyManagementExtensions
    {
        public static PropertyInfo PropertyInfoFor<TSubject, TResult>(this TSubject subject, 
                                                                      Expression<Func<TSubject, TResult>> propertyPointer)
        {
            return MethodInfoExtensions.GetPropertyInfo(propertyPointer);
        }
    }
}