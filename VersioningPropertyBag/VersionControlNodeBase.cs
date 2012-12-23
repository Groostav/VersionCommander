using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;

namespace VersionCommander
{
    internal abstract class VersionControlNodeBase : IVersionControlNode
    {
        public IEnumerable<IVersionControlNode> AllDescendents 
        {
            get { return new[]{this}.Union(Children.SelectMany(child => child.AllDescendents)); }
        }

        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }

        public void Accept(Action<IVersionControlNode> visitor)
        {
            visitor.Invoke(this);
            Children.ForEach(child => child.Accept(visitor));
        }

        public abstract void RollbackTo(long ticks);

        public abstract IVersionControlNode CurrentDepthCopy();

        public abstract IList<TimestampedPropertyVersionDelta> Mutations { get; }

        public abstract object Get(PropertyInfo targetProperty, long version);
        public abstract void Set(PropertyInfo targetProperty, object value, long version);
    }
}