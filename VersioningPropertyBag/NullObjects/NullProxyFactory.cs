<<<<<<< HEAD
﻿using VersionCommander.Implementation.Cloners;

namespace VersionCommander.Implementation.NullObjects
{
    public class NullProxyFactory : IProxyFactory
    {
        public TSubject CreateVersioning<TSubject>(ICloneFactory<TSubject> cloneFactory,
                                                   IVersionControlNode existingControlNode = null,
                                                   TSubject baseObject = null)
            where TSubject : class
        {
            throw new System.NotImplementedException();
        }
    }
=======
﻿using System.Collections.Generic;
using VersionCommander.Implementation.Cloners;

namespace VersionCommander.Implementation.NullObjects
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
>>>>>>> f8d34094a494492933f5dc19bf749c84b70c5bac
}