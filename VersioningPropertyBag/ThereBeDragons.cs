using System;

namespace VersionCommander.Implementation
{
    public class ThereBeDragons : Attribute
    {
        private readonly string _problemDescription;

        public ThereBeDragons()
        {
        }

        public ThereBeDragons(string problemDescription)
        {
            _problemDescription = problemDescription;
        }
    }

    /// <summary>
    /// Describes a class that has behavior that is so simply it isnt worth testing and/or is used by multiple 
    /// test suites without being stubbed/mocked.
    /// </summary>
    public class Simpleton : Attribute
    {
        private readonly string _description;

        public Simpleton()
        {
        }

        public Simpleton(string description)
        {
            _description = description;
        }
    }
}