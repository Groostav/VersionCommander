using System;

namespace VersionCommander
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
    }
}