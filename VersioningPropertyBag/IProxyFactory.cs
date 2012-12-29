using System.Collections.Generic;

namespace VersionCommander.Implementation
{
    public interface IProxyFactory
    {
        TSubject CreateVersioning<TSubject>(TSubject baseObject,
                                            ICloneFactory<TSubject> cloneFactory,
                                            IEnumerable<TimestampedPropertyVersionDelta> existingChanges)
            where TSubject : class;
    }
}