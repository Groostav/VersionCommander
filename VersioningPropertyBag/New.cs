using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Castle.DynamicProxy;
using VersionCommander.Implementation.Cloners;

namespace VersionCommander.Implementation
{
    public static class New
    {
        static New()
        {
            Generator = new ProxyGenerator();
            //Force AutoMapper into the GAC. this takes about 1.5s.
            new Thread(() => Assembly.GetAssembly(typeof (AutoMapper.Mapper))).Start();
        }

        private static readonly ProxyGenerator Generator;

        public static IList<TSubject> VersioningList<TSubject>()
            where TSubject : IVersionablePropertyBag
        {
            return new VersioningList<TSubject>();
        }

        public static TSubject Versioning<TSubject>(TSubject existing,
                                                    ICloneFactory<TSubject> cloneFactory,
                                                    IEnumerable<TimestampedPropertyVersionDelta> existingChanges)
            where TSubject : class
        {
            var clone = cloneFactory.CreateCloneOf(existing);
            var proxy = MakeVersioningProxy(clone, cloneFactory, existingChanges);
            return proxy;
        }

        public static TSubject MakeVersioningProxy<TSubject>(TSubject baseObject,
                                                             ICloneFactory<TSubject> cloneFactory,
                                                             IEnumerable<TimestampedPropertyVersionDelta> existingModifications = null)
            where TSubject : class
        {
            var controller = new PropertyBagVersionController<TSubject>(baseObject, 
                                                                     cloneFactory, 
                                                                     existingModifications,
                                                                     new VisitorFactory(),
                                                                     new ProxyFactory());

            var subjectInterceptor = new SubjectPropertyInterceptor<TSubject>(controller);
            var versionControlInterceptor = new VersionControlInterceptor<TSubject>(controller);

            var proxy = Generator.CreateClassProxyWithTarget(classToProxy:typeof (TSubject),
                                                             additionalInterfacesToProxy:new[]
                                                             {
                                                                 typeof (IVersionControlProvider)
                                                             },
                                                             target:baseObject,
                                                             options: ProxyGenerationOptions.Default,
                                                             constructorArguments: Enumerable.Empty<object>().ToArray(),
                                                             //note: order matters on interceptors
                                                             interceptors:new IInterceptor[]{versionControlInterceptor, subjectInterceptor});

            return (TSubject) proxy;
        }
    }
}
