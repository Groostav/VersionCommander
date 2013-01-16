using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Castle.DynamicProxy;
using VersionCommander.Implementation.Cloners;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Interceptors;
using VersionCommander.Implementation.NullObjects;

namespace VersionCommander.Implementation
{
    public class ProxyFactory : IProxyFactory
    {
        static ProxyFactory()
        {
            //Force AutoMapper into the GAC. this takes about 1.5s, good speedup if I'm not asked to version anything in that time.
            new Thread(() => Assembly.GetAssembly(typeof (AutoMapper.Mapper))).Start();
        }

        private readonly ProxyGenerator _generator;

        public ProxyFactory()
        {
            _generator = new ProxyGenerator();
        }

        public TSubject CreateVersioning<TSubject>(ICloneFactory<TSubject> cloneFactory, 
                                                   IVersionControlNode existingControlNode = null, 
                                                   TSubject existingObject = null) 
            where TSubject : class
        {
            var clone = existingObject == null ? cloneFactory.CreateNew() : cloneFactory.CreateCloneOf(existingObject);
            existingControlNode = existingControlNode ?? new NullVersionControlNode();

            var proxy = MakeVersioningProxy(clone, cloneFactory, existingControlNode.Mutations);
            proxy.AsVersionControlNode().Children.AddRange(existingControlNode.Children);
 
            return proxy;
        }

        public IList<TSubject> VersioningList<TSubject>()
            where TSubject : IVersionablePropertyBag
        {
            return new VersioningList<TSubject>();
        }

        private TSubject MakeVersioningProxy<TSubject>(TSubject baseObject,
                                                      ICloneFactory<TSubject> cloneFactory,
                                                      IEnumerable<TimestampedPropertyVersionDelta> existingModifications)
            where TSubject : class
        {
            var copiedExistingMutations = existingModifications.Select(mutation => new TimestampedPropertyVersionDelta(mutation));

            var controller = new PropertyBagVersionController<TSubject>(baseObject, 
                                                                        cloneFactory,
                                                                        copiedExistingMutations,
                                                                        new VisitorFactory(),
                                                                        new ProxyFactory());

            var subjectInterceptor = new SubjectPropertyInterceptor<TSubject>(controller);
            var versionControlInterceptor = new VersionControlInterceptor<TSubject>(controller);

            var proxy = _generator.CreateClassProxyWithTarget(classToProxy:                 typeof(TSubject),
                                                              additionalInterfacesToProxy:  new[] { typeof (IVersionControlledObject) },
                                                              target:                       baseObject,
                                                              options:                      ProxyGenerationOptions.Default,
                                                              constructorArguments:         Enumerable.Empty<object>().ToArray(),
                                                              //note: order matters on interceptors
                                                              interceptors:                 new IInterceptor[] { versionControlInterceptor, subjectInterceptor });
            return (TSubject) proxy;
        }
    }
}