using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation
{
    [DebuggerDisplay("TimestampedPropertyVersionDelta: Set {TargetSite.Name} to {Arguments[0]}")]
    public class TimestampedPropertyVersionDelta : TimestampedVersionDelta
    {
        public TimestampedPropertyVersionDelta(object setValue, MethodInfo targetSite, long timeStamp, bool isActive = true) 
            : base(new[]{setValue}, targetSite, timeStamp, isActive)
        {
        }

        public TimestampedPropertyVersionDelta(TimestampedPropertyVersionDelta delta, object newSetValue)
            : base(delta, new[]{newSetValue})
        {
        }

        public bool IsSettingVersioningObject()
        {
            //not sure this is a good dependency, but its convienient. Could refactor to "IsSetting<TSomething>()",
                //but then I lose the routing through the VersionControlNode extension method.
                //maybe the MS Explicit cast operator can help me here?
            return TargetSite.IsPropertySetter() && Arguments.Single().VersionControlNode() != null;
        }
    }

    [DebuggerDisplay("TimestampedVersionDelta: Invoke {TargetSite.Name} with {Arguments}")]
    public class TimestampedVersionDelta
    {
        private readonly List<object> _arguments;

        public TimestampedVersionDelta(IEnumerable<object> arguments, MethodInfo targetSite, long timeStamp, bool isActive = true)
        {
            if (arguments == null || targetSite == null) throw new ArgumentNullException();

            _arguments = arguments.ToList();
            TimeStamp = timeStamp;
            TargetSite = targetSite;
            IsActive = isActive;
        }

        public TimestampedVersionDelta(TimestampedVersionDelta delta, IEnumerable<object> newArguments)
        {
            _arguments = newArguments.ToList();

            TargetSite = delta.TargetSite;
            TimeStamp = delta.TimeStamp;
            IsActive = delta.IsActive;
        }

        public MethodInfo TargetSite { get; private set; }
        public IList<object> Arguments
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
    }
}