using System;
using System.Collections.Generic;

namespace DoubleR.Utils {

    /// <summary>
    /// Implementation of a multiset. This does not follow precisely the python DoubleR implementation.
    /// </summary>
    public class Multiset<K> {

        private Dictionary<K, int> dictionary = new Dictionary<K, int>();

        public override string ToString() {
            string repr = "{";
            foreach (K key in dictionary.Keys) {
                repr = String.Format("{0}{1}: {2},", repr, key, dictionary[key]);
            }
            return repr + "}";
        }

        public void Update(Multiset<K> source) {
            foreach (K key in source.dictionary.Keys) {
                for (int i = 1; i <= source.dictionary[key]; i++) {
                    Add(key);
                }
            }
        }

        public void DifferenceUpdate(Multiset<K> source) {
            foreach (K key in source.dictionary.Keys) {
                for (int i = 1; i <= source.dictionary[key]; i++) {
                    Remove(key);
                }
            }
        }

        public void Add(params K[] keys) {
            foreach (K key in keys) {
                if (dictionary.ContainsKey(key)) {
                    dictionary[key] += 1;
                } else {
                    dictionary.Add(key, 1);
                }
            }
        }

        public void Remove(params K[] keys) {
            foreach (K key in keys) {
                if (!dictionary.ContainsKey(key)) {
                    continue;
                }

                dictionary[key] -= 1;
                if (dictionary[key] == 0) {
                    dictionary.Remove(key);
                }
            }
        }

        public int this[K key] {
            get {
                if (!dictionary.ContainsKey(key)) {
                    return 0;
                }
                return dictionary[key];
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
