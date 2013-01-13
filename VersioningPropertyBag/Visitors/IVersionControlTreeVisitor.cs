namespace VersionCommander.Implementation.Visitors
{
    public interface IVersionControlTreeVisitor
    {
        void OnFirstEntry();
        void OnEntry(IVersionControlNode controlNode);
        void OnExit(IVersionControlNode controlNode);
        void OnLastExit();

        bool VisitAllNodes { get; }
    }

    public abstract class VersionControlTreeVisitorBase : IVersionControlTreeVisitor
    {
        public virtual void OnFirstEntry()
        {
        }

        public virtual void OnEntry(IVersionControlNode controlNode)
        {
        }

        public virtual void OnExit(IVersionControlNode controlNode)
        {
        }

        public virtual void OnLastExit()
        {
        }

        public virtual bool VisitAllNodes { get { return true; } }
    }
}