using System;
using System.Collections.Generic;
using System.Linq;

namespace VersionCommander.Extensions
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