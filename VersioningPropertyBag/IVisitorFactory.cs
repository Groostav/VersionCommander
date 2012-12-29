using System;
using System.Reflection;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    public interface IVisitorFactory
    {
        IVersionControlTreeVisitor MakeVisitor<TVisitor>()
            where TVisitor : IVersionControlTreeVisitor, new();

        IVersionControlTreeVisitor MakeRollbackVisitor(long targetVersion);
        IVersionControlTreeVisitor MakeDeltaApplicationVisitor(bool includeDescendents, bool makeActive, MethodInfo targetSite);
    }

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

        public IVersionControlTreeVisitor MakeDeltaApplicationVisitor(bool includeDescendents, bool makeActive, MethodInfo targetSite)
        {
            return new DeltaApplicationVisitor(includeDescendents, makeActive, targetSite);
        }
    }
}