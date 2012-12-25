using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    public abstract class VersionControlNodeBase : IVersionControlNode
    {

        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }

        public void Accept(IPropertyTreeVisitor visitor)
        {
            visitor.RunOn(this);
            Children.ForEach(child => child.Accept(visitor));
        }

        public abstract void RollbackTo(long targetVersion);

        public abstract IVersionControlNode CurrentDepthCopy();

        public abstract IList<TimestampedPropertyVersionDelta> Mutations { get; }

        public abstract object Get(PropertyInfo targetProperty, long version);
        public abstract void Set(PropertyInfo targetProperty, object value, long version);
    }
}