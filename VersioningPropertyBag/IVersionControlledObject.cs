namespace VersionCommander.Implementation
{
    public interface IVersionControlledObject
    {
        IVersionControlNode AsVersionControlNode();
        IVersionController<TSubject> AsVersionController<TSubject>();
        TSubject AsNativeObject<TSubject>() where TSubject : class;
    }
}