using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation
{
    [Simpleton]
    [DebuggerDisplay("TimestampedPropertyVersionDelta: Set {TargetSite.Name} to {_arguments[0]}")]
    public class TimestampedPropertyVersionDelta : TimestampedVersionDelta 
    {
        public TimestampedPropertyVersionDelta(object setValue, MethodInfo targetSite, long timeStamp, bool isActive = true) 
            : base(new[]{setValue}, targetSite, timeStamp, isActive)
        {
            if( ! targetSite.IsPropertySetter()) 
                throw new Exception(string.Format("method {0} is not a property setter.", targetSite));

            var actualType = targetSite.GetParameters().Single().ParameterType;

            if (setValue == null && actualType.IsValueType && actualType != typeof(Nullable<>)) //nullable being a struct is annoying.
                throw new Exception(String.Format("Attempting to set null to a value type"));
                //actually looking at the comments from microsoft symbole servers on nullable, Microsoft seems to think the whole concept
                //of nullable is pretty annoying. Wierd serialization issues. 

            if (setValue != null && ! setValue.GetType().IsAssignableTo(actualType)) 
                throw new Exception(String.Format("supplied value to set is not the correct type. Supplied value is a {0} but the setter is setting a {1}", 
                                                  setValue.GetType(), actualType));
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

            //full expensive call:
            return setValue.VersionControlNode() != null;
        }
    }

    [Simpleton]
    [DebuggerDisplay("TimestampedVersionDelta: Invoke {TargetSite.Name} with {Arguments}")]
    public class TimestampedVersionDelta
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
