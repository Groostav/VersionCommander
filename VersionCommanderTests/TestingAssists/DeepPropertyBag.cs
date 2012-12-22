using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoMapper;

namespace VersionCommander.Tests.TestingAssists
{
    [DebuggerDisplay("DeepPropertyBag : StringProperty = {StringProperty}")]
    public class DeepPropertyBag : ICloneable, IEquatable<DeepPropertyBag>, IVersionablePropertyBag
    {
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