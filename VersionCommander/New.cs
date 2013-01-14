
using System;
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
}