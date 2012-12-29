namespace VersionCommander.Implementation.Visitors
{
    public interface IVersionControlTreeVisitor
    {
        void RunOn(IVersionControlNode controlNode);
    }
}