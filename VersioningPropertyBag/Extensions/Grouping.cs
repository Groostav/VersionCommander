using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VersionCommander.Extensions
{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
        where TKey : IComparable<TKey>
    {
        private readonly Func<TElement, TKey> _keySelector;
        private readonly List<TElement> _elements; 

        public Grouping(Func<TElement, TKey> keySelector, TElement firstElement)
        {
            _keySelector = keySelector;
            _elements = new List<TElement>() {firstElement};
            Key = keySelector(firstElement);
        }

        public Grouping(Func<TElement, TKey> keySelector, params TElement[] elements)
        {
            if (elements == null) throw new ArgumentNullException("elements");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if( ! elements.Any()) throw new NotSupportedException("Sequence contains no elements. Cannot construct a grouping without a single member. " +
                                                                  "Did you mean to use new EmptyGrouping()?");

            var firstElement = elements.First();
            var firstElementKey = keySelector(firstElement);
            var problemElement = elements.FirstOrDefault(element => keySelector(element).CompareTo(firstElementKey) == 0);
            if(problemElement != null)
                throw new NotSupportedException(string.Format("the element '{0}' (with key '{1}') compared to the pivot value '{2}' (with key '{3}') " +
                                                              "does not return zero." +
                                                              "You cannot create a grouping of objects with a set of objects where not all " +
                                                              "objects compare to any other object with a non-zero result. " +
                                                              "(IE elements.All(element => keySelector(element).CompareTo(keySelector(elements.First()) must be true)",
                                                              problemElement,
                                                              keySelector(problemElement),
                                                              firstElement,
                                                              firstElementKey));

            _keySelector = keySelector;
            _elements = new List<TElement>(elements);
            Key = firstElementKey;
        }

        public void Add(TElement element)
        {
            if (_elements.Any() && _keySelector(element).CompareTo(Key) != 0)
                throw new NotImplementedException();

            _elements.Add(element);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TKey Key { get; private set; }
    }
}