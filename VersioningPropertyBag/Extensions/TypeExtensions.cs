using System;
using System.Linq;
using System.Reflection;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation
{
    public static class TypeExtensions
    {
        public static bool CanInterfaceAs(this Type thisType, Type interfaceType)
        {
            if(thisType == null) throw new ArgumentNullException("thisType");
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");

            return thisType.GetInterface(interfaceType.FullName) != null;
        }
        public static bool IsAssignableFrom<TAssignable>(this Type thisType)
        {
            if(thisType == null) throw new ArgumentNullException("thisType");

            return thisType.IsAssignableFrom(typeof (TAssignable));
        }
        public static bool IsAssignableTo<TAssignable>(this Type thisType)
        {
            if(thisType == null) throw new ArgumentNullException("thisType");

            return typeof (TAssignable).IsAssignableFrom(thisType);
        }
        public static bool IsAssignableTo(this Type thisType, Type typeToAssignto)
        {
            if(typeToAssignto == null) throw new ArgumentNullException("typeToAssignto");

            return typeToAssignto.IsAssignableFrom(thisType);
        }
        
        public static bool HasCopyConstructor(this Type thisType)
        {
            if(thisType == null) throw new ArgumentNullException("thisType");

            return GetCopyConstructor(thisType) != null;
        }

        public static ConstructorInfo GetCopyConstructor(this Type thisType)
        {
            if (thisType == null) throw new ArgumentNullException("thisType");

            return thisType.GetConstructors().SingleOrDefault(constructor => constructor.GetParameters().IsSingle() 
                                                                          && constructor.GetParameters().Single().GetType() == thisType);
        }
    }
}