﻿using System;
using System.Linq.Expressions;
using VersionCommander.Implementation;

namespace VersionCommander
{
    // ReSharper disable SuspiciousTypeConversion.Global -- plenty suspicious casts when working with dynamic proxies
    // ReSharper disable ExpressionIsAlwaysNull -- so if its not suspicious its always null huh :\
    public static class VersionablePropertyBagExtensions
    {
        public static TSubject WithoutModificationsPast<TSubject>(this TSubject subject, long ticks)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionControlProvider>(subject).GetVersionController<TSubject>();
            return cleanSubject.WithoutModificationsPast(ticks);
        }

        public static void UndoLastChange<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionControlProvider>(subject).GetVersionController<TSubject>();
            cleanSubject.UndoLastChange();
        }
        public static void UndoLastAssignment<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionControlProvider>(subject).GetVersionController<TSubject>();
            cleanSubject.UndoLastAssignment();
        }
        public static void UndoLastAssignmentTo<TSubject, TReturnable>(this TSubject subject, 
                                                                       Expression<Func<TSubject, TReturnable>> propertyPointer)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionControlProvider>(subject).GetVersionController<TSubject>();
            cleanSubject.UndoLastAssignmentTo(propertyPointer);
        }

        public static void RedoLastChange<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionControlProvider>(subject).GetVersionController<TSubject>();
            cleanSubject.RedoLastChange();
        }

        public static void RedoLastAssignment<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionControlProvider>(subject).GetVersionController<TSubject>();
            cleanSubject.RedoLastAssignment();
        }
        public static void RedoLastAssignmentTo<TSubject, TReturnable>(this TSubject subject,
                                                                       Expression<Func<TSubject, TReturnable>> propertyPointer)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndCast<IVersionControlProvider>(subject).GetVersionController<TSubject>();
            cleanSubject.RedoLastAssignmentTo(propertyPointer);
        }

        #region infrastruture extensions
        public static IVersionController<TSubject> VersionCommand<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            var cleanSubject = CheckAndTryCast<IVersionControlProvider>(subject).GetVersionController<TSubject>();
            return cleanSubject;
        }
        public static bool IsUnderVersionCommand<TSubject>(this TSubject subject)
            where TSubject : IVersionablePropertyBag
        {
            return CheckAndTryCast<IVersionControlProvider>(subject).GetVersionController<TSubject>() != null;
        }

        private static TDesired CheckAndCast<TDesired>(object subject)
        {
            return Implementation.Extensions.VersionablePropertyBagExtensions.CheckAndCast<TDesired>(subject);
        }
        private static TDesired CheckAndTryCast<TDesired>(object subject)
        {
            return Implementation.Extensions.VersionablePropertyBagExtensions.CheckAndTryCast<TDesired>(subject);
        }

        #endregion
    }
}