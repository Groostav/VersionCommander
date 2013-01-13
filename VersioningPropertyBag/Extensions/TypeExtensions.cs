using System;

namespace VersionCommander.Implementation
{
    public static class TypeExtensions
    {
        public static bool CanInterfaceAs(this Type thisType, Type interfaceType)
        {
            return thisType.GetInterface(interfaceType.FullName) != null;
        }
        public static bool IsAssignableTo<TAssignable>(this Type thisType)
        {
            return typeof (TAssignable).IsAssignableFrom(thisType);
        }
        public static bool IsAssignableTo(this Type thisType, Type typeToAssignto)
        {
            if(typeToAssignto == null) throw new ArgumentNullException("typeToAssignto");

            return typeToAssignto.IsAssignableFrom(thisType);
        }
    }
}