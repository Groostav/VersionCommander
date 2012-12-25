using System;
using System.Collections.Generic;
using System.Linq;

namespace VersionCommander.Implementation.Extensions
{
    public static class MsCorLibExtensions
    {
        public static IDictionary<TKey, TItem> ToDictionary<TKey, TItem>(this IEnumerable<KeyValuePair<TKey, TItem>> thisKeyItemSet)
        {
            return thisKeyItemSet.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static void AddRange<TItem>(this ICollection<TItem> collection, IEnumerable<TItem> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                collection.Add(item);
            }
        }

        public static bool IsSingle<TElement>(this IEnumerable<TElement> source)
        {
            return source.Count() == 1;
        }

        public static void ForEach<TElement>(this IEnumerable<TElement> source, Action action)
        {
            foreach (var element in source)
            {
                action.Invoke();
            }
        }


        public static IGrouping<TKey, TElement> WithMin<TElement, TKey>(this IEnumerable<TElement> source,
                                                                        Func<TElement, TKey> keySelector) 
            where TKey : IComparable<TKey>
        {
            return GetGroupingBy(source, keySelector, result => result < 0);
        }

        public static IGrouping<TKey, TElement> WithMax<TElement, TKey>(this IEnumerable<TElement> source,
                                                                             Func<TElement, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            return GetGroupingBy(source, keySelector, result => result > 0);
        }

        private static IGrouping<TKey, TElement> GetGroupingBy<TElement, TKey>(IEnumerable<TElement> source, 
                                                                               Func<TElement, TKey> keySelector,    
                                                                               Func<int, bool> comparator)
            where TKey : IComparable<TKey>
        {
            if (source == null) throw new ArgumentNullException();
            if (!source.Any()) return new EmptyGrouping<TKey, TElement>();

            var grouping = new Grouping<TKey, TElement>(keySelector, new[] { source.First() });
            foreach (var element in source.Skip(1))
            {
                if (comparator(keySelector(element).CompareTo(grouping.Key)))
                {
                    grouping = new Grouping<TKey, TElement>(keySelector, new[] {element});
                }
                else if (keySelector(element).CompareTo(grouping.Key) == 0)
                {
                    grouping.Add(element);
                }
                else
                {
                    continue;
                }
            }

            return grouping;
        }

        public static bool IsOrderedBy<TElement, TKey>(this IEnumerable<TElement> source,
                                                       Func<TElement, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            var sourceCopy = source.ToArray();

            var result = Enumerable.Range(0, sourceCopy.Length - 1)
                                   .Aggregate(seed:true, func:(current, index) => current & 
                                              keySelector(sourceCopy[index]).CompareTo(keySelector(sourceCopy[index + 1])) <= 0);

            return result;
        }
    }
}