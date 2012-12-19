using System;
using AutoMapper;

namespace VersionCommander
{
    public interface ICloneFactory<TCloned>
    {
        TCloned CreateCloneOf(TCloned target);
        TCloned CreateNew();
    }

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

    public class DefaultCloneFactory<TCloned> : ICloneFactory<TCloned>
        where TCloned : new()
    {
        public virtual TCloned CreateCloneOf(TCloned target)
        {
            return CloneHelper.TryCloneWithExplicitImplementationUnavailable(target);
        }
        public virtual TCloned CreateNew()
        {
            return new TCloned();
        }
    }

    public static class CloneHelper
    {
        public static TCloned TryCloneWithExplicitImplementationUnavailable<TCloned>(TCloned target)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull if target is a struct this is false, exactly as we'd want.
            if (target == null) throw new ArgumentNullException("target");
            // ReSharper restore CompareNonConstrainedGenericWithNull

            var targetAsCloneable = target as ICloneable;
            if (targetAsCloneable != null)
            {
                return (TCloned)targetAsCloneable.Clone();
            }
            else
            {
                return Mapper.Map<TCloned>(target);
            }
            //it could reflect and look for a copy constructor, but is that too much?
        }
    }
}