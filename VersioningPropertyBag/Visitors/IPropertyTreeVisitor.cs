namespace VersionCommander.Implementation.Visitors
{
    public interface IPropertyTreeVisitor
    {
        void RunOn(IVersionControlNode controlNode);
    }
}