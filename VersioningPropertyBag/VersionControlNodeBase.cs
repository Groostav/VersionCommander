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
            visitor.OnFirstEntry();
            RecursiveAccept(visitor);
            visitor.OnLastExit();
        }

        public void RecursiveAccept(IVersionControlTreeVisitor visitor)
        {
            visitor.OnEntry(this);
            Children.ForEach(child => child.RecursiveAccept(visitor));
            visitor.OnExit(this);
        }

        public abstract void RollbackTo(long targetVersion);
        public abstract object CurrentDepthCopy();

        public abstract object Get(PropertyInfo targetProperty, long version = long.MaxValue);
        public abstract void Set(PropertyInfo targetProperty, object value, long version);
    }
}