using System;
using System.Reflection;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    public interface IVisitorFactory
    {
        IPropertyTreeVisitor MakeVisitor<TVisitor>()
            where TVisitor : IPropertyTreeVisitor, new();

        IPropertyTreeVisitor MakeRollbackVisitor(long targetVersion);
        IPropertyTreeVisitor MakeDeltaApplicationVisitor(bool includeDescendents, bool isNowActive, MethodInfo targetSite);
    }

    public class VisitorFactory : IVisitorFactory
    {
        public IPropertyTreeVisitor MakeVisitor<TVisitor>() where TVisitor : IPropertyTreeVisitor, new()
        {
            return new TVisitor();
        }

        public IPropertyTreeVisitor MakeRollbackVisitor(long targetVersion)
        {
            return new RollbackVisitor(targetVersion);
        }

        public IPropertyTreeVisitor MakeDeltaApplicationVisitor(bool includeDescendents, bool isNowActive, MethodInfo targetSite)
        {
            return new DeltaApplicationVisitor(includeDescendents, isNowActive, targetSite);
        }
    }
}