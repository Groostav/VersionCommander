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
}