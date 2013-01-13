using System;

namespace VersionCommander.Implementation.Exceptions
{
    public class VersionDeltaNotFoundException : Exception
    {
        public VersionDeltaNotFoundException()
        {
        }

        public VersionDeltaNotFoundException(string message) : base(message)
        {
        }
    }
}