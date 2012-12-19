using System.Collections.Generic;
using System.Linq;

namespace VersionCommander
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
    }
}