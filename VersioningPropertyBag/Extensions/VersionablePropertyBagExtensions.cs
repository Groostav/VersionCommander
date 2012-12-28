using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation.Extensions
{
    public static class VersionablePropertyBagExtensions
    {
        public static IVersionControlNode VersionControlNode<TSubject>(this TSubject node)
        {
            var subjectAsT = CheckAndTryCast<IVersionControlProvider>(node);
            return subjectAsT == null ? null : subjectAsT.GetVersionControlNode();
        }

        public static TDesired CheckAndCast<TDesired>(object subject)
        {
            if (subject == null) { throw new ArgumentNullException("subject"); }
            if (!(subject is TDesired)) { throw new UntrackedObjectException(); }
            return (TDesired)subject;
        }

        public static TDesired CheckAndTryCast<TDesired>(object subject)
        {
            if (subject == null) { throw new ArgumentNullException("subject"); }
            return subject is TDesired ? (TDesired)subject : default(TDesired);
        } 
    }

    public class NullVersionControlProvider : IVersionControlProvider, IEquatable<IVersionControlProvider>
    {
        public IVersionControlNode GetVersionControlNode()
        {
            return new NullVersionControlNode();
        }

        public IVersionController<TSubject> GetVersionController<TSubject>()
        {
            return new NullVersionController<TSubject>();
        }

        #region equals

        public bool Equals(IVersionControlProvider other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (ReferenceEquals(other, null)) return true;
            return other is NullVersionControlProvider;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IVersionControlProvider)) return false;
            return Equals(obj as IVersionControlProvider);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(NullVersionControlProvider left, NullVersionControlProvider right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NullVersionControlProvider left, NullVersionControlProvider right)
        {
            return !Equals(left, right);
        }

        #endregion
    }

    public class NullVersionController<TSubject> : IVersionController<TSubject>, IEquatable<IVersionController<TSubject>>
    {
        public void UndoLastChange()
        {
            throw new NotImplementedException();
        }

        public void UndoLastAssignment()
        {
            throw new NotImplementedException();
        }

        public void UndoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            throw new NotImplementedException();
        }

        public void RedoLastChange()
        {
            throw new NotImplementedException();
        }

        public void RedoLastAssignment()
        {
            throw new NotImplementedException();
        }

        public void RedoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            throw new NotImplementedException();
        }

        public void RollbackTo(long targetVersion)
        {
            throw new NotImplementedException();
        }

        public TSubject WithoutVersionControl()
        {
            throw new NotImplementedException();
        }

        public TSubject WithoutModificationsPast(long ticks)
        {
            throw new NotImplementedException();
        }

        public TSubject GetCurrentVersion()
        {
            throw new NotImplementedException();
        }

        #region equals

        public bool Equals(IVersionController<TSubject> other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (ReferenceEquals(other, null)) return true;
            return other is NullVersionController<TSubject>;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IVersionController<TSubject>)) return false;
            return Equals(obj as IVersionController<TSubject>);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(NullVersionController<TSubject> left, NullVersionController<TSubject> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NullVersionController<TSubject> left, NullVersionController<TSubject> right)
        {
            return !Equals(left, right);
        }

        #endregion
    }

    public class NullVersionControlNode : IVersionControlNode
    {
        public void RollbackTo(long targetVersion)
        {
            throw new NotImplementedException();
        }

        public IVersionControlNode CurrentDepthCopy()
        {
            throw new NotImplementedException();
        }

        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }
        public void Accept(IPropertyTreeVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public IList<TimestampedPropertyVersionDelta> Mutations { get; private set; }
        public object Get(PropertyInfo targetProperty, long version)
        {
            throw new NotImplementedException();
        }

        public void Set(PropertyInfo targetProperty, object value, long version)
        {
            throw new NotImplementedException();
        }
    }
}