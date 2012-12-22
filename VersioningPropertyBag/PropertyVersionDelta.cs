using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VersionCommander.Extensions;

namespace VersionCommander
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
            //not sure this is a good dependency, but its convienient. Could refactor to "IsSettingObjectOfType<TSomething>()"
            return TargetSite.IsPropertySetter() && Arguments.Single().VersionControlNode() != null;
        }
    }

    [DebuggerDisplay("TimestampedVersionDelta: Invoke {TargetSite.Name} with {Arguments}")]
    public class TimestampedVersionDelta
    {
        public TimestampedVersionDelta(object[] arguments, MethodInfo targetSite, long timeStamp, bool isActive = true)
        {
            if (arguments == null || targetSite == null) throw new ArgumentNullException();

            TimeStamp = timeStamp;
            Arguments = arguments;
            TargetSite = targetSite;
            IsActive = isActive;
        }
        public TimestampedVersionDelta(TimestampedVersionDelta delta, object[] newArguments)
        {
            TargetSite = delta.TargetSite;
            TimeStamp = delta.TimeStamp;
            IsActive = delta.IsActive;

            Arguments = newArguments;
        }

        public MethodInfo TargetSite { get; private set; }
        public object[] Arguments { get; private set; }
        public long TimeStamp { get; private set; }

        public bool IsActive { get; set; } //dont like this, otherwise immutable object made mutable.

        public object InvokedOn(object target)
        {
            return TargetSite.Invoke(target, Arguments);
        }
    }
}