
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VersionCommander.Implementation.Extensions
{
    public static class MsCoreLibExtensions
    {
        public static IDictionary<TKey, TItem> ToDictionary<TKey, TItem>(this IEnumerable<KeyValuePair<TKey, TItem>> thisKeyItemSet)
        {
            return thisKeyItemSet.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static bool IsSupersetOf<TElement>(this IEnumerable<TElement> source,
                                                  IEnumerable<TElement> candidateSubset)
        {
            return candidateSubset.All(source.Contains);
        }

        public static void AddRange<TItem>(this ICollection<TItem> source, params TItem[] itemsToAdd)
        {
            AddRange(source, (IEnumerable<TItem>)itemsToAdd);
        }

        public static void AddRange<TItem>(this ICollection<TItem> source, IEnumerable<TItem> itemsToAdd)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (itemsToAdd == null) throw new ArgumentNullException("itemsToAdd");

            foreach (var item in itemsToAdd)
            {
                source.Add(item);
            }
        }
        
        public static void RemoveAll<TItem>(this ICollection<TItem> source, Func<TItem, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");

            foreach (var item in source.ToArray().Where(predicate))
            {
                source.Remove(item);
            }
        }

        public static IEnumerable<TElement> Union<TElement>(this IEnumerable<TElement> source, TElement newMember)
        {
            return source.Union(new[] {newMember});
        }
            
        [DebuggerStepThrough]
        public static void ForEach<TElement>(this IEnumerable<TElement> source, Action<TElement> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (var element in source)
            {
                action.Invoke(element);
            }
        }

        [DebuggerStepThrough]
        public static void ForEach<TElement>(this IEnumerable<TElement> source, Action action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (var element in source)
            {
                action.Invoke();
            }
        }

        [DebuggerStepThrough]
        public static IEnumerable<TElement> Except<TElement>(this IEnumerable<TElement> source, TElement excluded)
        {
            if (source == null) throw new ArgumentNullException("source");

            return source.Except(new[] {excluded});
        }

        [DebuggerStepThrough]
        public static bool IsSingle<TElement>(this IEnumerable<TElement> source)
        {
            if(source == null) throw new ArgumentNullException("source");

            //cant do Count == 1 as Count() is O(n) on non collection enumerables. 
            return source.Any() && ! source.Skip(1).Any();
        }


        public static IGrouping<TKey, TElement> WithMin<TElement, TKey>(this IEnumerable<TElement> source,
                                                                        Func<TElement, TKey> keySelector) 
            where TKey : IComparable<TKey>
        {
            return GetGroupingBy(source, keySelector, isBetter: compareResult => compareResult < 0);
        }

        public static IGrouping<TKey, TElement> WithMax<TElement, TKey>(this IEnumerable<TElement> source,
                                                                        Func<TElement, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            return GetGroupingBy(source, keySelector, isBetter: compareResult => compareResult > 0);
        }

        private static IGrouping<TKey, TElement> GetGroupingBy<TElement, TKey>(IEnumerable<TElement> source, 
                                                                               Func<TElement, TKey> keySelector,    
                                                                               Func<int/*TComparisonResult*/, bool> isBetter)
            where TKey : IComparable<TKey>
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (isBetter == null) throw new ArgumentNullException("isBetter");
            // ReSharper disable PossibleMultipleEnumeration -- Any(), First(), and Skip(1) arnt worth ToArray, since all of them are O(1). 
            if (!source.Any()) return new EmptyGrouping<TKey, TElement>();

            var grouping = new Grouping<TKey, TElement>(keySelector, new[] {source.First()});
            foreach (var element in source.Skip(1))
            {
                if (isBetter(keySelector(element).CompareTo(grouping.Key)))
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
            // ReSharper restore PossibleMultipleEnumeration
        }

        public static bool IsOrderedBy<TElement, TKey>(this IEnumerable<TElement> source,
                                                       Func<TElement, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");

            var sourceCopy = source.ToArray();

            for(var i = 0; i < sourceCopy.Length - 1; i++)
            {
                if (keySelector(sourceCopy[i]).CompareTo(keySelector(sourceCopy[i + 1])) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<TElement> SubsetOnRange<TElement>(this IList<TElement> source, 
                                                                    int startIndex,
                                                                    int lastIndex)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (startIndex < 0 || lastIndex >= source.Count) throw new IndexOutOfRangeException();

            var currentIndex = startIndex;
            for (var it = source.GetEnumerator(); it.MoveNext() && currentIndex < lastIndex; currentIndex++)
            {
                yield return source[currentIndex];
            }
        } 
    }
}