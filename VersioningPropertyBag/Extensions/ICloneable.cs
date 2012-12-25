namespace VersionCommander.Implementation
{
    public interface ICloneable<out TClone>
    {
        TClone Clone();
    }
}