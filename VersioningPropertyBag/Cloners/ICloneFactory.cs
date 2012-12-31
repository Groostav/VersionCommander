namespace VersionCommander.Implementation.Cloners
{
    public interface ICloneFactory<TCloned>
    {
        TCloned CreateCloneOf(TCloned target);
        TCloned CreateNew();
    }
}