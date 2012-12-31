using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation
{
    [DebuggerDisplay("TimestampedPropertyVersionDelta: Set {TargetSite.Name} to {_arguments[0]}")]
    public class TimestampedPropertyVersionDelta : TimestampedVersionDelta, ICloneable<TimestampedPropertyVersionDelta>
    {
        public TimestampedPropertyVersionDelta(object setValue, MethodInfo targetSite, long timeStamp, bool isActive = true) 
            : base(new[]{setValue}, targetSite, timeStamp, isActive)
        {
        }

        public TimestampedPropertyVersionDelta(TimestampedPropertyVersionDelta delta, object newSetValue)
            : base(delta, new[]{newSetValue})
        {
        }

        public TimestampedPropertyVersionDelta(TimestampedPropertyVersionDelta delta) 
            : base(delta)
        {
        }

        public bool IsSettingVersioningObject()
        {
            //not sure this is a good dependency, but its convienient. Could refactor to "IsSetting<TSomething>()",
                //but then I lose the routing through the VersionControlNode extension method.
                //maybe the MS Explicit cast operator can help me here?
            if ( ! TargetSite.IsPropertySetter())
            {
                return false;
            }
            var setValue = Arguments.Single();
            //"is" is probably a fair bit cheaper than hitting the interceptor, so lets try to fail it on a simple interface query:
            if (! (setValue is IVersionablePropertyBag)) //TODO this breaks my DLL layout.
            {
                return false;
            }
            //full expensive call:
            return setValue.VersionControlNode() != null;
        }

        public new TimestampedPropertyVersionDelta Clone()
        {
            //so this might be one of the reasons Shermer doesn't like clone: it doesnt inherit nicely.
                //were ok for this simple use case, but in the case where behavior is truely extended, we're boned. 
                //what I'd really like is a where this : TThis, and then the clone signature could be public TThis Clone();
            return new TimestampedPropertyVersionDelta(this);
        }
    }

    [DebuggerDisplay("TimestampedVersionDelta: Invoke {TargetSite.Name} with {Arguments}")]
    public class TimestampedVersionDelta : ICloneable<TimestampedVersionDelta>
    {
        private readonly List<object> _arguments;

        public TimestampedVersionDelta(TimestampedVersionDelta delta)
            : this(delta.Arguments, delta.TargetSite, delta.TimeStamp, delta.IsActive)
        {
        }

        public TimestampedVersionDelta(TimestampedVersionDelta delta, IEnumerable<object> newArguments)
            : this(newArguments, delta.TargetSite, delta.TimeStamp, delta.IsActive)
        {
        }

        public TimestampedVersionDelta(IEnumerable<object> arguments, MethodInfo targetSite, long timeStamp, bool isActive = true)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (targetSite == null) throw new ArgumentNullException("targetSite");

            _arguments = arguments.ToList();
            TimeStamp = timeStamp;
            TargetSite = targetSite;
            IsActive = isActive;
        }

        public MethodInfo TargetSite { get; private set; }
        public IEnumerable<object> Arguments
        {
            get { return _arguments.AsReadOnly(); }
        }

        public long TimeStamp { get; private set; }

        public bool IsActive { get; set; } //dont like this, otherwise immutable object made mutable.
                                           //infact its responsible for a bug. 
                                                //decorator?

        public object InvokedOn(object target)
        {
            return TargetSite.Invoke(target, Arguments.ToArray());
        }

        public TimestampedVersionDelta Clone()
        {
            return new TimestampedVersionDelta(this);
        }
    }
}