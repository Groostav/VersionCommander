using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    //I like this pattern thats emerging: use visitors to do the grunt-work and let this class be mainly about handling user intent
    //and exception refocusing.
    public class PropertyVersionController<TSubject> : VersionControlNodeBase, IVersionController<TSubject> 
        where TSubject : class
    {
        private readonly List<TimestampedPropertyVersionDelta> _mutations;
        private readonly IEnumerable<PropertyInfo> _knownProperties;
        private readonly TSubject _content;
        private readonly ICloneFactory<TSubject> _cloneFactory;
        private readonly IVisitorFactory _visitorFactory;

        public override IList<TimestampedPropertyVersionDelta> Mutations { get { return _mutations; } } 

        public PropertyVersionController(TSubject content,
                                         ICloneFactory<TSubject> cloneFactory,
                                         IEnumerable<TimestampedPropertyVersionDelta> existingChanges,
                                         IVisitorFactory visitorFactory)
        {
            Children =  new List<IVersionControlNode>();
            _mutations = new List<TimestampedPropertyVersionDelta>();

            _content = content;
            _cloneFactory = cloneFactory;
            _visitorFactory = visitorFactory;

            if (existingChanges != null)
            {
                _mutations.AddRange(existingChanges);
            }

            _knownProperties = typeof (TSubject).GetProperties();
        }

        public override void RollbackTo(long targetVersion)
        {
            Accept(_visitorFactory.MakeRollbackVisitor(targetVersion));
        }

        public void UndoLastChange()
        {
            Accept(_visitorFactory.MakeDeltaApplicationVisitor(includeDescendents:true, isNowActive:false, targetSite: null));
        }

        public void UndoLastAssignment()
        {
            Accept(_visitorFactory.MakeDeltaApplicationVisitor(includeDescendents:false, isNowActive:false, targetSite: null));
        }

        public void UndoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            var targetProperty = GetRequestedMember(targetSite);
            var targetSetter = targetProperty.GetSetMethod();

            Accept(_visitorFactory.MakeDeltaApplicationVisitor(includeDescendents: false, isNowActive: true, targetSite:targetSetter));
        }

        public void RedoLastChange()
        {
            Accept(_visitorFactory.MakeDeltaApplicationVisitor(includeDescendents:true, isNowActive:true, targetSite:null));
        }

        public void RedoLastAssignment()
        {
            Accept(_visitorFactory.MakeDeltaApplicationVisitor(includeDescendents:false, isNowActive:true, targetSite:null));
        }

        public void RedoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            var targetProperty = GetRequestedMember(targetSite);
            EnsureCanRedoChangeTo(targetProperty);
            var targetSetter = targetProperty.GetSetMethod();

            Accept(_visitorFactory.MakeDeltaApplicationVisitor(includeDescendents: false, isNowActive: true, targetSite:targetSetter));
        }

        public override IVersionControlNode CurrentDepthCopy()
        {
            return New.Versioning<TSubject>(_content, _cloneFactory, _mutations.Select(mutation => mutation.Clone())).VersionControlNode();
        }

        public TSubject GetCurrentVersion()
        {
            return WithoutModificationsPast(long.MaxValue);
        }
       
        public TSubject WithoutModificationsPast(long ticks)
        {
            var clone = New.Versioning<TSubject>(_content, _cloneFactory, Mutations);
            clone.VersionControlNode().Accept(_visitorFactory.MakeVisitor<FindAndCopyVersioningChildVisitor>());
            clone.VersionControlNode().Accept(_visitorFactory.MakeRollbackVisitor(ticks));

            return clone;
        }

        public TSubject WithoutVersionControl()
        {
            var clone = _cloneFactory.CreateCloneOf(_content);
            foreach (var versionDelta in Mutations)
            {
                versionDelta.InvokedOn(clone);
            }
            return clone;
        }

        public override object Get(PropertyInfo targetProperty, long version = long.MaxValue)
        {
            var lastReturned = _mutations.Where(mutation => mutation.TimeStamp < version && mutation.IsActive)
                                         .LastOrDefault(mutation => mutation.TargetSite == targetProperty.GetSetMethod());

            return lastReturned != null
                       ? lastReturned.Arguments.Single()
                       : targetProperty.GetGetMethod().Invoke(_content, new object[0]);
        }

        public override void Set(PropertyInfo targetProperty, object value, long version)
        {
            var targetSite = targetProperty.GetSetMethod();
            _mutations.RemoveAll(mutation => mutation.TargetSite == targetSite && ! mutation.IsActive);
            _mutations.Add(new TimestampedPropertyVersionDelta(value, targetSite, version));
        }

        private void EnsureCanUndoChangeTo(PropertyInfo targetMember)
        {
            EnsureCanXDo(targetMember, isRedo:false);
        }
        private void EnsureCanRedoChangeTo(PropertyInfo targetMember)
        {
            EnsureCanXDo(targetMember, isRedo:true);
        }

        private void EnsureCanXDo(PropertyInfo targetMember, bool isRedo)
        {
            var targetMethod = targetMember.GetSetMethod();
            if ( ! _mutations.Any(mutation => mutation.TargetSite == targetMethod && mutation.IsActive ^ isRedo))
            {
                throw new VersionDeltaNotFoundException(
                    string.Format("No change to {0} can be redone, as either no changes have been made yet, " +
                                  "or a previous call to set {0} deleted all previously undone values.", targetMember.Name));
            }
        }

        private PropertyInfo GetRequestedMember<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            var unwoundMessageChain = new Queue<MemberInfo>();
            var root = targetSite.Body;

            var nextMessageChainLink = root;
            while (nextMessageChainLink is MemberExpression)
            {
                unwoundMessageChain.Enqueue((nextMessageChainLink as MemberExpression).Member);
                nextMessageChainLink = (nextMessageChainLink as MemberExpression).Expression;
            }

            if (unwoundMessageChain.Count > 1)
            {
                throw new UntrackedObjectException(
                    string.Format(
                        "Cannot undo assignments to properties of child objects. If that child is itself versionable, " +
                        "invoke {0}() on the child directly",
                        MethodInfoExtensions.GetMethodInfo(() => UndoLastAssignmentTo<TTarget>(null)).Name));
            }
            else if (unwoundMessageChain.Count < 1)
            {
                throw new UntrackedObjectException(
                    string.Format("Version Commander cannot undo/redo an assignment to itself."));
            }

            var candidate = unwoundMessageChain.Dequeue();
            var targetMember = candidate as PropertyInfo;

            if (targetMember == null)
            {
                throw new UntrackedObjectException(
                    string.Format(
                        "Could not determine target member to rollback. Best candidate was {0} but it is not a property.",
                        candidate.Name));
            }

            var targetSetter = targetMember.GetSetMethod();

            if (targetSetter == null)
            {
                throw new UntrackedObjectException(
                    string.Format("No setter for the property {0} exists, thus it is not versioned",
                                  targetSite.Name));
            }

            return targetMember;
        }


    }
}