using System;

namespace VersionCommander.Implementation.Cloners
{
    public class DefaultCloneFactory<TCloned> : ICloneFactory<TCloned>
    {
        private readonly Func<TCloned> _clonedFactory;

        public DefaultCloneFactory(Func<TCloned> clonedFactory)
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