using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Castle.Core.Internal;

namespace VersionCommander
{
    internal class VersioningList<TElement> : IList<TElement>, IVersionControlNode, ICloneable
        where TElement : IVersionablePropertyBag
    {
        public void InternalRollback(long ticks)
        {
            throw new NotImplementedException();
        }

        public IVersionControlNode ShallowCopy()
        {
            return new VersioningList<TElement>(this);
        }

        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }

        private readonly IList<TElement> _backingList;
        private readonly List<TimestampedPropertyVersionDelta> _versionDeltas;

        public VersioningList(VersioningList<TElement> toCopy)
        {
            _backingList = new List<TElement>(toCopy._backingList);
            _versionDeltas = new List<TimestampedPropertyVersionDelta>(toCopy._versionDeltas);
            Children = new List<IVersionControlNode>(toCopy.Children);
        }

        public VersioningList()
        {
            _backingList = new List<TElement>();
            _versionDeltas = new List<TimestampedPropertyVersionDelta>();
            Children = new List<IVersionControlNode>();
        }

        #region IList<TElement>
        public IEnumerator<TElement> GetEnumerator()
        {
            return _backingList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TElement item)
        {
            TryAddToChildren(item);
            _versionDeltas.Add(new TimestampedPropertyVersionDelta(item,
                                                                   MethodInfoExtensions.GetMethodInfo<IList<TElement>>(list => list.Add(default(TElement))), 
                                                                   Stopwatch.GetTimestamp()));
            _backingList.Add(item);
        }

        public void Clear()
        {
            _backingList.Clear();
            Children.Clear();
            _versionDeltas.Add(new TimestampedPropertyVersionDelta(new object[0], 
                                                                   MethodInfoExtensions.GetMethodInfo<IList<TElement>>(list => list.Add(default(TElement))),
                                                                   Stopwatch.GetTimestamp()));
        }

        public bool Contains(TElement item)
        {
            return _backingList.Contains(item);
        }

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            throw new NotImplementedException();
            //need to think about how I want to handle version controllers
        }

        public bool Remove(TElement item)
        {
            var wasRemoved = _backingList.Remove(item);
            if (wasRemoved)
            {
                TryAddToChildren(item);
            }
            return wasRemoved;
        }

        public int Count { get { return _backingList.Count; } }
        public bool IsReadOnly { get { return _backingList.IsReadOnly; } }
        public int IndexOf(TElement item)
        {
            return _backingList.IndexOf(item);
        }

        public void Insert(int index, TElement item)
        {
            TryAddToChildren(item);
            _backingList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            TryRemoveFromChildren(_backingList[index]);
            _backingList.RemoveAt(index);
        }

        public TElement this[int index]
        {
            get { return _backingList[index]; }
            set 
            { 
                var element = _backingList[index];
                TryRemoveFromChildren(element);
                TryAddToChildren(value);
                _backingList[index] = value;
            }
        }
        #endregion

        #region ICloneable
        public object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IVersionControlNode
        public void Accept(Action<IVersionControlNode> visitor)
        {
            visitor.Invoke(this);
            Children.ForEach(visitor.Invoke);
        }

        public TResult Accept<TResult>(Func<IVersionControlNode, TResult> visitor)
        {
            throw new NotImplementedException();
        }

        public IList<TimestampedPropertyVersionDelta> Mutations 
        { 
            get { return _versionDeltas; } 
        }

        public object Get(PropertyInfo targetProperty)
        {
            throw new NotImplementedException("this call shouldn't have been made by anybody, as list props are immutable");
        }

        public void Set(PropertyInfo targetProperty, object value)
        {
            throw new NotImplementedException("this call shouldn't have been made by anybody, as list props are immutable");
        }
        #endregion

        private void TryAddToChildren(TElement item)
        {
            var controller = item.VersionControlNode();
            if (controller != null)
            {
                Children.Add(controller);
            }
        }
        private void TryRemoveFromChildren(TElement item)
        {
            var controller = item.VersionControlNode();
            if (controller != null)
            {
                Children.Remove(controller);
            }
        }
    }
}