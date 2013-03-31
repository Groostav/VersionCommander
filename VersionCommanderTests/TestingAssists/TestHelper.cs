
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
        #region stateless shortcut statics
        public static PropertyInfo DeepNestedVersioner
        {
            get { return MethodInfoExtensions.GetPropertyInfo<DeepPropertyBag, DeepPropertyBag>(x => x.DeepChild); }
        }
        public static PropertyInfo GrandDeepPropsVersioner
        {
            get { return MethodInfoExtensions.GetPropertyInfo<GrandDeepPropertyBag, DeepPropertyBag>(x => x.DeepChild); }
        }
        public static PropertyInfo DeepPropsFlatVersioner
        {
            get { return MethodInfoExtensions.GetPropertyInfo<DeepPropertyBag, FlatPropertyBag>(x => x.FlatChild); }
        }
        public static PropertyInfo DeepPropsString
        {
            get { return MethodInfoExtensions.GetPropertyInfo<DeepPropertyBag, string>(x => x.DeepStringProperty); }
        }
        public static PropertyInfo FlatPropsString
        {
            get { return MethodInfoExtensions.GetPropertyInfo<FlatPropertyBag, string>(x => x.StringProperty); }
        }
        #endregion

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
            ProvidedProxyFactory = ProvidedProxyFactory ?? A.Fake<IProxyFactory>();
            ProvidedControlNode = ProvidedControlNode ?? A.Fake<IVersionControlNode>();
            ProvidedFlatPropertyBag = ProvidedFlatPropertyBag ?? A.Fake<FakeFlatPropertyBag>();

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

        public class TestHelperObject
        {
        }

        public IVersionControlledObject MakeVersioningObject(IVersionControlNode parent = null)
        {
            return MakeVersioningNodeBasedOn<TestHelperObject>(parent);
        }
        public IVersionControlledObject MakeVersioningObject(IVersionControlledObject parent)
        {
            return MakeVersioningNodeBasedOn<TestHelperObject>(parent.GetVersionControlNode());
        }
        public IVersionControlledObject MakeVersioning<TSubject>(IVersionControlNode parent = null)
            where TSubject : class
        {
            return MakeVersioningNodeBasedOn<TSubject>(parent);
        }
        public IVersionControlledObject MakeVersioning<TSubject>(IVersionControlledObject parent) 
            where TSubject : class
        {
            return MakeVersioningNodeBasedOn<TSubject>(parent.GetVersionControlNode());
        }
   

        private IVersionControlledObject MakeVersioningNodeBasedOn<TSubject>(IVersionControlNode parent = null)
             where TSubject : class
        {
            var node = A.Fake<IVersionControlNode>(options => options
                .Implements(typeof(IVersionController<TSubject>))
                .Wrapping(new FakeVersionControlNodeBase()));

            var obj = A.Fake<TSubject>(options => options.Implements(typeof(IVersionControlledObject))) as IVersionControlledObject;

            A.CallTo(() => obj.GetVersionControlNode()).Returns(node);
            A.CallTo(() => obj.GetVersionController<TSubject>()).Returns(node as IVersionController<TSubject>);
            A.CallTo(() => obj.GetNativeObject<TSubject>()).Returns(obj as TSubject);

            if (parent != null)
            {
                parent.Children.Add(node);
                node.Parent = parent;
            }

            return obj;
        }

        public IVersionControlledObject ConfigureCurrentDepthCopy(IVersionControlledObject child)
        {
            var swappedChild = MakeVersioningObject();
            A.CallTo(() => child.GetVersionControlNode().CurrentDepthCopy()).Returns(swappedChild);

            var swappedNode = swappedChild.GetVersionControlNode();
            var childNode = child.GetVersionControlNode();

            swappedNode.Children.AddRange(childNode.Children);
            swappedNode.Parent = childNode.Parent;
            swappedNode.Mutations.AddRange(childNode.Mutations.Select(mutation => new TimestampedPropertyVersionDelta(mutation)));

            return swappedChild;
        }
    }
}