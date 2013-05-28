using System;
using System.Collections.Generic;
using System.Reflection;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    public abstract class VersionControlNodeBase : IVersionControlNode
    {
        protected VersionControlNodeBase()
        {
            Children = new List<IVersionControlNode>();
            Mutations = new List<TimestampedPropertyVersionDelta>();
        }

        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }
        public IList<TimestampedPropertyVersionDelta> Mutations { get; private set; }

        public void Accept(IVersionControlTreeVisitor visitor)
        {
            VisitorBehavior.Accept(this, visitor);
        }

        public void RecursiveAccept(IVersionControlTreeVisitor visitor)
        {
            VisitorBehavior.RecursiveAccept(this, visitor);
        }

        public abstract void RollbackTo(long targetVersion);
        public abstract object CurrentDepthCopy();

        public abstract object Get(PropertyInfo targetProperty, long version = long.MaxValue);
        public abstract void Set(PropertyInfo targetProperty, object value, long version);
    }

    public static class VisitorBehavior
    {
        public static void Accept(IVersionControlNode hostingNode, IVersionControlTreeVisitor visitor)
        {
            visitor.OnFirstEntry();
            hostingNode.RecursiveAccept(visitor);
            visitor.OnLastExit();
        }
        public static void RecursiveAccept(IVersionControlNode hostingNode, IVersionControlTreeVisitor visitor)
        {
            visitor.OnEntry(hostingNode);
            // ReSharper disable ForCanBeConvertedToForeach -- might cause a concurrency issue. TODO
            for (var i = 0; i < hostingNode.Children.Count; i++)
            // ReSharper restore ForCanBeConvertedToForeach
            {
                hostingNode.Children[i].RecursiveAccept(visitor);
            }
            visitor.OnExit(hostingNode);
        }
    }
}
