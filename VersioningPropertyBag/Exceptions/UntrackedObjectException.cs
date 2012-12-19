using System;

namespace VersionCommander.Exceptions
{
    public class UntrackedObjectException : Exception
    {
        public UntrackedObjectException() : base()
        {
        }

        public UntrackedObjectException(string message) : base(message)
        {
        }
    }
}