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
                                                                             long version,
                                                                             bool isActive = true)
        {
            return new[] {new TimestampedPropertyVersionDelta(value, method, version, isActive)};
        }

        public static IEnumerable<TimestampedPropertyVersionDelta> ChangeSet(IEnumerable<object> values,
                                                                             IEnumerable<MethodInfo> methods,
                                                                             IEnumerable<long> versions,
                                                                             IEnumerable<bool> isActives = null)
        {
            var flatValues = values.ToArray();
            var flatMethods = methods.ToArray();
            var flatVersions = versions.ToArray();
            var flatActives = isActives == null ? Enumerable.Repeat(true, flatValues.Length).ToArray() : isActives.ToArray();

            Debug.Assert(flatMethods.Length == flatVersions.Length && flatVersions.Length == flatValues.Length && flatValues.Length == flatActives.Length);

            foreach(var index in Enumerable.Range(0, flatMethods.Length))
            {
                yield return new TimestampedPropertyVersionDelta(flatValues[index], flatMethods[index], flatVersions[index], flatActives[index]);
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