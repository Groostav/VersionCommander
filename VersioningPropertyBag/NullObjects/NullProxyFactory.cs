
using VersionCommander.Implementation.Cloners;

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
}