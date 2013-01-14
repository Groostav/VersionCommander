using System.Collections.Generic;
using VersionCommander.Implementation.Cloners;

namespace VersionCommander.Implementation
{
    public interface IProxyFactory
    {
        TSubject CreateVersioning<TSubject>(ICloneFactory<TSubject> cloneFactory,
                                            IVersionControlNode existingControlNode = null,
                                            TSubject existingObject = null)
            where TSubject : class;
    }
}