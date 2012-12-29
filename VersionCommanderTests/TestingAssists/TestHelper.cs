using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FakeItEasy;
using FizzWare.NBuilder;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.UnitTests.TestingAssists
{
    public static class TestHelper
    {
        public const string NonNullDefaultString = "Non Null Default";

        public static IEnumerable<TimestampedPropertyVersionDelta> EmptyChangeSet()
        {
            return Enumerable.Empty<TimestampedPropertyVersionDelta>();
        }

        //TODO this should be returning a fake
        public static ICloneFactory<TCloneable> DefaultCloneFactoryFor<TCloneable>()
            where TCloneable : new()
        {
            return new DefaultCloneFactory<TCloneable>();
        }

        public static IVisitorFactory FakeVisitorFactory()
        {
            return A.Fake<IVisitorFactory>();
        }

        public static IEnumerable<TimestampedPropertyVersionDelta> ChangeSet(object value,
                                                                             MethodInfo method,
                                                                             long version,
                                                                             bool isActive = true)
        {
            return new[] {new TimestampedPropertyVersionDelta(value, method, version, isActive)};
        }

        //FIXME this signature is disgusting.
        public static IEnumerable<TimestampedPropertyVersionDelta> ChangeSet(IEnumerable<object> values,
                                                                             IEnumerable<MethodInfo> methods,
                                                                             IEnumerable<long> versions,
                                                                             IEnumerable<bool> isActives = null)
        {
            var flatValues = values.ToArray();
            var flatMethods = methods.ToArray();
            var flatVersions = versions.ToArray();
            var flatActives = isActives == null
                                  ? Enumerable.Repeat(true, flatValues.Length).ToArray()
                                  : isActives.ToArray();

            Debug.Assert(flatMethods.Length == flatVersions.Length && flatVersions.Length == flatValues.Length &&
                         flatValues.Length == flatActives.Length);

            return Enumerable.Range(0, flatMethods.Length)
                             .Select(
                                 index =>
                                 new TimestampedPropertyVersionDelta(flatValues[index], flatMethods[index],
                                                                     flatVersions[index], flatActives[index]))
                             .ToArray();
                //never yield return new! those three keywords should be banned when used in succession!
        }

        internal static bool IsAction<TActionInput>(this object argument, Action<TActionInput> action)
        {
            var cast = argument as Action<TActionInput>;
            if (cast == null) return false;

            return cast.Equals(action);
        }

        public static IVersionControlNode CreateAndAddVersioningChildTo(
            PropertyVersionController<FlatPropertyBag> controller)
        {
            var child = A.Fake<IVersionControlNode>();
            controller.Children.Add(child);
            return child;
        }

        public static TBuildable CreateWithNonDefaultProperties<TBuildable>()
        {
            return Builder<TBuildable>.CreateNew().Build();
        }

        public static TResult ProvidedNonDefaultFor<TBuildable, TResult>(Func<TBuildable, TResult> propertyPointer)
        {
            return propertyPointer.Invoke(CreateWithNonDefaultProperties<TBuildable>());
        }

        public static IProxyFactory MakeConfiguredProxyFactory()
        {
            var factory = A.Fake<IProxyFactory>();
            FakeProxyFactoryProvidedControlNode = A.Fake<IVersionControlNode>();
            FakeProxyFactoryProvidedPropertyBag = A.Fake<FakeFlatPropertyBag>();

            A.CallTo(() => factory.CreateVersioning<FlatPropertyBag>(null, null, null))
             .WithAnyArguments()
             .Returns(FakeProxyFactoryProvidedPropertyBag);

            A.CallTo(() => FakeProxyFactoryProvidedPropertyBag.GetVersionControlNode())
             .WithAnyArguments()
             .Returns(FakeProxyFactoryProvidedControlNode);

            return factory;
        }

        public static FakeFlatPropertyBag FakeProxyFactoryProvidedPropertyBag { get; private set; }
        public static IVersionControlNode FakeProxyFactoryProvidedControlNode { get; private set; }

        public static TimestampedPropertyVersionDelta MakeDontCareChange<TContent>()
        {
            var targetSite = typeof (TContent).GetProperties().FirstOrDefault(prop => prop.GetSetMethod() != null);
            if(targetSite == null) throw new NotImplementedException("couldnt find a suitable property");
            var setValue = targetSite.GetGetMethod().ReturnType.GetDefaultValue();

            return new TimestampedPropertyVersionDelta(setValue, targetSite.GetSetMethod(), -1L, isActive:true);
        }
    }
}