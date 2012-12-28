namespace VersionCommander.Implementation
{
    public interface IVersionControlProvider
    {
        IVersionControlNode GetVersionControlNode();
        IVersionController<TSubject> GetVersionController<TSubject>();
    }
}