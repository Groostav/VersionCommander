using System;

namespace VersionCommander.Implementation.Extensions
{
    public interface ICloneable<out TClone>
    {
        TClone Clone();
    }
}