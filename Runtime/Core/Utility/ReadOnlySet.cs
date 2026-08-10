using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Infohazard.StillTimeScript.Core.Utility {
    public class ReadOnlySet<T> : IReadOnlyCollection<T> {
        private readonly HashSet<T> _internalHashSet;

        public int Count => _internalHashSet.Count;

        public static ReadOnlySet<T> Empty { get; } = new(Enumerable.Empty<T>());

        public ReadOnlySet(IEnumerable<T> items) {
            _internalHashSet = new HashSet<T>(items);
        }

        public ReadOnlySet(HashSet<T> items) {
            _internalHashSet = items;
        }

        public HashSet<T>.Enumerator GetEnumerator() {
            return _internalHashSet.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool Contains(T item) {
            return _internalHashSet.Contains(item);
        }

        public List<TOther> ToList<TOther>(Func<T, TOther> converter) {
            List<TOther> result = new();
            foreach (T item in _internalHashSet) {
                TOther current = converter(item);

                if (current is null) {
                    StsLibrary.LogError($"Encountered item with null conversion value: {item}");
                    continue;
                }

                result.Add(current);
            }

            return result;
        }
    }
}
