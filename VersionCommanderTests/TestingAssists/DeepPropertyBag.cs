using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoMapper;
using VersionCommander.Implementation;

namespace VersionCommander.UnitTests.TestingAssists
{
    [DebuggerDisplay("DeepPropertyBag : StringProperty = {StringProperty}")]
    public class DeepPropertyBag : ICloneable,  IEquatable<DeepPropertyBag>, IVersionablePropertyBag
    {
        static DeepPropertyBag()
        {
            //TODO remove this
            Mapper.CreateMap<DeepPropertyBag, DeepPropertyBag>();
        }

        public virtual DeepPropertyBag DeepChild { get; set; }
        public virtual FlatPropertyBag FlatChild { get; set; }

        public virtual IList<FlatPropertyBag> ChildBags { get; set; }
        public virtual string DeepStringProperty { get; set; }

        public object Clone()
        {
            var returnable = Mapper.Map<DeepPropertyBag>(this);
            return returnable;
        }

        public bool Equals(DeepPropertyBag other)
        {
            throw new NotImplementedException();
        }
    }

    [ThereBeDragons("Refactor this out, it doesnt need to be here since a deep property bag has a deep property.")]
    public class GrandDeepPropertyBag 
    {
        public virtual DeepPropertyBag DeepChild { get; set; }
    }
}