using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using VersionCommander.Implementation;
using InternalNew = VersionCommander.Implementation.New;

namespace VersionCommander
{
    public static class New
    {
        public static IList<TSubject> VersioningList<TSubject>()
            where TSubject : IVersionablePropertyBag
        {
            return new VersioningList<TSubject>();
        }

        public static TSubject Versioning<TSubject>(Action<TSubject> constructionCustomizations = null)
            where TSubject : class, new()
        {
            var baseObject = new TSubject();
            var proxy = MakeVersioningProxy(baseObject, new DefaultCloneFactory<TSubject>());
            if (constructionCustomizations != null) constructionCustomizations.Invoke(proxy);
            return proxy;
        }

        public static TSubject Versioning<TSubject>(Func<TSubject> subjectFactory,
                                                    Action<TSubject> constructionCustomizations = null)
            where TSubject : class
        {
            var factory = new DefaultCloneWithDirectedNew<TSubject>(subjectFactory);
            var baseObject = factory.CreateNew();
            var proxy = MakeVersioningProxy(baseObject, factory);
            if (constructionCustomizations != null) constructionCustomizations.Invoke(proxy);
            return proxy;
        }

        private static TSubject MakeVersioningProxy<TSubject>(TSubject baseObject,
                                                              ICloneFactory<TSubject> cloneFactory,
                                                              IEnumerable<TimestampedPropertyVersionDelta> existingModifications = null)
            where TSubject : class
        {
            return InternalNew.MakeVersioningProxy(baseObject, cloneFactory, existingModifications);
        }
    }
}