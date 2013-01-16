
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VersionCommander.Implementation.Cloners;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    //I like this pattern thats emerging: use visitors to do the grunt-work and let this class be mainly about handling user intent
    //and exception refocusing.
    public class PropertyBagVersionController<TSubject> : VersionControlNodeBase, IVersionController<TSubject> 
        where TSubject : class
    {
        private readonly TSubject _content;
        private readonly ICloneFactory<TSubject> _cloneFactory;
        private readonly IVisitorFactory _visitorFactory;
        private readonly IProxyFactory _proxyFactory;

        public PropertyBagVersionController(TSubject content,
                                            ICloneFactory<TSubject> cloneFactory,
                                            IEnumerable<TimestampedPropertyVersionDelta> existingChanges,
                                            IVisitorFactory visitorFactory,
                                            IProxyFactory proxyFactory)
        {            
            _content = content;
            _cloneFactory = cloneFactory;
            _visitorFactory = visitorFactory;
            _proxyFactory = proxyFactory;

            if (existingChanges != null)
            {
                Mutations.AddRange(existingChanges);
            }
        }

        //TODO move this into the typed version controller?
        public override void RollbackTo(long targetVersion)
        {
            Accept(_visitorFactory.MakeRollbackVisitor(targetVersion));
        }

        public void UndoLastChange()
        {
            Accept(_visitorFactory.MakeDeltaApplicationVisitor(ChangeType.Undo, includeDescendents: true));
        }

        public void UndoLastAssignment()
        {
            Accept(_visitorFactory.MakeDeltaApplicationVisitor(ChangeType.Undo, includeDescendents: false));
        }

        public void UndoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            var targetProperty = GetRequestedMember(targetSite);
            EnsureCanUndoChangeTo(targetProperty);
            var targetSetter = targetProperty.GetSetMethod();

            Accept(_visitorFactory.MakeDeltaApplicationVisitor(ChangeType.Undo, includeDescendents: false, targetSite: targetSetter));
        }

        public void RedoLastChange()
        {
            Accept(_visitorFactory.MakeDeltaApplicationVisitor(ChangeType.Redo, includeDescendents: true));
        }

        public void RedoLastAssignment()
        {
            Accept(_visitorFactory.MakeDeltaApplicationVisitor(ChangeType.Redo, includeDescendents: false));
        }

        public void RedoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            var targetProperty = GetRequestedMember(targetSite);
            EnsureCanRedoChangeTo(targetProperty);
            var targetSetter = targetProperty.GetSetMethod();

            Accept(_visitorFactory.MakeDeltaApplicationVisitor(ChangeType.Redo, includeDescendents: false, targetSite: targetSetter));
        }

        public override object CurrentDepthCopy()
        {
            var newProxy = _proxyFactory.CreateVersioning(_cloneFactory, existingControlNode: this, existingObject: _content);
            var node = newProxy.AsVersionControlNode();

            node.Parent = Parent;
            node.Children.AddRange(Children);

            return newProxy;
        }

        public TSubject GetCurrentVersion()
        {
            return WithoutModificationsPast(long.MaxValue);
        }
       
        public TSubject WithoutModificationsPast(long ticks)
        {
            var clone = _proxyFactory.CreateVersioning(_cloneFactory, existingControlNode: this, existingObject: _content);
            var cloneControlNode = clone.AsVersionControlNode();
            Debug.Assert(cloneControlNode != null);

            cloneControlNode.Accept(_visitorFactory.MakeVisitor<FindAndCopyVersioningChildVisitor>());
            cloneControlNode.Accept(_visitorFactory.MakeRollbackVisitor(ticks));

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
            var lastReturned = Mutations.Where(mutation => mutation.TimeStamp < version && mutation.IsActive)
                                        .LastOrDefault(mutation => mutation.TargetSite == targetProperty.GetSetMethod());

            return lastReturned != null
                       ? lastReturned.Arguments.Single()
                       : targetProperty.GetGetMethod().Invoke(_content, new object[0]);
        }

        public override void Set(PropertyInfo targetProperty, object value, long version)
        {
            var targetSite = targetProperty.GetSetMethod();
            Mutations.RemoveAll(mutation => mutation.TargetSite == targetSite && ! mutation.IsActive);
            Mutations.Add(new TimestampedPropertyVersionDelta(value, targetSite, version));
        }

        private void EnsureCanUndoChangeTo(PropertyInfo targetMember)
        {
            EnsureCanXDo(targetMember, isRedo:false);
        }
        private void EnsureCanRedoChangeTo(PropertyInfo targetMember)
        {
            EnsureCanXDo(targetMember, isRedo:true);
        }

        // ReSharper disable UnusedParameter.Local -- "Ensure" methods are simply checks, thus isRedo used only for checks.
        private void EnsureCanXDo(PropertyInfo targetMember, bool isRedo)
        // ReSharper restore UnusedParameter.Local
        {
            var targetMethod = targetMember.GetSetMethod();
            if ( ! Mutations.Any(mutation => mutation.TargetSite == targetMethod && mutation.IsActive != isRedo))
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

            if (unwoundMessageChain.Count < 1)
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