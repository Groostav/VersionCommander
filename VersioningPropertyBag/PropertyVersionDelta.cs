using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VersionCommander.Extensions;

namespace VersionCommander
{
    [DebuggerDisplay("TimestampedPropertyVersionDelta: Set {TargetSite.Name} to {Arguments[0]}")]
    public class TimestampedPropertyVersionDelta : TimestampedVersionDelta, IImmutable
    {
        public TimestampedPropertyVersionDelta(object setValue, MethodInfo targetSite, long timeStamp) 
            : base(new[]{setValue}, targetSite, timeStamp)
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
    public class TimestampedVersionDelta : IImmutable
    {
        public TimestampedVersionDelta(object[] arguments, MethodInfo targetSite, long timeStamp)
        {
            if (arguments == null || targetSite == null) throw new ArgumentNullException();

            TimeStamp = timeStamp;
            Arguments = arguments;
            TargetSite = targetSite;
        }
        public TimestampedVersionDelta(TimestampedVersionDelta delta, object[] newArguments)
        {
            TargetSite = delta.TargetSite;
            TimeStamp = delta.TimeStamp;
            Arguments = newArguments;
        }

        public MethodInfo TargetSite { get; private set; }
        public object[] Arguments { get; private set; }
        public long TimeStamp { get; private set; }

        public object InvokedOn(object target)
        {
            return TargetSite.Invoke(target, Arguments);
        }
    }
}