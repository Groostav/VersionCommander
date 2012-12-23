using System;
using System.Collections.Generic;
using System.Reflection;

namespace VersionCommander
{
    internal interface IVersionControlNode
    {
        void RollbackTo(long ticks);
        IVersionControlNode CurrentDepthCopy();

        IList<IVersionControlNode> Children { get; set; }
        IEnumerable<IVersionControlNode> AllDescendents { get; }
        IVersionControlNode Parent { get; set; }

        void Accept(Action<IVersionControlNode> visitor);

        IList<TimestampedPropertyVersionDelta> Mutations { get; }

        object Get(PropertyInfo targetProperty, long version);
        void Set(PropertyInfo targetProperty, object value, long version);
    }
}