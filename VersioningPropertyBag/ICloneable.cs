namespace VersionCommander
{
    public interface ICloneable<out TClone>
    {
        TClone Clone();
    }
}