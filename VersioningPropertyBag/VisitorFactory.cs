using System.Reflection;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    public class VisitorFactory : IVisitorFactory
    {
        public IVersionControlTreeVisitor MakeVisitor<TVisitor>() where TVisitor : IVersionControlTreeVisitor, new()
        {
            return new TVisitor();
        }

        public IVersionControlTreeVisitor MakeRollbackVisitor(long targetVersion)
        {
            return new RollbackVisitor(targetVersion);
        }

        public IVersionControlTreeVisitor MakeDeltaApplicationVisitor(ChangeType changeType, bool includeDescendents, MethodInfo targetSite = null)
        {
            return new DeltaApplicationVisitor(changeType, targetSite, includeDescendents);
        }
    }
}