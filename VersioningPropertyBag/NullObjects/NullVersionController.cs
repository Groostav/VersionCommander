using System;
using System.Linq.Expressions;

namespace VersionCommander.Implementation.NullObjects
{
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
}