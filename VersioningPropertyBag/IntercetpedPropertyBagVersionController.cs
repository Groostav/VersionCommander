using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Core.Internal;
using VersionCommander.Exceptions;

namespace VersionCommander
{
    internal class IntercetpedPropertyBagVersionController<TSubject> : IVersionControlNode, IVersionController<TSubject> 
        where TSubject : class
    {
        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }

        private readonly List<TimestampedPropertyVersionDelta> _mutations;
        private readonly IEnumerable<PropertyInfo> _knownProperties;
        private readonly TSubject _content;
        private readonly ICloneFactory<TSubject> _cloneFactory;

        public IList<TimestampedPropertyVersionDelta> Mutations { get { return _mutations; } } 

        public IntercetpedPropertyBagVersionController(TSubject content,
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

        public void RollbackTo(long ticks)
        {
            InternalRollback(ticks);
            return;
        }
        public void InternalRollback(long ticks)
        {
            var relatedMutations = _mutations.Where(mutation => mutation.TimeStamp > ticks).ToArray();
            foreach (var mutation in relatedMutations)
            {
                _mutations.Remove(mutation);
            }
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
            var unwoundMessageChain = new Queue<MemberInfo>();
            var root = targetSite.Body;

            var nextMessageChainLink = root;
            while (nextMessageChainLink is MemberExpression)
            {
                unwoundMessageChain.Enqueue((nextMessageChainLink as MemberExpression).Member);
                nextMessageChainLink = (nextMessageChainLink as MemberExpression).Expression;
            }

            if (unwoundMessageChain.Count != 1)
            {
                throw new NotImplementedException("Dont yet support branching down from a parent to a child, @feature request it?");
            }

            var targetMember = unwoundMessageChain.Dequeue() as PropertyInfo;

            if (targetMember == null)
            {
                throw new NotImplementedException();
            }

            _mutations.Remove(_mutations.Last(mutation => mutation.TargetSite == targetMember.GetSetMethod()));
        }

        public IVersionControlNode ShallowCopy()
        {
            return new IntercetpedPropertyBagVersionController<TSubject>(_cloneFactory.CreateCloneOf(_content),
                                                                         _cloneFactory,
                                                                         _mutations);
        }
        public TSubject GetCurrentVersion()
        {
            return GetVersionAt(Stopwatch.GetTimestamp());
        }
        [ThereBeDragons]
        public TSubject GetVersionAt(long ticks)
        {
            //this is actually just in the controller, I actually need to hit dynamic proxies...
            var clone = New.Versioning<TSubject>(_content, _cloneFactory, Mutations);
            clone.VersionControlNode().Accept(ScanAndClone);
            clone.VersionControlNode().Accept(node => node.InternalRollback(ticks));

            return clone;
        }
        [ThereBeDragons]
        public void ScanAndClone(IVersionControlNode node)
        {
            Debug.Assert(! (typeof (TimestampedPropertyVersionDelta).IsAssignableTo<IEquatable<TimestampedPropertyVersionDelta>>()), 
                         "IndexOf() could get undesired matches if TimestampedPropertyVersionDelta.Equals() does anything other than reference equals." +
                         "It also doesn't take an equality comparer, so I cant simply force reference equals.");

            //command object vs delegate strikes: I used Mutations...., which referenced this, which got nicely closed in by C# and i recursed infinitely.
                //moral: command objects give you a little more type safetly.
            var candidates = node.Mutations.Where(mutation => mutation.IsSettingVersioningObject());

            foreach (var candidate in candidates)
            {
                var targetIndex = node.Mutations.IndexOf(candidate);
                var cloneVersionNode = candidate.Arguments.Single().VersionControlNode().ShallowCopy();
                cloneVersionNode.Accept(ScanAndClone); //hes been given new shallow memory, make sure he repeats this process.
                Mutations[targetIndex] = new TimestampedPropertyVersionDelta(candidate, cloneVersionNode);
            }

            //refresh candidates, since all mutations are now different and in new memory
            candidates = node.Mutations.Where(mutation => mutation.IsSettingVersioningObject());

            var children = candidates.GroupBy(mutation => mutation.TargetSite)
                                     .Select(group => group.Last())
                                     .Select(mutation => mutation.Arguments.Single().VersionControlNode()))

            node.Children = children.ToList();
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

        public void Accept(Action<IVersionControlNode> visitor)
        {
            visitor.Invoke(this);
            Children.ForEach(child => child.Accept(visitor));
        }

        public object Get(PropertyInfo targetProperty)
        {
            return GetVersionOfPropertyAt(targetProperty, Stopwatch.GetTimestamp());
        }
        public void Set(PropertyInfo targetProperty, object value)
        {
            _mutations.Add(new TimestampedPropertyVersionDelta(value, targetProperty.GetSetMethod(), Stopwatch.GetTimestamp()));
        }

        private object GetVersionOfPropertyAt(PropertyInfo targetProperty, long targetTimestamp)
        {
            var lastReturned = _mutations.Where(mutation => mutation.TimeStamp < targetTimestamp)
                                         .LastOrDefault(mutation => mutation.TargetSite == targetProperty.GetSetMethod());

            return lastReturned != null
                       ? lastReturned.Arguments.Single()
                       : targetProperty.GetGetMethod().Invoke(_content, new object[0]);
        }
    }
}