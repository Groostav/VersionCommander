using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;

namespace VersionCommander
{
    /// <summary>
    /// This iterface specifies that this is actively controlling the versioning of a TSubject. 
    /// </summary>
    public interface IVersionController<TSubject>
    {
        void UndoLastChange();
        void UndoLastAssignment();
        void UndoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite);

        void RedoLastChange();
        void RedoLastAssignment();
        void RedoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite);

        void RollbackTo(long ticks);

        TSubject WithoutVersionControl();

        TSubject WithoutModificationsPast(long ticks);
        TSubject GetCurrentVersion();
    }

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