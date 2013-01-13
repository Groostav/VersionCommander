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
        IVersionControlTreeVisitor MakeDeltaApplicationVisitor(ChangeType changeType, bool includeDescendents, MethodInfo targetSite = null);
    }
}