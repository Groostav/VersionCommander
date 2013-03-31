namespace VersionCommander.Implementation
{
    public interface IVersionControlledObject
    {
        IVersionControlNode GetVersionControlNode();
        IVersionController<TSubject> GetVersionController<TSubject>();
        TSubject GetNativeObject<TSubject>() where TSubject : class;
    }
}