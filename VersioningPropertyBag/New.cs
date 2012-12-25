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
            var repository = new PropertyVersionController<TSubject>(baseObject, cloneFactory, existingModifications);

            //repository.Accept(CloneAndUpdateChildRepos);
            //the problem is children. the problem is always children.
            //When you do this it creates a copy with all of the edits made, but any edits that included assignment of controllers to this 
            //controller must also have this method invoked with them. So I think I need to run through existingModifications, looking for setters
            //that were made with an argument that is itself versioning, then I need to invoke this method on those discoverd arguments. 

            //note: the interceptedPropertyBagVersionController has to be generic on TSubject or it cant do typed version control things (getVersionAt).

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