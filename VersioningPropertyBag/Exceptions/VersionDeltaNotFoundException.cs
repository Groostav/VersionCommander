using System;

namespace VersionCommander.Implementation.Exceptions
{
    public class VersionDeltaNotFoundException : Exception
    {
        public VersionDeltaNotFoundException(string message) : base(message)
        {
        }
    }
}