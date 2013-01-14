<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Cloners;

namespace VersionCommander
{
    public static class New
    {
        private static readonly ProxyFactory ProxyFactory;
        static New()
        {
            ProxyFactory = new ProxyFactory();
        }

        public static IList<TSubject> VersioningList<TSubject>()
            where TSubject : IVersionablePropertyBag
        {
            return ProxyFactory.VersioningList<TSubject>();
        }

        public static TSubject Versioning<TSubject>(Action<TSubject> constructionCustomizations = null)
            where TSubject : class, new()
        {
            return Versioning(() => new TSubject(), constructionCustomizations);
        }

        public static TSubject Versioning<TSubject>(Func<TSubject> subjectFactory,
                                                    Action<TSubject> constructionCustomizations = null)
            where TSubject : class
        {
            var proxy = ProxyFactory.CreateVersioning(new DefaultCloneFactory<TSubject>(subjectFactory));
            if (constructionCustomizations != null) constructionCustomizations.Invoke(proxy);
            return proxy;
        }

    }
=======
﻿using System;
using System.Collections.Generic;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Cloners;
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
>>>>>>> f8d34094a494492933f5dc19bf749c84b70c5bac
}