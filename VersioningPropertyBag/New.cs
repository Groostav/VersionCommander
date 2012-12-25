using System;
using System.Collections.Generic;
using Castle.DynamicProxy;

namespace VersionCommander.Implementation
{
    public static class New
    {
        static New()
        {
            Generator = new ProxyGenerator();
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
            var repository = new PropertyVersionController<TSubject>(baseObject, 
                                                                     cloneFactory, 
                                                                     existingModifications,
                                                                     new VisitorFactory());

            var subjectInterceptor = new SubjectPropertyInterceptor<TSubject>(repository);
            var versionControlInterceptor = new VersionControlInterceptor<TSubject>(repository);

            var proxy = Generator.CreateClassProxyWithTarget(classToProxy:typeof (TSubject),
                                                             additionalInterfacesToProxy:new[]
                                                             {
                                                                 typeof (IVersionController<TSubject>),
                                                                 typeof (IVersionControlNode)
                                                             },
                                                             target:baseObject,
                                                             options: ProxyGenerationOptions.Default,
                                                             constructorArguments: new object[0],
                                                             //note: order matters on interceptors
                                                             interceptors:new IInterceptor[]{versionControlInterceptor, subjectInterceptor});

            return (TSubject) proxy;
        }
    }
}