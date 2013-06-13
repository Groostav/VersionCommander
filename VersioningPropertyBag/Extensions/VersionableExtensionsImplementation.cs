using System;
using VersionCommander.Implementation.Exceptions;

namespace VersionCommander.Implementation.Extensions
{
    public static class VersionableExtensionsImplementation
    {
        public static IVersionControlNode GetVersionControlNode<TSubject>(this TSubject node)
        {
            var subjectAsT = CheckAndTryCast<IVersionControlledObject>(node);
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
}