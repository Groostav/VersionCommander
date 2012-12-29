using System;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.NullObjects
{
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
}