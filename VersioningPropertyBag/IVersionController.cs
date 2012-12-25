using System;
using System.Linq.Expressions;

namespace VersionCommander.Implementation
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

        void RollbackTo(long targetVersion);

        TSubject WithoutVersionControl();

        TSubject WithoutModificationsPast(long ticks);
        TSubject GetCurrentVersion();
    }
}