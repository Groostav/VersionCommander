using System;

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
        public VersionControlTreeVisitorBase()
        {
            _enteredOnce = false;
            _exitedOnce = false;
        }

        private bool _enteredOnce;
        private bool _exitedOnce;

        public virtual void OnFirstEntry()
        {
            if(_enteredOnce) throw new Exception();
            _enteredOnce = true;
        }

        public virtual void OnEntry(IVersionControlNode controlNode)
        {
        }

        public virtual void OnExit(IVersionControlNode controlNode)
        {
        }

        public virtual void OnLastExit()
        {
            if(_exitedOnce) throw new Exception();
            _exitedOnce = true;
        }

        public virtual bool VisitAllNodes { get { return true; } }
    }
}