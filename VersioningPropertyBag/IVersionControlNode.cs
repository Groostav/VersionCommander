using System;
using System.Collections.Generic;
using System.Reflection;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    public interface IVersionControlNode
    {
        void RollbackTo(long targetVersion);
        IVersionControlNode CurrentDepthCopy();

        IList<IVersionControlNode> Children { get; set; }
        IVersionControlNode Parent { get; set; }

        void Accept(IPropertyTreeVisitor visitor);

        IList<TimestampedPropertyVersionDelta> Mutations { get; }

        object Get(PropertyInfo targetProperty, long version);
        void Set(PropertyInfo targetProperty, object value, long version);
    }
}