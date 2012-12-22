using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace VersionCommander
{
    /// <summary>
    /// This iterface specifies that this is actively controlling the versioning of a TSubject. 
    /// </summary>
    public interface IVersionController<TSubject>
    {
        void RollbackTo(long ticks);
        void UndoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite);
        void RedoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite);

        TSubject WithoutVersionControl();

        TSubject GetVersionAt(long ticks);
        TSubject GetCurrentVersion();
    }

    internal interface IVersionControlNode
    {
        void InternalRollback(long ticks);
        IVersionControlNode CurrentDepthCopy();

        IList<IVersionControlNode> Children { get; set; }
        IVersionControlNode Parent { get; set; }

        void Accept(Action<IVersionControlNode> visitor);

        IList<TimestampedPropertyVersionDelta> Mutations { get; }

        object Get(PropertyInfo targetProperty);
        void Set(PropertyInfo targetProperty, object value);
    }
}