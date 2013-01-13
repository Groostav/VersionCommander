using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Cloners
{
    public static class CloneHelper
    {
        public static TCloned TryCloneWithExplicitImplementationUnavailable<TCloned>(TCloned target)
        {
            if (ReferenceEquals(target, null)) throw new ArgumentNullException("target");

            var targetAsCloneable = target as ICloneable;
            if (targetAsCloneable != null)
            {
                return (TCloned)targetAsCloneable.Clone();
            }
//            else if (target.GetType().HasCopyConstructor())
//            {
//                return target.GetType().GetCopyConstructor()
//            }
            else
            {
                return UseAutoMapper(target);
            }
        }

        private static TClone UseAutoMapper<TClone>(TClone target)
        {
            try
            {
                EnsureAutoMapperCanMap(typeof(TClone));
                return Mapper.Map<TClone>(target);
            }
            catch (AutoMapperMappingException)
            {
                //TODO wrap this
                throw;
            }
        }

        private static void EnsureAutoMapperCanMap(Type neededAsMappable)
        {
            if(neededAsMappable == null) throw new ArgumentNullException("neededAsMappable");

            if (Mapper.FindTypeMapFor(neededAsMappable, neededAsMappable) == null)
            {
                Mapper.CreateMap(neededAsMappable, neededAsMappable);
            }

            var typesToMap = neededAsMappable.GetProperties().Select(prop => prop.PropertyType)
                                                             .Where(type => ! type.IsPrimitive);

            foreach (var propertyType in typesToMap)
            {
                EnsureAutoMapperCanMap(propertyType);      
            }

            return;
        }
    }
}