using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Core.Internal;
using VersionCommander.Extensions;

namespace VersionCommander
{
    internal class PropertyVersionController<TSubject> : IVersionControlNode, IVersionController<TSubject> 
        where TSubject : class
    {
        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }

        private readonly List<TimestampedPropertyVersionDelta> _mutations;
        private readonly IEnumerable<PropertyInfo> _knownProperties;
        private readonly TSubject _content;
        private readonly ICloneFactory<TSubject> _cloneFactory;

        public IList<TimestampedPropertyVersionDelta> Mutations { get { return _mutations; } } 

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

        public IVersionControlNode CurrentDepthCopy()
        {
            return new PropertyVersionController<TSubject>(_cloneFactory.CreateCloneOf(_content),
                                                                         _cloneFactory,
                                                                         _mutations);
        }

        public TSubject GetCurrentVersion()
        {
            return GetVersionAt(Stopwatch.GetTimestamp());
        }
       
        public TSubject GetVersionAt(long ticks)
        {
            //this is actually just in the controller, I actually need to hit dynamic proxies...
            var clone = New.Versioning<TSubject>(_content, _cloneFactory, Mutations);
            clone.VersionControlNode().Accept(ScanAndClone);
            clone.VersionControlNode().Accept(node => node.InternalRollback(ticks));

            return clone;
        }

        [ThereBeDragons]
        internal void ScanAndClone(IVersionControlNode node)
        {
            //command object vs delegate strikes: I used Mutations...., which referenced this, which got nicely closed in by C# and i recursed infinitely.
                //moral: command objects give you a little more type safetly.

            var candidatesByIndex = GetCadidatesByIndex(node);
            //thanks to use of enumerables, this actually consumes very little stack space, meaning I can safely handle fairly large graphs
                //outside of image/audio processing though I suspect its rare to have multi-meg object trees.

            foreach (var indexCandidatePair in candidatesByIndex)
            {
                var versioningChild = indexCandidatePair.Value.Arguments.Single();
                var cloneVersionNode = versioningChild.VersionControlNode().CurrentDepthCopy();
                cloneVersionNode.Accept(ScanAndClone); //this node has new memory, but its still referencing the original nodes children. Update those references.
                Mutations[indexCandidatePair.Key] = new TimestampedPropertyVersionDelta(indexCandidatePair.Value, cloneVersionNode);
            }

            //refresh candidates, since all mutations are now different and in new memory
            candidatesByIndex = GetCadidatesByIndex(node);

            //TODO Mutations should be ordered, but a Contract or Debug.Assert would be nice here.
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