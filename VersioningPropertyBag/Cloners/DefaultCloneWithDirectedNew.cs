using System;
using VersionCommander.Implementation.Cloners;

namespace VersionCommander.Implementation
{
    public class DefaultCloneWithDirectedNew<TCloned> : ICloneFactory<TCloned>
    {
        private readonly Func<TCloned> _clonedFactory;

        public DefaultCloneWithDirectedNew(Func<TCloned> clonedFactory) : base()
        {
            _clonedFactory = clonedFactory;
        }

        public TCloned CreateNew()
        {
            return _clonedFactory.Invoke();
        }

        public TCloned CreateCloneOf(TCloned target)
        {
            return CloneHelper.TryCloneWithExplicitImplementationUnavailable(target);
        }
    }
}