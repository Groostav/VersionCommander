using System.Collections.Generic;
using VersionCommander.Implementation.Cloners;

namespace VersionCommander.Implementation
{
    public class ProxyFactory : IProxyFactory
    {
        public TSubject CreateVersioning<TSubject>(TSubject baseObject, 
                                             ICloneFactory<TSubject> cloneFactory, 
                                             IEnumerable<TimestampedPropertyVersionDelta> existingChanges) 
            where TSubject : class
        {
            return New.Versioning(baseObject, cloneFactory, existingChanges);
        }
    }
}