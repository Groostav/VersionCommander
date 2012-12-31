namespace VersionCommander.Implementation.Cloners
{
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
}