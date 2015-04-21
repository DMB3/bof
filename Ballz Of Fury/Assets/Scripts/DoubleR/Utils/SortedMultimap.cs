using System;

namespace DoubleR.Utils {

    /// <summary>
    /// Implementation of a multimap whose list of values is sorted.
    /// </summary>
    public class SortedMultimap<K, V> : Multimap<K, V> where V : IComparable<V> {

        public override void Add(K key, V value) {
            base.Add(key, value);
            dictionary[key].Sort();
        }

    }

}
