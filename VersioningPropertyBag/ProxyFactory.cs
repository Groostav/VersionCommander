using System.Collections.Generic;

namespace VersionCommander.Implementation
{
    public class ProxyFactory : IProxyFactory
    {
        public TSubject CreateVersioning<TSubject>(TSubject existing, 
                                             ICloneFactory<TSubject> cloneFactory, 
                                             IEnumerable<TimestampedPropertyVersionDelta> existingChanges) 
            where TSubject : class
        {
            return New.Versioning(existing, cloneFactory, existingChanges);
        }
    }
}