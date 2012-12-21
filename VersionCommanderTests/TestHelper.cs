using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VersionCommander.Extensions;

namespace VersionCommander.Tests
{
    public static class TestHelper
    {
        public static IEnumerable<TimestampedPropertyVersionDelta> EmptyChangeSet()
        {
            return Enumerable.Empty<TimestampedPropertyVersionDelta>();
        }

        public static ICloneFactory<TCloneable> DefaultCloneFactoryFor<TCloneable>()
            where TCloneable : new()
        {
            return new DefaultCloneFactory<TCloneable>();
        } 

        public static IEnumerable<TimestampedPropertyVersionDelta> ChangeSet(object value,
                                                                             MethodInfo method,
                                                                             long version)
        {
            return new[] {new TimestampedPropertyVersionDelta(value, method, version)};
        }

        public static IEnumerable<TimestampedPropertyVersionDelta> ChangeSet(object[] values,
                                                                             MethodInfo[] methods,
                                                                             long[] versions)
        {
            Debug.Assert(methods.Length == versions.Length && versions.Length == values.Length);
            foreach(var index in Enumerable.Range(0, methods.Length))
            {
                yield return new TimestampedPropertyVersionDelta(values[index], methods[index], versions[index]);
            }
        }

        internal static bool IsAction<TActionInput>(this object argument, Action<TActionInput> action)
        {
            var cast = argument as Action<TActionInput>;
            if (cast == null) return false;

            return cast.Equals(action);
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