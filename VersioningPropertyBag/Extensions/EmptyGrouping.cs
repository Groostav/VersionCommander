using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VersionCommander.Extensions
{
    /// <summary>
    /// An immutable grouping implementation that yields an empty set of elements and throws when attempting to read its key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used by this grouping</typeparam>
    /// <typeparam name="TElement">The type of elements contained in this grouping</typeparam>
    public class EmptyGrouping<TKey, TElement> : IGrouping<TKey, TElement>, IImmutable
    {
        public IEnumerator<TElement> GetEnumerator()
        {
            return Enumerable.Empty<TElement>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TKey Key { get { throw new NotSupportedException("Grouping does not contain any elements, thus does not have an assigned key.");} }
    }
}