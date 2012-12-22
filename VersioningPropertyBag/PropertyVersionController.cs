using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Core.Internal;
using VersionCommander.Exceptions;
using VersionCommander.Extensions;

namespace VersionCommander
{
    internal class PropertyVersionController<TSubject> : VersionControlNodeBase, IVersionController<TSubject> 
        where TSubject : class
    {
        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }

        private readonly List<TimestampedPropertyVersionDelta> _mutations;
        private readonly IEnumerable<PropertyInfo> _knownProperties;
        private readonly TSubject _content;
        private readonly ICloneFactory<TSubject> _cloneFactory;

        public override IList<TimestampedPropertyVersionDelta> Mutations { get { return _mutations; } } 

        public PropertyVersionController(TSubject content,
                                         ICloneFactory<TSubject> cloneFactory,
                                         IEnumerable<TimestampedPropertyVersionDelta> existingChanges)
        {
            Children =  new List<IVersionControlNode>();
            _mutations = new List<TimestampedPropertyVersionDelta>();
            _content = content;
            _cloneFactory = cloneFactory;

            if (existingChanges != null)
            {
                _mutations.AddRange(existingChanges);
            }

            _knownProperties = typeof (TSubject).GetProperties();
        }

        public override void RollbackTo(long ticks)
        {
            var relatedMutations = _mutations.Where(mutation => mutation.TimeStamp > ticks).ToArray();
            foreach (var mutation in relatedMutations)
            {
                _mutations.Remove(mutation);
            }
        }


        public void UndoLastChange()
        {
            throw new NotImplementedException();
        }

        public void UndoLastAssignment()
        {
            throw new NotImplementedException();
        }

        [ThereBeDragons]
        //whats a clean behavior for this. I do not want to write a compiler, but you users need to be able to specify that "undo the thing I did to a child".
        //lets assert the expression is a 2 car message train. If you want to go further than that, invoke Rollback on the child object since it would need to 
        //be under version control anyways.

        //this is going to get complex with the List. For that list, It will implement this interface, so whats the behavior there?
        //I need to allow linqing on the list itself, the value in
        //unversionedObject.VersioningList.RollbackLastCallto(list => list.Where(Condition).Select(item => item.Child));
        //the target here is to inv...
        //no I'd want to force you to use
        //unversioned.VersioningList.Where(Condition).ForEach(item => item.RollbackLastCallTo(item => item.Child))
        //so, strictly speaking, what I'd want is simply to allow rollbacks to Add and Remove?
        //I think thats closest to the interfaces perview...

        public void UndoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            var targetMember = GetRequestedMember(targetSite).GetSetMethod();
            var targetMutation = _mutations.Last(mutation => mutation.TargetSite == targetMember);
            targetMutation.IsActive = false;
        }

        public void RedoLastChange()
        {
            throw new NotImplementedException();
        }

        public void RedoLastAssignment()
        {
            throw new NotImplementedException();
        }

        public void RedoLastAssignmentTo<TTarget>(Expression<Func<TSubject, TTarget>> targetSite)
        {
            var targetProperty = GetRequestedMember(targetSite);
            EnsureCanRedoChangeTo(targetProperty);
            var targetSetter = targetProperty.GetSetMethod();

            var targetDelta = _mutations.First(mutation => mutation.TargetSite == targetSetter && !mutation.IsActive);
            targetDelta.IsActive = true;
        }

        private void EnsureCanRedoChangeTo(PropertyInfo targetMember)
        {
            var targetMethod = targetMember.GetSetMethod();
            if ( ! _mutations.Any(mutation => mutation.TargetSite == targetMethod && !mutation.IsActive))
            {
                throw new UntrackedObjectException(
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

            var targetMember = unwoundMessageChain.Dequeue() as PropertyInfo;

            if (targetMember == null)
            {
                throw new UntrackedObjectException(
                    string.Format(
                        "Could not determine target member to rollback. Best candidate was {0} but it is not a property.",
                        targetMember.Name));
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


        public override IVersionControlNode CurrentDepthCopy()
        {
            return new PropertyVersionController<TSubject>(_cloneFactory.CreateCloneOf(_content),
                                                                         _cloneFactory,
                                                                         _mutations);
        }

        public TSubject GetCurrentVersion()
        {
            return WithoutModificationsPast(long.MaxValue);
        }
       
        public TSubject WithoutModificationsPast(long ticks)
        {
            //this is actually just in the controller, I actually need to hit dynamic proxies...
            var clone = New.Versioning<TSubject>(_content, _cloneFactory, Mutations);
            clone.VersionControlNode().Accept(ScanAndClone);
            clone.VersionControlNode().Accept(node => node.RollbackTo(ticks));

            return clone;
        }

        [ThereBeDragons]
        internal void ScanAndClone(IVersionControlNode node)
        {
            Debug.Assert(Mutations.IsOrderedBy(mutation => mutation.TimeStamp));

            //command object vs delegate strikes: I used Mutations.Linq... which referenced this, which got nicely closed in by C# and i recursed infinitely.
                //moral: command objects give you a little more type safetly.

            var candidatesByIndex = GetCadidatesByIndex(node);
            //thanks to use of enumerables, this actually consumes very little stack space, meaning I can safely handle fairly large graphs
                //outside of image/audio processing though I suspect its rare to have multi-meg object trees.

            foreach (var indexCandidatePair in candidatesByIndex)
            {
                var versioningChild = indexCandidatePair.Value.Arguments.Single();
                var cloneVersionNode = versioningChild.VersionControlNode().CurrentDepthCopy();

                //this node has new memory, but its still referencing the original children.
                cloneVersionNode.Accept(ScanAndClone); //Update those references.

                Mutations[indexCandidatePair.Key] = new TimestampedPropertyVersionDelta(indexCandidatePair.Value, cloneVersionNode);
            }

            //refresh candidates, since all mutations are now different and in new memory
            candidatesByIndex = GetCadidatesByIndex(node);

            var children = candidatesByIndex.GroupBy(mutation => mutation.Value.TargetSite)
                                            .Select(group => group.Last())
                                            .Select(mutation => mutation.Value.Arguments.Single().VersionControlNode());

            node.Children = children.ToList();
        }

        private IEnumerable<KeyValuePair<int, TimestampedPropertyVersionDelta>> GetCadidatesByIndex(IVersionControlNode node)
        {            
            return from index in Enumerable.Range(0, node.Mutations.Count) 
                   where node.Mutations[index].IsSettingVersioningObject() 
                   select new KeyValuePair<int, TimestampedPropertyVersionDelta>(index, node.Mutations[index]);
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
            return GetVersionOfPropertyAt(targetProperty, version);
        }
        public override void Set(PropertyInfo targetProperty, object value, long version)
        {
            var targetSite = targetProperty.GetSetMethod();
            _mutations.RemoveAll(mutation => mutation.TargetSite == targetSite && !mutation.IsActive);
            _mutations.Add(new TimestampedPropertyVersionDelta(value, targetSite, version));
        }

        private object GetVersionOfPropertyAt(PropertyInfo targetProperty, long targetTimestamp)
        {
            var lastReturned = _mutations.Where(mutation => mutation.TimeStamp < targetTimestamp && mutation.IsActive)
                                         .LastOrDefault(mutation => mutation.TargetSite == targetProperty.GetSetMethod());

            return lastReturned != null
                       ? lastReturned.Arguments.Single()
                       : targetProperty.GetGetMethod().Invoke(_content, new object[0]);
        }
    }
}