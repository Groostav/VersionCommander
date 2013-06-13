
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    [ThereBeDragons("No tests, no uses, methods that get mad")]
    public class VersioningList<TElement> : VersionControlNodeBase, IList<TElement>, IVersionControlNode, ICloneable
        where TElement : IVersionable
    {
        private readonly MethodInfo AddMethod =
            MethodInfoExtensions.GetMethodInfo<IList<TElement>>(list => list.Add(default(TElement)));

        private readonly MethodInfo ClearMethod =
            MethodInfoExtensions.GetMethodInfo<IList<TElement>>(list => list.Clear());

        private readonly MethodInfo RemoveMethod =
            MethodInfoExtensions.GetMethodInfo<IList<TElement>>(list => list.Remove(default(TElement)));

        private readonly MethodInfo IndexSetMethod =
            MethodInfoExtensions.GetPropertyInfo<IList<TElement>, TElement>(list => list[0]).GetSetMethod();

        [ThereBeDragons("this is broken, this isnt a property delta.")] 
        private readonly List<TimestampedPropertyVersionDelta> _versionDeltas;

        private readonly IList<TElement> _backingList;

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
            _versionDeltas.Add(new TimestampedPropertyVersionDelta(item, AddMethod, Stopwatch.GetTimestamp()));
            _backingList.Add(item);
        }

        public void Clear()
        {
            _backingList.Clear();
            Children.Clear();
            _versionDeltas.Add(new TimestampedPropertyVersionDelta(new object[0], ClearMethod, Stopwatch.GetTimestamp()));
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
                TryRemoveFromChildren(item);
                _versionDeltas.Add(new TimestampedPropertyVersionDelta(item, RemoveMethod, Stopwatch.GetTimestamp()));
            }
            return wasRemoved;
        }

        public int Count
        {
            get { return _backingList.Count; }
        }

        public bool IsReadOnly
        {
            get { return _backingList.IsReadOnly; }
        }

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

        public override void RollbackTo(long targetVersion)
        {
            throw new NotImplementedException();
        }

        public override object CurrentDepthCopy()
        {
            return new VersioningList<TElement>(this);
        }

        public override object Get(PropertyInfo targetProperty, long version)
        {
            throw new NotImplementedException(
                "this call shouldn't have been made by anybody, as list props are immutable");
        }

        public override void Set(PropertyInfo targetProperty, object value, long version)
        {
            throw new NotImplementedException(
                "this call shouldn't have been made by anybody, as list props are immutable");
        }

        #endregion

        private void TryAddToChildren(TElement item)
        {
            var controller = item.GetVersionControlNode();
            if (controller != null)
            {
                Children.Add(controller);
                controller.Parent = this;
            }
        }

        private void TryRemoveFromChildren(TElement item)
        {
            var controller = item.GetVersionControlNode();
            if (controller == null)
            {
                return;
            }

            var wasRemoved = Children.Remove(controller);
            if (wasRemoved)
            {
                Debug.Assert(controller.Parent == this);
                controller.Parent = null;
            }
        }
    }
}