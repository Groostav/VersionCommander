using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoMapper;

namespace VersionCommander
{
    [DebuggerDisplay("DeepPropertyBag : Stringey = {Stringey}")]
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

    [DebuggerDisplay("FlatPropertyBag : Stringey = {Stringey}")]
    public class FlatPropertyBag : ICloneable, IEquatable<FlatPropertyBag>, IVersionablePropertyBag
    {
        public virtual string Stringey { get; set; }
        public virtual int County { get; set; }

        public object Clone()
        {
            return Mapper.Map<FlatPropertyBag>(this);
        }

        #region Equality Nonsense

        public bool Equals(FlatPropertyBag other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Stringey, other.Stringey) && County == other.County;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FlatPropertyBag)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Stringey != null ? Stringey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ County;
                return hashCode;
            }
        }

        public static bool operator ==(FlatPropertyBag left, FlatPropertyBag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FlatPropertyBag left, FlatPropertyBag right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}