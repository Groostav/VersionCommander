using System.Collections.Generic;

namespace VersionCommander.Implementation
{
    public class NullProxyFactory : IProxyFactory
    {
        public TSubject CreateVersioning<TSubject>(TSubject baseObject, 
                                                   ICloneFactory<TSubject> cloneFactory, 
                                                   IEnumerable<TimestampedPropertyVersionDelta> existingChanges)
            where TSubject : class
        {
            throw new System.NotImplementedException();
        }
    }
}