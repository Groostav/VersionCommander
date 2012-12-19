using System;

namespace VersionCommander.Exceptions
{
    public class VersionDeltaNotFoundException : Exception
    {
        public VersionDeltaNotFoundException(string message) : base(message)
        {
        }
    }
}