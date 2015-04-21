using System;
using System.Collections.Generic;

namespace DoubleR.Utils {

    /// <summary>
    /// Implementation of a multimap.
    /// </summary>
    public class Multimap<K, V> {

        protected Dictionary<K, List<V>> dictionary = new Dictionary<K, List<V>>();

        public override string ToString() {
            string repr = "{";
            foreach (K key in dictionary.Keys) {
                string listRepr = "[";
                foreach (V value in dictionary[key]) {
                    listRepr = String.Format("{0}{1},", listRepr, value);
                }
                listRepr = String.Format("{0}]", listRepr);
                repr = String.Format("{0}{1}: {2},", repr, key, listRepr);
            }
            return repr + "}";
        }

        public virtual void Add(K key, V value) {
            if (!dictionary.ContainsKey(key)) {
                dictionary.Add(key, new List<V>());
            }
            dictionary[key].Add(value);
        }

        public void Remove(K key, V value) {
            if (!dictionary.ContainsKey(key)) {
                return;
            }

            dictionary[key].Remove(value);
            if (dictionary[key].Count == 0) {
                dictionary.Remove(key);
            }
        }

        public IEnumerable<V> this[K key] {
            get {
                if (!dictionary.ContainsKey(key)) {
                    yield break;
                }
                foreach (V value in dictionary[key]) {
                    yield return value;
                }
            }
        }

        public HashSet<K> KeySet {
            get {
                HashSet<K> keyset = new HashSet<K>();
                foreach (K key in dictionary.Keys) {
                    keyset.Add(key);
                }
                return keyset;
            }
        }

        public bool ContainsKey(K key) {
            return dictionary.ContainsKey(key);
        }

    }

}
