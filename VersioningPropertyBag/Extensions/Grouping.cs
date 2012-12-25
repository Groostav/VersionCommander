using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VersionCommander.Implementation.Extensions
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

        public Grouping(Func<TElement, TKey> keySelector, IEnumerable<TElement> elements)
        {
            if (elements == null) throw new ArgumentNullException("elements");
            if (keySelector == null) throw new ArgumentNullException("keySelector");

            var retrieved = elements.ToList();
            if( ! retrieved.Any()) throw new NotSupportedException("Sequence contains no elements. Cannot construct a grouping without a single member. " +
                                                                   "Did you mean to use new EmptyGrouping()?");

            var firstElement = retrieved.First();
            var firstElementKey = keySelector(firstElement);
            if (retrieved.Any(element => keySelector(element).CompareTo(firstElementKey) != 0))
            {
                var problemElement = retrieved.First(element => keySelector(element).CompareTo(firstElementKey) != 0);
                var problemKey = keySelector(problemElement);
                var difference = problemKey.CompareTo(firstElementKey);
                throw new NotSupportedException(
                    string.Format(
                        "the element '{0}' (with key '{1}') compared to the leading grouping element '{2}' (with key '{3}') " +
                        "differs by {4}. " +
                        "You cannot create a grouping with a set of items where not all " +
                        "items compare to any other item with a non-zero result. " +
                        "(IE elements.All(element => keySelector(element).CompareTo(keySelector(elements.First()) must be true)",
                        problemElement,
                        problemKey,
                        firstElement,
                        firstElementKey,
                        difference));
            }

            _keySelector = keySelector;
            _elements = new List<TElement>(retrieved);
            Key = firstElementKey;
        }

        public void Add(TElement element)
        {
            var elementKey = _keySelector(element);
            var difference = elementKey.CompareTo(Key);
            if (difference != 0)
                throw new NotSupportedException(
                    string.Format("the added element '{0}' has a key '{1}', but the grouping has a key '{2}' (differs by {3})",
                                  element, elementKey, Key, difference));

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