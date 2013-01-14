
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FizzWare.NBuilder;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Cloners;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.UnitTests.TestingAssists
{
    public class TestHelper
    {
        public const string NonNullDefaultString = "Non Null Default";

        public TestHelper()
        {
            _cloneFactoryByClonedType = new Dictionary<Type, object>();
        }

        private readonly Dictionary<Type, object> _cloneFactoryByClonedType;
        public ICloneFactory<TClone> ProvidedCloneFactoryFor<TClone>()
        {
            object createdFactory;
            return _cloneFactoryByClonedType.TryGetValue(typeof (TClone), out createdFactory)
                       ? (ICloneFactory<TClone>) createdFactory
                       : null;
        } 
        public ICloneFactory<TClone> MakeConfiguredCloneFactoryFor<TClone>()
            where TClone : new()
        {
            var factory = A.Fake<ICloneFactory<TClone>>();
            _cloneFactoryByClonedType.Add(typeof(TClone), factory);
            return factory;
        }

        public IVersionControlTreeVisitor ProvidedRollbackVisitor { get; set; }
        public IVersionControlTreeVisitor ProvidedFindAndCopyVersioningChildVisitor { get; set; }
        public IVersionControlTreeVisitor ProvidedDeltaApplicationVisitor { get; set; }
        public IVersionControlTreeVisitor ProvidedDescendentAggregatorVisitor { get; set; }

        public IVisitorFactory ProvidedVisitorFactory { get; private set; }
        public IVisitorFactory MakeConfiguredVisitorFactory()
        {
            ProvidedVisitorFactory = A.Fake<IVisitorFactory>();

            ProvidedRollbackVisitor = ProvidedRollbackVisitor ?? A.Fake<IVersionControlTreeVisitor>();
            ProvidedDeltaApplicationVisitor = ProvidedDeltaApplicationVisitor ?? A.Fake<IVersionControlTreeVisitor>();
            ProvidedFindAndCopyVersioningChildVisitor = ProvidedFindAndCopyVersioningChildVisitor ?? A.Fake<IVersionControlTreeVisitor>();
            ProvidedDescendentAggregatorVisitor = ProvidedDescendentAggregatorVisitor ?? A.Fake<IVersionControlTreeVisitor>();

            A.CallTo(() => ProvidedVisitorFactory.MakeVisitor<FindAndCopyVersioningChildVisitor>()).Returns(ProvidedFindAndCopyVersioningChildVisitor);
            A.CallTo(() => ProvidedVisitorFactory.MakeVisitor<DescendentAggregatorVisitor>()).Returns(ProvidedDescendentAggregatorVisitor);
            A.CallTo(() => ProvidedVisitorFactory.MakeRollbackVisitor(0)).WithAnyArguments().Returns(ProvidedRollbackVisitor);
            A.CallTo(() => ProvidedVisitorFactory.MakeDeltaApplicationVisitor(ChangeType.Undo, false, null)).WithAnyArguments().Returns(ProvidedDeltaApplicationVisitor);

            return ProvidedVisitorFactory;
        }

        public IProxyFactory ProvidedProxyFactory { get; private set; }
        public FakeFlatPropertyBag ProvidedFlatPropertyBag { get; private set; }
        public IVersionControlNode ProvidedControlNode { get; private set; }

        public IProxyFactory MakeConfiguredProxyFactory()
        {
            ProvidedProxyFactory = A.Fake<IProxyFactory>();
            ProvidedControlNode = A.Fake<IVersionControlNode>();
            ProvidedFlatPropertyBag = A.Fake<FakeFlatPropertyBag>();

            A.CallTo(() => ProvidedProxyFactory.CreateVersioning<FlatPropertyBag>(null, null, null))
             .WithAnyArguments()
             .Returns(ProvidedFlatPropertyBag);

            A.CallTo(() => ProvidedFlatPropertyBag.GetVersionControlNode())
             .WithAnyArguments()
             .Returns(ProvidedControlNode);

            return ProvidedProxyFactory;
        }

        public IEnumerable<TimestampedPropertyVersionDelta> EmptyChangeSet()
        {
            return Enumerable.Empty<TimestampedPropertyVersionDelta>();
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

            Debug.Assert(flatMethods.Length == flatVersions.Length 
                        && flatVersions.Length == flatValues.Length 
                        && flatValues.Length == flatActives.Length);

            return Enumerable.Range(0, flatMethods.Length)
                             .Select(index => new TimestampedPropertyVersionDelta(flatValues[index], 
                                                                                  flatMethods[index],
                                                                                  flatVersions[index], 
                                                                                  flatActives[index]))
                             .ToArray();
                //never yield return new! those three keywords should be banned when used in succession!
        }

        public static IVersionControlNode CreateAndAddVersioningChildTo(
            PropertyBagVersionController<FlatPropertyBag> controller)
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

        public static TimestampedPropertyVersionDelta MakeDontCareChange<TContent>()
        {
            var targetSite = typeof (TContent).GetProperties().FirstOrDefault(prop => prop.GetSetMethod() != null);
            if(targetSite == null) throw new NotImplementedException("couldnt find a suitable property");
            var setValue = targetSite.GetGetMethod().ReturnType.GetDefaultValue();

            return new TimestampedPropertyVersionDelta(setValue, targetSite.GetSetMethod(), -1L, isActive:true);
        }

        public IVersionControlNode MakeVersionControlNodeWithChildren()
        {
            var node = A.Fake<VersionControlNodeBase>();
            var children = new[] {A.Fake<IVersionControlNode>(), A.Fake<IVersionControlNode>()};
            node.Children.AddRange(children);

            return node;
        }

        public IVersionControlNode MakeVersionControlNode()
        {
            return A.Fake<VersionControlNodeBase>();
        }

        public TimestampedPropertyVersionDelta CreateDeltaAndAssociateWithNode<TSetValue>(IVersionControlNode child, 
                                                                                          MethodInfo setMethod, 
                                                                                          long timestamp, 
                                                                                          bool isActive)
        {
            var fakeSetValue = A.Fake<TSetValue>(options => options.Implements(typeof(IVersionControlNode)));
            
            throw new NotImplementedException();

            return new TimestampedPropertyVersionDelta(fakeSetValue, setMethod, timestamp, isActive);
        }
    }
}