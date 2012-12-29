using System.Collections.Generic;

namespace VersionCommander.Implementation
{
    public interface IProxyFactory
    {
        TSubject CreateVersioning<TSubject>(TSubject existing,
                                           ICloneFactory<TSubject> cloneFactory,
                                           IEnumerable<TimestampedPropertyVersionDelta> existingChanges)
            where TSubject : class;
    }
}