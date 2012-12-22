using System;
using System.Linq.Expressions;
using VersionCommander.Exceptions;

namespace VersionCommander.Extensions
{
    // ReSharper disable SuspiciousTypeConversion.Global -- plenty suspicious casts when working with dynamic proxies
    // ReSharper disable ExpressionIsAlwaysNull -- so if its not suspicious its always null huh :\
    public static class VersionablePropertyBagExtensions
    {
        public static TSubject WithoutModificationsPast<TSubject>(this TSubject subject, long ticks)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionController<TSubject>>(subject);
            return cleanSubject.WithoutModificationsPast(ticks);
        }



        public static void UndoLastChange<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionController<TSubject>>(subject);
            cleanSubject.UndoLastChange();
        }
        public static void UndoLastAssignment<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionController<TSubject>>(subject);
            cleanSubject.UndoLastAssignment();
        }
        public static void UndoLastAssignmentTo<TSubject, TReturnable>(this TSubject subject, 
                                                                       Expression<Func<TSubject, TReturnable>> propertyPointer)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionController<TSubject>>(subject);
            cleanSubject.UndoLastAssignmentTo(propertyPointer);
        }



        public static void RedoLastChange<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionController<TSubject>>(subject);
            cleanSubject.RedoLastChange();
        }

        public static void RedoLastAssignment<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionController<TSubject>>(subject);
            cleanSubject.RedoLastAssignment();
        }
        public static void RedoLastAssignmentTo<TSubject, TReturnable>(this TSubject subject,
                                                                       Expression<Func<TSubject, TReturnable>> propertyPointer)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionController<TSubject>>(subject);
            cleanSubject.RedoLastAssignmentTo(propertyPointer);
        }

        #region infrastruture extensions
        public static IVersionController<TSubject> VersionCommand<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndTryCast<IVersionController<TSubject>>(subject);
            return cleanSubject;
        }
        public static bool IsUnderVersionCommand<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            return CheckAndTryCast<IVersionController<TSubject>>(subject) != null;
        }

        internal static IVersionControlNode VersionControlNode<TSubject>(this TSubject node)
        {
            var cleanSubject = CheckAndTryCast<IVersionControlNode>(node);
            return cleanSubject;
        }

        private static TDesired CheckAndCast<TDesired>(object subject) 
        {
            if (subject == null) { throw new ArgumentNullException("subject"); }
            if (!(subject is TDesired)) { throw new UntrackedObjectException(); }
            return (TDesired) subject;
        }
        
        private static TDesired CheckAndTryCast<TDesired>(object subject) 
        {
            if (subject == null) { throw new ArgumentNullException("subject"); }
            return subject is TDesired ? (TDesired)subject : default(TDesired);
        }

        #endregion
    }
}