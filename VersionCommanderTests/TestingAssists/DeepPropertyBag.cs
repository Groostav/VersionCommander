using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoMapper;
using VersionCommander.Implementation;

namespace VersionCommander.UnitTests.TestingAssists
{
    [DebuggerDisplay("DeepPropertyBag : StringProperty = {StringProperty}")]
    public class DeepPropertyBag : ICloneable, IEquatable<DeepPropertyBag>, IVersionablePropertyBag
    {
        static DeepPropertyBag()
        {
            Mapper.CreateMap<DeepPropertyBag, DeepPropertyBag>();
        }

        public virtual FlatPropertyBag SpecialChild { get; set; }
        public virtual IList<FlatPropertyBag> ChildBags { get; set; }
        public virtual string Stringey { get; set; }

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
}