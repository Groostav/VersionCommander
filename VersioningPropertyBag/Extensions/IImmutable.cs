namespace VersionCommander.Implementation
{
    /// <summary>
    /// Signal-Interface declaring that this type does not have any mutable members.
    /// Generic Implementors are not expected to enforce that their type-arguments also implement this interface,
    /// meaning a class interfacing as immutable may have members that give access to mutable types,
    /// but the class that is itself interfacing as immutable must not be subject to any kind of change post-construction.
    /// </summary>
    public interface IImmutable
    { 
    }
}